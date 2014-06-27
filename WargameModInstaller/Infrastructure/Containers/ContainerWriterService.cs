using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Containers.Edata;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers
{
    /// <summary>
    /// 
    /// </summary>
    public class ContainerWriterService : IContainerWriterService
    {
        protected readonly Dictionary<Type, ContentFileType> containerTypeToFileTypeMap;
        protected readonly Dictionary<ContentFileType, WriteFileFunc> writeFileFuncsMap;
        protected readonly Dictionary<ContentFileType, WriteRawFunc> writeRawFuncsMap;

        protected delegate void WriteFileFunc(IContainerFile containerFile, CancellationToken token);
        protected delegate byte[] WriteRawFunc(IContainerFile containerFile, CancellationToken token);

        public ContainerWriterService()
        {
            this.containerTypeToFileTypeMap = CreateContainerTypeToFileTypeMap();
            this.writeFileFuncsMap = CreateWriteFileFuncsMap();
            this.writeRawFuncsMap = CreateWriteRawFuncsMap();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFile"></param>
        public void WriteFile(IContainerFile containerFile)
        {
            WriteFile(containerFile, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFile"></param>
        /// <param name="token"></param>
        public void WriteFile(IContainerFile containerFile, CancellationToken token)
        {
            if (containerFile == null)
            {
                throw new ArgumentNullException("containerFile");
            }

            var fileType = ResolveFileType(containerFile);

            var writeFunc = writeFileFuncsMap[fileType];

            writeFunc(containerFile, token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFile"></param>
        /// <returns></returns>
        public byte[] WriteRaw(IContainerFile containerFile)
        {
            return WriteRaw(containerFile, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="containerFile"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public byte[] WriteRaw(IContainerFile containerFile, CancellationToken token)
        {
            if (containerFile == null)
            {
                throw new ArgumentNullException("containerFile");
            }

            var fileType = ResolveFileType(containerFile);

            var writeFunc = writeRawFuncsMap[fileType];

            byte[] rawFileData = writeFunc(containerFile, token);

            return rawFileData;
        }

        protected virtual Dictionary<Type, ContentFileType> CreateContainerTypeToFileTypeMap()
        {
            var result = new Dictionary<Type, ContentFileType>();
            result.Add(typeof(EdataFile), ContentFileType.Edata);
            //result.Add(typeof(ProxyFile), ContentFileType2.Prxypcpc);
            //result.Add(typeof(MeshFile), ContentFileType2.Mesh);

            return result;
        }

        protected virtual Dictionary<ContentFileType, WriteFileFunc> CreateWriteFileFuncsMap()
        {
            var map = new Dictionary<ContentFileType, WriteFileFunc>();
            map.Add(ContentFileType.Edata, (container, token) =>
            {
                (new EdataFileWriter()).Write((EdataFile)container, token);
            });

            return map;
        }

        protected virtual Dictionary<ContentFileType, WriteRawFunc> CreateWriteRawFuncsMap()
        {
            var map = new Dictionary<ContentFileType, WriteRawFunc>();
            map.Add(ContentFileType.Edata, (container, token) =>
            {
                return (new EdataBinWriter()).Write((EdataFile)container, token);
            });

            return map;
        }

        protected ContentFileType ResolveFileType(IContainerFile container)
        {
            foreach (var pair in containerTypeToFileTypeMap)
            {
                if (pair.Key.IsInstanceOfType(container))
                {
                    return pair.Value;
                }
            }

            return ContentFileType.Unknown;
        }

    }
}
