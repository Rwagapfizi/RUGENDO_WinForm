namespace RUGENDO_WinForm.Data;

public class CompanyRepository
{
    public Company? GetById(int id)
    {
        using var c = Db.Open();
        return c.QuerySingleOrDefault<Company>("SELECT * FROM companies WHERE id = @id", new { id });
    }

    public int Insert(Company co)
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>(
            "INSERT INTO companies (name, email, phone) VALUES (@Name, @Email, @Phone); SELECT LAST_INSERT_ID();", co);
    }

    public int Count()
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>("SELECT COUNT(*) FROM companies");
    }
}
