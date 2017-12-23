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

namespace SupplementProcessor
{
    /// <summary>
    /// Interaction logic for Vector2DField.xaml
    /// </summary>
    public partial class Vector2DField : UserControl
    {
        public event KeyEventHandler OnFieldKeyUp;
        public string Caption
        {
            set { LabelCaption.Content = value; }
            get { return (String)LabelCaption.Content; }
        }

        public double X
        {
            set { TextBoxX.Text = value.ToString(); }
            get
            {
                double result = 0;
                double.TryParse(TextBoxX.Text, out result);
                return result;
            }
        }

        public double Y
        {
            set { TextBoxY.Text = value.ToString(); }
            get
            {
                double result = 0;
                double.TryParse(TextBoxY.Text, out result);
                return result;
            }
        }

        public string XLabelContent
        {
            set { XLabel.Content = value; }
            get { return XLabel.Content.ToString(); }
        }

        public string YLabelContent
        {
            set { YLabel.Content = value; }
            get { return YLabel.Content.ToString(); }
        }

        public Vector2DField()
        {
            InitializeComponent();
        }

        public Point Value
        {
            get
            {
                double x, y;
                double.TryParse(TextBoxX.Text, out x);
                double.TryParse(TextBoxY.Text, out y);
                return new Point(x, y);
            }
            set
            {
                TextBoxX.Text = value.X.ToString();
                TextBoxY.Text = value.Y.ToString();
            }
        }

        public Point ValueInPixel
        {
            //get { return new Point(Math.Ceiling(Value.X * MainWindow.DpiX / 2.54d / 100d), Math.Ceiling(Value.Y * MainWindow.DpiY / 2.54d / 100d)); }
            get { return new Point((Value.X * MainWindow.DpiX / 2.54d / 100d), (Value.Y * MainWindow.DpiY / 2.54d / 100d)); }
            set
            {
                TextBoxX.Text = (value.X / MainWindow.DpiX * 2.54d * 100d).ToString();
                TextBoxY.Text = (value.Y / MainWindow.DpiY * 2.54d * 100d).ToString();
            }
        }

        internal Point ToPoint()
        {
            return Value;
        }

        internal void FromPixel(double x, double y)
        {
            X = x / MainWindow.DpiX * 2.54d * 100d;
            Y = y / MainWindow.DpiY * 2.54d * 100d;
        }

        private void TextBoxX_KeyUp(object sender, KeyEventArgs e)
        {
            if (OnFieldKeyUp != null)
                OnFieldKeyUp(this, e);
        }

        private void TextBoxY_KeyUp(object sender, KeyEventArgs e)
        {
            if (OnFieldKeyUp != null)
                OnFieldKeyUp(this, e);
        }
    }
}
