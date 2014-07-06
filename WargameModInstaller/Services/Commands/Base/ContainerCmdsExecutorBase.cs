using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Containers;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Services.Commands.Base
{
    public abstract class ContainerCmdsExecutorBase<T> : CmdGroupExecutorBase<T> where T : ICmdGroup
    {
        public ContainerCmdsExecutorBase(
            T cmdGroup, 
            ICmdExecutorFactory executorsFactory,
            IContainerReaderService containerReader,
            IContainerWriterService containerWriter)
            : base(cmdGroup, executorsFactory)
        {
            this.ContainerReaderService = containerReader;
            this.ContainerWriterService = containerWriter;
        }

        protected IContainerReaderService ContainerReaderService
        {
            get;
            private set;
        }

        protected IContainerWriterService ContainerWriterService
        {
            get;
            private set;
        }

        protected void SaveContainerChanges(IContainerFile container, CancellationToken token)
        {
            //Trzeba pomyśleć o innej ścieżce dla tego komunikatu, bo ta jest pełna i powoduje zbyt duże
            //zmiany długosci wyswietlanych komunikatów w interfejsie, poza tym jest pełna, a wyswietlane sa relatywne.
            CurrentMessage = String.Format(
                Properties.Resources.RebuildingParametrizedMsg,
                container.Path);

            ContainerWriterService.WriteFile(container, token);
        }

        protected IEnumerable<String> GetPathsOfContentFiles(IInstallCmd cmd)
        {
            var contentFilesPaths = new List<String>();

            var nestedTargetCmd = cmd as IHasNestedTarget;
            if (nestedTargetCmd != null && nestedTargetCmd.UsesNestedTargetContent)
            {
                contentFilesPaths.Add(nestedTargetCmd.NestedTargetPath.LastPart);
            }

            var nestedSourceCmd = cmd as IHasNestedSource;
            if (nestedSourceCmd != null && nestedSourceCmd.UsesNestedSourceContent)
            {
                contentFilesPaths.Add(nestedSourceCmd.NestedSourcePath.LastPart);
            }

            return contentFilesPaths;
        }

        protected IEnumerable<IContentFile> GetContentFiles(IContainerFile owner, IEnumerable<String> paths)
        {
            var resultContentFiles = new List<IContentFile>();
            foreach (var path in paths)
            {
                if (owner.ContainsContentFileWithPath(path))
                {
                    var contentFile = owner.GetContentFileByPath(path);
                    resultContentFiles.Add(contentFile);
                }
                else
                {
                    //Wygląda na to że nie można tu chyba użyć wyjątku na tym etapie, jako że niektóre ścieżki,
                    //które trafia do tej metody mogą pochodzić od komend które dopiero tworzą content, 
                    //więc wystąpił by wyjatek w teoretycznie poprawnej sytuacji.
                    var warning = String.Format("The following content file: \"{0}\"" +
                    "cannot be get from the specified container file.", path);
                    Common.Logging.LoggerFactory.Create(this.GetType()).Warn(warning);
                }
            }

            return resultContentFiles;
        }

        protected void LoadContentFiles(IEnumerable<IContentFile> files)
        {
            var notLoadedFiles = files.Where(x => !x.IsContentLoaded);
            ContainerReaderService.LoadContent(notLoadedFiles);

            //foreach (var file in files)
            //{
            //    if (!file.IsContentLoaded)
            //    {
            //        ContainerReaderService.LoadContent(file);
            //    }
            //}
        }

    }
}
