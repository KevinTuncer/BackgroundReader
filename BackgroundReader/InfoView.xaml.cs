using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;

namespace BackgroundReader
{
    /// <summary>
    /// Interaktionslogik für InfoView.xaml
    /// </summary>
    public partial class InfoView : Window
    {
        readonly MainWindow m;
        public InfoView(MainWindow M)         
        {
            m = M;
            InitializeComponent();
            textBox_hilfe.Text = "Vorlesen: strg + c\nVorlesen stoppen: strg + alt\nWort übersetzen: strg + alt + u\nText googlen: strg + alt + g\nMax/Minimieren: strg + alt + m\nNächste stimme: strg + alt + n\nLetzte Stimme: strg + alt + b\nStimmen-Nr: strg + alt + NumPad\nSchneller: strg + alt + +\nLangsamer: strg + alt + -\nLauter: strg + alt + .\nLeiser: strg + alt + ,\nBeenden: strg + alt + q\nHilfe: strg + alt + h";
        }

        enum Item { Speed, Volume, Number, Help };

        private static readonly int[] idS = [0, 0, 0, 0];
        private void Appear(Item item)
        {
            rectangle_hilfe.Visibility = System.Windows.Visibility.Hidden;
            textBox_hilfe.Visibility = System.Windows.Visibility.Hidden;

            rectangle_nr.Visibility = System.Windows.Visibility.Hidden;
            label_nr1.Visibility = System.Windows.Visibility.Hidden;
            label_nr2.Visibility = System.Windows.Visibility.Hidden;
            label_nr3.Visibility = System.Windows.Visibility.Hidden;
            
            label_volumen.Visibility = System.Windows.Visibility.Hidden;
            slider_volume.Visibility = System.Windows.Visibility.Hidden;
            rectangle_volumen.Visibility = System.Windows.Visibility.Hidden;
            
            label_speed.Visibility = System.Windows.Visibility.Hidden;
            rectangle_speed.Visibility = System.Windows.Visibility.Hidden;

            switch (item)
            {
                case Item.Number:
                    rectangle_nr.Visibility = System.Windows.Visibility.Visible;
                    label_nr1.Visibility = System.Windows.Visibility.Visible;
                    label_nr2.Visibility = System.Windows.Visibility.Visible;
                    label_nr3.Visibility = System.Windows.Visibility.Visible;
                    break;
                case Item.Volume:
                    label_volumen.Visibility = System.Windows.Visibility.Visible;
                    slider_volume.Visibility = System.Windows.Visibility.Visible;
                    rectangle_volumen.Visibility = System.Windows.Visibility.Visible;
                    break;
                case Item.Speed:
                    label_speed.Visibility = System.Windows.Visibility.Visible;
                    rectangle_speed.Visibility = System.Windows.Visibility.Visible;
                    break;
                case Item.Help:
                    rectangle_hilfe.Visibility = System.Windows.Visibility.Visible;
                    textBox_hilfe.Visibility = System.Windows.Visibility.Visible;
                    break;
            }

            this.Visibility = System.Windows.Visibility.Visible;
            Thread thread = new((obj) =>
            {
                if (obj != null)
                {
                    Disappear(obj);
                }
            });
            thread.Start(item);
        }

        private void Disappear(object item)
        {
            if (idS[(int)item] > 10000) idS[(int)item] = 0;

            int id = ++idS[(int)item];

            Thread.Sleep(2000);

            Dispatcher.Invoke(new Action(delegate()
            {                
                switch ((Item)item)
                {
                    case Item.Number:
                        if (id == idS[(int)Item.Number])
                        {
                            rectangle_nr.Visibility = System.Windows.Visibility.Hidden;
                            label_nr1.Visibility = System.Windows.Visibility.Hidden;
                            label_nr2.Visibility = System.Windows.Visibility.Hidden;
                            label_nr3.Visibility = System.Windows.Visibility.Hidden;
                        }
                        break;
                    case Item.Volume:
                        if (id == idS[(int)Item.Volume])
                        {
                            label_volumen.Visibility = System.Windows.Visibility.Hidden;
                            slider_volume.Visibility = System.Windows.Visibility.Hidden;
                            rectangle_volumen.Visibility = System.Windows.Visibility.Hidden;
                        }
                        break;
                    case Item.Speed:
                        if (id == idS[(int)Item.Speed])
                        {
                            label_speed.Visibility = System.Windows.Visibility.Hidden;
                            rectangle_speed.Visibility = System.Windows.Visibility.Hidden;
                        }
                        break;
                    case Item.Help:
                        if (id == idS[(int)Item.Help])
                        {
                            rectangle_hilfe.Visibility = System.Windows.Visibility.Hidden;
                            textBox_hilfe.Visibility = System.Windows.Visibility.Hidden;
                        }
                        break;
                }

                if (label_nr2.Visibility == System.Windows.Visibility.Hidden && label_volumen.Visibility == System.Windows.Visibility.Hidden && label_speed.Visibility == System.Windows.Visibility.Hidden && rectangle_hilfe.Visibility == System.Windows.Visibility.Hidden)
                {
                    this.Visibility = System.Windows.Visibility.Hidden;
                }
            }));
        }

        private void Slider_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m.slider_Volume != null && m.slider_Volume.Value != e.NewValue)
            {
                m.slider_Volume.Value = e.NewValue;
            }
            label_volumen.Content = (int)e.NewValue;

            Appear(Item.Volume);
        }

        public void Label_nr_Changed()
        {
            Appear(Item.Number);
        }

        public void Label_speed_Changed()
        {  
            Appear(Item.Speed);
        }

        public void Help_view()
        {
            Appear(Item.Help);
        }
    }
}
