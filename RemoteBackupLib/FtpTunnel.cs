using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;

namespace RemoteBackupLib {
    public class FtpTunnel {

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private FtpInstruction ftpInstruct = null;

        public FtpTunnel(FtpInstruction ftpInstruct) {
            this.ftpInstruct = ftpInstruct;
        } // ctor

        /** Meant to be called by a background thread.
         * Synchronizes selected folders on the remote FTP server with matching local folders.
         */
        public bool restoreFilesFromFTPServer() {
            logger.Trace("restoreFilesFromFTPServer starting...");
            bool success = true;
            try {
                if (ftpInstruct.Ftp.IsConnected == false) {
                    connectToFtpServer(ftpInstruct);
                }
                logger.Debug("Downloading...");
                if (ftpInstruct.FtpItems != null) {
                    logger.Debug("FtpItems item count: " + ftpInstruct.FtpItems.Count);
                    foreach (FtpItem ftpItem in ftpInstruct.FtpItems) {
                        if (ftpItem.IsDirectory) { // Synchronize entire remote directory to local file system.
                            logger.Trace("Restore remote folder.");
                            string localTargetFolder = findLocalPathFromRemote(ftpItem);
                            if (localTargetFolder != null && localTargetFolder.Length > 0) {
                                string remoteTargetFolder = absolutePath(ftpItem);
                                if (remoteTargetFolder != null && remoteTargetFolder.Length > 0) {
                                    logger.Debug("Switching to remote folder: " + remoteTargetFolder);
                                    success = ftpInstruct.Ftp.ChangeRemoteDir(remoteTargetFolder);
                                    if (success) {
                                        // mode=0: Download all files
                                        // mode=1: Download all files that do not exist on the local filesystem.
                                        // mode=2: Download newer or non-existant files.
                                        // mode=3: Download only newer files. If a file does not already exist on 
                                        //              the local filesystem, it is not downloaded from the server.
                                        int mode = 2;
                                        logger.Debug("Synchronizing " + localTargetFolder + " from remote FTP server.");
                                        success = ftpInstruct.Ftp.SyncLocalTree(localTargetFolder, mode);
                                        logger.Debug("Success: " + success.ToString());
                                    } else {
                                        logger.Error("Could not switch to remote directory: "+remoteTargetFolder);
                                    }
                                }
                            }
                            logger.Debug("Synchronized restore complete.");
                        } else { // Restore single file from remote FTP server.
                            success = restoreSingleFile(ftpItem);
                        }
                    }
                } else {
                    logger.Debug("FtpItems list is empty.  Nothing to restore.");

                }
            } catch (Exception ex) {
                logger.Error("Exception in restoreFilesFromFTPServer().");
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
            }
            return success;
        }

        private bool restoreSingleFile(FtpItem ftpItem) {
            bool success = false;
            logger.Trace("Restore remote file: " + ftpItem.Name);

            string remoteTargetFolder = absolutePath(ftpItem);
            if (remoteTargetFolder != null && remoteTargetFolder.Length > 0) {
                remoteTargetFolder = getPathWithOutLastSegment(remoteTargetFolder, '/');
                logger.Debug("Switching to remote folder: " + remoteTargetFolder);
                success = ftpInstruct.Ftp.ChangeRemoteDir(remoteTargetFolder);
                string localPath = findLocalPathFromRemote(ftpItem);
                logger.Debug("Found localPath: " + localPath);
                // Before restore, only copy if the local file does not exist.
                if (restoreNeeded(localPath)) {
                    string containingFolder = getPathWithOutLastSegment(localPath, '/');
                    createLocalPathIfNonExistent(containingFolder);
                    success = ftpInstruct.Ftp.GetFile(ftpItem.Name, localPath);
                }
            }
            return success;
        }

        private bool restoreNeeded(String localPath) {
            bool doRestore = false;
            FileInfo file = new FileInfo(localPath);
            if (file.Exists == false) {
                doRestore = true;
                logger.Debug("File doesn't exist.  Restore it.");
            } else {
                logger.Debug("File already exists.  Skipping restore.");
            }
            return doRestore;
        }

