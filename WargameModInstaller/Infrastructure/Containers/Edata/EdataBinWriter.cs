using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public class EdataBinWriter : EdataWriterBase, IEdataBinWriter
    {
        public byte[] Write(EdataFile edata)
        {
            return WriteContentInternal(edata);
        }

        public byte[] Write(EdataFile edata, CancellationToken token)
        {
            return WriteContentInternal(edata, token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFile"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual byte[] WriteContentInternal(EdataFile edataFile, CancellationToken? token = null)
        {
            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            var edataContentFiles = edataFile.ContentFiles.OfType<EdataContentFile>();

            using (MemoryStream edataStream = new MemoryStream())
            {
                if (!AnyContentFileExceedsAvailableSpace(edataFile) &&
                    !edataFile.HasContentFilesCollectionChanged)
                {
                    var header = edataFile.Header;

                    var dictEntries = CreateDictionaryEntriesOfContentFiles(edataContentFiles);
                    uint dictOffset = header.DictOffset;
                    uint dictLength = GetDictionaryLength(dictEntries);
                    uint dictEnd = dictOffset + dictLength;

                    //Clear the old part of file up to content.
                    WritePadding(edataStream, 0, header.FileOffset);

                    WriteLoadedContentByReplace(edataStream, edataContentFiles);

                    AssignContentFilesInfoToDictEntries(edataContentFiles, dictEntries);

                    var dictWriteInfo = WriteDictionary(edataStream, dictEntries, dictOffset);

                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictLength = dictWriteInfo.Length;

                    WriteHeader(edataStream, header, 0);
                }
                else
                {
                    //W tym przypadku rozmieszczamy wszystko od zera wg wartości obliczonych.
                    var dictEntries = CreateDictionaryEntriesOfContentFiles(edataContentFiles);
                    uint dictOffset = GetDictionaryOffset();
                    uint dictLength = GetDictionaryLength(dictEntries);
                    uint dictEnd = dictOffset + dictLength;

                    uint fileOffset = GetFileOffset(dictEnd);

                    WritePadding(edataStream, 0, fileOffset);

                    var contentWriteInfo = WriteLoadedContent(edataStream, edataContentFiles, fileOffset);

                    AssignContentFilesInfoToDictEntries(edataContentFiles, dictEntries);

                    var dictWriteInfo = WriteDictionary(edataStream, dictEntries, dictOffset);

                    var header = edataFile.Header;
                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictOffset = dictOffset;
                    header.DictLength = dictWriteInfo.Length;
                    header.FileOffset = fileOffset;
                    header.FileLenght = contentWriteInfo.Length;

                    WriteHeader(edataStream, header, 0);
                }

                return edataStream.ToArray();
            }
        }

    }
}
