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
    [Activity(Label = "Demo18FrameLayout")]
    public class Demo18FrameLayout : Activity
    {
        List<ImageView> images = new List<ImageView>();

        int current;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo18FrameLayout);

            images.Add(FindViewById<ImageView>(Resource.Id.image1));
            images.Add(FindViewById<ImageView>(Resource.Id.image2));
            images.Add(FindViewById<ImageView>(Resource.Id.image3));
            current = images.Count - 1;
            FindViewById<Button>(Resource.Id.btnNext).Click += delegate
            {
                images[current].Visibility = ViewStates.Invisible;
                current--;
                if (current < 0) current = images.Count - 1;
                images[current].Visibility = ViewStates.Visible;
            };
        }
    }
}