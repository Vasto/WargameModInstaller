using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileUtilities
    {
        public static void CopyFileEx(String source, String destination)
        {
            CopyFileExWrapper.CopyFile(new FileInfo(source), new FileInfo(destination));
        }

        public static void CopyFileEx(String source, String destination, CancellationToken token)
        {
            try
            {
                CopyFileExWrapper.CopyFile(
                    new FileInfo(source),
                    new FileInfo(destination),
                    CopyFileOptions.None,
                    (src, dest, state, fileSize, bytesTransferred) =>
                    {
                        return token.IsCancellationRequested ?
                            CopyFileCallbackAction.Cancel :
                            CopyFileCallbackAction.Continue;
                    });
            }
            catch (Exception ex)
            {
                //Converiosn to the OperationCancelledException;
                token.ThrowIfCancellationRequested();

                throw;
            }
        }

        public static void CopyFileEx(String source, String destination, CopyFileProgressCallback progressCallback)
        {
            CopyFileExWrapper.CopyFile(
                new FileInfo(source),
                new FileInfo(destination),
                CopyFileOptions.None,
                (src, dest, state, fileSize, bytesTransferred) =>
                {
                    progressCallback(src.FullName, dest.FullName, fileSize, bytesTransferred);

                    return CopyFileCallbackAction.Continue;
                });
        }

        public static void CopyFileEx(String source, String destination, CancellationToken token, CopyFileProgressCallback progressCallback)
        {
            try
            {
                CopyFileExWrapper.CopyFile(
                    new FileInfo(source),
                    new FileInfo(destination),
                    CopyFileOptions.None,
                    (src, dest, state, fileSize, bytesTransferred) =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            return CopyFileCallbackAction.Cancel;
                        }
                        else
                        {
                            progressCallback(src.FullName, dest.FullName, fileSize, bytesTransferred);

                            return CopyFileCallbackAction.Continue;
                        }
                    });
            }
            catch (Exception ex)
            {
                //Converiosn to the OperationCancelledException;
                token.ThrowIfCancellationRequested();

                throw;
            }
        }

        public delegate void CopyFileProgressCallback (String source, String destination, long totalFileSize, long totalBytesTransferred);

    }

}