        public bool backupFilesToFTPServer() {
            logger.Trace("backupFilesToFTPServer starting ...");
            bool success = true;
            try {
                logger.Debug("Checking Ftp connection.");
                if (ftpInstruct.Ftp.IsConnected == false) {
                    logger.Debug("Not connected.  Attempting to connect.");
                    connectToFtpServer(ftpInstruct);
                }
                // Note: if PutFile fails and you get this in your LastErrorText:
                // PortReply: 530 Only client IP address allowed for PORT command.
                // It indicates that you must first clear the control channel
                // because your FTP client sits behind a router doing NAT (network 
                // address translation).  The router is not able to translate the 
                // IP address in the PORT command if the control channel is encrypted.
                // success = ftp.PutFile(localFilename, remoteFilename);

                logger.Debug("Uploading...");

                if (ftpInstruct.FtpItems != null) {
                    foreach (FtpItem ftpItem in ftpInstruct.FtpItems) {
                        if (ftpItem.IsDirectory) {
                            logger.Debug("Synchronizing " + ftpItem.Name + " to FTP server.");
                            // success = backupFolder(ftpItem);
                            success = synchronizeLocalFolderToRemote(ftpItem);
                        } else {
                            // CDD-TODO: check the dates on the files.  Only upload if newer.
                            logger.Debug("[backupFilesToFTPServer] Backing up: " + ftpItem.FullName);
                            backupLocalFile(ftpItem);
                        }
                    }
                }

                // You may examine the LastErrorText even when successful
                // to review the details of the file transfer.  You can verify
                // that the file was transferred over a secure channel:
                // MessageBox.Show(ftp.LastErrorText);

                if (success == false) {
                    logger.Error(ftpInstruct.Ftp.LastErrorText);
                    logger.Error(ftpInstruct.Ftp.SessionLog);
                }
            } catch (Exception ex) {
                logger.Error("Exception in backupFilesToFTPServer().");
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
            }
            return success;
        }

        /** Recursively strips off all '/' characters from the beginning of the
         * passed in string parameter.
         */
        private string removeLeadingSlashes(string path) {
            if (path.StartsWith("/")) {
                return removeLeadingSlashes(path.Substring(1));
            } else
                return path;
        }

        private void switchToRemoteRootFolder() {
            ftpInstruct.Ftp.ChangeRemoteDir("/");
        }


        /** Used to create an 'absolute' path consisting of the current, passed in folder
         * and all of its parent folders.
         */
        private string absolutePath(FtpItem item) {
            if (item != null) {
                if (item.ParentItem == null) { // root folder.  name is /
                    return item.Name;
                } else {
                    string path = absolutePath(item.ParentItem) + "/" + item.Name;
                    if (path.StartsWith("//")) {
                        path = path.Substring(1);
                    }
                    return path;
                }
            } else {
                return "";
            }
        }

        /** C:\Documents and Settings\Administrator\Desktop\
         * 
         *  Resolve the partial absolute path.
         *  Look for that path under the special folders.  
         *  If found, set local path to that folder.
         * 
         */
        private string findLocalPathFromRemote(FtpItem folder) {
            logger.Trace("findOrCreateLocalPathFromRemote starting ...");
            string targetPath = null;
            string remotePath = absolutePath(folder);

            remotePath = this.removeLeadingSlashes(remotePath);
            string firstSegRemotePath = firstPathSegment(remotePath, '/');

            // CDD-TODO: 
            // 1. Get first segment of the remote absolute path.
            // 2. Get last segment of each of the local speical folders.
            // 3. #1 should match up with one of the segments from #2.
            // 4. That is how to know where to put the files being downloaded.

            logger.Debug("absolutePath: " + absolutePath(folder));
            logger.Debug("Special folders: ");
            foreach (string path in ftpInstruct.LocalSpecialFolders) {
                logger.Debug(path);
                if (lastPathSegment(path, '\\').Equals(firstSegRemotePath)) {
                    logger.Debug("Matched path: "+path);
                    targetPath = appendRemoteToLocalPath(path, remotePath);
                    break;
                }
            }
            // CDD-TODO: if local path doesn't exist, need to create it or pull down entire absolute
            // path (or at least the part of the path that is missing.
            return targetPath;
        }

        private void createLocalPathIfNonExistent(string targetPath) {
            logger.Trace("createLocalPathIfNonExistent starting ...");
            logger.Debug("Path to check: " + targetPath);
            DirectoryInfo dirInfo = new DirectoryInfo(targetPath);
            if (dirInfo.Exists == false) {
                logger.Trace("Path does not exist yet.  Need to create.");
                System.IO.Directory.CreateDirectory(targetPath);
            }
        }

