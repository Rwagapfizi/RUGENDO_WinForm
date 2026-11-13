using RUGENDO_WinForm.Helpers;

namespace RUGENDO_WinForm.Helpers;

/// <summary>Small helpers so the forms stay readable.</summary>
public static class Ui
{
    public static Label Lbl(string text) =>
        new() { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(8, 9, 2, 3) };

    public static TextBox Txt(int width = 150, bool password = false) =>
        new() { Width = width, UseSystemPasswordChar = password, Margin = new Padding(2, 5, 6, 3) };

    public static Button Btn(string text, EventHandler onClick)
    {
        var b = new Button { Text = text, AutoSize = true, Padding = new Padding(8, 2, 8, 2), Margin = new Padding(6, 4, 2, 3) };
        b.Click += onClick;
        return b;
    }

    public static FlowLayoutPanel Bar() =>
        new() { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(6) };

    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
        BackgroundColor = SystemColors.Window
    };

    /// <summary>Top bar + grid that fills the rest of the tab.</summary>
    public static TableLayoutPanel Page(Control top, Control fill)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        t.Controls.Add(top, 0, 0);
        t.Controls.Add(fill, 0, 1);
        return t;
    }

    public static void Bind<T>(DataGridView g, IEnumerable<T> data)
    {
        g.DataSource = new BindingList<T>(data.ToList());
        foreach (DataGridViewColumn c in g.Columns)
        {
            if (c.ValueType == typeof(DateTime))
                c.DefaultCellStyle.Format = c.Name == "PurchasedAt" ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd";
            else if (c.ValueType == typeof(decimal))
                c.DefaultCellStyle.Format = "N0";
        }
    }

    public static T? Selected<T>(DataGridView g) where T : class => g.CurrentRow?.DataBoundItem as T;

    public static void Info(string m) => MessageBox.Show(m, "RUGENDO", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static void Error(string m) => MessageBox.Show(m, "RUGENDO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Show(Result r) { if (r.Ok) Info(r.Message); else Error(r.Message); }
}
