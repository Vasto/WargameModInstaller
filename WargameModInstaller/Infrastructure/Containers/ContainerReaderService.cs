using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Containers.Edata;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers
{
    /// <summary>
    /// 
    /// </summary>
    public class ContainerReaderService : IContainerReaderService
    {
        protected readonly HashSet<ContentFileType> knownContainers;
        protected readonly Dictionary<ContentFileType, ReadFileFunc> readFileFuncsMap;
        protected readonly Dictionary<ContentFileType, ReadRawFunc> readRawFuncsMap;
        protected readonly Dictionary<ContentFileType, LoadFunc> loadFuncsMap;

        protected delegate IContainerFile ReadFileFunc(String path, bool loadContent, CancellationToken token);
        protected delegate IContainerFile ReadRawFunc(byte[] raw, bool loadContent, CancellationToken token);
        protected delegate void LoadFunc(IContentFile file, CancellationToken token);

        public ContainerReaderService()
        {
            knownContainers = CreateKnownContainers();
            readFileFuncsMap = CreateReadFileFuncsMap();
            readRawFuncsMap = CreateReadRawFuncsMap();
            loadFuncsMap = CreateLoadFuncsMap();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFilePath"></param>
        /// <param name="loadContent"></param>
        /// <returns></returns>
        public IContainerFile ReadFile(String containerFilePath, bool loadContent)
        {
            return ReadFile(containerFilePath, loadContent, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFilePath"></param>
        /// <param name="loadContent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IContainerFile ReadFile(String containerFilePath, bool loadContent, CancellationToken token)
        {
            if (!File.Exists(containerFilePath))
            {
                throw new ArgumentException("A file with the specified path doesn't exist.", "containerFilePath");
            }

            var longestMagic = knownContainers.Max(x => x.MagicBytes.Length);
            byte[] magicBuffer = new byte[longestMagic];

            using (Stream source = File.OpenRead(containerFilePath))
            {
                source.Read(magicBuffer, 0, magicBuffer.Length);
            }

            var containerType = ResolveFileType(magicBuffer);

            var readFunc = readFileFuncsMap[containerType];

            var containerFile = readFunc(containerFilePath, loadContent, token);

            return containerFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawContainerFile"></param>
        /// <param name="loadContent"></param>
        /// <returns></returns>
        public IContainerFile ReadRaw(byte[] rawContainerFile, bool loadContent)
        {
            return ReadRaw(rawContainerFile, loadContent, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawContainerFile"></param>
        /// <param name="loadContent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public IContainerFile ReadRaw(byte[] rawContainerFile, bool loadContent, CancellationToken token)
        {
            if (rawContainerFile == null)
            {
                throw new ArgumentNullException("rawContainerFile");
            }

            var longestMagic = knownContainers.Max(x => x.MagicBytes.Length);
            byte[] magicBuffer = new byte[longestMagic];

            using (Stream source = new MemoryStream(rawContainerFile))
            {
                source.Read(magicBuffer, 0, magicBuffer.Length);
            }

            var containerType = ResolveFileType(magicBuffer); //Może zwrócić unknown, obsłużyć

            var readFunc = readRawFuncsMap[containerType];

            var containerFile = readFunc(rawContainerFile, loadContent, token);

            return containerFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(IContentFile file)
        {
            LoadContent(new[] { file });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        public void LoadContent(IEnumerable<IContentFile> files)
        {
            foreach (var file in files)
            {
                if (file.Owner == null)
                {
                    throw new ArgumentException(
                        String.Format("'{0}' is not assigned to any Edata File Owner.", file), "file");
                }

                var containerType = ResolveFileType(file); //Może zwrócić unknown, obsłużyć

                var loadFunc = loadFuncsMap[containerType];

                loadFunc(file, CancellationToken.None);
            }
        }

        protected virtual HashSet<ContentFileType> CreateKnownContainers()
        {
            var result = new HashSet<ContentFileType>();
            result.Add(ContentFileType.Edata);
            //result.Add(ContentFileType2.Prxypcpc);
            //result.Add(ContentFileType2.Mesh);

            return result;
        }

        protected virtual Dictionary<ContentFileType, ReadFileFunc> CreateReadFileFuncsMap()
        {
            var map = new Dictionary<ContentFileType, ReadFileFunc>();
            map.Add(ContentFileType.Edata, (path, loadContent, token) =>
            {
                return (new EdataFileReader()).Read(path, loadContent, token);
            });

            return map;
        }

        protected virtual Dictionary<ContentFileType, ReadRawFunc> CreateReadRawFuncsMap()
        {
            var map = new Dictionary<ContentFileType, ReadRawFunc>();
            map.Add(ContentFileType.Edata, (data, loadContent, token) =>
            {
                return (new EdataBinReader()).Read(data, loadContent, token);
            });

            return map;
        }

        protected virtual Dictionary<ContentFileType, LoadFunc> CreateLoadFuncsMap()
        {
            var map = new Dictionary<ContentFileType, LoadFunc>();
            map.Add(ContentFileType.Edata, (contentFile, token) =>
            {
                (new EdataFileReader()).LoadContent((EdataContentFile)contentFile);
            });


            return map;
        }

        protected ContentFileType ResolveFileType(byte[] binData)
        {
            foreach (var containerType in knownContainers)
            {
                if(MiscUtilities.ComparerByteArrays(
                    containerType.MagicBytes, 
                    binData.Take(containerType.MagicBytes.Length).ToArray()))
                {
                    return containerType;
                }
            }

            return ContentFileType.Unknown;
        }

        protected ContentFileType ResolveFileType(IContentFile file)
        {
            //return file.FileType;
            //Until switched to ContentFileType2:

            return ContentFileType.Edata;
        }


    }
}
