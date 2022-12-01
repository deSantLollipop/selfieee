using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Vision;
using Android.Gms.Vision.Faces;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Annotation;
using Android.Util;
using Android.Views;
using Android.Widget;
using Org.Opencv.Core;
using Point = Android.Graphics.Point;

namespace selfeee_cam
{
    class FaceDetection
    {
        Bitmap myBitmap; //face
        Bitmap origin;
        Bitmap compare;

        float origin_H, origin_W, origin_X, origin_Y;
        public float compare_H, compare_W, compare_X, compare_Y;

        Frame frame;
        SparseArray sparseArray;
        IList<Landmark> landmarks;
        IList<Landmark> tempLandmarks;
        IList<Landmark> additlandmarks;

        FileStream stream;

        void Init(Context ApplicationContext)
        {
            Bitmap b = PrivatePhotosBitmap.choosedBitmaps.First().Value;
            myBitmap = b.Copy(Bitmap.Config.Argb8888, true);
        }

        void ResourceInit(Context ApplicationContext)
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InMutable = true;
            options.InJustDecodeBounds = false;

            //ТУТ выбор фотки //пока что панк
            Bitmap newBitmap = BitmapFactory.DecodeResource(ApplicationContext.Resources, Resource.Drawable.punk);
            myBitmap = newBitmap.Copy(Bitmap.Config.Argb8888, true);

        }

        short DetectFace(Context ApplicationContext)
        {
            FaceDetector faceDetector = new FaceDetector.Builder(ApplicationContext)
                    .SetTrackingEnabled(false)
                    .SetLandmarkType(LandmarkDetectionType.All)
                    .SetMode(FaceDetectionMode.Accurate)
                    .Build();
            if (!faceDetector.IsOperational)
            {
                return -1;
            }
            frame = new Frame.Builder().SetBitmap(myBitmap).Build();
            sparseArray = faceDetector.Detect(frame);
            if (sparseArray.Size() == 0)                                                                // есть ли лицо
            {
                return -2;
            }
            else
            {
                return 1;
            }
        }

        public Bitmap DrawDetectLandmarks(Context ApplicationContext)
        {

            Paint myRectPaint = new Paint();
            myRectPaint.StrokeWidth = 5;
            myRectPaint.Color = Color.Green;
            myRectPaint.SetStyle(Paint.Style.Stroke);

            Paint landmarksPaint = new Paint();
            landmarksPaint.StrokeWidth = 10;
            landmarksPaint.Color = Color.Red;
            landmarksPaint.SetStyle(Paint.Style.Stroke);

            Bitmap tempBitmap = Bitmap.CreateBitmap(myBitmap.Width, myBitmap.Height, Bitmap.Config.Rgb565);
            Canvas tempCanvas = new Canvas(tempBitmap);
            tempCanvas.DrawBitmap(myBitmap, 0, 0, null);

            for (int i = 0; i < sparseArray.Size(); i++)
            {
                Face face = (Face)sparseArray.ValueAt(i);
                float x1 = face.Position.X;
                float y1 = face.Position.Y;
                float x2 = x1 + face.Width;
                float y2 = y1 + face.Height;
                tempCanvas.DrawRoundRect(new RectF(x1, y1, x2, y2), 2, 2, myRectPaint);

                //not kill
                //for (int l = 0; l < additlandmarks.Count; l++)
                //{
                //    PointF pos = additlandmarks[l].Position;
                //    tempCanvas.DrawPoint(pos.X, pos.Y, landmarksPaint);
                //}
            }

            #region //only if we save this (DONT KILL THIS CODE PLEASE !!!) *UPD*
            string sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            string v = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string filePath = System.IO.Path.Combine(sdCardPath, "test-date-time" + v + ".jpg");
            //Всеволод: перенёс то что выше сюда потому что ты дал им статику. А статические переменные не изменяются, они намертво сидят в памяти, поэтому
            //если выбрать другую фотку и нажать кнопку 'Are you punk?' то названия файла бы не изменилось.
            //Но вообще, надо ли нам записывать это?
            bool da = false;
            if (da)
            {
                stream = new FileStream(filePath, FileMode.Create);
                tempBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, stream);
                stream.Close();
            }
            #endregion 
        
