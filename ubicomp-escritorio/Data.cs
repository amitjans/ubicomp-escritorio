using System;

namespace ubicomp_escritorio
{
    internal class Data
    {
        public int Gsr_average { get; set; }

        public long Time { get; set; }

        public float Siemens
        {
            get
            {
                try
                {
                    return (float) ((1 / (double) (((1024 + 2 * Gsr_average) * 10000) / (512 - Gsr_average))) * 1000000);
                }
                catch (Exception e)
                {
                    return 0;
                }
            }
        }

        public Data(int gsr_average)
        {
            this.Gsr_average = gsr_average;
            Time = DateTime.Now.Ticks;
        }

        public override string ToString()
        {
            return Time + ";" + Gsr_average + ";" + Siemens;
        }

        public string ToStringWithHourFormat()
        {
            return new DateTime(Time).ToString("hh:mm:ss") + ";" + Gsr_average + ";" + Siemens;
        }
    }
}