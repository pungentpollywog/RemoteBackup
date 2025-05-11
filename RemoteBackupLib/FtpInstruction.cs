using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NLog;

namespace RemoteBackupLib {
    public class FtpInstruction {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private List<FtpItem> ftpItems = null;
        private bool isUpload;
        private Chilkat.Ftp2 ftp = null;
        private List<string> localSpecialFolders = null;
        private bool useFtps = false;

        public bool UseFtps {
            get { return useFtps; }
            set { useFtps = value; }
        }

        public List<string> LocalSpecialFolders {
            get { return localSpecialFolders; }
            set { localSpecialFolders = value; }
        }

        public Chilkat.Ftp2 Ftp {
            get { return ftp; }
            set { ftp = value; }
        }
        private string hostname = null;

        public string Hostname {
            get { return hostname; }
            set { hostname = value; }
        }
        private string port = null;

        public string Port {
            get { return port; }
            set { port = value; }
        }

        private string username = null;

        public string Username {
            get { return username; }
            set { username = value; }
        }
        private string password = null;

        public string Password {
            get { return password; }
            set { password = value; }
        }

        public bool IsUpload {
            get { return isUpload; }
            set { isUpload = value; }
        }

        internal List<FtpItem> FtpItems {
            get { return ftpItems; }
            set { ftpItems = value; }
        }

        private void initializeFtpItemsList() {
            if (FtpItems == null) {
                FtpItems = new List<FtpItem>();
            } else {
                FtpItems.Clear();
            }
        }

        public void prepareFtpInstructionsFromRemote(ListView.SelectedListViewItemCollection selectedItems) {
            initializeFtpItemsList();
            foreach (ListViewItem item in selectedItems) {
                if (item.Tag == null) {
                    logger.Debug("item.Tag is null");
                } else if (item.Tag is FtpItem) {
                    logger.Debug("item.Tag is an FtpItem");
                } else {
                    logger.Debug("item.Tag is not an FtpItem");
                }

                if (item != null && item.Tag is FtpItem) {
                    FtpItem ftpItem = (FtpItem)item.Tag;
                    logger.Debug("(Remote) File FullName: " + ftpItem.FullName);
                    this.FtpItems.Add(ftpItem);
                } else {
                    logger.Trace("not adding item.");

                }
            }
        }

        public void prepareFtpInstructions(ListView.SelectedListViewItemCollection selectedItems) {
            initializeFtpItemsList();
            foreach (ListViewItem item in selectedItems) {
                FtpItem ftpItem = null;
                if (item.Tag is DirectoryInfo) {
                    DirectoryInfo folder = (DirectoryInfo)item.Tag;
                    ftpItem = new FtpItem(folder.Name, 1, folder.LastAccessTime.ToLocalTime(), true);
                    ftpItem.FullName = folder.FullName;
                } else if (item.Tag is FileInfo) {
                    FileInfo file = (FileInfo)item.Tag;
                    ftpItem = new FtpItem(file.Name, file.Length, file.LastAccessTime.ToLocalTime(), false);
                    ftpItem.FullName = file.FullName;
                    logger.Debug("(Local) File FullName: " + ftpItem.FullName);
                }
                this.FtpItems.Add(ftpItem);
            }
        }

        public void prepareFtpInstructions(FtpItem ftpItem) {
            initializeFtpItemsList();
            this.FtpItems.Add(ftpItem);
        }


        public void prepareFtpInstructionsFromSpecialFolders() {
            initializeFtpItemsList();
            foreach (string path in this.LocalSpecialFolders) {
                FtpItem ftpItem = new FtpItem(path, 1, new DateTime(), true);
                ftpItem.FullName = path;
                this.FtpItems.Add(ftpItem);
            }
        }

    }
}
