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
using WargameModInstaller.Model;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Services.Config;

namespace WargameModInstaller.Infrastructure.Commands
{
    //To do: Make this unaware of any high level services like the ISettingsProvider.
    //There should be a some high level command providing service which would use this reader, 
    //and eventually use other services to adjust the commands values.

    /// <summary>
    /// 
    /// </summary>
    public class InstallCmdReader : WMIReaderBase<XElement, IEnumerable<IInstallCmd>>, IInstallCmdReader
    {
        private readonly String installCommandsElementPath = "WargameModInstallerConfig/InstallCommands";
        private readonly ISettingsProvider settingsProvider;
        private readonly bool criticalCmdsDefault;
        private readonly bool useMipMapsDefault;
        private readonly bool overwriteIfExistDefault;
        private readonly bool compressDefault;

        public InstallCmdReader(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
            this.useMipMapsDefault = false;
            this.compressDefault = true;
            this.overwriteIfExistDefault = true;
            this.criticalCmdsDefault = settingsProvider
                .GetGeneralSettings(GeneralSettingEntryType.CriticalCommands)
                .Value
                .ToOrDefault<bool>();
            this.GroupProductionRules = CreateGroupProductionRules().OrderByDescending(x => x.Priority);
        }

        protected IEnumerable<GroupProductionRule> GroupProductionRules
        {
            get;
            private set;
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
                if (rootElement == null)
                {
                    return cmds;
                }

                foreach (var cmdQuery in ReadingQueries.Values)
                {
                    var queryResult = cmdQuery(rootElement);
                    cmds.AddRange(queryResult);
                }

                int id = 0;
                cmds.ForEach(cmd => cmd.Id = id++);
            }
            catch (XmlException ex)
            {
                Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return cmds;
        }

