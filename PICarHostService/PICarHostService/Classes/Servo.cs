using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices;
using Windows.Devices.Pwm;

namespace PICarHostService
{
    class Servo
    {
     
        private double currentPos { get; set; }
        public String Name { get; set; }
        public PwmPin PowerPin { get; set; }
        public int PINNo { get; set; }

        public Servo(String ServoName, int GPIOPin)
        {
            Name = ServoName;
            PINNo = GPIOPin;
        }


        public void Test()
        {
            ServoTo270();
            ServoTo90();
            ServoTo0();
        }

        public async Task Init()
        {
            try
            {
                if (LightningProvider.IsLightningEnabled)
                {
                    LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
                    var pwmControllers = await PwmController.GetControllersAsync(LightningPwmProvider.GetPwmProvider());


                    var pwmController = pwmControllers[1]; // use the on-device controller
                    pwmController.SetDesiredFrequency(50);

                    PowerPin = pwmController.OpenPin(PINNo);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(Name + " servo class through an exception:" + ex.Message);
            }
        }


        public void Init(PwmController pwrController)
        {
            try
            {
                PowerPin = pwrController.OpenPin(PINNo);
            }
            catch (Exception ex)
            {

                throw new Exception(Name + " servo class through an exception:" + ex.Message);
            }
        }

        public void RotateRight()
        {
            if (currentPos >0.1)
                ServoToPosition(currentPos - 0.4);

        }

        public void RotateLeft()
        {
            if (currentPos <2.5)
                ServoToPosition(currentPos + 0.4);
        }

        private void ServoToPosition(double pos)
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                PowerPin.Start();
                PowerPin.SetActiveDutyCyclePercentage((double)(pos / 20.0));
                mre.Wait(TimeSpan.FromMilliseconds(500));
                PowerPin.Stop();
                currentPos = pos;
            }
;
        }

        public void ServoTo270()
        {
            var mre = new ManualResetEventSlim(false);
            using(mre)
            {
                PowerPin.Start();
                PowerPin.SetActiveDutyCyclePercentage((double)(2.5 / 20.0));
                mre.Wait(TimeSpan.FromMilliseconds(500));
                PowerPin.Stop();
                currentPos = 2.5;
            }

        }

        public void ServoTo0()
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                PowerPin.Start();
                PowerPin.SetActiveDutyCyclePercentage((double)(1.6 / 20.0));
                mre.Wait(TimeSpan.FromMilliseconds(500));
                PowerPin.Stop();
                currentPos = 1.6;
            }

        }

        public void ServoTo90()
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                PowerPin.Start();
                PowerPin.SetActiveDutyCyclePercentage((double)(0.5 / 20.0));
                mre.Wait(TimeSpan.FromMilliseconds(500));
                PowerPin.Stop();
                currentPos = 0.5;
            }

        }


        public void ServoToBack()
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                PowerPin.Start();
                PowerPin.SetActiveDutyCyclePercentage((double)(2.0 / 20.0));
                mre.Wait(TimeSpan.FromMilliseconds(500));
                PowerPin.Stop();
            }

        }

    }
}
