using Dapper;
using RUGENDO_WinForm.Data;
using RUGENDO_WinForm.Helpers;
using RUGENDO_WinForm.Views;

namespace RUGENDO_WinForm
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            DefaultTypeMap.MatchNamesWithUnderscores = true;   // full_name -> FullName

            try { DbSeeder.EnsureDemoData(); }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot reach the MariaDB database.\n\nCheck appsettings.json and make sure database/schema.sql was executed.\n\n" + ex.Message,
                    "RUGENDO – Database error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Login -> dashboard -> (logout returns here, closing the window exits the app)
            while (true)
            {
                using var login = new LoginForm();
                if (login.ShowDialog() != DialogResult.OK) break;

                Form main = Session.Current!.Role == "WORKER" ? new WorkerForm() : new CustomerForm();
                Application.Run(main);
                main.Dispose();

                if (!Session.LogoutRequested) break;
            }
        }
    }
}