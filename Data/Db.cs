using MySqlConnector;
using System.Text.Json;

namespace RUGENDO_WinForm.Data;

public static class Db
{
    static string? _cs;

    public static string ConnectionString
    {
        get
        {
            if (_cs == null)
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                _cs = doc.RootElement.GetProperty("ConnectionString").GetString()!;
            }
            return _cs;
        }
    }

    public static MySqlConnection Open()
    {
        var c = new MySqlConnection(ConnectionString);
        c.Open();
        return c;
    }
}
