using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands
{
    //To do: reconsider method names

    public abstract class ReplaceImageCmdExecutorBase<T> : CmdExecutorBase<T> where T : IInstallCmd
    {
        public ReplaceImageCmdExecutorBase(T command) : base(command)
        {

        }

        protected EdataContentFile GetEdataContentFileByPath(EdataFile edataFile, String edataContentPath)
        {
            var edataContentFile = edataFile.ContentFiles.FirstOrDefault(f => f.Path == edataContentPath);
            if (edataContentFile == null)
            {
                throw new CmdExecutionFailedException(
                    String.Format("Cannot load \"{0}\"", edataContentPath),
                    WargameModInstaller.Properties.Resources.LoadEdataErrorMsg);
            }

            return edataContentFile;
        }

        protected TgvImage GetTgvFromDDS(String sourceFullPath)
        {
            ITgvFileReader tgvReader = new TgvDDSReader();
            TgvImage newtgv = tgvReader.Read(sourceFullPath);

            return newtgv;
        }

        protected TgvImage GetTgvFromContent(EdataContentFile edataContentFile)
        {
            ITgvBinReader rawReader = new TgvBinReader();
            TgvImage oldTgv = rawReader.Read(edataContentFile.Content);

            return oldTgv;
        }

        protected byte[] ConvertTgvToBytes(TgvImage tgv)
        {
            ITgvBinWriter tgvRawWriter = new TgvNoMipMapBinWriter();
            var bytes = tgvRawWriter.Write(tgv);

            return bytes;
        }

        protected bool CanGetEdataFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext != null)
            {
                return sharedEdataContext.EdataFile != null;
            }
            else
            {
                return false;
            }
        }

        protected EdataFile GetEdataFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext != null)
            {
                return sharedEdataContext.EdataFile;
            }
            else
            {
                throw new InvalidOperationException("Cannot get Edata file from the given context");
            }
        }

        protected IList<EdataContentFile> GetContentFilesHierarchy(EdataFile root, string[] subContetPaths)
        {
            var contentFilesList = new List<EdataContentFile>();

            var edataBinReader = new EdataBinReader();
            EdataFile lastContentOwner = root;
            foreach (var path in subContetPaths)
            {
                EdataContentFile cf = GetEdataContentFileByPath(lastContentOwner, path);
                contentFilesList.Add(cf);
                if (cf.FileType == EdataContentFileType.Package)
                {
                    lastContentOwner = edataBinReader.Read(cf.Content);
                }
                else
                {
                    break;
                }
            }

            return contentFilesList;
        }

        protected void AssignContentUpHierarchy(IEnumerable<EdataContentFile> contentFilesList, byte[] content)
        {
            var reversedConententFilesList = new List<EdataContentFile>(contentFilesList);
            reversedConententFilesList.Reverse();

            var edataBinWriter = new EdataBinWriter();
            byte[] lastContent = content;
            foreach (var cf in reversedConententFilesList)
            {
                cf.Content = lastContent;
                cf.Size = lastContent.Length;

                if (cf.Owner.IsVirtual)
                {
                    lastContent = edataBinWriter.Write(cf.Owner);
                }
                else
                {
                    break;
                }
            }
        }

    }

}
