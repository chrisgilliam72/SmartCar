using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltraborgLib;
using Windows.Devices;
using Windows.Devices.Gpio;
using Windows.Devices.Pwm;

namespace PICarHostService
{
    class SonicSensorController
    {

        private int SensorNo { get; set; }
        private UltraborgServo Servo { get; set; }

        private UltraborgSensor Sensor { get; set; }

        public SonicSensorController ()
        {
            SensorNo = 1;
            Servo = new UltraborgServo(1,-0.1);
            Sensor = new UltraborgSensor(1);
        }


        public void Init(Ultraborg ultraborg)
        {
            try
            {
                Servo.Init(ultraborg);
                Sensor.Init(ultraborg);

            }
            catch (Exception ex)
            {
                throw new Exception("The sonic sensor controller threw an exception:" + ex.Message);
            }
        }


        public void Test()
        {
            Servo.Test();
        }

        public String  ProcessQuery(String requestedAction)
        {
            String statusReturn = "";
            Debug.WriteLine("Sonic Sensor controller action:" + requestedAction);
            LoggingProcessor.AddTrace("Sonic Sensor controller action:" + requestedAction);
            switch (requestedAction)
            {
                case "servoleft": ServoToLeft(); statusReturn= "OK";break;
                case "servoright":  ServoToRight(); ; statusReturn= "OK";break;
                case "servocenter":  ServoToCenter(); ; statusReturn= "OK";break;
                case "distance":  var distance =  MeasureAverageDistance(); statusReturn = distance.ToString(); break; 
                default: statusReturn = "Unknown action";break;

            }

            Debug.WriteLine("Sonic Sensor action completed");
            LoggingProcessor.AddTrace("Sonic Sensor action completed");
            return statusReturn; 
        }


        private void ServoToLeft()
        {
            Servo.ServoTo90();
        }

        private void ServoToCenter()
        {

            Servo.ServoTo0();
        }

        private void ServoToRight()
        {
            Servo.ServoTo270();
        }

        public double MeasureAverageDistance()
        {
            var distance = Sensor.GetDistance();
            Debug.WriteLine("Sonic Sensor controller measured distance : " + distance);
            LoggingProcessor.AddTrace("Sonic Sensor controller measured distance: " + distance);
            return distance;       
        }

        //public async Task<double> MeasureAverageDistance()
        //{
        //    Debug.WriteLine("Sonic Sensor controller distance request started");
        //    LoggingProcessor.AddTrace("Sonic Sensor controller distance request started");

        //    double distance = 0.0;
        //    double numSamples = 0;
        //    for (int i=0;i<1;i++)
        //    {
        //        var tmpDist = MeasureDistance();
        //        if (tmpDist > 0)
        //        {
        //            distance += tmpDist;
        //            numSamples++;
        //        }
        //        await Task.Delay(TimeSpan.FromMilliseconds(100));
        //    }

        //    if (numSamples > 0)
        //        distance = distance / numSamples;

        //    Debug.WriteLine("Sonic Sensor controller distance samples: "+numSamples);
        //    LoggingProcessor.AddTrace("Sonic Sensor controller distance samples: " + numSamples);

        //    Debug.WriteLine("Sonic Sensor controller average measured distance : " + distance);
        //    LoggingProcessor.AddTrace("Sonic Sensor controller average measured distance: " + distance);

        //    Debug.WriteLine("Sonic Sensor controller distance request completed");
        //    LoggingProcessor.AddTrace("Sonic Sensor controller distance request completed");
        //    return distance;
        //}
        //private double MeasureDistance()
        //{
        //    double distance = 0.0;
        //    bool timeOut = false;
        //    var mre = new ManualResetEventSlim(false);
        //    pinTrig.Write(GpioPinValue.High);
        //    mre.Wait(TimeSpan.FromMilliseconds(0.015));
        //    pinTrig.Write(GpioPinValue.Low);

        //    var sw = new Stopwatch();
        //    var sw_timeout = new Stopwatch();
        //    sw_timeout.Start();

        //    // Wait for pulse
        //    while (pinEcho.Read() != GpioPinValue.High)
        //    {
        //        if (sw_timeout.ElapsedMilliseconds > 100)
        //        {
        //            distance = -1;
        //            timeOut = true;
        //            break;
        //        }

        //    }
        //    sw.Start();

        //    // Wait for pulse end
        //    while (pinEcho.Read() == GpioPinValue.High)
        //    {
        //        if (sw_timeout.ElapsedMilliseconds > 100)
        //        {
        //            distance = -1;
        //            timeOut = true;
        //            break;
        //        }

        //    }
        //    sw.Stop();
        //    sw_timeout.Stop();
        //    if (!timeOut)
        //    {
        //        double timeMS = sw.Elapsed.TotalMilliseconds;
        //        distance = sw.Elapsed.TotalSeconds * 17150;
        //    }

        //    return distance;
        //}
    }
}
