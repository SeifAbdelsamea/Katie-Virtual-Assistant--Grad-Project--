using Android.App;
using Android.OS;
using Android.Content;
using Android.Widget;
using Android.Speech;
using AlertDialog = Android.App.AlertDialog;
using System;
using static Android.Bluetooth.BluetoothClass;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using Environment = System.Environment;
using System.Net.Http;
using System.Threading.Tasks;

namespace STT
{
    [Activity(Label = "Kattie", MainLauncher = true, Icon = "@drawable/icon")]
     public class MainActivity : Activity
     {
          private bool isRecording;
          private readonly int VOICE = 10;
          private TextView textBox;
          private Button recButton;
        
        

        protected override void OnCreate(Bundle bundle)
           {
               base.OnCreate(bundle);
     
               // set the isRecording flag to false (not recording)
               isRecording = false;

               // Set our view from the "main" layout resource
               SetContentView(Resource.Layout.activity_main);

               // get the resources from the layout
               recButton = FindViewById<Button>(Resource.Id.btnRecord);
               textBox = FindViewById<TextView>(Resource.Id.textYourText);

               // check to see if we can actually record - if we can, assign the event to the button
               string rec = Android.Content.PM.PackageManager.FeatureMicrophone;

               if (rec != "android.hardware.microphone")
               {
                
                   // no microphone, no recording. Disable the button and output an alert
                   var alert = new AlertDialog.Builder(recButton.Context);
                   alert.SetTitle("You don't seem to have a microphone to record with");
                   alert.SetPositiveButton("OK", (sender, e) =>
                   {
                       textBox.Text = "No microphone present";
                       recButton.Enabled = false;
                       return;
                   });

                   alert.Show();
               }
               else
                   recButton.Click += delegate
                   {
                       // change the text on the button
                       recButton.Text = "End Recording";
                       isRecording = !isRecording;
                       if (isRecording)
                       {
                          
                           // create the intent and start the activity
                           var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                           voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                           //voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
                           voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, "en-US");
                           // put a message on the modal dialog
                           voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                           // if there is more then 1.5s of silence, consider the speech over
                           voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                           voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                           voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                           voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 3);
                          
                           


                           // you can specify other languages recognised here, for example
                           // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                        //   voiceIntent.PutExtra("android.speech.extra.EXTRA_ADDITIONAL_LANGUAGES", new String[] { "ar-SA" }); //arabic recognizer
                         //  voiceIntent.PutExtra(RecognizerIntent.ExtraLanguagePreference, "ar-SA");
                        //   voiceIntent.PutExtra(RecognizerIntent.EXTRA_ONLY_RETURN_LANGUAGE_PREFERENCE, "ar-SA");
                           // if you wish it to recognise the default Locale language and German
                           // if you do use another locale, regional dialects may not be recognised very well

                           voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                           StartActivityForResult(voiceIntent, VOICE);
                       }
                   };

           }


          protected override void OnActivityResult(int requestCode, Result resultVal, Intent data)
          {
              if (requestCode == VOICE)
              {
                 // textBox.Text = ""; //cleannign the outputed text on the screen

                  if (resultVal == Result.Ok)
                  {
                      textBox.Text = "";

                      var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                      if (matches.Count != 0)
                      {

                          string textInput = textBox.Text + matches[0];
                          string questions = matches[0];


                          // limit the output to 500 characters
                          if (textInput.Length > 500)
                              textInput = textInput.Substring(0, 500);
                          textBox.Text = textInput;


                          if(questions=="how are you")
                          {
                              Xamarin.Essentials.TextToSpeech.SpeakAsync("Can't Complain");

                              questions = "";
                              textBox.Text = "";
                              recButton.Text = "Start Recording";
                          }
                          //// WEATHER
                          else if(questions=="what is the weather" || questions == "tell me the weather" || questions == "what's the weather" || questions == "what is the weather now")
                        {
                            try
                            {
                                string temp = getWeather();

                                Xamarin.Essentials.TextToSpeech.SpeakAsync("Currently it's" + temp + "celsius");
                            }
                            catch
                            {
                                textBox.Text = "Internet Connection needed for this action";
                                Xamarin.Essentials.TextToSpeech.SpeakAsync(" Internet Connection needed");
                            }
                          }

                        //// WIKIPEDIA
                        else if (questions=="what do you know about football")
                          {
                            string x = "football";
                            try
                            {
                                string temp = Wikipedia(x);
                                Xamarin.Essentials.TextToSpeech.SpeakAsync(temp);
                            }
                            catch
                            {
                                textBox.Text = "This action requires internet connection";
                                Xamarin.Essentials.TextToSpeech.SpeakAsync(" Internet Connection needed");
                            }
                          }
                          else if(questions=="hello" || questions=="hi")
                          {
                              Xamarin.Essentials.TextToSpeech.SpeakAsync("Hello there");
                          }
                          else if(questions=="what is your name"||questions=="what's your name")
                          {
                              Xamarin.Essentials.TextToSpeech.SpeakAsync("my name is katty");
                              textBox.Text = "";
                              questions = "";
                              recButton.Text = "Start Recording";  
                          }
                          else if (questions=="how old are you")
                          {
                              Xamarin.Essentials.TextToSpeech.SpeakAsync("I don't know, Ask Seif ");
                              textBox.Text = "";
                              questions = "";
                              recButton.Text = "Start Recording";

                          }
                          //DATE
                          else if(questions=="what is the date"|| questions== "what's the date")
                          {
                              string date  = DateTime.Now.Date.ToShortDateString();
                              Xamarin.Essentials.TextToSpeech.SpeakAsync(date);
                              textBox.Text = date;
                          }
                          //TIME
                          else if (questions == "what is the time" || questions == "what's the time" || questions == "what time is it now")
                          {
                              Xamarin.Essentials.TextToSpeech.SpeakAsync("The Time is");
                              ///////time form 9:00 
                              ///
                              string time= DateTime.Now.ToShortTimeString();  
                              Xamarin.Essentials.TextToSpeech.SpeakAsync(time);
                              Xamarin.Essentials.TextToSpeech.SpeakAsync("minutes");
                              //////time  am or pm  before midday or after midday
                              ///
                              string am = DateTime.Now.ToString("tt");
                              if (am == "am")
                              {
                                  Xamarin.Essentials.TextToSpeech.SpeakAsync("a");
                                  Xamarin.Essentials.TextToSpeech.SpeakAsync("m");
                              }
                              else if (am=="pm")
                              {
                                  Xamarin.Essentials.TextToSpeech.SpeakAsync("p");
                                  Xamarin.Essentials.TextToSpeech.SpeakAsync("m");
                              }

                              textBox.Text = time+am;     
                          }
                          else if (questions=="what is the date and time")
                          {
                              string str = DateTime.Now.ToString("dd/mm/yyyy hh:mm tt");
                              Xamarin.Essentials.TextToSpeech.SpeakAsync(str);
                              textBox.Text = str;
                          }
                          else if (questions == "event")
                          {
                            GoogleCalendarAPI();
                          }
                          else if (questions=="movie")
                        {
                            movieFinderApiAsync();
                        }
                       
                      }
                      else
                          textBox.Text = "No speech was recognised";
                      // change the text back on the button
                      recButton.Text = "Start Recording";
                      //textBox.Text = "";

                  }

              }

              base.OnActivityResult(requestCode, resultVal, data);
          }
        public string getWeather()
        {
            using (WebClient web = new WebClient())
            {
                string temp = "";
                double num;
                string url = string.Format("http://api.openweathermap.org/data/2.5/weather?q=cairo&appid=45108c2076f9aa8757af5d28701332d0");
                var json = web.DownloadString(url);
                WeatherInfo.root Info = JsonConvert.DeserializeObject<WeatherInfo.root>(json); //read json file from api 
                temp = Info.main.temp.ToString();
                num = Double.Parse(temp);
                num = num - 273.15; //covert K to C
                temp=num.ToString(); 
                return temp;
            }
        }
        public string Wikipedia(string x)
        {
            using (WebClient wiki = new WebClient())
            {
                var a = wiki.DownloadString("https://en.wikipedia.org/w/api.php?action=opensearch&search=" + x);
                var b = Newtonsoft.Json.JsonConvert.DeserializeObject(a);
                string[] c = b.ToString().Split("[");
                var i = c[3];
                i.ToString();
                return i;
            }
        }

        //GoogleCalendarAPI()
        public void GoogleCalendarAPI()
        {
            string[] Scopes = { CalendarService.Scope.CalendarReadonly };
            string ApplicationName = "Google Calendar API .NET Quickstart";

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                // Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 5;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            //  Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                textBox.Text = "";
                foreach (var eventItem in events.Items)
                {
                    /*  string when = eventItem.Start.DateTime.ToString();
                      if (String.IsNullOrEmpty(when))
                      {
                          when = eventItem.Start.Date;
                      }
                      Console.WriteLine("{0} ({1})", eventItem.Summary, when);*/
                    textBox.Text += eventItem.Summary + Environment.NewLine;
                }
            }
            else
            {
                //Console.WriteLine("No upcoming events found.");
                textBox.Text = "No Upcoming Events";
            }
            //  Console.Read();

        }

        //Movie Finder API
        public async Task movieFinderApiAsync()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://online-movie-database.p.rapidapi.com/auto-complete?q=game"),
                Headers =
                {
                    { "X-RapidAPI-Host", "online-movie-database.p.rapidapi.com" },
                    { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
                },
            };

            using (var response = await client.SendAsync(request))
                
            {
                var result = await response.Content.ReadAsStringAsync();
                //dynamic json = JsonConvert.DeserializeObject(result);
                Movie.root Info = JsonConvert.DeserializeObject<Movie.root>(result);
                var temp = Info.d.l.ToString();
                var x = Info.d.Items;
                textBox.Text = temp;
              //  textBox.Text = x[0];
                // WeatherInfo.root Info = JsonConvert.DeserializeObject<WeatherInfo.root>(json); //read json file from api 
                //temp = Info.main.temp.ToString();

                // response.EnsureSuccessStatusCode();
                // var json = await response.Content.ReadAsStringAsync();

                // var b = JsonConvert.DeserializeObject(json);
                // WeatherInfo.root Info = JsonConvert.DeserializeObject<WeatherInfo.root>(json);
                //  Movie.root Info= JsonConvert.DeserializeObject<Movie.root>(json);
                // var i = Info.d.l;
                //  string name = Info.d.Items[0].l.ToString();
                // textBox.Text = name;

            }
        }

    }
}