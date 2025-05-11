using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.IO;

namespace RemoteBackupLib {

    public class Configuration {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static string userConfigFileName = "Remote Backup\\RemoteBackupUserConfig.xml";
        private static string appConfigFileName = "Remote Backup\\RemoteBackup\\RemoteBackupAppConfig.xml";
        private ConfigFile userConfigFile = null;
        private ConfigFile appConfigFile = null;
        private List<string> primaryFolders = null;

        public List<string> PrimaryFolders {
            get { return primaryFolders; }
            set { primaryFolders = value; }
        }

        private List<string> customFolders = null;

        public List<string> CustomFolders {
            get { return customFolders; }
            set { customFolders = value; }
        }

        private string userId;

        public string UserId {
            get { return userId; }
            set { userId = value; }
        }
        private string password;

        public string Password {
            get { return password; }
            set { password = value; }
        }
        private string server;

        public string Server {
            get { return server; }
            set { server = value; }
        }
        private string port;

        public string Port {
            get { return port; }
            set { port = value; }
        }
        private bool includeDesktop;

        public bool IncludeDesktop {
            get { return includeDesktop; }
            set { includeDesktop = value; }
        }
        private bool includeDocuments;

        public bool IncludeDocuments {
            get { return includeDocuments; }
            set { includeDocuments = value; }
        }
        private bool includeFavorites;

        public bool IncludeFavorites {
            get { return includeFavorites; }
            set { includeFavorites = value; }
        }
        private bool useFtps;

        public bool UseFtps {
            get { return useFtps; }
            set { useFtps = value; }
        }
        private string localPanelLabel;

        public string LocalPanelLabel {
            get { return localPanelLabel; }
            set { localPanelLabel = value; }
        }
        private string remotePanelLabel;

        public string RemotePanelLabel {
            get { return remotePanelLabel; }
            set { remotePanelLabel = value; }
        }

        public Configuration() {
        } 

