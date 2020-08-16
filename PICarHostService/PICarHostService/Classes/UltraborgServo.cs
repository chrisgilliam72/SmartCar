using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UltraborgLib;

namespace PICarHostService
{
    class UltraborgServo
    {
        private int ServoNo { get; set; }

        private int ServoMax { get; set; }

        private int ServoMin { get; set; }

        private double ServoNeutral { get; set; }

        private Ultraborg Ultraborg { get; set; }

        private double _position { get; set; }

        public UltraborgServo(int servoNo, double servoNeutral)
        {
            ServoNo = servoNo;
            ServoNeutral = servoNeutral;
            _position = servoNeutral;
        }

        public void Init(Ultraborg ultraborg)
        {
            Ultraborg = ultraborg;
            var limitsServo = Ultraborg.GetServoLimits(ServoNo);

            ServoMax = limitsServo.Maximum;
            ServoMin = limitsServo.Minimum;
        }

        public void Test()
        {
            var mre = new ManualResetEventSlim(false);
            using (mre)
            {
                ServoTo270();
                mre.Wait(TimeSpan.FromMilliseconds(500));
                mre.Reset();
                ServoTo90();
                mre.Wait(TimeSpan.FromMilliseconds(500));
                mre.Reset();
                ServoTo0();
                mre.Wait(TimeSpan.FromMilliseconds(500));
                mre.Reset();

            }          
          
        }

        public void RotateRight()
        {
            if (_position<1)
            {
                _position += 0.25;
            }

            Ultraborg.SetServoPosition(ServoNo, _position, ServoMin, ServoMax);
        }

        public void RotateLeft()
        {
            if (_position>-1)
                _position -= 0.25;
            Ultraborg.SetServoPosition(ServoNo, _position, ServoMin, ServoMax);
        }

        private void ServoToPosition(int pos)
        {
            Ultraborg.SetServoPosition(pos, -1, ServoMin, ServoMax);
        }

        public void ServoTo270()
        {
            Ultraborg.SetServoPosition(ServoNo, -1, ServoMin, ServoMax);
        }

        public void ServoTo0()
        {
            Ultraborg.SetServoPosition(ServoNo, ServoNeutral, ServoMin, ServoMax);
        }

        public void ServoTo90()
        {
            Ultraborg.SetServoPosition(ServoNo, 1, ServoMin, ServoMax);
        }


    }
}
