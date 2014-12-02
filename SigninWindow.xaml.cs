using System.Collections.Specialized;
using System.Web;
using System.Windows;
using System.Windows.Navigation;

namespace OneDriveExplorer
{
    public partial class SigninWindow : Window
    {
        public SigninWindow()
        {
            InitializeComponent();
            WebBrowser.Navigate(LiveLogin.SignInUrl);
        }

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(e.Uri.Query);
            string code = queryString["code"];
            if (string.IsNullOrEmpty(code)) return;
            using (var login = new LiveLogin())
            {
                login.AuthorizationCode = code;
            }
            Close();
        }
    }
}