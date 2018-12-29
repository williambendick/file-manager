using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using file_manager.Models;

namespace file_manager.Controllers
{
    public class DefaultController : Controller
    {
        public ActionResult Index()
        {
            return File("~/index.html", "text/html");
        }
    }

    public class APIController : Controller
    {
        public ActionResult GetDirectoryItems(string apiPath, string query, string directoriesOnly)
        {
            try
            {
                string srchPattern = query == null ? "*" : "*" + query + "*";

                SearchOption srchOption = query == null ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

                if (apiPath == null) { apiPath = Settings.ApiRoot; }

                DirectoryInfo drctryInfo = new DirectoryInfo(Path.Combine(Settings.ServerPath, apiPath));

                if (!drctryInfo.Exists) { return new HttpStatusCodeResult(404, "The selected directory was not found"); }

                DirectoryItem rootDrctry = new DirectoryItem(drctryInfo.Name, "directory", apiPath, 0, true);

                if (query == null)
                {
                    foreach (DirectoryInfo drctry in drctryInfo.GetDirectories())
                    {
                        rootDrctry.FolderCount++;
                        rootDrctry.Items.Add(new DirectoryItem(drctry.Name, "directory", apiPath));
                    }
                }

                if (directoriesOnly == null)
                {
                    foreach (FileInfo file in drctryInfo.GetFiles(srchPattern, srchOption))
                    {
                        rootDrctry.FileCount++;
                        rootDrctry.ByteCount += file.Length;
                        rootDrctry.Items.Add(new DirectoryItem(file.Name, "file",
                            file.DirectoryName.Remove(0, Settings.ServerPath.Length).TrimStart('\\').Replace("\\", "/"),
                            file.Length, false, query != null));
                    }
                }

                rootDrctry.SetSize();
                return Json(rootDrctry, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                string message = e.Message.Contains("Access to the path") ? "Access to this path is denied." : e.Message;
                return new HttpStatusCodeResult(400, message);
            }
        }

        public ActionResult MoveItems(ProcessItems processItems)
        {
            var result = ProcessItems.Process(processItems, "move");
            return new HttpStatusCodeResult(result.Item1, result.Item2);
        }

        public ActionResult CopyItems(ProcessItems processItems)
        {
            var result = ProcessItems.Process(processItems, "copy");
            return new HttpStatusCodeResult(result.Item1, result.Item2);
        }

        public ActionResult DeleteItems(ProcessItems processItems)
        {
            var result = ProcessItems.Process(processItems, "delete");
            return new HttpStatusCodeResult(result.Item1, result.Item2);
        }
    }

    public class DownloadController : Controller
    {
        public FileResult DownloadFile(string apiPath, string file)
        {
            return File(Path.Combine(Settings.ServerPath, apiPath, file), System.Net.Mime.MediaTypeNames.Application.Octet, file);
        }
    }

    public class UploadController : Controller
    {
        public ActionResult UploadFiles(string apiPath)
        {
            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    HttpPostedFileBase file = Request.Files[i];
                    string fileName = Path.GetFileName(file.FileName);
                    Stream fileContent = file.InputStream;

                    file.SaveAs(Path.Combine(Settings.ServerPath, apiPath, fileName));
                }

                return Content("Uploaded " + Request.Files.Count + " file" + (Request.Files.Count == 1 ? "" : "s"));
            }
            catch (Exception e)
            {
                string message = e.Message.Contains("Access to the path") ? "Access to this path is denied." : e.Message;
                return new HttpStatusCodeResult(400, message);
            }
        }
    }

}
