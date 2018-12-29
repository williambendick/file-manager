using System;
using System.Collections.Generic;

namespace file_manager.Models
{
    public class DirectoryItem
    {        
        public string Name { get; set; }
        public string Type { get; set; }
        public string ItemPath { get; set; }
        public int? FileCount { get; set; }
        public int? FolderCount { get; set; }
        public long? ByteCount { get; set; }
        public string Size { get; set; }
        public bool IsSearch { get; set; }
        public List<DirectoryItem> Items { get; set; }

        public DirectoryItem() { }

        public DirectoryItem(string name, string type, string itemPath = null, long ?byteCount = null, bool isRoot = false, bool isSearch = false)
        {
            Name = name;
            Type = type;
            ItemPath = itemPath;
            ByteCount = byteCount;
            IsSearch = isSearch;
            FolderCount = isRoot ? 0 : (int?)null;
            FileCount = isRoot ? 0 : (int?)null;
            if (isRoot) { Items = new List<DirectoryItem>(); }
            if (byteCount != null){ SetSize(); }            
        }

        public void SetSize()
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB" };

            int i = 0;
            decimal val = (decimal)ByteCount;
            while (Math.Round(val) >= 1000)
            {
                val /= 1024;
                i++;
            }

            Size = string.Format("{0:N0} {1}", val, suffixes[i]);
        }
    }
}
