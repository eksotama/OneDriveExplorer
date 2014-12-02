using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using DokanNet;
using FileAccess = DokanNet.FileAccess;

namespace OneDriveExplorer
{
    internal class DokanOneDrive : IDokanOperations
    {
        public DokanOneDrive(char driveLetter, string volumeLabel)
        {
            DriveLetter = driveLetter;
            VolumeLabel = volumeLabel;
        }

        public char DriveLetter { get; private set; }
        public string VolumeLabel { get; private set; }

        public NtStatusCodes Cleanup(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes CloseFile(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes CreateDirectory(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
            FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            if (fileName.EndsWith("desktop.ini", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith("autorun.inf", StringComparison.OrdinalIgnoreCase)) //....
            {
                return NtStatusCodes.ObjectNameNotFound;
            }


            switch (mode)
            {
                case FileMode.Open:
                    if (fileName != "\\")
                    {
                        return NtStatusCodes.ObjectNameNotFound;
                    }
                    break;
            }
            return NtStatusCodes.Success;
        }

        public NtStatusCodes DeleteDirectory(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes DeleteFile(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            files = new FileInformation[0];
            return NtStatusCodes.Success;
        }

        public NtStatusCodes FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes GetDiskFreeSpace(out long free, out long total, out long used, DokanFileInfo info)
        {
            total = 10L*1024*1024*1024;
            free = 9L*1024*1024*1024;
            used = total - free;
            return NtStatusCodes.Success;
        }

        public NtStatusCodes GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            var now = DateTime.Now;
            if (fileName != "\\")
            {
                fileInfo = new FileInformation();
                fileInfo.CreationTime = fileInfo.LastAccessTime = fileInfo.LastWriteTime = now;
                return NtStatusCodes.ObjectNameNotFound;
            }
            fileInfo = new FileInformation
            {
                Attributes =
                    FileAttributes.NotContentIndexed | FileAttributes.Directory,
                FileName = String.Empty,
                // GetInfo info doesn't use it maybe for sorting .
                CreationTime = now,
                LastAccessTime = now,
                LastWriteTime = now,
                Length = 0 // Windows directories use length of 0 
            };
            return NtStatusCodes.Success;
        }

        public NtStatusCodes GetFileSecurity(string fileName, out FileSystemSecurity security,
            AccessControlSections sections, DokanFileInfo info)
        {
            const FileSystemRights rights = FileSystemRights.ReadPermissions | FileSystemRights.ReadExtendedAttributes
                                            | FileSystemRights.ReadAttributes | FileSystemRights.Synchronize 
                                            | FileSystemRights.ReadData
                                            | FileSystemRights.Write | FileSystemRights.Traverse;
            security = info.IsDirectory ? new DirectorySecurity() as FileSystemSecurity : new FileSecurity();
            security.AddAccessRule(new FileSystemAccessRule("Everyone", rights, AccessControlType.Allow));
            security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl ^ rights,
                AccessControlType.Deny));
            security.SetOwner(new NTAccount("None"));
            security.SetGroup(new NTAccount("None"));

            return NtStatusCodes.Success;
        }

        public NtStatusCodes GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = VolumeLabel;
            fileSystemName = "NTFS";

            features = FileSystemFeatures.CasePreservedNames | FileSystemFeatures.CaseSensitiveSearch |
                       FileSystemFeatures.SupportsRemoteStorage | FileSystemFeatures.UnicodeOnDisk;
            return NtStatusCodes.Success;
        }

        public NtStatusCodes LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes OpenDirectory(string fileName, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            bytesRead = 0;
            return NtStatusCodes.Success;
        }

        public NtStatusCodes SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes SetFileSecurity(string fileName, FileSystemSecurity security,
            AccessControlSections sections, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
            DateTime? lastWriteTime, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return NtStatusCodes.Success;
        }

        public NtStatusCodes Unmount(DokanFileInfo info)
        {
            return Dokan.Unmount(DriveLetter) ? NtStatusCodes.VolumeMounted : NtStatusCodes.WrongVolume;
        }

        public NtStatusCodes WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset,
            DokanFileInfo info)
        {
            bytesWritten = 0;
            return NtStatusCodes.Success;
        }
    }
}