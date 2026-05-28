using Microsoft.Extensions.Options;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.User;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IVoteRepository _voteRepository;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IStoryRepository storyRepository,
        IVoteRepository voteRepository,
        IWebHostEnvironment env,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _storyRepository = storyRepository;
        _voteRepository = voteRepository;
        _env = env;
        _logger = logger;
    }

    public async Task<ProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден.");

        var stats = await _userRepository.GetUserStatsAsync(userId, cancellationToken);

        return new ProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.AvatarImageUrl,
            user.CreatedAt,
            stats.StoriesCount,
            stats.VotesCount,
            stats.CommentsCount);
    }

    public async Task<UserProfileExtended> GetProfileExtendedAsync(int userId, CancellationToken cancellationToken)
    {
        var profile = await GetProfileAsync(userId, cancellationToken);
        
        var (stories, _) = await _storyRepository.GetByAuthorAsync(userId, 1, 10, cancellationToken);
        var storiesList = stories
            .Select(s => new StoryListItem(
                s.Id,
                s.Title,
                s.CoverImageUrl,
                s.CreatedAt,
                s.Chapters?.Count ?? 0,
                s.Rating))
            .ToList();

        return new UserProfileExtended(profile, storiesList.AsReadOnly());
    }

    public async Task<Result<ProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result<ProfileResponse>.Failure("Пользователь не найден.");

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (request.Username.Length < 3)
                return Result<ProfileResponse>.Failure("Имя пользователя должно быть не менее 3 символов.");

            if (request.Username != user.Username && await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
                return Result<ProfileResponse>.Failure("Это имя пользователя уже занято.");

            user.Username = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!request.Email.Contains("@"))
                return Result<ProfileResponse>.Failure("Неверный формат email.");

            // Проверяем, изменился ли email (сравниваем без учёта регистра и пробелов)
            string newEmail = request.Email.Trim().ToLowerInvariant();
            string currentEmail = user.Email?.Trim().ToLowerInvariant() ?? "";
            bool emailChanged = newEmail != currentEmail;
            
            if (emailChanged && await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
                return Result<ProfileResponse>.Failure("Этот email уже зарегистрирован.");

            user.Email = request.Email.Trim();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("Profile updated for user {UserId}", userId);

        var stats = await _userRepository.GetUserStatsAsync(userId, cancellationToken);
        return Result<ProfileResponse>.Success(new ProfileResponse(
            user.Id,
            user.Username,
            user.Email,
            user.AvatarImageUrl,
            user.CreatedAt,
            stats.StoriesCount,
            stats.VotesCount,
            stats.CommentsCount));
    }

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure("Пользователь не найден.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return Result.Failure("Текущий пароль неверен.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return Result.Failure("Новый пароль должен содержать не менее 6 символов.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> UploadAvatarAsync(int userId, IFormFile avatar, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure("Пользователь не найден.");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            return Result.Failure("Разрешены только изображения: JPG, PNG, WEBP");

        // Удаляем старый аватар если есть
        if (!string.IsNullOrWhiteSpace(user.AvatarImageUrl))
        {
            var oldAvatarPath = Path.Combine(_env.ContentRootPath, user.AvatarImageUrl.TrimStart('/'));
            if (File.Exists(oldAvatarPath))
                File.Delete(oldAvatarPath);
        }

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads", "avatars");
        Directory.CreateDirectory(uploadsFolder);
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await avatar.CopyToAsync(stream, cancellationToken);

        user.AvatarImageUrl = $"/uploads/avatars/{fileName}";
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Avatar uploaded for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<Result> DeleteAccountAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        if (user == null)
            return Result.Failure("Пользователь не найден.");

        // Удаляем все файлы аватара
        if (!string.IsNullOrWhiteSpace(user.AvatarImageUrl))
        {
            var avatarPath = Path.Combine(_env.ContentRootPath, user.AvatarImageUrl.TrimStart('/'));
            if (File.Exists(avatarPath))
                File.Delete(avatarPath);
        }

        await _userRepository.DeleteAsync(userId, cancellationToken);
        _logger.LogInformation("Account deleted for user {UserId}", userId);
        return Result.Success();
    }

    public async Task<PagedResponse<VoteHistoryResponse>> GetVoteHistoryAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var skip = (page - 1) * pageSize;
        var pagedVotes = await _voteRepository.GetVotesByUserAsync(userId, skip, pageSize, cancellationToken);

        var voteHistory = pagedVotes.Items.Select(v => new VoteHistoryResponse(
            v.ChoiceId,
            v.Choice.ChapterId,
            v.Choice.Chapter.StoryId,
            v.Choice.Chapter.Story.Title,
            $"Глава {v.Choice.Chapter.SequenceNumber}",
            v.SelectedOption,
            v.SelectedOption == 1 ? v.Choice.Option1Text : v.Choice.Option2Text,
            v.Choice.IsClosed,
            v.Choice.WinningOption,
            v.Choice.Option1Votes,
            v.Choice.Option2Votes,
            v.Choice.ExpiresAt // Используем дату истечения выбора как дату голосования
        )).ToList();

        return new PagedResponse<VoteHistoryResponse>(voteHistory.AsReadOnly(), pagedVotes.TotalCount, page, pageSize);
    }

    public async Task<PagedResponse<StoryListItem>> GetMyStoriesAsync(int userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var (stories, totalCount) = await _storyRepository.GetByAuthorAsync(userId, (page - 1) * pageSize, pageSize, cancellationToken);
        
        var storyList = stories
            .Select(s => new StoryListItem(
                s.Id,
                s.Title,
                s.CoverImageUrl,
                s.CreatedAt,
                s.Chapters?.Count ?? 0,
                s.Rating))
            .ToList();

        return new PagedResponse<StoryListItem>(storyList.AsReadOnly(), totalCount, page, pageSize);
    }
}