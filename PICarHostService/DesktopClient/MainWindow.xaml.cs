using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DesktopClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window,  INotifyPropertyChanged
    {
        private int _speedFactor;
        public int SpeedFactor
        {
            get
            {
                return _speedFactor;
            }

            set
            {
                _speedFactor = value;
                RaisePropertyChanged("SpeedFactor");
            }
        }

        public Brush PingColor
        {
            get
            {
                return _LastPingResponse == "pong" ? Brushes.Green : Brushes.Red;
            }
        }
        private String _LastPingResponse;
        public String LastPingResponse
        {
            get
            {
                return _LastPingResponse;
            }
            set
            {
                _LastPingResponse = value;
                RaisePropertyChanged("PingColor");
            }
        }
        
        private String _lastResponse;
        public String LastResponse
        {
            get
            {
                return _lastResponse;
            }
            set
            {
                _lastResponse = value;
                RaisePropertyChanged("LastResponse");
            }
        }

        private readonly DispatcherTimer dispatcherTimer;
        private readonly DispatcherTimer dispatcherPingTimer;
        private TcpClient tcpClient;

        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);

            dispatcherPingTimer = new DispatcherTimer();
            dispatcherPingTimer.Tick += DispatcherPingTimer_Tick; ;
            dispatcherPingTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            dispatcherPingTimer.Start();
            SpeedFactor = 10;
            LastPingResponse = "";
        }

        private async void DispatcherPingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var response = await ExecuteWebCommand(@"http://picar:8081/", "system", "ping", "1");
            }
            catch (Exception ex)
            {
                LastPingResponse = "";
            }
        }

        public void ConnectToCamera()
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect("picar", 5000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void DisconnectCamera()
        {
            if (tcpClient!=null && tcpClient.Connected)
            {
                byte[] writebuff = new byte[] { 0 };

                var networkStream = tcpClient.GetStream();

                networkStream.Write(writebuff, 0, 1);
                tcpClient.Close();
            }

        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            RefreshImage();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        //private void ExecuteWebCommand(String url)
        //{
        //    var client = new WebClient();
        //    client.Headers["User-Agent"] =
        //        "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0)";

        //    var response = client.DownloadString(url);

        //}


        //private void ExecuteWebCommand(String url, String Processor, String Command)
        //{
        //    ServicePointManager.Expect100Continue = false;
        //    var lstCommandArray = new List<PICarCommand>();
        //    var command = new PICarCommand();
        //    command.Controller = Processor;
        //    command.Command = Command;
        //    command.Parameters = "1";
        //    lstCommandArray.Add(command);

        //    var jsonContent = JsonConvert.SerializeObject(lstCommandArray);
        //    Create a request using a URL that can receive a post.
        //   WebRequest request = WebRequest.Create(url);
        //    request.Method = "POST";

        //    byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
        //    request.ContentType = "application/json";
        //    request.ContentLength = byteArray.Length;
        //    Stream dataStream = request.GetRequestStream();

        //    dataStream.Write(byteArray, 0, byteArray.Length);
        //    dataStream.Close();
        //    WebResponse response = request.GetResponse();

        //}

        private void RefreshImage()
        {
            try
            {

                List<byte> ImgArray = new List<byte>();
                byte[] writebuff = new byte[] { 1 };

                var networkStream = tcpClient.GetStream();
                int bytesRead = 0;

                networkStream.Write(writebuff, 0, 1);

                if (networkStream.CanRead)
                {


                    do
                    {
                        byte[] inStream = new byte[150000];
                        bytesRead = networkStream.Read(inStream, 0, 150000);
                        ImgArray.AddRange(inStream);
                    }
                    while (networkStream.DataAvailable);



                    var ms = new MemoryStream(ImgArray.ToArray());

                    var bitmapImg = new BitmapImage();
                    bitmapImg.BeginInit();
                    bitmapImg.StreamSource = ms;
                    bitmapImg.EndInit();
                    this.CamImage.Source = bitmapImg;

                }


            }
            catch (Exception )
            {

            }


        }

        private async Task<String> ExecuteWebCommand(String url, String Processor, String Command, String parameters)
        {


            try
            {
                ServicePointManager.Expect100Continue = false;
                var client = new HttpClient();
                using (client)
                {
                    var lstCommandArray = new List<PICarCommand>();
                    var command = new PICarCommand
                    {
                        Controller = Processor,
                        Command = Command,
                        Parameters = parameters
                    };
                    lstCommandArray.Add(command);

                    var jsonContent = JsonConvert.SerializeObject(lstCommandArray);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);

                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var currentResponseBuffer = LastResponse;

                    LastResponse = stringResponse + Environment.NewLine + currentResponseBuffer;
                    webbrowser.NavigateToString(LastResponse);
                    return stringResponse;
                }

            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = (HttpWebResponse)ex.Response;
                    LastResponse = response.StatusCode.ToString();
                    return LastResponse;
                }
                else
                {
                    LastResponse = ex.Status.ToString();
                    return LastResponse;
                }
            }
        }

        private async void  Btn_ClickForward(object sender, RoutedEventArgs e)
        {

            await ExecuteWebCommand(@"http://picar:8081/", "motor", "forward", "1");
        }

        private async void Btn_ClickBack(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "motor", "back","1");
        }

        private async void Btn_ClickRight(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "motor", "right","1");
        }

        private async void Btn_ClickLeft(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "motor", "left","1");
        }

        private async void btn_ClickLightsOn(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "rgb", "on","");
        }

        private async void btn_ClickLightsOff(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "rgb", "off","");
        }

        private async void Btn_SpeedUpdateClick(object sender, RoutedEventArgs e)
        {
            double speedratio = (SpeedFactor / 100.0);

            await ExecuteWebCommand(@"http://picar:8081/", "motor", "speed", speedratio.ToString());
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W: await ExecuteWebCommand(@"http://picar:8081/", "motor", "forward", "0");break;
                case Key.A: await ExecuteWebCommand(@"http://picar:8081/", "motor", "left", "0"); break;
                case Key.D: await ExecuteWebCommand(@"http://picar:8081/", "motor", "right", "0"); break;
                case Key.S: await ExecuteWebCommand(@"http://picar:8081/", "motor", "back", "0"); break;
            }
        }

        private async void Btn_ClickSonicGetDistance(object sender, RoutedEventArgs e)
        {
          await  ExecuteWebCommand(@"http://picar:8081/", "sonic", "distance", "0");
        }
        private async void Btn_ClickSonicRight(object sender, RoutedEventArgs e)
        {
             await  ExecuteWebCommand(@"http://picar:8081/", "sonic", "servoleft", "0");
        }


        private async void Btn_ClickSonicLeft(object sender, RoutedEventArgs e)
        {
           await ExecuteWebCommand(@"http://picar:8081/", "sonic", "servoright", "0");
        }

        private async void Btn_ClickSonicCenter(object sender, RoutedEventArgs e)
        {
           await ExecuteWebCommand(@"http://picar:8081/", "sonic", "servocenter", "0");
        }

        private async void btnClick_CamLeft(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "cam", "left", "0");
        }

        private async void Btn_ClickCamRight(object sender, RoutedEventArgs e)
        {
          await  ExecuteWebCommand(@"http://picar:8081/", "cam", "right", "0");
        }



        private async void Btn_ClickCamDown(object sender, RoutedEventArgs e)
        {
           await  ExecuteWebCommand(@"http://picar:8081/", "cam", "down", "0");
        }

        private async void btnClick_CamUp(object sender, RoutedEventArgs e)
        {
           await  ExecuteWebCommand(@"http://picar:8081/", "cam", "up", "0");
        }

        private async void Window_KeyUp(object sender, KeyEventArgs e)
        {
           await  ExecuteWebCommand(@"http://picar:8081/", "motor", "stop", "0");
        }

        private async void Btn_ClickAutoPilot(object sender, RoutedEventArgs e)
        {
            await ExecuteWebCommand(@"http://picar:8081/", "autopilot", "go", "5");
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            LastResponse = " ";
            webbrowser.Navigate("about:blank");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ConnectToCamera();
            //dispatcherTimer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DisconnectCamera();
        }

   

    }

}
