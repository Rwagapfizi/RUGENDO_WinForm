namespace RUGENDO_WinForm.Controllers;

/// <summary>FR-08 ticket format and pricing management.</summary>
public class TicketFormatController
{
    readonly TicketFormatRepository _formats = new();

    public Result<List<TicketFormat>> List() => Safe.Run(() =>
    {
        var w = Session.Worker;
        if (w == null) return Result<List<TicketFormat>>.Fail("Access denied.");
        return Result<List<TicketFormat>>.Success(_formats.ListByCompany(w.CompanyId!.Value));
    });

    public Result Create(string name, string description, string price) => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        var err = Check(name, price, out var p); if (err != null) return Result.Fail(err);

        _formats.Insert(new TicketFormat { CompanyId = w.CompanyId!.Value, Name = name.Trim(), Description = description.Trim(), Price = p });
        return Result.Success("Ticket format created.");
    });

    public Result Update(int id, string name, string description, string price) => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        var err = Check(name, price, out var p); if (err != null) return Result.Fail(err);

        int rows = _formats.Update(new TicketFormat
        { Id = id, CompanyId = w.CompanyId!.Value, Name = name.Trim(), Description = description.Trim(), Price = p });
        return rows == 1 ? Result.Success("Ticket format updated.") : Result.Fail("Ticket format not found for your company.");
    });

    static string? Check(string name, string price, out decimal p)
    {
        p = 0;
        if (string.IsNullOrWhiteSpace(name)) return "Format name is required.";
        if (!decimal.TryParse(price, out p) || p <= 0) return "Price must be a number greater than zero.";
        return null;
    }
}
