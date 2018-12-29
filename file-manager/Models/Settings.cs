using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;

namespace file_manager.Models    
{
    public static class Settings
    {
        //////////////////////////////////////////////////////////////////////////
        // Home directory can be set here using a full path or a folder in application root        
        // full path: ServerPath = @"C:\FolderName1\FolderName2";  
        // application root: ServerPath = @"FolderName1";
        // the directory is created if it does not already exist
        //////////////////////////////////////////////////////////////////////////

        public static readonly string ServerPath = @"C:\Users\main";

        public static readonly string ApiRoot;

        static Settings()
        {
            if (ServerPath.IndexOfAny(":\\".ToCharArray()) == -1) { ServerPath = Path.Combine(HttpContext.Current.Server.MapPath("~"), ServerPath); }

            if (!Directory.Exists(ServerPath)) { Directory.CreateDirectory(ServerPath); }

            ApiRoot = new DirectoryInfo(ServerPath).Name;

            ServerPath = Directory.GetParent(ServerPath.TrimEnd('\\')).FullName;
        }
    }
}