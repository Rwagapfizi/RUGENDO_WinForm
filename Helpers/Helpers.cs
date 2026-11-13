using RUGENDO_WinForm.Models;
using System.Text.RegularExpressions;

namespace RUGENDO_WinForm.Helpers;

public record Result(bool Ok, string Message)
{
    public static Result Success(string message = "Done.") => new(true, message);
    public static Result Fail(string message) => new(false, message);
}

public record Result<T>(bool Ok, string Message, T? Value = default)
{
    public static Result<T> Success(T value, string message = "Done.") => new(true, message, value);
    public static Result<T> Fail(string message) => new(false, message);
}

public static class Session
{
    public static User? Current { get; private set; }
    public static bool LogoutRequested { get; set; }

    public static void Start(User u) { Current = u; LogoutRequested = false; }
    public static void End() => Current = null;

    public static User? Worker => Current is { Role: "WORKER", CompanyId: not null } ? Current : null;
    public static User? Customer => Current is { Role: "CUSTOMER" } ? Current : null;
}

public static class PasswordHasher
{
    public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public static bool Verify(string password, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}

public static class Validator
{
    static readonly Regex EmailRx = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    static readonly Regex PhoneRx = new(@"^\+?\d{9,15}$", RegexOptions.Compiled);

    public static bool IsEmail(string s) => EmailRx.IsMatch(s.Trim());
    public static bool IsPhone(string s) => PhoneRx.IsMatch(s.Trim());
    public static bool IsStrongPassword(string s) =>
        s.Length >= 8 && s.Any(char.IsLetter) && s.Any(char.IsDigit);
}

/// <summary>Turns unexpected database failures into friendly messages (FR-13).</summary>
public static class Safe
{
    public static Result Run(Func<Result> action)
    {
        try { return action(); }
        catch (MySqlException ex) { return Result.Fail($"Database error: {ex.Message}"); }
        catch (Exception ex) { return Result.Fail($"Unexpected error: {ex.Message}"); }
    }

    public static Result<T> Run<T>(Func<Result<T>> action)
    {
        try { return action(); }
        catch (MySqlException ex) { return Result<T>.Fail($"Database error: {ex.Message}"); }
        catch (Exception ex) { return Result<T>.Fail($"Unexpected error: {ex.Message}"); }
    }
}
