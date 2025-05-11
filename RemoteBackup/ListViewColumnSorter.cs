using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using NLog;

namespace RemoteBackup {
    class ListViewColumnSorter : IComparer {

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private CaseInsensitiveComparer ObjectCompare;

        public ListViewColumnSorter() {
            // Initialize the CaseInsensitiveComparer object
            ObjectCompare = new CaseInsensitiveComparer();
        }

        /// <summary>
        /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
        /// </summary>
        /// <param name="x">First object to be compared</param>
        /// <param name="y">Second object to be compared</param>
        /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
        public int Compare(object x, object y) {
            int compareResult=0;
            string directory = "Directory";
            ListViewItem listviewX, listviewY;
            // Cast the objects to be compared to ListViewItem objects
            listviewX = (ListViewItem)x;
            listviewY = (ListViewItem)y;
            try {
                if (listviewX != null && listviewX.SubItems[0] != null && listviewY != null && listviewY.SubItems[0] != null) {
                    // Compare the two items
                    if (listviewX.SubItems[1] != null && listviewY.SubItems[1] != null &&
                        listviewX.SubItems[1].Text.Equals(directory) && listviewY.SubItems[1].Text.Equals(directory) == false) {
                        return -1;
                    } else if (listviewX.SubItems[1] != null && listviewY.SubItems[1] != null &&
                        listviewX.SubItems[1].Text.Equals(directory) == false && listviewY.SubItems[1].Text.Equals(directory)) {
                        return 1;
                    } else {
                        if (listviewX.SubItems[0].Text!=null && listviewY.SubItems[0].Text!=null) {
                            compareResult = ObjectCompare.Compare(listviewX.SubItems[0].Text, listviewY.SubItems[0].Text);
                        }
                    }
                }
            } catch(Exception ex) {
                logger.Error("Exception: " + ex.Message + " " + ex.StackTrace);
            }
            return compareResult;
        }

    }
}
