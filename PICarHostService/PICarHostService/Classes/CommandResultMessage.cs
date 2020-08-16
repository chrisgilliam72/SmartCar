using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICarHostService
{
   

    public sealed class  CommandResultMessage
    {
        public int Seq { get; set; }
        public String Type { get; set; }
        public String Message { get; set; }
        public IEnumerable<ControllerComandResult> Results { get; set; }

        public CommandResultMessage(int seq, String message)
        {
            Seq = seq;
            Type = "Trace";
            Message = message;
        }

        public CommandResultMessage( IEnumerable<ControllerComandResult> listResults)
        {
            Seq = 0;
            Type = "Response";
            Results = listResults;
        }
    }
}
