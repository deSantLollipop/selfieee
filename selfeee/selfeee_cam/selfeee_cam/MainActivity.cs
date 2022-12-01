using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using System;
using Android;
using Plugin.Media;
using Android.Graphics;
using System.IO;
using Android.Content;
using Xamarin.Essentials;
using System.Collections.Generic;
using Android.Views;
using Android.Support.V4.Widget;
using System.ComponentModel;
using System.Threading;
using V7Toolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;
using Refractored.Controls;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Android.Runtime;
using Android.Graphics.Drawables;
using Android.Text.Style;
using Android.Text;
using System.Timers;
using ImageMagick;

namespace selfeee_cam
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        Button captureButton;
        Button uploadButton;
        Button collageButton;
        Button punkButton;

        SwipeRefreshLayout photosLayout;

        V7Toolbar toolbar;

        TextView userName;
        View headerView;

        DrawerLayout drawerLayout;
        NavigationView navigationView;

        GridView gridBase;
        ImageAdapter ImgAdapt;

        //Intent notiBackgroundIntent;

        bool tmpCheck = true;

        public static readonly int PickImageId = 1000;
        static readonly HttpClient client = new HttpClient();

        Handler handler;

        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            captureButton = (Button)FindViewById(Resource.Id.captureButton);
            uploadButton = (Button)FindViewById(Resource.Id.uploadButton);

            //nextButton = (Button)FindViewById(Resource.Id.nextButton);
            //thisImageView = (ImageView)FindViewById(Resource.Id.thisImageView);           

            collageButton = (Button)FindViewById(Resource.Id.makeCollage);
            punkButton = (Button)FindViewById(Resource.Id.btnPunk);

            gridBase = FindViewById<GridView>(Resource.Id.newprivatePhotos);

            photosLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.photoLayout);

            gridBase.ItemLongClick += gridBase_ItemLongClick;

            RequestPermissions(permissionGroup, 0);

            captureButton.Click += captureButton_Click;
            uploadButton.Click += uploadButton_Click;
            //nextButton.Click += nextButton_Click;
            collageButton.Click += CollageButton_Click;
            punkButton.Click += PunkButton_Click;

            photosLayout.SetColorSchemeColors(Color.Black);
            photosLayout.Refresh += RefreshLayout_Refresh;
            drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            // Create ActionBarDrawerToggle button and add it to the toolbar  
            toolbar = FindViewById<V7Toolbar>(Resource.Id.toolbar);
            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            TypefaceSpan span = new TypefaceSpan(typeface);
            SpannableStringBuilder title = new SpannableStringBuilder(" " + Resources.GetString(Resource.String.app_name));
            title.SetSpan(span, 0, title.Length(), 0);
            title.SetSpan(new RelativeSizeSpan(1.5f), 0, title.Length(), 0);
            toolbar.TitleFormatted = title;
            SetSupportActionBar(toolbar);
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.drawer_open, Resource.String.drawer_close);
            drawerLayout.SetDrawerListener(drawerToggle);
            drawerToggle.SyncState();

            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            setupDrawerContent(navigationView); //Calling Function  

            Vto_Margins();
            //ViewTreeObserver vto = photosLayout.ViewTreeObserver;
            //vto.GlobalLayout += Globallayout_handler;
            //vto.GlobalLayout += (sender, args) =>
            //{
            //    setMargins(photosLayout, captureButton.Height, toolbar.Height);                
            //};                       

            headerView = navigationView.GetHeaderView(0);
            userName = headerView.FindViewById<TextView>(Resource.Id.navheader_username);

            InitProfile();

            MainActivityContext.mContext = this;

            //ViewTreeObserver vto = photosLayout.ViewTreeObserver;
            //vto.GlobalLayout -= Globallayout_handler;

            captureButton.SetTypeface(typeface, TypefaceStyle.Normal);
            uploadButton.SetTypeface(typeface, TypefaceStyle.Normal);
            collageButton.SetTypeface(typeface, TypefaceStyle.Normal);
            userName.SetTypeface(typeface, TypefaceStyle.Normal);
            punkButton.SetTypeface(typeface, TypefaceStyle.Normal);

            PrivatePhotosBitmap.choosedBitmaps = new Dictionary<int, Bitmap>();

            if (MainActivityContext.Anonymous == false)
                CheckConnectivity();
        }

        private void PunkButton_Click(object sender, EventArgs e)
        {          
            if (PrivatePhotosBitmap.choosedBitmaps.Count == 1)
            {
                StartActivity(typeof(PunkActivity));
            }
            else
                Toast.MakeText(MainActivityContext.mContext, "Please select only one photo", ToastLength.Short).Show();
        }

        private void CheckConnectivity() //работает суперхуёво
        {
            handler = new Handler();
            int delay = 2500;

            handler.PostDelayed(new Java.Lang.Runnable(async () =>
            {
                bool result = await OnTimedEvent();
                if (result != false)
                    handler.PostDelayed(new Java.Lang.Runnable(() => { CheckConnectivity(); }), delay);
            }), delay);
        }

        private async Task<bool> OnTimedEvent()
        {
            bool isAlive = await CheckServerConnection();
            if (isAlive == false)
            {
                RunOnUiThread(() => { Toast.MakeText(Application.Context, "Lost server connection. Switching to Anonymous mode. Please Re-Login.", ToastLength.Long).Show(); });
                MainActivityContext.Anonymous = true;
            }
            return isAlive;
        }

        private static async Task<bool> CheckServerConnection()
        {
            try
            {
                string getPath = string.Format("http://192.168.0.103:120/api/users/isAlive");
                HttpResponseMessage response = await client.GetAsync(getPath);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }

        private void Vto_Margins()
        {
            BackgroundWorker work = new BackgroundWorker();
            work.DoWork += (sender, args) =>
            {
                ViewTreeObserver vto = photosLayout.ViewTreeObserver;
                vto.GlobalLayout += Globallayout_handler;
                Thread.Sleep(5000);
                vto.GlobalLayout -= Globallayout_handler;
            };
            work.RunWorkerAsync();
        }

        void Globallayout_handler(object sender, EventArgs e)
        {
            setMargins(photosLayout, captureButton.Height, toolbar.Height);
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            float scale = Resources.DisplayMetrics.Density;
            float mDistanceToTriggerSync = toolbar.Height * scale * 2;

            var action = ev.Action;

            if (action == MotionEventActions.Down)
            {
                MotionEvent eventCopy = MotionEvent.Obtain(ev);
                float eventY = eventCopy.GetY();

                if (eventY > mDistanceToTriggerSync)
                    photosLayout.Enabled = false;
                else
                    photosLayout.Enabled = true;
            }

            return base.DispatchTouchEvent(ev);

        }

        public override void OnBackPressed()//todo 2x click to exit
        {
            for (int i = 0; i < gridBase.ChildCount; i++)
            {
                ImageView child = (ImageView)gridBase.GetChildAt(i);
                child.SetPadding(8, 8, 8, 8);
                child.SetBackgroundColor(Color.Transparent);
            }
            PrivatePhotosBitmap.choosedBitmaps.Clear();
        }

        private async void InitProfile()
        {
            if (MainActivityContext.Anonymous == false)
            {
                //userName.Text = MainActivityContext.userName;

                //string dpPath = System.IO.Path.Combine(Application.Context.GetExternalFilesDir(null).AbsolutePath, "user.db3");
                //var db = new SQLiteConnection(dpPath);
                //var data = db.Table<LoginTable>(); //Call Table  
                //var data1 = (from values in data
                //             where values.username == MainActivityContext.userName
                //             select new LoginTable
                //             {
                //                 id = values.id,
                //                 username = values.username,
                //                 password = values.password,
                //                 profileimage = values.profileimage
                //             }).ToList<LoginTable>();
                //if (data1.Count != 0)
                //{
                //    foreach (var val in data1)
                //    {
                //        if (val.profileimage != null)
                //            headerView.FindViewById<CircleImageView>(Resource.Id.userProfileImage).SetImageBitmap(BitmapFactory.DecodeByteArray(val.profileimage, 0, val.profileimage.Length));
                //    }
                //}
                userName.Text = MainActivityContext.userName;
                if (PrivatePhotosBitmap.UserProfileImage != null)
                    headerView.FindViewById<CircleImageView>(Resource.Id.userProfileImage).SetImageBitmap(PrivatePhotosBitmap.UserProfileImage);
                if (PrivatePhotosBitmap.Bitmaps == null)
                {
                    bool result = await DownloadFromServer();
                    if (result == false)
                        Toast.MakeText(this, "Server connection error", ToastLength.Short).Show();
                    //PrivatePhotosBitmap.Bitmaps = InitPhotos();
                    else
                        UploadFewPhoto();
                }
                else
                    UploadFewPhoto();

            }
            else
            {
                userName.Text = "Anonymous";
                headerView.FindViewById<CircleImageView>(Resource.Id.userProfileImage).SetImageBitmap(((BitmapDrawable)(Resources.GetDrawable(Resource.Drawable.logo3))).Bitmap);

                string filesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath + "/Pictures/";

                DirectoryInfo d = new DirectoryInfo(filesPath);

                if (d.Exists == false)
                    Directory.CreateDirectory(filesPath);

                if (PrivatePhotosBitmap.Bitmaps != null)
                    UploadFewPhoto();
            }
        }

        private void setupDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                e.MenuItem.SetChecked(true);
                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_settings:
                        StartActivity(typeof(SettingsActivity));
                        break;
                    case Resource.Id.nav_logout:
                        OnLogOut();
                        Finish();
                        StartActivity(typeof(LoginActivity));
                        break;
                    case Resource.Id.nav_about:
                        StartActivity(typeof(AboutActivity));
                        break;
                    default:
                        break;
                }
                drawerLayout.CloseDrawers();
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            navigationView.InflateMenu(Resource.Menu.nav_menu); //Navigation Drawer Layout Menu Creation  

            int[] tmpRes = new int[] { Resource.Id.nav_home, Resource.Id.nav_settings, Resource.Id.nav_about, Resource.Id.nav_logout };

            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            TypefaceSpan span = new TypefaceSpan(typeface);
            var color = new Color(51, 51, 51);

            foreach (var tmp in tmpRes)
            {
                var text = navigationView.Menu.FindItem(tmp).TitleFormatted;
                SpannableStringBuilder title = new SpannableStringBuilder(" " + text);
                title.SetSpan(span, 0, title.Length(), 0);
                title.SetSpan(new RelativeSizeSpan(1.5f), 0, title.Length(), 0);
                title.SetSpan(new ForegroundColorSpan(color), 0, title.Length(), 0);
                navigationView.Menu.FindItem(tmp).SetTitle(title);
            }

            MenuInflater.Inflate(Resource.Layout.top_buttons, menu);

            tmpRes = new int[] { Resource.Id.RemoveSelected, Resource.Id.RemoveAll };
            foreach (var tmp in tmpRes)
            {
                var text = menu.FindItem(tmp).TitleFormatted;
                SpannableStringBuilder title = new SpannableStringBuilder(" " + text);
                title.SetSpan(span, 0, title.Length(), 0);
                title.SetSpan(new RelativeSizeSpan(1.0f), 0, title.Length(), 0);
                menu.FindItem(tmp).SetTitle(title);
            }

            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.FindItem(Resource.Id.RemoveSelected).SetEnabled(PrivatePhotosBitmap.choosedBitmaps.Count > 0 ? true : false);
            menu.FindItem(Resource.Id.RemoveAll).SetEnabled(PrivatePhotosBitmap.Bitmaps != null ? (PrivatePhotosBitmap.Bitmaps.Length > 0) : false);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)  ///обслуга кнопочек на тулбаре
        {
            switch (item.ItemId)
            {
                case Resource.Id.RemoveSelected:
                    PrivatePhotosBitmap.Bitmaps = RemoveIndices(PrivatePhotosBitmap.Bitmaps, PrivatePhotosBitmap.choosedBitmaps.Keys.ToList());
                    foreach (var i in PrivatePhotosBitmap.choosedBitmaps.Keys.ToList())
                    {
                        PrivatePhotosBitmap.choosedBitmaps.Remove(i);
                        gridBase.RemoveViewInLayout(gridBase.GetChildAt(i));
                    }
                    if (MainActivityContext.Anonymous == false)
                        UploadToServer(PrivatePhotosBitmap.Bitmaps);
                    UploadFewPhoto();
                    break;
                case Resource.Id.RemoveAll:
                    Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                    short result = 0;
                    alert.SetTitle("Are you sure?");
                    alert.SetMessage("This action cannot be undone.");

                    alert.SetNegativeButton("No", (senderAlert, args) =>
                    {
                    });

                    alert.SetPositiveButton("Yes", (senderAlert, args) =>
                    {
                        PrivatePhotosBitmap.Bitmaps = null;
                        PrivatePhotosBitmap.choosedBitmaps.Clear();
                        gridBase.RemoveAllViewsInLayout();
                        if (MainActivityContext.Anonymous == false)
                            UploadToServer(PrivatePhotosBitmap.Bitmaps);
                        UploadFewPhoto();
                    });

                    Dialog dialog = alert.Create();
                    dialog.Show();

                    break;
            }

            //Toast.MakeText(this, "Action selected: " + item.TitleFormatted,
            //    ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }

        private void RefreshLayout_Refresh(object sender, EventArgs e)
        {
            if (MainActivityContext.Anonymous == true || tmpCheck != false)
            {
                tmpCheck = false;
                BackgroundWorker work = new BackgroundWorker();
                work.DoWork += Work_DoWork;
                refreshPhotos();
                work.RunWorkerCompleted += Work_RunWorkerCompleted;
                work.RunWorkerAsync();
            }
            else
                photosLayout.Refreshing = false;
        }

        private async void refreshPhotos()
        {
            /// эту часть надо будет заменить - очень не оптимально перезагружать все фотки ради обновления, надо только подгружать новые
            if (MainActivityContext.Anonymous == true)
                UploadFewPhoto();
            //PrivatePhotosBitmap.Bitmaps = InitPhotos();
            else
            {
                bool result = await DownloadFromServer();
                if (result == false)
                    Toast.MakeText(this, "Server error", ToastLength.Short).Show();
                else
                    UploadFewPhoto();
            }
            PrivatePhotosBitmap.choosedBitmaps.Clear();
            ///
        }

        private async void UploadToServer(Bitmap[] images)
        {
            List<Bitmap> imagesList = new List<Bitmap>(images);

            string result = JsonConvert.SerializeObject(ByteBitmapConverter.BitmapToByteArray(imagesList));

            string putPath = string.Format(@"http://192.168.0.103:120/api/users/pis{0}", MainActivityContext.id);
            var content = new StringContent(result, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PutAsync(putPath, content);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    Toast.MakeText(this, "Succeed", ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                //Toast.MakeText(this, e.ToString(), ToastLength.Short).Show();
                Toast.MakeText(this, "Server connection error", ToastLength.Short).Show();
            }
        }

        private async Task<bool> DownloadFromServer()
        {
            try
            {
                string getPath = string.Format(@"http://192.168.0.103:120/api/users/gi_{0}", MainActivityContext.id);
                HttpResponseMessage response = await client.GetAsync(getPath);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Toast.MakeText(this, "Succeed", ToastLength.Short).Show();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    byte[][] bsObj = JsonConvert.DeserializeObject<byte[][]>(responseBody);
                    PrivatePhotosBitmap.Bitmaps = ByteBitmapConverter.ByteToBitmapList(bsObj).ToArray();
                    tmpCheck = true;
                    return true;
                }
                else return false;
            }
            catch (Exception e)
            {
                //Toast.MakeText(this, e.ToString(), ToastLength.Short).Show();
                Toast.MakeText(this, "Server connection error", ToastLength.Short).Show();
                tmpCheck = true;
                return false;
            }
        }

        private void Work_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            photosLayout.Refreshing = false;
        }
        private void Work_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
        }

        private void CollageButton_Click(object sender, EventArgs e)
        {
            /// WORKING CODE BELOW DO NOT KILL !!!
            if (PrivatePhotosBitmap.choosedBitmaps.Count >= 2)
                StartActivity(typeof(CollageActivity));
            else
                Toast.MakeText(MainActivityContext.mContext, "Please select at least 2 photos first", ToastLength.Short).Show();        
        }

        private void setMargins(View view, int bottom, int top)  //отступ для футера с кнопками
        {
            ViewGroup.MarginLayoutParams p = (ViewGroup.MarginLayoutParams)view.LayoutParameters;

            float scale = Resources.DisplayMetrics.Density;

            int b = (int)(bottom + scale * 5) * 2;
            int t = (int)(top + scale * 5);

            p.SetMargins(0, t, 0, b);
            view.RequestLayout();
        }

        private Bitmap[] InitPhotos()
        {
            string filesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath + "/Pictures/";

            DirectoryInfo d = new DirectoryInfo(filesPath);

            if (d.Exists == false)
                Directory.CreateDirectory(filesPath);

            FileInfo[] Files = d.GetFiles("*.jpg");
            List<string> imageList = new List<string>();

            foreach (FileInfo file in Files)
                imageList.Add(filesPath + file.Name);


            byte[] imageArray = null;
            Bitmap bitmap = null;
            Stack<Bitmap> imagesList = new Stack<Bitmap>();
            Bitmap[] imagesArray;

            //BitmapFactory.Options options = new BitmapFactory.Options();//сжимаем при загрузке
            //options.InSampleSize = 20;
            //options.InJustDecodeBounds = false;

            foreach (var tmp in imageList)
            {
                imageArray = System.IO.File.ReadAllBytes(tmp);//
                //bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length, options);
                bitmap = decodeSampledBitmapFromArray(imageArray, 0, imageArray.Length, 350, 600);
                imagesList.Push(bitmap);
            }

            imagesArray = imagesList.ToArray();

            foreach (FileInfo file in Files)
                file.Delete(); foreach (FileInfo file in Files)
                imageList.Add(filesPath + file.Name);

            return imagesArray;
        }

        private int calculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                int halfHeight = height / 2;
                int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight
                        && (halfWidth / inSampleSize) >= reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        public Bitmap decodeSampledBitmapFromArray(byte[] res, int offset, int length, int reqWidth, int reqHeight)
        {

            // First decode with inJustDecodeBounds=true to check dimensions
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeByteArray(res, offset, res.Length, options);

            // Calculate inSampleSize
            options.InSampleSize = calculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            return BitmapFactory.DecodeByteArray(res, offset, res.Length, options);
        }


        private void gridBase_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            if (!PrivatePhotosBitmap.choosedBitmaps.ContainsKey(e.Position))
            {
                e.View.SetPadding(20, 20, 20, 20);
                e.View.SetBackgroundColor(Color.ParseColor("#b19cd9"));
                PrivatePhotosBitmap.choosedBitmaps.Add(e.Position, PrivatePhotosBitmap.Bitmaps[e.Position]);
            }
            else
            {
                e.View.SetPadding(8, 8, 8, 8);
                e.View.SetBackgroundColor(Color.Transparent);
                PrivatePhotosBitmap.choosedBitmaps.Remove(e.Position);
            }
        }

        private void UploadFewPhoto()
        {
            try
            {
                if (PrivatePhotosBitmap.Bitmaps != null)
                {
                    ImgAdapt = new ImageAdapter(this, Resources.DisplayMetrics.WidthPixels * 0.95f);

                    //ImageAdapter.Images = (Bitmap[])imagesArray.Clone();

                    ImgAdapt.Images = (Bitmap[])PrivatePhotosBitmap.Bitmaps.Clone();

                    gridBase.Adapter = ImgAdapt;
                }
            }
            catch (Exception e)
            {
                Toast.MakeText(this, e.ToString(), ToastLength.Short).Show();
            }
        }

        //private void StartBackgroundService()
        //{
        //    notiBackgroundIntent = new Intent(this, typeof(BackgroundService));
        //    this.StartService(notiBackgroundIntent);
        //}   

        private void captureButton_Click(object sender, EventArgs e)
        {
            TakePhoto();
        }

        private void uploadButton_Click(object sender, System.EventArgs e)
        {
            UploadPhotos(); //надо будет переделать в будущем этот метод - он должен загружать с галереи в наш фолдер фотки
        }

        private void UploadPhotos()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.PutExtra(Intent.ExtraAllowMultiple, true);
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), PickImageId);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
            {
                Random r = new Random();
                GetActualPathFileUri getActualPathFileUri = new GetActualPathFileUri(this);
                ClipData clipData = data.ClipData;
                if (clipData != null)
                {
                    for (int i = 0; i < clipData.ItemCount; i++)
                    {
                        ClipData.Item item = clipData.GetItemAt(i);
                        Android.Net.Uri uri = item.Uri;
                        File.Copy(getActualPathFileUri.GetActualPathFromFile(uri), Application.Context.GetExternalFilesDir(null).AbsolutePath +
                            "/Pictures/" + "image_" + DateTime.Now.ToShortDateString().ToString() + "_" + DateTime.Now.ToLongTimeString().ToString() + "_" +
                            r.Next(int.MaxValue).ToString() + ".jpg", true);
                    }
                }
                else if (data != null)
                {
                    Android.Net.Uri uri = data.Data;
                    File.Copy(getActualPathFileUri.GetActualPathFromFile(uri), Application.Context.GetExternalFilesDir(null).AbsolutePath +
                            "/Pictures/" + "image_" + DateTime.Now.ToShortDateString().ToString() + "_" + DateTime.Now.ToLongTimeString().ToString() + "_" +
                            r.Next(int.MaxValue).ToString() + ".jpg", true);
                }

                if (PrivatePhotosBitmap.Bitmaps != null)
                    PrivatePhotosBitmap.Bitmaps = PrivatePhotosBitmap.Bitmaps.Concat(InitPhotos()).ToArray();
                else
                    PrivatePhotosBitmap.Bitmaps = InitPhotos().ToArray();
                if (MainActivityContext.Anonymous == false)
                    UploadToServer(PrivatePhotosBitmap.Bitmaps);
                UploadFewPhoto();
            }
        }

        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 100,
                Name = "image_" + DateTime.Now.ToShortDateString().ToString() + "_" + DateTime.Now.ToLongTimeString().ToString() + ".jpg",
                //SaveToAlbum = true //сохранение наших фоток в галерею
            });

            if (file == null)
                return;

            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            //////watermark
            //Bitmap newBitmap;
            //using (var aBitmapToApplyWaterMarkTo = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length))
            //using (var waterMarkBitmap = await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.cam_ico))
            //{
            //    newBitmap = aBitmapToApplyWaterMarkTo.Copy(aBitmapToApplyWaterMarkTo.GetConfig(), true);
            //    using (var canvas = new Canvas(newBitmap))
            //    {
            //        canvas.DrawBitmap(waterMarkBitmap, newBitmap.Width - 500, newBitmap.Height - 500, null);//заменить числа на проценты
            //    }
            //}
            //using (var fileStream = new FileStream(file.Path, FileMode.OpenOrCreate, FileAccess.Write))
            //{
            //    await newBitmap.CompressAsync(Bitmap.CompressFormat.Png, 100, fileStream);
            //}
            //newBitmap.Dispose();
            //////watermark
            imageArray = System.IO.File.ReadAllBytes(file.Path);

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            File.Delete(file.Path);
            //thisImageView.SetImageBitmap(bitmap);
            Bitmap[] tmp = new Bitmap[] { bitmap };
            if (PrivatePhotosBitmap.Bitmaps != null)
                PrivatePhotosBitmap.Bitmaps = PrivatePhotosBitmap.Bitmaps.Concat(tmp).ToArray();
            else
                PrivatePhotosBitmap.Bitmaps = tmp;
            if (MainActivityContext.Anonymous == false)
                UploadToServer(PrivatePhotosBitmap.Bitmaps);
            UploadFewPhoto();
        }

        protected override void OnStop()
        {
            //UploadToServer(PrivatePhotosBitmap.Bitmaps);
            //PrivatePhotosBitmap.Bitmaps = null;
            //PrivatePhotosBitmap.choosedBitmaps.Clear();
            ImgAdapt = null;
            GC.Collect();

            base.OnStop();
        }

        private void OnLogOut()
        {
            MainActivityContext.Anonymous = false;
            MainActivityContext.id = 0;
            MainActivityContext.mContext = null;
            MainActivityContext.userData = null;
            MainActivityContext.userName = null;
            PrivatePhotosBitmap.Bitmaps = null;
            PrivatePhotosBitmap.UserProfileImage = null;
            PrivatePhotosBitmap.choosedBitmaps.Clear();
            GC.Collect();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private Bitmap[] RemoveIndices(Bitmap[] IndicesArray, List<int> RemoveAt)
        {
            List<Bitmap> tmpList = new List<Bitmap>();

            foreach (var tmp in RemoveAt)
                tmpList.Add(IndicesArray[tmp]);

            foreach (var tmp in tmpList)
                IndicesArray = IndicesArray.Where(val => val != tmp).ToArray();

            return IndicesArray;
        }
    }
}

