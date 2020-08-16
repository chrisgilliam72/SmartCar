using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PICarHostService
{

    class TraceMessage
    {
        public int index { get; set; }
        public String Message { get; set; }
        public DateTime Time { get; set; }

        public override string ToString()
        {
            var stringMess= "("+index.ToString()+") --"+Time.ToString("t", DateTimeFormatInfo.CurrentInfo)+"-- "+Message;
            return stringMess;
        }
    }
}
