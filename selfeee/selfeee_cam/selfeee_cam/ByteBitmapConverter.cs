using System.Collections.Generic;
using System.IO;
using Android.Graphics;

namespace selfeee_cam
{
    public static class ByteBitmapConverter
    {
        public static List<Bitmap> ByteToBitmapList(byte[][] byteArrayIn)
        {
            var res = new List<Bitmap>();
            foreach (byte[] img in byteArrayIn)
            {
                var bitmap = BitmapFactory.DecodeByteArray(img, 0, img.Length);
                res.Add(bitmap);
            }
            return res;
        }

        public static byte[][] BitmapToByteArray(List<Bitmap> bmp)
        {
            var res = new List<byte[]>();
            foreach (Bitmap item in bmp)
            {
                using (var ms = new MemoryStream())
                {
                    item.Compress(Bitmap.CompressFormat.Png, 0, ms);
                    res.Add(ms.ToArray());
                }
            }
            return res.ToArray();
        }
    }
}
