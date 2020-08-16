using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICarHostService
{
    public sealed class CommandMsg
    {
        public String Controller { get; set; }
        public String Command { get; set; }
        public String Parameters { get; set; }

 
    }
}
