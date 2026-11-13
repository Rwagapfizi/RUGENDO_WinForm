using RUGENDO_WinForm.Models;

namespace RUGENDO_WinForm.Data;

public class RouteRepository
{
    public List<BusRoute> ListByCompany(int companyId)
    {
        using var c = Db.Open();
        return c.Query<BusRoute>(
            "SELECT * FROM routes WHERE company_id = @companyId ORDER BY origin, destination",
            new { companyId }).ToList();
    }

    public BusRoute? GetForCompany(int id, int companyId)
    {
        using var c = Db.Open();
        return c.QuerySingleOrDefault<BusRoute>(
            "SELECT * FROM routes WHERE id = @id AND company_id = @companyId", new { id, companyId });
    }

    public int Insert(BusRoute r)
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>(@"
            INSERT INTO routes (company_id, origin, destination, active)
            VALUES (@CompanyId, @Origin, @Destination, @Active);
            SELECT LAST_INSERT_ID();", r);
    }

    public int Update(BusRoute r)
    {
        using var c = Db.Open();
        return c.Execute(@"
            UPDATE routes SET origin = @Origin, destination = @Destination, active = @Active
            WHERE id = @Id AND company_id = @CompanyId", r);
    }

    /// <summary>Routes with schedules/tickets must keep history, so removal = deactivation.</summary>
    public int Deactivate(int id, int companyId)
    {
        using var c = Db.Open();
        return c.Execute("UPDATE routes SET active = 0 WHERE id = @id AND company_id = @companyId",
            new { id, companyId });
    }
}
