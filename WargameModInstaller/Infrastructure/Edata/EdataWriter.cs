using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Utilities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Infrastructure.Edata
{
    public class EdataWriter : IEdataWriter
    {
        public EdataWriter() 
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public void Write(EdataFile fileToWrite)
        {
            WriteContentInternal(fileToWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Write(EdataFile fileToWrite, CancellationToken token)
        {
            WriteContentInternal(fileToWrite, token);
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected void WriteContentInternal(EdataFile edataFile, CancellationToken? token = null)
        {
            if (!File.Exists(edataFile.Path)) //Uważać tu na ścieżke jaka ma edata pełną czy relatywną..
            {
                throw new ArgumentException(
                    String.Format("Edata file with the following path: '{0}' doesn't exist", edataFile.Path),
                    "edataFile");
            }

            //To do: Przerobić to tak by przyjmowało EdataFile jako parametr i zamiast używania tej mapy, na podstawie
            //załadowanych EdataContentFile, podmieniało odpowiednie dane. Tylko jeśli ich content byłby załadowany.
            //W przeciwnym razie, tak jak teraz były by odczytywane z pliku.

            String temporaryEdataPath = GetTemporaryEdataPath(edataFile.Path);

            var reserveBuffer = new byte[200];

            FileStream sourceEdata = null;
            FileStream newEdata = null;
            try
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                sourceEdata = new FileStream(edataFile.Path, FileMode.Open);
                newEdata = new FileStream(temporaryEdataPath, FileMode.Create);

                var sourceEdataHeader = edataFile.Header;
                var headerPart = new byte[sourceEdataHeader.FileOffset];
                sourceEdata.Read(headerPart, 0, headerPart.Length);
                newEdata.Write(headerPart, 0, headerPart.Length);

                sourceEdata.Seek(sourceEdataHeader.FileOffset, SeekOrigin.Begin);

                uint filesContentLength = 0;

                foreach (EdataContentFile file in edataFile.ContentFiles)
                {
                    //Cancel if requested;
                    token.ThrowIfCanceledAndNotNull();


                    long oldOffset = file.Offset;
                    file.Offset = newEdata.Position - sourceEdataHeader.FileOffset;

                    byte[] fileBuffer;

                    if (file.IsContentLoaded)
                    {
                        fileBuffer = file.Content;
                        file.Size = file.Content.Length; // To przenieść do klasy tak aby było ustawiane przy zmianie contnetu
                    }
                    else
                    {
                        fileBuffer = new byte[file.Size];
                        sourceEdata.Seek(oldOffset + sourceEdataHeader.FileOffset, SeekOrigin.Begin);
                        sourceEdata.Read(fileBuffer, 0, fileBuffer.Length);
                    }

                    file.Checksum = MD5.Create().ComputeHash(fileBuffer);

                    //Zastanowić się nad tym, czy by nie uzupełniać róznicy miedzy starym a nowym plikiem pustymi danymi, 
                    //tak jak robi to reserveBuffer, tylko o odpowiedniej wielkości - miało by to być w celu sprawdzenia czy rzekomy
                    //tak naprawde nie wiadomo czym do końca spowodowany problem nie ładowania modeli w amory przestałby istnieć.
                    newEdata.Write(fileBuffer, 0, fileBuffer.Length);
                    newEdata.Write(reserveBuffer, 0, reserveBuffer.Length);

                    filesContentLength += (uint)fileBuffer.Length + (uint)reserveBuffer.Length;
                }

                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                newEdata.Seek(0x25, SeekOrigin.Begin);
                newEdata.Write(BitConverter.GetBytes(filesContentLength), 0, 4);


                newEdata.Seek(sourceEdataHeader.DirOffset, SeekOrigin.Begin);
                long dirEnd = sourceEdataHeader.DirOffset + sourceEdataHeader.DirLengh;
                uint id = 0;

                //Odtworzenie słownika?
                while (newEdata.Position < dirEnd)
                {
                    //Cancel if requested;
                    token.ThrowIfCanceledAndNotNull();


                    var buffer = new byte[4];
                    newEdata.Read(buffer, 0, 4);
                    int fileGroupId = BitConverter.ToInt32(buffer, 0);

                    if (fileGroupId == 0)
                    {
                        EdataContentFile curFile = edataFile.ContentFiles
                            .Single(x => x.Id == id);

                        // FileEntrySize
                        newEdata.Seek(4, SeekOrigin.Current);

                        buffer = BitConverter.GetBytes(curFile.Offset);
                        newEdata.Write(buffer, 0, buffer.Length);

                        buffer = BitConverter.GetBytes(curFile.Size);
                        newEdata.Write(buffer, 0, buffer.Length);

                        byte[] checkSum = curFile.Checksum;
                        newEdata.Write(checkSum, 0, checkSum.Length);

                        string name = MiscUtilities.ReadString(newEdata);

                        if ((name.Length + 1) % 2 == 1)
                        {
                            newEdata.Seek(1, SeekOrigin.Current);
                        }

                        id++;
                    }
                    else if (fileGroupId > 0)
                    {
                        newEdata.Seek(4, SeekOrigin.Current);
                        string name = MiscUtilities.ReadString(newEdata);

                        if ((name.Length + 1) % 2 == 1)
                        {
                            newEdata.Seek(1, SeekOrigin.Current);
                        }
                    }
                }

                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                newEdata.Seek(sourceEdataHeader.DirOffset, SeekOrigin.Begin);
                var dirBuffer = new byte[sourceEdataHeader.DirLengh];
                newEdata.Read(dirBuffer, 0, dirBuffer.Length);

                byte[] dirCheckSum = MD5.Create().ComputeHash(dirBuffer);

                newEdata.Seek(0x31, SeekOrigin.Begin);
                newEdata.Write(dirCheckSum, 0, dirCheckSum.Length);

                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                //Free file handles before the file move and delete
                CloseEdataFilesStreams(sourceEdata, newEdata);

                //Replace temporary file
                //Assuming that temporary file is placed next to the orginal file, lets just delete orginal one and rename temporary.
                File.Delete(edataFile.Path);
                File.Move(temporaryEdataPath, edataFile.Path);
            }
            catch (Exception ex)
            {
                throw;
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

        protected void WriteContentFastInternal(IDictionary<String, byte[]> pathToContentMap, CancellationToken? token = null)
        {
            //Odczytywać kilka plików za jednym zamachem az odczyt osiągnie ustalony rozmiar.

            throw new NotImplementedException();
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
