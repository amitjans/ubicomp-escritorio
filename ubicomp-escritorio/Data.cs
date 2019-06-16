using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubicomp_escritorio
{
    class Data
    {
        public int Value { get; set; }

        public int Resistance {
            get {
                return ((1024 + 2 * Value) * 10000) / (512 - Value);
            }
        }

        public Data(int value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value + " " + Resistance;
        }
    }
}
