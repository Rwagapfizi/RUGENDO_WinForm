using System.Text;

namespace RUGENDO_WinForm.Controllers;

/// <summary>FR-09 purchasing, FR-10 ticket management, FR-11 booking history, FR-12 receipts.</summary>
public class TicketController
{
    readonly TicketRepository _tickets = new();

    public Result<TicketDetail> Purchase(int scheduleId) => Safe.Run(() =>
    {
        var cust = Session.Customer;
        if (cust == null) return Result<TicketDetail>.Fail("Only logged-in customers can purchase tickets.");

        var (ticket, error) = _tickets.Purchase(cust.Id, scheduleId);
        return ticket == null ? Result<TicketDetail>.Fail(error)
                              : Result<TicketDetail>.Success(ticket, "Ticket purchased successfully!");
    });

    /// <summary>Customer's purchased tickets = their booking history.</summary>
    public Result<List<TicketDetail>> MyTickets() => Safe.Run(() =>
    {
        var cust = Session.Customer;
        if (cust == null) return Result<List<TicketDetail>>.Fail("Access denied.");
        return Result<List<TicketDetail>>.Success(_tickets.ListForCustomer(cust.Id));
    });

    public Result<List<TicketDetail>> CompanyTickets() => Safe.Run(() =>
    {
        var w = Session.Worker;
        if (w == null) return Result<List<TicketDetail>>.Fail("Access denied.");
        return Result<List<TicketDetail>>.Success(_tickets.ListForCompany(w.CompanyId!.Value));
    });

    public static string BuildReceipt(TicketDetail t)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("            RUGENDO - RECEIPT");
        sb.AppendLine("========================================");
        sb.AppendLine($"Receipt No : {t.ReceiptNumber}");
        sb.AppendLine($"Ticket No  : {t.TicketCode}");
        sb.AppendLine($"Issued     : {t.PurchasedAt:yyyy-MM-dd HH:mm}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Passenger  : {t.CustomerName}");
        sb.AppendLine($"Company    : {t.Company}");
        sb.AppendLine($"Route      : {t.Origin} -> {t.Destination}");
        sb.AppendLine($"Departure  : {t.DepartureDate:yyyy-MM-dd} {t.DepartureTime:hh\\:mm}");
        sb.AppendLine($"Bus plate  : {t.BusPlate}");
        sb.AppendLine($"Seat       : {t.SeatNumber}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"AMOUNT PAID: {t.PricePaid:N0} RWF");
        sb.AppendLine($"Status     : {t.Status}");
        sb.AppendLine("========================================");
        sb.AppendLine("  Thank you for travelling with RUGENDO");
        return sb.ToString();
    }
}
