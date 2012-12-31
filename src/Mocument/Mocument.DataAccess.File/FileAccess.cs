/*
 * Author: Łukasz Świątkowski
 * http://www.lukesw.net/
 * 
 * This code is provided “AS IS” without warranty of any kind
 * under a Creative Commons Attribution 3.0 Unported License.
 * 
 * http://creativecommons.org/licenses/by/3.0/
 * http://blog.lukesw.net/2009/07/wait-until-file-is-closed.html
 */

using System;
using System.IO;
using System.Threading;

namespace Mocument.DataAccess.File
{
    public static class FileExt
    {
        public static FileStream TryOpenFile(string path, FileAccess access, FileShare share)
        {
            try
            {
                if (!System.IO.File.Exists(path)) return null;
                return System.IO.File.Open(path, FileMode.Open, access, share);
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        public static FileStream WaitAndOpenFile(string path, FileAccess access, FileShare share, TimeSpan timeout)
        {
            DateTime dt = DateTime.UtcNow;
            FileStream fs;
            while ((fs = TryOpenFile(path, access, share)) == null && (DateTime.UtcNow - dt) < timeout)
            {
                Thread.Sleep(250); // who knows better way and wants a free cookie? ;)
            }
            return fs;
        }

        #region " Other Methods"

        public static FileStream TryOpenFileForReading(this FileInfo fileInfo)
        {
            return TryOpenFileForReading(fileInfo.FullName);
        }

        public static FileStream TryOpenFileForReading(string path)
        {
            return TryOpenFile(path, FileAccess.Read);
        }

        public static FileStream TryOpenFileForWriting(this FileInfo fileInfo)
        {
            return TryOpenFileForWriting(fileInfo.FullName);
        }

        public static FileStream TryOpenFileForWriting(string path)
        {
            return TryOpenFile(path, FileAccess.ReadWrite);
        }

        public static FileStream TryOpenFile(this FileInfo fileInfo, FileAccess access)
        {
            return TryOpenFile(fileInfo.FullName, access);
        }

        public static FileStream TryOpenFile(string path, FileAccess access)
        {
            return TryOpenFile(path, access, FileShare.None);
        }

        public static FileStream TryOpenFile(this FileInfo fileInfo, FileAccess access, FileShare share)
        {
            return TryOpenFile(fileInfo.FullName, access, share);
        }

        public static FileStream WaitAndOpenFileForReading(this FileInfo fileInfo, TimeSpan timeout)
        {
            return WaitAndOpenFileForReading(fileInfo.FullName, timeout);
        }

        public static FileStream WaitAndOpenFileForReading(string path, TimeSpan timeout)
        {
            return WaitAndOpenFile(path, FileAccess.Read, timeout);
        }

        public static FileStream WaitAndOpenFileForWriting(this FileInfo fileInfo, TimeSpan timeout)
        {
            return WaitAndOpenFileForWriting(fileInfo.FullName, timeout);
        }

        public static FileStream WaitAndOpenFileForWriting(string path, TimeSpan timeout)
        {
            return WaitAndOpenFile(path, FileAccess.ReadWrite, timeout);
        }

        public static FileStream WaitAndOpenFile(this FileInfo fileInfo, FileAccess access, TimeSpan timeout)
        {
            return WaitAndOpenFile(fileInfo.FullName, access, timeout);
        }

        public static FileStream WaitAndOpenFile(string path, FileAccess access, TimeSpan timeout)
        {
            return WaitAndOpenFile(path, access, FileShare.None, timeout);
        }

        public static FileStream WaitAndOpenFile(this FileInfo fileInfo, FileAccess access, FileShare share,
                                                 TimeSpan timeout)
        {
            return WaitAndOpenFile(fileInfo.FullName, access, share, timeout);
        }

        #endregion
    }
}