using DevExpress.XtraSplashScreen;
using System;
using System.Windows.Forms;

namespace winforms_templates
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SplashScreenManager.ShowForm(typeof(LoadingScreen));

            frmMain mainForm = new frmMain();
            mainForm.Shown += MainForm_Shown;
            Application.Run(mainForm);
        }

        private static void MainForm_Shown(object sender, EventArgs e)
        {
            SplashScreenManager.CloseForm();
        }
    }

}
