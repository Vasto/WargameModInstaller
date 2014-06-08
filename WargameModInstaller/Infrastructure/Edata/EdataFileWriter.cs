using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public class EdataFileWriter : EdataWriterBase, IEdataFileWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileToWrite"></param>
        public void Write(EdataFile fileToWrite)
        {
            WriteContentInternal(fileToWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileToWrite"></param>
        /// <param name="token"></param>
        public void Write(EdataFile fileToWrite, CancellationToken token)
        {
            WriteContentInternal(fileToWrite, token);
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void WriteContentInternal(EdataFile edataFile, CancellationToken? token = null)
        {
            //Uważać tu na ścieżke jaka ma edata pełną czy relatywną..
            //No i dodatkowo od tego momentu może być pusta. A z racji tego że tylko możemy podmieniać edata nie pasuje
            //dodawać dodatkowy argument ścieżki do zapisu, bo to jasno wskazywało by że możemy zapisywać do dowolnej lokacji
            // a tak naprawde można tylko podmieniać edata.
            if (!File.Exists(edataFile.Path)) 
            {
                throw new ArgumentException(
                    String.Format("Edata file with the following path: '{0}' doesn't exist", edataFile.Path),
                    "edataFile");
            }

            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            if (CanUseReplacementWrite(edataFile))
            {
                using(var sourceEdata = new FileStream(edataFile.Path, FileMode.Open))
                {
                    WriteHeader(sourceEdata, edataFile, token);

                    ReplaceLoadedContent(sourceEdata, edataFile, token);

                    WriteDictionary(sourceEdata, edataFile, token);

                    //-------------------------
                    CreateDictionaryEntriesForContentFiles(edataFile.ContentFiles);
                }
            }
            else
            {
                //Try in the current dir to avoid double file moving
                String temporaryEdataPath = GetTemporaryEdataPathInCurrentLocation(edataFile.Path);
                if ((new FileInfo(edataFile.Path).Length > (new DriveInfo(temporaryEdataPath).AvailableFreeSpace)))
                {
                    temporaryEdataPath = TryGetTemporaryEdataPathWhereFree(edataFile.Path);
                    if (temporaryEdataPath == null)
                    {
                        throw new IOException("Not enough disk space for rebuilding edata.");
                    }
                }

                //To avoid nested try catches.
                FileStream sourceEdata = null;
                FileStream newEdata = null;
                try
                {
                    sourceEdata = new FileStream(edataFile.Path, FileMode.Open);
                    newEdata = new FileStream(temporaryEdataPath, FileMode.Create);

                    WriteHeader(newEdata, edataFile, token);

                    WriteNotLoadedContent(sourceEdata, newEdata, edataFile, token);

                    WriteDictionary(newEdata, edataFile, token);

                    //-------------------------
                    CreateDictionaryEntriesForContentFiles(edataFile.ContentFiles);

                    //Free file handles before the file move and delete
                    CloseEdataFilesStreams(sourceEdata, newEdata);

                    //Replace temporary file
                    File.Delete(edataFile.Path);
                    File.Move(temporaryEdataPath, edataFile.Path);
                }
                finally
                {
                    //Spr czy zostały już zwolnione...?
                    CloseEdataFilesStreams(sourceEdata, newEdata);

                    if (File.Exists(temporaryEdataPath))
                    {
                        File.Delete(temporaryEdataPath);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected String GetTemporaryEdataPathInCurrentLocation(String oldeEdataPath)
        {
            var oldEdataFileInfo = new FileInfo(oldeEdataPath);
            var temporaryEdataPath = Path.Combine(
                oldEdataFileInfo.DirectoryName,
                Path.GetFileNameWithoutExtension(oldEdataFileInfo.Name) + ".tmp");

            return temporaryEdataPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldEdataPath"></param>
        /// <returns></returns>
        protected String TryGetTemporaryEdataPathWhereFree(String oldEdataPath)
        {
            var oldEdataFileInfo = new FileInfo(oldEdataPath);

            var fixedDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);
            foreach (var drive in fixedDrives)
            {
                if (drive.AvailableFreeSpace > oldEdataFileInfo.Length)
                {
                    var tempFileName = Path.GetFileNameWithoutExtension(oldEdataFileInfo.Name) + ".tmp";
                    var temporaryEdataPath = Path.Combine(drive.Name, tempFileName);
                    //Create path if not exist
                    //PathUtilities.CreateDirectoryIfNotExist(temporaryEdataPath);

                    return temporaryEdataPath;
                }
            }

            return null;
        }

        private void CloseEdataFilesStreams(FileStream sourceEdata, FileStream newEdata)
        {
            if (newEdata != null)
            {
                newEdata.Close();
            }

            if (sourceEdata != null)
            {
                sourceEdata.Close();
            }
        }

    }

}