        /// <summary>
        /// Reads all install command entires belonging to the given components entries collection.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public IEnumerable<IInstallCmd> ReadAll(String filePath, IEnumerable<String> components)
        {
            var cmds = new List<IInstallCmd>();
            try
            {
                XDocument configFile = XDocument.Load(filePath);
                XElement rootElement = configFile.XPathSelectElement(installCommandsElementPath);
                if (rootElement == null)
                {
                    return cmds;
                }

                var cmdParentElements = new List<XElement>();
                var componentsNames = new HashSet<String>(components);
                foreach (var name in componentsNames)
                {
                    var element = rootElement.XPathSelectElement(String.Format("//*[@name=\"{0}\"]", name));
                    if (element != null)
                    {
                        cmdParentElements.Add(element);
                    }
                }

                foreach (var element in cmdParentElements)
                {
                    foreach (var cmdQuery in ReadingQueries.Values)
                    {
                        var queryResult = cmdQuery(element);
                        cmds.AddRange(queryResult);
                    }
                }

                int id = 0;
                cmds.ForEach(cmd => cmd.Id = id++);
            }
            catch (XmlException ex)
            {
                Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return cmds;
        }

        /// <summary>
        /// Reads all install comand entries and groups them if possible.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual IEnumerable<ICmdGroup> ReadGroups(String filePath)
        {
            var cmdGroupsList = new List<ICmdGroup>();
            var cmdsList = ReadAll(filePath).ToList();

            try
            {
                cmdGroupsList = CreateCommandGroups(cmdsList);

                cmdGroupsList.OrderByDescending(group => group.Priority);
            }
            catch (XmlException ex)
            {
                Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return cmdGroupsList;
        }

        /// <summary>
        /// Reads all install comand entires belonging to the given components entries collection
        /// and groups them if possible.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        public IEnumerable<ICmdGroup> ReadGroups(String filePath, IEnumerable<String> components)
        {
            var cmdGroupsList = new List<ICmdGroup>();
            var cmdsList = ReadAll(filePath, components).ToList();

            try
            {
                cmdGroupsList = CreateCommandGroups(cmdsList);

                cmdGroupsList.OrderByDescending(group => group.Priority);
            }
            catch (XmlException ex)
            {
                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return cmdGroupsList;
        }

        protected List<ICmdGroup> CreateCommandGroups(List<IInstallCmd> cmdsList)
        {
            var cmdGroupsList = new List<ICmdGroup>();

            foreach (var rule in GroupProductionRules)
            {
                var group = rule.ProduceGroup(cmdsList);
                foreach (var grp in group)
                {
                    cmdsList.RemoveAll(cmd => grp.Commands.Contains(cmd)); // Do zastąpienia przez hasz set
                }

                cmdGroupsList.AddRange(group);
            }

            return cmdGroupsList;
        }

        protected override Dictionary<WMIEntryType, Func<XElement, IEnumerable<IInstallCmd>>> CreateReadingQueries()
        {
            var queries = new Dictionary<WMIEntryType, Func<XElement, IEnumerable<IInstallCmd>>>();
            queries.Add(CmdEntryType.CopyGameFile, ReadCopyModFileCmds);
            queries.Add(CmdEntryType.CopyModFile, ReadCopyGameFileCmds);
            queries.Add(CmdEntryType.RemoveFile, ReadRemoveFileCmds);
            queries.Add(CmdEntryType.ReplaceImage, ReadReplaceImageCmds);
            queries.Add(CmdEntryType.ReplaceImagePart, ReadReplaceImageTileCmds);
            queries.Add(CmdEntryType.ReplaceImageTile, ReadReplaceImagePartCmds);
            queries.Add(CmdEntryType.ReplaceContent, ReadReplaceContentCmds);
            queries.Add(CmdEntryType.AlterDictionary, ReadAlterDictionaryCmds);
            queries.Add(CmdEntryType.AddContent, ReadAddContentCmds);
            queries.Add(CmdEntryType.AddImage, ReadAddImageCmds);

            return queries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Higher priority number means higher priority rule.</remarks>
        protected virtual IEnumerable<GroupProductionRule> CreateGroupProductionRules()
        {
            var rules = new List<GroupProductionRule>();
            rules.Add(new GroupProductionRule(3, SharedNestedTargetCmdGroupProductionRule));
            rules.Add(new GroupProductionRule(2, SharedTargetCmdGroupProductionRule));
            rules.Add(new GroupProductionRule(1, BasicCmdGroupProductionRule));

            return rules;
        }

        private IEnumerable<ICmdGroup> BasicCmdGroupProductionRule(IEnumerable<IInstallCmd> cmds)
        {
            var validCmds = cmds.Where(cmd =>
                cmd is CopyGameFileCmd ||
                cmd is CopyModFileCmd ||
                cmd is RemoveFileCmd);

            var samePriorityGroups = validCmds.GroupBy(cmd => cmd.Priority);

            var resultGroups = new List<BasicCmdGroup>();
            foreach (var group in samePriorityGroups)
            {
                var newBasicGroup = new BasicCmdGroup(group, group.Key);
                resultGroups.Add(newBasicGroup);
            }

            return resultGroups;
        }

        private IEnumerable<ICmdGroup> SharedTargetCmdGroupProductionRule(IEnumerable<IInstallCmd> cmds)
        {
            var validCmds = cmds.Where(cmd =>
                cmd is ReplaceImageCmd ||
                cmd is ReplaceImagePartCmd ||
                cmd is ReplaceImageTileCmd ||
                cmd is ReplaceContentCmd ||
                cmd is AlterDictionaryCmd ||
                cmd is AddContentCmd ||
                cmd is AddImageCmd);

            var groups = from cmd in validCmds
                         group cmd by new {
                             ((IHasTarget)cmd).TargetPath,
                             cmd.Priority
                         };

            var resultGroups = new List<SharedTargetCmdGroup>();
            foreach (var group in groups)
            {
                var newGroup = new SharedTargetCmdGroup(
                    group,
                    group.Key.TargetPath,
                    group.Key.Priority);
                resultGroups.Add(newGroup);
            }

            return resultGroups;
        }

        private IEnumerable<ICmdGroup> SharedNestedTargetCmdGroupProductionRule(IEnumerable<IInstallCmd> cmds)
        {
            var multipleNestedCmds = cmds
                .Where(cmd => cmd is IHasNestedTarget && 
                    ((IHasNestedTarget)cmd).NestedTargetPath.PathType == ContentPathType.MultipleNested);

            var validCmds = multipleNestedCmds.Where(cmd =>
                cmd is ReplaceImageCmd ||
                cmd is ReplaceImagePartCmd ||
                cmd is ReplaceImageTileCmd ||
                cmd is ReplaceContentCmd ||
                cmd is AlterDictionaryCmd ||
                cmd is AddContentCmd ||
                cmd is AddImageCmd);
                
            var groups = from cmd in validCmds
                         group cmd by new { 
                             cmd.Priority,
                             ((IHasTarget)cmd).TargetPath, 
                             ((IHasNestedTarget)cmd).NestedTargetPath.PreLastPart 
                         };

            var resultGroups = new List<SharedNestedTargetCmdGroup>();
            foreach (var group in groups)
            {
                var nestedTargetPath = new ContentPath(
                    group.Key.PreLastPart);
                var newGroup = new SharedNestedTargetCmdGroup(
                    group, 
                    group.Key.TargetPath,
                    nestedTargetPath,
                    group.Key.Priority);
                resultGroups.Add(newGroup);
            }

            return resultGroups;
        }


        private IEnumerable<CopyModFileCmd> ReadCopyModFileCmds(XElement source)
        {
            var result = new List<CopyModFileCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.CopyModFile.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(4);

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
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(5);

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
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
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
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var useMipMaps = cmdElement.Attribute("useMipMaps").ValueOr<bool>(useMipMapsDefault);
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceImageCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.UseMipMaps = useMipMaps;
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
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var column = cmdElement.Attribute("column").ValueOrDefault<int?>();
                var row = cmdElement.Attribute("row").ValueOrDefault<int?>();
                var tileSize = cmdElement.Attribute("tileSize").ValueOr<int>(256);
                var useMipMaps = cmdElement.Attribute("useMipMaps").ValueOr<bool>(useMipMapsDefault);
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceImageTileCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.Column = column;
                newCmd.Row = row;
                newCmd.TileSize = tileSize;
                newCmd.UseMipMaps = useMipMaps;
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
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var xPos = cmdElement.Attribute("xPos").ValueOr<int>(0);
                var yPos = cmdElement.Attribute("yPos").ValueOr<int>(0);
                var useMipMaps = cmdElement.Attribute("useMipMaps").ValueOr<bool>(useMipMapsDefault);
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceImagePartCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.XPosition = xPos;
                newCmd.YPosition = yPos;
                newCmd.UseMipMaps = useMipMaps;
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
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                var newCmd = new ReplaceContentCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<AlterDictionaryCmd> ReadAlterDictionaryCmds(XElement source)
        {
            var result = new List<AlterDictionaryCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.AlterDictionary.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(2);

                //Read Entries
                var addEntryElements = cmdElement.Elements("AddEntry");
                var addEntries = ReadEntriesFromElements(addEntryElements);

                var delEntryElements = cmdElement.Elements("RemoveEntry");
                var delEntries = ReadEntriesFromElements(delEntryElements).Select(x => x.Key).ToList();

                var renameEntryElements = cmdElement.Elements("RenameEntry");
                var renameEntries = ReadEntriesFromElements(renameEntryElements);

                //For backward compatibility, reading of an old rename entries
                var legacyEntriesElements = cmdElement.Elements("Entry");
                var legacyEntries = ReadEntriesFromElements(legacyEntriesElements);

                renameEntries = renameEntries.Concat(legacyEntries).ToList();

                var newCmd = new AlterDictionaryCmd();
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.AddedEntries = addEntries;
                newCmd.RemovedEntries = delEntries;
                newCmd.RenamedEntries = renameEntries;
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<KeyValuePair<String, String>> ReadEntriesFromElements(IEnumerable<XElement> elements)
        {
            var entries = new List<KeyValuePair<String, String>>();
            foreach (var entryElement in elements)
            {
                //First try to read the attribute, because an empty tag element value returns an empty string not null.
                var value = entryElement.Attribute("value").ValueNullSafe() ??
                    entryElement.ValueNullSafe();

                var hash = entryElement.Attribute("hash").ValueNullSafe();
                var newEntry = new KeyValuePair<String, String>(hash, value);
                entries.Add(newEntry);
            }

            return entries;
        }

        private IEnumerable<AddContentCmd> ReadAddContentCmds(XElement source)
        {
            var result = new List<AddContentCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.AddContent.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var overwrite = cmdElement.Attribute("overwriteIfExist").ValueOr<bool>(overwriteIfExistDefault);
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(3);

                var newCmd = new AddContentCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.OverwriteIfExist = overwrite;
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        private IEnumerable<AddImageCmd> ReadAddImageCmds(XElement source)
        {
            var result = new List<AddImageCmd>();

            var cmdElementsCollection = source.Elements(CmdEntryType.AddImage.Name);
            foreach (var cmdElement in cmdElementsCollection)
            {
                var sourcePath = cmdElement.Attribute("sourcePath").ValueNullSafe();
                var targetPath = cmdElement.Attribute("targetPath").ValueNullSafe();
                var contentPath = cmdElement.Attribute("targetContentPath").ValueNullSafe();
                var overwrite = cmdElement.Attribute("overwriteIfExist").ValueOr<bool>(overwriteIfExistDefault);
                var useMipMaps = cmdElement.Attribute("useMipMaps").ValueOr<bool>(useMipMapsDefault);
                var compress = cmdElement.Attribute("useCompression").ValueOr<bool>(compressDefault);
                var checksum = cmdElement.Attribute("checksum").ValueNullSafe();
                var isCritical = cmdElement.Attribute("isCritical").ValueOr<bool>(criticalCmdsDefault);
                var priority = cmdElement.Attribute("priority").ValueOr<int>(3);

                var newCmd = new AddImageCmd();
                newCmd.SourcePath = new InstallEntityPath(sourcePath);
                newCmd.TargetPath = new InstallEntityPath(targetPath);
                newCmd.NestedTargetPath = new ContentPath(contentPath);
                newCmd.OverwriteIfExist = overwrite;
                newCmd.UseMipMaps = useMipMaps;
                newCmd.UseCompression = compress;
                newCmd.Checksum = checksum;
                newCmd.IsCritical = isCritical;
                newCmd.Priority = priority;

                result.Add(newCmd);
            }

            return result;
        }

        #region Nested Class GroupProductionRule

        protected class GroupProductionRule
        {
            public GroupProductionRule(int priority,
                Func<IEnumerable<IInstallCmd>, IEnumerable<ICmdGroup>> productionRule)
            {
                this.Priority = priority;
                this.ProduceGroup = productionRule;
            }

            public int Priority 
            { 
                get; 
                private set;
            }

            public Func<IEnumerable<IInstallCmd>, IEnumerable<ICmdGroup>> ProduceGroup
            { 
                get; 
                private set; 
            }
        }

        #endregion //Nested Class GroupProductionRule

    }

}
