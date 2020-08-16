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

    class RGBContoller
    {

        const int PINR = 22;
        const int PING = 27;
        const int PINB = 17;


        private GpioPin PinR { get; set; }
        private GpioPin PinG { get; set; }
        private GpioPin PinB { get; set; }

        private UltraborgServo Servo { get; set; }

       
        public RGBContoller()
        {
           
            Servo = new UltraborgServo(2,0);
        }

        public void Init(Ultraborg ultraborg, GpioController gpio)
        {
            try
            {
                Servo.Init(ultraborg);
                PinR = gpio.OpenPin(PINR);
                PinG = gpio.OpenPin(PING);
                PinB = gpio.OpenPin(PINB);

                PinR.SetDriveMode(GpioPinDriveMode.Output);
                PinG.SetDriveMode(GpioPinDriveMode.Output);
                PinB.SetDriveMode(GpioPinDriveMode.Output);
            }

            catch (Exception ex)
            {

                throw new Exception("The RGB controller threw an exception:" + ex.Message);
            }
        }


        public void ProcessQuery(String requestedAction, string parameter)
        {
            Debug.WriteLine("RGB controller action :" + requestedAction);
            LoggingProcessor.AddTrace("RGB controller action:" + requestedAction);

            if (requestedAction == "servo")
                ProcessServoQuery(parameter);
            else
                ProcessRGBQuery(requestedAction, parameter);

            Debug.WriteLine("RGB action completed");
            LoggingProcessor.AddTrace("RGB action completed");
        }

        private void ProcessServoQuery(string servoParameter)
        {
            switch (servoParameter)
            {
                case "left": Servo.ServoTo90();break;
                case "right": Servo.ServoTo270(); break;
                case "center": Servo.ServoTo0(); break;
            }
        }

        private void ProcessRGBQuery (string requestedAction, string rgbParameter)
        {
            switch (rgbParameter)
            {
                case "all": ToggleAllColors(requestedAction == "rgbon"); break;
                case "red": LightsRed(requestedAction == "rgbon"); break;
                case "green": LightsGreen(requestedAction == "rgbon"); break;
                case "blue": LightsBlue(requestedAction == "rgbon"); break;
            }
        }

        public void Test()
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                LightsRed(true);
                mre.Wait(TimeSpan.FromMilliseconds(500));
                mre.Reset();
                LightsRed(false);
                LightsGreen(true);
                mre.Wait(TimeSpan.FromMilliseconds(500));
                LightsGreen(false) ;
                mre.Reset();
                LightsBlue(true);
                mre.Wait(TimeSpan.FromMilliseconds(500));
                mre.Reset();
                LightsOff();
                mre.Wait(TimeSpan.FromMilliseconds(500));
                Servo.Test();
            }

        }

        public void LightsRed(bool On)
        {
            PinR.Write(On ?GpioPinValue.High: GpioPinValue.Low);
        }

        public void LightsBlue(bool On)
        {
            PinB.Write(On ? GpioPinValue.High : GpioPinValue.Low);
        }

        public void LightsGreen(bool On)
        {
            PinG.Write(On ? GpioPinValue.High : GpioPinValue.Low);
        }

        public void ToggleAllColors(bool On)
        {
            if (On)
                LightsOn();
            else
                LightsOff();
        }

        public void LightsOn()
        {
            LoggingProcessor.AddTrace("Start LightsOn");
            PinR.Write(GpioPinValue.High);
            PinG.Write(GpioPinValue.High);
            PinB.Write(GpioPinValue.High);
            LoggingProcessor.AddTrace("End LightsOn");
        }

        public void LightsOff()
        {
            LoggingProcessor.AddTrace("Start LightsOff");
            PinR.Write(GpioPinValue.Low);
            PinG.Write(GpioPinValue.Low);
            PinB.Write(GpioPinValue.Low);
            LoggingProcessor.AddTrace("End LightsOff");
        }

        //public void Init(PwmController pwmController, GpioController gpio)
        //{
        //    try
        //    {



        //        Servo.Init(pwmController);


        //    }
        //    catch (Exception ex)
        //    {

        //        throw new Exception("The RGB controller threw an exception:" + ex.Message);
        //    }

        //}
    }
}
