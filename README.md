# LivePlot

Социальная платформа для интерактивных историй, где сообщество формирует сюжет в реальном времени через бинарный выбор.

## Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server

## Локальный запуск

### 0. Склонируйте репозиторий
`git clone https://github.com/george-leontev/story-maker-api.git`

### 1. Создайте базу данных

Откройте SQL Server Management Studio (SSMS), подключитесь к вашему SQL Server и выполните:

```sql
CREATE DATABASE LivePlotDB;
```

### 2. Настройте подключение

Скопируйте файл `.env.example` в `.env`:

Откройте `.env` и укажите ваши данные:

```
DB_SERVER=127.0.0.1
DB_PORT=1433
DB_NAME=LivePlotDB
DB_USER=sa
DB_PASSWORD=your_password
```

> **Файл `.env` не отслеживается git.** Не коммитьте его в репозиторий.

#### Пример строки подключения

**Локальный SQL Server:**
```
DB_SERVER=localhost
DB_PORT=1433
```

Если ваш формат подключения отличается — отредактируйте шаблон connection string в `Program.cs` и appsettings.json.

### 3. Примените миграции

```bash
dotnet restore
dotnet ef database update
```

### 4. Запустите приложение

```bash
dotnet run
```

API будет доступен по адресу  `http://localhost:5157`.

Swagger UI: `http://localhost:5157/swagger`
