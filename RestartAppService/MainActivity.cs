using System;

using Android.App;
using Android.OS;
using Android.Content;
using Android.Views;

using RestartAppService.Util;

namespace RestartAppService
{
    [Activity(Label = "RestartAppService", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // 隐藏标题栏  
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            // 隐藏状态栏  
            this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // 将背景设置为空
            this.Window.SetBackgroundDrawable(null);
            
            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // 守护线程
            if (DaemonThread == null)
            {
                DaemonThread = new Java.Lang.Thread(() =>
                {
                    bIsDaemonThreadOn = true;
                    while (bIsDaemonThreadOn)
                    {
                        System.Threading.Thread.Sleep(Settings.DaemonRate);
                        Daemon();
                    }
                });
            }
            if (!DaemonThread.IsAlive)
            {
                DaemonThread.Start();
            }

            // 后台运行
            this.MoveTaskToBack(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // 释放资源
            bIsDaemonThreadOn = false;
            if (DaemonThread != null)
            {
                DaemonThread.Dispose();
                DaemonThread = null;
            }

            if (am != null)
            {
                am.Dispose();
                am = null;
            }

            RestartApp(this.PackageName);  // 重启自己
        }

        /// <summary>
        /// 判断时间点是否在范围内
        /// </summary>
        /// <param name="point">时间点</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">终止时间</param>
        public bool IsTimePointIn(DateTime point, string startTime = "01:20", string endTime = "6:40")
        {
            TimeSpan ts = point.TimeOfDay;
            TimeSpan timeSpan1 = DateTime.Parse(startTime).TimeOfDay;
            TimeSpan timeSpan2 = DateTime.Parse(endTime).TimeOfDay;

            if (ts > timeSpan1 && ts < timeSpan2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 守护线程
        /// </summary>
        public Java.Lang.Thread DaemonThread = null;

        /// <summary>
        /// 守护线程控制
        /// </summary>
        public bool bIsDaemonThreadOn = false;

        /// <summary>
        /// 系统全局Activity管理器
        /// </summary>
        public ActivityManager am = null;

        /// <summary>
        /// 守护
        /// </summary>
        public void Daemon()
        {
            // 获取系统全局Activity管理器
            if (am == null)
            {
                am = this.GetSystemService(Context.ActivityService) as ActivityManager;
            }
            // 获取当前运行中进程
            var runningAppProcesses = am.RunningAppProcesses;
            bool bIsRunning = false;  // 被守护程序是否在运行中
            foreach (var one in runningAppProcesses)
            {
                var importance = one.Importance;  // 当前状态（前/后台）
                string processName = one.ProcessName;
                if (processName == this.PackageName && importance == Importance.Foreground)
                {
                    // 后台运行
                    this.MoveTaskToBack(true);
                }
                else if (processName == Settings.PeerName)
                {
                    // 运行中
                    bIsRunning = true;
                    break;
                }
            }
            if (!bIsRunning)
            {
                //if (IsTimePointIn(DateTime.UtcNow))
                if (IsTimePointIn(DateTime.Now))
                {
                    // 01:30~6:30 AdSubApp会关闭
                    // 不重启，sleep 10min
                    System.Threading.Thread.Sleep(1000 * 60 * 10);
                }
                else
                {
                    // 未运行，则重启之！
                    RestartApp(Settings.PeerName);
                }
            }
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        /// <param name="packageName">APP程序包名</param>
        /// <param name="triggerAtMillis">延时启动时间（默认3000ms）</param>
        public void RestartApp(string packageName, long triggerAtMillis = 3000)
        {
            int requestCode = 123456 + System.DateTime.Now.Millisecond;
            using (Intent iStartActivity = PackageManager.GetLaunchIntentForPackage(packageName))
            {
                iStartActivity.AddCategory(Intent.CategoryLauncher);
                iStartActivity.SetAction(Intent.ActionMain);
                iStartActivity.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                iStartActivity.PutExtra("mode", "restart");
                if (triggerAtMillis > 0)
                {
                    using (PendingIntent operation = PendingIntent.GetActivity(this, requestCode, iStartActivity, PendingIntentFlags.OneShot))
                    using (AlarmManager am = GetSystemService(Context.AlarmService) as AlarmManager)
                    {
                        am.Set(AlarmType.RtcWakeup, triggerAtMillis, operation);
                    }
                }
                else
                {
                    StartActivity(iStartActivity);
                }
            }
        }
    }
}