        private string firstPathSegment(string path, char splitChar) {
            string firstSeg = null;
            if (path != null) {
                string[] parts = path.Split(splitChar);
                if (parts != null && parts.Length > 0) {
                    firstSeg=parts[0];
                }
            }
            return firstSeg;
        }

        public string lastPathSegment(string path, char splitChar) {
            string lastSeg = null;
            if (path!=null) {
                string[] parts = path.Split(splitChar);
                if (parts!=null && parts.Length>0) {
                    lastSeg = parts[parts.Length-1];
                    if (lastSeg == null || lastSeg.Trim().Length == 0) {
                        if (parts.Length > 1) { // In case a custom folder has a trailing slash. C:\Temp\
                            // parts would be "C:", "Temp", and "".  So we want index 1 not 2.  (Length is 3.)
                            lastSeg = parts[parts.Length-2];
                        }
                    }
                }
            }
            return lastSeg;
        }

        private string getpathAfterFirstSeg(string path) {
            string newPath = "";
            if (path != null) {
                string firstSeg = firstPathSegment(path, '/');
                if (firstSeg.Length < path.Length) {
                    newPath = path.Substring(firstSeg.Length);
                }
            }
            return removeLeadingSlashes(newPath); 
        }

        public string getPathWithOutLastSegment(string path, char splitChar) {
            string lastSeg = lastPathSegment(path, splitChar);
            int newLength = path.Length - lastSeg.Length;
            return path.Substring(0, newLength);
        }

        /** Append the portion of the remote folder after the first segment
         * to the local special folder.
         */
        private string appendRemoteToLocalPath(string local, string remote) {
            logger.Trace("appendRemoteToLocalPath starting...");
            string newPath = Path.Combine(local, getpathAfterFirstSeg(remote));
            logger.Debug("Target path for restore: "+newPath);
            return newPath;
        }

        /** If path is one of the special folders, just use the last segment.
         *  If the path is not a special folder, use the part after the special folder section.
         */
        private void findOrCreateRemotePathFromLocal(FtpItem folder) {
            try {
                bool foundMatch = false;
                logger.Trace("findOrCreateRemotePathFromLocal starting...");
                string path = folder.FullName;
                logger.Debug("path: " + path);
                // Determine what the remote path should look like
                foreach (string localSpecialFolder in ftpInstruct.LocalSpecialFolders) {
                    if (path.Equals(localSpecialFolder)) {
                        // Use the lastSegment of the specialFolder
                        path = lastPathSegment(path, '\\');
                        foundMatch = true;
                        break;
                    } else if (path.StartsWith(localSpecialFolder) && path.Length > localSpecialFolder.Length) {
                        String lastSeg = lastPathSegment(localSpecialFolder, '\\');
                        // Use the part after the special folder
                        path = path.Substring(localSpecialFolder.Length - lastSeg.Length);
                        foundMatch = true;
                        break;
                    }
                }
                // Switch to or create the remote path
                if (foundMatch == true) {
                    logger.Debug("New path: " + path);
                    String[] folders = path.Split('\\');
                    foreach (string target in folders) {
                        if (target.Trim().Length > 0) {
                            logger.Debug("looking for " + target);
                            if (ftpInstruct.Ftp.ChangeRemoteDir(target) == false) {
                                logger.Debug(target + " not found. Creating " + target);
                                bool success = ftpInstruct.Ftp.CreateRemoteDir(target);
                                if (success) {
                                    success = ftpInstruct.Ftp.ChangeRemoteDir(target);
                                }
                            } else {
                                logger.Debug("Found " + target);
                            }
                        }
                    }
                } else {
                    logger.Debug("Unable to create remote path from local path.");
                    logger.Debug("Local path is: " + folder.FullName);
                }
            } catch (Exception ex) {
                logger.Error("Exception: "+ex.Message+" "+ex.StackTrace);
            }
        }

        private FtpItem getParentFtpItemFromFullPath(string fullPath) {
            FtpItem parentFolder = null;
            try {
                logger.Debug("[getParentFtpItemFromFullPath] fullPath: " + fullPath);
                string folderName = getPathWithOutLastSegment(fullPath, '\\');
                parentFolder = new FtpItem(folderName, 1, (new DateTime()).ToLocalTime(), true);
                parentFolder.FullName = folderName;
            } catch (Exception ex) {
                logger.Error("[getParentFtpItemFromFullPath]"+ex.Message+" "+ex.StackTrace);
            }
            return parentFolder;
        }

