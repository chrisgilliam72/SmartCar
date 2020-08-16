using Microsoft.IoT.Lightning.Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraborgLib;
using Windows.Devices;
using Windows.Devices.Pwm;

namespace PICarHostService
{
    class CameraController
    {

        private UltraborgServo HorizontalServo { get; set; }
        private UltraborgServo VerticalServo { get; set; }



        public CameraController()
        {
            HorizontalServo = new UltraborgServo(3,-0.05);
            VerticalServo = new UltraborgServo(4,0.05);

        }

        public void ProcessQuery(String requestedAction)
        {
            Debug.WriteLine("Camera controller action:" + requestedAction);
            LoggingProcessor.AddTrace("Camera controller action:" + requestedAction);
            switch (requestedAction)
            {
                case "left": HorizontalServo.RotateLeft();break;
                case "right": HorizontalServo.RotateRight();break ;
                case "up": VerticalServo.RotateLeft(); break;
                case "down": VerticalServo.RotateRight(); break;
            }

            Debug.WriteLine("Camera controller action completed");
            LoggingProcessor.AddTrace("Camera controller action completed");
        }


        public void Init(Ultraborg ultraborg)
        {
            try
            {

                HorizontalServo.Init(ultraborg);
                HorizontalServo.Test();

                VerticalServo.Init(ultraborg);
                VerticalServo.Test();



            }
            catch  (Exception ex)
            {
                throw new Exception("The camera controller threw an exception:" + ex.Message);
            }
        }

      
    }
}
