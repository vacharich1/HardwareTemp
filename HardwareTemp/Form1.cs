﻿using System;
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
        public static bool twoGPUs = true;
        public static int numberOfCores = 4;
        //End of configuration

        //Serial Configuration
        public static bool Arduino_Enabled = false;
        public static string COM_Port = "NullCOM";
        public static int Baud_Rate = 115200;
        //End of Serial configuration
        
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
            Config_Display(twoGPUs);
            Pop_Data();            
            InitComp(cpt);
            timer1.Start();
        }



        //Main methods

        public void MainLoop() {
            Update_Values(cpt);
            AvgMinMax();
            Display();
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
                            //General Settings
                            reader.ReadLine();
                            numberOfCores = parse_int(reader.ReadLine());
                            twoGPUs = int_to_bool(parse_int(reader.ReadLine()));

                            //Arduino Settings
                            reader.ReadLine();
                            Arduino_Enabled = int_to_bool(parse_int(reader.ReadLine()));
                            COM_Port = reader.ReadLine();
                            Baud_Rate = parse_int(reader.ReadLine());

                        }
                        catch {
                            Reset_Settings();
                        }
                    }
                }
                //Reset Display when Config changed
                Config_Display(twoGPUs);
            }
            else {

                System.Windows.Forms.MessageBox.Show("Please go in the config menu to set the number of cores your computer has.", "First Launch Message");

                Reset_Settings();

                using (var stream = new FileStream(current_Path + filename, FileMode.Create)) {
                    using (var writer = new StreamWriter(stream)) {

                        //Write General Settings
                        writer.WriteLine("General Settings");
                        writer.WriteLine(numberOfCores.ToString());
                        writer.WriteLine(bool_to_int(twoGPUs).ToString());
                        //Write Arduino Settings
                        writer.WriteLine("Arduino Settings");
                        writer.WriteLine(bool_to_int(Arduino_Enabled));
                        writer.WriteLine(COM_Port);
                        writer.WriteLine(Baud_Rate.ToString());

                    }
                }
            }
        }

        public static void Send_Data() {
            if (Arduino_Enabled) {
                SerialPort curr_port = new SerialPort(COM_Port, Baud_Rate);
                curr_port.Open();
                curr_port.Write("data protocol");
                curr_port.Close();
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

        public void Reset_Settings() {

            //General
            numberOfCores = 4;
            twoGPUs = false;

            //Arduino
            Arduino_Enabled = false;
            COM_Port = "NullCOM";
            Baud_Rate = 115200;

        }




        //Events

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {

            SerialPort sp = (SerialPort)sender;
            byte[] buffer = new byte[sp.BytesToRead];
            sp.Read(buffer, 0, sp.BytesToRead);

            if (buffer[0] == 64 && buffer[1] == 80 && buffer[2] == 67) { //If data received = "@PC", then send data
                Send_Data();
            }

        }

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
