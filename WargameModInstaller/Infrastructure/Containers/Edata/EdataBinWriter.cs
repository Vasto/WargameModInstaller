using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public class EdataBinWriter : EdataWriterBase, IEdataBinWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFile"></param>
        /// <returns></returns>
        public virtual byte[] Write(EdataFile edataFile)
        {
            return Write(edataFile, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFile"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual byte[] Write(EdataFile edataFile, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            var edataContentFiles = edataFile.ContentFiles.OfType<EdataContentFile>();

            using (MemoryStream edataStream = new MemoryStream())
            {
                if (!AnyContentFileExceedsAvailableSpace(edataFile) &&
                    !edataFile.HasContentFilesCollectionChanged)
                {
                    var header = edataFile.Header;

                    var dictRoot = CreateDictionaryEntries(edataContentFiles);
                    uint dictOffset = header.DictOffset;
                    uint dictLength = ComputeDictionaryLength(dictRoot);
                    uint dictEnd = dictOffset + dictLength;

                    //Clear the old part of file up to content.
                    WritePadding(edataStream, 0, header.FileOffset);

                    WriteLoadedContentByReplace(edataStream, edataContentFiles, token);

                    AssignContentFilesInfoToDictEntries(edataContentFiles, dictRoot);

                    var dictWriteInfo = WriteDictionary(edataStream, dictRoot, dictOffset);

                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictLength = dictWriteInfo.Length;

                    WriteHeader(edataStream, header, 0);
                }
                else
                {
                    //W tym przypadku rozmieszczamy wszystko od zera wg wartości obliczonych.
                    var dictRoot = CreateDictionaryEntries(edataContentFiles);
                    uint dictOffset = GetDictionaryOffset();
                    uint dictLength = ComputeDictionaryLength(dictRoot);
                    uint dictEnd = dictOffset + dictLength;

                    uint fileOffset = GetFileOffset(dictEnd);

                    WritePadding(edataStream, 0, fileOffset);

                    var contentWriteInfo = WriteLoadedContent(edataStream, edataContentFiles, fileOffset, token);

                    AssignContentFilesInfoToDictEntries(edataContentFiles, dictRoot);

                    var dictWriteInfo = WriteDictionary(edataStream, dictRoot, dictOffset);

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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="edataFile"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //protected virtual byte[] WriteContentInternal(EdataFile edataFile, CancellationToken token)
        //{

        //}

    }
}
