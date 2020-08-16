using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;
using Windows.Devices.Gpio;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using Windows.UI.Popups;
using System.Threading;
using System.Diagnostics;

namespace PICarHostService
{
    class MotorProcessor
    {
        //const int Left_motor_pwm = 17;
        //const int Right_motor_pwm = 18;

        //const int Left_motor_go = 27;
        //const int Left_motor_back = 22;
        //const int Right_motor_go = 23;
        //const int Right_motor_back = 24;

        const int Left_motor_pwm = 16;
        const int Right_motor_pwm = 13;

        const int Left_motor_go = 19;
        const int Left_motor_back = 6;
        const int Right_motor_go = 20;
        const int Right_motor_back = 21;


        private GpioPin pinLeftGo;
        private GpioPin pinLeftBack;
        private GpioPin pinRightGo;
        private GpioPin pinRightBack;
        private PwmPin pinLeftMotorPWM;
        private PwmPin pinRightMotorPWM;
        public double SpeedFactor { get; set; }

       readonly ManualResetEvent CommandsWaitingEvent = new ManualResetEvent(false);

        public List<MotorCommand> CommandQueue { get; set; }

        public MotorProcessor()
        {
            CommandQueue = new List<MotorCommand>();
            SpeedFactor = 0.5;
        }

        public String Init(PwmController pwmController, GpioController gpio)
        {
            try
            {
              
                pinLeftMotorPWM = pwmController.OpenPin(Left_motor_pwm);
                pinLeftMotorPWM.Polarity = PwmPulsePolarity.ActiveHigh;
                pinLeftMotorPWM.SetActiveDutyCyclePercentage(0.5);
                pinLeftMotorPWM.Start();

                pinRightMotorPWM = pwmController.OpenPin(Right_motor_pwm);
                pinRightMotorPWM.Polarity = PwmPulsePolarity.ActiveHigh;
                pinRightMotorPWM.SetActiveDutyCyclePercentage(0.5);
                pinRightMotorPWM.Start();

                pinLeftGo = gpio.OpenPin(Left_motor_go);
                pinLeftGo.Write(GpioPinValue.Low);
                pinLeftGo.SetDriveMode(GpioPinDriveMode.Output);

                pinLeftBack = gpio.OpenPin(Left_motor_back);
                pinLeftBack.Write(GpioPinValue.Low);
                pinLeftBack.SetDriveMode(GpioPinDriveMode.Output);


                pinRightGo = gpio.OpenPin(Right_motor_go);
                pinRightGo.Write(GpioPinValue.Low);
                pinRightGo.SetDriveMode(GpioPinDriveMode.Output);

                pinRightBack = gpio.OpenPin(Right_motor_back);
                pinRightBack.Write(GpioPinValue.Low);
                pinRightBack.SetDriveMode(GpioPinDriveMode.Output);

                var actionProcMessages = new Action(ProcessQueueMessages);
                Task.Run(actionProcMessages);
              

                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("The motor processor threw an exception:" + ex.Message);
            }
        }

        public void ProcessQuery(String requestedAction, double parameterValue)
        {
           
            var motorCommand = new MotorCommand { Command = requestedAction, Parameter = parameterValue };
            CommandQueue.Add(motorCommand);

            CommandsWaitingEvent.Set();
        }

        public void ProcessQueueMessages()
        {
            Debug.WriteLine("Queue Processor has started");
            while (true)
            {
                CommandsWaitingEvent.WaitOne();
                do
                {
                    var nextCommand = CommandQueue.FirstOrDefault();
                    if (nextCommand != null)
                    {
                        CommandQueue.Remove(nextCommand);
                        ExcecuteCommand(nextCommand.Command, nextCommand.Parameter);
                    }
                }
                while (CommandQueue.Count > 0);
                CommandsWaitingEvent.Reset();
            }

           
        }

        public void ExcecuteCommand(String requestedAction, double parameterValue)
        {
            Debug.WriteLine("Executing motor processor action:" + requestedAction);
            LoggingProcessor.AddTrace("Executing motor processor action:" + requestedAction);
            switch (requestedAction)
            {
                case "forward":  GoForward(parameterValue); break;
                case "back":  GoBackwards(parameterValue); break;
                case "right":  TurnRight(parameterValue); ; break;
                case "left":  TurnLeft(parameterValue); break;
                case "speed": UpdateSpeedFactor(parameterValue); break;
                case "stop": Stop(); break;
            }

            Debug.WriteLine("Motor processor action completed");
            LoggingProcessor.AddTrace("Motor processor action completed");
        }
        
        public void UpdateSpeedFactor(double speedFactor)
        {
            Debug.WriteLine("updating speed factor :"+ speedFactor);
            LoggingProcessor.AddTrace("updating speed factor :" + speedFactor);
            this.SpeedFactor = speedFactor;
            pinLeftMotorPWM.SetActiveDutyCyclePercentage(speedFactor);
            pinRightMotorPWM.SetActiveDutyCyclePercentage(speedFactor);
            Debug.WriteLine("speed factor updated");
            LoggingProcessor.AddTrace("speed factor updated");
        }

