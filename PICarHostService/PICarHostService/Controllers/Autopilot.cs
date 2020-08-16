using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PICarHostService
{
    class Autopilot
    {
        public MotorProcessor MotorProcessor { get; set; }
        public SonicSensorController SonicController { get; set; }

        private Timer _distanceTimer;

        public double LastDistance;
        public void ProcessQuery(String requestedAction, double parameterValue)
        {
           
            Debug.WriteLine("Autopilot action:" + requestedAction+" , parameter:"+ parameterValue);
            LoggingProcessor.AddTrace("Autopilot action:" + requestedAction + " , parameter:" + parameterValue);
            switch (requestedAction)
            {
                case "go": Go((int)parameterValue, 0.1);break;
            }

            Debug.WriteLine("Autopilot action completed");
            LoggingProcessor.AddTrace("Autopilot action completed");
           
        }

        public Autopilot(MotorProcessor motorProcessor, SonicSensorController sonicController)
        {
            MotorProcessor = motorProcessor;
            SonicController = sonicController;
            _distanceTimer = new Timer(TimeOutGetDistance, null, Timeout.Infinite, Timeout.Infinite);
        }

        private void Go (int seconds, double speedFactor)
        {

            _distanceTimer.Change(100,500);
            //210cm in 5 s at 50% speed 52cm/s at 50% speed

            MotorProcessor.SpeedFactor = speedFactor;
            MotorProcessor.StartForward();


        }

        public  void TimeOutGetDistance(object stateInfo)
        {
            Debug.WriteLine("Autopilot TimeOutGetDistance");
            LoggingProcessor.AddTrace("Autopilot TimeOutGetDistance");
            LastDistance =  SonicController.MeasureAverageDistance();

            Debug.WriteLine("Autopilot distance measured:"+ LastDistance);
            LoggingProcessor.AddTrace("Autopilot distance measured: "+ LastDistance);
            if (LastDistance < 20)
            {
                _distanceTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Debug.WriteLine("Autopilot stopping motor");
                LoggingProcessor.AddTrace("Autopilot stopping motor");
                MotorProcessor.Stop();
              
            }
             
        }
    }
}
