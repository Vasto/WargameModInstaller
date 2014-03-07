using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Model;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Services.Config;

namespace WargameModInstaller.Infrastructure.Commands
{
    //To do: Make this independant of any high level services like the ISettingsProvider.
    //There should be a some high level command providing service which would use this reader, 
    //and eventually use other services to adjust the commands values.

    /// <summary>
    /// 
    /// </summary>
    public class InstallCmdReader : WMIReaderBase<XElement, IEnumerable<IInstallCmd>>, IInstallCmdReader
    {
        private readonly String installCommandsElementPath = "WargameModInstallerConfig/InstallCommands";
        private readonly ISettingsProvider settingsProvider;
        private readonly bool defaultCriticalValue;

        public InstallCmdReader(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
            this.defaultCriticalValue = settingsProvider
                .GetGeneralSettings(GeneralSettingEntryType.CriticalCommands)
                .Value
                .ToOrDefault<bool>();
        }

        /// <summary>
        /// Reads all install command entires.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IEnumerable<IInstallCmd> ReadAll(String filePath)
        {
            var cmds = new List<IInstallCmd>();
            try
            {
                XDocument configFile = XDocument.Load(filePath);
                XElement rootElement = configFile.XPathSelectElement(installCommandsElementPath);
                if (rootElement != null)
                {
                    foreach (var cmdQuery in ReadingQueries.Values)
                    {
                        var queryResult = cmdQuery(rootElement);
                        cmds.AddRange(queryResult);
                    }

                    int id = 0;
                    cmds.ForEach(cmd => cmd.Id = id++);
                }
            }
            catch (XmlException ex)
            {
                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return cmds;
        }

        /// <summary>
        /// Reads install command entires of a specified type.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IEnumerable<IInstallCmd> Read(String file, CmdEntryType entryType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads all install comand entries and groups them if possible.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IEnumerable<ICmdGroup> ReadGroups(String filePath)
        {
            var cmdGroupsList = new List<ICmdGroup>();

            try
            {
                XDocument configFile = XDocument.Load(filePath);
                XElement rootElement = configFile.XPathSelectElement(installCommandsElementPath);
                if (rootElement != null)
                {
                    var cmds = new List<IInstallCmd>();
                    foreach (var cmdQuery in ReadingQueries.Values)
                    {
                        var queryResult = cmdQuery(rootElement);
                        cmds.AddRange(queryResult);
                    }

                    int id = 0;
                    cmds.ForEach(cmd => cmd.Id = id++);

                    //Make a more generic way to produce groups and define something like the group production rules.
                    //At this point, below part neeeds to be totaly redone...

                    var cmdPriorityGroups = cmds.GroupBy(cmd => cmd.Priority);
                    foreach (var priorityGroup in cmdPriorityGroups)
                    {
                        //Create groups with the same priority and target
                        var cmdTargetGroups = priorityGroup
                            .OfType<IHasTarget>()
                            .GroupBy(cmd => cmd.TargetPath);
                        foreach (var targetGroup in cmdTargetGroups)
                        {
                            //Assign appropriate command types, with the same edata target to the shared Edata cmds group.
                            var replaceImageCmds = new List<IInstallCmd>();
                            replaceImageCmds.AddRange(targetGroup.OfType<ReplaceImageCmd>());
                            replaceImageCmds.AddRange(targetGroup.OfType<ReplaceImagePartCmd>());
                            replaceImageCmds.AddRange(targetGroup.OfType<ReplaceImageTileCmd>());
                            replaceImageCmds.AddRange(targetGroup.OfType<ReplaceContentCmd>());
                            if (replaceImageCmds.Count > 0)
                            {
                                var sharedEdataCmdGroup = new SharedEdataCmdGroup(
                                    replaceImageCmds,
                                    new InstallEntityPath(targetGroup.Key),
                                    priorityGroup.Key);

                                cmdGroupsList.Add(sharedEdataCmdGroup);
                            }
                        }

                        //Assign remaining commands to the normal group
                        var samePriorityCmds = priorityGroup.Except(cmdGroupsList.SelectMany(group => group.Commands));
                        if (samePriorityCmds.Count() > 0)
                        {
                            var samePriorityCmdsGroup = new BasicCmdGroup(samePriorityCmds, priorityGroup.Key);

                            cmdGroupsList.Add(samePriorityCmdsGroup);
                        }
                    }

                    cmdGroupsList.OrderByDescending(group => group.Priority);
                }
            }
            catch (XmlException ex)
            {
                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return cmdGroupsList;
        }

        protected override Dictionary<WMIEntryType, Func<XElement, IEnumerable<IInstallCmd>>> CreateReadingQueries()
        {
            var result = new Dictionary<WMIEntryType, Func<XElement, IEnumerable<IInstallCmd>>>();
            result.Add(CmdEntryType.CopyGameFile, ReadCopyModFileCmds);
            result.Add(CmdEntryType.CopyModFile, ReadCopyGameFileCmds);
            result.Add(CmdEntryType.RemoveFile, ReadRemoveFileCmds);
            result.Add(CmdEntryType.ReplaceImage, ReadReplaceImageCmds);
            result.Add(CmdEntryType.ReplaceImagePart, ReadReplaceImageTileCmds);
            result.Add(CmdEntryType.ReplaceImageTile, ReadReplaceImagePartCmds);
            result.Add(CmdEntryType.ReplaceContent, ReadReplaceContentCmds);

            return result;
        }

        private IEnumerable<CopyModFileCmd> ReadCopyModFileCmds(XElement source)
        {
            var result = new List<CopyModFileCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.CopyModFile.Name);//zastapić to przez entry name 
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(3);

                //Any validation here is not a good idea. Commands should be left over in an invalid state, 
                //so eventually installation will fail if they are marked as critical.
                var newCmd = new CopyModFileCmd();
                //We set up path type to a expected path type.
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<CopyGameFileCmd> ReadCopyGameFileCmds(XElement source)
        {
            var result = new List<CopyGameFileCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.CopyGameFile.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(4);

                var newCmd = new CopyGameFileCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<RemoveFileCmd> ReadRemoveFileCmds(XElement source)
        {
            var result = new List<RemoveFileCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.RemoveFile.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(1);

                var newCmd = new RemoveFileCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<ReplaceImageCmd> ReadReplaceImageCmds(XElement source)
        {
            var result = new List<ReplaceImageCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.ReplaceImage.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var edataImagePath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceImageCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.TargetContentPath = new ContentPath(edataImagePath);
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<ReplaceImageTileCmd> ReadReplaceImageTileCmds(XElement source)
        {
            var result = new List<ReplaceImageTileCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.ReplaceImageTile.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var edataImagePath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var column = cmdElement.Attribute("column").ValueOrDefault<int?>();
                var row = cmdElement.Attribute("row").ValueOrDefault<int?>();
                var tileSize = cmdElement.Attribute("tileSize").ValueOr<int>(256);
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceImageTileCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.TargetContentPath = new ContentPath(edataImagePath);
                newCmd.Column = column;
                newCmd.Row = row;
                newCmd.TileSize = tileSize;
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<ReplaceImagePartCmd> ReadReplaceImagePartCmds(XElement source)
        {
            var result = new List<ReplaceImagePartCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.ReplaceImagePart.Name); 
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var edataImagePath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var xPos = cmdElement.Attribute("xPos").ValueOr<int>(0);
                var yPos = cmdElement.Attribute("yPos").ValueOr<int>(0);
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceImagePartCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.TargetContentPath = new ContentPath(edataImagePath);
                newCmd.XPosition = xPos;
                newCmd.YPosition = yPos;
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<ReplaceContentCmd> ReadReplaceContentCmds(XElement source)
        {
            var result = new List<ReplaceContentCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.ReplaceContent.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var edataImagePath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(defaultCriticalValue);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceContentCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.TargetContentPath = new ContentPath(edataImagePath);
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

    }

}
