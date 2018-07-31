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
    /// Interaction logic for Plot.xaml
    /// </summary>
    public partial class Plot : UserControl
    {


        private double maxX = 90;
        public double MaxX
        {
            get { return this.maxX; }
            set
            {
                this.maxX = value;
                RefreshPoints();
            }
        }
        private double maxY = 90;
        public double MaxY
        {
            get { return this.maxY; }
            set
            {
                this.maxY = value;
                RefreshPoints();
            }
        }

        private double pointX = 90;
        public double PointX
        {
            get { return this.pointX; }
            set
            {
                this.pointX = value;
                RefreshPoints();
            }
        }

        private double pointY = 90;
        public double PointY
        {
            get { return this.pointY; }
            set
            {
                this.pointY = value;
                RefreshPoints();
            }
        }

        private void RefreshPoints()
        {
            try
            {

                Canvas.SetLeft(circlePoint, pointX / maxX * this.Width);
                Canvas.SetTop(circlePoint, pointY / maxY * this.Height);

            }
            catch (Exception)
            {
            }
        }


        public Plot()
        {
            InitializeComponent();
        }
    }
}
