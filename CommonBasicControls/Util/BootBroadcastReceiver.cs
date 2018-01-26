using Android.Content;

namespace CommonBasicControls.Util
{
    [BroadcastReceiver]
    public class BootBroadcastReceiver : BroadcastReceiver
    {
        private MainActivity mainActivity;

        // 必须提供无参数的构造函数
        public BootBroadcastReceiver() { }

        public BootBroadcastReceiver(MainActivity bdMapMain)
        {
            mainActivity = bdMapMain;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(Intent.ActionBootCompleted))
            {
                Intent iStartActivity = new Intent(context, typeof(MainActivity));
                iStartActivity.AddCategory(Intent.CategoryLauncher);
                iStartActivity.SetAction(Intent.ActionMain);
                iStartActivity.AddFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);
                iStartActivity.PutExtra("mode", "startup");
                context.StartActivity(iStartActivity);
            }
        }
    }
}