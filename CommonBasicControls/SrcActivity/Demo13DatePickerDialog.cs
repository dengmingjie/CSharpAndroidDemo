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
    [Activity(Label = "Demo13DatePicker")]
    public class Demo13DatePickerDialog : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo13DatePickerDialog);

            var date = DateTime.Now;
            var hour = date.Hour;
            var minute = date.Minute;

            var textDateInfo = FindViewById<TextView>(Resource.Id.textDateInfo);
            var btnDate = FindViewById<Button>(Resource.Id.btnDate);
            btnDate.Click += delegate
            {
                var dialog = new DatePickerDialog(this, (sender, args) =>
                {
                    date = args.Date;
                    textDateInfo.Text = string.Format("选择的日期为：{0:yyyy-MM-dd}", date);
                },
                date.Year,
                date.Month - 1,  // Andoid的月份从0开始计数，所以要减1
                date.Day);
                dialog.Show();
            };

            var textTimeInfo = FindViewById<TextView>(Resource.Id.textTimeInfo);
            var btnTime = FindViewById<Button>(Resource.Id.btnTime);
            btnTime.Click += delegate
            {
                var dialog = new TimePickerDialog(this, (sender, args) =>
                {
                    hour = args.HourOfDay;
                    minute = args.Minute;
                    textTimeInfo.Text = string.Format("选择的时间为：{0:00}:{1:00}", hour, minute);
                }, hour, minute, true);  // true:24小时制, false:12小时制
                dialog.Show();
            };
        }
    }
}