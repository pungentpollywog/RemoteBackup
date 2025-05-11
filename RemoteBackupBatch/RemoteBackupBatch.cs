using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using RemoteBackupLib;
using System.ComponentModel;
using System.Threading;

namespace RemoteBackupBatch {
    class RemoteBackupBatch {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Configuration config = null;
        private Chilkat.Ftp2 ftp = null;
        // private bool m_bgRunning = false;
        // private bool m_abort = false;
        private string m_ftpLastError = "";
        private string m_ftpSessionLog = "";
        private BackgroundWorker m_bgWorker = new BackgroundWorker();
        private string userId = null;
        private string password = null;
        private string serverName = null;
        private string port = null;
        private bool useFtps = false;
        private bool includeDesktop = false;
        private bool includeDocuments = false;
        private bool includeFavorites = false;
        private AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private bool includeCustomFolders = true;

        private void initialize() {
            config = new Configuration();
            config.queryUserConfigFile();
            config.queryAppConfigFile();
            setLocalAttributesFromConfig();
            config.populatePrimaryFoldersList();
            if (this.includeCustomFolders) {
                config.addCustomFoldersToPrimaryFoldersList();
            }
            // Setup ftp component
            ftp = new Chilkat.Ftp2();
            ftp.UnlockComponent("CHRISTFTP_aQwoowLG5Uno");  // License key for Chilkat.Ftp2 (ChilkatDotNet4)
            // Setup background worker
            m_bgWorker.WorkerReportsProgress = true;
            m_bgWorker.WorkerSupportsCancellation = true;
            m_bgWorker.DoWork += new DoWorkEventHandler(m_bgWorker_DoWork);
            m_bgWorker.ProgressChanged += new ProgressChangedEventHandler(m_bgWorker_ProgressChanged);
            m_bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_bgWorker_RunWorkerCompleted);
        }

        private void logStartup() {
            logger.Info("");
            logger.Info("--------------------------------------------------------------");
            logger.Info("RemoteBackup Batch starting up.");
            logger.Info("--------------------------------------------------------------");
        }

