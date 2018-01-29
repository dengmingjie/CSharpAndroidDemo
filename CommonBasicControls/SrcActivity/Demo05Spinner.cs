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
    [Activity(Label = "Demo05Spinner")]
    public class Demo05Spinner : Activity
    {
        private Spinner spinner;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo05Spinner);

            var adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.CourseItems, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            spinner = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner.Adapter = adapter;
            spinner.ItemSelected += Spinner_ItemSelected;

            var btn = FindViewById<Button>(Resource.Id.buttonOK);
            btn.Click += delegate
            {
                string s = spinner.SelectedItem.ToString();
                Toast.MakeText(this, s, ToastLength.Long).Show();
            };

            CActivityManager.GetInstence().AddActivity(this);
        }

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string s = string.Format("所选课程为：{0}", spinner.GetItemAtPosition(e.Position));
            Toast.MakeText(this, s, ToastLength.Long).Show();
        }
    }
}