        private bool backupLocalFile(FtpItem file) {
            switchToRemoteRootFolder();
            if (file == null) {
                logger.Debug("file is null");
            } else if (file.ParentItem == null) {
                logger.Debug("file.ParentItem is null.  Resolving parent folder from FullName.");
                file.ParentItem = getParentFtpItemFromFullPath(file.FullName);
            }
            bool success=false;
            if (file.ParentItem!=null) {
                findOrCreateRemotePathFromLocal(file.ParentItem);
                logger.Debug("Backing up: " + file.FullName);
                success = ftpInstruct.Ftp.PutFile(file.FullName, file.Name);
            }
            return success;
        }


        /** Recursively copies the contents of the local folder (passed in parameter) to the 
         * same folder on the remote ftp server using the selected mode.  
         * 
         * Modes for SyncRemoteTree(folder, mode)
         * mode=0: Upload all files
         * mode=1: Upload all files that do not exist on the FTP server.
         * mode=2: Upload newer or non-existant files.
         * mode=3: Upload only newer files. If a file does not already exist on the FTP server, it is not uploaded.
         * mode=4: transfer missing files or files with size differences.
         * mode=5: same as mode 4, but also newer files.
         */
        private bool synchronizeLocalFolderToRemote(FtpItem folder) {
            switchToRemoteRootFolder();
            findOrCreateRemotePathFromLocal(folder);
            logger.Debug("Synchronizing local folder to remote folder.");
            logger.Debug(folder.FullName);
            bool success = true;
            if (success) {
                success = this.ftpInstruct.Ftp.SyncRemoteTree(folder.FullName, 2); // mode = 2
            }
            if (success) {
                success = ftpInstruct.Ftp.ChangeRemoteDir("../");
            }
            return success;
        }

        private bool backupFolder(FtpItem folder) {
            logger.Debug("Backing up folder, FullName: " + folder.Name);
            bool success = ftpInstruct.Ftp.CreateRemoteDir(folder.Name);
            if (success) {
                success = ftpInstruct.Ftp.ChangeRemoteDir(folder.Name);
            }
            if (success) {
                success = ftpInstruct.Ftp.PutTree(folder.FullName);
            }
            if (success) {
                success = ftpInstruct.Ftp.ChangeRemoteDir("../");
            }
            return success;
        }

        /** The CreatePlan method recursively descends the local filesystem directory tree and creates a "plan" of all the files 
         *  and directories that will be uploaded by PutPlan. The PutPlan method accepts a "plan", and keeps a log file of its 
         *  progress as it proceeds. If PutPlan fails, it can be called again with the same log file to resume the upload.
         *  If PutPlan fails, two additional attempts will be made to save the remaining files. 
         * 
         */
        private bool backupFolderWithPlan(FtpItem folder) {
            bool success = false;
            string plan = ftpInstruct.Ftp.CreatePlan(folder.FullName);
            int attempt = 0;
            do {
                success = ftpInstruct.Ftp.PutPlan(plan, "planLog.txt");
            } while (success == false && ++attempt <= 3);
            return success;
        }


