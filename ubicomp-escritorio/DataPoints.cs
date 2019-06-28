using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ubicomp_escritorio
{
    class DataPoints
    {
        public string X { get; set; }
        public double Y { get; set; }

        public DataPoints(string x, double y)
        {
            X = x;
            Y = y;
        }
    }
}
