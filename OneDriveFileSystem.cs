using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DokanNet;
using Microsoft.Win32;

namespace OneDriveExplorer
{
    [Serializable]
    public class OneDriveFileSystem : IDisposable, ISerializable
    {
        private readonly CancellationTokenSource _mountCancel = new CancellationTokenSource();
        private readonly AutoResetEvent _pauseEvent = new AutoResetEvent(false);
        private readonly CancellationTokenSource _threadCancel = new CancellationTokenSource();
        private bool _exeptionThrown;
        private DokanOneDrive _filesystem;

        private Exception _lastExeption;
        private Thread _mountThread;

        public OneDriveFileSystem(char letter = 'O', string name = "OneDrive Explorer")
        {
            Letter = letter;
            Name = name;
        }

        public OneDriveFileSystem(SerializationInfo info,
            StreamingContext context)
        {
            Name = info.GetString("name");
            Letter = info.GetChar("drive");
        }

        public event EventHandler<EventArgs> StatusChanged;

        public char Letter { get; set; }
        public string Name { get; set; }

        public DriveStatus Status { get; private set; }

        public void Dispose()
        {
            Debug.WriteLine("Dispose");


            if (_threadCancel != null) _threadCancel.Cancel();
            if (_pauseEvent != null) _pauseEvent.Set();
            try
            {
                Dokan.RemoveMountPoint(String.Format("{0}:\\", Letter));
                if (_filesystem != null)
                {
                    //filesystem.Dispose();


                    _filesystem = null;
                }
            }
            catch
            {
                Status = DriveStatus.Unmounted;
            }
            finally
            {
                _filesystem = null;
            }


            if (_mountCancel != null)
            {
                _mountCancel.Dispose();
            }
            if (_threadCancel != null)
            {
                _threadCancel.Dispose();
            }
            if (_pauseEvent != null)
            {
                _pauseEvent.Dispose();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", Name);
            info.AddValue("drive", Letter);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Mount()
        {
            Debug.WriteLine("Mount");


            if (Directory.GetLogicalDrives().Any(drive => drive[0] == Letter))
            {
                throw new Exception("Drive with the same letter exists");
            }


            Status = DriveStatus.Mounting;

            try
            {
                SetupFilesystem();
            }
            catch
            {
                Status = DriveStatus.Unmounted;
                throw;
            }

            SetupMountThread();


            var mountEvent = Task.Factory.StartNew(() =>
            {
                while (!_mountCancel.IsCancellationRequested &&
                       Directory.GetLogicalDrives().All(
                           drive => drive[0] != Letter))
                {
                    Thread.Sleep(1000);
                }
            }, _mountCancel.Token);


            _pauseEvent.Set();

            mountEvent.Wait();

            if (_exeptionThrown)
            {
                _exeptionThrown = false;

                throw _lastExeption;
            }
            //SetNetworkDriveName(Connection, Name);
            Status = DriveStatus.Mounted;
            OnStatusChanged(EventArgs.Empty);
        }

        public static void SetNetworkDriveName(string connection, string name)
        {
            var mountPoints = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MountPoints2", false);
            if (mountPoints == null) return;
            var drive = mountPoints.OpenSubKey(connection.Replace("\\", "#"), true);
            if (drive == null) return;
            drive.SetValue("_LabelFromReg", name);
        }

        public override string ToString()
        {
            return String.Format("{0}[{1}:]", Name, Letter);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Unmount()
        {
            Debug.WriteLine("Unmount");

            Status = DriveStatus.Unmounting;
            try
            {
                // Dokan.Unmount(Letter);
                Dokan.RemoveMountPoint(String.Format("{0}:\\", Letter));
                if (_filesystem != null)
                {
                    //_filesystem.Dispose();
                }
            }
            catch
            {
                Status = DriveStatus.Unmounted;
                OnStatusChanged(EventArgs.Empty);
            }
            finally
            {
                _filesystem = null;
            }
        }

        private void MountLoop()
        {
            while (true)
            {
                Debug.WriteLine("Thread:Pause");

                _pauseEvent.WaitOne(-1);
                if (_threadCancel.IsCancellationRequested)
                {
                    Debug.WriteLine("Thread:Cancel");
                    break;
                }

                Debug.WriteLine("Thread:Mount");


                try
                {
                    _filesystem.Mount(String.Format("{0}:\\", Letter),
                        DokanOptions.RemovableDrive | DokanOptions.KeepAlive);
                }
                catch (Exception e)
                {
                    _lastExeption = e;
                    _exeptionThrown = true;
                    _mountCancel.Cancel();
                }
                Status = DriveStatus.Unmounted;
                if (!_exeptionThrown)
                {
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }


        private void OnStatusChanged(EventArgs args)
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, args);
            }
        }


        private void SetupFilesystem()
        {
            Debug.WriteLine("SetupFilesystem");

            _filesystem = new DokanOneDrive(Letter, Name);
            //Debug.WriteLine("Connecting...");
            //_filesystem.Connect();
        }

        private void SetupMountThread()
        {
            if (_mountThread == null)
            {
                Debug.WriteLine("Thread:Created");
                _mountThread = new Thread(MountLoop) {IsBackground = true};
                _mountThread.Start();
            }
        }
    }
}