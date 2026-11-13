namespace RUGENDO_WinForm.Views;

public class RegisterForm : Form
{
    readonly TextBox _name = Ui.Txt(240), _email = Ui.Txt(240), _phone = Ui.Txt(240);
    readonly TextBox _pw = Ui.Txt(240, true), _confirm = Ui.Txt(240, true);
    readonly AuthController _auth = new();

    public RegisterForm()
    {
        Text = "RUGENDO – Create account";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false;
        ClientSize = new Size(340, 430);

        var ok = Ui.Btn("Register", OnRegister);
        var cancel = Ui.Btn("Cancel", (_, _) => Close());
        AcceptButton = ok; CancelButton = cancel;

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(30, 15, 20, 10) };
        foreach (var (label, box) in new (string, TextBox)[]
        {
            ("Full name", _name), ("Email", _email), ("Phone (e.g. +250788123456)", _phone),
            ("Password (8+ chars, letters & digits)", _pw), ("Confirm password", _confirm)
        })
        {
            flow.Controls.Add(Ui.Lbl(label));
            flow.Controls.Add(box);
        }
        var buttons = new FlowLayoutPanel { AutoSize = true };
        buttons.Controls.Add(ok); buttons.Controls.Add(cancel);
        flow.Controls.Add(buttons);
        Controls.Add(flow);
    }

    void OnRegister(object? s, EventArgs e)
    {
        var r = _auth.Register(_name.Text, _email.Text, _phone.Text, _pw.Text, _confirm.Text);
        Ui.Show(r);
        if (r.Ok) Close();
    }
}
