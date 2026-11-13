namespace RUGENDO_WinForm.Data;

public class ScheduleRepository
{
    public List<ScheduleRow> ListByCompany(int companyId)
    {
        using var c = Db.Open();
        return c.Query<ScheduleRow>(@"
            SELECT s.id, s.route_id, s.ticket_format_id,
                   CONCAT(r.origin, ' → ', r.destination) AS route,
                   tf.name AS format, s.departure_date, s.departure_time, s.bus_plate,
                   s.total_seats, s.available_seats, tf.price
            FROM schedules s
            JOIN routes r ON r.id = s.route_id
            JOIN ticket_formats tf ON tf.id = s.ticket_format_id
            WHERE r.company_id = @companyId
            ORDER BY s.departure_date, s.departure_time", new { companyId }).ToList();
    }

    public int Insert(Schedule s)
    {
        using var c = Db.Open();
        return c.ExecuteScalar<int>(@"
            INSERT INTO schedules (route_id, ticket_format_id, departure_date, departure_time,
                                   bus_plate, total_seats, available_seats)
            VALUES (@RouteId, @TicketFormatId, @DepartureDate, @DepartureTime,
                    @BusPlate, @TotalSeats, @TotalSeats);
            SELECT LAST_INSERT_ID();", s);
    }

    /// <summary>
    /// Updates a schedule whose current route belongs to the company. This is a single-table
    /// UPDATE so MariaDB evaluates SET assignments left to right: available_seats is assigned
    /// BEFORE total_seats. Returns 0 rows if the new capacity is below the seats already sold.
    /// </summary>
    public int Update(Schedule s, int companyId)
    {
        using var c = Db.Open();
        return c.Execute(@"
            UPDATE schedules
            SET available_seats = available_seats + (@TotalSeats - total_seats),
                total_seats = @TotalSeats,
                route_id = @RouteId, ticket_format_id = @TicketFormatId,
                departure_date = @DepartureDate, departure_time = @DepartureTime,
                bus_plate = @BusPlate
            WHERE id = @Id
              AND route_id IN (SELECT id FROM routes WHERE company_id = @companyId)
              AND (available_seats + (@TotalSeats - total_seats)) >= 0",
            new
            {
                s.Id,
                s.RouteId,
                s.TicketFormatId,
                s.DepartureDate,
                s.DepartureTime,
                s.BusPlate,
                s.TotalSeats,
                companyId
            });
    }

    public List<TripResult> Search(string origin, string destination, DateTime? date)
    {
        using var c = Db.Open();
        return c.Query<TripResult>(@"
            SELECT s.id AS schedule_id, co.name AS company, r.origin, r.destination,
                   s.departure_date, s.departure_time, tf.price, s.available_seats, s.bus_plate
            FROM schedules s
            JOIN routes r ON r.id = s.route_id AND r.active = 1
            JOIN companies co ON co.id = r.company_id
            JOIN ticket_formats tf ON tf.id = s.ticket_format_id
            WHERE r.origin LIKE @o AND r.destination LIKE @d
              AND (@date IS NULL OR s.departure_date = @date)
              AND TIMESTAMP(s.departure_date, s.departure_time) > NOW()
              AND s.available_seats > 0
            ORDER BY s.departure_date, s.departure_time",
            new { o = $"%{origin.Trim()}%", d = $"%{destination.Trim()}%", date }).ToList();
    }
}