        public void connectToFtpServer(FtpInstruction ftpInstruct) {
            logger.Trace("connectToFtpServer starting...");
            if (ftpInstruct.Ftp.IsConnected == true) {
                logger.Debug("Already connected.  Skipping connect.");
                return;
            }

            // The most common method of doing secure FTP file transfers is
            // by AUTH TLS (also known as AUTH SSL).
            // The client connects on the standard unencrypted FTP port 21
            // and then issues an "AUTH TLS" command to convert the TCP/IP channel
            // to a TLS encrypted channel.  All communications from that point onward,
            // including data transfers, are encrypted.

            // Set the AuthTls property = true if configured to use it. 
            // This can be set in the App Config file.  
            if (ftpInstruct.UseFtps == true) {
                ftpInstruct.Ftp.AuthTls = true;
            }

            // Leave the Ssl property false.  The Ssl property is used
            // for doing Secure FTP over SSL on port 990 (implicit SSL)
            // FTP Implicit SSL is covered in another example.
            //  ftp.Ssl = false;

            // Set the FTP hostname, login, and password.
            ftpInstruct.Ftp.Hostname = ftpInstruct.Hostname;
            int port = 21;
            try {
                int.TryParse(ftpInstruct.Port, out port);
            } catch (Exception ex) {
                logger.Error("Could not parse port value of " + ftpInstruct.Port);
                logger.Error("Defaulting to port 21.");
                logger.Error("Exception " + ex.Message + " " + ex.StackTrace);
            }
            ftpInstruct.Ftp.Port = port;
            ftpInstruct.Ftp.Username = ftpInstruct.Username;
            ftpInstruct.Ftp.Password = ftpInstruct.Password;

            // Session logging is not required, but I'm turning it on here to see
            // what transpires during the FTP session.  The session log provides the
            // unencrypted FTP requests and responses.
            ftpInstruct.Ftp.KeepSessionLog = true;

            // Connect and login to the FTP server.  This establishs a control connection
            // to the FTP server (port 21), converts the channel to TLS (because AuthTls is true),
            // and then authenticates.
            bool success = ftpInstruct.Ftp.Connect();
            if (success == false) {
                logger.Error(ftpInstruct.Ftp.SessionLog);
                logger.Error(ftpInstruct.Ftp.LastErrorText);
                return;
            }
            // You may need to clear the control channel if your FTP client
            // is located behind a network-address-translating router
            // such as with a Cable/DSL router.
            ftpInstruct.Ftp.ClearControlChannel();
            logger.Trace("connectToFtpServer exiting...");
        }

        /** Attempt to backup any files and folders selected.  For folders, the CreatePlan/PutPlan methods will 
         * be used to increase the chances of success.  
         */
        //private void backupFilesToFTPServerWithPlan(ListView.SelectedListViewItemCollection selectedItems) {
        //    try {
        //        if (ftp.IsConnected == false) {
        //            logIt("Connecting...");
        //            connectToFtpServer();
        //        }
        //        logIt("Uploading...");
        //        bool success = true;
        //        foreach (ListViewItem item in selectedItems) {
        //            logIt("\t" + item.Text);
        //            if (item.Tag is DirectoryInfo) {
        //                logIt("backing up folder: " + item.Text);
        //                DirectoryInfo folder = (DirectoryInfo)item.Tag;
        //                success = backupFolderWithPlan(folder);
        //            } else if (item.Tag is FileInfo) {
        //                logIt("backing up file: " + item.Text);
        //                FileInfo file = (FileInfo)item.Tag;
        //                logIt("FullName: " + file.FullName);
        //                if (success) {
        //                    success = ftp.PutFile(file.FullName, file.FullName);
        //                }
        //            } else {
        //                logIt("Unable to determine item as neither file nor folder.");
        //            }
        //            if (success == false) {
        //                logIt("!!!! An error occurred while backing up "+item.Text+ "!!!!");
        //                break;
        //            }
        //        }
        //    } catch (Exception ex) {
        //        logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
        //    }
        //}

        /** The CreatePlan method recursively descends the local filesystem directory tree and creates a "plan" of all the files 
         *  and directories that will be uploaded by PutPlan. The PutPlan method accepts a "plan", and keeps a log file of its 
         *  progress as it proceeds. If PutPlan fails, it can be called again with the same log file to resume the upload.
         *  If PutPlan fails, two additional attempts will be made to save the remaining files. 
         * 
         */
        //private bool backupFolderWithPlan(DirectoryInfo folder) {
        //    bool success = false;
        //    string plan = ftp.CreatePlan(folder.FullName);
        //    int attempt = 0;
        //    do {
        //        success = ftp.PutPlan(plan, "planLog.txt");
        //    } while (success==false && ++attempt<=3);
        //    return success;
        //}

        //private bool backupFolder(DirectoryInfo folder) {
        //    bool success = false;
        //    try {
        //        logIt("Backing up folder, FullName: " + folder.FullName);
        //        success = ftp.CreateRemoteDir(folder.Name);
        //        if (success) {
        //            success = ftp.ChangeRemoteDir(folder.Name);
        //        }
        //        if (success) {
        //            success = ftp.PutTree(folder.FullName);
        //        }
        //        if (success) {
        //            success = ftp.ChangeRemoteDir("../");
        //        }
        //    } catch (Exception ex) {
        //        logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
        //    }
        //    return success;
        //}



    }
}
