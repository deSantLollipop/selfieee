using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Content;
using System;

namespace selfeee_cam
{
    public class NotificationLogic
    {
        public System.Timers.Timer timer = new System.Timers.Timer();

        static readonly int NOTIFICATION_ID = 1000;
        static readonly string CHANNEL_ID = "location_notification";

        Context mContext;
        public NotificationLogic(Context context)
        {
            mContext = context;
            CreateNotificationChannel();
            OnTimedEvent();
        }

        private void OnTimedEvent()              //Функция нотьки запускается в установленое время  / 
        {                                                                                       //Notification Func - launching on time
            // Set up an intent so that tapping the notifications returns to this app:
            Intent intent = new Intent(mContext, typeof(MainActivity));

            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(mContext, pendingIntentId, intent, PendingIntentFlags.OneShot);


            // Pass the current button press count value to the next activity:
            var valuesForActivity = new Bundle();

            // Build the notification:
            var builder = new NotificationCompat.Builder(mContext, CHANNEL_ID)
                            .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                            .SetContentIntent(pendingIntent)  // запустить активность из нотьки
                            .SetContentTitle("selfeee Noti") // Set the title
                            .SetSmallIcon(Resource.Drawable.cam_ico) // This is the icon to display
                            .SetContentText($"Hello, гусь"); // the message to display.

            // Finally, publish the notification:
            var notificationManager = NotificationManagerCompat.From(mContext);
            notificationManager.Notify(NOTIFICATION_ID, builder.Build());
        }

        public void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var name = "abc";//Resources.GetString(Resource.String.channel_name);
            var description = "123";//GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(CHANNEL_ID, name, NotificationImportance.Default)
            {
                Description = description
            };


            var notificationManager = (NotificationManager)mContext.GetSystemService("notification");
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}