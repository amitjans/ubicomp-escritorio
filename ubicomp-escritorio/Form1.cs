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
        private bool conectado;
        private bool grabar;
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
            button1.Enabled = false;
            button2.Enabled = false;
            conectado = false;
            grabar = false;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            values = new List<Data>();
            grabar = true;
            label2.Text = "Recibiendo datos";
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            label2.Text = "Deteniendo";
            grabar = false;
            label2.Text = "Guardando datos";
            List<string> lines = new List<string>();
            lines.Add("id;time;gsr_average;hr_ohm");
            for (int i = 0; i < values.Count; i++)
            {
                lines.Add(i + ";" + values[i].ToString());
            }

            string output = "{ \"Name\":\"" + textBox1.Text.Trim() + "\", \"sensor\":" + JsonConvert.SerializeObject(values) + "}";

            System.IO.File.WriteAllLines(System.IO.Directory.GetCurrentDirectory() + "\\" + textBox1.Text.Trim() + ".csv", lines);
            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\" + textBox1.Text.Trim() + ".json", output);

            label2.Text = "Enviando al servidor";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://ubicomp.azurewebsites.net/api/Values");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(output);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }

            button1.Enabled = true;
            button2.Enabled = false;
            label2.Text = "Listo!!!";
        }

        public void ThreadProc()
        {
            try
            {
                while (conectado)
                {
                    var temp = new Data(int.Parse(arduino.ReadLine()));
                    if (grabar)
                    {
                        values.Add(temp);
                    }
                    Console.WriteLine(temp.ToStringWithHourFormat());
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

        private void Button3_Click(object sender, EventArgs e)
        {
            if (arduino.IsOpen)
            {
                conectado = false;
                t.Join();
                arduino.Close();
                button3.Text = "Conectar";
                button1.Enabled = false;
                conectado = false;
            }
            else
            {
                arduino.PortName = comboBox1.SelectedItem.ToString();
                arduino.Open();
                button3.Text = "Desconectar";
                button1.Enabled = true;
                conectado = true;
                t = new Thread(new ThreadStart(ThreadProc));
                t.Start();
            }
        }
    }
}