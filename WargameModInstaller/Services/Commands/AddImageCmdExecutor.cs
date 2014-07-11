using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Model.Containers.Edata;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    public class AddImageCmdExecutor : ModImageBySourceCmdExecutor<AddImageCmd>
    {
        public AddImageCmdExecutor(AddImageCmd command)
            : base(command)
        {
            this.DefaultExecutionErrorMsg = String.Format(
                Properties.Resources.AddContentErrorParamMsg, 
                Command.SourcePath);
        }

        protected override void ExecuteCommandsLogic(CmdsExecutionData data)
        {
            if (!data.ContainerFile.ContainsContentFileWithPath(data.ContentPath))
            {
                TgvImage image = DDSFileToTgv(data.ModificationSourcePath, !Command.UseMipMaps);
                //Trzeba mieć na to oko, czy nie powoduje problemów, bo przy replace była używana checksuma starego obrazka.
                image.SourceChecksum = !String.IsNullOrEmpty(Command.Checksum) 
                    ? MiscUtilities.HexByteStringToByteArray(Command.Checksum)
                    : image.ComputeContentChecksum();
                image.IsCompressed = Command.UseCompression;

                var newContentFile = new EdataContentFile();
                newContentFile.Path = data.ContentPath;
                newContentFile.LoadCustomContent(TgvToBytes(image, !Command.UseMipMaps));

                data.ContainerFile.AddContentFile(newContentFile);
            }
            else if (Command.OverwriteIfExist)
            {
                var contentFile = data.ContainerFile.GetContentFileByPath(data.ContentPath);

                //To nie będzie potrzebne tutaj jeśli nie bedzie trzeba ładować i wykorzystywać starego obrazka.
                if (contentFile.FileType != ContentFileType.Image)
                {
                    throw new CmdExecutionFailedException(
                        String.Format("Invalid TargetContentPath: \"{0}\". It doesn't target an image content file.", data.ContentPath),
                        DefaultExecutionErrorMsg);
                }

                //Ładowanie starego obrazka nie będzie potrzebne jeśli bedzie pewnośc ze recznie wygenerowana checkusma jest ok.
                //Ok, tu kiedyś było odczytywanie starych danych z starego obrazka, teraz checksuma jest generowana na nowo,
                //żeby uniknac konieczności ładowania tych starych danych, bo nie sa potrzeben przy dodawaniu, a tutaj były by potrzebne
                //i trudno to rozwiązać przy założeniu ze dane są ładowane na zewnątrz.

                //TgvImage oldTgv = BytesToTgv(contentFile.Content);
                TgvImage image = DDSFileToTgv(data.ModificationSourcePath, !Command.UseMipMaps);
                image.SourceChecksum = !String.IsNullOrEmpty(Command.Checksum)
                    ? MiscUtilities.HexByteStringToByteArray(Command.Checksum)
                    : image.ComputeContentChecksum();
                image.IsCompressed = Command.UseCompression;

                contentFile.LoadCustomContent(TgvToBytes(image, !Command.UseMipMaps));
            }
        }

    }
}
