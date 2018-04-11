using Android.Content;

namespace AdSubApp.Util
{
    [BroadcastReceiver]
    public class BootBroadcastReceiver : BroadcastReceiver
    {
        // 必须提供无参数的构造函数
        public BootBroadcastReceiver() { }

        public override void OnReceive(Context context, Intent intent)
        {
            //Settings.RuntimeLog.Warning(Intent.ActionBootCompleted);
            if (intent.Action.Equals(Intent.ActionBootCompleted))
            {
                // 立即启动：未完全生效、后台运行
                Intent iStartActivity = new Intent(context, typeof(MainActivity));
                iStartActivity.AddCategory(Intent.CategoryLauncher);
                iStartActivity.SetAction(Intent.ActionMain);
                iStartActivity.AddFlags(ActivityFlags.NewTask | ActivityFlags.ResetTaskIfNeeded);
                iStartActivity.PutExtra("mode", "startup");
                context.StartActivity(iStartActivity);
                //Settings.RuntimeLog.Warning("Ready to startup...");
            }
        }
    }
}