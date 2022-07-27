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
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Linq;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

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
                           var voiceIntent = new Android.Content.Intent(RecognizerIntent.ActionRecognizeSpeech);
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




                        

                           voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
                        StartActivityForResult(voiceIntent, VOICE);
                    }
                };

        }


        protected override void OnActivityResult(int requestCode, Result resultVal, Android.Content.Intent data)
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


                        if (questions == "how are you")
                        {
                            Xamarin.Essentials.TextToSpeech.SpeakAsync("Can't Complain");

                            questions = "";
                            textBox.Text = "";
                            recButton.Text = "Start Recording";
                        }
                        //// WEATHER
                        else if (questions == "what is the weather" || questions == "tell me the weather" || questions == "what's the weather" || questions == "what is the weather now")
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
                        else if (questions == "what do you know about football")
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
                        else if (questions == "hello" || questions == "hi")
                        {
                            Xamarin.Essentials.TextToSpeech.SpeakAsync("Hello there");
                        }
                        else if (questions == "what is your name" || questions == "what's your name")
                        {
                            Xamarin.Essentials.TextToSpeech.SpeakAsync("my name is katty");
                            textBox.Text = "";
                            questions = "";
                            recButton.Text = "Start Recording";
                        }
                        else if (questions == "how old are you")
                        {
                            Xamarin.Essentials.TextToSpeech.SpeakAsync("I don't know, Ask Seif ");
                            textBox.Text = "";
                            questions = "";
                            recButton.Text = "Start Recording";

                        }
                        //DATE
                        else if (questions == "what is the date" || questions == "what's the date")
                        {
                            string date = DateTime.Now.Date.ToShortDateString();
                            Xamarin.Essentials.TextToSpeech.SpeakAsync(date);
                            textBox.Text = date;
                        }
                        //TIME
                        else if (questions == "what is the time" || questions == "what's the time" || questions == "what time is it now")
                        {
                            Xamarin.Essentials.TextToSpeech.SpeakAsync("The Time is");
                            ///////time form 9:00 
                            ///
                            string time = DateTime.Now.ToShortTimeString();
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
                            else if (am == "pm")
                            {
                                Xamarin.Essentials.TextToSpeech.SpeakAsync("p");
                                Xamarin.Essentials.TextToSpeech.SpeakAsync("m");
                            }

                            textBox.Text = time + am;
                        }
                        else if (questions == "what is the date and time")
                        {
                            string str = DateTime.Now.ToString("dd/mm/yyyy hh:mm tt");
                            Xamarin.Essentials.TextToSpeech.SpeakAsync(str);
                            textBox.Text = str;
                        }
                        else if (questions == "event")
                        {
                            GoogleCalendarAPI();
                        }
                        else if (questions == "movie" || questions == "recommend a movie ")
                        {
                            // Xamarin.Essentials.TextToSpeech.SpeakAsync("What kind of movies you want to watch");

                            movieFinderApiAsync("death note");
                        }
                        else if (questions == "apple")
                        {
                            caloriesAsync();
                        }
                        else if (questions == "who is Elon Musk")
                        {
                            GoogleSearch("Elon Musk");
                        }
                        else if (questions=="love")
                        {
                            LoveCalc();
                        }
                        else if (questions=="coronavirus")
                        {
                            Coronavirus();
                        }
                        else if (questions == "coronavirus Egypt")
                        {
                            CoronavirusEgypt();
                        }
                        else if (questions=="give me a joke" || questions=="say a joke" || questions=="joke")
                        {
                            RandomJoke();
                        }
                        else
                        {
                            NLP(questions);
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
                temp = num.ToString();
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
        public async Task movieFinderApiAsync(string mov)
        {

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://online-movie-database.p.rapidapi.com/auto-complete?q=" + mov),
                Headers =
                {
                    { "X-RapidAPI-Host", "online-movie-database.p.rapidapi.com" },
                    { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
                },
            };

            using (var response = await client.SendAsync(request))

            {
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(result);
                Xamarin.Essentials.TextToSpeech.SpeakAsync("you can watch" + json["d"][0]["l"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("its a " + json["d"][0]["q"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("rank" + json["d"][0]["rank"].Value<string>());
                textBox.Text = json["d"][1]["l"].Value<string>();

            }
        }
        //Calories API
        public async Task caloriesAsync()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://food-calorie-data-search.p.rapidapi.com/api/search?keyword=apple"),
                Headers =
    {
        { "X-RapidAPI-Host", "food-calorie-data-search.p.rapidapi.com" },
        { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

              //  Calories.root Info = JsonConvert.DeserializeObject<Calories.root>(result);
                //string xd = Info.calories.energ_kcal.ToString();
                //textBox.Text = xd;
                
                JObject json = JObject.Parse(body);    //Whhy mate !!!!
                Xamarin.Essentials.TextToSpeech.SpeakAsync("according to Google search" + json[0]["shrt_desc"].Value<string>());
              
                textBox.Text = json[0]["shrt_desc"].Value<string>();

            }
        }
        public async Task GoogleSearch(string search)
        {
            string x = search + "&num=100&lr=lang_en&hl=en&cr=US";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://google-search3.p.rapidapi.com/api/v1/search/q="+x),
                Headers =
    {
        { "X-User-Agent", "desktop" },
        { "X-Proxy-Location", "US" },
        { "X-RapidAPI-Host", "google-search3.p.rapidapi.com" },
        { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
            


               JObject json = JObject.Parse(body);

                Xamarin.Essentials.TextToSpeech.SpeakAsync("according to Google search" + json["results"][0]["title"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync(json["results"][0]["description"].Value<string>());

            }
        }
        public async Task LoveCalc()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://love-calculator.p.rapidapi.com/getPercentage?fname=Seif&sname=Gehad "),
                Headers =
    {
        { "X-RapidAPI-Host", "love-calculator.p.rapidapi.com" },
        { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(body);

                Xamarin.Essentials.TextToSpeech.SpeakAsync( "Your Percentage is"+ json["percentage"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("Go for it and "+ json["result"].Value<string>());
               
            }
        }
        public async Task Coronavirus()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://covid-19-statistics.p.rapidapi.com/reports/total"),
                Headers =
    {
        { "X-RapidAPI-Host", "covid-19-statistics.p.rapidapi.com" },
        { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(body);

                Xamarin.Essentials.TextToSpeech.SpeakAsync("according to Covid 19 Statistics today" + json["data"]["date"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("the total number of cases. "+json["data"]["confirmed"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("Number of Confirmed deaths.  " + json["data"]["deaths"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("Number of Active Cases. " + json["data"]["active"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("Please stay safe and take care");

            }
        }
        public async  Task CoronavirusEgypt()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://covid-193.p.rapidapi.com/history?country=egypt"),
                Headers =
    {
        { "X-RapidAPI-Host", "covid-193.p.rapidapi.com" },
        { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(body);

                Xamarin.Essentials.TextToSpeech.SpeakAsync("according to Covid 19 Statistics today in " + json["response"][0]["country"].Value<string>()+ json["response"][0]["day"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("the total number of active cases . " + json["response"][0]["cases"]["active"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("the total number of critical cases . " + json["response"][0]["cases"]["critical"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("Number of Confirmed deaths.  " + json["response"][0]["deaths"]["total"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync("Please stay safe and take care");
            }
        }
        public async Task RandomJoke()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://dad-jokes.p.rapidapi.com/random/joke/png"),
                Headers =
     {
        { "X-RapidAPI-Host", "dad-jokes.p.rapidapi.com" },
        { "X-RapidAPI-Key", "51b091de6bmsh98c28b1b26d43adp1725cejsn38d1b94663a8" },
     },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(body);
                Xamarin.Essentials.TextToSpeech.SpeakAsync(json["body"]["setup"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync(json["body"]["punchline"].Value<string>());
                
            }
        }
        public static async Task LU()
        {
            //Create variables to hold your authoring key and resource names.
            var key = "5acc96d63f8749b294f9111dd177fa5e"; //"PASTE_YOUR_LUIS_AUTHORING_SUBSCRIPTION_KEY_HERE";

            var authoringEndpoint = "https://newkatva-authoring.cognitiveservices.azure.com/";
            var predictionEndpoint = "https://newkatva.cognitiveservices.azure.com/";

            //Create variables to hold your endpoints, app name, version, and intent name.
            var appName = "NewVA";
            var versionId = "0.1";
            var intentName = "OrderPizzaIntent";

            //Create an ApiKeyServiceClientCredentials object with your key, and use it with your endpoint to create an LUISAuthoringClient object.
            var credentials = new Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.ApiKeyServiceClientCredentials(key);//authoring p key
            var client = new LUISAuthoringClient(credentials) { Endpoint = authoringEndpoint };


            //Create a ApplicationCreateObject. The name and language culture are required properties. Call the Apps.AddAsync method. The response is the app ID.
            var newApp = new ApplicationCreateObject
            {
                Culture = "en-us",
                Name = appName,
                InitialVersionId = versionId
            };

            var appId = await client.Apps.AddAsync(newApp);


            //Create a ModelCreateObject with the name of the unique intent then pass the app ID, version ID, and the ModelCreateObject to the Model.AddIntentAsync method. The response is the intent ID.
            await client.Model.AddIntentAsync(appId, versionId, new ModelCreateObject()
            {
                Name = intentName
            });
        }
        public async Task NLP(string text)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://newkatva.cognitiveservices.azure.com/luis/prediction/v3.0/apps/9a40e63b-7c4b-46ad-ae00-0d540c39816e/slots/production/predict?verbose=true&show-all-intents=true&log=true&subscription-key="key"query=" + text),
                Headers =
                {
                    { "X-RapidAPI-Host", "https://newkatva-authoring.cognitiveservices.azure.com/" },
                    { "X-RapidAPI-Key", "key" },
                },
            };

            using (var response = await client.SendAsync(request))

            {
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(result);

                textBox.Text = "query : "+ json["query"].Value<string>() +". "+"Top Intent:  "+ json["prediction"]["topIntent"].Value<string>()+". "+"Person name:  "+ json["prediction"]["entities"]["personName"][0].Value<string>();
               // textBox.Text = json["prediction"]["topIntent"].Value<string>();
                //textBox.Text = json["prediction"]["entities"]["personName"][0].Value<string>();

                Xamarin.Essentials.TextToSpeech.SpeakAsync(json["prediction"]["topIntent"].Value<string>());
                Xamarin.Essentials.TextToSpeech.SpeakAsync(json["prediction"]["entities"]["personName"][0].Value<string>());
            }
    }
     }

}


