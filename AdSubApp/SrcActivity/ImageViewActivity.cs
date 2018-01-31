using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Views.Animations;

using AdSubApp.Util;

namespace AdSubApp.SrcActivity
{
    [Activity(Label = "ImageViewActivity")]
    public class ImageViewActivity : Activity
    {
        private ImageView imageView = null;

        private Bitmap bm = null;

        private AlphaAnimation animation = null;

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
                    Settings.RuntimeLog.Severe("imageView Exception: " + e.ToString());
                }
            }

            CActivityManager.GetInstence().AddActivity(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ClearAnimation();

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

            if (Intent != null)
            {
                if (Intent.Extras != null)
                {
                    Intent.Extras.Dispose();
                }
                Intent.Dispose();
                Intent = null;
            }

            CActivityManager.GetInstence().FinishSingleActivity(this);
        }

        protected override void OnStart()
        {
            base.OnStart();

            // 创建淡入动画
            animation = new AlphaAnimation(0.0f, 1.0f);

            // 设置动画的执行时间  
            animation.Duration = 1500;

            // 使用StartAnimation方法执行动画  
            imageView.StartAnimation(animation);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        /// <summary>
        /// 释放动画资源
        /// </summary>
        private void ClearAnimation()
        {
            imageView.ClearAnimation();
            if (imageView.Animation != null)
            {
                imageView.Animation.Dispose();
                imageView.Animation = null;
            }
            if (animation != null)
            {
                animation.Dispose();
                animation = null;
            }
        }
    }
}