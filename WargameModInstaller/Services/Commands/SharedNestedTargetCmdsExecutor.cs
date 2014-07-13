using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Containers;
using WargameModInstaller.Infrastructure.Content;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Services.Commands.Base;
using WargameModInstaller.Services.Containers;

namespace WargameModInstaller.Services.Commands
{
    public class SharedNestedTargetCmdsExecutor : ContainerCmdsExecutorBase<SharedNestedTargetCmdGroup>
    {
        public SharedNestedTargetCmdsExecutor(
            SharedNestedTargetCmdGroup cmdGroup, 
            ICmdExecutorFactory executorsFactory,
            IContainerReaderService containerReader,
            IContainerWriterService containerWriter)
            : base(cmdGroup, executorsFactory, containerReader, containerWriter)
        {
            this.TotalSteps += 2;
        }

        public override void Execute(CmdExecutionContext context, CancellationToken token)
        {
            //Ta metoda bez bloków łapania wyjątków, w przypadku ewentualnego wyjątku pochodzącego z kodu z poza execute, 
            //spowoduje wykrzaczenie się całej instalacji. Może trzeba zaimplementować IsCritical także dla CmdGroup...

            String targetFullPath = CommandGroup.TargetPath
                .GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetFullPath))
            {
                //Jeśli ten plik nie istnieje to szlag wszystkie komendy wewnętrzne.
                throw new CmdExecutionFailedException(
                    String.Format("A specified target file: \"{0}\" doesn't exist", targetFullPath),
                    String.Format(Properties.Resources.NotExistingFileOperationErrorParamMsg, targetFullPath));
            }

            String rootContentPath = CommandGroup.NestedTargetPath.Parts.FirstOrDefault();
            if (rootContentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Command's TargetContentPath doesn't contain any proper content path part.");
            }

            CurrentStep = 0;

            List<String> temporaryCreatedContainers = null;
            try
            {
                Stack<ContainerHierarchyEntity> containersHierarchy = null;

                //Tu wciąż brak komunikatu co się dzieje, tak więc wciąz jest wyświetlany poprzedni
                //czyli backup lub initialization jeśli ta komenda jest pierwszą.
                UnrollContainers(targetFullPath, out temporaryCreatedContainers, out containersHierarchy);

                var lastContainerFile = containersHierarchy.Peek().ContainerFile;

                //Interesuje nas tylko ostatni, bo to jego content bedzie ładowany, 
                //reszta jest tylko rozwinieta na dysku ale nia ma załadowane wiecej jak jeden plik.
                ContainerFileLoadManager loadManager = new ContainerFileLoadManager();
                loadManager.MaxLoadReached += (sender, args) =>
                {
                    CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, CommandGroup.TargetPath);

                    ContainerWriterService.WriteFile(lastContainerFile, token);
                    loadManager.FreeManagedFilesLoad();
                };

                var newExecutionContext = new SharedContainerCmdExecContext(
                    context.InstallerSourceDirectory,
                    context.InstallerTargetDirectory,
                    lastContainerFile);

                foreach (var executor in Executors)
                {
                    var cmd = GetExecutorsAssociatedCommand(executor);
                    var paths = GetPathsOfContentFiles(cmd);
                    var contentFiles = GetContentFiles(lastContainerFile, paths);

                    loadManager.AddManagedFiles(contentFiles);

                    LoadContentFiles(contentFiles);

                    executor.Execute(newExecutionContext, token);
                }

                CurrentStep++;
                CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, CommandGroup.TargetPath);

                RollContainers(containersHierarchy, token);
            }
            finally
            {
                if (temporaryCreatedContainers != null)
                {
                    temporaryCreatedContainers.ForEach(x => File.Delete(x));
                }
            }

            //Set max, completed
            CurrentStep = TotalSteps;
        }

        private void UnrollContainers(
            String targetPath, 
            out List<String> tempContainers,
            out Stack<ContainerHierarchyEntity> containersHierarchy)
        {
            tempContainers = new List<String>();
            containersHierarchy = new Stack<ContainerHierarchyEntity>();

            IContentFileWriter contentWriter = new ContentFileWriter();

            IContainerFile topContainer = ContainerReaderService.ReadFile(targetPath, false);
            containersHierarchy.Push(new ContainerHierarchyEntity(topContainer));

            foreach (var containerPath in CommandGroup.NestedTargetPath.Parts)
            {
                var containerFile = containersHierarchy.Peek().ContainerFile;
                var contentFile = containerFile.GetContentFileByPath(containerPath);
                //if (contentFile.FileType == ContentFileType.Edata)
                //{
                    ContainerReaderService.LoadContent(contentFile);
                    byte[] content = contentFile.Content;

                    //Czytamy z pamięci żeby szybciej było, ale nie łądujemy zawartości żeby oszczedzić miejsce.
                    var virtualContainer = ContainerReaderService.ReadRaw(content, false);

                    //Tutaj mieć na uwadze, że ten sposób pozyskiwania ścieżki jest bardzo podatny na problemy, miejsce wolne itd.
                    String lastPath = Path.Combine(Path.GetDirectoryName(targetPath), Path.GetFileName(contentFile.Path));
                    contentWriter.Write(lastPath, content);

                    virtualContainer.Path = lastPath;

                    //Tutaj brakuje jakiejś możliwosci zmiany ścieżki dla kontenera 
                    //i zmiany stanu z wirtualnego do przechowywanego na dysku.

                    containersHierarchy.Push(new ContainerHierarchyEntity(virtualContainer, containerPath));

                    tempContainers.Add(lastPath);
                //}
            }
        }

        private void RollContainers(
            Stack<ContainerHierarchyEntity> containersHierarchy,
            CancellationToken token)
        {
            IContentFileReader contentFileReader = new ContentFileReader();

            while (containersHierarchy.Count > 0)
            {
                var currentEntity = containersHierarchy.Pop();
                var parentEntity = containersHierarchy.Count > 0 ? containersHierarchy.Peek() : null;

                ContainerWriterService.WriteFile(currentEntity.ContainerFile, token);
                if (parentEntity != null)
                {
                    var owner = parentEntity.ContainerFile.GetContentFileByPath(currentEntity.OwnerPath);
                    var content = contentFileReader.Read(currentEntity.ContainerFile.Path);
                    owner.LoadOrginalContent(content); //Original or Custom?
                }
            }
        }

        #region Nested Class ContainerHierarchyEntity

        protected class ContainerHierarchyEntity
        {
            public ContainerHierarchyEntity(IContainerFile file)
            {
                this.ContainerFile = file;
                this.IsRoot = true;
            }

            public ContainerHierarchyEntity(IContainerFile file, String ownerPath)
            {
                this.ContainerFile = file;
                this.OwnerPath = ownerPath;
                this.IsRoot = false;
            }

            public bool IsRoot { get; private set; }
            public String OwnerPath { get; private set; }
            public IContainerFile ContainerFile { get; private set; }
        }

        #endregion //ContainerHierarchyEntity

    }
}
