using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Directories.Reader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var collections = args.ToList();

            collections.Add("d:\\");
            collections.Add("e:\\");
            collections.Add("f:\\");
            collections.Add("g:\\");
            collections.Add("h:\\");
            collections.Add("j:\\");

            var datetime = DateTime.Now.ToString("yyyy-dd-M-HH-mm", CultureInfo.InvariantCulture);

            File.WriteAllText($"$collections-{datetime}.md", string.Join(", ", collections));

            foreach (string path in collections)
            {
                if (File.Exists(path))
                {
                    // This path is a file
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }

            File.WriteAllText($"$directories-{datetime}.md", _sbDirectories.ToString());
            File.WriteAllText($"$paths-{datetime}.md", _sbPaths.ToString());
            File.WriteAllText($"$errors-{datetime}.md", _sbErrors.ToString());
        }

        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            if (IgnoreList.Contains(targetDirectory.Split('\\').Last()))
            {
                return;
            }

            _sbDirectories.AppendLine($"`{targetDirectory}`");

            // Process the list of files found in the directory.
            try
            {
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                {
                    ProcessFile(fileName);
                }
            }
            catch (Exception e)
            {
                _sbErrors.AppendLine($"`{e.Message}`");
            }

            // Recurse into subdirectories of this directory.
            try
            {
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    ProcessDirectory(subdirectory);
                }
            }
            catch (Exception e)
            {
                _sbErrors.AppendLine($"`{e.Message}`");
            }
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!Extensions.Contains(fileInfo.Extension))
            {
                return;
            }

            Console.WriteLine("Processed file '{0}'.", path);
            _sbPaths.AppendLine($"`{path}`");
        }

        private static StringBuilder _sbDirectories = new StringBuilder();
        private static StringBuilder _sbPaths = new StringBuilder();
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
            ".mp4"
        };
    }
}