        public void GoForward(double seconds)
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                StartForward();
                if (seconds > 0)
                {
                    Debug.WriteLine("forward delay start :" + seconds);
                    LoggingProcessor.AddTrace("forward delay start :" + seconds);
                    mre.Wait(TimeSpan.FromSeconds(seconds));
                    Debug.WriteLine("forward delay complete");
                    LoggingProcessor.AddTrace("forward delay complete");
                    Stop();
                }
            }
        }

        public void GoBackwards(double seconds)
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                StartBackwards();
                if (seconds > 0)
                {
                    Debug.WriteLine("backwards delay start :" + seconds);
                    LoggingProcessor.AddTrace("backwards delay start :" + seconds);
                    mre.Wait(TimeSpan.FromSeconds(seconds));
                    Debug.WriteLine("backwards backwards delay complete ");
                    LoggingProcessor.AddTrace("backwards delay complete");
                    Stop();
                }
            }

        }

        public void TurnRight(double seconds)
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                StartTurnRight();
                if (seconds > 0)
                {
                    Debug.WriteLine("turn right delay start :" + seconds);
                    LoggingProcessor.AddTrace("turn right start :" + seconds);
                    mre.Wait(TimeSpan.FromSeconds(seconds));
                    Debug.WriteLine("turn right end ");
                    LoggingProcessor.AddTrace("turn right end ");
                    Stop();
                    UpdateSpeedFactor(SpeedFactor);
                }
            }
        }

        public void TurnLeft(double seconds)
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                StartTurnLeft();
                if (seconds > 0)
                {
                    Debug.WriteLine("turn left delay start :" + seconds);
                    LoggingProcessor.AddTrace("turn left start :" + seconds);
                    mre.Wait(TimeSpan.FromSeconds(seconds));
                    Debug.WriteLine("turn left end :" + seconds);
                    LoggingProcessor.AddTrace("turn left end ");
                    Stop();
                    UpdateSpeedFactor(SpeedFactor);
                }
            }

        }

        public void StartForward()
        {
            Debug.WriteLine("Start Forward Start");
            LoggingProcessor.AddTrace("Start Forward Start");
            pinLeftGo.Write(GpioPinValue.High);
            pinLeftBack.Write(GpioPinValue.Low);
            pinRightGo.Write(GpioPinValue.High);
            pinRightBack.Write(GpioPinValue.Low);
            Debug.WriteLine("Start Forward End");
            LoggingProcessor.AddTrace("Start Forward End");

        }


        public void StartBackwards()
        {
            Debug.WriteLine("Start Backward Start");
            LoggingProcessor.AddTrace("Start Backward Start");
            pinLeftGo.Write(GpioPinValue.Low);
            pinLeftBack.Write(GpioPinValue.High);
            pinRightGo.Write(GpioPinValue.Low);
            pinRightBack.Write(GpioPinValue.High);
            Debug.WriteLine("Start Backward End");
            LoggingProcessor.AddTrace("Start Backward End");

        }

        public void StartTurnRight()
        {
            Debug.WriteLine("Start TurnRight Start");
            LoggingProcessor.AddTrace("Start TurnRight Start");
            pinRightMotorPWM.SetActiveDutyCyclePercentage(0.2);
            pinRightGo.Write(GpioPinValue.High);
            pinRightBack.Write(GpioPinValue.Low);
            pinLeftBack.Write(GpioPinValue.Low);
            pinLeftGo.Write(GpioPinValue.Low);
            Debug.WriteLine("Start TurnRight End");
            LoggingProcessor.AddTrace("Start TurnRight End");

        }

        public void StartTurnLeft()
        {
            Debug.WriteLine("Start TurnLeft Start");
            LoggingProcessor.AddTrace("Start TurnLeft Start");
            pinLeftMotorPWM.SetActiveDutyCyclePercentage(0.2);
            pinLeftGo.Write(GpioPinValue.High);
            pinLeftBack.Write(GpioPinValue.Low);
            pinRightGo.Write(GpioPinValue.Low);
            pinRightBack.Write(GpioPinValue.Low);
            Debug.WriteLine("Start TurnLeft End");
            LoggingProcessor.AddTrace("Start TurnLeft End");

        }

        public void Stop()
        {
            Debug.WriteLine("Sending stop signal");
            LoggingProcessor.AddTrace("Sending stop signal");
            pinLeftGo.Write(GpioPinValue.Low);
            pinRightGo.Write(GpioPinValue.Low);
            pinLeftBack.Write(GpioPinValue.Low);
            pinRightBack.Write(GpioPinValue.Low);
            UpdateSpeedFactor(this.SpeedFactor);
            Debug.WriteLine("stopped");
            LoggingProcessor.AddTrace("stop");
        }
    }

}
