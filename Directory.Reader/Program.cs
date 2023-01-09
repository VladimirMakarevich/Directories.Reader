using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Directories.Reader
{
    public class ContainerModel
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public List<ContainerModel> SubContainers { get; set; }
        public List<FileModel> Files { get; set; }
    }

    public class FileModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
    }

    public class Program
    {
        public static List<ContainerModel> Directories = new List<ContainerModel>();

        public static void Main(string[] args)
        {
            var collections = args.ToList();

            //collections.Add("d:\\");
            //collections.Add("e:\\");
            //collections.Add("f:\\");
            //collections.Add("g:\\");
            //collections.Add("h:\\");
            //collections.Add("j:\\");

            collections.Add("C:\\$VladimirMakarevich");

            var datetime = DateTime.Now.ToString("yyyy-dd-M-HH-mm", CultureInfo.InvariantCulture);

            foreach (string path in collections)
            {
                if (Directory.Exists(path))
                {
                    // This path is a container
                    var container = new ContainerModel();
                    container.Path = path;
                    container.Name = GetNameFromPath(path);

                    ProcessDirectory(path, container);

                    Directories.Add(container);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or container.", path);
                }
            }

            File.WriteAllText($"$errors-{datetime}.md", _sbErrors.ToString());
            File.WriteAllText($"json-notes-{datetime}.md", JsonConvert.SerializeObject(Directories, Formatting.Indented));

            ProcessMdFormat(datetime);
        }

        private static void ProcessMdFormat(string datetime)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("---");
            sb.AppendLine($"title: {Environment.UserDomainName.ToLowerInvariant()}-explorer");
            sb.AppendLine($"date created: {DateTime.Now.ToString("yyyy-M-dd HH:mm", CultureInfo.InvariantCulture)}");
            sb.AppendLine($"date updated: {DateTime.Now.ToString("yyyy-M-dd HH:mm", CultureInfo.InvariantCulture)}");
            sb.AppendLine("---");

            sb.AppendLine("");
            sb.AppendLine("# System Info:");
            sb.AppendLine("");
            sb.AppendLine($"> UserDomainName: {Environment.UserDomainName}.");
            sb.AppendLine($"> UserName: {Environment.UserName}.");
            sb.AppendLine($"> Version: {Environment.Version}.");
            sb.AppendLine("");

            foreach (var container in Directories)
            {
                var nestingCount = 0;

                sb.AppendLine($"### =={container.Name}==");
                sb.AppendLine("");
                sb.AppendLine($"> {container.Path}");
                sb.AppendLine("");

                if (container.Files != null && container.Files.Any())
                {
                    ProcessFilesMdFormat(container.Files, sb, nestingCount);
                }

                if (container.SubContainers != null && container.SubContainers.Any())
                {
                    ProcessSubContainersMdFormat(container.SubContainers, sb, nestingCount);
                }

                sb.AppendLine("");
                sb.AppendLine("---");
                sb.AppendLine("");
            }
            
            sb.AppendLine("`**keywords:**`");
            sb.AppendLine("");
            sb.AppendLine("#notes");
            sb.AppendLine("#explorer");
            sb.AppendLine($"#{Environment.UserDomainName}");
            sb.AppendLine("");

            File.WriteAllText($"notes-{Environment.UserDomainName.ToLowerInvariant()}-{datetime}.md", sb.ToString());
        }

        private static void ProcessSubContainersMdFormat(List<ContainerModel> subContainers, StringBuilder sb, int nestingCount)
        {
            foreach (var container in subContainers)
            {
                sb.AppendLine($"{GetNestingWhitespaces(nestingCount)}- !{container.Name}");
                if (container.SubContainers != null && container.SubContainers.Any())
                {
                    if (container.Files != null && container.Files.Any())
                    {
                        ProcessFilesMdFormat(container.Files, sb, nestingCount + 1);
                    }

                    if (container.SubContainers != null && container.SubContainers.Any())
                    {
                        ProcessSubContainersMdFormat(container.SubContainers, sb, nestingCount + 1);
                    }
                }
            }
        }

        private static void ProcessFilesMdFormat(List<FileModel> containerFiles, StringBuilder sb, int nestingCount)
        {
            foreach (var containerFile in containerFiles)
            {
                sb.AppendLine($"{GetNestingWhitespaces(nestingCount)}- `{containerFile.Name}`");
            }
        }

        public static string GetNestingWhitespaces(int nestingCount)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < nestingCount; i++)
            {
                sb.Append("\t");
            }

            return sb.ToString();
        }

        // Process all files in the container passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory, ContainerModel container)
        {
            if (IgnoreList.Contains(GetNameFromPath(targetDirectory)))
            {
                return;
            }

            // Process the list of files found in the container.
            try
            {
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                {
                    ProcessFile(fileName, container);
                }
            }
            catch (Exception e)
            {
                _sbErrors.AppendLine($"`{e.Message}`");
            }

            // Recurse into subdirectories of this container.
            try
            {
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    var subContainer = new ContainerModel()
                    {
                        Name = GetNameFromPath(subdirectory)
                    };

                    container.SubContainers ??= new List<ContainerModel>();
                    container.SubContainers.Add(subContainer);

                    ProcessDirectory(subdirectory, subContainer);
                }
            }
            catch (Exception e)
            {
                _sbErrors.AppendLine($"`{e.Message}`");
            }
        }

        private static string GetNameFromPath(string targetDirectory)
        {
            return targetDirectory.Split("\\").Last();
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path, ContainerModel container)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!Extensions.Contains(fileInfo.Extension))
            {
                return;
            }

            Console.WriteLine("Processed file '{0}'.", path);

            container.Files ??= new List<FileModel>();
            container.Files.Add(new FileModel()
            {
                Name = fileInfo.Name,
                Extension = fileInfo.Extension
            });
        }

        private static StringBuilder _sbErrors = new StringBuilder();

        private static readonly List<string> IgnoreList = new List<string>()
        {
            "$RECYCLE.BIN",
            "System Volume Information",
            "found.000",
            ".vs",
            "bin",
            "obj",
            "__MACOSX",
            ".git",
            ".idea",
            "node_modules",
            "__1"
        };

        private static readonly List<string> Extensions = new List<string>()
        {
            ".docx",
            ".doc",
            ".xlsx",
            ".xls",
            ".csv",
            ".pdf",
            ".zip",
            ".rar",
            ".iso",
            ".exe",
            ".mp4",
            ".epub",
            ".fb2",
            ".m4v"
        };
    }
}