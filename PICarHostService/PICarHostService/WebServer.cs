using Microsoft.IoT.Lightning.Providers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltraborgLib;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace PICarHostService
{
    internal class MyWebserver
    {
        private const uint BufferSize = 8192;

        readonly ManualResetEvent SystemStopEvent = new ManualResetEvent(false);
        Autopilot AutoPilotController { get; set; }
        MotorProcessor MotorProcessor { get; set; }
        RGBContoller RGBController { get; set; }
        SonicSensorController SonicController { get; set; }
        SystemController SystemController { get; set; }
        CameraController CameraController { get; set; }
        StreamSocketListener WebServer { get; set; }
        Ultraborg Ultraborg { get; set; }
        public MyWebserver()
        {
            Ultraborg = new Ultraborg();
            WebServer = new StreamSocketListener();
           MotorProcessor = new MotorProcessor
            {
                SpeedFactor = 0.5
            };
            RGBController = new RGBContoller();
            SonicController = new SonicSensorController();
            SystemController = new SystemController();
            CameraController = new CameraController();
            AutoPilotController = new Autopilot(MotorProcessor, SonicController);
        }

        public async Task Start()
        {
            try
            {
                if (LightningProvider.IsLightningEnabled)
                {
                    LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
                    var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());
                    var gpio = (await GpioController.GetControllersAsync(LightningGpioProvider.GetGpioProvider()))[0];
                    var pwmController = pwmControllers[1]; // use the on-device controller
                    pwmController.SetDesiredFrequency(50);

                    await Ultraborg.Init(true);
                    MotorProcessor.Init(pwmController, gpio);
                    CameraController.Init(Ultraborg);
                    SonicController.Init(Ultraborg);
                    RGBController.Init(Ultraborg, gpio);
                    SonicController.Test();
                    RGBController.Test();


                    await WebServer.BindServiceNameAsync("8081");
                    WebServer.ConnectionReceived += WebServer_ConnectionReceived;

                    SystemStopEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(" The web server threw an exception: " + ex.Message);
            }


        }

        private async void WebServer_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                Debug.WriteLine("web server connection recieved ...");
                var request = new StringBuilder();

                using (var input = args.Socket.InputStream)
                {
                    var data = new byte[BufferSize];
                    var buffer = data.AsBuffer();
                    var dataRead = BufferSize;

                    while (dataRead == BufferSize)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(200));
                        Debug.WriteLine("web server processing...");
                        LoggingProcessor.AddTrace("web server processing...");
                        var readRes = await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        Debug.WriteLine("web message processed");
                        LoggingProcessor.AddTrace("web message processed");
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;

                    }
                }

                string query = GetQuery(request);
                var result =  ExecuteAction(query);
                using (var output = args.Socket.OutputStream)
                {
                    if (LoggingProcessor.TracingEnabled)
                    {
                        String returnMsg = "";
                        var traceMessageList = new List<CommandResultMessage>();
                        int seq = 1;
                        var traceMsgs = LoggingProcessor.GetNewTraceMessages();
                        foreach (var traceMsg in traceMsgs)
                        {
                            var resultMessage = new CommandResultMessage(seq++, traceMsg);
                            traceMessageList.Add(resultMessage);
                        }

                        returnMsg = JsonConvert.SerializeObject(traceMessageList);
                        await WriteResult(args.Socket.OutputStream, returnMsg);
                    }
                    String resultMsg = "";
                    var cmdResultMessage = new CommandResultMessage(result);
                    resultMsg = JsonConvert.SerializeObject(cmdResultMessage);
                    await WriteResult(args.Socket.OutputStream, resultMsg);
                } 
            }
            catch (IOException ex)
            {

            }
            catch (Exception ex)
            {

            }
        }

        private async Task WriteJSonResult(IOutputStream output, String message)
        {
            using (output)
            {
                using (var response = output.AsStreamForWrite())
                {
                   
                    using (var bodyStream = new MemoryStream())
                    {
                        
                        var dataArray = Encoding.UTF8.GetBytes(message);
                        await response.WriteAsync(dataArray,
                                                  0, dataArray.Length);
                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            }
        }
        /*
         *                     var html = Encoding.UTF8.GetBytes(
                    $"<html><head><title>PI Car Host</title></head><body>{message}</body></html>");
         */
        private async Task WriteResult(IOutputStream output, String message)
        {
            using (output)
            {
                using (var response = output.AsStreamForWrite())
                {
                    var html = Encoding.UTF8.GetBytes(message);
                    using (var bodyStream = new MemoryStream(html))
                    {
                        var header = $"HTTP/1.1 200 OK\r\nContent-Length: {bodyStream.Length}\r\nConnection: close\r\nContent-Type:application/json; charset=utf-8\r\n\r\n";
                        var headerArray = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerArray,
                                                  0, headerArray.Length);
                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            }

        }

        private List<ControllerComandResult> ExecuteAction(String jsonAction)
        {
            var controllerResultLst = new List<ControllerComandResult>();
            if (!String.IsNullOrEmpty(jsonAction) && !String.IsNullOrWhiteSpace(jsonAction))
            {
             
                var commandMsgLst = JsonConvert.DeserializeObject<List<CommandMsg>>(jsonAction);
                foreach (var commandMsg in commandMsgLst)
                {
                    var commandResult = new ControllerComandResult(commandMsg.Controller, commandMsg.Command);
                    if (commandMsg != null)
                    { 
                        switch (commandMsg.Controller)
                        {
                            case "autopilot": AutoPilotController.ProcessQuery(commandMsg.Command, Convert.ToDouble(commandMsg.Parameters)); break;
                            case "motor":  MotorProcessor.ProcessQuery(commandMsg.Command, Convert.ToDouble(commandMsg.Parameters)); commandResult.Result = "OK"; break;
                            case "rgb": RGBController.ProcessQuery(commandMsg.Command, commandMsg.Parameters); commandResult.Result="OK"; break;
                            case "sonic": commandResult.Result = SonicController.ProcessQuery(commandMsg.Command);break;
                            case "system": commandResult.Result = SystemController.ProcessQuery(commandMsg.Command);break;
                            case "cam": CameraController.ProcessQuery(commandMsg.Command);break;
                            default: commandResult.Result= "Invalid controller";break;
                        }

                       
                    }
                    controllerResultLst.Add(commandResult);
                }
            }
            else
            {
                var errorCmdResult = new ControllerComandResult("", "", "Invalid JSON String");
                controllerResultLst.Add(errorCmdResult);
            }

            return controllerResultLst;
        }



        private string GetQuery(StringBuilder request)
        {

            var requestLines = request.ToString().Split(Environment.NewLine.ToCharArray());
            var newLines = requestLines.Where(x => x.Count() > 0).ToList();
            var firstline = newLines.FirstOrDefault(x=>x.StartsWith("["));
            var lastLine = newLines.LastOrDefault(x=>x.Contains("]"));
            if (!String.IsNullOrEmpty(firstline) &&  !String.IsNullOrEmpty(lastLine) )
            {
                var jsonString = firstline;
                if ( firstline != lastLine)
                {               
                    var currentLine = firstline;
                    int index = newLines.IndexOf(currentLine);
                    do
                    {
                        jsonString += newLines[++index];
                    } while (index < newLines.Count - 1);
                    var endIndex = jsonString.LastIndexOf(']');
                    jsonString = jsonString.Substring(0, endIndex + 1);
                }
                else
                {
                    var endIndex = jsonString.LastIndexOf(']');
                    jsonString = jsonString.Substring(0, endIndex + 1);
                }
                return jsonString;
            }

            return null;
        }
    }
}
