using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace file_manager.Models
{
    public class ProcessItems
    {
        public string Destination { get; set; }
        public DirectoryItem[] Sources { get; set; }

        public static Tuple<int, string> Process(ProcessItems processItems, string processType)
        {
            string sourcePath, desinationPath, exceptionMessage = "";
            string processTypeText = processType == "move" ? "moved" : processType == "copy" ? "copied" : "deleted";
            int errorCount = 0;

            foreach (DirectoryItem item in processItems.Sources)
            {
                try
                {
                    sourcePath = Path.Combine(Settings.ServerPath, item.ItemPath.Replace("/", "\\"), item.Name);
                    desinationPath = processType == "delete" ? "" : Path.Combine(Settings.ServerPath, processItems.Destination, item.Name);

                    if (item.Type == "directory")
                    {
                        switch (processType)
                        {
                            case "move":
                                Directory.Move(sourcePath, desinationPath); break;
                            case "copy":
                                if (Directory.Exists(desinationPath) ||
                                    sourcePath == Path.Combine(Settings.ServerPath, processItems.Destination))
                                {
                                    errorCount++;
                                    exceptionMessage = "A directory with the same name as the copied directory already exists.";
                                }
                                else
                                {
                                    DirectoryInfo diSource = new DirectoryInfo(sourcePath);
                                    DirectoryInfo diTarget = new DirectoryInfo(desinationPath);
                                    exceptionMessage = CopyDirectories(diSource, diTarget);
                                    if (exceptionMessage != "") { errorCount++; }
                                }
                                break;
                            case "delete":
                                Directory.Delete(sourcePath, true); break;
                        }
                    }
                    else
                    {
                        switch (processType)
                        {
                            case "move":
                                System.IO.File.Move(sourcePath, desinationPath); break;
                            case "copy":
                                System.IO.File.Copy(sourcePath, desinationPath, false); break;
                            case "delete":
                                System.IO.File.Delete(sourcePath); break;
                        }
                    }
                }
                catch (Exception e)
                {
                    errorCount++;
                    exceptionMessage = e.Message.Contains("Access to the path") ? "Access to this path is denied." : e.Message.Replace("\r\n", string.Empty);
                }
            }

            if (errorCount == 0)
            {
                return Tuple.Create(200, $"{processItems.Sources.Count()} items were {processTypeText} successfully");
            }
            else
            {
                return Tuple.Create(400, $"{processItems.Sources.Count() - errorCount} items were {processTypeText} successfully, " +
                    $"{errorCount} items were unsuccessful, last error: {exceptionMessage}");
            }
        }

        public static string CopyDirectories(DirectoryInfo source, DirectoryInfo destination)
        {
            string error = "";

            try
            {
                Directory.CreateDirectory(destination.FullName);

                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(destination.ToString(), fi.Name), false);
                }

                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextDestinationSubDir = destination.CreateSubdirectory(diSourceSubDir.Name);
                    CopyDirectories(diSourceSubDir, nextDestinationSubDir);
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            return error;
        }

    }
}