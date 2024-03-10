using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment_1
{
    internal class Sensor
    {
        // Class for analog and digital sensors
        // Made by Kim Langvannskås
        // Last modified 27.01.2024

        double dVal;
        int sId;
        Random rSenVal;
        bool sDigital;
        public Sensor(int id, bool digital)
        {
            // Initializing a sensor
            sId = id;
            rSenVal = new Random(id);
            dVal = 0.5F;
            sDigital = digital;
        }
        public double GetValue()
        {
            // Returns a new value for the sensor
            double nextVal = rSenVal.NextDouble();
            if (sDigital) 
            { 
                if (nextVal < 0.5) { return 0; }
                else { return 1; }
            }
            else
            {
                dVal += (nextVal * 0.2 - 0.1);
                if (dVal < 0.0) { dVal = 0.0; }
                else if (dVal > 1.0){ dVal = 1.0; }
                return Math.Round(dVal, 2);
            }
        }
        public int GetSensId()
        {
            // Returns id tag of sensor
            return sId;
        }
    }
}
