using DevExpress.XtraSplashScreen;
using System;

namespace winforms_templates
{
    public partial class LoadingScreen : SplashScreen
    {
        public LoadingScreen()
        {
            InitializeComponent();
        }

        #region Overrides

        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }

        #endregion Overrides

        public enum SplashScreenCommand
        {
        }
    }
}