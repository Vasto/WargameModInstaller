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
        protected delegate IContainerFile ReadFileFunc(String path, bool loadContent, CancellationToken token);
        protected delegate IContainerFile ReadRawFunc(byte[] raw, bool loadContent, CancellationToken token);
        //protected delegate void LoadFileFunc(IContentFile file, CancellationToken token);
        protected delegate void LoadFilesFunc(IEnumerable<IContentFile> files, CancellationToken token);

        public ContainerReaderService()
        {
            this.KnownContainerTypes = CreateKnownContainerTypes();
            this.ContainerFileTypes = CreateContainerFileTypesMap();
            this.ReadFileFuncs = CreateReadFileFuncsMap();
            this.ReadRawFuncs = CreateReadRawFuncsMap();
            this.LoadFilesFuncs = CreateLoadFuncsMap();
        }

        protected HashSet<ContentFileType> KnownContainerTypes
        {
            get;
            private set;
        }

        protected Dictionary<Type, ContentFileType> ContainerFileTypes
        {
            get;
            private set;
        }

        protected Dictionary<ContentFileType, ReadFileFunc> ReadFileFuncs
        {
            get;
            private set;
        }

        protected Dictionary<ContentFileType, ReadRawFunc> ReadRawFuncs
        {
            get;
            private set;
        }

        protected Dictionary<ContentFileType, LoadFilesFunc> LoadFilesFuncs
        {
            get;
            private set;
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

            var longestMagic = KnownContainerTypes.Max(x => x.MagicBytes.Length);
            byte[] magicBuffer = new byte[longestMagic];

            using (Stream source = File.OpenRead(containerFilePath))
            {
                source.Read(magicBuffer, 0, magicBuffer.Length);
            }

            var containerType = ResolveFileType(magicBuffer);

            var readFunc = ReadFileFuncs[containerType];

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

            var longestMagic = KnownContainerTypes.Max(x => x.MagicBytes.Length);
            byte[] magicBuffer = new byte[longestMagic];

            using (Stream source = new MemoryStream(rawContainerFile))
            {
                source.Read(magicBuffer, 0, magicBuffer.Length);
            }

            var containerType = ResolveFileType(magicBuffer); //Może zwrócić unknown, obsłużyć

            var readFunc = ReadRawFuncs[containerType];

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
            var filesGroupedByOwner = files.GroupBy(x => x.Owner);

            foreach (var fileGroup in filesGroupedByOwner)
            {
                var owner = fileGroup.Key;
                if (owner == null)
                {
                    throw new ArgumentException("One of content files is not assigned to any container file.");
                }

                var containerType = ResolveFileType(owner);

                var loadFunc = LoadFilesFuncs[containerType];

                loadFunc(fileGroup, CancellationToken.None);
            }
        }

        protected virtual HashSet<ContentFileType> CreateKnownContainerTypes()
        {
            var result = new HashSet<ContentFileType>();
            result.Add(ContentFileType.Edata);
            //result.Add(ContentFileType2.Prxypcpc);
            //result.Add(ContentFileType2.Mesh);

            return result;
        }

        protected virtual Dictionary<Type, ContentFileType> CreateContainerFileTypesMap()
        {
            var map = new Dictionary<Type, ContentFileType>();
            map.Add(typeof(EdataFile), ContentFileType.Edata);

            return map;
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

        protected virtual Dictionary<ContentFileType, LoadFilesFunc> CreateLoadFuncsMap()
        {
            var map = new Dictionary<ContentFileType, LoadFilesFunc>();
            map.Add(ContentFileType.Edata, (contentFiles, token) =>
            {
                (new EdataFileReader()).LoadContent(contentFiles.OfType<EdataContentFile>());
            });

            return map;
        }

        protected ContentFileType ResolveFileType(byte[] binData)
        {
            foreach (var containerType in KnownContainerTypes)
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

        protected ContentFileType ResolveFileType(IContainerFile file)
        {
            //To powinno wywnioskować na podstawie typu ownera, a nie samego pliku przekazanego
            //poki co wspeiramy tylkoe data wiec ejst ok, ale ne dłuższ mete zle

            foreach (var typeFileTypePair in ContainerFileTypes)
            {
                if (typeFileTypePair.Key.IsInstanceOfType(file))
                {
                    return typeFileTypePair.Value;
                }
            }

            return ContentFileType.Unknown;
        }


    }
}
