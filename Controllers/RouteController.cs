namespace RUGENDO_WinForm.Controllers;

/// <summary>FR-05 bus route management (workers, own company only).</summary>
public class RouteController
{
    readonly RouteRepository _routes = new();

    public Result<List<BusRoute>> List() => Safe.Run(() =>
    {
        var w = Session.Worker;
        if (w == null) return Result<List<BusRoute>>.Fail("Access denied.");
        return Result<List<BusRoute>>.Success(_routes.ListByCompany(w.CompanyId!.Value));
    });

    public Result Create(string origin, string destination) => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        var err = Check(origin, destination); if (err != null) return Result.Fail(err);

        _routes.Insert(new BusRoute { CompanyId = w.CompanyId!.Value, Origin = origin.Trim(), Destination = destination.Trim(), Active = true });
        return Result.Success("Route created.");
    });

    public Result Update(int id, string origin, string destination, bool active) => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        var err = Check(origin, destination); if (err != null) return Result.Fail(err);

        int rows = _routes.Update(new BusRoute
        { Id = id, CompanyId = w.CompanyId!.Value, Origin = origin.Trim(), Destination = destination.Trim(), Active = active });
        return rows == 1 ? Result.Success("Route updated.") : Result.Fail("Route not found for your company.");
    });

    public Result Remove(int id) => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        int rows = _routes.Deactivate(id, w.CompanyId!.Value);
        return rows == 1 ? Result.Success("Route removed from operation (history is kept).")
                         : Result.Fail("Route not found for your company.");
    });

    static string? Check(string o, string d)
    {
        if (string.IsNullOrWhiteSpace(o) || string.IsNullOrWhiteSpace(d)) return "Origin and destination are required.";
        if (string.Equals(o.Trim(), d.Trim(), StringComparison.OrdinalIgnoreCase)) return "Origin and destination must differ.";
        return null;
    }
}
