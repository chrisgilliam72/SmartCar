using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace PICarHostService
{
   class CamServer
    {
        private readonly StreamSocketListener _listener;
        private Camera _camera;

        public CamServer(Camera camera)
        {
            _listener = new StreamSocketListener();
            _listener.ConnectionReceived += _listener_ConnectionReceived; ;
            _listener.Control.KeepAlive = false;
            _listener.Control.NoDelay = false;
            _listener.Control.QualityOfService = SocketQualityOfService.LowLatency;
            _camera = camera;
        }

        private async void _listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var socket = args.Socket;
            var data = new byte[1];
            var buffer = data.AsBuffer();

            var cameraFrame = _camera.Frame.ToArray();
            await socket.OutputStream.WriteAsync(cameraFrame.AsBuffer());
            await socket.OutputStream.FlushAsync();
           
        }

        public async Task Start()
        {
            await _listener.BindServiceNameAsync(5000.ToString());
        }
    }
}
