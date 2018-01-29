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
    [Activity(Label = "Demo09SeekBar")]
    public class Demo09SeekBar : Activity, SeekBar.IOnSeekBarChangeListener
    {
        // 注意：
        // 用法1（常用）不要求实现SeekBar.IOnSeekBarChangeListener接口
        // 用法2（不常用）要求必须实现SeekBar.IOnSeekBarChangeListener接口

        SeekBar seekBar1, seekBar2;

        TextView textView1, textView2, textView3;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo09SeekBar);

            seekBar1 = FindViewById<SeekBar>(Resource.Id.seekBar1);
            textView1 = FindViewById<TextView>(Resource.Id.textView1);

            seekBar2 = FindViewById<SeekBar>(Resource.Id.seekBar2);
            textView2 = FindViewById<TextView>(Resource.Id.textView2);
            textView3 = FindViewById<TextView>(Resource.Id.textView3);

            // 用法1：（常用）
            seekBar1.ProgressChanged += (s, e) => 
            {
                if (e.FromUser)
                {
                    textView1.Text = string.Format("seekBar1 值为 {0}", e.Progress);
                }
            };

            // 用法2：（用接口实现）
            seekBar2.SetOnSeekBarChangeListener(this);

            CActivityManager.GetInstence().AddActivity(this);
        }

        #region 用法2需要实现的SeekBar.IOnSeekBarChangeListener接口

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            if (fromUser)
            {
                textView2.Text = string.Format("seekBar2 值已调整到 {0}", seekBar.Progress);
            }
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            textView3.Text = "用户开始拖动滑动条。";
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            textView3.Text = "用户停止拖动滑动条。";
        }

        #endregion
    }
}