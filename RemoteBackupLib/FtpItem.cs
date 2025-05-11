using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteBackupLib {
    public class FtpItem {

        private FtpItem parentItem=null;

        private List<FtpItem> children = null;

        private System.Windows.Forms.TreeNode treeNode = null;

        public System.Windows.Forms.TreeNode TreeNode {
            get { return treeNode; }
            set { treeNode = value; }
        }

        private string localTargetPath = null;

        public string LocalTargetPath {
            get { return localTargetPath; }
            set { localTargetPath = value; }
        }

        public List<FtpItem> Children {
            get { return children; }
            set { children = value; }
        }

        public FtpItem ParentItem {
            get { return parentItem; }
            set { parentItem = value; }
        }

        private bool isDirectory = false;

        public bool IsDirectory {
            get { return isDirectory; }
            set { isDirectory = value; }
        }
        private string name = null;

        public string LowerName {
            get { return Name.ToLower(); }
        }

        public string Name {
            get { return name; }
            set { name = value; }
        }

        private string fullName = null;

        public string FullName {
            get { return fullName; }
            set { fullName = value; }
        }

        private long size = 0;

        public long Size {
            get { return size; }
            set { size = value; }
        }
        private DateTime modDate;

        public DateTime ModDate {
            get { return modDate; }
            set { modDate = value; }
        }

        public FtpItem(string name, long size, DateTime lastModified, bool isDirectory) {
            this.Name = name;
            this.IsDirectory = isDirectory;
            this.ModDate = lastModified;
        }

        public override string ToString() {
            return this.Name + ", " + this.Size + ", " + this.ModDate.ToShortDateString() + ", isDir: " + IsDirectory.ToString();
        }
    }
}
