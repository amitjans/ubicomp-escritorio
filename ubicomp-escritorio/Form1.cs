using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace ubicomp_escritorio
{
    public partial class Form1 : Form
    {
        private List<Data> values;
        private static readonly HttpClient client = new HttpClient();
        private System.IO.Ports.SerialPort arduino;
        private Thread t;
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
            label2.Text = "Recibiendo datos";
            button1.Enabled = false;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            label2.Text = "Deteniendo";
            if (arduino.IsOpen)
            {
                arduino.Close();
            }
            t.Join();
            label2.Text = "Guardando datos";
            List<string> lines = new List<string>();
            lines.Add("id time gsr_average hr_ohm");
            for (int i = 0; i < values.Count; i++)
            {
                lines.Add(i + " " + values[i].ToString());
            }

            System.IO.File.WriteAllLines(System.IO.Directory.GetCurrentDirectory() + "\\" + textBox1.Text.Trim() + ".txt", lines);
            label2.Text = "Enviando al servidor";
            string output = JsonConvert.SerializeObject(values);

            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://localhost:44366/api/Values");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write("{ \"Name\":\"" + textBox1.Text.Trim() + "\", \"sensor\":" + output + "}");
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            button1.Enabled = true;
            label2.Text = "Listo!!!";
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (arduino.IsOpen)
            {
                arduino.Close();
            }
        }
    }
}