using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Selfeee_API
{
    public static class ByteBitmapConverter
    {
        public static List<Bitmap> ByteToBitmapList(byte[][] byteArrayIn)
        {
            var res = new List<Bitmap>();
            foreach (byte[] img in byteArrayIn)
            {
                using (var ms = new MemoryStream(img))
                {
                    var bitmap = (Bitmap)Image.FromStream(ms);
                    res.Add(bitmap);
                }
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
                    item.Save(ms, ImageFormat.Bmp);
                    res.Add(ms.ToArray());
                }
            }
            return res.ToArray();
        }
    }
}
