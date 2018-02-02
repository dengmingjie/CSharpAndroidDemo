using System.Text;
using System.Threading;

using Android.OS;
using Android.App;

using Java.IO;
using Java.Net;
using Java.Util.Logging;

using Org.Json;

namespace AdSubApp.Util
{
    public class Settings
    {
        ~Settings()
        {
            semVideoCompleted.Dispose();
            semVideoCompleted = null;
            HeartBeatParams.Dispose();
            HeartBeatParams = null;
            url.Dispose();
            url = null;
            if (_runtimeLog != null)
            {
                var handlers = _runtimeLog.GetHandlers();
                foreach (var one in handlers)
                {
                    one.Flush();
                    one.Close();
                }
            }
        }

        /// <summary>
        /// 软件版本
        /// </summary>
        public static string Version = "0.0";

        /// <summary>
        /// 运行时日志
        /// </summary>
        private static Logger _runtimeLog = null;
        public static Logger RuntimeLog
        {
            get
            {
                if (_runtimeLog == null)
                {
                    // 创建/获取日志对象
                    _runtimeLog = Logger.GetLogger(Application.Context.GetString(Resource.String.ApplicationName));
                    _runtimeLog.Level = Level.Info;  // 设置日志等级

                    // 设置控制台输出
                    using (ConsoleHandler consoleHandler = new ConsoleHandler())
                    {
                        consoleHandler.Level = Level.All;  // 设置日志等级
                        _runtimeLog.AddHandler(consoleHandler);
                    }

                    // 设置文件输出
                    using (FileHandler fileHandler = new FileHandler(AppPath + "/runtime.log", true))
                    {
                        fileHandler.Formatter = new LogFormatter();  // 设置日志格式
                        fileHandler.Level = Level.Info;  // 设置日志等级
                        _runtimeLog.AddHandler(fileHandler);
                    }
                }
                return _runtimeLog;
            }
            set
            {
                _runtimeLog = value;
            }
        }

        /// <summary>
        /// 当前进程navtive堆中总内存大小
        /// </summary>
        public static long lNativeHeapSize = 0;

        /// <summary>
        /// 视频结束信号量
        /// </summary>
        public static Semaphore semVideoCompleted = new Semaphore(0, 1, "semVideoCompleted");

        /// <summary>
        /// 服务器地址（http://IP:PORT）
        /// </summary>
        public static string ServersUrl = "http://39.106.27.51:9099";

        /// <summary>
        /// 服务器URL
        /// </summary>
        public static URL url = new URL(Settings.ServersUrl);

        //public static string wsUri = "wss://nc.1015bar.com/websocket/websocketTest";
        public static string wsUri = "ws://39.106.27.51:7082/nc-web/websocketTest";

        /// <summary>
        /// 心跳频率（默认30000ms）
        /// </summary>
        public static int HeartBeatRate = 30000;

        /// <summary>
        /// 心跳参数
        /// </summary>
        public static JSONObject HeartBeatParams = new JSONObject();

        /// <summary>
        /// 被守护程序名
        /// </summary>
        public static string PeerName = "RestartAppService.RestartAppService";

        /// <summary>
        /// APP目录
        /// </summary>
        private static string _appPath = null;
        public static string AppPath
        {
            get
            {
                if (_appPath == null)
                {
                    // 判断SD卡是否存在
                    if (Environment.ExternalStorageState == Environment.MediaMounted)
                    {
                        // 设置APP目录
                        _appPath = Environment.ExternalStorageDirectory.AbsolutePath + "/" + Application.Context.GetString(Resource.String.ApplicationName);
                        using (File file = new File(_appPath))
                        {
                            if (!file.Exists())
                            {
                                file.Mkdirs();
                            }
                        }
                    }
                    else  // SD卡不存在
                    {
                        // 设置为当前目录
                        _appPath = "./" + Application.Context.GetString(Resource.String.ApplicationName);
                    }
                }
                return _appPath;
            }
            set
            {
                _appPath = value;
            }
        }

        /// <summary>
        /// 系统全局Activity管理器
        /// </summary>
        public static ActivityManager am = null;

        /// <summary>
        /// 系统全局Notification管理器
        /// </summary>
        public static NotificationManager nm = null;

        /// <summary>
        /// cpuId
        /// </summary>
        private static string _cpuId = null;
        public static string CpuId
        {
            get
            {
                if (_cpuId == null)
                {
                    using (File file = new File("/dev/box1015_cpuid"))
                    {
                        if (file.Exists())
                        {
                            byte[] buffer = new byte[12];
                            using (FileInputStream fis = new FileInputStream(file))
                            {
                                fis.Read(buffer, 0, buffer.Length);
                            }
                            StringBuilder ret = new StringBuilder();
                            foreach (byte b in buffer)
                            {
                                //{0:X2} 大写
                                ret.AppendFormat("{0:x2}", b);
                            }
                            _cpuId = ret.ToString();
                            buffer = null;
                        }
                    }
                }
                return _cpuId;
            }
            set
            {
                _cpuId = value;
            }
        }
    }
}