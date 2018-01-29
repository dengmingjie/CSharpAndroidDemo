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
    [Activity(Label = "Demo02Login")]
    public class Demo02Login : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo02Login);

            Button btn = FindViewById<Button>(Resource.Id.buttonLogin);
            btn.Click += Btn_Click;  // 技巧：按+=后，连续按两次<Tab>键

            CActivityManager.GetInstence().AddActivity(this);
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            var userName = FindViewById<EditText>(Resource.Id.editTextUserName);
            var pwd = FindViewById<EditText>(Resource.Id.editTextPwd);
            Toast.MakeText(this, string.Format("用户名：{0}, 密码：{1}", 
                userName.Text, pwd.Text), ToastLength.Long).Show();  // 技巧：按空格
        }
    }
}

