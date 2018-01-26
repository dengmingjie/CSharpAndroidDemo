using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CommonBasicControls.Util;
using Android.Graphics;

namespace CommonBasicControls.SrcActivity
{
    [Activity(Label = "Demo2003ImageView")]
    public class Demo2003ImageView : Activity
    {
        private ImageView imageView;

        private Bitmap bm = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CActivityManager.GetInstence().AddActivity(this);

            // 隐藏标题栏  
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            // 隐藏状态栏  
            this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // 将背景设置为空
            this.Window.SetBackgroundDrawable(null);

            // Create your application here
            SetContentView(Resource.Layout.Demo2003ImageView);

            imageView = FindViewById<ImageView>(Resource.Id.imageView1);
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
            imageView.SetImageBitmap(null);
            imageView.DestroyDrawingCache();
            imageView.Drawable.Dispose();
            imageView.Dispose();
            imageView = null;
            CActivityManager.GetInstence().FinishSingleActivity(this);
            base.OnDestroy();
        }
    }
}