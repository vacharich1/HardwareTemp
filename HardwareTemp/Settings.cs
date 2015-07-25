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



        //Main methods

        public void Read_Settings() {

            if (!File.Exists(current_Path + filename)) {
                File.Create(current_Path + filename);
            }

            using (var stream = new FileStream(current_Path + filename, FileMode.Open)) {
                using (var reader = new StreamReader(stream)) {

                    try {

                        string work = reader.ReadToEnd();

                        numericUpDown1.Value = parse_int(Find_Parameter(work, "Number_of_cores"));
                        checkBox2.Checked = int_to_bool(parse_int(Find_Parameter(work, "TwoGPUs")));

                        checkBox1.Checked = int_to_bool(parse_int(Find_Parameter(work, "Arduino_Enabled")));
                        label8.Text = Find_Parameter(work, "COM_port");
                        numericUpDown2.Value = parse_int(Find_Parameter(work, "Baud_Rate"));

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
                    writer.WriteLine( Write_Parameter( "Number_of_cores", numericUpDown1.Value.ToString() ) );
                    writer.WriteLine( Write_Parameter("TwoGPUs", bool_to_int(checkBox2.Checked).ToString()));

                    writer.WriteLine();

                    //Write Arduino Settings
                    writer.WriteLine("Arduino Settings");
                    writer.WriteLine( Write_Parameter("Arduino_Enabled", bool_to_int(checkBox1.Checked).ToString()));
                    writer.WriteLine( Write_Parameter( "COM_port", label8.Text ) );
                    writer.WriteLine( Write_Parameter( "Baud_Rate", numericUpDown2.Value.ToString() ) );

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

        public static string Find_Parameter(string input, string parameter_name) {

            int param_index = input.IndexOf(parameter_name);

            while (input[param_index] != '<') {
                param_index++;
            }
            param_index++;

            string ret = "";
            while (input[param_index] != '>') {
                ret += input[param_index];
                param_index++;
            }

            return ret;
        }

        public static string Write_Parameter(string param_name, string param_data) {
            return (param_name + " = " + "<" + param_data + ">");
        }
        


        //Secondary methods

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

        public int bool_to_int(bool enabled) {
            if (enabled) {
                return 1;
            }
            else {
                return 0;
            }
        }

        public bool int_to_bool(int value) {
            if (value == 0) {
                return false;
            }
            else {
                return true;
            }
        }
        


        //Events

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
