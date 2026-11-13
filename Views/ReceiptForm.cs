namespace RUGENDO_WinForm.Views;

public class ReceiptForm : Form
{
    readonly string _text;

    public ReceiptForm(TicketDetail ticket)
    {
        _text = TicketController.BuildReceipt(ticket);
        Text = $"Receipt {ticket.ReceiptNumber}";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(480, 440);

        var box = new TextBox
        {
            Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 10),
            Text = _text.Replace("\r\n", "\n").Replace("\n", "\r\n"), BackColor = Color.White
        };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
        bar.Controls.Add(Ui.Btn("Save as .txt", OnSave));
        bar.Controls.Add(Ui.Btn("Close", (_, _) => Close()));

        Controls.Add(box);   // Fill first, then docked-edge controls
        Controls.Add(bar);
    }

    void OnSave(object? s, EventArgs e)
    {
        using var dlg = new SaveFileDialog { Filter = "Text file|*.txt", FileName = "receipt.txt" };
        if (dlg.ShowDialog(this) == DialogResult.OK) File.WriteAllText(dlg.FileName, _text);
    }
}
