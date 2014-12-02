using System;
using System.IO;
using System.Windows;
using OneDriveExplorer.Properties;

namespace OneDriveExplorer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OneDriveFileSystem _fileSystem = new OneDriveFileSystem();
        private readonly LiveLogin _login = new LiveLogin();
        private readonly ViewModel _model = new ViewModel();

        public MainWindow()
        {
            InitializeComponent();
            _fileSystem.StatusChanged += _fileSystem_StatusChanged;
            _model.StateChanged += _model_StateChanged;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _model.CurrentState =
                _login.IsLoggedIn
                    ? (Directory.Exists(string.Format("{0}://", _fileSystem.Letter))
                        ? ViewModel.State.Mounted
                        : ViewModel.State.Authenticated)
                    : ViewModel.State.Unauthenticated;
        }

        private void Mount(object sender, RoutedEventArgs routedEventArgs)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _model.CurrentState = ViewModel.State.Mounting;
                _fileSystem.Mount();
            }));
        }

        private void SignIn(object sender, RoutedEventArgs routedEventArgs)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _model.CurrentState = ViewModel.State.Authenticating;
                var window = new SigninWindow();
                window.Closed +=
                    (o, e) =>
                    {
                        Settings.Default.Reload();
                        _model.CurrentState = _login.IsLoggedIn
                            ? ViewModel.State.Authenticated
                            : ViewModel.State.Unauthenticated;
                    };
                window.ShowDialog();
            }));
        }

        private void Unmount(object sender, RoutedEventArgs routedEventArgs)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _model.CurrentState = ViewModel.State.Unmounting;
                _fileSystem.Unmount();
            }));
        }

        private void _fileSystem_StatusChanged(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (_fileSystem.Status)
                {
                    case DriveStatus.Mounted:
                        _model.CurrentState = ViewModel.State.Mounted;
                        break;
                    case DriveStatus.Unmounted:
                        _model.CurrentState = ViewModel.State.Authenticated;
                        break;
                }
            }));
        }

        private void _model_StateChanged(object sender, EventArgs eventArgs)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (_model.PreviousState)
                {
                    case ViewModel.State.Unauthenticated:
                        ActionButton.Click -= SignIn;
                        break;
                    case ViewModel.State.Authenticated:
                        ActionButton.Click -= Mount;
                        break;
                    case ViewModel.State.Mounted:
                        ActionButton.Click -= Unmount;
                        break;
                }
                switch (_model.CurrentState)
                {
                    case ViewModel.State.Unauthenticated:
                        ActionButton.IsEnabled = true;
                        ActionButton.Content = "Sign In";
                        ActionButton.Click += SignIn;
                        break;
                    case ViewModel.State.Authenticated:
                        _login.GetAccessToken(); // TODO: wait for access token before changing state
                        ActionButton.IsEnabled = true;
                        ActionButton.Content = "Mount OneDrive";
                        ActionButton.Click += Mount;
                        break;
                    case ViewModel.State.Mounted:
                        ActionButton.IsEnabled = true;
                        ActionButton.Content = "Unmount OneDrive";
                        ActionButton.Click += Unmount;
                        break;
                    default:
                        ActionButton.IsEnabled = false;
                        break;
                }
            }));
        }
    }
}