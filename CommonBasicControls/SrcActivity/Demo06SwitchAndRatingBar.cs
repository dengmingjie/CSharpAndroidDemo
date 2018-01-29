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
    [Activity(Label = "Demo06SwitchAndRatingBar")]
    public class Demo06SwitchAndRatingBar : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo06SwitchAndRatingBar);

            Button button = FindViewById<Button>(Resource.Id.button);
            button.Click += delegate
            {
                Toast.MakeText(this, "Beep Boop", ToastLength.Short).Show();
            };

            var editText = FindViewById<EditText>(Resource.Id.edittext);
            //---------------------------------------------------------
            //技巧：按+=后，连续按两次<Tab>键，就会自动生成事件处理程序
            //---------------------------------------------------------
            editText.KeyPress += EditText_KeyPress;

            var checkbox = FindViewById<CheckBox>(Resource.Id.checkbox);
            checkbox.Click += delegate
            {
                if (checkbox.Checked)
                    Toast.MakeText(this, "Selected", ToastLength.Short).Show();
                else
                    Toast.MakeText(this, "Not selected", ToastLength.Short).Show();
            };

            var radioRed = FindViewById<RadioButton>(Resource.Id.radio_red);
            var radioBlue = FindViewById<RadioButton>(Resource.Id.radio_blue);
            radioRed.Click += Radio_Click;
            radioBlue.Click += Radio_Click;

            Switch toggleButton = FindViewById<Switch>(Resource.Id.togglebutton);
            toggleButton.Click += (o, e) => 
            {
                if (toggleButton.Checked)
                    Toast.MakeText(this, "Checked", ToastLength.Short).Show();
                else
                    Toast.MakeText(this, "Not checked", ToastLength.Short).Show();
            };

            RatingBar ratingbar = FindViewById<RatingBar>(Resource.Id.ratingbar);
            ratingbar.RatingBarChange += (o, e) => 
            {
                Toast.MakeText(this, "New Rating: " + ratingbar.Rating.ToString(), ToastLength.Short).Show();
            };

            CActivityManager.GetInstence().AddActivity(this);
        }

        private void EditText_KeyPress(object sender, View.KeyEventArgs e)
        {
            var editText = sender as EditText;
            e.Handled = false;
            if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
            {
                Toast.MakeText(this, editText.Text, ToastLength.Short).Show();
                e.Handled = true;
            }
        }

        private void Radio_Click(object sender, EventArgs e)
        {
            RadioButton r = sender as RadioButton;
            Toast.MakeText(this, r.Text, ToastLength.Short).Show();
        }
    }
}

