namespace RUGENDO_WinForm.Data;

public class TicketRepository
{
    const string DetailSelect = @"
        SELECT t.id, t.ticket_code, u.full_name AS customer_name, co.name AS company,
               r.origin, r.destination, s.departure_date, s.departure_time, s.bus_plate,
               t.seat_number, t.price_paid, t.purchased_at, t.status,
               COALESCE(rc.receipt_number, '') AS receipt_number
        FROM tickets t
        JOIN users u ON u.id = t.customer_id
        JOIN companies co ON co.id = t.company_id
        JOIN schedules s ON s.id = t.schedule_id
        JOIN routes r ON r.id = s.route_id
        LEFT JOIN receipts rc ON rc.ticket_id = t.id ";

    public class Locked
    {
        public int Id { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public DateTime DepartureDate { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public int CompanyId { get; set; }
        public bool Active { get; set; }
        public decimal Price { get; set; }
    }

    /// <summary>
    /// FR-09 / FR-12: validates availability, records the ticket, decrements seats and
    /// issues the receipt in ONE transaction. Row lock prevents double-selling a seat.
    /// </summary>
    public (TicketDetail? ticket, string error) Purchase(int customerId, int scheduleId)
    {
        using var c = Db.Open();
        using var tx = c.BeginTransaction();

        var s = c.QuerySingleOrDefault<Locked>(@"
            SELECT s.id, s.total_seats, s.available_seats, s.departure_date, s.departure_time,
                   r.company_id, r.active, tf.price
            FROM schedules s
            JOIN routes r ON r.id = s.route_id
            JOIN ticket_formats tf ON tf.id = s.ticket_format_id
            WHERE s.id = @scheduleId FOR UPDATE", new { scheduleId }, tx);

        if (s == null) return (null, "This trip no longer exists.");
        if (!s.Active) return (null, "This route is no longer in operation.");
        if (s.DepartureDate.Date + s.DepartureTime <= DateTime.Now) return (null, "This trip has already departed.");
        if (s.AvailableSeats <= 0) return (null, "Sorry, this trip is sold out.");

        int seat = s.TotalSeats - s.AvailableSeats + 1;
        string suffix = Guid.NewGuid().ToString("N")[..8].ToUpper();

        int ticketId = c.ExecuteScalar<int>(@"
            INSERT INTO tickets (ticket_code, customer_id, schedule_id, company_id, seat_number, price_paid)
            VALUES (@code, @customerId, @scheduleId, @companyId, @seat, @price);
            SELECT LAST_INSERT_ID();",
            new { code = "RGD-" + suffix, customerId, scheduleId, companyId = s.CompanyId, seat, price = s.Price }, tx);

        c.Execute("UPDATE schedules SET available_seats = available_seats - 1 WHERE id = @scheduleId",
            new { scheduleId }, tx);

        c.Execute(@"INSERT INTO receipts (ticket_id, receipt_number, amount)
                    VALUES (@ticketId, @num, @amount)",
            new { ticketId, num = "RCT-" + suffix, amount = s.Price }, tx);

        tx.Commit();
        return (GetDetail(ticketId), "");
    }

    public TicketDetail? GetDetail(int ticketId)
    {
        using var c = Db.Open();
        return c.QuerySingleOrDefault<TicketDetail>(DetailSelect + "WHERE t.id = @ticketId", new { ticketId });
    }

    public List<TicketDetail> ListForCustomer(int customerId)
    {
        using var c = Db.Open();
        return c.Query<TicketDetail>(DetailSelect + "WHERE t.customer_id = @customerId ORDER BY t.purchased_at DESC",
            new { customerId }).ToList();
    }

    public List<TicketDetail> ListForCompany(int companyId)
    {
        using var c = Db.Open();
        return c.Query<TicketDetail>(DetailSelect + "WHERE t.company_id = @companyId ORDER BY t.purchased_at DESC",
            new { companyId }).ToList();
    }
}
