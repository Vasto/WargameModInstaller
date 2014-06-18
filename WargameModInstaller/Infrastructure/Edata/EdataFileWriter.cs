using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
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

        protected void WriteContentInternal(EdataFile edataFile, CancellationToken? token = null)
        {
            //Uważać tu na ścieżke jaka ma edata pełną czy relatywną..
            //No i dodatkowo od tego momentu może być pusta. A z racji tego że tylko możemy podmieniać edata nie pasuje
            //dodawać dodatkowy argument ścieżki do zapisu, bo to jasno wskazywało by że możemy zapisywać do dowolnej lokacji
            // a tak naprawde można tylko podmieniać edata.
            if (!File.Exists(edataFile.Path))
            {
                throw new ArgumentException(
                    String.Format("A following Edata file: \"{0}\" doesn't exist", edataFile.Path),
                    "edataFile");
            }

            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            //Wypieprzyć z tąd wszystkie zależności od starych metod, i stworzyć nowe metody zapisu poszczegołnych elementów
            //realizujące nową koncepcję wykorzystujaca model wpsiów słownika i być może w przyszłosci samego słownika.

            //W przypadku konieczności odbudowy słownika trzeba poszerzyć określenie czy można użyć ReplacemnetWrite
            //Jeśli nie ma nowego pliku, i słownik mieści sie w miejsce starego, to można użyć replacement write
            //Jeśli natomiast dodano nowy plik, lub słownik nie miesci sie na miejsce starego to trzeba zbudować plik Edata od zera.

            //Update na dobrą sprawę można założyć, że jeśli nie ma zmian w plikach to słownik zawszę będzie się mieścił w miejsce starego.
            if (!AnyContentFileExceedsAvailableSpace(edataFile) &&
                !edataFile.HasContentFilesCollectionChanged)
            {
                using (var sourceEdata = new FileStream(edataFile.Path, FileMode.Open))
                {
                    //Taka uwaga: Nie robić canceli w trakcie zapisu czy to nagłowka czy słownika, zeby w przypadku przerwania bez backapu zminimalizwoać
                    //            szanse na uszkodzenie modyifkowane go pliku.

                    //W tym przypadku używamy starego rozmieszczenia danych żeby nie odbudowywac pliku od nowa.
                    var header = edataFile.Header;

                    var dictEntries = CreateDictionaryEntriesOfContentFiles(edataFile.ContentFiles);
                    uint dictOffset = header.DictOffset;
                    uint dictLength = GetDictionaryLength(dictEntries);
                    uint dictEnd = dictOffset + dictLength;

                    //Clear the old part of file up to content.
                    WritePadding(sourceEdata, 0, header.FileOffset);

                    WriteLoadedContentByReplace(sourceEdata, edataFile.ContentFiles);

                    AssignContentFilesInfoToDictEntries(edataFile.ContentFiles, dictEntries);

                    var dictWriteInfo = WriteDictionary(sourceEdata, dictEntries, dictOffset);

                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictLength = dictWriteInfo.Length;

                    WriteHeader(sourceEdata, header, 0);
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
                        throw new IOException(
                            String.Format("Not enough free disk space for rebuilding the \"{0}\" file.",
                            edataFile.Path));
                    }
                }

                //To avoid too many nested try catches.
                FileStream sourceEdata = null;
                FileStream newEdata = null;
                try
                {
                    sourceEdata = new FileStream(edataFile.Path, FileMode.Open);
                    newEdata = new FileStream(temporaryEdataPath, FileMode.Create);

                    //W tym przypadku rozmieszczamy wszystko od zera wg wartości obliczonych.
                    var dictEntries = CreateDictionaryEntriesOfContentFiles(edataFile.ContentFiles);
                    uint dictOffset = GetDictionaryOffset();
                    uint dictLength = GetDictionaryLength(dictEntries);
                    uint dictEnd = dictOffset + dictLength;

                    uint fileOffset = GetFileOffset(dictEnd);

                    WritePadding(newEdata, 0, fileOffset);

                    var contentWriteInfo = WriteNotLoadedContent(sourceEdata, newEdata, edataFile.ContentFiles, fileOffset);

                    AssignContentFilesInfoToDictEntries(edataFile.ContentFiles, dictEntries);

                    var dictWriteInfo = WriteDictionary(newEdata, dictEntries, dictOffset);

                    var header = edataFile.Header;
                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictOffset = dictOffset;
                    header.DictLength = dictWriteInfo.Length;
                    header.FileOffset = fileOffset;
                    header.FileLenght = contentWriteInfo.Length;

                    WriteHeader(newEdata, header, 0);

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

        //Zmienione z private na protected na razie
        protected void CloseEdataFilesStreams(FileStream sourceEdata, FileStream newEdata)
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
