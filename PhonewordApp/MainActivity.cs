using System;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Runtime;
using Android.Views;
using System.Collections.Generic;

namespace PhonewordApp
{
    [Activity(Label = "PhonewordApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        static readonly List<string> phoneNumbers = new List<string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var phoneNumberText = FindViewById<EditText>(Resource.Id.PhoneNumberText);
            var buttonTranslate = FindViewById<Button>(Resource.Id.buttonTranslate);
            var buttonCall = FindViewById<Button>(Resource.Id.buttonCall);
            buttonCall.Enabled = false;  // 禁用【拨号】按钮

            string translatedNumber = string.Empty;
            buttonTranslate.Click += (s, e) =>
            {
                translatedNumber = PhonewordTranslator.ToNumber(phoneNumberText.Text);
                if (string.IsNullOrWhiteSpace(translatedNumber))
                {
                    buttonCall.Text = "拨号";
                    buttonCall.Enabled = false;
                }
                else
                {
                    buttonCall.Text = "播出号码：" + translatedNumber + "，单击确认！";
                    buttonCall.Enabled = true;
                }
            };

            var buttonCallHistory = FindViewById<Button>(Resource.Id.buttonCallHistory);
            buttonCallHistory.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(CallHistoryActivity));
                intent.PutStringArrayListExtra("phone_numbers", phoneNumbers);
                StartActivity(intent);
            };

            buttonCall.Click += (s, e) =>
            {
                phoneNumbers.Add(translatedNumber);
                buttonCallHistory.Enabled = true;
                // 当单击【拨号】时，尝试拨号
                var callDialog = new AlertDialog.Builder(this);
                callDialog.SetMessage("电话：" + translatedNumber + "，拨号吗？");
                callDialog.SetNeutralButton("拨号", delegate
                {
                    // Create intent to dial phone
                    var callIntent = new Intent(Intent.ActionCall);
                    callIntent.SetData(Android.Net.Uri.Parse("tel:" + translatedNumber));
                    StartActivity(callIntent);
                });
                callDialog.SetNegativeButton("取消", delegate { });
                callDialog.Show();
            };
        }
    }
}

