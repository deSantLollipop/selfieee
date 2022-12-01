using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Xamarin.Essentials;
using Java.Interop;
using Android.Views;
using Android.Graphics;
using Plugin.Media;
using System.Threading.Tasks;
using Java.IO;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace selfeee_cam
{
    [Activity(Label = "SettingsActivity")]
    public class SettingsActivity : Activity
    {
        Button backButton;
        //TextView timeDisplay;
        //Button timeSelectButton;
        Button setImageButton;
        Button confirmButton;

        byte[] imageByte;

        static readonly HttpClient client = new HttpClient();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layout_settings);

            backButton = (Button)FindViewById(Resource.Id.backButton);
            confirmButton = (Button)FindViewById(Resource.Id.confirmButton);

            //timeDisplay = (TextView)FindViewById(Resource.Id.time_display);
            //timeSelectButton = (Button)FindViewById(Resource.Id.select_button);

            setImageButton = (Button)FindViewById(Resource.Id.btnProfileImage);

            //timeSelectButton.Click += TimeSelectOnClick;

            Platform.Init(this, savedInstanceState);
            DateTime tmpTime = Preferences.Get("timeToNotify", DateTime.Now);
            //timeDisplay.Text = tmpTime.ToShortTimeString();

            backButton.Click += backButton_Click;
            setImageButton.Click += SetImageButton_Click;
            confirmButton.Click += Confirm_Click;


            ViewTreeObserver vto = setImageButton.ViewTreeObserver;
            vto.GlobalLayout += (sender, args) =>
            {
                if (MainActivityContext.Anonymous == true)
                    setImageButton.Enabled = false;
            };

            ViewTreeObserver vto1 = confirmButton.ViewTreeObserver;
            vto1.GlobalLayout += (sender, args) =>
            {
                if (MainActivityContext.Anonymous == true)
                    confirmButton.Enabled = false;
            };

            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            backButton.SetTypeface(typeface, TypefaceStyle.Normal);
            setImageButton.SetTypeface(typeface, TypefaceStyle.Normal);
            confirmButton.SetTypeface(typeface, TypefaceStyle.Normal);
        }

        private async void Confirm_Click(object sender, EventArgs e)
        {
            if (imageByte != null)
            {
                PrivatePhotosBitmap.UserProfileImage = BitmapFactory.DecodeByteArray(imageByte, 0, imageByte.Length);
                MainActivityContext.userData.imageprofile = imageByte;
                string result = JsonConvert.SerializeObject(MainActivityContext.userData.imageprofile);
                await SetImageAsync(result);
                
                //string dpPath = System.IO.Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "user.db3");
                //var db = new SQLiteConnection(dpPath);
                //var data = db.Table<LoginTable>();
                //var data1 = (from values in data
                //             where values.username == MainActivityContext.userName
                //             select values).Single();
                //data1.profileimage = (byte[])imageByte.Clone();
                //db.Update(data1);
            }
        }

        private async Task SetImageAsync(string data)
        {
            string putPath = string.Format(@"http://192.168.0.103:120/api/users/ppi{0}", MainActivityContext.id);
            var content = new StringContent(data, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PutAsync(putPath, content);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    Toast.MakeText(this, "Succeed", ToastLength.Short).Show();
            }
            catch(Exception e)
            {
                Toast.MakeText(this, e.ToString(), ToastLength.Short).Show();
            }
               
        }

        private async void SetImageButton_Click(object sender, EventArgs e)
        {
            await UploadPhoto();
        }

        public static byte[] getBitmapAsByteArray(Bitmap bitmap)
        {
            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 0, new System.IO.MemoryStream());
            return outputStream.ToByteArray();
        }

        //private void TimeSelectOnClick(object sender, EventArgs eventArgs)
        //{
        //    TimePickerFragment frag = TimePickerFragment.NewInstance(
        //        delegate (DateTime time)
        //        {
        //            timeDisplay.Text = time.ToShortTimeString();

        //            Preferences.Set("timeToNotify", time);
        //            SetAlarm(time);
        //        });

        //    frag.Show(FragmentManager, TimePickerFragment.TAG);
        //}

        //private void SetAlarm(DateTime time)
        //{
        //    Intent alarmIntent = new Intent(this, typeof(AlarmBroadcast));
        //    PendingIntent pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);
        //    AlarmManager alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();

        //    alarmManager.SetRepeating(AlarmType.RtcWakeup, AlarmBroadcast.FirstReminder(time), AlarmBroadcast.reminderInterval, pending);
        //    PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, alarmIntent, 0);
        //}

        private void backButton_Click(object sender, EventArgs e)
        {
            base.OnBackPressed();
        }

        async Task UploadPhoto()
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                    CompressionQuality = 50

                });
                imageByte = System.IO.File.ReadAllBytes(file.Path);
                Bitmap bitmap = BitmapFactory.DecodeByteArray(imageByte, 0, imageByte.Length);
                FindViewById<ImageView>(Resource.Id.newUserProfileImage).SetImageBitmap(bitmap);

                string filesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath + "/Pictures/temp/";
                DirectoryInfo d = new DirectoryInfo(filesPath);

                if (d.Exists == true)
                {
                    FileInfo[] Files = d.GetFiles("*.jpg");
                    foreach (FileInfo tempfile in Files)
                        System.IO.File.Delete(filesPath + tempfile.Name);
                }

                confirmButton.ViewTreeObserver.GlobalLayout += (sender, args) =>
                {
                    if (MainActivityContext.Anonymous == true || imageByte == null)
                        confirmButton.Enabled = false;
                };
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.ToString(), ToastLength.Short).Show();
            }
        }

        protected override void OnStop()
        {
            string filesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath + "/Pictures/temp/";
            DirectoryInfo d = new DirectoryInfo(filesPath);

            if (d.Exists == true)
            {
                FileInfo[] Files = d.GetFiles("*.jpg");
                foreach (FileInfo file in Files)
                    System.IO.File.Delete(filesPath + file.Name);
            }

            base.OnStop();
        }

    }
}