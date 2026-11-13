namespace RUGENDO_WinForm.Data;

public class TicketFormatRepository
{
    public List<TicketFormat> ListByCompany(int companyId)
    {
        using var c = Db.Open();
        return c.Query<TicketFormat>(
            "SELECT * FROM ticket_formats WHERE company_id = @companyId ORDER BY name",
            new { companyId }).ToList();
    }

    public TicketFormat? GetForCompany(int id, int companyId)
    {
        using var c = Db.Open();
        return c.QuerySingleOrDefault<TicketFormat>(
            "SELECT * FROM ticket_formats WHERE id = @id AND company_id = @companyId", new { id, companyId });
    }

    public int Insert(TicketFormat t)
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>(@"
            INSERT INTO ticket_formats (company_id, name, description, price)
            VALUES (@CompanyId, @Name, @Description, @Price);
            SELECT LAST_INSERT_ID();", t);
    }

    public int Update(TicketFormat t)
    {
        using var c = Db.Open();
        return c.Execute(@"
            UPDATE ticket_formats SET name = @Name, description = @Description, price = @Price
            WHERE id = @Id AND company_id = @CompanyId", t);
    }
}
