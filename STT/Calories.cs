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
    public class Calories
    {
        public int id { get; set; }

        public float energ_kcal { get; set; }

        public class root
        {
            public Calories calories { get; set; }
        }
    }
}