using Android.App;
using Android.Content;
using Android.Icu.Util;
using Android.OS;
using System;
using Xamarin.Essentials;

namespace selfeee_cam
{
    [Service]
    class BackgroundService : Service
    {       
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // From shared code or in your PCL
            //Toast.MakeText(this, "halo", ToastLength.Long).Show();
            
            // не удаляю, но пока не нужен

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        
    }
}