            return tempBitmap;
        
        }

        void DetectLandmarks(Context ApplicationContext, bool who)
        {

            for (int i = 0; i < sparseArray.Size(); i++) // если оно в цикле абы находить разные лица то записываю я и так последнее получается
            {
                Face face = (Face)sparseArray.ValueAt(i);

                if (who)
                {
                    origin_H = face.Height;
                    origin_W = face.Width;
                    origin_X = face.Position.X;
                    origin_Y = face.Position.Y;
                    origin = Bitmap.CreateBitmap(myBitmap, (int)face.Position.X, (int)face.Position.Y, (int)face.Width, (int)face.Height);
                }
                else
                {
                    compare_H = face.Height;
                    compare_W = face.Width;
                    compare_X = face.Position.X;
                    compare_Y = face.Position.Y;
                    compare = Bitmap.CreateBitmap(myBitmap, (int)face.Position.X, (int)face.Position.Y, (int)face.Width, (int)face.Height);
                }

                landmarks = face.Landmarks;
            }
        }

        void PunkFace(Context ApplicationContext)                  
        {
            ResourceInit(ApplicationContext);

            if (DetectFace(ApplicationContext) != 1)
                return;

            DetectLandmarks(ApplicationContext, true);  // bool = true - указываем, что функцию вызываем от сюда

            tempLandmarks = landmarks;
        }

        float CompareLandmarks(Context ApplicationContext)
        {
            #region preparing landmarks
            
            //additlandmarks = landmarks; //do not kill this
            
            foreach (var mark in tempLandmarks)
            {
                mark.Position.X -= origin_X;
                mark.Position.Y -= origin_Y;
            }
            foreach (var mark in landmarks)
            {
                mark.Position.X -= compare_X;
                mark.Position.Y -= compare_Y;
            }

            float dioOrigin = (float)Math.Pow((Math.Pow(origin_H, 2.0) + Math.Pow(origin_W, 2.0)), 0.5);
            float dioCompare = (float)Math.Pow((Math.Pow(compare_H, 2.0) + Math.Pow(compare_W, 2.0)), 0.5);

            float coef = dioCompare / dioOrigin;

            foreach (var mark in tempLandmarks)
            {
                mark.Position.X *= coef;
                mark.Position.Y *= coef;
            }

            #endregion

            #region comparing landmarks coordinates 
            int j = 0;
            float[] delta = new float[landmarks.Count];

            float[] deltaProcentExeptEar = new float[landmarks.Count - 4];
            Dictionary<LandmarkType, float> dictonary = new Dictionary<LandmarkType, float>();

            float x = 0.0F, y = 0.0F, tmp = 0.0F, mean = 0.0F, sum = 0.0F, res = 0.0F;


            for (int i = 0; i < landmarks.Count; i++)
            {
                if ((landmarks[i].Type == LandmarkType.LeftEar) || (landmarks[i].Type == LandmarkType.LeftEarTip)
                    || (landmarks[i].Type == LandmarkType.RightEar) || (landmarks[i].Type == LandmarkType.RightEarTip))
                    continue;
                else
                {

                    x = (float)Math.Pow(landmarks[i].Position.X - tempLandmarks[i].Position.X, 2.0);
                    y = (float)Math.Pow(landmarks[i].Position.Y - tempLandmarks[i].Position.Y, 2.0);

                    delta[i] = (float)Math.Pow(x + y, 0.5);

                    tmp = (float)Math.Pow(Math.Pow(tempLandmarks[i].Position.X, 2.0) + Math.Pow(tempLandmarks[i].Position.Y, 2.0), 0.5);

                    deltaProcentExeptEar[j] = (100 - (delta[i] * 100) / tmp);
                    dictonary.Add(landmarks[i].Type, deltaProcentExeptEar[j]);
                    j++;
                }
            }

            #endregion

            #region simularity conditions and additional calculations
            
            float sim = 100.0f;
            j = 0;
            foreach (KeyValuePair<LandmarkType, float> kvp in dictonary)
            {
                if (kvp.Value < sim)
                {
                    sim = kvp.Value;
                    j++;
                }
            }

            j = 0;
            bool sim_flag = true;
            foreach (KeyValuePair<LandmarkType, float> kvp in dictonary)
            {
                if (kvp.Key == LandmarkType.LeftEye && kvp.Value <= 92.0f)
                    sim_flag = false;
                if (kvp.Key == LandmarkType.RightEye && kvp.Value <= 92.0f)
                    sim_flag = false;
                if (kvp.Key == LandmarkType.BottomMouth && kvp.Value <= 92.0f)
                    sim_flag = false;
            }

            if (sim >= 96 && sim_flag == true)
                res = sim;
            else if (sim_flag == true && sim >= 90)
            {
                foreach (var i in deltaProcentExeptEar)
                    sum += i;
                mean = sum / (deltaProcentExeptEar.Length);
                res = mean;
            }
            else if (sim_flag == true && sim >= 85 && sim < 90)
            {
                foreach (var i in deltaProcentExeptEar)
                    sum += i;
                mean = sum / (deltaProcentExeptEar.Length * 1.1f);
                res = mean;
            }
            else if (sim_flag == true && sim >= 60 && sim < 90)
            {
                foreach (var i in deltaProcentExeptEar)
                    sum += i;
                mean = sum / (deltaProcentExeptEar.Length * 1.2f);
                res = mean;
            }
            else if (sim_flag == true)
            {
                foreach (var i in deltaProcentExeptEar)
                    sum += i;
                mean = sum / (deltaProcentExeptEar.Length * 1.5f);
                res = mean;
            }
            else if (sim_flag == false)
            {
                foreach (var i in deltaProcentExeptEar)
                    sum += i;
                mean = sum / (deltaProcentExeptEar.Length * 2.0f);
                res = (mean + sim) / 2.0f;
            }
            #endregion

            return res;
        }

        public float AreYouPunk(Context ApplicationContext) 
        {
            float your_coefficient;

            PunkFace(ApplicationContext);

            Init(ApplicationContext);

            if (DetectFace(ApplicationContext) == -1)//не работает распознавалка
                return -1;
            else if (DetectFace(ApplicationContext) == -2)//не может найти face
                return -2;

            DetectLandmarks(ApplicationContext, false); // bool = false - указываем, что функцию вызываем от сюда

            return your_coefficient = CompareLandmarks(ApplicationContext);

        }
    }
}