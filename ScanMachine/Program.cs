using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using DevExpress.XtraSplashScreen;
using Forms;
using System;
using System.Collections.Generic;
using System.Linq;
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
            SplashScreenManager.ShowForm(typeof(BaseSplashScreen));

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
