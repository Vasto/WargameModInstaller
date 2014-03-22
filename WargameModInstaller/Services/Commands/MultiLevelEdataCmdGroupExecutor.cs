using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Content;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Services.Commands
{
    public class MultiLevelEdataCmdGroupExecutor : CmdGroupExecutorBase<MultiLevelEdataCmdGroup>
    {
        public MultiLevelEdataCmdGroupExecutor(MultiLevelEdataCmdGroup cmdGroup, ICmdExecutorFactory executorsFactory)
            : base(cmdGroup, executorsFactory)
        {
            this.TotalSteps += 2;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            //Ta metoda bez bloków łapania wyjątków, w przypadku ewentualnego wyjątku pochodzącego z kodu z poza execute, spowoduje 
            //wykrzaczenie się całej instalacji. Może trzeba zaimplementować IsCritical także dla CmdGroup...

            String targetfullPath = CommandGroup.CommonEdataPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                //Jeśli ten plik nie istnieje to szlag wszystkie komendy wewnętrzne.
                throw new CmdExecutionFailedException("Specified Edata file doesn't exist",
                    String.Format(Properties.Resources.NotExistingFileOperationErrorParametrizedMsg, targetfullPath));
            }

            String rootContentPath = CommandGroup.CommonMultiLevelEdataPath.Parts.FirstOrDefault();
            if (rootContentPath == null)
            {
                throw new CmdExecutionFailedException("Invalid command's TargetContentPath value.");
            }

            CurrentStep = 0;

            //Update, wszyskite laduja na dysku, tak bedzie najbezpieczniej jednak.
            //Teraz tylko wyzwolić pojedyncze command executory od tego całęgo bajzlu, zagniezdzonymi pakietami, 
            //poprostu wrócić do sytuacji z przed dodania tego, maja świadomośc tylko ostaniego edata i chu ich obchodzi reszta.

            //Pozatym wypadało by zoragnizować jakąś liste może tych edata odczytanych w ten poniższy spsoób, zeby nie robić teog pozniej 2 raz.
            //Wazne zeby kazdy znich miał czysty content, nie był wirtualny a odnosił sie do konkretnego pliku na dysku.

            List<String> temporaryCreatedEdatas;
            Stack<EdataHierarchyEntity> edataHierarchy;

            UnrollEdatas(targetfullPath, out temporaryCreatedEdatas, out edataHierarchy);

            var newExecutionContext = new SharedEdataCmdExecutionContext(
                context.InstallerSourceDirectory,
                context.InstallerTargetDirectory,
                edataHierarchy.Peek().EdataFile);

            foreach (var executor in CommandExecutors)
            {
                executor.Execute(newExecutionContext, token.Value);
            }

            CurrentStep++;
            CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, this.CommandGroup.CommonEdataPath);

            RollEdatas(edataHierarchy, token);

            foreach (var tmpEdata in temporaryCreatedEdatas)
            {
                File.Delete(tmpEdata);
            }

            //Set max, completed
            CurrentStep = TotalSteps;
        }

        private void UnrollEdatas(
            String targetfullPath, 
            out List<String> temporaryEdatas,
            out Stack<EdataHierarchyEntity> edataHierarchy)
        {
            temporaryEdatas = new List<String>();
            edataHierarchy = new Stack<EdataHierarchyEntity>();

            IEdataFileReader edataFileReader = new EdataFileReader();
            IEdataBinReader edataBinReader = new EdataBinReader();
            IContentFileWriter contentWriter = new ContentFileWriter();

            EdataFile mainEdataFile = edataFileReader.Read(targetfullPath, false);
            edataHierarchy.Push(new EdataHierarchyEntity(mainEdataFile));

            foreach (var edataPath in CommandGroup.CommonMultiLevelEdataPath.Parts)
            {
                var contentFile = GetEdataContentFileByPath(edataHierarchy.Peek().EdataFile, edataPath);
                if (contentFile.FileType == EdataContentFileType.Package)
                {
                    var content = edataFileReader.ReadContent(contentFile);
                    //Czytamy z pamięci żeby szybciej było, ale nie łądujemy zawartości żeby oszczedzić miejsce.
                    var virtualEdata = edataBinReader.Read(content, false);

                    //Tutaj mieć na uwadze, że ten sposób pozyskiwania ścieżki jest bardzo podatny na problemy, miejsce wolne itd.
                    String lastPath = Path.Combine(Path.GetDirectoryName(targetfullPath), Path.GetFileName(contentFile.Path));
                    contentWriter.Write(lastPath, content);

                    var lastEdata = new EdataFile(lastPath, virtualEdata.Header, virtualEdata.PostHeaderData, virtualEdata.ContentFiles);
                    edataHierarchy.Push(new EdataHierarchyEntity(lastEdata, edataPath));

                    temporaryEdatas.Add(lastPath);
                }
            }
        }

        private void RollEdatas(Stack<EdataHierarchyEntity> edataHierarchy, CancellationToken? token)
        {
            IEdataFileWriter edataFileWriter = new EdataFileWriter();
            IContentFileReader contentFileReader = new ContentFileReader();

            while (edataHierarchy.Count > 0)
            {
                var currentEntity = edataHierarchy.Pop();
                var parentEntity = edataHierarchy.Count > 0 ? edataHierarchy.Peek() : null;

                edataFileWriter.Write(currentEntity.EdataFile, token.Value);
                if (parentEntity != null)
                {
                    var holdingFileOfParentEdata = GetEdataContentFileByPath(parentEntity.EdataFile, currentEntity.OwnerPath);
                    holdingFileOfParentEdata.Content = contentFileReader.Read(currentEntity.EdataFile.Path);
                }
            }
        }

        protected EdataContentFile GetEdataContentFileByPath(EdataFile edataFile, String edataContentPath)
        {
            var edataContentFile = edataFile.ContentFiles.FirstOrDefault(f => f.Path == edataContentPath);
            if (edataContentFile == null)
            {
                throw new CmdExecutionFailedException(
                    String.Format("Cannot load \"{0}\"", edataContentPath),
                    String.Format(Properties.Resources.ContentFileNotFoundParametrizedMsg, edataContentPath));
            }

            return edataContentFile;
        }

        #region Nested Class EdataHierarchyEntity

        protected class EdataHierarchyEntity
        {
            public EdataHierarchyEntity(EdataFile file)
            {
                this.EdataFile = file;
                this.IsRoot = true;
            }

            public EdataHierarchyEntity(EdataFile file, String ownerPath)
            {
                this.EdataFile = file;
                this.OwnerPath = ownerPath;
                this.IsRoot = false;
            }

            public bool IsRoot { get; private set; }
            public String OwnerPath { get; private set; }
            public EdataFile EdataFile { get; private set; }
        }

        #endregion //EdataHierarchyEntity

    }
}
