using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using NLog;
using RemoteBackupLib;


namespace RemoteBackup {
    public partial class RemoteBackupForm : Form {

        private Chilkat.Ftp2 ftp = null;
        private Timer timer = new Timer();
        private Timer progressTimer = new Timer();
        private bool m_bgRunning = false;
        private bool m_abort = false;
        private string m_ftpLastError = "";
        private string m_ftpSessionLog = "";
        private BackgroundWorker m_bgWorker = new BackgroundWorker();
        private FtpItem remoteStructure = null;
        private bool useFtps = false;
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool includeDesktop = true;
        private bool includeDocuments = true;
        private bool includeFavorites = true;
        private bool isFtpUpload = false;
        private Configuration config = null;
        private bool includeCustomFolders = true;

        public RemoteBackupForm() {
            try {
                InitializeComponent();
                logStartup();
                // Local File-system Explorer
                treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
                listView1.MouseDoubleClick += new MouseEventHandler(listView1_MouseDoubleClick);
                // listView1.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(listView1_ItemSelectionChanged);
                // Remote File-system Explorer
                treeViewRemote.NodeMouseClick += new TreeNodeMouseClickEventHandler(treeViewRemote_NodeMouseClick);
                listViewRemote.MouseDoubleClick += new MouseEventHandler(listViewRemote_MouseDoubleClick);
                // listViewRemote.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(listViewRemote_ItemSelectionChanged);
                // Background worker
                m_bgWorker.WorkerReportsProgress = true;
                m_bgWorker.WorkerSupportsCancellation = true;
                m_bgWorker.DoWork += new DoWorkEventHandler(m_bgWorker_DoWork);
                m_bgWorker.ProgressChanged += new ProgressChangedEventHandler(m_bgWorker_ProgressChanged);
                m_bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_bgWorker_RunWorkerCompleted);
                // Disable the server name and port fields
                serverNameTextBox.Enabled = false;
                portTextBox.Enabled = false;
                // Default current user's name as the userId field
                string userName = string.Empty;
                try {
                    userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    FtpTunnel tunnel = new FtpTunnel(null);
                    userName = tunnel.lastPathSegment(userName, '\\');
                } catch (Exception e) {
                    logger.Error("Exception while retrieving current userName: " + e.Message + " " + e.StackTrace);
                }
                userIdTextBox.Text = userName;
            } catch(Exception e) {
                logger.Error("Exception in ctor: " + e.Message + " " + e.StackTrace);
            }
        } // ctor

        private void RemoteBackupForm_Load(object sender, EventArgs e) {
            try {
                ftp = new Chilkat.Ftp2();
                ftp.UnlockComponent("CHRISTFTP_aQwoowLG5Uno");  // License key for Chilkat.Ftp2 (ChilkatDotNet4)
                initializeTimer();
                // this.initializeProgressTimer(); // CDD-TODO: do not leave this here.  Start and stop where appropriate.
                // Make sure events are enabled.  Get heartbeat events every 100 millisec.
                ftp.EnableEvents = true;
                ftp.HeartbeatMs = 100;
                ftp.KeepSessionLog = true;
                // Add event callbacks for m_ftp
                ftp.OnAbortCheck += new Chilkat.Ftp2.AbortCheckEventHandler(m_ftp_OnAbortCheck);
                ftp.OnFtpPercentDone += new Chilkat.Ftp2.FtpPercentDoneEventHandler(m_ftp_OnFtpPercentDone);
                // initializeSpecialFolderButtons();
                config = new Configuration();
                config.queryUserConfigFile();
                config.queryAppConfigFile();
                setLocalAttributesFromConfig();
                config.populatePrimaryFoldersList();
                if (this.includeCustomFolders) {
                    config.addCustomFoldersToPrimaryFoldersList();
                }
                populateLocalTreeViewFromRoot();
                putFilesButton.Enabled = pullFilesButton.Enabled = false;
            } catch(Exception ex) {
                logger.Error("Exception in form load: "+ex.Message+" "+ex.StackTrace);
            }
        }

        private void logStartup() {
            logger.Info("");
            logger.Info("--------------------------------------------------------------");
            logger.Info("RemoteBackup GUI starting up.");
            logger.Info("--------------------------------------------------------------");
        }

        private void setLocalAttributesFromConfig() {
            this.userIdTextBox.Text = config.UserId;
            this.passwordTextBox.Text = config.Password;
            this.serverNameTextBox.Text = config.Server;
            this.portTextBox.Text = config.Port;
            this.useFtps = config.UseFtps;
            this.LocalPanelLabel.Text = config.LocalPanelLabel;
            this.remotePanelLabel.Text = config.RemotePanelLabel;
            this.includeDesktop = config.IncludeDesktop;
            this.includeDocuments = config.IncludeDocuments;
            this.includeFavorites = config.IncludeFavorites;
        }


        // could finish this: RadioButton m_radiogroup1Checked = null;
        // not using radio buttons anymore though.
        private void initializeSpecialFolderButtons() {
            FtpTunnel tunnel = new FtpTunnel(null);
            RadioButtonManager buttonManager = new RadioButtonManager();
            buttonManager.Options.Add("Desktop");
            //buttonManager.Options.Add(tunnel.lastPathSegment(getDocumentsPath(), '\\'));
            buttonManager.generateRadioButtonsFromOptions();
            buttonManager.RadioButtons.Sort((x, y) => string.Compare(x.Text, y.Text) * -1 );
            foreach (RadioButton button in buttonManager.RadioButtons) {
                // this.specialFoldersGroupBox1.Controls.Add(button);
                // button.Click += new EventHandler(
            }
            //this.specialFoldersGroupBox1.Enabled=false; // enable this later
        }

