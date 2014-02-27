using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Edata;
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

            String temporaryEdataPath = GetTemporaryEdataPath(edataFile.Path);
            FileStream sourceEdata = null;
            FileStream newEdata = null;
            try
            {
                sourceEdata = new FileStream(edataFile.Path, FileMode.Open);
                newEdata = new FileStream(temporaryEdataPath, FileMode.Create);

                WriteHeader(newEdata, edataFile, token);

                WriteNotLoadedContent(sourceEdata, newEdata, edataFile, token);

                WriteDictionary(newEdata, edataFile, token);

                //Free file handles before the file move and delete
                CloseEdataFilesStreams(sourceEdata, newEdata);

                //Replace temporary file
                //Assuming that temporary file is placed next to the orginal file, lets just delete orginal one and rename temporary.
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

        /// <summary>
        /// To do: Zastanowić się czy nie powinno poszukać wolnego miejsca na innej partycji w razie braku...
        /// </summary>
        /// <returns></returns>
        protected String GetTemporaryEdataPath(String edataPath)
        {
            var currentEdataFileInfo = new FileInfo(edataPath);
            var temporaryEdataPath = Path.Combine(
                currentEdataFileInfo.DirectoryName,
                Path.GetFileNameWithoutExtension(currentEdataFileInfo.Name) + ".tmp");

            return temporaryEdataPath;
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
