using System.Text.RegularExpressions;
using Npgsql;

class Program
{
    static readonly Regex UserRegex = new(@"^[A-Za-z0-9_.]{1,64}$", RegexOptions.Compiled);

    static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var pwd = new Stack<char>();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key is ConsoleKey.Backspace && pwd.Count > 0)
            {
                pwd.Pop();
                continue;
            }
            if (!char.IsControl(key.KeyChar)) pwd.Push(key.KeyChar);
        }
        Console.WriteLine();
        return new string(pwd.Reverse().ToArray());
    }

    static Npgsql.SslMode ParseSsl(string v) =>
        Enum.TryParse<Npgsql.SslMode>(v, ignoreCase: true, out var m) ? m : Npgsql.SslMode.Disable;

    static async Task<int> Main()
    {
        try
        {
            var cfg = AppConfig.Load();

            Console.Write("Введите логин: ");
            var username = Console.ReadLine()?.Trim() ?? "";
            if (!UserRegex.IsMatch(username))
            {
                Console.Error.WriteLine("Некорректный логин.");
                return 2;
            }

            var password = ReadPassword("Введите пароль: ");

            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = cfg.Host,
                Port = cfg.Port,
                Database = cfg.Database,
                Username = username,
                Password = password,
                SslMode = ParseSsl(cfg.SslMode),
                Timeout = cfg.TimeoutSeconds,
                CommandTimeout = cfg.CommandTimeoutSeconds,
            };

            await using var conn = new NpgsqlConnection(csb.ConnectionString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand("SELECT version();", conn);
            var version = await cmd.ExecuteScalarAsync();

            Console.WriteLine("PostgreSQL version:");
            Console.WriteLine(version?.ToString());

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка: {ex.Message}");
            return 1;
        }
    }
}
