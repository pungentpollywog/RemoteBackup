namespace RemoteBackup {
    partial class RemoteBackupForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteBackupForm));
            this.userIdTextBox = new System.Windows.Forms.TextBox();
            this.userIdLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.serverNameTextBox = new System.Windows.Forms.TextBox();
            this.serverNameLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.putFilesButton = new System.Windows.Forms.Button();
            this.pullFilesButton = new System.Windows.Forms.Button();
            this.StatusBox = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.listView1 = new System.Windows.Forms.ListView();
            this.nameColHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lastModColHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.remoteSplitContainer = new System.Windows.Forms.SplitContainer();
            this.treeViewRemote = new System.Windows.Forms.TreeView();
            this.listViewRemote = new System.Windows.Forms.ListView();
            this.NameColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TypeColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastModColHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.remoteRefreshButton = new System.Windows.Forms.Button();
            this.heartbeatProgressBar = new System.Windows.Forms.ProgressBar();
            this.heartbeatProgLabel = new System.Windows.Forms.Label();
            this.progressLabel = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.LocalPanelLabel = new System.Windows.Forms.Label();
            this.remotePanelLabel = new System.Windows.Forms.Label();
            this.abortButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.remoteSplitContainer)).BeginInit();
            this.remoteSplitContainer.Panel1.SuspendLayout();
            this.remoteSplitContainer.Panel2.SuspendLayout();
            this.remoteSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // userIdTextBox
            // 
            this.userIdTextBox.Location = new System.Drawing.Point(83, 28);
            this.userIdTextBox.Name = "userIdTextBox";
            this.userIdTextBox.Size = new System.Drawing.Size(262, 20);
            this.userIdTextBox.TabIndex = 0;
            // 
            // userIdLabel
            // 
            this.userIdLabel.AutoSize = true;
            this.userIdLabel.Location = new System.Drawing.Point(13, 31);
            this.userIdLabel.Name = "userIdLabel";
            this.userIdLabel.Size = new System.Drawing.Size(46, 13);
            this.userIdLabel.TabIndex = 1;
            this.userIdLabel.Text = "User ID:";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(555, 26);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.PasswordChar = '*';
            this.passwordTextBox.Size = new System.Drawing.Size(232, 20);
            this.passwordTextBox.TabIndex = 2;
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(480, 28);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(56, 13);
            this.passwordLabel.TabIndex = 3;
            this.passwordLabel.Text = "Password:";
            // 
            // serverNameTextBox
            // 
            this.serverNameTextBox.Location = new System.Drawing.Point(83, 54);
            this.serverNameTextBox.Name = "serverNameTextBox";
            this.serverNameTextBox.Size = new System.Drawing.Size(262, 20);
            this.serverNameTextBox.TabIndex = 4;
            // 
            // serverNameLabel
            // 
            this.serverNameLabel.AutoSize = true;
            this.serverNameLabel.Location = new System.Drawing.Point(13, 57);
            this.serverNameLabel.Name = "serverNameLabel";
            this.serverNameLabel.Size = new System.Drawing.Size(64, 13);
            this.serverNameLabel.TabIndex = 5;
            this.serverNameLabel.Text = "FTP Server:";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(480, 54);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(52, 13);
            this.portLabel.TabIndex = 6;
            this.portLabel.Text = "FTP Port:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(555, 51);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(36, 20);
            this.portTextBox.TabIndex = 7;
            this.portTextBox.Text = "21";
            // 
            // putFilesButton
            // 
            this.putFilesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.putFilesButton.Location = new System.Drawing.Point(433, 263);
            this.putFilesButton.Name = "putFilesButton";
            this.putFilesButton.Size = new System.Drawing.Size(29, 25);
            this.putFilesButton.TabIndex = 12;
            this.putFilesButton.Text = "->";
            this.putFilesButton.UseVisualStyleBackColor = true;
            this.putFilesButton.Click += new System.EventHandler(this.putFilesButton_Click);
            // 
            // pullFilesButton
            // 
            this.pullFilesButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pullFilesButton.Location = new System.Drawing.Point(433, 294);
            this.pullFilesButton.Name = "pullFilesButton";
            this.pullFilesButton.Size = new System.Drawing.Size(29, 25);
            this.pullFilesButton.TabIndex = 13;
            this.pullFilesButton.Text = "<-";
            this.pullFilesButton.UseVisualStyleBackColor = true;
            this.pullFilesButton.Click += new System.EventHandler(this.pullFilesButton_Click);
            // 
            // StatusBox
            // 
            this.StatusBox.Location = new System.Drawing.Point(14, 549);
            this.StatusBox.Multiline = true;
            this.StatusBox.Name = "StatusBox";
            this.StatusBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.StatusBox.Size = new System.Drawing.Size(863, 105);
            this.StatusBox.TabIndex = 14;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(15, 114);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            this.splitContainer1.Size = new System.Drawing.Size(398, 388);
            this.splitContainer1.SplitterDistance = 168;
            this.splitContainer1.TabIndex = 15;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.ImageIndex = 0;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.SelectedImageIndex = 0;
            this.treeView1.Size = new System.Drawing.Size(168, 388);
            this.treeView1.TabIndex = 0;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "FOLDER.ICO");
            this.imageList1.Images.SetKeyName(1, "DOCL.ICO");
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColHeader1,
            this.typeColHeader1,
            this.lastModColHeader1});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.ShowItemToolTips = true;
            this.listView1.Size = new System.Drawing.Size(226, 388);
            this.listView1.SmallImageList = this.imageList1;
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // nameColHeader1
            // 
            this.nameColHeader1.Text = "Name";
            this.nameColHeader1.Width = 95;
            // 
            // typeColHeader1
            // 
            this.typeColHeader1.Text = "Type";
            this.typeColHeader1.Width = 47;
            // 
            // lastModColHeader1
            // 
            this.lastModColHeader1.Text = "Last Modified";
            this.lastModColHeader1.Width = 78;
            // 
            // remoteSplitContainer
            // 
            this.remoteSplitContainer.Location = new System.Drawing.Point(483, 114);
            this.remoteSplitContainer.Name = "remoteSplitContainer";
            // 
            // remoteSplitContainer.Panel1
            // 
            this.remoteSplitContainer.Panel1.Controls.Add(this.treeViewRemote);
            // 
            // remoteSplitContainer.Panel2
            // 
            this.remoteSplitContainer.Panel2.Controls.Add(this.listViewRemote);
            this.remoteSplitContainer.Size = new System.Drawing.Size(397, 388);
            this.remoteSplitContainer.SplitterDistance = 154;
            this.remoteSplitContainer.TabIndex = 17;
            // 
            // treeViewRemote
            // 
            this.treeViewRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRemote.ImageIndex = 0;
            this.treeViewRemote.ImageList = this.imageList1;
            this.treeViewRemote.Location = new System.Drawing.Point(0, 0);
            this.treeViewRemote.Name = "treeViewRemote";
            this.treeViewRemote.SelectedImageIndex = 0;
            this.treeViewRemote.Size = new System.Drawing.Size(154, 388);
            this.treeViewRemote.TabIndex = 0;
            // 
            // listViewRemote
            // 
            this.listViewRemote.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.NameColHeader,
            this.TypeColHeader,
            this.LastModColHeader});
            this.listViewRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewRemote.Location = new System.Drawing.Point(0, 0);
            this.listViewRemote.Name = "listViewRemote";
            this.listViewRemote.Size = new System.Drawing.Size(239, 388);
            this.listViewRemote.SmallImageList = this.imageList1;
            this.listViewRemote.TabIndex = 0;
            this.listViewRemote.UseCompatibleStateImageBehavior = false;
            this.listViewRemote.View = System.Windows.Forms.View.Details;
            // 
            // NameColHeader
            // 
            this.NameColHeader.Text = "Name";
            this.NameColHeader.Width = 107;
            // 
            // TypeColHeader
            // 
            this.TypeColHeader.Text = "Type";
            this.TypeColHeader.Width = 48;
            // 
            // LastModColHeader
            // 
            this.LastModColHeader.Text = "Last Modified";
            this.LastModColHeader.Width = 79;
            // 
            // remoteRefreshButton
            // 
            this.remoteRefreshButton.Location = new System.Drawing.Point(802, 24);
            this.remoteRefreshButton.Name = "remoteRefreshButton";
            this.remoteRefreshButton.Size = new System.Drawing.Size(75, 23);
            this.remoteRefreshButton.TabIndex = 18;
            this.remoteRefreshButton.Text = "Connect";
            this.remoteRefreshButton.UseVisualStyleBackColor = true;
            this.remoteRefreshButton.Click += new System.EventHandler(this.remoteRefreshButton_Click);
            // 
            // heartbeatProgressBar
            // 
            this.heartbeatProgressBar.Location = new System.Drawing.Point(147, 524);
            this.heartbeatProgressBar.Name = "heartbeatProgressBar";
            this.heartbeatProgressBar.Size = new System.Drawing.Size(100, 14);
            this.heartbeatProgressBar.TabIndex = 19;
            // 
            // heartbeatProgLabel
            // 
            this.heartbeatProgLabel.AutoSize = true;
            this.heartbeatProgLabel.Location = new System.Drawing.Point(12, 524);
            this.heartbeatProgLabel.Name = "heartbeatProgLabel";
            this.heartbeatProgLabel.Size = new System.Drawing.Size(129, 13);
            this.heartbeatProgLabel.TabIndex = 20;
            this.heartbeatProgLabel.Text = "Heartbeat Activity Monitor";
            // 
            // progressLabel
            // 
            this.progressLabel.AutoSize = true;
            this.progressLabel.Location = new System.Drawing.Point(365, 523);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(97, 13);
            this.progressLabel.TabIndex = 21;
            this.progressLabel.Text = "Progess (0%-100%)";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(483, 523);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(395, 13);
            this.progressBar1.TabIndex = 22;
            // 
            // LocalPanelLabel
            // 
            this.LocalPanelLabel.AutoSize = true;
            this.LocalPanelLabel.Location = new System.Drawing.Point(13, 94);
            this.LocalPanelLabel.Name = "LocalPanelLabel";
            this.LocalPanelLabel.Size = new System.Drawing.Size(33, 13);
            this.LocalPanelLabel.TabIndex = 23;
            this.LocalPanelLabel.Text = "Local";
            // 
            // remotePanelLabel
            // 
            this.remotePanelLabel.AutoSize = true;
            this.remotePanelLabel.Location = new System.Drawing.Point(480, 94);
            this.remotePanelLabel.Name = "remotePanelLabel";
            this.remotePanelLabel.Size = new System.Drawing.Size(44, 13);
            this.remotePanelLabel.TabIndex = 24;
            this.remotePanelLabel.Text = "Remote";
            // 
            // abortButton
            // 
            this.abortButton.Location = new System.Drawing.Point(270, 518);
            this.abortButton.Name = "abortButton";
            this.abortButton.Size = new System.Drawing.Size(75, 23);
            this.abortButton.TabIndex = 25;
            this.abortButton.Text = "Abort";
            this.abortButton.UseVisualStyleBackColor = true;
            this.abortButton.Click += new System.EventHandler(this.abortButton_Click);
            // 
            // RemoteBackupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 666);
            this.Controls.Add(this.abortButton);
            this.Controls.Add(this.remotePanelLabel);
            this.Controls.Add(this.LocalPanelLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.progressLabel);
            this.Controls.Add(this.heartbeatProgLabel);
            this.Controls.Add(this.heartbeatProgressBar);
            this.Controls.Add(this.remoteRefreshButton);
            this.Controls.Add(this.remoteSplitContainer);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.StatusBox);
            this.Controls.Add(this.pullFilesButton);
            this.Controls.Add(this.putFilesButton);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.serverNameLabel);
            this.Controls.Add(this.serverNameTextBox);
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.passwordTextBox);
            this.Controls.Add(this.userIdLabel);
            this.Controls.Add(this.userIdTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RemoteBackupForm";
            this.Text = "Remote Backup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RemoteBackupForm_Closing);
            this.Load += new System.EventHandler(this.RemoteBackupForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.remoteSplitContainer.Panel1.ResumeLayout(false);
            this.remoteSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.remoteSplitContainer)).EndInit();
            this.remoteSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox userIdTextBox;
        private System.Windows.Forms.Label userIdLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox serverNameTextBox;
        private System.Windows.Forms.Label serverNameLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Button putFilesButton;
        private System.Windows.Forms.Button pullFilesButton;
        private System.Windows.Forms.TextBox StatusBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader nameColHeader1;
        private System.Windows.Forms.ColumnHeader typeColHeader1;
        private System.Windows.Forms.ColumnHeader lastModColHeader1;
        private System.Windows.Forms.SplitContainer remoteSplitContainer;
        private System.Windows.Forms.TreeView treeViewRemote;
        private System.Windows.Forms.ListView listViewRemote;
        private System.Windows.Forms.ColumnHeader NameColHeader;
        private System.Windows.Forms.ColumnHeader TypeColHeader;
        private System.Windows.Forms.ColumnHeader LastModColHeader;
        private System.Windows.Forms.Button remoteRefreshButton;
        private System.Windows.Forms.ProgressBar heartbeatProgressBar;
        private System.Windows.Forms.Label heartbeatProgLabel;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label LocalPanelLabel;
        private System.Windows.Forms.Label remotePanelLabel;
        private System.Windows.Forms.Button abortButton;
    }
}

