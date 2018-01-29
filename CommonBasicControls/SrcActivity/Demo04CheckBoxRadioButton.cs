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
    [Activity(Label = "Demo04CheckBoxRadioButton")]
    public class Demo04CheckBoxRadioButton : Activity
    {
        CheckBox red, green;
        RadioButton nan, nv;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo04CheckBoxRadioButton);

            red = FindViewById<CheckBox>(Resource.Id.checkBoxRed);
            green = FindViewById<CheckBox>(Resource.Id.checkBoxGreen);
            nan = FindViewById<RadioButton>(Resource.Id.radioButtonMale);
            nv = FindViewById<RadioButton>(Resource.Id.radioButtonFamale);

            var button = FindViewById<Button>(Resource.Id.btnOK);
            button.Click += Button_Click;

            CActivityManager.GetInstence().AddActivity(this);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            string s1 = "性别：" + (nan.Checked ? "男" : "女");
            string s2 = "喜欢的颜色：";
            if (red.Checked) s2 += red.Text;
            if (green.Checked) s2 += " " + green.Text;
            Toast.MakeText(this, string.Format("{0}\n{1}", s1, s2), ToastLength.Long).Show();
        }
    }
}