namespace RUGENDO_WinForm.Data;

/// <summary>Creates demo data on first run so the app can be tried immediately.</summary>
public static class DbSeeder
{
    public static void EnsureDemoData()
    {
        var companies = new CompanyRepository();
        var users = new UserRepository();
        if (companies.Count() > 0 || users.Count() > 0) return;

        int companyId = companies.Insert(new Company
        { Name = "Volcano Express", Email = "info@volcano.rw", Phone = "+250788000001" });

        users.Insert(new User
        {
            FullName = "Demo Worker", Email = "worker@rugendo.rw", Phone = "+250788000002",
            PasswordHash = PasswordHasher.Hash("Worker123"), Role = "WORKER", CompanyId = companyId
        });
        users.Insert(new User
        {
            FullName = "Demo Customer", Email = "customer@rugendo.rw", Phone = "+250788000003",
            PasswordHash = PasswordHasher.Hash("Customer123"), Role = "CUSTOMER"
        });

        var routes = new RouteRepository();
        int r1 = routes.Insert(new BusRoute { CompanyId = companyId, Origin = "Kigali", Destination = "Musanze", Active = true });
        int r2 = routes.Insert(new BusRoute { CompanyId = companyId, Origin = "Kigali", Destination = "Huye", Active = true });

        var formats = new TicketFormatRepository();
        int f1 = formats.Insert(new TicketFormat { CompanyId = companyId, Name = "Standard", Description = "Regular seat", Price = 3500 });
        int f2 = formats.Insert(new TicketFormat { CompanyId = companyId, Name = "VIP", Description = "Extra legroom", Price = 6000 });

        var schedules = new ScheduleRepository();
        var tomorrow = DateTime.Today.AddDays(1);
        schedules.Insert(new Schedule { RouteId = r1, TicketFormatId = f1, DepartureDate = tomorrow,
            DepartureTime = new TimeSpan(8, 0, 0), BusPlate = "RAH 123A", TotalSeats = 30 });
        schedules.Insert(new Schedule { RouteId = r2, TicketFormatId = f2, DepartureDate = tomorrow,
            DepartureTime = new TimeSpan(14, 30, 0), BusPlate = "RAG 456B", TotalSeats = 20 });
    }
}
