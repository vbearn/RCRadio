using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AryaBit.AirCraft.Radio.UI.Controls
{
    /// <summary>
    /// Interaction logic for VTailViewer.xaml
    /// </summary>
    public partial class VTailViewer : UserControl
    {


        private double leftValue = 90;
        public double LeftValue
        {
            get { return this.leftValue; }
            set
            {
                this.leftValue = value;
                RefreshLines();
            }
        }


        private double rightValue = 90;
        public double RightValue
        {
            get { return this.rightValue; }
            set
            {
                this.rightValue = value;
                RefreshLines();
            }
        }

        private double rightAngleOffset = -45;
        public double RightAngleOffset
        {
            get { return this.rightAngleOffset; }
            set
            {
                this.rightAngleOffset = value;
                RefreshLines();
            }
        }


        private double leftAngleOffset = +45;
        public double LeftAngleOffset
        {
            get { return this.leftAngleOffset; }
            set
            {
                this.leftAngleOffset = value;
                RefreshLines();
            }
        }


        Point leftThingCenter, rightThingCenter;
        double thingRadius;

        public VTailViewer()
        {
            InitializeComponent();

            leftThingCenter = new Point(thingLeft.X2, thingLeft.Y2);
            rightThingCenter = new Point(thingRight.X2, thingRight.Y2);

            thingRadius = Math.Sqrt(Math.Pow(thingLeft.X1 - thingLeft.X2, 2) + Math.Pow(thingLeft.Y1 - thingLeft.Y2, 2));

            RefreshLines();

        }

        private void RefreshLines()
        {
            try
            {

                Point leftThingyPos = GetPointInCircleCircumfence(leftValue + leftAngleOffset, leftThingCenter, thingRadius);
                thingLeft.X1 = leftThingyPos.X;
                thingLeft.Y1 = leftThingyPos.Y;

                Point rightThingyPos = GetPointInCircleCircumfence(rightValue + rightAngleOffset, rightThingCenter, thingRadius);
                thingRight.X1 = rightThingyPos.X;
                thingRight.Y1 = rightThingyPos.Y;

            }
            catch (Exception)
            {
            }
        }

        private Point GetPointInCircleCircumfence(double angle, Point center, double radius)
        {
            double angleRd = angle * 2 * Math.PI / (double)360 ;

            Point circPoint = new Point();
            circPoint.X = center.X + radius * Math.Cos(angleRd);
            circPoint.Y = center.Y - radius * Math.Sin(angleRd);

            return circPoint;
        }

    }
}
