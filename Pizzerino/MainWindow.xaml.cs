using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;


namespace Pizzerino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        
        private readonly BackgroundWorker listenWorker = new BackgroundWorker();
        private readonly BackgroundWorker speakWorker = new BackgroundWorker();
        static SpeechSynthesizer ss;
        static SpeechRecognitionEngine sre;
        int state = 0;
        bool confirmed=true;
        bool given = false;
        List<Int32> pizzaNumbers = new List<Int32>();
        int pizzaNumber;
        public MainWindow()
        {

            InitializeComponent();
            listenWorker.DoWork += Listen_Worker_DoWork;
            listenWorker.RunWorkerAsync();
            //speakWorker.DoWork += Speak_Worker_DoWork;
            //speakWorker.WorkerSupportsCancellation = true;
            //speakWorker.RunWorkerAsync();
        }


        /*
        private void Speak_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ss = new SpeechSynthesizer();
            ss.SetOutputToDefaultAudioDevice();
            Powitanie(ss);
        }
        public void Powitanie(SpeechSynthesizer ss)
        {
            ss.SpeakAsync("Witaj w piccerii Piccerino. Możesz tutaj głosowo zamówić piccę. Aby rozpocząć proces zamawiania powiedz, Zamawiam. Aby zrezygnować powiedz, Stop");
            ss.SpeakAsync("Witaj");
        }
        */
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.ParentWindowRef = this;
            this.ParentFrame.Navigate(new Page1());
        }

        private void Listen_Worker_DoWork(object sender, DoWorkEventArgs e)
        {            
            ss = new SpeechSynthesizer();
            ss.SetOutputToDefaultAudioDevice();
            CultureInfo ci = new CultureInfo("pl-PL");
            sre = new SpeechRecognitionEngine();
            sre.SetInputToDefaultAudioDevice();
            sre.SpeechRecognized += Sre_SpeechRecognized;
            Grammar grammar = new Grammar("C:\\Users\\demby\\source\\repos\\Pizzerino\\Pizzerino\\Gramatyka.xml", "rootRule");
            grammar.Enabled = true;
            sre.LoadGrammar(grammar);
            
            sre.RecognizeAsync(RecognizeMode.Multiple);
            ss.SpeakAsync("Witaj w picceri Piccerino. Możesz tutaj głosowo zamówić piccę. Aby rozpocząć proces zamawiania powiedz, Zamawiam. Aby zrezygnować powiedz, Stop. Aby ponownie usłyszeć instrukcję powiedz, pomoc");
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            
            String txt = e.Result.Text;
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                // polecenieLabel.Content = e.Result.Text;
            }));


            float confidence = e.Result.Confidence;
            Console.WriteLine("Rozpoznano: " + txt + " - z pewnością: " + confidence);

            if (confidence > 0.7)
            {
                ss.SpeakAsyncCancelAll();

                command(e);
                
                if (state == 1)
                {
                    Console.WriteLine("STAN 3");
                    pickPizza(e);
                    confirmPizza(e, pizzaNumber);
                }
                else if (state == 2)
                {
                    Console.WriteLine("STAN 1");
                    pickStreet(e);
                    confirm(e);
                }
                else if (state == 3)
                {
                    Console.WriteLine("STAN 2");
                    phoneNumber(e);
                    confirm(e);
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // firstLabel.Content = first;
                    // operatorLabel.Content = znak;
                    // secondLabel.Content = second;
                    // sumLabel.Content = calosci.ToString() + " reszty " + reszta.ToString();

                }));
            }
            else
            {
                ss.SpeakAsync("Prosze powtórzyć");
                return;
            }
        }

        private void pickPizza(SpeechRecognizedEventArgs e)
        {
            try
            {
                pizzaNumber = (Convert.ToInt32(e.Result.Semantics["pizzaNumber"].Value));   
                Console.WriteLine("Dodana pizza ma numer: " + pizzaNumber);
                given = true;
                ss.SpeakAsync("Czy dodana pizza się zgadza ?");
            }
            catch
            {

            }

        }

        private void phoneNumber(SpeechRecognizedEventArgs e)
        {
            try
            {
            double[] phoneNumber = new double[9];
            phoneNumber[0] = Convert.ToDouble(e.Result.Semantics["number1"].Value);
            phoneNumber[1] = Convert.ToDouble(e.Result.Semantics["number2"].Value);
            phoneNumber[2] = Convert.ToDouble(e.Result.Semantics["number3"].Value);
            phoneNumber[3] = Convert.ToDouble(e.Result.Semantics["number4"].Value);
            phoneNumber[4] = Convert.ToDouble(e.Result.Semantics["number5"].Value);
            phoneNumber[5] = Convert.ToDouble(e.Result.Semantics["number6"].Value);
            phoneNumber[6] = Convert.ToDouble(e.Result.Semantics["number7"].Value);
            phoneNumber[7] = Convert.ToDouble(e.Result.Semantics["number8"].Value);
            phoneNumber[8] = Convert.ToDouble(e.Result.Semantics["number9"].Value);
            double number = phoneNumber[0] * 100000000 + phoneNumber[1] * 10000000 + phoneNumber[2] * 1000000 + phoneNumber[3] * 100000 + phoneNumber[4] * 10000 + phoneNumber[5] * 1000 + phoneNumber[6] * 100 + phoneNumber[7] * 10 + phoneNumber[8];
            Console.WriteLine("Podany numer telefonu to: " + number);
            ss.SpeakAsync("Czy numer telefonu się zgadza ?");
            given = true;
            }
            catch
            {

            }

        }

        private void confirm(SpeechRecognizedEventArgs e)
        {
            try
            {
                String confirm = e.Result.Semantics["confirm"].Value.ToString();
                if (confirm.Equals("tak")&&given==true)
                {
                    given = false;
                    confirmed = true;
                    ss.SpeakAsync("Aby przejść dalej powiedz, dalej");
                }                
                else if (confirm.Equals("tak") && given == false)
                {
                    ss.SpeakAsync("Podaj dane przed potwierdzeniem");
                }
                else if(confirm.Equals("nie"))
                {
                    ss.SpeakAsync("Podaj dane ponownie");
                }

            }
            catch
            {

            }
        }


        private void confirmPizza(SpeechRecognizedEventArgs e, int pizzaNumber )
        {
            try
            {
                String confirm = e.Result.Semantics["confirm"].Value.ToString();
                if (confirm.Equals("tak") && given == true)
                {

                    pizzaNumbers.Add(pizzaNumber);

                    confirmed = true;
                    given = false;
                    ss.SpeakAsync("Aby dodać kolejną pizze powiedz, dodaj, i numer wybranej pizzy. Aby zakończyć wybór pizz powiedz, dalej");
                }
                else if (confirm.Equals("tak") && given == false)
                {
                    ss.SpeakAsync("Podaj dane przed potwierdzeniem");
                }
                else if (confirm.Equals("nie"))
                {
                    ss.SpeakAsync("Podaj dane ponownie");
                }

            }
            catch
            {

            }
        }

        private  void pickStreet(SpeechRecognizedEventArgs e)
        {
            try
            {
                String street = e.Result.Semantics["street"].Value.ToString();
                double secondNumber=-1;
                double houseNumber = Convert.ToDouble(e.Result.Semantics["number1"].Value);
                try
                {
                secondNumber = Convert.ToDouble(e.Result.Semantics["number2"].Value);
                }
                catch
                {

                }

                
                Console.WriteLine("Podana ulica to: " + street + " numer domu: " + houseNumber + " numer mieszkania "+ secondNumber);
                ss.SpeakAsync("Jeśli podany adres się zgadza powiez, tak, jeśli nie, powiedz, nie");
                given = true;
            }
            catch
            {

            }
        }

        private void command(SpeechRecognizedEventArgs e)
        {
            try
            {
                String polecenie = e.Result.Semantics["command"].Value.ToString();
                Console.WriteLine("Podane polecenie to: " + polecenie);
                if (polecenie.Equals("zamawiam") || polecenie.Equals("dalej") && confirmed)
                {
                    switch (state)
                    {
                        case 0:
                            ss.SpeakAsync("Proszę wybrać pizzę z listy dostępnych. Aby dodać wybraną pizzę do zamówienia powiedz, dodaj, a następnie numer wybranej pizzy.");
                             state++;
                            confirmed = false;
                            break;

                        case 1:
                            ss.SpeakAsync("Najpierw proszę podać ulicę z listy ulic do których oferujemy dostawę następnie, numer domu, przez, numer mieszkania. W przypadku mieszkania w domu jednorodzinny należy za numer mieszkania podać zero");
                            state++;
                            given = false;
                            break;
                        case 2:                          
                            ss.SpeakAsync("Podaj numer telefonu");
                            state++;
                            given = false;
                            break;
                        case 3:
                            ss.SpeakAsync("Czy wszystkie dane się zgadzają ? Jeśli tak powiedz, tak, jeśli nie, aby ponownie rozpocząć wprowadzanie danych powiedz, od nowa");
                            state++;
                            given = false;
                            break;
                        default:

                            break;
                    }
                    
                                               
                    
                                    }
                else if (polecenie.Equals("koniec"))
                {
                    CloseAplication();
                }
                else if (polecenie.Equals("pomoc"))
                {
                    switch (state)
                    {
                        case 0:
                            ss.SpeakAsync("Aby rozpocząć proces zamawiania powiedz, Zamawiam. Aby zrezygnować powiedz, Stop. Aby ponownie usłyszeć instrukcję powiedz, pomoc");
                            break;
                        case 1:
                            ss.SpeakAsync("Proszę powiedzieć, dodaj, a następnie numer wybranej pizzy");
                            break;
                        case 2:
                            ss.SpeakAsync("Najpierw proszę podać ulicę z listy ulic do których oferujemy dostawę następnie, numer domu, przez, numer mieszkania. W przypadku mieszkania w domu jednorodzinny należy za numer mieszkania podać zero");
                            break;
                            
                        case 3:
                            ss.SpeakAsync("Proszę podać dziewięcio cyfrowy numer telefonu podając kolejno pojedynczo liczby");
                            break;

                        default:
                            Console.WriteLine("");
                            break;
                    }
                }
                else
                {
                    ss.SpeakAsync("Proszę podać potrzebne dane oraz je zatwierdzić.");
                }
            }
            catch
            {

            }
        }

        private void CloseAplication()
        {
            //ss.Speak("Zapraszamy ponownie");
            Environment.Exit(0);
        }





        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.ParentWindowRef = this;
            this.ParentFrame.Navigate(new Page1());
        }
    }
}
