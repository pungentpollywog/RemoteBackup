using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemoteBackup {
    class RadioButtonManager {

        private List<string> options = null;
        private List<RadioButton> radioButtons = null;

        public List<RadioButton> RadioButtons {
            get { return radioButtons; }
            set { radioButtons = value; }
        }

        public List<string> Options {
            get { return options; }
            set { options = value; }
        }

        public RadioButtonManager() {
            options = new List<string>();
            radioButtons = new List<RadioButton>();
        }

        public void generateRadioButtonsFromOptions() {
            bool first = true;
            if (options != null) {
                // options.Sort();
                options.Sort((x, y) => string.Compare(x, y));
                foreach (string option in options) {
                    Console.WriteLine("option: "+option);
                    RadioButton radio = new RadioButton();
                    radio.Dock = DockStyle.Top;
                    radio.Text = option;
                    if (first) {
                        radio.Checked = true;
                        first = false;
                    }
                    RadioButtons.Add(radio);
                }
            }
        }


    }
}
