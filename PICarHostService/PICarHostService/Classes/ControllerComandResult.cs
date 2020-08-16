using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICarHostService
{
    public sealed class ControllerComandResult
    {
        public String Controller { get; set; }
        public String Command { get; set; }
        public String Result { get; set; }

        public ControllerComandResult(string controller, string command)
        {
            Controller = controller;
            Command = command;
            Result = "";
        }

        public ControllerComandResult(string controller, string command, string result)
        {
            Controller = controller;
            Command = command;
            Result = result;
        }
    }
}
