namespace NogoYa.API.Extensions;

/// <summary>
/// Render (and Heroku, Fly, Neon) provide a connection string as a single
/// `DATABASE_URL` env var in the form:
///   postgres://user:password@host:port/database
/// Npgsql's connection string format is different ("Host=...;Username=...;Password=...").
/// This helper performs the conversion when DATABASE_URL is present.
/// </summary>
public static class DatabaseUrlParser
{
    public static string? FromEnvironment()
    {
        var raw = Environment.GetEnvironmentVariable("DATABASE_URL");
        return string.IsNullOrWhiteSpace(raw) ? null : Convert(raw);
    }

    public static string Convert(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var db   = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        // SSL is required by all managed Postgres providers.
        return
            $"Host={uri.Host};Port={port};Database={db};" +
            $"Username={user};Password={pass};" +
            $"SSL Mode=Require;Trust Server Certificate=true;Include Error Detail=true";
    }
}
