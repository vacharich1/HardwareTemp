using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;

namespace HardwareTemp {

    public partial class Settings : Form {

        private Form1 form_parent = null;

        string current_Path;
        string filename = "\\settings.ini";


        public Settings(Form1 f) {
            form_parent = f;
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e) {
            current_Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            label6.Text = "Suggested : " + Environment.ProcessorCount.ToString();
            Read_Settings();
            ScanCOM();
        }



        public void Read_Settings() {

            if (!File.Exists(current_Path + filename)) {
                File.Create(current_Path + filename);
            }

            using (var stream = new FileStream(current_Path + filename, FileMode.Open)) {
                using (var reader = new StreamReader(stream)) {

                    try {
                        //General Settings
                        reader.ReadLine();
                        numericUpDown1.Value = parse_int( reader.ReadLine() );
                        checkBox2.Checked = checkbox_read( parse_int( reader.ReadLine() ));

                        //Arduino Settings
                        reader.ReadLine();
                        checkBox1.Checked = checkbox_read( parse_int( reader.ReadLine() ));
                        label8.Text = reader.ReadLine();
                        numericUpDown2.Value = parse_int( reader.ReadLine() );

                    }
                    catch {
                        Reset_Settings();
                    }
                }
            }
        }

        public void Write_Settings() {

            if(!File.Exists(current_Path + filename)) {
                File.Create(current_Path + filename);
            }

            using (var stream = new FileStream(current_Path + filename, FileMode.Truncate)) {
                using (var writer = new StreamWriter(stream)) {

                    //Write General Settings
                    writer.WriteLine("General Settings");
                    writer.WriteLine(numericUpDown1.Value.ToString());
                    writer.WriteLine(checkbox_write(checkBox2.Checked));
                    //Write Arduino Settings
                    writer.WriteLine("Arduino Settings");
                    writer.WriteLine(checkbox_write(checkBox1.Checked));
                    writer.WriteLine(label8.Text);
                    writer.WriteLine(numericUpDown2.Value.ToString());

                }
            }


        }

        public void Reset_Settings() {

            //General Settings
            numericUpDown1.Value = 4;
            checkBox2.Checked = false;

            //Arduino Settings
            checkBox1.Checked = false;
            label8.Text = null;
            numericUpDown2.Value = 115200;

        }


        public void ScanCOM() {
            listBox1.Items.Clear();
            string[] scanned = SerialPort.GetPortNames();
            foreach (string s in scanned) {
                listBox1.Items.Add(s);
            }
        }

        public int parse_int(string text) {

            int r = 0;

            if (Int32.TryParse(text, out r)) {
                return r;
            }
            return 0;
        }

        public int checkbox_write(bool enabled) {
            if (enabled) {
                return 1;
            }
            else {
                return 0;
            }
        }

        public bool checkbox_read(int value) {
            if (value == 0) {
                return false;
            }
            else {
                return true;
            }
        }



        private void button1_Click(object sender, EventArgs e) {
            Write_Settings();
            form_parent.Load_Config();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            label8.Text = listBox1.GetItemText(listBox1.SelectedItem);
        }

        private void label8_Click(object sender, EventArgs e) {
            label8.Text = "";
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e) {
            label8.Text = listBox1.GetItemText(listBox1.SelectedItem);
        }


    }
}
