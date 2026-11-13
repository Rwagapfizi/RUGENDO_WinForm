namespace RUGENDO_WinForm.Views;

public class LoginForm : Form
{
    readonly TextBox _email = Ui.Txt(260);
    readonly TextBox _password = Ui.Txt(260, true);
    readonly AuthController _auth = new();

    public LoginForm()
    {
        Text = "RUGENDO – Login";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(340, 300);

        var login = Ui.Btn("Log in", OnLogin);
        var register = Ui.Btn("Create customer account", OnRegister);
        AcceptButton = login;

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(30, 20, 20, 10) };
        flow.Controls.Add(new Label { Text = "RUGENDO", Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true });
        flow.Controls.Add(new Label { Text = "Bus ticket booking & management", AutoSize = true, ForeColor = Color.Gray });
        flow.Controls.Add(Ui.Lbl("Email"));
        flow.Controls.Add(_email);
        flow.Controls.Add(Ui.Lbl("Password"));
        flow.Controls.Add(_password);
        flow.Controls.Add(login);
        flow.Controls.Add(register);
        Controls.Add(flow);
    }

    void OnLogin(object? s, EventArgs e)
    {
        var r = _auth.Login(_email.Text, _password.Text);
        if (!r.Ok) { Ui.Error(r.Message); return; }
        DialogResult = DialogResult.OK;
    }

    void OnRegister(object? s, EventArgs e)
    {
        using var f = new RegisterForm();
        f.ShowDialog(this);
    }
}
