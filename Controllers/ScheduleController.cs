namespace RUGENDO_WinForm.Controllers;

/// <summary>FR-06 schedule management (workers) and FR-07 trip search (customers).</summary>
public class ScheduleController
{
    readonly ScheduleRepository _schedules = new();
    readonly RouteRepository _routes = new();
    readonly TicketFormatRepository _formats = new();

    public Result<List<ScheduleRow>> List() => Safe.Run(() =>
    {
        var w = Session.Worker;
        if (w == null) return Result<List<ScheduleRow>>.Fail("Access denied.");
        return Result<List<ScheduleRow>>.Success(_schedules.ListByCompany(w.CompanyId!.Value));
    });

    public Result Create(BusRoute? route, TicketFormat? format, DateTime date, TimeSpan time, string plate, string seats)
        => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        var err = Check(route, format, date, time, plate, seats, w.CompanyId!.Value, out int total);
        if (err != null) return Result.Fail(err);

        _schedules.Insert(new Schedule
        {
            RouteId = route!.Id, TicketFormatId = format!.Id, DepartureDate = date.Date,
            DepartureTime = time, BusPlate = plate.Trim().ToUpper(), TotalSeats = total
        });
        return Result.Success("Schedule created.");
    });

    public Result Update(int id, BusRoute? route, TicketFormat? format, DateTime date, TimeSpan time, string plate, string seats)
        => Safe.Run(() =>
    {
        var w = Session.Worker; if (w == null) return Result.Fail("Access denied.");
        var err = Check(route, format, date, time, plate, seats, w.CompanyId!.Value, out int total);
        if (err != null) return Result.Fail(err);

        int rows = _schedules.Update(new Schedule
        {
            Id = id, RouteId = route!.Id, TicketFormatId = format!.Id, DepartureDate = date.Date,
            DepartureTime = time, BusPlate = plate.Trim().ToUpper(), TotalSeats = total
        }, w.CompanyId!.Value);

        return rows == 1 ? Result.Success("Schedule updated.")
            : Result.Fail("Update failed: schedule not found for your company, or capacity is below seats already sold.");
    });

    /// <summary>Customer-facing search. Origin/destination may be partial text.</summary>
    public Result<List<TripResult>> Search(string origin, string destination, DateTime? date) => Safe.Run(() =>
    {
        if (Session.Customer == null) return Result<List<TripResult>>.Fail("Only customers can search for trips.");
        if (string.IsNullOrWhiteSpace(origin) && string.IsNullOrWhiteSpace(destination))
            return Result<List<TripResult>>.Fail("Enter a departure and/or destination location.");
        return Result<List<TripResult>>.Success(_schedules.Search(origin, destination, date));
    });

    string? Check(BusRoute? route, TicketFormat? format, DateTime date, TimeSpan time, string plate, string seats,
                  int companyId, out int total)
    {
        total = 0;
        if (route == null || format == null) return "Select a route and a ticket format.";
        // Re-check ownership server-side: never trust what the UI sent.
        if (_routes.GetForCompany(route.Id, companyId) == null) return "That route does not belong to your company.";
        if (_formats.GetForCompany(format.Id, companyId) == null) return "That ticket format does not belong to your company.";
        if (!route.Active) return "Cannot schedule an inactive route.";
        if (string.IsNullOrWhiteSpace(plate)) return "Bus plate number is required.";
        if (!int.TryParse(seats, out total) || total < 1 || total > 100) return "Seats must be a whole number between 1 and 100.";
        if (date.Date + time <= DateTime.Now) return "Departure must be in the future.";
        return null;
    }
}
