namespace Common
{
    public class Global
    {
        #region Private Fields

        private static string _ConnectionString;
        private static string _AppUserName;
        private static int _UserID;

        #endregion Private Fields

        #region Public Properties

        public static string ConnectionString
        {
            get { return _ConnectionString; }
            set { _ConnectionString = value; }
        }

        public static string AppUserName
        {
            get { return _AppUserName; }
            set { _AppUserName = value; }
        }

        public static int UserID
        {
            get { return _UserID; }
            set { _UserID = value; }
        }

        #endregion Public Properties
    }
}