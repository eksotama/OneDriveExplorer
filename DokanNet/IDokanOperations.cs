using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;

namespace DokanNet
{
    public interface IDokanOperations
    {
        NtStatusCodes CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode,
                              FileOptions options, FileAttributes attributes, DokanFileInfo info);

        NtStatusCodes OpenDirectory(string fileName, DokanFileInfo info);

        NtStatusCodes CreateDirectory(string fileName, DokanFileInfo info);

        NtStatusCodes Cleanup(string fileName, DokanFileInfo info);

        NtStatusCodes CloseFile(string fileName, DokanFileInfo info);

        NtStatusCodes ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset,
                            DokanFileInfo info);

        NtStatusCodes WriteFile(string fileName, byte[] buffer, out int bytesWritten,
                             long offset, DokanFileInfo info);

        NtStatusCodes FlushFileBuffers(string fileName, DokanFileInfo info);

        NtStatusCodes GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info);

        NtStatusCodes FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info);

        NtStatusCodes SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info);

        NtStatusCodes SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime,
                               DateTime? lastWriteTime, DokanFileInfo info);

        NtStatusCodes DeleteFile(string fileName, DokanFileInfo info);

        NtStatusCodes DeleteDirectory(string fileName, DokanFileInfo info);

        NtStatusCodes MoveFile(string oldName, string newName, bool replace, DokanFileInfo info);

        NtStatusCodes SetEndOfFile(string fileName, long length, DokanFileInfo info);

        NtStatusCodes SetAllocationSize(string fileName, long length, DokanFileInfo info);

        NtStatusCodes LockFile(string fileName, long offset, long length, DokanFileInfo info);

        NtStatusCodes UnlockFile(string fileName, long offset, long length, DokanFileInfo info);

        NtStatusCodes GetDiskFreeSpace(out long free, out long total, out long used,
                                    DokanFileInfo info);

        NtStatusCodes GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
                                        out string fileSystemName, DokanFileInfo info);

        NtStatusCodes GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        NtStatusCodes SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
                                   DokanFileInfo info);

        NtStatusCodes Unmount(DokanFileInfo info);
    }
}