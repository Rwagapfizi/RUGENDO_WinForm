namespace RUGENDO_WinForm.Views;

public class CustomerForm : Form
{
    readonly ScheduleController _schedules = new();
    readonly TicketController _tickets = new();
    readonly AuthController _auth = new();

    readonly TextBox _origin = Ui.Txt(120), _destination = Ui.Txt(120);
    readonly DateTimePicker _date = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false, Width = 120 };
    readonly DataGridView _tripsGrid = Ui.Grid();
    readonly DataGridView _myGrid = Ui.Grid();

    public CustomerForm()
    {
        Text = $"RUGENDO – Customer: {Session.Current!.FullName}";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(960, 540);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildSearchTab());
        tabs.TabPages.Add(BuildMyTicketsTab());
        tabs.SelectedIndexChanged += (_, _) => { if (tabs.SelectedIndex == 1) LoadMyTickets(); };

        var logout = Ui.Btn("Log out", (_, _) => { _auth.Logout(); Close(); });
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
        top.Controls.Add(logout);

        Controls.Add(tabs);  // Fill first
        Controls.Add(top);
    }

    TabPage BuildSearchTab()
    {
        var bar = Ui.Bar();
        bar.Controls.Add(Ui.Lbl("From")); bar.Controls.Add(_origin);
        bar.Controls.Add(Ui.Lbl("To")); bar.Controls.Add(_destination);
        bar.Controls.Add(Ui.Lbl("Date (optional)")); bar.Controls.Add(_date);
        bar.Controls.Add(Ui.Btn("Search trips", OnSearch));
        bar.Controls.Add(Ui.Btn("Buy selected ticket", OnBuy));

        var page = new TabPage("Search & Buy");
        page.Controls.Add(Ui.Page(bar, _tripsGrid));
        return page;
    }

    TabPage BuildMyTicketsTab()
    {
        var bar = Ui.Bar();
        bar.Controls.Add(Ui.Btn("Refresh", (_, _) => LoadMyTickets()));
        bar.Controls.Add(Ui.Btn("View receipt", OnViewReceipt));

        var page = new TabPage("My Tickets & History");
        page.Controls.Add(Ui.Page(bar, _myGrid));
        return page;
    }

    void OnSearch(object? s, EventArgs e)
    {
        DateTime? date = _date.Checked ? _date.Value.Date : null;
        var r = _schedules.Search(_origin.Text, _destination.Text, date);
        if (!r.Ok) { Ui.Error(r.Message); return; }
        Ui.Bind(_tripsGrid, r.Value!);
        if (r.Value!.Count == 0) Ui.Info("No matching trips found.");
    }

    void OnBuy(object? s, EventArgs e)
    {
        var trip = Ui.Selected<TripResult>(_tripsGrid);
        if (trip == null) { Ui.Error("Select a trip first."); return; }

        var confirm = MessageBox.Show(
            $"Buy a ticket for {trip.Origin} → {trip.Destination}\n{trip.DepartureDate:yyyy-MM-dd} {trip.DepartureTime:hh\\:mm} ({trip.Company})\nPrice: {trip.Price:N0} RWF?",
            "Confirm purchase", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        var r = _tickets.Purchase(trip.ScheduleId);
        if (!r.Ok) { Ui.Error(r.Message); return; }

        using (var rec = new ReceiptForm(r.Value!)) rec.ShowDialog(this);
        OnSearch(null, EventArgs.Empty);   // refresh seat counts
        LoadMyTickets();
    }

    void LoadMyTickets()
    {
        var r = _tickets.MyTickets();
        if (!r.Ok) { Ui.Error(r.Message); return; }
        Ui.Bind(_myGrid, r.Value!);
    }

    void OnViewReceipt(object? s, EventArgs e)
    {
        var t = Ui.Selected<TicketDetail>(_myGrid);
        if (t == null) { Ui.Error("Select a ticket first."); return; }
        using var rec = new ReceiptForm(t);
        rec.ShowDialog(this);
    }
}
