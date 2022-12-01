using System;
using System.Collections.Generic;

using System.IO;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Graphics;

using Android.Media;
using Android.Widget;

namespace selfeee_cam
{
    class Collage
    {
        public Bitmap doCollage(int photosNum) 
        {
           
            List<Bitmap> bitmaps = new List<Bitmap>();         //список битмапов

            Bitmap result = null;

            if (PrivatePhotosBitmap.choosedBitmaps.Count >= 2)
            {
                foreach (var tmp in PrivatePhotosBitmap.choosedBitmaps.Values)
                    bitmaps.Add(tmp);
                
                bitmaps.Reverse(); //проверка с дурацкой картинкой

                //////              код работы над списком битмапов

                int width = 0;
                List<int> imageHeights = new List<int>();                     //спсисок высот картинок
                List<int> imageWidth = new List<int>();                       //список широт картинок


                #region for saving
                //var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                //string v = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                //var filePath = System.IO.Path.Combine(sdCardPath, "collage" + v + ".jpg");
                //var stream = new FileStream(filePath, FileMode.Create);
                #endregion

                int x = 0, y = 0;

                int height;

                switch (photosNum)
                {
                    case 0:                                                                           ////все фотки в ряд

                        for (int i = 0; i < bitmaps.Count; i++)
                        {
                            bitmaps[i] = prepareBitmaps(bitmaps[i], bitmaps[0].Width, bitmaps[0].Height);
                            width += bitmaps[i].Width + 10;// Всеволод: тут ширину считать надо было для всех фоток, добрый день
                        }

                        //int height = imageHeights[0];                               //записываем самое меньшее? значение высоты в переменную

                        height = bitmaps[0].Height + 10;// Всеволод: тут что-то не работает, линию выше я закомментировал потому что там тупо пустая листа, а эту скопировал с кейса ниже


                        Bitmap bitmap0 = Bitmap.CreateBitmap(width, height + 10, Bitmap.Config.Argb8888);  //создаем битмап с определенной шириной и высотой
                        bitmap0.EraseColor(Android.Graphics.Color.White);
                        Canvas canvas0 = new Canvas(bitmap0);                                         //канвас из нашего битмапа
                        Rect baseRect = new Rect(0, 0, bitmaps[0].Width, bitmaps[0].Height);
                        width = 0;

                        x = 5;
                        y = 5;

                        for (int i = 0; i < bitmaps.Count; i++)                                      //проходимся по всем битмапам в списке
                        {
                            baseRect = new Rect(x, y, bitmaps[i].Width + x, bitmaps[i].Height + y); // first image
                            canvas0.DrawBitmap(bitmaps[i], null, baseRect, null);
                            x += bitmaps[i].Width + 10;
                        }

                        result = bitmap0;

                        break;

                    case 2:
                        if (PrivatePhotosBitmap.choosedBitmaps.Count == 2)
                        {
                            for (int i = 1; i < bitmaps.Count; i++)
                            {
                                bitmaps[i] = prepareBitmaps(bitmaps[i], bitmaps[0].Width, bitmaps[0].Height);
                            }

                            width = bitmaps[0].Width + bitmaps[1].Width + 20;
                            height = bitmaps[0].Height + 10;

                            Bitmap bitmap2 = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                            bitmap2.EraseColor(Android.Graphics.Color.White);
                            Canvas canvas2 = new Canvas(bitmap2);

                            x = 5;
                            y = 5;
                            baseRect = new Rect(x, y, bitmaps[0].Width + x, bitmaps[0].Height + y);
                            canvas2.DrawBitmap(bitmaps[0], null, baseRect, null);

                            x = bitmaps[0].Width + 15;
                            y = 5;
                            baseRect = new Rect(x, y, bitmaps[1].Width + x, bitmaps[1].Height + y);
                            canvas2.DrawBitmap(bitmaps[1], null, baseRect, null);

                            result = bitmap2;
                        }
                        else
                            Toast.MakeText(Android.App.Application.Context, "Only for 2 selected images", ToastLength.Short).Show();
                        break;

                    case 3:
                        if (PrivatePhotosBitmap.choosedBitmaps.Count == 3)
                        {
                            for (int i = 1; i < bitmaps.Count; i++)
                            {
                                bitmaps[i] = prepareBitmaps(bitmaps[i], bitmaps[0].Width, bitmaps[0].Height);
                            }

                            if (bitmaps[0].Width == bitmaps[1].Width && bitmaps[0].Width != bitmaps[2].Width)
                            {
                                Bitmap image = bitmaps[0].Copy(Bitmap.Config.Argb8888, true);
                                bitmaps[0] = bitmaps[2].Copy(Bitmap.Config.Argb8888, true);
                                bitmaps[2] = image.Copy(Bitmap.Config.Argb8888, true);
                            }

                            if (bitmaps[0].Width == bitmaps[2].Width && bitmaps[0].Width != bitmaps[1].Width)
                            {
                                Bitmap image = bitmaps[0].Copy(Bitmap.Config.Argb8888, true);
                                bitmaps[0] = bitmaps[1].Copy(Bitmap.Config.Argb8888, true);
                                bitmaps[1] = image.Copy(Bitmap.Config.Argb8888, true);
                            }

                            width = bitmaps[0].Width * 2 + bitmaps[1].Width + 20;
                            height = bitmaps[0].Height * 2 + 20;
                            Bitmap bitmap3 = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                            bitmap3.EraseColor(Android.Graphics.Color.White);
                            Canvas canvas3 = new Canvas(bitmap3);

                            y = 5;
                            x = 5;
                            baseRect = new Rect(x, y, bitmaps[1].Width + x, bitmaps[1].Height + y);
                            canvas3.DrawBitmap(bitmaps[1], null, baseRect, null);

                            y = bitmaps[1].Height + 15;
                            x = 5;
                            baseRect = new Rect(x, y, bitmaps[2].Width + x, bitmaps[2].Height + y);
                            canvas3.DrawBitmap(bitmaps[2], null, baseRect, null);

                            y = (height - bitmaps[0].Height * 2) / 2;
                            x = bitmaps[1].Width + 15;
                            baseRect = new Rect(x, y, bitmaps[0].Width * 2 + x, bitmaps[0].Height * 2 + y);
                            canvas3.DrawBitmap(bitmaps[0], null, baseRect, null);

                            result = bitmap3;
                        }
                        else
                            Toast.MakeText(Android.App.Application.Context, "Only for 3 selected images", ToastLength.Short).Show();

                        break;

                    default:
                        Console.WriteLine("Default case. In this case.. Houston, we have a problem..");
                        break;
                }
            }
            else
            {
                Toast.MakeText(MainActivityContext.mContext, "Please select at least 2 photos first", ToastLength.Short).Show();
            }

            if (result != null)
            {
                ////watermark
                Bitmap newBitmap;
                Bitmap aBitmapToApplyWaterMarkTo = result;
                Bitmap waterMarkBitmap = BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.logo3);

                newBitmap = aBitmapToApplyWaterMarkTo.Copy(aBitmapToApplyWaterMarkTo.GetConfig(), true);
                var canvas = new Canvas(newBitmap);

                canvas.DrawBitmap(Bitmap.CreateScaledBitmap(waterMarkBitmap,100,100,false), newBitmap.Width - 100, newBitmap.Height - 100, null);
                
                return newBitmap;
                ////watermark
            }

                return result;
            }

            private static Bitmap prepareBitmaps(Bitmap image, int maxWidth, int maxHeight)
            {
                if (maxHeight > 0 && maxWidth > 0)
                {
                    int width = image.Width;
                    int height = image.Height;
                    float ratioBitmap = (float)width / (float)height;
                    float ratioMax = (float)maxWidth / (float)maxHeight;

                    int finalWidth = maxWidth;
                    int finalHeight = maxHeight;
                    if (ratioMax > ratioBitmap)
                    {
                        finalWidth = (int)((float)maxHeight * ratioBitmap);
                    }
                    else
                    {
                        finalHeight = (int)((float)maxWidth / ratioBitmap);
                    }
                    image = Bitmap.CreateScaledBitmap(image, finalWidth, finalHeight, true);
                    return image;
                }
                else
                {
                    return image;
                }
            }
        }
    }