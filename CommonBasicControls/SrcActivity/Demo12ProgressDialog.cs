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

namespace CommonBasicControls.SrcActivity
{
    [Activity(Label = "Demo12ProgressDialog")]
    public class Demo12ProgressDialog : Activity
    {
        private int progress;

        private ProgressDialog dialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo12ProgressDialog);

            var btnProgress = FindViewById<Button>(Resource.Id.btnPprogress);
            btnProgress.Click += BtnProgress_Click;

            var btnCircle = FindViewById<Button>(Resource.Id.btnCircle);
            btnCircle.Click += BtnCircle_Click;
        }

        private void BtnProgress_Click(object sender, EventArgs e)
        {
            progress = 0;
            dialog = new ProgressDialog(this)
            {
                Progress = progress,
                Indeterminate = false,
            };
            dialog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            dialog.SetTitle("条形进度条示例");
            dialog.SetMessage("正在下载……");
            dialog.SetIcon(Resource.Drawable.Icon);
            dialog.SetCancelable(true);
            dialog.SetButton("取消", delegate
            {
                Toast.MakeText(this, "已取消下载！", ToastLength.Long).Show();
            });
            dialog.Show();
            RunTask(showProgress: false);
        }

        private void BtnCircle_Click(object sender, EventArgs e)
        {
            progress = 0;
            dialog = new ProgressDialog(this)
            {
                Progress = progress,
                Indeterminate = false
            };
            dialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            dialog.SetTitle("环转圆形进度条示例");
            dialog.SetMessage("正在下载……");
            dialog.SetIcon(Resource.Drawable.Icon);
            dialog.SetCancelable(true);
            dialog.SetButton("取消", delegate
            {
                Toast.MakeText(this, "已取消下载！", ToastLength.Long).Show();
            });
            dialog.Show();
            RunTask(showProgress: true);
        }

        // 模拟长时间执行的任务
        private void RunTask(bool showProgress)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                while (progress < 100)
                {
                    dialog.Progress = progress++;
                    if (showProgress)
                    {
                        RunOnUiThread(() =>
                        {
                            dialog.SetMessage("正在下载……" + progress + "%");
                        });
                    }
                    System.Threading.Thread.Sleep(100);
                }
                dialog.Cancel();
            });
        }
    }
}