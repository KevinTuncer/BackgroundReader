/*
 *  This project is a simple background reader that reads the text from the clipboard.
 *  It was one of my first C# projects as teenager, but I am still using it all the time. So, the code has a lot of potential for improvement.
 *  
 *  Author: Kevin Kei Tuncer
 *
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Speech.Synthesis;
using System.Threading;
using System.Collections.ObjectModel;
using System.Diagnostics;
//using System.Speech.Recognition;
using System.Text;
using NTextCat;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace BackgroundReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly int select = 0;
        readonly ReadOnlyCollection<InstalledVoice> voices;
        bool isTextTranslator = false;

        private static readonly SpeechSynthesizer synthesizer = new();

        private void ReadCopy()
        {
            SetPlayOrPause(true);   // disable pause
            Stop();

            Thread.Sleep(100);
            string text;
            try
            {
                IDataObject iData = Clipboard.GetDataObject();

                text = Convert.ToString(iData?.GetData(DataFormats.UnicodeText)) ?? "";
            }
            catch
            {
                text = "";
            }
            textBox_ReadContent.Text = text;

            Read(text);
        }

        private void AddOnAddress(string adress)
        {
            if (!isTextTranslator)
            {
                isTextTranslator = true;
                IDataObject iData = Clipboard.GetDataObject();
                Process.Start(adress + Convert.ToString(iData.GetData(DataFormats.UnicodeText)));
                Thread.Sleep(1000);
                isTextTranslator = false;
            }
        }

        private void ChangeSpeed(int deltaspeed) // bei null dann = 0;
        {
            if (deltaspeed == 0)
                slider_Speed.Value = 0;
            else
                slider_Speed.Value += deltaspeed;
        }

        private void ChangeReader(int deltacombo)
        {
            if (deltacombo < 0 && ComboBox_voice.SelectedIndex > 0)
            {
                ComboBox_voice.SelectedIndex--;
            }
            else if (deltacombo > 0 && ComboBox_voice.SelectedIndex < ComboBox_voice.Items.Count)
            {
                ComboBox_voice.SelectedIndex++;
            }
        }

        private void SetReader(int comboNumber)
        {
            Dispatcher.Invoke(new Action(delegate ()
            {
                if (ComboBox_voice.Items.Count > comboNumber) { ComboBox_voice.SelectedIndex = comboNumber; };
            }));
        }

        bool canKeyRead = true;
        private void Global_KeyDown(object sender, RawKeyEventArgs e)
        {
            //handler codes go here as needed.
            Dispatcher.Invoke(new Action(delegate () {
        
                bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Key.LeftCtrl == e.Key || Key.RightCtrl == e.Key;
                if (isCtrl)
                {
                    if (!canKeyRead)
                        return;
                    canKeyRead = false;

                    bool isAlt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt) || Key.LeftAlt == e.Key || Key.RightAlt == e.Key;

                    bool isKey_C = Keyboard.IsKeyDown(Key.C) | e.Key == Key.C;
                    if (isKey_C)
                    {
                        ReadCopy();
                    }

                    if (isAlt)
                    {
                        switch (e.Key)
                        {
                            case Key.U:
                                AddOnAddress("http://www.dict.cc/?s=");
                                break;
                            case Key.G:
                                AddOnAddress("https://www.google.de/search?q=");
                                break;
                            case Key.P:
                                SetPlayOrPause(true);
                                break;
                            case Key.OemComma:
                                Stop();
                                slider_Volume.Value--;
                                break;
                            case Key.OemPeriod:
                                Stop();
                                slider_Volume.Value++;
                                break;
                            case Key.M:
                                // Fenster ein und ausblenden
                                Visibility_window();
                                break;
                            case Key.Add:
                                if (Keyboard.IsKeyDown(Key.Subtract))
                                    ChangeSpeed(0);
                                else
                                    ChangeSpeed(1);
                                break;
                            case Key.Subtract:
                                Stop();
                                if (Keyboard.IsKeyDown(Key.Add))
                                    ChangeSpeed(0);
                                else
                                    ChangeSpeed(-1);
                                break;
                            case Key.B:
                                Stop();
                                ChangeReader(-1);
                                break;
                            case Key.N:
                                ChangeReader(1);
                                break;
                            //case Key.Q:           
                            //    EndAllThreads();
                            //    this.Close();
                            //    break;
                            case Key.H:
                                infoView.Help_view();
                                break;
                            case Key.NumPad1: SetReader(1); break;
                            case Key.NumPad2: SetReader(2); break;
                            case Key.NumPad3: SetReader(3); break;
                            case Key.NumPad4: SetReader(4); break;
                            case Key.NumPad5: SetReader(5); break;
                            case Key.NumPad6: SetReader(6); break;
                            case Key.NumPad7: SetReader(7); break;
                            case Key.NumPad8: SetReader(8); break;
                            case Key.NumPad9: SetReader(9); break;
                            case Key.NumPad0: SetReader(0); break;

                            default:
                                SetPlayOrPause(false);
                                break;
                        }
                    }
                    canKeyRead = true;
                }
            }));
        }

        private static readonly KeyboardListener KListener = new();

        private void Set_Window_Title()
        {
            this.Title = "Kei's BackgroundReader - " + synthesizer.Voice.Description.ToString().Replace("IVONA 2", "").Trim();
        }

        readonly InfoView infoView;
        public MainWindow()
        {
            infoView = new InfoView(this);
            InitializeComponent();

            infoView.Left = System.Windows.SystemParameters.WorkArea.Width - infoView.Width;
            infoView.Top = System.Windows.SystemParameters.WorkArea.Height - infoView.Height;

            infoView.Show();
            int i = 0;
            voices = synthesizer.GetInstalledVoices();
            foreach (InstalledVoice inVoice in voices)
            {
                VoiceInfo vi = inVoice.VoiceInfo;
                try
                {
                    synthesizer.SelectVoice(vi.Name);   // mache stimmen können nicht benutzt werden, diese werden vorher auf Funktion getestet um die try funktion zu beenden

                    if (inVoice.Enabled)
                    {
                        string addString = vi.Description.ToString().Replace("IVONA 2", "").Trim();
                        ComboBox_voice.Items.Add(addString);

                        if (vi == synthesizer.Voice)
                        {
                            select = i;
                        }
                        i++;
                    }
                }
                catch { }
            }

            ComboBox_voice.SelectedIndex = select;

            Load_settings();

            Set_Window_Title();

            //Greating();

            CheckBox_Autostart.IsChecked = Autostart.IsAutostartEnabled(); 

            KListener.KeyDown += new RawKeyEventHandler(Global_KeyDown);

            Visibility = setting.startHidden ? Visibility.Hidden : Visibility.Visible;

            // Remove autostart of the old app name if it has been changed
            if(setting.autostartRegName.Length > 0 && setting.autostartRegName != Autostart.GetAppName())
            {
                Autostart.ToggleAutostart(false, Autostart.registryPath, setting.autostartRegName);
            }
        }

        #region TranslationFunctions

        private static string Gender_de(VoiceGender gender)
        {
            return gender switch
            {
                VoiceGender.Female => "Weiblich",
                VoiceGender.Male => "Männlich",
                VoiceGender.Neutral => "Neutrum",
                VoiceGender.NotSet => "Nicht definiert",
                _ => "",
            };
        }

        private static string Age_de(VoiceAge age)
        {
            return age switch
            {
                VoiceAge.Adult => "Erwahsener",
                VoiceAge.Child => "Kind",
                VoiceAge.Senior => "Senior",
                VoiceAge.Teen => "Jugendlicher",
                VoiceAge.NotSet => "Nicht definiert",
                _ => "",
            };
        }

        #endregion

        #region Geerting

        private void Greeting()
        {
            if (DateTime.Now.Hour < 3 || DateTime.Now.Hour >= 18)
            {
                Greeting_Evening();
            }
            else if (DateTime.Now.Hour < 18 && DateTime.Now.Hour >= 12)
            {
                Greeting_Noon();
            }
            else if (DateTime.Now.Hour < 12)
            {
                Greeting_Morning();
            }
        }

        private static string GetDateString()
        {
            if (synthesizer.Voice.Culture.Parent.ToString() == "de")
            {
                return "Es ist " + DateTime.Now.ToLongDateString() + " . " + DateTime.Now.ToShortTimeString();
            }
            else
                return "It's " + DateTime.Now.ToShortDateString() + " . " + DateTime.Now.ToShortTimeString();
        }

        private void Greeting_Evening()
        {
            if (synthesizer.Voice.Culture.Parent.ToString() == "de")
            {
                Read("Guten Abend, ich bin " + synthesizer.Voice.Name.Replace("IVONA 2", "").Trim() + ". " + GetDateString());
            }
            else
            {
                Read("Good evening, I am " + synthesizer.Voice.Name.Replace("IVONA 2", "").Trim() + ". " + GetDateString());
            }
        }
        private void Greeting_Morning()
        {
            if (synthesizer.Voice.Culture.Parent.ToString() == "de")
            {
                Read("Guten Morgen, ich bin " + synthesizer.Voice.Name.Replace("IVONA 2", "").Trim() + ". " + GetDateString());
            }
            else
            {
                Read("Good morning, I am " + synthesizer.Voice.Name.Replace("IVONA 2", "").Trim() + ". " + GetDateString());
            }
        }
        private void Greeting_Noon()
        {
            if (synthesizer.Voice.Culture.Parent.ToString() == "de")
            {
                Read("Guten Tag, ich bin " + synthesizer.Voice.Name.Replace("IVONA 2", "").Trim() + ". " + GetDateString());
            }
            else
            {
                Read("Good day, I am " + synthesizer.Voice.Name.Replace("IVONA 2", "").Trim() + ". " + GetDateString());
            }
        }

        #endregion

        private void Button_read_Click(object sender, RoutedEventArgs e)
        {
            Read(textBox_ReadContent.Text);
        }

        Dictionary<string, int> prioVoice = [];

        private void SetVoiceByLanguage(LanguageInfo langInfo)
        {
            label_language.Content = langInfo.Iso639_2T;
            if (prioVoice.TryGetValue(langInfo.Iso639_2T, out int value))
            {
                ComboBox_voice.SelectedIndex = value;
            }
            else
            {
                // Search for this language
                for (int i = 0; i < voices.Count; ++i)
                {
                    if (voices[i].VoiceInfo.Culture.ThreeLetterISOLanguageName == langInfo.Iso639_2T)
                    {
                        ComboBox_voice.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void SetLanguageByText(string text)
        {
            // Only if the language selecto file is available 
            if (!System.IO.File.Exists(textBox_languageSelectorFilePath.Text))
            {
                textBox_languageSelectorFilePath.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }

            textBox_languageSelectorFilePath.BorderBrush = System.Windows.Media.Brushes.Green;

            var factory = new RankedLanguageIdentifierFactory();
            //set the dictionary path
            var identifier = factory.Load(textBox_languageSelectorFilePath.Text);
            //get the language
            var languages = identifier.Identify(text);
            var mostCertainLanguage = languages.FirstOrDefault();
            if (mostCertainLanguage != null)
            {
                SetVoiceByLanguage(mostCertainLanguage.Item1);
            }
        }

        private void Read(string vtext)
        {
            if (CheckBox_languageSelector.IsChecked == true)
                SetLanguageByText(vtext);

            if (CheckBox_textFilter.IsChecked == true)
            {
                vtext = TextFilter(vtext);
                textBox_ReadContent.Text = vtext;
            }

            if (synthesizer.State == SynthesizerState.Speaking)
            {
                Stop();
            }

            synthesizer.SpeakAsync(vtext);
        }

        private void SetPlayOrPause(bool play)
        {
            if (play)
            {
                synthesizer.Resume();
                Button_pause.Content = "Pause";
            }
            else
            {
                synthesizer.Pause();
                Button_pause.Content = "Weiter";
            }
        }

        private void Pause_switch()
        {
            SetPlayOrPause((int)synthesizer.State == 2);
        }

        private static void Stop()
        {
            if (synthesizer.State == SynthesizerState.Speaking)
                synthesizer.SpeakAsyncCancelAll();
        }

        private static string TextFilter(string text)
        {
            StringBuilder sb = new();
            int start = 0, end = 0;

            while (end >= 0 && text.Length > end)
            {
                end = text.IndexOf('[', start);
                int s = end + 1;
                if (end >= 0 && text.Length > s)
                {
                    int e = text.IndexOf(']', s);
                    sb.Append(text.AsSpan(start, end - start));
                    if(e > 0 && text.Length > e)
                    {
                        start = e + 1;
                    } 
                    else
                    {
                        start = text.Length;
                    }
                }
                else
                {
                    sb.Append(text.AsSpan(start, text.Length - start));
                    end = text.Length;
                }
            }

            return sb.ToString().Replace("Δ", "delta").Replace("/", "durch");
        }

        private void Visibility_window()
        {
            if (this.Visibility == System.Windows.Visibility.Hidden)
            {
                this.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
            Save_setting();
        }

        //bool Is_close = false;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            synthesizer.Dispose();
            Save_setting();
            infoView?.Close();

            //Is_close = true;
        }

        private void Button_pause_Click(object sender, RoutedEventArgs e)
        {
            Pause_switch();
        }

        private void Slider_Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            infoView.slider_volume.Value = e.NewValue;
            synthesizer.Volume = (int)e.NewValue;
        }

        private void Button_stop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        int v = 0;
        bool vchange = false;
        private void Speed_change()
        {
            if (!vchange)
            {
                vchange = true;
                Dispatcher.Invoke(new Action(delegate ()
                {
                    synthesizer.Rate = v;
                }));

                Thread.Sleep(500);
                synthesizer.Rate = v;
                vchange = false;
            }
        }

        private void Slider_Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            v = (int)e.NewValue;

            infoView.label_speed.Content = "Speed level " + v;
            infoView.Label_speed_Changed();

            new Thread(Speed_change).Start();
        }

        private void ComboBox_voice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (synthesizer.Voice.Description.ToString().Replace("IVONA 2", "").Trim() != ComboBox_voice.SelectedItem.ToString())
            {
                Stop();
                foreach (InstalledVoice inVoice in voices)
                {
                    VoiceInfo vi = inVoice.VoiceInfo;

                    if (vi.Description.ToString().Replace("IVONA 2", "").Trim() == ComboBox_voice.SelectedItem.ToString())
                    {
                        try
                        {
                            synthesizer.SelectVoice(vi.Name);
                            Set_Window_Title();

                            if (ComboBox_voice.SelectedIndex + 1 < voices.Count)
                                infoView.label_nr1.Content = voices[ComboBox_voice.SelectedIndex + 1].VoiceInfo.Name.Replace("IVONA 2", "");
                            else
                                infoView.label_nr1.Content = "";

                            infoView.label_nr2.Content = vi.Name.Replace("IVONA 2", "");

                            if (ComboBox_voice.SelectedIndex - 1 >= 0)
                                infoView.label_nr3.Content = voices[ComboBox_voice.SelectedIndex - 1].VoiceInfo.Name.Replace("IVONA 2", "");
                            else
                                infoView.label_nr3.Content = "";


                            prioVoice[vi.Culture.ThreeLetterISOLanguageName] = ComboBox_voice.SelectedIndex;
                            infoView.Label_nr_Changed();

                        }
                        catch
                        {
                            MessageBox.Show("Fehler: Die ausgewählte Stimme ist nicht installiert oder wurde deaktiviert.");
                        }
                    }
                }
            }
        }

        readonly Properties.Settings setting = new();
        private void Save_setting()
        {
            // The properties are defined in the project settings
            setting.volumen = slider_Volume.Value;
            setting.speed = slider_Speed.Value;
            setting.stimme = ComboBox_voice.SelectedIndex;
            setting.autoLanguage = CheckBox_languageSelector.IsChecked == true;
            setting.textFilter = CheckBox_textFilter.IsChecked == true;

            // Store the path if it exists
            if (System.IO.File.Exists(textBox_languageSelectorFilePath.Text))
                setting.autoLanguageFilePath = textBox_languageSelectorFilePath.Text;

            setting.prioAutoLangVoice = System.Text.Json.JsonSerializer.Serialize(prioVoice);

            setting.Save();
            //Properties.Settings.Default.Reset();
        }

        private void Load_settings()
        {
            slider_Volume.Value = setting.volumen;
            slider_Speed.Value = setting.speed;
            ComboBox_voice.SelectedIndex = setting.stimme;
            CheckBox_languageSelector.IsChecked = setting.autoLanguage;
            CheckBox_textFilter.IsChecked = setting.textFilter;
            textBox_languageSelectorFilePath.Text = setting.autoLanguageFilePath;
            CheckBox_HiddenStart.IsChecked = setting.startHidden;

            // Convert from store to dictionary
            try
            {
                prioVoice = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(setting.prioAutoLangVoice) ?? [];
            }
            catch
            {
            }
        }




        private void CheckBox_Autostart_Checked(object sender, RoutedEventArgs e)
        {
            Autostart.ToggleAutostart(true, null, null);
            setting.autostartRegName = Autostart.GetAppName();
            setting.Save();
        }

        private void CheckBox_Autostart_Unchecked(object sender, RoutedEventArgs e)
        {
            Autostart.ToggleAutostart(false, null, null);
        }

        private void CheckBox_HiddenStart_Checked(object sender, RoutedEventArgs e)
        {
            setting.startHidden = true;
            setting.Save();
        }

        private void CheckBox_HiddenStart_Unchecked(object sender, RoutedEventArgs e)
        {
            setting.startHidden = false;
            setting.Save();
        }

        private void Button_hide_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("You can reveal the window with the keyboard shortcut:\nctrl + alt + m\nDo you want to continue?", "Hide window info", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                Visibility_window();
            }
        }
    }
}