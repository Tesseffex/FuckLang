using System;
using System.Collections.Generic;
using System.Text;

namespace Fuck
{
    public class Value(double v)
    {
        public double V = v;

        public override string ToString() => V.ToString();
    }
}
