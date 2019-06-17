using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubicomp_escritorio
{
    class Data
    {
        public int Gsr_average { get; set; }

        public long Time { get; set; }

        public int Hr_ohm {
            get {
                return (Gsr_average != 512 ? ((1024 + 2 * Gsr_average) * 10000) / (512 - Gsr_average) : 0);
            }
        }

        public Data(int gsr_average)
        {
            this.Gsr_average = gsr_average;
            Time = DateTime.Now.Ticks;
        }

        public override string ToString()
        {
            return Time + " " + Gsr_average + " " + Hr_ohm;
        }
    }
}
