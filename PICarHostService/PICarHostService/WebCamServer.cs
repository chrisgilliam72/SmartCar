using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICarHostService
{
    class WebCamServer
    {
        private Camera Camera { get; set; }
        private CamServer CamServer { get; set; }

        public WebCamServer()
        {
            Camera = new Camera();
            CamServer = new CamServer(Camera);
        } 

        public async void StartServer()
        {
            var mediaFrameFormats = await Camera.GetMediaFrameFormatsAsync();
            ConfigurationFile.SetSupportedVideoFrameFormats(mediaFrameFormats);
            var videoSetting = await ConfigurationFile.Read(mediaFrameFormats);

            await Camera.Initialize(videoSetting);
            Camera.Start();

            await CamServer.Start();
        }
    }

}
