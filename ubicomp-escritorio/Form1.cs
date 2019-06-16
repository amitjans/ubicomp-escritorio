using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace ubicomp_escritorio
{
    public partial class Form1 : Form
    {
        System.IO.Ports.SerialPort arduino;
        List<Data> values;
        Thread t;

        public Form1()
        {
            InitializeComponent();
            arduino = new System.IO.Ports.SerialPort();
            foreach (string item in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(item);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            arduino.PortName = comboBox1.SelectedItem.ToString();
            arduino.Open();
            values = new List<Data>();
            t = new Thread(new ThreadStart(ThreadProc));
            t.Start();
            label2.Text = "Recibiendo";
            button1.Enabled = false;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (arduino.IsOpen)
            {
                arduino.Close();
            }
            t.Join();
            List<string> lines = new List<string>();
            lines.Add("id gsr_average hr_ohm");
            for (int i = 0; i < values.Count; i++)
            {
                lines.Add(i + " " + values[i].ToString());
            }

            System.IO.File.WriteAllLines(System.IO.Directory.GetCurrentDirectory() + "\\" + textBox1.Text.Trim() + ".txt", lines);
            button1.Enabled = true;
        }

        public void ThreadProc()
        {
            try
            {
                while (true)
                {
                    var temp = new Data(int.Parse(arduino.ReadLine()));
                    values.Add(temp);
                    Console.WriteLine(temp);
                    Thread.Sleep(0);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