        private void initializeTimer() {
            timer.Tick += new EventHandler(doTimerAction);
            timer.Interval = 300000; // every 5 mins
            timer.Enabled = true;
            timer.Start();
        }

        private void initializeProgressTimer() {
            progressTimer.Tick += new EventHandler(doProgressTimerAction);
            progressTimer.Interval = 5000;
            progressTimer.Enabled = true;
            progressTimer.Start();
        }

        private void pullFilesButton_Click(object sender, EventArgs e) {
            try {
                // The most common method of doing secure FTP file transfers is
                // by AUTH TLS (also known as AUTH SSL).
                // The client connects on the standard unencrypted FTP port 21
                // and then issues an "AUTH TLS" command to convert the TCP/IP channel
                // to a TLS encrypted channel.  All communications from that point onward,
                // including data transfers, are encrypted.

                // This C# FTPS example shows to how do secure FTP uploads and 
                // downloads using AUTH TLS.
                // Chilkat.Ftp2 ftp = new Chilkat.Ftp2();

                toggleClickablesOn(false);
                if (ftp.IsConnected == false) {
                    connectToFtpServer();
                }
                logIt("Downloading...");
                // Instantiate and initialize ftpInstruction
                FtpInstruction ftpInstruct = new FtpInstruction();
                ftpInstruct.Ftp = ftp;
                ftpInstruct.IsUpload = false;
                ftpInstruct.Hostname = this.serverNameTextBox.Text;
                ftpInstruct.Port = this.portTextBox.Text;
                ftpInstruct.Username = this.userIdTextBox.Text;
                ftpInstruct.Password = this.passwordTextBox.Text;
                ftpInstruct.UseFtps = this.useFtps;
                // Set list of available special folders
                populateSpecialFoldersList(ftpInstruct);
                // First look for selected item in the local ListView panel.
                ListView.SelectedListViewItemCollection selectedListViewItems = listViewRemote.SelectedItems;
                logger.Debug("selectedListViewItems.Count: " + selectedListViewItems.Count);
                if (selectedListViewItems.Count > 0) { // Look for selections
                    logger.Trace("calling ftpInstruct.prepareFtpInstructionsFromRemote(selectedListViewItems)");
                    ftpInstruct.prepareFtpInstructionsFromRemote(selectedListViewItems);
                    // Run in a background thread.
                    m_bgWorker.RunWorkerAsync(ftpInstruct); // fires DoWork
                    // NOTE: After DoWork finishes, RunWorkerCompleted will 
                    // call refreshRemoteDisplay and toggleClickablesOn.
                } else { // If nothing found, use the selection in the local Tree View panel.
                    logger.Debug("Nothing select in remote ListView panel.  Checking remote TreeView panel.");
                    TreeNode selectedTreeNode = treeViewRemote.SelectedNode;
                    if (selectedTreeNode != null) {
                        FtpItem folder = (FtpItem)selectedTreeNode.Tag;
                        if (folder != null) {
                            logIt("Restoring " + folder.Name);
                            ftpInstruct.prepareFtpInstructions(folder);
                            // Run in a background thread.
                            m_bgWorker.RunWorkerAsync(ftpInstruct); // fires DoWork
                            // NOTE: After DoWork finishes, RunWorkerCompleted will 
                            // call refreshRemoteDisplay and toggleClickablesOn.
                        }
                    } else {
                        logIt("Could not resolve selected folder in either TreeView or ListView.");
                        toggleClickablesOn(true);
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void toggleClickablesOn(bool enabled) {
            this.treeViewRemote.Enabled = enabled;
            this.listViewRemote.Enabled = enabled;
            this.putFilesButton.Enabled = enabled;
            this.pullFilesButton.Enabled = enabled;
            this.remoteRefreshButton.Enabled = enabled;
            this.abortButton.Enabled = !enabled;
        }

        /** 
         *  NOTE: After RunWorkerAsync() (i.e. DoWork) finishes, RunWorkerCompleted will 
         *  call refreshRemoteDisplay and toggleClickablesOn.
         */
        private void putFilesButton_Click(object sender, EventArgs e) {
            try {
                logIt("Uploading...");
                // Disable buttons until task is complete.
                toggleClickablesOn(false);
                // Instantiate and initialize ftpInstruction
                FtpInstruction ftpInstruct = new FtpInstruction();
                ftpInstruct.Ftp = ftp;
                ftpInstruct.IsUpload = true;
                ftpInstruct.Hostname = this.serverNameTextBox.Text;
                ftpInstruct.Port = this.portTextBox.Text;
                ftpInstruct.Username = this.userIdTextBox.Text;
                ftpInstruct.Password = this.passwordTextBox.Text;
                ftpInstruct.UseFtps = this.useFtps;
                // Set list of available special folders
                populateSpecialFoldersList(ftpInstruct);
                // First look for selected item in the local ListView panel.
                ListView.SelectedListViewItemCollection selectedListViewItems = listView1.SelectedItems;
                if (selectedListViewItems.Count > 0) { // Look for selections
                    ftpInstruct.prepareFtpInstructions(selectedListViewItems);
                    // Run in a background thread.
                    m_bgWorker.RunWorkerAsync(ftpInstruct); // fires DoWork
                } else { // If nothing found, use the selection in the local Tree View panel.
                    logger.Debug("Nothing select in local ListView panel.  Checking local TreeView panel.");
                    TreeNode selectedTreeNode = treeView1.SelectedNode;
                    logger.Debug("selected items: " + selectedTreeNode.Nodes.Count);
                    logger.Debug("name: "+selectedTreeNode.Text);

                    if (selectedTreeNode != null) {
                        DirectoryInfo folder = (DirectoryInfo)selectedTreeNode.Tag;
                        if (folder != null) {
                            logIt("Backing up " + folder.Name);
                            FtpItem ftpItem = new FtpItem(folder.Name, folder.GetDirectories().Length, folder.LastAccessTime, true);
                            ftpItem.FullName = folder.FullName;
                            ftpInstruct.prepareFtpInstructions(ftpItem);
                            // Run in a background thread.
                            m_bgWorker.RunWorkerAsync(ftpInstruct); // fires DoWork
                        } else { // Tag is null.  So check to see if we are on the "All" folder.
                            logger.Debug("selectedTreeNode.Name: " + selectedTreeNode.Name);

                            if (selectedTreeNode.Text.Equals("All")) {
                                ftpInstruct.prepareFtpInstructionsFromSpecialFolders();
                                // Run in a background thread.
                                m_bgWorker.RunWorkerAsync(ftpInstruct); // fires DoWork
                            } else {
                                toggleClickablesOn(true);
                            }
                        }
                    } else {
                        logIt("Could not resolve selected folder in either TreeView or ListView.");
                        toggleClickablesOn(true);
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
                toggleClickablesOn(true);
            }
        }

        private void populateSpecialFoldersList(FtpInstruction ftpInstruct) {
            try {
                if (ftpInstruct.LocalSpecialFolders == null) {
                    ftpInstruct.LocalSpecialFolders = new List<string>();
                }
                foreach (string path in config.PrimaryFolders) {
                    ftpInstruct.LocalSpecialFolders.Add(path);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void refreshLocalTreeView() {
            logIt("Refreshing local display.");
            // CDD-TODO: first recursively clear sub-directories.
            treeView1.Nodes.Clear();
            populateLocalTreeViewFromRoot();
        }

        // Notes: 
        // DirectoryInfo info = new DirectoryInfo(@"../.."); // one up from the current location

        private bool populateLocalTreeViewFromRoot() {
            bool success = true;
            logger.Trace("populateLocalTreeViewFromRoot starting...");
            try {
                // Setup "All" as the rootNode.  
                TreeNode rootNode = new TreeNode("All");
                rootNode.Tag = null;
                foreach (string path in config.PrimaryFolders) {
                    addPrimaryFolderToRootNode(rootNode, path);
                }
                // Check to see if we should add custom folders.
                //if (includeCustomFolders) {
                //    foreach (string path in config.CustomFolders) {
                //        addCustomFoldersToRootNode(rootNode, path);
                //    }
                //}
                // Add rootNode to the local treeView
                treeView1.Nodes.Add(rootNode);
            } catch (Exception ex) {
                logger.Error(ex.Message+" "+ex.StackTrace);
                success = false;
            }
            return success;
        }


        /** Add passed in primaryFolder to rootNode.
         */
        private void addPrimaryFolderToRootNode(TreeNode rootNode, string path) {
            try {
                // Add passed in primaryFolder to rootNode.
                logger.Debug("Adding "+path+" to rootNode.");
                TreeNode primaryNode = null;
                DirectoryInfo primaryInfo = new DirectoryInfo(path);
                if (primaryInfo.Exists) {
                    primaryNode = new TreeNode(primaryInfo.Name);
                    primaryNode.Tag = primaryInfo;
                    // Add subfolders to favorites
                    getDirectories(primaryInfo.GetDirectories(), primaryNode);
                    rootNode.Nodes.Add(primaryNode);
                }
            } catch (Exception e) {
                logger.Error("Exception: " + e.Message + " " + e.StackTrace);
            }
        }

        private void getDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo) {
            try {
                TreeNode aNode;
                DirectoryInfo[] subSubDirs;
                foreach (DirectoryInfo subDir in subDirs) {
                    aNode = new TreeNode(subDir.Name, 0, 0);
                    aNode.Tag = subDir;
                    aNode.ImageKey = "folder";
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0) {
                        getDirectories(subSubDirs, aNode);
                    }
                    nodeToAddTo.Nodes.Add(aNode);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        void listView1_NodeMouseClick(object sender,
            ListViewItemSelectionChangedEventHandler e) {
                MessageBox.Show("You are in the listView1_NodeMouseClick event.");
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            try {
                logger.Trace("treeView1_NodeMouseClick received.");
                TreeNode newSelected = e.Node;
                if (newSelected != null) {
                    listView1.Items.Clear();
                    if (newSelected.Tag != null) {
                        DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
                        populateListViewLocal(nodeDirInfo);
                    } // else {
                    //    logger.Debug("Tag for " + newSelected.Name + " is null.");
                    //    addSpecialFoldersToListViewLocal(newSelected);
                    //}
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void addSpecialFoldersToListViewLocal(TreeNode rootNode) {
            try {
                ListViewItem.ListViewSubItem[] subItems;
                ListViewItem item = null;
                foreach (TreeNode node in rootNode.Nodes) { // the special folder nodes
                    item = new ListViewItem(node.Text, 0);
                    if (node.Tag != null) {
                        item.Tag = node.Tag;
                        DirectoryInfo dir = (DirectoryInfo)node.Tag;
                        subItems = new ListViewItem.ListViewSubItem[]
                        {new ListViewItem.ListViewSubItem(item, "Directory"), 
                         new ListViewItem.ListViewSubItem(item, 
						    dir.LastAccessTime.ToShortDateString())};
                        item.SubItems.AddRange(subItems);
                        listView1.Items.Add(item);
                    } else {
                        logger.Error("Tag is null for node: "+node.Name);
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void populateListViewLocal(DirectoryInfo nodeDirInfo) {
            if (nodeDirInfo == null) {
                throw new Exception("Directory must not be null.");
            }
            try {
                ListViewItem.ListViewSubItem[] subItems;
                ListViewItem item = null;
                // Add a special 1-up folder
                if (nodeDirInfo.Parent != null) {
                    item = new ListViewItem("..", 0);
                    item.Tag = nodeDirInfo;
                    listView1.Items.Add(item);
                }
                foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories()) {
                    item = new ListViewItem(dir.Name, 0);
                    item.Tag = dir;
                    subItems = new ListViewItem.ListViewSubItem[]
                        {new ListViewItem.ListViewSubItem(item, "File Folder"), 
                         new ListViewItem.ListViewSubItem(item, 
						    dir.LastWriteTime.ToShortDateString()+" "+dir.LastWriteTime.ToShortTimeString())};
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
                foreach (FileInfo file in nodeDirInfo.GetFiles()) {
                    item = new ListViewItem(file.Name, 1);
                    item.Tag = file;
                    // FYI - To know the parent directory: file.Directory
                    subItems = new ListViewItem.ListViewSubItem[]
                        { new ListViewItem.ListViewSubItem(item, "File"), 
                         new ListViewItem.ListViewSubItem(item, 
						    file.LastWriteTime.ToShortDateString()+" "+file.LastWriteTime.ToShortTimeString())};
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }

                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        //private void listView1_SelectedIndexChanged_UsingItems(object sender, System.EventArgs e) {
        //    MessageBox.Show("You are in the listView1_SelectedIndexChanged event.");
        //    ListView.SelectedListViewItemCollection selection =
        //        this.listView1.SelectedItems;
        //    foreach (ListViewItem item in selection) {
        //        logIt(item.Name);
        //    }
        //}

        //private void listView1_ItemSelectionChanged(object sender, System.EventArgs e) {
        //    // MessageBox.Show("You are in the ItemSelectionChange event.");
        //    logIt("You are in the listView1_ItemSelectionChanged event.");
        //    foreach (ListViewItem item in listView1.SelectedItems) {
        //        logIt(item.Text);
        //    }
        //}

        //private void listViewRemote_ItemSelectionChanged(object sender, System.EventArgs e) {
        //    logIt("You are in the listViewRemote_ItemSelectionChanged event.");
        //    foreach (ListViewItem item in listViewRemote.SelectedItems) {
        //        logIt(item.Text);
        //    }
        //}

        /** Only okay to move up if the parent is not one of the primary folders.
         * This method compares the passed in folder to the primary folders.  
         * If there is a match, it's not okay to move up.  Otherwise, returns true.
         */ 
        private bool okayToMoveUp(DirectoryInfo parent) {
            bool moveUpOkay=true;
            // Make sure the parent is not one of the primary folders
            foreach (string path in config.PrimaryFolders) {
                if (parent.FullName.Equals(path)) {
                    moveUpOkay = false;
                    break;
                }
            }
            return moveUpOkay;
        }

        private void listViewRemote_MouseDoubleClick(object sender, MouseEventArgs e) {
            try {
                logger.Trace("listViewRemote_MouseDoubleClick event.");
                ListView.SelectedListViewItemCollection selection = listViewRemote.SelectedItems;
                if (selection.Count == 1) { // Should only be one element since we are double-clicking.
                    ListViewItem item = selection[0];
                    if (item!=null) {
                        if (item.Tag != null) {
                            FtpItem ftpItem = (FtpItem)item.Tag;
                            if (ftpItem.IsDirectory) {
                                if (item.Text.Equals("..")) {
                                    if (ftpItem.ParentItem != null) {
                                        logger.Trace("Moving up to (remote) parent folder.");
                                        logger.Debug("Parent: " + ftpItem.ParentItem.FullName);
                                        listViewRemote.Items.Clear();
                                        populateListViewRemoteFromParent(ftpItem.ParentItem);
                                        //setTreeViewSelectedToMatchingDirectoryInfo(treeViewRemote, ftpItem.ParentItem.FullName);
                                        findMatchingRemoteNodeAndSet(treeViewRemote.Nodes, ftpItem.ParentItem, treeViewRemote);
                                    } else {
                                        logger.Error("ftpItem.ParentItem is null.");
                                    }
                                } else {
                                    logger.Debug("Populating remote list view with directory: "+item.Text);
                                    listViewRemote.Items.Clear();
                                    populateListViewRemoteFromParent(ftpItem);
                                    // Consider parent folder as well.  That way we won't get /Temp when it should be /Archive/Temp
                                    findMatchingRemoteNodeAndSet(treeViewRemote.Nodes, ftpItem, treeViewRemote);
                                    logger.Debug("treeView1 selected: " + treeViewRemote.SelectedNode.FullPath);
                                }
                            }
                        } else {
                            logger.Error("Tag of selected ftpItem is null.  Must not be null.");
                        } 
                    } else {
                        logger.Error("ListViewItem is null.");
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e) {
            try {
                logger.Trace("listView1_MouseDoubleClick received.");
                // Switch to the root dir of listView1 to the folder clicked on.
                ListView.SelectedListViewItemCollection selection = this.listView1.SelectedItems;
                if (selection.Count == 1) { // Should only be one element since we are double-clicking.
                    ListViewItem item = selection[0];
                    if (item.Tag == null) {
                        logger.Debug("At the top");
                    } else {
                        if (item.Tag is DirectoryInfo) {
                            if (item.Text.Equals("..")) {
                                DirectoryInfo dirInfo = (DirectoryInfo)item.Tag;
                                if (okayToMoveUp(dirInfo)) {
                                    logger.Trace("Moving up to (local) parent folder.");
                                    logger.Trace("dirInfo:        " + dirInfo.FullName);
                                    logger.Debug("dirInfo.Parent: " + dirInfo.Parent.FullName);
                                    logger.Trace("Clearing and populating local listView.");
                                    listView1.Items.Clear();
                                    populateListViewLocal(dirInfo.Parent);
                                    setTreeViewSelectedToMatchingDirectoryInfo(this.treeView1, dirInfo.Parent.FullName);
                                } else {
                                    // Don't go up any higher.  Next one up is "All", however, it's not an actual
                                    // Directory.  
                                    setTreeViewSelectedToMatchingDirectoryInfo(treeView1, dirInfo.FullName);
                                }
                            } else {
                                logger.Debug("Populating local ListView with directory: " + item.Text);
                                listView1.Items.Clear();
                                populateListViewLocal((DirectoryInfo)item.Tag);
                                setTreeViewSelectedToMatchingDirectoryInfo(this.treeView1, ((DirectoryInfo)item.Tag).FullName);
                                logger.Debug("treeView1 selected: " + treeView1.SelectedNode.FullPath);
                            }
                        } else {
                            logger.Trace("Double-click on file is ignored.");
                        }
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        /** Start at the rootNode and search the DirectoryInfo Tags for the matching dirInfo.
         *  Once found, set the matching node as selected in the TreeView
         */
        private void setTreeViewSelectedToMatchingDirectoryInfo(TreeView treeView, string fullFolderName) {
            try {
                findMatchingNodeAndSet(treeView.Nodes, fullFolderName, treeView);
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        /** Finds the matching node in nodes that matches the dirInfo passed in.  
         */ 
        private void findMatchingNodeAndSet(TreeNodeCollection nodes, string fullFolderName, TreeView treeView) {
            try {
                if (nodes==null) {
                    return;
                } else {
                    foreach (TreeNode node in nodes) {
                        if (node.Tag!=null) {
                            DirectoryInfo info = (DirectoryInfo) node.Tag;
                            if (info.FullName.Equals(fullFolderName)) { // found a match
                                //  matchedNode = node;
                                treeView.SelectedNode = node;
                                treeView.SelectedNode.Expand();
                                treeView.Focus();
                                break;
                            } else if (node.Nodes!=null && node.Nodes.Count>0) {
                                findMatchingNodeAndSet(node.Nodes, fullFolderName, treeView);
                            }
                        } else if (node.Nodes != null && node.Nodes.Count>0) { // special case for starting at the root
                            findMatchingNodeAndSet(node.Nodes, fullFolderName, treeView);
                        }
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        /** Finds the matching node in nodes that matches the dirInfo passed in.  
         * Consider parent folder as well.  That way we won't get /Temp when it should be /Archive/Temp
         */
        private void findMatchingRemoteNodeAndSet(TreeNodeCollection nodes, FtpItem matchItem, TreeView treeView) {
            try {
                if (nodes == null) {
                    return;
                } else {
                    foreach (TreeNode node in nodes) {
                        if (node.Tag != null) {
                            FtpItem ftpItem = (FtpItem)node.Tag;
                            if (ftpItem !=null && ftpItem.Name != null &&
                                matchItem!=null && matchItem.Name!=null && ftpItem.Name.Equals(matchItem.Name)) { // found a match
                                // Check the parent too
                                if (ftpItem.ParentItem!=null && matchItem.ParentItem!=null && 
                                        matchItem.ParentItem.Name.Equals(ftpItem.ParentItem.Name)) {
                                    treeView.SelectedNode = node;
                                    treeView.SelectedNode.Expand();
                                    treeView.Focus();
                                    break;
                                }
                            } else if (node.Nodes != null && node.Nodes.Count > 0) {
                                findMatchingRemoteNodeAndSet(node.Nodes, matchItem, treeView);
                            }
                        } else if (node.Nodes != null && node.Nodes.Count > 0) { // special case for starting at the root
                            findMatchingRemoteNodeAndSet(node.Nodes, matchItem, treeView);
                        }
                    }
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        public bool connectToFtpServer() {
            try {
                if (ftp.IsConnected == true) {
                    logIt("Already connected.  Skipping connect.");
                    return true;
                }

                // The most common method of doing secure FTP file transfers is
                // by AUTH TLS (also known as AUTH SSL).
                // The client connects on the standard unencrypted FTP port 21
                // and then issues an "AUTH TLS" command to convert the TCP/IP channel
                // to a TLS encrypted channel.  All communications from that point onward,
                // including data transfers, are encrypted.

                // Set the AuthTls property = true if configured to use it. 
                // This can be set in the App Config file.  
                if (this.useFtps == true) {
                    ftp.AuthTls = true;
                }

                // Leave the Ssl property false.  The Ssl property is used
                // for doing Secure FTP over SSL on port 990 (implicit SSL)
                // FTP Implicit SSL is covered in another example.
                //  ftp.Ssl = false;

                // Set the FTP hostname, login, and password.
                ftp.Hostname = this.serverNameTextBox.Text;
                int port = 21;
                try {
                    int.TryParse(this.portTextBox.Text, out port);
                } catch (Exception ex) {
                    logger.Error("Could not parse port value of "+this.portTextBox.Text);
                    logger.Error("Defaulting to port 21.");
                    logger.Error("Exception "+ex.Message+" "+ex.StackTrace);
                }
                ftp.Port = port;
                ftp.Username = this.userIdTextBox.Text;
                ftp.Password = this.passwordTextBox.Text;

                // Session logging is not required, but I'm turning it on here to see
                // what transpires during the FTP session.  The session log provides the
                // unencrypted FTP requests and responses.
                ftp.KeepSessionLog = true;

                // Connect and login to the FTP server.  This establishs a control connection
                // to the FTP server (port 21), converts the channel to TLS (because AuthTls is true),
                // and then authenticates.
                bool success = ftp.Connect();
                if (success) {
                    success = ftp.LoginVerified; // Double check the authentication
                }
                if (success==false) {
                    logger.Error(ftp.SessionLog);
                    logger.Error(ftp.LastErrorText);
                    return false;
                }
                // You may need to clear the control channel if your FTP client
                // is located behind a network-address-translating router
                // such as with a Cable/DSL router.
                ftp.ClearControlChannel();
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
            return true;
        }

        private void disconnectFromFtpServer() {
            try {
                ftp.Disconnect();
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private string absolutePath(FtpItem item) {
            if (item != null) {
                if (item.ParentItem == null) { // root folder.  name is /
                    return item.Name;
                } else { 
                    return absolutePath(item.ParentItem) + "/" + item.Name;
                }
            } else {
                return "";
            }
        }


        /** Gets the subfolders and files within the passed in remote folder.
         *  Adds those subfolders and files as Children to the passed in FtpItem (folder).
         *
         * Returns the number of children.
         */
        private int getRemoteDirectoryListing(FtpItem folder, bool goUpOneFirst) {
            try {
                string remoteDirectory = "/";
                if (folder == null) {
                    throw new Exception("parentItem must not be null.");
                }
                if (folder != null) {
                    remoteDirectory = absolutePath(folder);
                    logger.Debug("remoteDirectory: " + remoteDirectory);
                }
                if (folder.Children == null) {
                    folder.Children = new List<FtpItem>();
                } else {
                    folder.Children.Clear();
                }
                if (goUpOneFirst) {
                    logger.Debug("Switching to parent directory.");
                    ftp.ChangeRemoteDir(@"..");
                }
                logger.Debug("Switching to remote directory: " + remoteDirectory);
                if (ftp.ChangeRemoteDir(remoteDirectory)) {
                    int numItems = ftp.NumFilesAndDirs;
                    if (numItems < 0) {
                        logIt("An error occured while retrieving the remote directory listing.  Please try again.");
                        // MessageBox.Show(ftp.LastErrorText);
                    } else {
                        logger.Debug("num items: " + numItems + " in " + remoteDirectory);
                        for (int idx = 0; idx < numItems; idx++) {
                            string name = ftp.GetFilename(idx);
                            if (name.Equals(".", StringComparison.CurrentCultureIgnoreCase)) {
                                // name = "/";
                                continue;
                            }
                            if (name.Equals("..", StringComparison.CurrentCultureIgnoreCase)) {
                                continue;
                            }
                            DateTime modDate = ftp.GetLastModifiedTime(idx);
                            int size = ftp.GetSize(idx);
                            FtpItem item = new FtpItem(name, size, modDate, ftp.GetIsDirectory(idx));
                            item.ParentItem = folder;
                            folder.Children.Add(item);
                        }
                        if (folder.Children.Count > 0) {
                            folder.Children.Sort((x, y) => string.Compare(x.LowerName, y.LowerName));
                        }
                    }
                } else {
                    logger.Error("Unable to switch to remote directory: " + remoteDirectory);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
            return folder.Children.Count;
        }

        private void populateRemoteTreeView(FtpItem parent) {
            try {
                if (parent!=null) {
                    TreeNode remoteRootNode = null;
                    // First populate the root node
                    if (this.treeViewRemote.Nodes.Count == 0) {
                        remoteRootNode = new TreeNode(parent.Name);
                        remoteRootNode.Tag = parent;
                        parent.TreeNode = remoteRootNode;
                    } 
                    // Now add the sub folders
                    if (parent.TreeNode.Nodes.Count == 0) { // Only do once per root
                        if (parent.Children != null && parent.Children.Count > 0) {
                            foreach (FtpItem child in parent.Children) {
                                if (child.IsDirectory) {
                                    TreeNode aNode = new TreeNode(child.Name, 0, 0);
                                    aNode.Tag = child;
                                    aNode.ImageKey = "folder";
                                    child.TreeNode = aNode;
                                    parent.TreeNode.Nodes.Add(aNode);
                                }
                            }
                        }
                    }
                    // Only do this once for the main root node.
                    if (this.treeViewRemote.Nodes.Count == 0) {
                        this.treeViewRemote.Nodes.Add(remoteRootNode);
                    }
                } else {
                    logger.Error("error: parent is null.");
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void populateListViewRemoteFromParent(FtpItem parent) {
            try {
                logger.Trace("populateListViewRemoteFromParent called.");

                this.listViewRemote.Items.Clear();
                ListViewItem.ListViewSubItem[] listViewColumns = null;
                ListViewItem listViewItem = null;

                if (parent == null) {
                    logger.Debug("parent is null");
                } else if (parent.Children == null) {
                    logger.Debug(parent.Name + " has no children.");
                } else {
                    logger.Debug("Number of items in " + parent.Name + ": " + parent.Children.Count);
                }

                // If the clicked on remote list node does not have children yet.  Get the listing from the FTP server.
                if (parent.Children == null) {
                    toggleClickablesOn(false);
                    bool clickablesOff = true;
                    if (getRemoteDirectoryListing(parent, false) > 0) {
                        populateRemoteTreeView(parent);
                    }
                    if (clickablesOff) {
                        toggleClickablesOn(true);
                    }
                }

                if (parent!=null && parent.Children != null && parent.Children.Count > 0) {
                    foreach (FtpItem item in parent.Children) {
                        if (item.IsDirectory) { // add directory
                            listViewItem = new ListViewItem(item.Name, 0);
                            listViewColumns = new ListViewItem.ListViewSubItem[] {
                                                new ListViewItem.ListViewSubItem(listViewItem, "Directory"),
                                                new ListViewItem.ListViewSubItem(listViewItem, 
                                                    item.ModDate.ToString())
                                            };
                            listViewItem.SubItems.AddRange(listViewColumns);
                            listViewItem.Tag = item;
                            listViewRemote.Items.Add(listViewItem);
                        } else { // add file
                            listViewItem = new ListViewItem(item.Name, 1);
                            listViewColumns = new ListViewItem.ListViewSubItem[] {
                                                new ListViewItem.ListViewSubItem(listViewItem, "File"),
                                                new ListViewItem.ListViewSubItem(listViewItem, 
                                                    item.ModDate.ToString())
                                            };
                            listViewItem.SubItems.AddRange(listViewColumns);
                            listViewItem.Tag = item;
                            listViewRemote.Items.Add(listViewItem);
                        }
                        listViewRemote.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    }

                    ListViewColumnSorter sorter = new ListViewColumnSorter();
                    listViewRemote.ListViewItemSorter = sorter;
                    listViewRemote.Sort();
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }


        //private void populateListViewRemote(FtpItem ftpItem) {
        //    this.listViewRemote.Items.Clear();
        //    ListViewItem.ListViewSubItem[] listViewColumns=null;
        //    ListViewItem listViewItem = null;
        //    if (getRemoteDirectoryListing(ftpItem, true)>0) {
        //        foreach (FtpItem item in ftpItem.Children) {
        //            if (item.IsDirectory) { // add directory
        //                listViewItem = new ListViewItem(item.Name, 0);
        //                listViewColumns = new ListViewItem.ListViewSubItem[] {
        //                                    new ListViewItem.ListViewSubItem(listViewItem, "Directory"),
        //                                    new ListViewItem.ListViewSubItem(listViewItem, item.ModDate.ToShortDateString())
        //                                };
        //                listViewItem.SubItems.AddRange(listViewColumns);
        //                listViewRemote.Items.Add(listViewItem);
        //            } else { // add file
        //                listViewItem = new ListViewItem(item.Name, 1);
        //                listViewColumns = new ListViewItem.ListViewSubItem[] {
        //                                    new ListViewItem.ListViewSubItem(listViewItem, "File"),
        //                                    new ListViewItem.ListViewSubItem(listViewItem, item.ModDate.ToShortDateString())
        //                                };
        //                listViewItem.SubItems.AddRange(listViewColumns);
        //                listViewRemote.Items.Add(listViewItem);
        //            }
        //            listViewRemote.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        //        }
        //    } else {
        //        logIt("Unable to retrieve directory listing for "+ftpItem.Name);
        //    }
        //}

        private void logIt(string message) {
            try {
                this.StatusBox.Text += message + Environment.NewLine;
                logger.Info(message);
                this.StatusBox.SelectionStart = this.StatusBox.TextLength;
                this.StatusBox.ScrollToCaret();
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void doProgressTimerAction(object sender, EventArgs e) {
            if (ftp.AsyncFinished == false) {
                this.logIt(ftp.AsyncBytesReceived + " bytes received" + "\r\n");
                this.logIt(ftp.DownloadRate + " bytes per second" + "\r\n");
            } else {
                logIt("Async finished");
            }
        }

        private void doTimerAction(object sender, EventArgs e) {
            try {
                if (ftp.IsConnected==true) {
                    logIt("Performing Noop.");
                    ftp.Noop();
                } else {
                    logIt("Not connected to FTP server.  Re-connecting.");
                    connectToFtpServer();
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        void treeViewRemote_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            try {
                logger.Trace("treeViewRemote_NodeMouseClick starting ...");
                TreeNode newSelected = e.Node;
                FtpItem item = (FtpItem)newSelected.Tag;
                logger.Debug("item selected: " + item.ToString());
                logger.Debug("selected item.TreeNode.Nodes.Count:     " + item.TreeNode.Nodes.Count);
                if (item.Children != null) {
                    logger.Debug("selected item.Children.Count:       " + item.Children.Count);
                } else {
                    logger.Debug("item.Children == null");
                }
                bool clickablesOff = false;
                // If no sub-directories, and no children, attempt to get listing from remote FTP server.
                if (item.TreeNode.Nodes.Count == 0 && (item.Children==null || item.Children.Count==0)) {
                    toggleClickablesOn(false);
                    clickablesOff = true;
                    if (getRemoteDirectoryListing(item, true) > 0) {
                        populateRemoteTreeView(item);
                    }
                }
                populateListViewRemoteFromParent(item);
                if (clickablesOff) {
                    toggleClickablesOn(true);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void remoteRefreshButton_Click(object sender, EventArgs e) {
            try {
                logger.Trace("remoteRefreshButton_Click received.");
                toggleClickablesOn(false);
                refreshRemoteDisplay();
                toggleClickablesOn(true);
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        /** NOTE: The more folders and files on the remote ftp server, the longer this takes.
         */
        //private string getRemoteDirectoryXml() {
        //    logger.Trace("getRemoteDirectoryXml starting...");
        //    if (ftp.IsConnected == false) {
        //        connectToFtpServer();
        //    }
        //    string strXml = ftp.DirTreeXml();
        //    this.StatusBox.Text += strXml + "\r\n";
        //    logger.Debug(strXml);
        //    return strXml;
        //}

        private void refreshRemoteDisplay() {
            try {
                if (ftp.IsConnected == false || ftp.LoginVerified==false) {
                    if (connectToFtpServer() == false) {
                        logger.Error("Exiting refreshRemoteDisplay.  Could not connect to FTP server.");
                    } else {
                        logIt("Successfully connected to remote FTP server.");
                    }
                }
                if (ftp.IsConnected && ftp.LoginVerified) {
                    logIt("Refreshing remote display.");
                    this.treeViewRemote.Nodes.Clear();
                    if (remoteStructure == null) {
                        remoteStructure = new FtpItem("/", 0, DateTime.Now, true);
                    } else {
                        remoteStructure.Children.Clear();
                        remoteStructure.TreeNode.Nodes.Clear();
                        // CDD-TODO: recursively clear all children, grandchildren, etc.
                    }
                    getRemoteDirectoryListing(remoteStructure, false);
                    populateRemoteTreeView(remoteStructure);
                    populateListViewRemoteFromParent(remoteStructure);
                } else {
                    logIt("Unable to connect.  Please check login credentials (i.e. userId and password.)");
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void RemoteBackupForm_Closing(object sender, EventArgs e) {
            if (ftp.IsConnected && this.passwordTextBox.Text.Trim().Length > 0 &&
                this.userIdTextBox.Text.Trim().Length > 0) {
                config.saveSettingsToConfigFile(this.userIdTextBox.Text, this.passwordTextBox.Text);
            }
            this.disconnectFromFtpServer();
        }

        private void m_bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            try {
                // Display the error information and session log.
                // If the operation succeeded, the last error text will provide information about
                // the operation -- the presence of content does not indicate an error.
                bool success = (bool)e.Result;
                if (success) {
                    logIt("Success!");
                } else {
                    logger.Error(m_ftpLastError);
                    logger.Error(m_ftpSessionLog);
                    logIt("Check log file."); // i.e. Failure
                }
                // Depending on upload or download, refresh remote or local display.
                if (this.isFtpUpload) {
                    refreshRemoteDisplay();
                }
                // Re-enable Clickables.
                toggleClickablesOn(true);
                if (this.isFtpUpload == false) {
                    refreshLocalTreeView();
                }
                m_bgWorker.Dispose(); // Done.
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void m_bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            try {
                if (e.ProgressPercentage == 0) {
                    // This is an AbortCheck callback...
                    int value = heartbeatProgressBar.Value;
                    if (value > 90) {
                        heartbeatProgressBar.Value = 0;
                    } else {
                        heartbeatProgressBar.Value += 10;
                    }
                } else {
                    // This is called from an OnFtpPercentDone event callback.
                    progressBar1.Value = e.ProgressPercentage;
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void m_bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            try {
                // Indicate that we are in the background thread
                // The FTP object's event callback will need this.
                m_bgRunning = true;

                // We passed the filename as an argument.
                // However, we could've just as well accessed the filename
                // via the txtFilename textbox control.
                FtpInstruction ftpInstruct = e.Argument as FtpInstruction;

                // Is this an upload or download?
                if (ftpInstruct.IsUpload) {
                    // This causes both AbortCheck and FtpPercentDone events to
                    // fire.  The callback methods are m_ftp_OnAbortCheck and
                    // m_ftp_OnFtpPercentDone.  These methods in turn call the
                    // m_bgWorker.ReportProgress method, causing the
                    // m_bgWorker_ProgressChanged to be called.  The progress
                    // bars can only be updated from a background thread
                    // in the m_bgWorker_ProgressChanged event.  The same applies
                    // to any other Form controls.  
                    this.isFtpUpload = true;
                    FtpTunnel tunnel = new FtpTunnel(ftpInstruct);
                    bool success = tunnel.backupFilesToFTPServer();
                    e.Result = success;
                } else {
                    this.isFtpUpload = false;
                    FtpTunnel tunnel = new FtpTunnel(ftpInstruct);
                    bool success = tunnel.restoreFilesFromFTPServer();
                    e.Result = success;
                }
                // Display the last-error and session log regardless of success/failure...
                m_ftpLastError = ftp.LastErrorText;
                m_ftpSessionLog = ftp.SessionLog;
                m_bgRunning = false;
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
            return;
        }

        void m_ftp_OnFtpPercentDone(object sender, Chilkat.FtpPercentDoneEventArgs args) {
            try {
                // We need to update progress bars from the BackgroundWorker's
                // event callback.
                if (m_bgRunning) {
                    m_bgWorker.ReportProgress(args.PercentDone);
                }
                args.Abort = m_abort;
                releaseAbortButton();
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        void m_ftp_OnAbortCheck(object sender, Chilkat.AbortCheckEventArgs args) {
            try {
                if (m_bgRunning) {
                    // Passing a 0 will indicat that this is an AbortCheck call...
                    // We need to update progress bars from the BackgroundWorker's
                    // event callback.
                    m_bgWorker.ReportProgress(0);
                    args.Abort = m_abort;
                } else {
                    int value = heartbeatProgressBar.Value;
                    if (value > 90) {
                        heartbeatProgressBar.Value = 0;
                    } else {
                        heartbeatProgressBar.Value += 10;
                    }
                    // If this is in the foreground, we want to keep the UI responsive.
                    Application.DoEvents();
                    // m_abort gets set to true if the Abort button is pressed.
                    args.Abort = m_abort;

                }
                releaseAbortButton();
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void abortButton_Click(object sender, EventArgs e) {
            m_abort = true;
            this.abortButton.Enabled = false;
        }

        private void releaseAbortButton() {
            m_abort = false;
            this.abortButton.Enabled = true;
        }

    }
}
