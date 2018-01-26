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
using Android.Net;
using Android.Media;
using CommonBasicControls.Util;

namespace CommonBasicControls.SrcActivity
{
    [Activity(Label = "Demo2002VideoView")]
    public class Demo2002VideoView : Activity, ISurfaceHolderCallback
    {
        private VideoView videoView;
        
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
            SetContentView(Resource.Layout.Demo2002VideoView);

            videoView = FindViewById<VideoView>(Resource.Id.videoView);

            holder = videoView.Holder;
            holder.AddCallback(this);

            mediaPlayer = new MediaPlayer();
            mediaPlayer.Prepared += MediaPlayer_Prepared;
            mediaPlayer.Completion += MediaPlayer_Completion;

            string uri = Intent.Extras.GetString("uri") ?? "";
            if (string.IsNullOrEmpty(uri))
            {
                CActivityManager.GetInstence().FinishSingleActivity(this);
            }
            else
            {
                bool bIsLoop = false;
                if (!string.IsNullOrEmpty(Intent.Extras.GetString("bIsLoop")) && Intent.Extras.GetString("bIsLoop") == "true")
                {
                    bIsLoop = true;
                }
                StartVideoPlayer(uri, bIsLoop);
            }
        }

        protected override void OnDestroy()
        {
            StopVideoPlayer();
            mediaPlayer.SetDisplay(null);
            mediaPlayer.Release();
            mediaPlayer.Dispose();
            mediaPlayer = null;
            holder.Dispose();
            holder = null;
            videoView.Holder.Dispose();
            videoView.DestroyDrawingCache();
            videoView.Dispose();
            videoView = null;
            CActivityManager.GetInstence().FinishSingleActivity(this);
            Settings.semVideoCompleted.Release();
            base.OnDestroy();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnPause()
        {
            mediaPlayer.Pause();
            base.OnPause();
        }

        protected override void OnStop()
        {
            StopVideoPlayer();
            base.OnStop();
        }

        protected override void OnRestart()
        {
            CActivityManager.GetInstence().FinishSingleActivity(this);
            base.OnRestart();
        }

        /// <summary>
        /// 开启视频播放器
        /// </summary>
        /// <param name="uri">统一资源标志符</param>
        /// <param name="bIsLoop">是否循环</param>
        public void StartVideoPlayer(string uri, bool bIsLoop = false)
        {
            StopVideoPlayer();

            using (var tmp = Android.Net.Uri.Parse("file://" + uri))
            {
                mediaPlayer.SetDataSource(this, tmp);
            }
            if (bIsLoop)
            {
                mediaPlayer.Looping = true;
            }
            mediaPlayer.Prepare();
            mediaPlayer.Start();
        }

        /// <summary>
        /// 关闭视频播放器
        /// </summary>
        public bool StopVideoPlayer()
        {
            if (mediaPlayer.IsPlaying)
            {
                mediaPlayer.Stop();
            }

            return true;
        }

        MediaPlayer mediaPlayer;

        private void MediaPlayer_Prepared(object sender, EventArgs e)
        {
            Console.WriteLine("MediaPlayer_Prepared");
        }

        private void MediaPlayer_Completion(object sender, EventArgs e)
        {
            Console.WriteLine("MediaPlayer_Completion");
            CActivityManager.GetInstence().FinishSingleActivity(this);
        }

        ISurfaceHolder holder;

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceCreated");
            mediaPlayer.SetDisplay(holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            Console.WriteLine("SurfaceDestroyed");
            StopVideoPlayer();
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int w, int h)
        {
            Console.WriteLine("SurfaceChanged");
        }
    }
}

