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

namespace CommonBasicControls.SrcActivity
{
    [Activity(Label = "Demo10Toast")]
    public class Demo10Toast : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CActivityManager.GetInstence().AddActivity(this);

            // Create your application here
            SetContentView(Resource.Layout.Demo10Toast);

            var btnDefault = FindViewById<Button>(Resource.Id.btnDefault);
            btnDefault.Click += Button_Click;

            var btnPhoto = FindViewById<Button>(Resource.Id.btnPhoto);
            btnPhoto.Click += Button_Click;

            var btnPosition = FindViewById<Button>(Resource.Id.btnPosition);
            btnPosition.Click += Button_Click;

            var btnCustom = FindViewById<Button>(Resource.Id.btnCustom);
            btnCustom.Click += Button_Click;

            var btnThread = FindViewById<Button>(Resource.Id.btnThread);
            btnThread.Click += Button_Click;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Id)
            {
                case Resource.Id.btnDefault:
                    Toast.MakeText(this, "这是默认的位置和样式", ToastLength.Short).Show();
                    break;
                case Resource.Id.btnPhoto:
                    {
                        var toast = Toast.MakeText(this, "这是带图片的样式（默认位置）", ToastLength.Short);
                        LinearLayout toastView = (LinearLayout)toast.View;
                        ImageView imageCodeProject = new ImageView(this);
                        imageCodeProject.SetImageResource(Resource.Drawable.Icon);
                        toastView.AddView(imageCodeProject, 0);
                        toast.Show();
                    }
                    break;
                case Resource.Id.btnPosition:
                    {
                        var toast = Toast.MakeText(this, "这是自定义的位置（默认样式）", ToastLength.Long);
                        // 从中心位置向上偏移300
                        toast.SetGravity(GravityFlags.Center, 0, -300);
                        toast.Show();
                    }
                    break;
                case Resource.Id.btnCustom:
                    {
                        var toast = new Toast(this);
                        toast.View = GetCustomView("这是自定义的样式的标题", "这是自定义的样式的提示信息", Resource.Drawable.Icon);
                        // 从中心位置向上偏移300
                        toast.SetGravity(GravityFlags.Center, 0, -300);
                        toast.Duration = ToastLength.Long;
                        toast.Show();
                    }
                    break;
                case Resource.Id.btnThread:
                    // 建议的办法：
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "这是来自其他线程的提示信息", ToastLength.Long).Show();
                    });

                    // 也可以用下面的办法实现（用Handler实现后台线程与UI线程的交互）：
                    //var h = new Handler();
                    //h.Post(() => 
                    //{
                    //    Toast.MakeText(this, "这是来自其他线程的提示信息", ToastLength.Long).Show();
                    //});
                    break;
            }
        }

        /// <summary>
        /// 获取用Toast显示的自定义视图
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="prompt">提示信息</param>
        /// <param name="pictureId">图片资源的ID</param>
        /// <returns>自定义的视图</returns>
        private View GetCustomView(string title, string prompt, int pictureId)
        {
            // 用指定的XML资源文件填充视图的层次结构
            View customView = this.LayoutInflater.Inflate(
                Resource.Layout.Demo10CustomToast, 
                FindViewById<ViewGroup>(Resource.Id.custom));
            
            // 设置标题
            var textViewTitle = customView.FindViewById<TextView>(Resource.Id.title);
            textViewTitle.Text = title;
            
            // 设置显示的图像
            var picture = (ImageView)customView.FindViewById(Resource.Id.picture);
            picture.SetImageResource(pictureId);
            
            // 设置显示的文本内容
            TextView textViewPrompt = customView.FindViewById<TextView>(Resource.Id.prompt);
            textViewPrompt.Text = prompt;

            return customView;
        }
    }
}