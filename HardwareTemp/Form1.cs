using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
//Monitoring library
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
//Serial
using System.IO;
using System.IO.Ports;


namespace HardwareTemp {

    public partial class Form1 : Form {


        // Configuration
        public static bool twoGPUs = false;
        public static int numberOfCores = Environment.ProcessorCount;


        //Serial Configuration
        public static bool Arduino_Enabled = false;
        public static string COM_Port = "NullCOM";
        public static int Baud_Rate = 115200;


        //Configuration File
        string current_Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        string filename = "\\settings.ini";


        Computer cpt = new Computer();

        public static int[,] data;

        bool mouseDown;



        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Load_Config();
            Pop_Data();            
            InitComp(cpt);
            timer1.Start();
        }



        //Main methods

        public void MainLoop() {
            Update_Values(cpt);
            AvgMinMax();
            Display();
            Send_Data();
        }
        
        public static void InitComp(Computer cmp) {
            cmp.Open();
            cmp.CPUEnabled = true;
            cmp.GPUEnabled = true;
        }

        public void Pop_Data() {

            int[,] tmp = new int[3,4];

            for (int y = 0; y < 3; y++)
                for (int x = 0; x < 4; x++)
                    if (x != 2) {
                        tmp[y, x] = 0;
                    }
                    else {
                        tmp[y, x] = 100;
                    }

            data = tmp;
        }