        public void queryUserConfigFile() {
            try {
                string userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                logger.Debug("userConfigPath: " + userConfigPath);
                String fullFileName = Path.Combine(userConfigPath, userConfigFileName);
                userConfigFile = new ConfigFile(fullFileName);
                if (userConfigFile.tryLoadConfigDoc()) {
                    string userId = userConfigFile.getElementAttributeValue("//settings/userId", "value");
                    if (userId != null && userId.Trim().Length > 0) {
                        this.UserId = userId;
                    }
                    string password = userConfigFile.getElementAttributeValue("//settings/password", "value");
                    if (password != null && password.Trim().Length > 0) {
                        this.Password = password;
                    }
                } else {
                    logger.Error("Could not load user config file: " + userConfigPath);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void queryAppConfigFile() {
            try {
                logger.Trace("queryAppConfigFile starting...");
                string appConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                logger.Debug("Application configPath: " + appConfigPath);
                string fullFileName = Path.Combine(appConfigPath, appConfigFileName);
                appConfigFile = new ConfigFile(fullFileName);
                if (appConfigFile.tryLoadConfigDoc()) {
                    string serverName = appConfigFile.getElementAttributeValue("//settings/server", "value");
                    logger.Debug("serverName: " + serverName);
                    if (serverName != null && serverName.Trim().Length > 0) {
                        this.Server = serverName;
                    }
                    string port = appConfigFile.getElementAttributeValue("//settings/port", "value");
                    if (port != null && port.Trim().Length > 0) {
                        this.Port = port;
                    }
                    // Pull the value of use-ftps.  If true, set a flag in ftpInstruct and use ftps.
                    string strUseFtps = appConfigFile.getElementAttributeValue("//settings/useFtps", "value");
                    bool useFtps = false;
                    Boolean.TryParse(strUseFtps, out useFtps);
                    this.UseFtps = useFtps;
                    logger.Debug("useFtps: " + useFtps);
                    string localPanelLabel = appConfigFile.getElementAttributeValue("//settings/localPanelLabel", "value");
                    if (localPanelLabel != null && localPanelLabel.Trim().Length > 0) {
                        this.LocalPanelLabel = localPanelLabel;
                    }
                    string remotePanelLabel = appConfigFile.getElementAttributeValue("//settings/remotePanelLabel", "value");
                    if (remotePanelLabel != null && remotePanelLabel.Trim().Length > 0) {
                        this.RemotePanelLabel = remotePanelLabel;
                    }

                    List<string> customFolders = appConfigFile.getElementValues("customFolder");
                    if (CustomFolders == null) {
                        CustomFolders = new List<string>();
                    }
                    foreach (string folderName in customFolders) {
                        logger.Debug("customFolder: " + folderName);
                        if (folderExistsLocally(folderName)) {
                            logger.Debug("Folder exists.  Adding to CustomFolders.");
                            CustomFolders.Add(folderName);
                        } else {
                            logger.Debug("Folder does not exist in local file system.  Excluding from CustomFolders.");
                        }
                    }

                    //string strincludeFavorites = appConfigFile.getElementAttributeValue("//settings/includeFavorites", "value");
                    //Boolean.TryParse(strincludeFavorites, out includeFavorites);
                    bool includeDesktop = false;
                    setBooleanFromConfigAttribute(out includeDesktop, "//settings/includeDesktop");
                    this.IncludeDesktop = includeDesktop;
                    bool includeDocuments = false;
                    setBooleanFromConfigAttribute(out includeDocuments, "//settings/includeDocuments");
                    this.IncludeDocuments = includeDocuments;
                    bool includeFavorites = false;
                    setBooleanFromConfigAttribute(out includeFavorites, "//settings/includeFavorites");
                    this.IncludeFavorites = includeFavorites;
                } else {
                    logger.Error("Could not load app config file: " + appConfigPath);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private bool folderExistsLocally(string fullFolderName) {
            bool exists = false;
            FileInfo file = new FileInfo(fullFolderName);
            if (file!=null) {
                exists = Directory.Exists(fullFolderName);
            }
            return exists;
        }

        /** Default value is true.  If false in the config file, it will be set to false.
         */
        private void setBooleanFromConfigAttribute(out bool boolToSet, string attrName) {
            boolToSet = true;
            string strincludeFavorites = appConfigFile.getElementAttributeValue(attrName, "value");
            if (strincludeFavorites == null || strincludeFavorites.Trim().Length == 0) {
                logger.Debug(attrName + " not found.");
            } else {
                Boolean.TryParse(strincludeFavorites, out boolToSet);
                logger.Debug(attrName + " set to " + boolToSet.ToString());
            }
        }

        /** Saves passed in userId and password to the RemoteBackupUserConfig.xml file.
         */
        public void saveSettingsToConfigFile(string userId, string password) {
            try {
                // Save the credentials used to login.
                // Update userId if it is different that what's in the config file.
                string userIdInConfig = userConfigFile.getElementAttributeValue("//settings/userId", "value");
                if (userIdInConfig != null && userIdInConfig.Equals(userId) == false) {
                    userConfigFile.setElementAttributeValue("//settings/userId", "value", userId);
                }
                // Update password if it is different that what's in the config file.
                string passwordInConfig = userConfigFile.getElementAttributeValue("//settings/password", "value");
                if (passwordInConfig != null && passwordInConfig.Equals(password) == false) {
                    userConfigFile.setElementAttributeValue("//settings/password", "value", password);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void populatePrimaryFoldersList() {
            if (this.PrimaryFolders == null) {
                PrimaryFolders = new List<string>();
            }
            // Include the special folders conditionally based on the app config file.
            if (includeDesktop) {
                PrimaryFolders.Add(getDesktopPath());
            }
            if (includeDocuments) {
                PrimaryFolders.Add(getDocumentsPath());
            }
            if (includeFavorites) {
                PrimaryFolders.Add(getFavoritesPath());
            }
        }

        public void addCustomFoldersToPrimaryFoldersList() {
            foreach (string path in this.CustomFolders) {
                if (path != null && path.Length > 0) {
                    if (folderExistsLocally(path)) {
                        PrimaryFolders.Add(path);
                    }
                }
            }
        }

        private string getDesktopPath() {
            //string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //string pathDesktop = Path.Combine(pathUser, "Desktop");
            //return pathDesktop;
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private string getDocumentsPath() {
            //logIt("Favorities: \t\t" + Environment.GetFolderPath(Environment.SpecialFolder.Favorites));
            //logIt("(My) Documents: \t\t" + System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            //logIt("Desktop: \t\t\t" + Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            //logIt("DesktopDirectory: \t\t" + Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
            //logIt("Common Documents: \t" + Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments));
            //logIt("Personal: \t\t\t"+ Environment.GetFolderPath(Environment.SpecialFolder.Personal));
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        private string getFavoritesPath() {
            return Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
        }

    }
}
