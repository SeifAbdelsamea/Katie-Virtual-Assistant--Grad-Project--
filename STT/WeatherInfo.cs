using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STT
{
     class WeatherInfo
    {
        public class main
        {
           public double temp { get; set; }
        }
        public class root
        {
            public main main { get; set; }
        }
    }
}