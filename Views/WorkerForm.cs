namespace RUGENDO_WinForm.Views;

public class WorkerForm : Form
{
    readonly CompanyController _company = new();
    readonly RouteController _routes = new();
    readonly TicketFormatController _formats = new();
    readonly ScheduleController _schedules = new();
    readonly TicketController _tickets = new();
    readonly AuthController _auth = new();

    // Company
    readonly Label _companyInfo = new() { AutoSize = true, Font = new Font("Segoe UI", 12), Padding = new Padding(15) };

    // Routes
    readonly TextBox _rOrigin = Ui.Txt(130), _rDest = Ui.Txt(130);
    readonly CheckBox _rActive = new() { Text = "Active", Checked = true, AutoSize = true, Margin = new Padding(8, 8, 2, 3) };
    readonly DataGridView _rGrid = Ui.Grid();

    // Ticket formats
    readonly TextBox _fName = Ui.Txt(110), _fDesc = Ui.Txt(200), _fPrice = Ui.Txt(80);
    readonly DataGridView _fGrid = Ui.Grid();

    // Schedules
    readonly ComboBox _sRoute = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox _sFormat = new() { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly DateTimePicker _sDate = new() { Format = DateTimePickerFormat.Short, Width = 105 };
    readonly DateTimePicker _sTime = new() { Format = DateTimePickerFormat.Custom, CustomFormat = "HH:mm", ShowUpDown = true, Width = 65 };
    readonly TextBox _sPlate = Ui.Txt(90), _sSeats = Ui.Txt(45);
    readonly DataGridView _sGrid = Ui.Grid();

    // Tickets
    readonly DataGridView _tGrid = Ui.Grid();

    public WorkerForm()
    {
        Text = $"RUGENDO – Worker: {Session.Current!.FullName}";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 600);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildCompanyTab());
        tabs.TabPages.Add(BuildRoutesTab());
        tabs.TabPages.Add(BuildFormatsTab());
        tabs.TabPages.Add(BuildSchedulesTab());
        tabs.TabPages.Add(BuildTicketsTab());

        var logout = Ui.Btn("Log out", (_, _) => { _auth.Logout(); Close(); });
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
        top.Controls.Add(logout);

        Controls.Add(tabs);
        Controls.Add(top);

