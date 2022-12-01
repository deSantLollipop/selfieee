using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace selfeee_cam
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    class AlarmBroadcast : BroadcastReceiver
    {
        public static long reminderInterval = 24 * 60 * 60 * 1000;
        private static DateTime tmpTime;

        public static long FirstReminder(DateTime time)
        {
            tmpTime = time;
            Java.Util.Calendar calendar = Java.Util.Calendar.Instance;
            calendar.Set(Java.Util.CalendarField.HourOfDay, time.Hour);
            calendar.Set(Java.Util.CalendarField.Minute, time.Minute);
            calendar.Set(Java.Util.CalendarField.Second, 20);
            return calendar.TimeInMillis;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            Console.WriteLine("BootReceiver: OnReceive");
            NotificationLogic notification = new NotificationLogic(MainActivityContext.mContext);

            var alarmIntent = new Intent(context, typeof(AlarmBroadcast));
            var pending = PendingIntent.GetBroadcast(context, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);

            alarmManager.SetRepeating(AlarmType.RtcWakeup, FirstReminder(tmpTime), reminderInterval, pending);
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, alarmIntent, 0);
        }
    }
}