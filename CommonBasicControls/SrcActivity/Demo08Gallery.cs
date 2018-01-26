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
    [Activity(Label = "Demo08Gallery")]
    public class Demo08Gallery : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CActivityManager.GetInstence().AddActivity(this);

            // Create your application here
            SetContentView(Resource.Layout.Demo08Gallery);

            var g = FindViewById<Gallery>(Resource.Id.gallery);
            g.Adapter = new ImageAdapter(this)
            {
                CurrentWidth = 550,
                CurrentHeight = 550
            };
            g.ItemClick += Gallery_ItemClick;
        }

        private void Gallery_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Toast.MakeText(this, e.Position.ToString(), ToastLength.Short).Show();
        }
    }

    public class ImageAdapter : BaseAdapter
    {
        private Context context;

        private int[] thumbIds = {
            Resource.Drawable.sample_0,
            Resource.Drawable.sample_1,
            Resource.Drawable.sample_2,
            Resource.Drawable.sample_3,
            Resource.Drawable.sample_4,
            Resource.Drawable.sample_5,
            Resource.Drawable.sample_6,
            Resource.Drawable.sample_7
        };

        // 默认值为500（这是C# 6.0新增的功能，仅VS2015可以这样用）
        public int CurrentWidth { get; set; } = 500;
        public int CurrentHeight { get; set; } = 500;

        public ImageAdapter(Context c)
        {
            context = c;
        }

        public override int Count
        {
            get { return thumbIds.Length; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ImageView i = new ImageView(context);
            i.SetImageResource(thumbIds[position]);
            i.LayoutParameters = new Gallery.LayoutParams(500, 500);
            i.SetScaleType(ImageView.ScaleType.FitXy);
            return i;
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }
    }
}