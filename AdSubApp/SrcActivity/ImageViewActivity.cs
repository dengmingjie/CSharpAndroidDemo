using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using AdSubApp.Util;

namespace AdSubApp.SrcActivity
{
    [Activity(Label = "ImageViewActivity")]
    public class ImageViewActivity : Activity
    {
        private ImageView imageView = null;

        private Bitmap bm = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 隐藏标题栏  
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            // 隐藏状态栏  
            this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // 将背景设置为空
            this.Window.SetBackgroundDrawable(null);

            // Create your application here
            SetContentView(Resource.Layout.ImageView);

            if (imageView == null)
            {
                imageView = FindViewById<ImageView>(Resource.Id.imageView1);
            }
            imageView.SetScaleType(ImageView.ScaleType.FitXy);

            string uri = Intent.Extras.GetString("uri") ?? "";
            if (string.IsNullOrEmpty(uri))
            {
                CActivityManager.GetInstence().FinishSingleActivity(this);
            }
            else
            {
                try
                {
                    //using (var tmp = Android.Net.Uri.Parse("file://" + uri))
                    //{
                    //    imageView.SetImageURI(tmp);
                    //}

                    bm = BitmapFactory.DecodeFile(uri);
                    imageView.SetImageBitmap(bm);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("imageView Exception: " + e.Message);
                }
            }

            CActivityManager.GetInstence().AddActivity(this);
        }

        protected override void OnDestroy()
        {
            if (bm != null)
            {
                if (!bm.IsRecycled)
                {
                    bm.Recycle();
                }
                bm.Dispose();
                bm = null;
            }

            if (imageView != null)
            {
                imageView.SetImageBitmap(null);
                imageView.DestroyDrawingCache();
                imageView.Drawable.Dispose();
                imageView.Dispose();
                imageView = null;
            }
            
            CActivityManager.GetInstence().FinishSingleActivity(this);
            base.OnDestroy();
        }
    }
}