//  /u/FestiveCore  for  /r/pcmasterrace
//
// Requirements :
// - OpenHardwareMonitor Library
// 
// If you are copying the project into Visual Studio, you will need 19 labels.
// The program is made for a 4-cores CPU and 2 GPUs
// If you want to change the number of cores, just go into the method AvgMinMax() and change the two dividers in the CPU average.
// If you have an ATI/AMD GPU, change the code in GPU Loads and Temps in Update_Temps() method.
// If you have only one GPU, remove all stuff with GPU2 and the variable ct that is used to switch through the 2 GPUs.
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
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;

namespace HardwareTemp {

    public partial class Form1 : Form {

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



        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            timer1.Start();
            InitComp(cpt);
        }

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

                if (ct == 0)
                    ct++;
                else
                    ct--;


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
                    else if (sensor.Hardware.HardwareType == HardwareType.GpuNvidia) {  //Change by HardwareType.GpuAti if you have an AMD/ATI GPU
                        if (ct != 0) {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                GPU1_cur = (int)sensor.Value;
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                GPU1_Load = (int)sensor.Value;
                        } else {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Value != null)
                                GPU2_cur = (int)sensor.Value;
                            if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core" && sensor.Value != null)
                                GPU2_Load = (int)sensor.Value;
                        }



                    }
                    
                }
            }


        }

        public static void AvgMinMax() {

            //CPU Average, divided by 4 for a 4-cores CPU
            CPU_cur = CPU_cur / 4;
            CPU_Load = CPU_Load / 4;

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
            if (GPU2_cur < GPU2_min)
                GPU2_min = GPU2_cur;
            if (GPU2_cur > GPU2_max)
                GPU2_max = GPU2_cur;
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
            label13.Text = GPU2_cur.ToString() + " °C";
            label14.Text = GPU2_min.ToString() + " °C";
            label15.Text = GPU2_max.ToString() + " °C";

            //Loads
            label17.Text = CPU_Load.ToString() + " %";
            label18.Text = GPU1_Load.ToString() + " %";
            label19.Text = GPU2_Load.ToString() + " %";
        }

        private void timer1_Tick(object sender, EventArgs e) {
            MainLoop();
        }
                
    }
}
