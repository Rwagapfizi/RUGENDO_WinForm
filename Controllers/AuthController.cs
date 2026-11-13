namespace RUGENDO_WinForm.Controllers;

/// <summary>FR-01 registration, FR-02 authentication, FR-03 roles.</summary>
public class AuthController
{
    readonly UserRepository _users = new();

    public Result Register(string fullName, string email, string phone, string password, string confirm)
    {
        fullName = fullName.Trim(); email = email.Trim().ToLowerInvariant(); phone = phone.Trim();

        if (fullName.Length < 3) return Result.Fail("Full name must be at least 3 characters.");
        if (!Validator.IsEmail(email)) return Result.Fail("Enter a valid email address.");
        if (!Validator.IsPhone(phone)) return Result.Fail("Enter a valid phone number (9–15 digits, optional +).");
        if (!Validator.IsStrongPassword(password))
            return Result.Fail("Password must be at least 8 characters and contain letters and digits.");
        if (password != confirm) return Result.Fail("Passwords do not match.");

        return Safe.Run(() =>
        {
            if (_users.GetByEmail(email) != null)
                return Result.Fail("An account with this email already exists.");

            _users.Insert(new User
            {
                FullName = fullName,
                Email = email,
                Phone = phone,
                PasswordHash = PasswordHasher.Hash(password),
                Role = "CUSTOMER"   // public sign-up = customer only
            });
            return Result.Success("Account created. You can now log in.");
        });
    }

    public Result Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            return Result.Fail("Enter your email and password.");

        return Safe.Run(() =>
        {
            var user = _users.GetByEmail(email.Trim().ToLowerInvariant());
            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                return Result.Fail("Invalid email or password.");

            Session.Start(user);
            return Result.Success($"Welcome, {user.FullName}!");
        });
    }

    public void Logout()
    {
        Session.End();
        Session.LogoutRequested = true;
    }
}
