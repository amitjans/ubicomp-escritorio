using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace ubicomp_escritorio
{
    public partial class Form1 : Form
    {
        private bool conectado;
        private bool grabar;
        private List<Data> values;
        private static readonly HttpClient client = new HttpClient();
        private System.IO.Ports.SerialPort arduino;
        private Thread s;
        private Thread t;

        private long init;

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
            CheckForIllegalCrossThreadCalls = false;
            init = 0;
            s = new Thread(new ThreadStart(ThreadSockets));
            s.Start();
            chart1.Series["Series1"].ChartType = SeriesChartType.Line;
            chart1.Titles.Add("Siemens");
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            values = new List<Data>();
            grabar = true;
            init = 0;
            label2.Text = "Recibiendo datos";
            button1.Enabled = false;
            button2.Enabled = true;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            label2.Text = "Deteniendo";
            grabar = false;
            init = 0;
            label2.Text = "Guardando datos";

            Guardar();

            button1.Enabled = true;
            button2.Enabled = false;
            label2.Text = "Listo!!!";
        }

        public void Guardar() {
            List<string> lines = new List<string>();
            lines.Add("id;time;gsr_average;siemens");
            for (int i = 0; i < values.Count; i++)
            {
                lines.Add(i + ";" + values[i].ToString());
            }

            if (string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                textBox1.Text = "Sessión " + new DateTime(values[0].Time).ToString("hh-mm-ss");
            }

            string output = "{ \"Name\":\"" + textBox1.Text.Trim() + "\", \"sensor\":" + JsonConvert.SerializeObject(values) + "}";

            System.IO.File.WriteAllLines(System.IO.Directory.GetCurrentDirectory() + "\\" + textBox1.Text.Trim() + ".csv", lines);
            System.IO.File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\" + textBox1.Text.Trim() + ".json", output);

            label2.Text = "Enviando al servidor";
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://ubicomp.azurewebsites.net/api/Values");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(output);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                //{
                //    var result = streamReader.ReadToEnd();
                //}
            }
            catch (Exception)
            {
                Console.WriteLine("Upps. A ocurrido un error al enviar los datos al servidor. Verifique si se enviaron y en caso de que no utilize el archivo .json generado");
            }
        }

        public void ThreadProc()
        {
            try
            {
                while (conectado)
                {
                    var temp = new Data(int.Parse(arduino.ReadLine()));
                    chart1.Series["Series1"].Points.AddXY(new DateTime(temp.Time).ToString("mm:ss"), (double)temp.Siemens);
                    chart1.Update();
                    if (grabar)
                    {
                        values.Add(temp);
                        if (init == 0)
                        {
                            init = temp.Time + (long) (numericUpDown1.Value * 10000000);
                        } else if (init < temp.Time)
                        {
                            grabar = false;
                            button2.Enabled = false;
                            button1.Enabled = true;
                            Guardar();
                        }
                    }
                    Console.WriteLine(temp.ToStringWithHourFormat());
                    Thread.Sleep(0);
                }
            }
            catch (Exception)
            {
            }
        }

        public void ThreadSockets()
        {
            try
            {
                Socket listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket conexion;
                IPEndPoint connect = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6400);

                listen.Bind(connect);
                listen.Listen(10);
                conexion = listen.Accept();
                while (true)
                {
                    var recibir_info = new byte[100];
                    var data = "";
                    var array_size = 0;

                    array_size = conexion.Receive(recibir_info, 0, recibir_info.Length, 0);
                    Array.Resize(ref recibir_info, array_size);
                    data = Encoding.Default.GetString(recibir_info);
                    Console.WriteLine(data);
                    var command = data.Split(":"[0])[0];
                    if (command.Equals("start"))
                    {
                        textBox1.Text = data.Split(":"[0])[1];
                        numericUpDown1.Value = 1000;
                        button1.PerformClick();
                    } else if (command.Equals("stop"))
                    {
                        button2.PerformClick();
                    }
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