        Load += (_, _) => { LoadCompany(); LoadRoutes(); LoadFormats(); LoadSchedules(); LoadTickets(); };
    }

    // ---------------- Company ----------------
    TabPage BuildCompanyTab()
    {
        var p = new TabPage("Company");
        p.Controls.Add(_companyInfo);
        return p;
    }

    void LoadCompany()
    {
        var r = _company.GetMyCompany();
        _companyInfo.Text = r.Ok
            ? $"Company:  {r.Value!.Name}\nEmail:      {r.Value.Email}\nPhone:      {r.Value.Phone}"
            : r.Message;
    }

    // ---------------- Routes ----------------
    TabPage BuildRoutesTab()
    {
        var bar = Ui.Bar();
        bar.Controls.Add(Ui.Lbl("Origin")); bar.Controls.Add(_rOrigin);
        bar.Controls.Add(Ui.Lbl("Destination")); bar.Controls.Add(_rDest);
        bar.Controls.Add(_rActive);
        bar.Controls.Add(Ui.Btn("Add", (_, _) =>
        {
            var r = _routes.Create(_rOrigin.Text, _rDest.Text);
            Ui.Show(r); if (r.Ok) { LoadRoutes(); ClearRouteFields(); }
        }));
        bar.Controls.Add(Ui.Btn("Update selected", (_, _) =>
        {
            var sel = Ui.Selected<BusRoute>(_rGrid); if (sel == null) { Ui.Error("Select a route first."); return; }
            var r = _routes.Update(sel.Id, _rOrigin.Text, _rDest.Text, _rActive.Checked);
            Ui.Show(r); if (r.Ok) LoadRoutes();
        }));
        bar.Controls.Add(Ui.Btn("Remove selected", (_, _) =>
        {
            var sel = Ui.Selected<BusRoute>(_rGrid); if (sel == null) { Ui.Error("Select a route first."); return; }
            if (MessageBox.Show($"Remove {sel.Label} from operation?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            var r = _routes.Remove(sel.Id);
            Ui.Show(r); if (r.Ok) LoadRoutes();
        }));
        _rGrid.SelectionChanged += (_, _) =>
        {
            var sel = Ui.Selected<BusRoute>(_rGrid); if (sel == null) return;
            _rOrigin.Text = sel.Origin; _rDest.Text = sel.Destination; _rActive.Checked = sel.Active;
        };

        var page = new TabPage("Routes");
        page.Controls.Add(Ui.Page(bar, _rGrid));
        return page;
    }

    void ClearRouteFields() { _rOrigin.Clear(); _rDest.Clear(); _rActive.Checked = true; }

    void LoadRoutes()
    {
        var r = _routes.List();
        if (!r.Ok) { Ui.Error(r.Message); return; }
        Ui.Bind(_rGrid, r.Value!);
        _sRoute.DataSource = r.Value!.ToList();   // keep schedule combo in sync
    }

    // ---------------- Ticket formats ----------------
    TabPage BuildFormatsTab()
    {
        var bar = Ui.Bar();
        bar.Controls.Add(Ui.Lbl("Name")); bar.Controls.Add(_fName);
        bar.Controls.Add(Ui.Lbl("Description")); bar.Controls.Add(_fDesc);
        bar.Controls.Add(Ui.Lbl("Price (RWF)")); bar.Controls.Add(_fPrice);
        bar.Controls.Add(Ui.Btn("Add", (_, _) =>
        {
            var r = _formats.Create(_fName.Text, _fDesc.Text, _fPrice.Text);
            Ui.Show(r); if (r.Ok) { LoadFormats(); _fName.Clear(); _fDesc.Clear(); _fPrice.Clear(); }
        }));
        bar.Controls.Add(Ui.Btn("Update selected", (_, _) =>
        {
            var sel = Ui.Selected<TicketFormat>(_fGrid); if (sel == null) { Ui.Error("Select a format first."); return; }
            var r = _formats.Update(sel.Id, _fName.Text, _fDesc.Text, _fPrice.Text);
            Ui.Show(r); if (r.Ok) LoadFormats();
        }));
        _fGrid.SelectionChanged += (_, _) =>
        {
            var sel = Ui.Selected<TicketFormat>(_fGrid); if (sel == null) return;
            _fName.Text = sel.Name; _fDesc.Text = sel.Description; _fPrice.Text = sel.Price.ToString("0.##");
        };

        var page = new TabPage("Ticket formats & pricing");
        page.Controls.Add(Ui.Page(bar, _fGrid));
        return page;
    }

    void LoadFormats()
    {
        var r = _formats.List();
        if (!r.Ok) { Ui.Error(r.Message); return; }
        Ui.Bind(_fGrid, r.Value!);
        _sFormat.DataSource = r.Value!.ToList();
    }

    // ---------------- Schedules ----------------
    TabPage BuildSchedulesTab()
    {
        var bar = Ui.Bar();
        bar.Controls.Add(Ui.Lbl("Route")); bar.Controls.Add(_sRoute);
        bar.Controls.Add(Ui.Lbl("Format")); bar.Controls.Add(_sFormat);
        bar.Controls.Add(Ui.Lbl("Date")); bar.Controls.Add(_sDate);
        bar.Controls.Add(Ui.Lbl("Time")); bar.Controls.Add(_sTime);
        bar.Controls.Add(Ui.Lbl("Bus plate")); bar.Controls.Add(_sPlate);
        bar.Controls.Add(Ui.Lbl("Seats")); bar.Controls.Add(_sSeats);
        bar.Controls.Add(Ui.Btn("Add", (_, _) =>
        {
            var r = _schedules.Create(_sRoute.SelectedItem as BusRoute, _sFormat.SelectedItem as TicketFormat,
                _sDate.Value.Date, _sTime.Value.TimeOfDay, _sPlate.Text, _sSeats.Text);
            Ui.Show(r); if (r.Ok) LoadSchedules();
        }));
        bar.Controls.Add(Ui.Btn("Update selected", (_, _) =>
        {
            var sel = Ui.Selected<ScheduleRow>(_sGrid); if (sel == null) { Ui.Error("Select a schedule first."); return; }
            var r = _schedules.Update(sel.Id, _sRoute.SelectedItem as BusRoute, _sFormat.SelectedItem as TicketFormat,
                _sDate.Value.Date, _sTime.Value.TimeOfDay, _sPlate.Text, _sSeats.Text);
            Ui.Show(r); if (r.Ok) LoadSchedules();
        }));
        _sGrid.SelectionChanged += (_, _) =>
        {
            var sel = Ui.Selected<ScheduleRow>(_sGrid); if (sel == null) return;
            //_sRoute.SelectedValue = sel.RouteId; _sFormat.SelectedValue = sel.TicketFormatId;
            _sRoute.SelectedItem = _sRoute.Items.Cast<BusRoute>().FirstOrDefault(x => x.Id == sel.RouteId);
            _sFormat.SelectedItem = _sFormat.Items.Cast<TicketFormat>().FirstOrDefault(x => x.Id == sel.TicketFormatId);
            _sDate.Value = sel.DepartureDate < _sDate.MinDate ? _sDate.MinDate : sel.DepartureDate;
            _sTime.Value = DateTime.Today + sel.DepartureTime;
            _sPlate.Text = sel.BusPlate; _sSeats.Text = sel.TotalSeats.ToString();
        };

        var page = new TabPage("Schedules");
        page.Controls.Add(Ui.Page(bar, _sGrid));
        return page;
    }

    void LoadSchedules()
    {
        var r = _schedules.List();
        if (!r.Ok) { Ui.Error(r.Message); return; }
        Ui.Bind(_sGrid, r.Value!);
    }

    // ---------------- Tickets ----------------
    TabPage BuildTicketsTab()
    {
        var bar = Ui.Bar();
        bar.Controls.Add(Ui.Btn("Refresh", (_, _) => { LoadTickets(); LoadSchedules(); }));
        var page = new TabPage("Company tickets");
        page.Controls.Add(Ui.Page(bar, _tGrid));
        return page;
    }

    void LoadTickets()
    {
        var r = _tickets.CompanyTickets();
        if (!r.Ok) { Ui.Error(r.Message); return; }
        Ui.Bind(_tGrid, r.Value!);
    }
}
