using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Media;

using AdSubApp.Util;

namespace AdSubApp.SrcActivity
{
    [Activity(Label = "VideoViewActivity")]
    public class VideoViewActivity : Activity, ISurfaceHolderCallback
    {
        private VideoView videoView = null;

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
            SetContentView(Resource.Layout.VideoView);

            if (videoView == null)
            {
                videoView = FindViewById<VideoView>(Resource.Id.videoView);
            }

            if (holder == null)
            {
                holder = videoView.Holder;
            }
            holder.AddCallback(this);

            if (mediaPlayer == null)
            {
                mediaPlayer = new MediaPlayer();
            }
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

            CActivityManager.GetInstence().AddActivity(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StopVideoPlayer();

            if (mediaPlayer != null)
            {
                mediaPlayer.SetDisplay(null);
                mediaPlayer.Release();
                mediaPlayer.Dispose();
                mediaPlayer = null;
            }

            if (holder != null)
            {
                holder.Dispose();
                holder = null;
            }

            if (videoView != null)
            {
                videoView.Holder.Dispose();
                videoView.DestroyDrawingCache();
                videoView.Dispose();
                videoView = null;
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
            Settings.semVideoCompleted.Release();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnResume()
        {
            if (!mediaPlayer.IsPlaying)
            {
                // 继续播放
                mediaPlayer.Start();
            }
            base.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            mediaPlayer.Pause();
        }

        protected override void OnStop()
        {
            base.OnStop();
            StopVideoPlayer();
        }

        protected override void OnRestart()
        {
            base.OnRestart();
            CActivityManager.GetInstence().FinishSingleActivity(this);
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

        MediaPlayer mediaPlayer = null;

        private void MediaPlayer_Prepared(object sender, EventArgs e)
        {
            Console.WriteLine("MediaPlayer_Prepared");
        }

        private void MediaPlayer_Completion(object sender, EventArgs e)
        {
            Console.WriteLine("MediaPlayer_Completion");
            CActivityManager.GetInstence().FinishSingleActivity(this);
        }

        ISurfaceHolder holder = null;

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

