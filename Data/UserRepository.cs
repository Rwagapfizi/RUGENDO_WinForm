namespace RUGENDO_WinForm.Data;

public class UserRepository
{
    public User? GetByEmail(string email)
    {
        using var c = Db.Open();
        return c.QuerySingleOrDefault<User>("SELECT * FROM users WHERE email = @email", new { email });
    }

    public int Insert(User u)
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>(@"
            INSERT INTO users (full_name, email, phone, password_hash, role, company_id)
            VALUES (@FullName, @Email, @Phone, @PasswordHash, @Role, @CompanyId);
            SELECT LAST_INSERT_ID();", u);
    }

    public int Count()
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>("SELECT COUNT(*) FROM users");
    }
}
