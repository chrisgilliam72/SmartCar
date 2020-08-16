using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace PICarHostService
{
    class SystemController
    {
        public String ProcessQuery(String requestedAction)
        {
            Debug.WriteLine("System controller action :" + requestedAction);
            LoggingProcessor.AddTrace("System controller action:" + requestedAction);

            switch (requestedAction)
            {
                case "ping": return "pong";
                case "quit": Quit(); return " OK";
                case "echo": return " System echo";
            }
            return "Unknown action";
        }

        public void Quit()
        {
            CoreApplication.Exit();
        }
    }
}