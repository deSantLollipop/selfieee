using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Xamarin.Essentials;

namespace selfeee_cam
{
    [Activity(Label = "CollageActivity")]
    public class CollageActivity : Activity
    {
        sbyte collageType = 0;

        TextView txtCollage;
        Spinner spinner;
        Button btnMakeCollage;
        ImageView imgCollage;

        Button btnBack;
        Button btnSave;
        Button btnExport;

        string filePath;
        string filesPath;
        string fileName;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CollageLayout);

            txtCollage = FindViewById<TextView>(Resource.Id.txtCollage);
            spinner = FindViewById<Spinner>(Resource.Id.spinner);
            btnMakeCollage = FindViewById<Button>(Resource.Id.btnMakeCollage);
            imgCollage = FindViewById<ImageView>(Resource.Id.collageResultImg);

            btnBack = FindViewById<Button>(Resource.Id.btnCollageBack);
            btnSave = FindViewById<Button>(Resource.Id.btnSaveCollage);
            btnExport = FindViewById<Button>(Resource.Id.btnExportCollage);

            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            TypefaceSpan span = new TypefaceSpan(typeface);

            txtCollage.SetTypeface(typeface, TypefaceStyle.Normal);
            btnMakeCollage.SetTypeface(typeface, TypefaceStyle.Normal);
            btnBack.SetTypeface(typeface, TypefaceStyle.Normal);
            btnSave.SetTypeface(typeface, TypefaceStyle.Normal);
            btnExport.SetTypeface(typeface, TypefaceStyle.Normal);


            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);

            var items = new List<string>() { "Choose option", "All images in line", "2 images", "3 images" };
            List<SpannableStringBuilder> itemsFormatted = new List<SpannableStringBuilder>();

            foreach (var tmp in items)
            {
                SpannableStringBuilder title = new SpannableStringBuilder(" " + tmp);
                title.SetSpan(span, 0, title.Length(), 0);
                title.SetSpan(new RelativeSizeSpan(1.0f), 0, title.Length(), 0);

                itemsFormatted.Add(title);
            }


            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, itemsFormatted);//CreateFromResource(this, Resource.Array.collageTypes_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            btnMakeCollage.Click += BtnMakeCollage_Click;
            btnBack.Click += BtnBack_Click;
            btnSave.Click += BtnSave_Click;
            btnExport.Click += BtnExport_Click;

            btnSave.Enabled = false;
            btnExport.Enabled = false;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            Java.IO.File f = new Java.IO.File(filePath);

            var context = Android.App.Application.Context;
            var component = new Android.Content.ComponentName(context, Java.Lang.Class.FromType(typeof(Android.Support.V4.Content.FileProvider)));
            var info = context.PackageManager.GetProviderInfo(component, Android.Content.PM.PackageInfoFlags.MetaData);
            var authority = info.Authority;

            Android.Net.Uri contentUri = FileProvider.GetUriForFile(this, authority,f);
            ShareImageUri(contentUri);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);

            string newDir = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim) + "/Camera/" + "/Selfieee";
            Java.IO.File storageDir = new Java.IO.File(newDir);
            if (!storageDir.Exists())
                storageDir.Mkdirs();

            string newFileDir = System.IO.Path.Combine(newDir, fileName);
            System.IO.File.Copy(filePath, newFileDir);

            Java.IO.File f = new Java.IO.File(newFileDir);

            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(f);
            mediaScanIntent.SetData(contentUri);
            this.SendBroadcast(mediaScanIntent);

            Toast.MakeText(this, "Saved to gallery", ToastLength.Long).Show();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            base.OnBackPressed();
        }

        private void BtnMakeCollage_Click(object sender, EventArgs e)
        {
            Collage collage = new Collage();
            Bitmap tmp = null;
            switch (collageType)
            {
                case 0:
                    Toast.MakeText(this, "Please, select option first", ToastLength.Short).Show();
                    break;
                case 1:
                    tmp = collage.doCollage(0);
                    if (tmp != null)
                        imgCollage.SetImageBitmap(tmp);
                    break;
                case 2:
                    tmp = collage.doCollage(2);
                    if (tmp != null)
                        imgCollage.SetImageBitmap(tmp);
                    break;
                case 3:
                    tmp = collage.doCollage(3);
                    if (tmp != null)
                        imgCollage.SetImageBitmap(tmp);
                    break;
                default:
                    Toast.MakeText(this, "Collage type isn't choosed. Please contact support", ToastLength.Long);
                    break;
            }
            if (tmp != null)
                ExportBitmapAsPNG(tmp);
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            collageType = (sbyte)e.Position;
        }

        void ExportBitmapAsPNG(Bitmap bitmap)
        {
            filesPath = Application.Context.GetExternalFilesDir(null).AbsolutePath + "/Pictures/temp";
            DirectoryInfo d = new DirectoryInfo(filesPath);
            if (d.Exists == false)
                Directory.CreateDirectory(filesPath);

            fileName = "ImgS" + DateTime.Now.ToFileTimeUtc() + ".png";
            filePath = System.IO.Path.Combine(filesPath, fileName);
            var stream = new FileStream(filePath, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            stream.Close();

            btnSave.Enabled = true;
            btnExport.Enabled = true;
        }

        private void ShareImageUri(Android.Net.Uri uri)
        {
            Intent intent = new Intent(Android.Content.Intent.ActionSend);
            intent.PutExtra(Intent.ExtraStream, uri);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);
            intent.SetType("image/png");
            StartActivity(intent);
        }

        protected override void OnStop()
        {
            DirectoryInfo d = null;
            if (filesPath != null)
            {
                d = new DirectoryInfo(filesPath);

                if (d.Exists == true)
                {
                    FileInfo[] Files = d.GetFiles();

                    foreach (FileInfo file in Files)
                        file.Delete();
                }
            }
            base.OnStop();
        }

    }

}