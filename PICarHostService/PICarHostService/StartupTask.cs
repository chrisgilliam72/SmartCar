using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using System.Threading.Tasks;
// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace PICarHostService
{
    public sealed class StartupTask : IBackgroundTask
    {
   

        public async  void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral  _Deferral = taskInstance.GetDeferral();

            var webserver = new MyWebserver();

            await webserver.Start();

            // 
            // TODO: Insert code to perform background work
            //
            // If you start any asynchronous methods here, prevent the task
            // from closing prematurely by using BackgroundTaskDeferral as.dd
            // described in http://aka.ms/backgroundtaskdeferral
            //

            _Deferral.Complete();
        }
    }
}
