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
    [Activity(Label = "Demo07PopupMenu")]
    public class Demo07PopupMenu : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo07PopupMenu);

            Button btn = FindViewById<Button>(Resource.Id.popupButton);
            btn.Click += (s, arg) =>
            {
                PopupMenu menu = new PopupMenu(this, btn);
                menu.Inflate(Resource.Menu.Demo07PopupMenu);
                menu.MenuItemClick += (sender, args) =>
                {
                    string str = string.Format("你选择了：{0}", args.Item);
                    Toast.MakeText(this, str, ToastLength.Short).Show();
                };
                menu.DismissEvent += (sender, args) =>
                {
                    // 菜单消失后可在此事件中做一些后续处理
                    Toast.MakeText(this, "弹出菜单消失了", ToastLength.Short).Show();
                };
                menu.Show();
            };

            CActivityManager.GetInstence().AddActivity(this);
        }
    }
}