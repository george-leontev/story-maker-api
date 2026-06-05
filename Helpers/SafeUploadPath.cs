namespace StoryMakerApi.Helpers;

public static class SafeUploadPath
{
    /// <summary>
    /// Resolves a relative upload URL (e.g. "/uploads/avatars/<guid>.jpg") to a full path
    /// only if it lies inside ContentRoot/uploads/{subfolder}. Returns null otherwise.
    /// Prevents path traversal via "../" segments or absolute URLs supplied by clients.
    /// </summary>
    public static string? ResolveOrNull(string? relativeUrl, string contentRoot, string subfolder)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl)) return null;

        var allowedRoot = Path.GetFullPath(Path.Combine(contentRoot, "uploads", subfolder));
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(Path.Combine(contentRoot, relativeUrl.TrimStart('/', '\\')));
        }
        catch
        {
            return null;
        }

        var rootWithSep = allowedRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase) ? fullPath : null;
    }

    public static void TryDelete(string? relativeUrl, string contentRoot, string subfolder)
    {
        var path = ResolveOrNull(relativeUrl, contentRoot, subfolder);
        if (path != null && File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