        private void m_bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            try {
                // Display the error information and session log.
                // If the operation succeeded, the last error text will provide information about
                // the operation -- the presence of content does not indicate an error.
                bool success = (bool)e.Result;
                if (success) {
                    string msg = "Completed successfully!";
                    logger.Info(msg);
                    Console.WriteLine(msg);
                } else {
                    logger.Error(m_ftpLastError);
                    logger.Error(m_ftpSessionLog);
                    Console.WriteLine("Completed with errors.  Check log file.");
                }
                // clean-up.
                m_bgWorker.Dispose(); // Done.
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void m_bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            try {
                if (e.ProgressPercentage == 0) {
                    // This is an AbortCheck callback...
                    // int value = heartbeatProgressBar.Value;
                    Console.Write(".");
                } else {
                    // This is called from an OnFtpPercentDone event callback.
                    int percentDone = e.ProgressPercentage;
                    Console.Write(" "+percentDone+"% ");
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        /** This will backup the folders specified by FtpInstruct to the FTP server.
         */ 
        private void m_bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            try {
                // Indicate that we are in the background thread
                // The FTP object's event callback will need this.
                //m_bgRunning = true;
                // We passed the filename as an argument.
                // However, we could've just as well accessed the filename
                // via the txtFilename textbox control.
                FtpInstruction ftpInstruct = e.Argument as FtpInstruction;
                // This causes both AbortCheck and FtpPercentDone events to
                // fire.  The callback methods are m_ftp_OnAbortCheck and
                // m_ftp_OnFtpPercentDone.  These methods in turn call the
                // m_bgWorker.ReportProgress method, causing the
                // m_bgWorker_ProgressChanged to be called.  The progress
                // bars can only be updated from a background thread
                // in the m_bgWorker_ProgressChanged event.  The same applies
                // to any other Form controls.  
                FtpTunnel tunnel = new FtpTunnel(ftpInstruct);
                bool success = tunnel.backupFilesToFTPServer();
                e.Result = success;
                // Display the last-error and session log regardless of success/failure...
                m_ftpLastError = ftp.LastErrorText;
                m_ftpSessionLog = ftp.SessionLog;
                _resetEvent.Set(); // signal that the worker is done.
                //m_bgRunning = false;
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
            return;
        }

        /** This can be called in the main thread.
         */ 
        private void synchronizeToRemoteFtp(FtpInstruction ftpInstruct) {
            try {
                FtpTunnel tunnel = new FtpTunnel(ftpInstruct);
                bool success = tunnel.backupFilesToFTPServer();
                // Display the last-error and session log regardless of success/failure...
                if (success == false) {
                    logger.Error(ftp.LastErrorText);
                    logger.Error(ftp.SessionLog);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void setLocalAttributesFromConfig() {
            userId = config.UserId;
            password = config.Password;
            serverName = config.Server;
            port = config.Port;
            useFtps = config.UseFtps;
            includeDesktop = config.IncludeDesktop;
            includeDocuments = config.IncludeDocuments;
            includeFavorites = config.IncludeFavorites;
        }

        private void populateSpecialFoldersList(FtpInstruction ftpInstruct) {
            try {
                if (ftpInstruct.LocalSpecialFolders == null) {
                    ftpInstruct.LocalSpecialFolders = new List<string>();
                }
                echo("Synchronizing the following folder to the remote ftp server "+this.serverName+":");
                foreach (string path in config.PrimaryFolders) {
                    echo(path);
                    ftpInstruct.LocalSpecialFolders.Add(path);
                }
            } catch (Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
        }

        private void echo(string message) {
            Console.WriteLine(message);
            logger.Info(message);
        }

        public bool connectToFtpServer() {
            try {
                if (ftp.IsConnected == true) {
                    logger.Debug("Already connected.  Skipping connect.");
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
                ftp.Hostname = this.serverName;
                int port = 21;
                try {
                    int.TryParse(this.port, out port);
                } catch (Exception ex) {
                    logger.Error("Could not parse port value of " + this.port);
                    logger.Error("Defaulting to port 21.");
                    logger.Error("Exception " + ex.Message + " " + ex.StackTrace);
                }
                ftp.Port = port;
                ftp.Username = this.userId;
                ftp.Password = this.password;

                // Session logging is not required, but I'm turning it on here to see
                // what transpires during the FTP session.  The session log provides the
                // unencrypted FTP requests and responses.
                ftp.KeepSessionLog = true;

                // Connect and login to the FTP server.  This establishs a control connection
                // to the FTP server (port 21), converts the channel to TLS (because AuthTls is true),
                // and then authenticates.
                bool success = false;
                int numAttempts = 0;
                int maxAttempts = 3;
                do {
                    logger.Debug("Attempting to connect to remote FTP server.");
                    success = ftp.Connect();
                    if (success == false) {
                        logger.Error(ftp.SessionLog);
                        logger.Error(ftp.LastErrorText);
                        int delay = 1000*30*RandomNumber(1, 5); // A random delay between 30 secs and 2.5 mins.
                        logger.Error("-----------------------------");
                        logger.Error("Retry after "+delay/1000+" seconds");
                        logger.Error("-----------------------------");
                        Thread.Sleep(delay);
                    }
                } while (success == false && ++numAttempts < maxAttempts);

                if (success==false) {
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

        private int RandomNumber(int min, int max) {
            Random random = new Random();
            return random.Next(min, max);
        }


        private void doBackup() {
            logger.Trace("doBackup starting...");
            try {
                // Instantiate and initialize ftpInstruction
                FtpInstruction ftpInstruct = new FtpInstruction();
                ftpInstruct.Ftp = ftp;
                ftpInstruct.IsUpload = true;
                ftpInstruct.Hostname = serverName;
                ftpInstruct.Port = port;
                ftpInstruct.Username = userId;
                ftpInstruct.Password = password;
                ftpInstruct.UseFtps = useFtps;
                // Set list of available special folders
                populateSpecialFoldersList(ftpInstruct);
                // Add the special folders to the list of folders to be backed-up.
                ftpInstruct.prepareFtpInstructionsFromSpecialFolders();
                // Backup in a background task
                //m_bgRunning = true;
                //m_bgWorker.RunWorkerAsync(ftpInstruct); // fires DoWork
                // Run in main thread:
                synchronizeToRemoteFtp(ftpInstruct);
                logger.Trace("doBackup exiting...");
            } catch(Exception ex) {
                logger.Error("Exception in doBackup.");
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
            }
        }

        static void Main(string[] args) {
            // CDD-TODO: use a different log file for the batch.  Need to add an appender in NLog.config
            try {
                RemoteBackupBatch batch = new RemoteBackupBatch();
                batch.config = new Configuration();
                batch.logStartup();
                batch.initialize();
                if (batch.connectToFtpServer()) {
                    batch.doBackup();
                    // Use the following if executing the backup in a background worker.  Otherwise, don't need to block.
                    // batch._resetEvent.WaitOne(); // blocks until _resetEvent.Set() call is made.
                    Console.WriteLine();
                    batch.echo("RemoteBackupBatch exiting successfully.");
                } else {
                    logger.Error("Could not connect to the remote FTP server.");
                }
            } catch (Exception ex) {
                logger.Error("Exception in Remote Backup Batch.");
                logger.Error(ex.Message);
                logger.Error(ex.StackTrace);
            }
        }
    }
}
