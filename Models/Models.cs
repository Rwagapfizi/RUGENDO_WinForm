namespace RUGENDO_WinForm.Models;

public class User
{
    [Browsable(false)] public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    [Browsable(false)] public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "CUSTOMER";   // CUSTOMER | WORKER
    [Browsable(false)] public int? CompanyId { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}

public class BusRoute
{
    [Browsable(false)] public int Id { get; set; }
    [Browsable(false)] public int CompanyId { get; set; }
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public bool Active { get; set; }
    [Browsable(false)] public string Label => $"{Origin} → {Destination}";
    public override string ToString() => Label;
}

public class TicketFormat
{
    [Browsable(false)] public int Id { get; set; }
    [Browsable(false)] public int CompanyId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    [Browsable(false)] public string Label => $"{Name} ({Price:N0} RWF)";
    public override string ToString() => Label;
}

public class Schedule
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int TicketFormatId { get; set; }
    public DateTime DepartureDate { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public string BusPlate { get; set; } = "";
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
}

/// <summary>Schedule joined with route and ticket format, for display to workers.</summary>
public class ScheduleRow
{
    [Browsable(false)] public int Id { get; set; }
    [Browsable(false)] public int RouteId { get; set; }
    [Browsable(false)] public int TicketFormatId { get; set; }
    public string Route { get; set; } = "";
    public string Format { get; set; } = "";
    public DateTime DepartureDate { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public string BusPlate { get; set; } = "";
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
}

/// <summary>A search result shown to customers (FR-07).</summary>
public class TripResult
{
    [Browsable(false)] public int ScheduleId { get; set; }
    public string Company { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateTime DepartureDate { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public string BusPlate { get; set; } = "";
}

public class TicketDetail
{
    [Browsable(false)] public int Id { get; set; }
    public string TicketCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Company { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateTime DepartureDate { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public string BusPlate { get; set; } = "";
    public int SeatNumber { get; set; }
    public decimal PricePaid { get; set; }
    public DateTime PurchasedAt { get; set; }
    public string Status { get; set; } = "";
    public string ReceiptNumber { get; set; } = "";
}
