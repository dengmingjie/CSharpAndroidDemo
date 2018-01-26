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
    [Activity(Label = "Demo11AlertDialog")]
    public class Demo11AlertDialog : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Demo11AlertDialog);

            var btn1 = FindViewById<Button>(Resource.Id.demo1);
            btn1.Click += delegate
            {
                // 基本用法1（一个按钮）
                var dialog = new AlertDialog.Builder(this)
                    .SetTitle("用法1")
                    .SetMessage("这是用法1示例的警告信息！")
                    .SetIcon(Resource.Drawable.Icon)
                    .SetNeutralButton("确定", (s1, e1) =>
                    {
                        Toast.MakeText(this, "OK", ToastLength.Long).Show();
                    });
                dialog.Create().Show();
            };

            var btn2 = FindViewById<Button>(Resource.Id.demo2);
            btn2.Click += delegate
            {
                // 基本用法2（两个按钮）
                var dialog = new AlertDialog.Builder(this)
                    .SetTitle("用法2")
                    .SetMessage("如果继续，将丢失所有未保存的信息，继续吗？")
                    .SetIcon(Resource.Drawable.Icon)
                    .SetNegativeButton("是", (sender, args) =>
                    {
                        var btnClicked = (sender as AlertDialog).GetButton(args.Which);
                        Toast.MakeText(this, "你单击了" + btnClicked.Text, ToastLength.Long).Show();
                    })
                    .SetPositiveButton("否", (sender, args) =>
                    {
                        var btnClicked = (sender as AlertDialog).GetButton(args.Which);
                        Toast.MakeText(this, "你单击了" + btnClicked.Text, ToastLength.Long).Show();
                    });
                dialog.Create().Show();
            };

            var btn3 = FindViewById<Button>(Resource.Id.demo3);
            btn3.Click += delegate
            {
                // 基本用法3（三个按钮）
                var dialog = new AlertDialog.Builder(this)
                    .SetTitle("用法3")
                    .SetMessage("你准备采用哪种排列方式？")
                    .SetIcon(Resource.Drawable.Icon)
                    .SetNegativeButton("从小到大", delegate
                    {
                        Toast.MakeText(this, "你单击了[从小到大]", ToastLength.Long).Show();
                    })
                    .SetNeutralButton("从大到小", delegate
                    {
                        Toast.MakeText(this, "你单击了[从大到小]", ToastLength.Long).Show();
                    })
                    .SetPositiveButton("原始顺序", delegate
                    {
                        Toast.MakeText(this, "你单击了[原始顺序]", ToastLength.Long).Show();
                    });
                dialog.Create().Show();
            };

            var btn4 = FindViewById<Button>(Resource.Id.demo4);
            btn4.Click += delegate
            {
                // 包含单选按钮的对话框
                string[] colors = { "红色", "绿色", "蓝色", "深红色" };
                int n = 0;
                var dialog = new AlertDialog.Builder(this)
                    .SetTitle("用法4-请选择你喜欢的颜色（单选）：")
                    .SetSingleChoiceItems(colors, 0, (sender, args) =>
                    {
                        n = args.Which;
                    })
                   .SetNeutralButton("确定", (sender, args) =>
                   {
                       Toast.MakeText(this, "你选择了：" + colors[n], ToastLength.Long).Show();
                   });
                dialog.Create().Show();
            };

            var btn5 = FindViewById<Button>(Resource.Id.demo5);
            btn5.Click += delegate
            {
                //包含复选框的对话框
                string[] items = { "足球", "篮球", "乒乓球", "排球" };
                bool[] selectedItems = new bool[items.Length];
                var dialog = new AlertDialog.Builder(this)
                    .SetTitle("用法5-请选择你参加的体育活动（可多选）：")
                    .SetMultiChoiceItems(items, selectedItems, (s, e) =>
                    {
                        if (e.IsChecked) selectedItems[e.Which] = true;
                    })
                   .SetNeutralButton("确定", delegate
                   {
                       List<string> list = new List<string>();
                       for (int i = 0; i < items.Length; i++)
                       {
                           if (selectedItems[i] == true)
                           {
                               list.Add(items[i]);
                           }
                       }
                       Toast.MakeText(this, "你的选择是：" + string.Join("、", list.ToArray()), ToastLength.Long).Show();
                   });
                dialog.Create().Show();
            };
        }
    }
}