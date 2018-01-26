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
    [Activity(Label = "Demo03MultiResolution")]
    public class Demo03MultiResolution : Activity
    {
        private int photo_index = 0;
        private List<int> photo_ids = new List<int>()
        {
            Resource.Drawable.sample_0, Resource.Drawable.sample_1,
            Resource.Drawable.sample_2, Resource.Drawable.sample_3,
            Resource.Drawable.sample_4, Resource.Drawable.sample_5,
            Resource.Drawable.sample_6, Resource.Drawable.sample_7
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CActivityManager.GetInstence().AddActivity(this);

            // Create your application here
            SetContentView(Resource.Layout.Demo03MultiResolution);

            ShowPhoto(photo_index);

            Button nextButton = FindViewById<Button>(Resource.Id.next_button);
            nextButton.Click += delegate
            {
                photo_index = (photo_index + 1) % photo_ids.Count;
                ShowPhoto(photo_index);
            };
        }

        //---------------------------------------------------------------------------
        // 技巧：
        // 先键入override，然后按空格选择要重写的方法，
        // 而不是直接键入这些方法（所有需要重写的方法都是这样做）
        //----------------------------------------------------------------------------
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("photo_index", photo_index);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle outState)
        {
            photo_index = outState.GetInt("photo_index");
            ShowPhoto(photo_index);
            base.OnRestoreInstanceState(outState);
        }

        private void ShowPhoto(int photoIndex)
        {
            ImageView imageView = FindViewById<ImageView>(Resource.Id.image_view);
            imageView.SetImageResource(photo_ids[photoIndex]);
            TextView statusText = FindViewById<TextView>(Resource.Id.status_text);
            statusText.Text = String.Format("{0}/{1}", photoIndex + 1, photo_ids.Count);
        }
    }
}