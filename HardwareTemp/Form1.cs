﻿//  /u/FestiveCore  for  /r/pcmasterrace
//
// Requirements :
// - OpenHardwareMonitor Library
// 
// If you are copying the project into Visual Studio, you will need 19 labels.
// If you don't have a system with a CPU with 4 cores and two GPUs, check the Configuration in the variables.
//
// The timer is used for the main loop as WinForms is an event driven achitecture.


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

namespace HardwareTemp {

    public partial class Form1 : Form {


        // Configuration
        public static bool twoGPUs = true;
        public static int numberOfCores = 4; //Number of logical cores, if your CPU doesn't have hyperthreading, it's the same as the number of physical core, otherwise it's doubled
        //End of configuration

        Computer cpt = new Computer();

        public static int CPU_cur = 0;
        public static int CPU_min = 100;
        public static int CPU_max = 0;

        public static int GPU1_cur = 0;
        public static int GPU1_min = 100;
        public static int GPU1_max = 0;

        public static int GPU2_cur = 0;
        public static int GPU2_min = 100;
        public static int GPU2_max = 0;

        public static int CPU_Load = 0;
        public static int GPU1_Load = 0;
        public static int GPU2_Load = 0;

        bool mouseDown;

        public Form1() {
            InitializeComponent();

            //Hide second GPU label if on a single GPU system.
            if (!twoGPUs) {
                this.Height = 160;
                label6.Hide();
                label19.Hide();
                label13.Hide();
                label14.Hide();
                label15.Hide();

                label5.Text = "GPU";
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            timer1.Start();
            InitComp(cpt);
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

        public static void Update_Values(Computer cmp) {

            CPU_cur = 0;
            CPU_Load = 0;
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
                            CPU_cur += (int)sensor.Value;
                        }
                        if (sensor.SensorType == SensorType.Load && sensor.Name != "CPU Total" && sensor.Value != null) {
                            CPU_Load += (int)sensor.Value;
                        }
                    }
                    //GPU Temps & Loads
                    else if (sensor.Hardware.HardwareType == HardwareType.GpuNvidia || sensor.Hardware.HardwareType == HardwareType.GpuAti) {

                        if (twoGPUs) {
                            if (ct != 0) {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                    GPU1_cur = (int)sensor.Value;
                                if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                    GPU1_Load = (int)sensor.Value;
                            }
                            else {
                                if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                    GPU2_cur = (int)sensor.Value;
                                if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                    GPU2_Load = (int)sensor.Value;
                            }
                        }
                        else {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                GPU1_cur = (int)sensor.Value;
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                GPU1_Load = (int)sensor.Value;
                        }

                    }
                    
                }
            }


        }

        public static void AvgMinMax() {

            //CPU Average, divided by 4 for a 4-cores CPU
            CPU_cur = CPU_cur / numberOfCores;
            CPU_Load = CPU_Load / numberOfCores;

            //MinMax CPU
            if (CPU_cur < CPU_min)
                CPU_min = CPU_cur;
            if (CPU_cur > CPU_max)
                CPU_max = CPU_cur;

            //MinMax GPU1
            if (GPU1_cur < GPU1_min)
                GPU1_min = GPU1_cur;
            if (GPU1_cur > GPU1_max)
                GPU1_max = GPU1_cur;

            //MinMax GPU2
            if (twoGPUs) {
                if (GPU2_cur < GPU2_min)
                    GPU2_min = GPU2_cur;
                if (GPU2_cur > GPU2_max)
                    GPU2_max = GPU2_cur;
            }
        }

        public void Display() {           

            //CPU
            label7.Text = CPU_cur.ToString() + " °C";
            label8.Text = CPU_min.ToString() + " °C";
            label9.Text = CPU_max.ToString() + " °C";

            //GPU 1
            label10.Text = GPU1_cur.ToString() + " °C";
            label11.Text = GPU1_min.ToString() + " °C";
            label12.Text = GPU1_max.ToString() + " °C";

            //GPU 2
            if (twoGPUs) {
                label13.Text = GPU2_cur.ToString() + " °C";
                label14.Text = GPU2_min.ToString() + " °C";
                label15.Text = GPU2_max.ToString() + " °C";
            }

            //Loads
            label17.Text = CPU_Load.ToString() + " %";
            label18.Text = GPU1_Load.ToString() + " %";
            label19.Text = GPU2_Load.ToString() + " %";
        }



        //Events

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
        }
                
    }
}
