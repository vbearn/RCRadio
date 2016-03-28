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
    /// Interaction logic for TuneSlider.xaml
    /// </summary>
    public partial class TuneSlider : UserControl
    {

        public event RoutedPropertyChangedEventHandler<double> ValueChanged;
        public double Maximum
        {
            get { return sld.Maximum; }
            set { sld.Maximum = value; }
        }
        public double Minimum
        {
            get { return sld.Minimum; }
            set { sld.Minimum = value; }
        }
        public double Value
        {
            get { return sld.Value; }
            set { sld.Value = value; }
        }

        public TuneSlider()
        {
            InitializeComponent();

        }

        private void sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (btnTune != null)
                btnTune.Content = e.NewValue;

            if (ValueChanged != null)
                ValueChanged(sender, e);
        }

        private void btnTune_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new InputDialog("Enter the value:", Value.ToString());
            if (inputDialog.ShowDialog() == true)
            {
                double parsedValue;
                if (double.TryParse(inputDialog.Answer, out parsedValue))
                {
                    Value = parsedValue;
                }
            }
        }
    }
}
