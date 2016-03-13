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
    /// Interaction logic for TouchJoystick.xaml
    /// </summary>
    public partial class TouchJoystick : UserControl
    {
        public TouchJoystick()
        {
            InitializeComponent();
            SetControlPositions();

        }

        private double maxXValue = 180;
        private double maxYValue = 180;
        private double xValue = 0;
        public double XValue
        {
            get { return this.xValue; }
            set
            {
                this.xValue = value;
                SetControlPositions();
            }
        }

        private double yValue = 0;
        public double YValue
        {
            get { return this.yValue; }
            set
            {
                this.yValue = value;
                SetControlPositions();
            }
        }

        private void SetControlPositions()
        {
            try
            {
                imgHAxis.Margin = new Thickness(
                    imgHAxis.Margin.Left,
                    CalculateYTop(imgHAxis.Height),
                    0,
                    0);

                imgKnob.Margin = new Thickness(
                    CalculateXLeft(imgKnob.Width),
                    CalculateYTop(imgKnob.Height),
                    imgKnob.Margin.Right,
                    imgKnob.Margin.Bottom);
            }
            catch (Exception)
            {
            }

        }

        private double CalculateYTop(double controlHeight)
        {
            return (this.Height / 2 - controlHeight / 2) + (((this.yValue - this.maxYValue / 2) / (this.maxYValue / 2)) * this.Height / (double)5.5);
        }

        private double CalculateXLeft(double controlWidth)
        {
            double x = (this.Width / 2 - controlWidth / 2) + (((this.xValue - this.maxXValue / 2) / (this.maxXValue / 2)) * this.Width / (double)5.5);
            Console.WriteLine(x);
            return x;
        }
    }
}
