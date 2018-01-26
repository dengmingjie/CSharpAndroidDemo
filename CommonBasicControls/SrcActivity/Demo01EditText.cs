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
using Android.Graphics;
using CommonBasicControls.Util;

namespace CommonBasicControls.SrcActivity
{
    [Activity(Label = "Demo01EditText")]
    public class Demo01EditText : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CActivityManager.GetInstence().AddActivity(this);

            // Create your application here
            SetContentView(Resource.Layout.Demo01EditText);

            var txtResult = FindViewById<TextView>(Resource.Id.txtResult);
            txtResult.SetTextColor(Color.Red);
            txtResult.Enabled = false;

            var txt1 = FindViewById<EditText>(Resource.Id.editText1);
            txt1.TextChanged += (s, e) =>
            {
                txtResult.Text = "输入的内容为：" + txt1.Text;
            };

            var txt2 = FindViewById<EditText>(Resource.Id.editText2);
            txt2.TextChanged += (s, e) =>
            {
                txtResult.Text = "输入的内容为：" + txt2.Text;
            };
        }
    }
}

