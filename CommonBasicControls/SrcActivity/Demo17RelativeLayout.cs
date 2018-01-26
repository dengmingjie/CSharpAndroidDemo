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
    [Activity(Label = "Demo17RelativeLayout")]
    public class Demo17RelativeLayout : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo17RelativeLayout);

            FindViewById<Button>(Resource.Id.ok).Click += delegate
            {
                Toast.MakeText(this, "你单击了[确定]", ToastLength.Long).Show();
            };
            FindViewById<Button>(Resource.Id.cancel).Click += delegate
            {
                Toast.MakeText(this, "你单击了[取消]", ToastLength.Long).Show();
            };
        }
    }
}