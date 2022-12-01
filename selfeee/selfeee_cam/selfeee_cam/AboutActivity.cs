using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;

namespace selfeee_cam
{
    [Activity(Label = "AboutActivity")]
    public class AboutActivity : Activity
    {
        Button aboutBackButton;
        TextView aboutText;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.about_layout);

            aboutBackButton = (Button)FindViewById(Resource.Id.aboutBackButton);
            aboutText = (TextView)FindViewById(Resource.Id.aboutText);

            aboutBackButton.Click += AboutBackButton_Click;

            string htmString = string.Empty;
            AssetManager assests = this.Assets;

            using (StreamReader sr = new StreamReader(assests.Open("about.htm")))
            {
                htmString = sr.ReadToEnd();
            }

            aboutText.SetText(Html.FromHtml(htmString), TextView.BufferType.Spannable);

            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            aboutBackButton.SetTypeface(typeface, TypefaceStyle.Normal);
            aboutText.SetTypeface(typeface, TypefaceStyle.Normal);
        }

        private void AboutBackButton_Click(object sender, EventArgs e)
        {
            base.OnBackPressed();
        }
    }
}