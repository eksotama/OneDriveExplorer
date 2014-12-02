using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using OneDriveExplorer.Properties;

namespace OneDriveExplorer
{
    public class LiveLogin : IDisposable
    {
        public const string ApiUrl = @"https://apis.live.net/v5.0/";
        public const string ClientId = "0000000044131D5B";
        public const string ClientSecret = "cJRq77mNikfien-idd5TrlLddU7P2DxJ";
        public const string Scope = "wl.basic";

        public static string AccessTokenUrl =
            String.Format(
                @"https://login.live.com/oauth20_token.srf?client_id={0}&client_secret={1}&redirect_uri=https://login.live.com/oauth20_desktop.srf&grant_type=authorization_code&code=",
                ClientId, ClientSecret);

        public static Uri SignInUrl =
            new Uri(
                String.Format(
                    @"https://login.live.com/oauth20_authorize.srf?client_id={0}&redirect_uri=https://login.live.com/oauth20_desktop.srf&response_type=code&scope={1}",
                    ClientId, Scope));

        public event EventHandler<ErrorEventArgs> AccessTokenError;
        public event EventHandler AccessTokenReceived;
        public event EventHandler<ErrorEventArgs> UserInfoError;
        public event EventHandler UserInfoReceived;

        public string AccessToken
        {
            get { return Settings.Default.LiveAccessToken; }
        }

        public string AuthorizationCode
        {
            get { return Settings.Default.LiveAuthorizationCode; }
            set
            {
                Clear();
                Settings.Default.LiveAuthorizationCode = value;
            }
        }

        public bool IsLoggedIn
        {
            get { return !string.IsNullOrWhiteSpace(AuthorizationCode); }
        }

        public string UserInfo
        {
            get { return Settings.Default.LiveUserProfile; }
        }

        public void Clear()
        {
            Settings.Default.LiveAccessToken = null;
            Settings.Default.LiveAuthorizationCode = null;
            Settings.Default.LiveUserProfile = null;
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)

        public void GetAccessToken()
        {
            if (string.IsNullOrWhiteSpace(AuthorizationCode)) return;

            string requestUrl = AccessTokenUrl + AuthorizationCode;
            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    if (AccessTokenError != null)
                    {
                        AccessTokenError(this, new ErrorEventArgs(e.Error));
                    }
                }
                else if (!e.Cancelled)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.Result);
                    if (!data.ContainsKey("access_token")) return;
                    Settings.Default.LiveAccessToken = data["access_token"];
                    if (AccessTokenReceived != null)
                    {
                        AccessTokenReceived(this, EventArgs.Empty);
                    }
                    GetUserInfo();
                }
            };
            wc.DownloadStringAsync(new Uri(requestUrl));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (
                    Settings.Default.PropertyValues.Cast<SettingsPropertyValue>()
                        .Any(propertyValue => propertyValue.IsDirty))
                {
                    Settings.Default.Save();
                }
            }
        }

        private void GetUserInfo()
        {
            if (!IsLoggedIn) return;
            string requestUrl = ApiUrl + "me?access_token=" + AccessToken;

            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    if (UserInfoError != null)
                    {
                        UserInfoError(this, new ErrorEventArgs(e.Error));
                    }
                }
                else if (!e.Cancelled)
                {
                    if (UserInfoReceived != null)
                    {
                        UserInfoReceived(this, EventArgs.Empty);
                    }
                }
            };
            wc.DownloadStringAsync(new Uri(requestUrl));
        }
    }
}