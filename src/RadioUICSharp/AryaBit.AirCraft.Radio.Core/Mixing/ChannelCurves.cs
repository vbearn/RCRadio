using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AryaBit.AirCraft.Radio.Core
{
    class ChannelCurves
    {

        public static double LinearWithRange(double positiveInputValue, double maxInputValue, double rangeMin, double rangeMax)
        {
            return LinearWithRange(positiveInputValue, 0, maxInputValue, rangeMin, rangeMax);
        }
        public static double LinearWithRange(double inputValue, double minInputValue, double maxInputValue, double rangeMin, double rangeMax)
        {
            return ((inputValue - minInputValue) / (maxInputValue - minInputValue) * (rangeMax - rangeMin)) + rangeMin;
        }

        // y = 0.5x^3 + 0.6x
        public static double SymmetricDegree3(double input)
        {
            return (0.000001 * Math.Pow(input, 3) + 0.01 * Math.Pow(input, 1));
        }

        public static double DoChannelCurve(double positiveInputValue, double maxInputValue, CurveMode curveMode, double rangeMin, double rangeMax)
        {
            double mappedValue;
            switch (curveMode)
            {
                case CurveMode.Linear:
                    mappedValue = LinearWithRange(positiveInputValue, maxInputValue, rangeMin, rangeMax);
                    break;
                case CurveMode.SymmetricDegree3:

                    double mppositiveInputValue = LinearWithRange(positiveInputValue, maxInputValue, 0, 2000);
                    double mpmaxInputValue = 2000;

                    double diagVal = SymmetricDegree3(mppositiveInputValue - mpmaxInputValue / 2);
                    double diagMax = SymmetricDegree3(mpmaxInputValue / 2);

                    //mappedValue = LinearWithRange(diagVal, diagMax, rangeMin, rangeMax);

                    mappedValue = LinearWithRange(diagVal , -diagMax, diagMax, 0, maxInputValue);
                    //mappedValue += maxInputValue / 2;
                    mappedValue = LinearWithRange(mappedValue, maxInputValue, rangeMin, rangeMax);
                    break;
                default:
                    throw new Exception();
            }
            return mappedValue;
        }

    }
}