        public static void Update_Values(Computer cmp) {

            data[0,1] = 0;
            data[0,0] = 0;
            int ct = 0;
            
            foreach (var hardware in cmp.Hardware) {

                hardware.Update();

                if (twoGPUs) {
                    if (ct == 0)
                        ct++;
                    else
                        ct--;
                }


                foreach (var sensor in hardware.Sensors) {

                    //CPU temps & Loads
                    if (sensor.Hardware.HardwareType == HardwareType.CPU) {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Value != null && sensor.Name != "CPU Package") {
                            data[0, 1] += (int)sensor.Value;
                        }
                        if (sensor.SensorType == SensorType.Load && sensor.Name != "CPU Total" && sensor.Value != null) {
                            data[0,0] += (int)sensor.Value;
                        }
                    }
                    //GPU Temps & Loads
                    else if (sensor.Hardware.HardwareType == HardwareType.GpuNvidia || sensor.Hardware.HardwareType == HardwareType.GpuAti) {

                        if (twoGPUs) {
                            if (ct != 0) {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                    data[1, 1] = (int)sensor.Value;
                                if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                    data[1, 0] = (int)sensor.Value;
                            }
                            else {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                    data[2, 1] = (int)sensor.Value;
                                if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                    data[2, 0] = (int)sensor.Value;
                            }
                        }
                        else {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                data[1, 1] = (int)sensor.Value;
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                data[1, 0] = (int)sensor.Value;
                        }

                    }
                    
                }
            }


        }

        public static void AvgMinMax() {

            //CPU Average, divided by 4 for a 4-cores CPU
            data[0, 1] = data[0, 1] / numberOfCores;
            data[0,0] = data[0,0] / numberOfCores;

            //MinMax CPU
            if (data[0, 1] < data[0, 2])
                data[0, 2] = data[0, 1];
            if (data[0, 1] > data[0, 3])
                data[0, 3] = data[0, 1];

            //MinMax GPU1
            if (data[1, 1] < data[1, 2])
                data[1, 2] = data[1, 1];
            if (data[1, 1] > data[1,3])
                data[1, 3] = data[1, 1];

            //MinMax GPU2
            if (twoGPUs) {
                if (data[2, 1] < data[2, 2])
                    data[2, 2] = data[2, 1];
                if (data[2, 1] > data[2,3])
                    data[2, 3] = data[2, 1];
            }
        }

        public void Display() {           

            //CPU
            label7.Text = data[0, 1].ToString() + " °C";
            label8.Text = data[0, 2].ToString() + " °C";
            label9.Text = data[0, 3].ToString() + " °C";

            //GPU 1
            label10.Text = data[1, 1].ToString() + " °C";
            label11.Text = data[1, 2].ToString() + " °C";
            label12.Text = data[1, 3].ToString() + " °C";

            //GPU 2
            if (twoGPUs) {
                label13.Text = data[2, 1].ToString() + " °C";
                label14.Text = data[2, 2].ToString() + " °C";
                label15.Text = data[2, 3].ToString() + " °C";
            }

            //Loads
            label17.Text = data[0,0].ToString() + " %";
            label18.Text = data[1, 0].ToString() + " %";
            label19.Text = data[2, 0].ToString() + " %";
        }

        public void Config_Display(bool multi_GPU) {
            if (!multi_GPU) {
                this.Height = 160;
                label6.Hide();
                label19.Hide();
                label13.Hide();
                label14.Hide();
                label15.Hide();
                label5.Text = "GPU";
            }
            else {
                this.Height = 210;
                label6.Show();
                label19.Show();
                label13.Show();
                label14.Show();
                label15.Show();
                label5.Text = "GPU1";
            }
        }

        public void Load_Config() {

            if (File.Exists(current_Path + filename)) {

                using (var stream = new FileStream(current_Path + filename, FileMode.Open)) {
                    using (var reader = new StreamReader(stream)) {

                        try {

                            string work = reader.ReadToEnd();

                            numberOfCores = parse_int( Find_Parameter( work, "Number_of_cores"));
                            twoGPUs = int_to_bool(parse_int( Find_Parameter( work, "TwoGPUs")));

                            Arduino_Enabled = int_to_bool(parse_int( Find_Parameter( work, "Arduino_Enabled")));
                            COM_Port = Find_Parameter( work, "COM_port");
                            Baud_Rate = parse_int( Find_Parameter(work, "Baud_Rate"));

                        }
                        catch {
                            Reset_Settings(4, false, false, "NullCOM", 115200);
                        }
                    }
                }
                //Reset Display when Config changed
                Config_Display(twoGPUs);
            }
            else {
                System.Windows.Forms.MessageBox.Show("Check out the config menu", "First Launch Message");
                Reset_Settings(4, false, false, "NullCOM", 115200);
            }
        }

        public void Reset_Settings(int cores, bool GPUs, bool arduino, string port, int baud) {

            numberOfCores = cores;
            twoGPUs = GPUs;

            Arduino_Enabled = arduino;
            COM_Port = port;
            Baud_Rate = baud;

            using (var stream = new FileStream(current_Path + filename, FileMode.Create)) {
                using (var writer = new StreamWriter(stream)) {

                    //Write General Settings                        
                    writer.WriteLine("General Settings");
                    writer.WriteLine("----------------");
                    writer.WriteLine(Write_Parameter("Number_of_cores", numberOfCores.ToString()));
                    writer.WriteLine(Write_Parameter("TwoGPUs", bool_to_int(twoGPUs).ToString()));

                    writer.WriteLine();

                    //Write Arduino Settings
                    writer.WriteLine("Arduino Settings");
                    writer.WriteLine("----------------");
                    writer.WriteLine(Write_Parameter("Arduino_Enabled", bool_to_int(Arduino_Enabled).ToString()));
                    writer.WriteLine(Write_Parameter("COM_port", COM_Port));
                    writer.WriteLine(Write_Parameter("Baud_Rate", Baud_Rate.ToString()));

                }
            }

            Config_Display(GPUs);

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

        public void Send_Data() {

            if (Arduino_Enabled) {

                try {

                    // Data Format :
                    // CPU_Load / CPU_Temp #   GPU1_Load / GPU1_Temp #   !
                    // or if in two GPUs
                    // CPU_Load / CPU_Temp #   GPU1_Load / GPU1_Temp #   & GPU2_Load / GPU2_Temp # !
                    //Example :
                    //  51/38#32/67#&82/21#!

                    SerialPort curr_port = new SerialPort(COM_Port, Baud_Rate);
                    curr_port.Open();

                    curr_port.Write(data[0, 0].ToString());
                    curr_port.Write("/");
                    curr_port.Write(data[0, 1].ToString());
                    curr_port.Write("#");

                    curr_port.Write(data[1, 0].ToString());
                    curr_port.Write("/");
                    curr_port.Write(data[1, 1].ToString());
                    curr_port.Write("#");

                    if (twoGPUs) {
                        curr_port.Write("&");

                        curr_port.Write(data[2, 0].ToString());
                        curr_port.Write("/");
                        curr_port.Write(data[2, 1].ToString());
                        curr_port.Write("#");
                    }

                    curr_port.Write("!");
                    curr_port.Close();
                }
                catch {
                    Reset_Settings(numberOfCores, twoGPUs, false, COM_Port, Baud_Rate); //Just disable Arduino link if it fails to send data through Serial port
                    Load_Config();
                }
            }
        }



        //Secondary methods

        public bool int_to_bool(int value) {
            if (value == 0) {
                return false;
            }
            else {
                return true;
            }
        }

        public int bool_to_int(bool enabled) {
            if (enabled) {
                return 1;
            }
            else {
                return 0;
            }
        }

        public int parse_int(string text) {

            int r = 0;

            if (Int32.TryParse(text, out r)) {
                return r;
            }
            return 0;
        }



        //Events

        /*
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {

            SerialPort sp = (SerialPort)sender;
            byte[] buffer = new byte[sp.BytesToRead];
            sp.Read(buffer, 0, sp.BytesToRead);

            //Send_Data();

            if (buffer[0] == 64 && buffer[1] == 80 && buffer[2] == 67) { //If data received = "@PC", then send data
                Send_Data();
            }

        }
        */

        private void timer1_Tick(object sender, EventArgs e) {
            MainLoop();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e) {
            mouseDown = true;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e) {
            if (mouseDown) {
                this.SetDesktopLocation(MousePosition.X - 275, MousePosition.Y - 105);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e) {
            mouseDown = false;
        }

        private void label20_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void label21_Click(object sender, EventArgs e) {
            this.WindowState = FormWindowState.Minimized;
            notifyIcon1.Visible = true;
            this.ShowInTaskbar = false;            
        }

        private void label22_Click(object sender, EventArgs e) {
            Settings settingsForm = new Settings(this);
            settingsForm.ShowDialog();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
            this.ShowInTaskbar = true;
        }
            

 

    }
}
