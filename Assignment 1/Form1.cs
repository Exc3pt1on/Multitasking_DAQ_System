using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


namespace Assignment_1
{
    public partial class Form1 : Form
    {
        private List<double> A1 = new List<double>();
        private List<double> A2 = new List<double>();
        private List<double> A3 = new List<double>();
        private List<double> A4 = new List<double>();
        private List<double> A5 = new List<double>();
        private List<double> A6 = new List<double>();
        private bool D1 = false;
        private bool D2 = false;
        private bool D3 = false;
        private bool sampling = false;
        private System.Threading.Timer timer;
        private System.Threading.Timer timer2;
        private Sensor[] sObjAnalog = new Sensor[6];
        private Sensor[] sObjDigital = new Sensor[3];
        private Color D1Color = Color.Gray;
        private Color D2Color = Color.Gray;
        private Color D3Color = Color.Gray;
        private double sampleTime = 2.4;
        private double logTime = 3.0;
        private int logCounter = 0;


        public Form1()
        {
            InitializeComponent();
        }
        
        private void btnStartSample_Click(object sender, EventArgs e)
        {
            // Method to start sample sensor data
            int counter, maxSidDigital = 3, maxSidAnalog = 6;
            sampling = true;
            // Create an array of sensor objects
            for (counter = 0; counter < maxSidDigital; counter++)
            {
                sObjDigital[counter] = new Sensor(counter, true);
            }
            for (counter = 0; counter < maxSidAnalog; counter++)
            {
                sObjAnalog[counter] = new Sensor(counter, false);
            }
            // Start thread
            timer = new System.Threading.Timer(SampleData, null, TimeSpan.Zero, TimeSpan.FromSeconds(sampleTime));

            txtStatusSample.Text = "Sampling";
            txtStatusSample.BackColor = Color.Green;
          
        }
        private void SampleData(object state)
        {
            // This method will be called every 2.4 seconds to sample data from the sensor
            double[] samplesA = new double[6];
            for (int i = 0; i < samplesA.Length; i++)
            {
                samplesA[i] = sObjAnalog[i].GetValue();
            }
            double[] samplesD = new double[3];
            for (int i = 0; i < samplesD.Length; i++)
            {
                samplesD[i] = sObjDigital[i].GetValue();
            }

            A1.Add(samplesA[0]);
            A2.Add(samplesA[1]);
            A3.Add(samplesA[2]);
            A4.Add(samplesA[3]);
            A5.Add(samplesA[4]);
            A6.Add(samplesA[5]);
            if (samplesD[0] == 1) { D1 = true; } else { D1 = false; }
            if (samplesD[1] == 1) { D2 = true; } else { D2 = false; }
            if (samplesD[2] == 1) { D3 = true; } else { D3 = false; }

            // Invoke UI updates on the UI thread
            this.Invoke((MethodInvoker)delegate
            {
                txtA1.Text = A1[A1.Count - 1].ToString("F2");
                txtA2.Text = A2[A2.Count - 1].ToString("F2");
                txtA3.Text = A3[A3.Count - 1].ToString("F2");
                txtA4.Text = A4[A4.Count - 1].ToString("F2");
                txtA5.Text = A5[A5.Count - 1].ToString("F2");
                txtA6.Text = A6[A6.Count - 1].ToString("F2");
                if (D1 == true) { D1Color = Color.Green; } else { D1Color = Color.Red; }
                if (D2 == true) { D2Color = Color.Green; } else { D2Color = Color.Red; }
                if (D3 == true) { D3Color = Color.Green; } else { D3Color = Color.Red; }
                panD1.Invalidate();
                panD2.Invalidate();
                panD3.Invalidate();
                DateTime dateTime = DateTime.Now;
                DateTime nextSampleTime = dateTime.AddSeconds(sampleTime);
                txtNextSample.Text = nextSampleTime.ToString("HH:mm:ss");
            });
        }
        private void btnStopSample_Click(object sender, EventArgs e)
        {
            // Method to stop sampling data
            sampling = false;
            // Stop the timer
            timer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            txtStatusSample.Text = "Not sampling";
            txtStatusSample.BackColor = Color.FromArgb(255, 128, 128);

            //Stop log thread (cannot log if sample is stopped)
            timer2?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            txtStatusLog.Text = "Not logging";
            txtStatusLog.BackColor = Color.FromArgb(255, 128, 128);
        }

