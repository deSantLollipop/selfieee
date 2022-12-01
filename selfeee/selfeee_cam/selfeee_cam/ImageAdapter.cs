using Android.Graphics;
using Android.Views;
using Android.Widget;
using System.Runtime.Remoting.Contexts;

namespace selfeee_cam
{
    public class ImageAdapter : BaseAdapter
    {
        Android.Content.Context context;
        float pixelWidth;

        public ImageAdapter(Android.Content.Context c, float pixelWidth)
        {
            context = c;
            this.pixelWidth = pixelWidth;
        }

        public override int Count
        {
            get { return Images.Length; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        // create a new ImageView for each item referenced by the Adapter
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ImageView imageView;
            int width, height;
            CalculateLayoutParams(pixelWidth, out width, out height);

            if (convertView == null)
            {  // if it's not recycled, initialize some attributes
                imageView = new ImageView(context);
                imageView.LayoutParameters = new GridView.LayoutParams(width, height); 
                imageView.SetScaleType(ImageView.ScaleType.CenterCrop);
                imageView.CropToPadding = true;
                imageView.SetPadding(8, 8, 8, 8);
            }
            else
            {
                imageView = (ImageView)convertView;
            }

            if (PrivatePhotosBitmap.choosedBitmaps.ContainsKey(position))
            {
                imageView.SetPadding(20, 20, 20, 20);
                imageView.SetBackgroundColor(Color.ParseColor("#b19cd9")); ;
            }
            else
            {
                imageView.SetPadding(8, 8, 8, 8);
                imageView.SetBackgroundColor(Color.Transparent);
            }

            imageView.SetImageBitmap(Images[position]);
            return imageView;
        }

        private void CalculateLayoutParams(float widthPixels, out int width, out int height)
        {
            width =  (int)(widthPixels/3.25);
            height = (int)(width * 1.25);
        }

        // references to our images
        public  Bitmap[] Images { get; set; }
    }
}