using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using NLog;
// using System.Xml.Linq;

namespace RemoteBackupLib {
    public class ConfigFile {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        XmlDocument xmlDoc = null;

        string fullFileName = null;

        public string ConfigPath {
            get { return fullFileName; }
            set { fullFileName = value; }
        }

        public ConfigFile(string path) {
            this.fullFileName = path;
            xmlDoc = new XmlDocument();
        }

        public bool tryLoadConfigDoc() {
            bool success = false;
            try {
                if (ConfigPath == null || ConfigPath.Trim().Length == 0) {
                    throw new Exception("Config path for the XML settings file has not been set.");
                }
                // Now that we have a XmlDoc and a ConfigPath, try to load the file into memory.
                xmlDoc.Load(ConfigPath);
                success = true; // got this far so must be good.
            } catch (Exception e) {
                logger.Error("Exception in tryLoadConfigDoc: " + e.Message);
            }
            return success;
        }

        /** If there is more than one element with the same name under //settings, 
         * this will return a list of the values for those elements.
         */
        public List<string> getElementValues(string elementName) {
            logger.Trace("Looking for element(s) named " + elementName);
            List<string> values = new List<string>();
            string val = null;
            XmlNodeList nodeList = xmlDoc.SelectNodes("//settings");
            foreach (XmlNode node in nodeList) {
                XmlNodeList innerNodeList = node.SelectNodes(elementName);
                foreach (XmlNode xn in innerNodeList) {
                    if (xn != null) {
                        val = xn.InnerText;
                        logger.Trace("element value: " + val);
                        values.Add(val);
                    }
                }
            }
            return values;
        }

        public string getElementValue(string elementName) {
            logger.Trace("Looking for element named " + elementName);
            string value = null;
            XmlNodeList nodeList = xmlDoc.SelectNodes("//settings");
            foreach (XmlNode node in nodeList) {
                XmlNode xn = node.SelectSingleNode(elementName);
                if (xn != null) {
                    value = xn.InnerText;
                    logger.Trace("element value: "+value);
                }
            }
            return value;
        }

        public string getElementAttributeValue(string elementName, string attributeName) {
            logger.Trace("getElementAttributeValue starting...");
            string attrValue = null;
            try {
                XmlNode node = xmlDoc.SelectSingleNode(elementName); // e.g. "//settings/server"
                if (node != null) {
                    logger.Trace("Node name: " + node.Name);
                    foreach (XmlAttribute attr in node.Attributes) {
                        if (attr.Name.Equals(attributeName)) {
                            attrValue = attr.Value;
                            break;
                        }
                    }
                }
                logger.Trace("attrValue: " + attrValue);
            } catch(Exception e) {
                logger.Error("Exception in getElementAttributeValue"+e.Message+" \n"+e.StackTrace);
            }
            return attrValue;
        }

        public void setElementAttributeValue(string elementName, string attributeName, string newAttrValue) {
            XmlNode node = xmlDoc.SelectSingleNode(elementName); // e.g. "//settings/server"
            try {
                if (node != null) {
                    for (int idx = 0; idx < node.Attributes.Count; idx++) {
                        if (node.Attributes[idx].Name.Equals(attributeName)) {
                            node.Attributes[idx].Value = newAttrValue;
                            xmlDoc.Save(fullFileName);
                            break;
                        }
                    }
                }
            } catch (Exception e) {
                logger.Error("Exception in setElementAttributeValue" + e.Message + " \n" + e.StackTrace);
            }
        }

    }
}
