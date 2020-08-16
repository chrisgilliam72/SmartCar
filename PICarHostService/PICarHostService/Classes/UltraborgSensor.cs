using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltraborgLib;

namespace PICarHostService
{
    class UltraborgSensor
    {
        private int SensorNo { get; set; }
        private Ultraborg Ultraborg { get; set; }

        public UltraborgSensor(int sensorNo)
        {
            SensorNo = sensorNo;
        }

        public void Init(Ultraborg ultraborg)
        {
            Ultraborg = ultraborg;

        }

        public double GetDistance()
        {
            return Ultraborg.GetDistance(SensorNo);
        }
    }
}
