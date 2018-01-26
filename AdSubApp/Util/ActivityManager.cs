using System;
using System.Collections.Generic;

using Android.App;

namespace AdSubApp.Util
{
    public class CActivityManager
    {
        private static CActivityManager _temp = null;

        public static CActivityManager GetInstence()
        {
            if (_temp == null)
            {
                _temp = new CActivityManager();
            }
            return _temp;
        }

        public CActivityManager()
        {

        }

        private List<Activity> activityList = new List<Activity>();

        public void AddActivity(Activity activity)
        {
            if (!activityList.Contains(activity))
            {
                activityList.Add(activity);
            }
        }

        public void FinishAllActivity()
        {
            foreach (Activity activity in activityList)
            {
                activity.Finish();
            }
        }

        public void FinishSingleActivity(Activity activity)
        {
            if (activity != null)
            {
                if (activityList.Contains(activity))
                {
                    activityList.Remove(activity);
                }

                activity.Finish();
                activity = null;
            }
        }

        public void FinishSingleActivityByType(Type t)
        {
            Activity tempActivity = null;

            foreach (Activity activity in activityList)
            {
                if (activity.GetType().Equals(t))
                {
                    tempActivity = activity;
                    break;
                }
            }

            FinishSingleActivity(tempActivity);
        }

        public Activity GetSingleActivityByType(Type t)
        {
            Activity tempActivity = null;

            foreach (Activity activity in activityList)
            {
                if (activity.GetType().Equals(t))
                {
                    tempActivity = activity;
                    break;
                }
            }

            return tempActivity;
        }
    }
}

