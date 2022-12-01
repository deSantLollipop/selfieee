using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bitmap = Android.Graphics.Bitmap;

namespace selfeee_cam
{
    [Activity(Label = "PunkActivity")]
    public class PunkActivity : Activity
    {
        Button btnBack;
        ImageView imgPunk;
        TextView txtPunk;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.punk_layout);

            imgPunk = FindViewById<ImageView>(Resource.Id.punkResultImg);
            btnBack = FindViewById<Button>(Resource.Id.btnPunkBack);
            txtPunk = (TextView)FindViewById(Resource.Id.txtPunk);

            btnBack.Click += BtnBack_Click;

            Typeface typeface = Typeface.CreateFromAsset(Assets, "Satisfy-Regular.ttf");
            btnBack.SetTypeface(typeface, TypefaceStyle.Normal);
            txtPunk.SetTypeface(typeface, TypefaceStyle.Normal);

            imgPunk.SetImageBitmap(PrivatePhotosBitmap.choosedBitmaps.First().Value);

            string tmp = string.Empty;

            FaceDetection faceDetection = new FaceDetection();
            float punkCoef = faceDetection.AreYouPunk(ApplicationContext);
            
            int x = (int)faceDetection.compare_X, y = (int)faceDetection.compare_Y,
                  h = (int)faceDetection.compare_H, w = (int)faceDetection.compare_W;
            Bitmap bitmap = PrivatePhotosBitmap.choosedBitmaps.First().Value;

            //Bitmap bitmap = faceDetection.DrawDetectLandmarks(ApplicationContext);
            
            
            if (punkCoef == -1)
            {
                tmp = "Face Detector could not be set up on your device";
                txtPunk.SetText(tmp, TextView.BufferType.Spannable);
            }
            else if (punkCoef == -2)
            {
                tmp = "Could not detect face, try another photo";
                txtPunk.SetText(tmp, TextView.BufferType.Spannable);
            }
            else
            {
                tmp = $"You are Punk at {Math.Round(punkCoef,2)}%";
                txtPunk.SetText(tmp, TextView.BufferType.Spannable);

                #region in progress

                // Drawable[] layers = new Drawable[2];
                // layers[0] = new BitmapDrawable(PrivatePhotosBitmap.choosedBitmaps.First().Value);
                // Drawable[] layers = new Drawable[1];
                // layers[0] = new BitmapDrawable(bitmap);

                Bitmap bitmap1 = Bitmap.CreateBitmap(bitmap, x, y, w,h);

                #endregion
                
                Drawable[] layers = new Drawable[2];
                layers[0] = new BitmapDrawable(bitmap1);

                layers[1] = new BitmapDrawable(BitmapFactory.DecodeResource(ApplicationContext.Resources, Resource.Drawable.punk));

                layers[1].SetAlpha((int)Math.Floor(255 * (punkCoef / 2.5 / 100)));


                LayerDrawable layerDrawable = new LayerDrawable(layers);
                imgPunk.SetImageDrawable(layerDrawable);
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            base.OnBackPressed();
        }
    }
}