        private void btnStartLog_Click(object sender, EventArgs e)
        {
            // Method to start logging data
            if (sampling)
            {
                //Start log thread
                timer2 = new System.Threading.Timer(Log, null, TimeSpan.Zero, TimeSpan.FromSeconds(logTime));
                txtStatusLog.Text = "Logging";
                txtStatusLog.BackColor = Color.Green;
            }
            
        }

        private void btnStopLog_Click(object sender, EventArgs e)
        {
            // Method to stop logging data
            // Stop log thread
            timer2?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            txtStatusLog.Text = "Not logging";
            txtStatusLog.BackColor = Color.FromArgb(255, 128, 128);
        }
        private void Log(object state)
        {
            // Method for saving sensor data to CSV file
            string[] headers = { "Time", "Analog 1", "Analog 2", "Analog 3", "Analog 4", "Analog 5", "Analog 6", 
                "Digital 1", "Digital 2", "Digital 3"};
            DateTime dateTime = DateTime.Now;
            string[] row = 
                { 
                dateTime.ToString(), 
                MovingAverage(A1).ToString(), 
                MovingAverage(A2).ToString(), 
                MovingAverage(A3).ToString(),
                MovingAverage(A4).ToString(),
                MovingAverage(A5).ToString(),
                MovingAverage(A6).ToString(),
                D1.ToString(), 
                D2.ToString(),
                D3.ToString(),
                };
            // Specify the file path
            string filePath = txtLogFile.Text + ".csv";
            try
            {
                // Check if the file exists
                if (!File.Exists(filePath))
                {
                    // If the file doesn't exist, write the headers
                    SaveToCsv(filePath, headers);
                }
                // Save data to CSV file (appending)
                SaveToCsv(filePath, row); // Appending row1
            }
            catch (Exception)
            {
                throw;
            }
            logCounter++;
            this.Invoke((MethodInvoker)delegate
            {
                //Print counter and next log time to form
                txtLogCount.Text = logCounter.ToString();
                DateTime nextLogTime = dateTime.AddSeconds(logTime);
                txtNextLog.Text = nextLogTime.ToString("HH:mm:ss");
            });
        }
        
        private void SaveToCsv(string filePath, string[] values)
        {
            // Use StreamWriter to write to the CSV file
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                // Combine the values into a CSV-formatted line
                string csvLine = string.Join(";", values);

                // Write the line to the file
                sw.WriteLine(csvLine);
            }
        }

        private double MovingAverage(List<double> data, int windowSize = 10)
        {
            // Method for filtering the analog signals
            double result;

            int start = Math.Max(0, data.Count - windowSize);
            int end = data.Count;

            // Calculate the average of the values in the window
            double sum = 0;
            for (int i = start; i < end; i++)
            {
                sum += data[i];
            }
            result = sum / (end - start);
            
            return Math.Round(result, 2);
        }

        private void panD1_Paint(object sender, PaintEventArgs e)
        {
            // Painting digital indicator
            using (Graphics g = e.Graphics)
            {
                Rectangle ellipseBounds = new Rectangle(0, 0, 25, 25);
                using (SolidBrush brush = new SolidBrush(D1Color))
                {
                    g.FillEllipse(brush, ellipseBounds);
                }
                
            }
        }

        private void panD2_Paint(object sender, PaintEventArgs e)
        {
            // Painting digital indicator
            using (Graphics g = e.Graphics)
            {
                Rectangle ellipseBounds = new Rectangle(0, 0, 25, 25);
                using (SolidBrush brush = new SolidBrush(D2Color))
                {
                    g.FillEllipse(brush, ellipseBounds);
                }
            }
        }

        private void panD3_Paint(object sender, PaintEventArgs e)
        {
            // Painting digital indicator
            using (Graphics g = e.Graphics)
            {
                Rectangle ellipseBounds = new Rectangle(0, 0, 25, 25);
                using (SolidBrush brush = new SolidBrush(D3Color))
                {
                    g.FillEllipse(brush, ellipseBounds);
                }
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Press the 'Start Sampling' button to start sample sensor data from 9 sensors\n" + 
                "Press the 'Start Logging' to store the data in the path you write in 'Log file name'");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This is an application for sampling and logging data\n Made by Kim Langvannskås");
        }
    }
}
