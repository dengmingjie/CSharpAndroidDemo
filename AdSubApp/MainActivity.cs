using System;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Views;

using AdSubApp.Util;
using AdSubApp.SrcActivity;

using Org.Json;
using Java.IO;
using Java.Net;
using Java.Util.Logging;

namespace AdSubApp
{
    [Activity(Label = "AdSubApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        /* Called when the activity is first created. */
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // 隐藏标题栏  
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            // 隐藏状态栏  
            this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            // 将背景设置为空
            this.Window.SetBackgroundDrawable(null);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // 判断SD卡是否存在 
            if (Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted)
            {
                // 设置APP目录
                Settings.AppPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + GetString(Resource.String.ApplicationName);
                using (File file = new File(Settings.AppPath))
                {
                    if (!file.Exists())
                    {
                        bool bSuccess = file.Mkdirs();
                        // If failed, do something
                        //this.Finish();
                    }
                }
            }
            else  // SD卡不存在
            {
                // 设置为当前目录
                Settings.AppPath = "./" + GetString(Resource.String.ApplicationName);
            }

            // 初始化运行时日志
            InitRuntimeLog();

            // 获取启动方式
            string mode = null;
            if (Intent.Extras != null)
            {
                mode = Intent.Extras.GetString("mode");
            }
            // 解析启动方式
            if (ParseMode(mode) == 1)
            {
                // 开机自启动：未完全生效，后台运行
                // 故重启！
                //RestartApp();

                //// 重启后亦不生效，自杀后，通过守护进程开启
                //Settings.RuntimeLog.Info("重启后亦不生效，自杀后，通过守护进程开启");
                //Daemon();  // 关闭前，查看守护进程
                //Finish();
                //System.Threading.Thread.Sleep(8);
                //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                //return;
            }

            // 节目控制线程
            if (ProgramControlThread == null)
            {
                ProgramControlThread = new Java.Lang.Thread(() =>
                {
                    bIsProgramControlThreadOn = true;
                    while (bIsProgramControlThreadOn)
                    {
                        System.Threading.Thread.Sleep(2000);
                        ProgramControl();
                    }
                });
            }
            if (!ProgramControlThread.IsAlive)
            {
                ProgramControlThread.Start();
            }

            // 心跳线程
            if (HeartBeatThread == null)
            {
                HeartBeatThread = new Java.Lang.Thread(() =>
                {
                    bIsHeartBeatThreadOn = true;
                    while (bIsHeartBeatThreadOn)
                    {
                        System.Threading.Thread.Sleep(Settings.HeartBeatRate);
                        MemoryCheck();
                        HeartBeat();
                        //if (IsTimePointIn(DateTime.UtcNow))
                        if (IsTimePointIn(DateTime.Now))
                        {
                            // 01:30~6:30 关闭程序
                            Settings.RuntimeLog.Info("01:30~6:30 关闭程序");
                            Daemon();  // 关闭前，查看守护进程
                            Finish();
                            System.Threading.Thread.Sleep(8);
                            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                        }
                    }
                });
            }
            if (!HeartBeatThread.IsAlive)
            {
                HeartBeatThread.Start();
            }

            //// WebSocket线程
            //if (WebSocketThread == null)
            //{
            //    WebSocketThread = new Java.Lang.Thread(() =>
            //    {
            //        WebSocketHandler();
            //    });
            //}
            //if (!WebSocketThread.IsAlive)
            //{
            //    WebSocketThread.Start();
            //}
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            bIsWebSocketHandlerOn = false;
            if (wsClient != null)
            {
                wsClient.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "AdSubApp closed", System.Threading.CancellationToken.None).Wait(1000);
                wsClient.Dispose();
                wsClient = null;
            }
            if (WebSocketThread != null)
            {
                WebSocketThread.Dispose();
                WebSocketThread = null;
            }

            bIsHeartBeatThreadOn = false;
            if (HeartBeatThread != null)
            {
                HeartBeatThread.Dispose();
                HeartBeatThread = null;
            }

            bIsProgramControlThreadOn = false;
            bIsProgramLoop = false;
            if (ProgramControlThread != null)
            {
                ProgramControlThread.Dispose();
                ProgramControlThread = null;
            }

            CActivityManager.GetInstence().FinishAllActivity();
            Settings.RuntimeLog.Info("OnDestroy");
            FreeRuntimeLog();
        }

        /// <summary>
        /// 初始化运行时日志
        /// </summary>
        public void InitRuntimeLog()
        {
            // 创建/获取日志对象
            Settings.RuntimeLog = Logger.GetLogger(GetString(Resource.String.ApplicationName));
            Settings.RuntimeLog.Level = Level.Info;  // 设置日志等级

            // 设置控制台输出
            using (ConsoleHandler consoleHandler = new ConsoleHandler())
            {
                //consoleHandler.Formatter = new LogFormatter();  // 设置日志格式
                consoleHandler.Level = Level.All;  // 设置日志等级
                Settings.RuntimeLog.AddHandler(consoleHandler);
            }

            // 设置文件输出
            using (FileHandler fileHandler = new FileHandler(Settings.AppPath + "/runtime.log"))
            {
                fileHandler.Formatter = new LogFormatter();  // 设置日志格式
                fileHandler.Level = Level.Info;  // 设置日志等级
                Settings.RuntimeLog.AddHandler(fileHandler);
            }

            //Settings.RuntimeLog.Info("RuntimeLog test");
        }

        /// <summary>
        /// 释放运行时日志
        /// </summary>
        public void FreeRuntimeLog()
        {
            if (Settings.RuntimeLog != null)
            {
                var handlers = Settings.RuntimeLog.GetHandlers();
                foreach (var one in handlers)
                {
                    one.Flush();
                    one.Close();
                }
            }
        }

        /// <summary>
        /// 解析启动方式
        /// </summary>
        /// <param name="mode">启动方式</param>
        public int ParseMode(string mode)
        {
            int nMode = 0;
            if (!string.IsNullOrEmpty(mode))
            {
                if (mode == "startup")
                {
                    // 开机启动
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "开机启动", ToastLength.Short).Show();
                    });
                    Settings.RuntimeLog.Info("开机启动");
                    Settings.HeartBeatParams.Put("lastCmd", mode + "_Success");
                    nMode = 1;
                }
                else if (mode == "restart")
                {
                    // 重启应用
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "重启应用", ToastLength.Short).Show();
                    });
                    Settings.RuntimeLog.Info("重启应用");
                    Settings.HeartBeatParams.Put("lastCmd", mode + "_Success");
                    nMode = 2;
                }
                else
                {
                    // 未知的启动方式
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "未知的启动方式", ToastLength.Short).Show();
                    });
                    Settings.RuntimeLog.Info("未知的启动方式");
                    nMode = 3;
                }
            }
            else
            {
                // 手动启动
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "手动启动", ToastLength.Short).Show();
                });
                Settings.RuntimeLog.Info("手动启动");
                Settings.HeartBeatParams.Put("lastCmd", "start_Success");
                nMode = 0;
            }
            return nMode;
        }

        /// <summary>
        /// 判断时间点是否在范围内
        /// </summary>
        /// <param name="point">时间点</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">终止时间</param>
        public bool IsTimePointIn(DateTime point, string startTime = "01:30", string endTime = "6:30")
        {
            TimeSpan ts = point.TimeOfDay;
            TimeSpan timeSpan1 = DateTime.Parse(startTime).TimeOfDay;
            TimeSpan timeSpan2 = DateTime.Parse(endTime).TimeOfDay;

            if (ts > timeSpan1 && ts < timeSpan2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 内存检查
        /// </summary>
        public void MemoryCheck()
        {
            // 3.0(HoneyComb)以上的版本可以通过 largeHeap="true" 来申请更多的堆内存

            // 当前进程navtive堆中总内存大小
            long lNativeHeapSize = Android.OS.Debug.NativeHeapSize;

            // 当前进程navtive堆中已使用内存大小
            long lNativeHeapAllocatedSize = Android.OS.Debug.NativeHeapAllocatedSize;

            // 当前进程navtive堆中剩余内存大小
            long lNativeHeapFreeSize = Android.OS.Debug.NativeHeapFreeSize;

            if (lNativeHeapFreeSize <= 1048576 * 3)  // 3Mb
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "当前进程navtive堆内存不足", ToastLength.Long).Show();
                });
                Settings.RuntimeLog.Warning("当前进程navtive堆内存不足");
                System.GC.Collect();  // 垃圾回收
            }
            else
            {
                if (Settings.lNativeHeapSize != 0 && Settings.lNativeHeapSize != lNativeHeapSize)
                {
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "当前进程navtive堆内存重新分配", ToastLength.Long).Show();
                    });
                    Settings.RuntimeLog.Warning("当前进程navtive堆内存重新分配");
                    System.GC.Collect();  // 垃圾回收
                }
                Settings.lNativeHeapSize = lNativeHeapSize;
            }
        }

        /// <summary>
        /// 守护
        /// </summary>
        public void Daemon()
        {
            // 获取系统全局Activity管理器
            if (Settings.am == null)
            {
                Settings.am = this.GetSystemService(Context.ActivityService) as ActivityManager;
            }
            // 获取当前运行中进程
            var runningAppProcesses = Settings.am.RunningAppProcesses;
            bool bIsRunning = false;  // 被守护程序是否在运行中
            foreach (var one in runningAppProcesses)
            {
                var importance = one.Importance;  // 当前状态（前/后台）
                string processName = one.ProcessName;
                if (processName == Settings.PeerName)
                {
                    // 运行中
                    bIsRunning = true;
                    Settings.RuntimeLog.Info("被守护程序运行中");
                    break;
                }
            }
            if (!bIsRunning)
            {
                // 未运行，则重启之！
                Settings.RuntimeLog.Info("被守护程序未运行，则重启之！");
                RestartApp(Settings.PeerName, 500);
            }
        }

        /// <summary>
        /// 重启应用
        /// </summary>
        /// <param name="packageName">APP程序包名</param>
        /// <param name="triggerAtMillis">延时启动时间（默认3000ms）</param>
        public void RestartApp(string packageName, long triggerAtMillis = 3000)
        {
            int requestCode = 123456 + System.DateTime.Now.Millisecond;
            using (Intent iStartActivity = PackageManager.GetLaunchIntentForPackage(packageName))
            {
                iStartActivity.AddCategory(Intent.CategoryLauncher);
                iStartActivity.SetAction(Intent.ActionMain);
                iStartActivity.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                iStartActivity.PutExtra("mode", "restart");
                if (triggerAtMillis > 0)
                {
                    using (PendingIntent operation = PendingIntent.GetActivity(this, requestCode, iStartActivity, PendingIntentFlags.OneShot))
                    using (AlarmManager am = GetSystemService(Context.AlarmService) as AlarmManager)
                    {
                        am.Set(AlarmType.Rtc, triggerAtMillis, operation);
                    }
                }
                else
                {
                    StartActivity(iStartActivity);
                }
            }
        }

        /// <summary>
        /// 配置文件读写锁
        /// </summary>
        private readonly object ConfigLocker = new object();

        /// <summary>
        /// 读取配置文件
        /// </summary>
        public JSONObject ReadConfig()
        {
            lock (ConfigLocker)
            {
                JSONObject jsonObject = null;

                using (File config = new File(Settings.AppPath + "/" + GetString(Resource.String.configFile)))
                {
                    if (config.Exists())
                    {
                        try
                        {
                            using (FileInputStream fis = new FileInputStream(config))
                            {
                                int length = fis.Available();
                                byte[] jsonBuffer = new byte[length];
                                fis.Read(jsonBuffer);
                                using (var jsonStr = new Java.Lang.String(jsonBuffer))
                                {
                                    jsonObject = new JSONObject((string)jsonStr);
                                }
                                jsonBuffer = null;
                            }
                        }
                        catch (IOException e)
                        {
                            jsonObject = null;
                            System.Console.WriteLine("Parse Config IOException: " + e.Message);
                            Settings.RuntimeLog.Severe("Parse Config IOException: " + e.ToString());
                        }
                    }
                    else
                    {
                        // 配置文件不存在
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "配置文件不存在", ToastLength.Short).Show();
                        });
                        Settings.RuntimeLog.Warning("配置文件不存在");

                        //try
                        //{
                        //    // 写入配置文件
                        //    JSONObject jsonObject = new JSONObject();
                        //    jsonObject.Put("config1", "1");
                        //    jsonObject.Put("timeStamp", "0");
                        //    string defult = jsonObject.ToString();
                        //    var stream = new System.IO.FileStream(config.Path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
                        //    var writer = new OutputStreamWriter(stream);
                        //    writer.Write(defult, 0, defult.Length);
                        //    writer.Flush();
                        //    writer.Close();
                        //}
                        //catch (IOException e)
                        //{
                        //    e.PrintStackTrace();
                        //}
                    }
                }

                return jsonObject;
            }
        }

        /// <summary>
        /// 节目控制线程
        /// </summary>
        public Java.Lang.Thread ProgramControlThread = null;

        /// <summary>
        /// 节目控制线程控制
        /// </summary>
        public bool bIsProgramControlThreadOn = false;

        /// <summary>
        /// 节目循环
        /// </summary>
        public bool bIsProgramLoop = false;

        /// <summary>
        /// 节目控制
        /// </summary>
        public void ProgramControl()
        {
            // 读取配置文件
            JSONObject jsonConfig = ReadConfig();

            if (jsonConfig != null)
            {
                // 正在载入节目
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "正在载入节目...", ToastLength.Long).Show();
                });
                Settings.RuntimeLog.Info("正在载入节目...");
                bIsProgramLoop = true;
            }

            // 节目循环
            using (JSONObject program = jsonConfig.OptJSONObject("program"))
            using (JSONArray resources = program.OptJSONArray("resources"))
            {
                int i = -1, nLength = resources.Length();
                while (bIsProgramLoop)
                {
                    i = (i + 1) % nLength;  // 资源循环
                    CActivityManager.GetInstence().FinishSingleActivityByType(typeof(VideoViewActivity));  // 关闭视频播放
                    CActivityManager.GetInstence().FinishSingleActivityByType(typeof(ImageViewActivity));  // 关闭图片播放
                    if (!bIsProgramLoop)
                    {
                        // 重新载入节目
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "重新载入节目...", ToastLength.Long).Show();
                        });
                        Settings.RuntimeLog.Info("重新载入节目...");
                        break;
                    }
                    System.Threading.Thread.Sleep(50);
                    string name, type, md5, url, duration;
                    using (JSONObject resource = resources.GetJSONObject(i))
                    {
                        name = resource.OptString("name");
                        type = resource.OptString("type");
                        md5 = resource.OptString("md5");
                        url = resource.OptString("url");
                        duration = resource.OptString("duration");
                    }
                    if (type == "video")
                    {
                        // 播放视频
                        RunOnUiThread(() =>
                        {
                            using (Intent intentVideo = new Intent(this, typeof(VideoViewActivity)))
                            {
                                intentVideo.PutExtra("uri", Settings.AppPath + "/" + type + "/" + name);
                                StartActivity(intentVideo);
                            }
                        });
                        Settings.semVideoCompleted.WaitOne();  // 等待视频结束
                    }
                    else if (type == "image")
                    {
                        // 播放图片
                        RunOnUiThread(() =>
                        {
                            using (Intent intentImage = new Intent(this, typeof(SrcActivity.ImageViewActivity)))
                            {
                                intentImage.PutExtra("uri", Settings.AppPath + "/" + type + "/" + name);
                                StartActivity(intentImage);
                            }
                        });

                        int nDuration = int.Parse(duration) + 1;  // 图片播放时间
                        System.Threading.Thread.Sleep(nDuration * 1000);
                    }
                    else
                    {
                        // other type
                    }
                }
            }

            if (jsonConfig != null)
            {
                jsonConfig.Dispose();
                jsonConfig = null;
            }
        }

        /// <summary>
        /// 心跳线程
        /// </summary>
        public Java.Lang.Thread HeartBeatThread = null;

        /// <summary>
        /// 心跳线程控制
        /// </summary>
        public bool bIsHeartBeatThreadOn = false;

        /// <summary>
        /// 心跳请求
        /// </summary>
        public void HeartBeat()
        {
            try
            {
                // 创建HTTP连接
                using (HttpURLConnection httpConn = Settings.url.OpenConnection() as HttpURLConnection)
                {
                    httpConn.RequestMethod = "POST";  // 启用POST方式
                    httpConn.UseCaches = false;  // 不启用缓存
                    httpConn.DoOutput = true;  // 启用输出流
                    httpConn.DoInput = true;  // 启用输入流
                    httpConn.InstanceFollowRedirects = true;  // 启用HTTP重定向
                    //httpConn.SetRequestProperty("Content-Type", "application/x-www-form-urlencoded");  // 设置请求类型
                    httpConn.SetRequestProperty("Content-Type", "application/json");  // 设置请求类型
                    httpConn.ConnectTimeout = 10000;  // 设置超时时间

                    // 获取输出流
                    using (DataOutputStream outStream = new DataOutputStream(httpConn.OutputStream))
                    {
                        // 格式化心跳参数
                        if (!Settings.HeartBeatParams.Has("action"))
                        {
                            Settings.HeartBeatParams.Put("action", "AdSubAppHeartBeat");
                        }
                        if (!Settings.HeartBeatParams.Has("cpuId"))
                        {
                            //Settings.HeartBeatParams.Put("cpuId", Settings.CpuId);
                            Settings.HeartBeatParams.Put("cpuId", "666999");
                        }
                        if (!Settings.HeartBeatParams.Has("version"))
                        {
                            Settings.HeartBeatParams.Put("version", Settings.Version);
                        }
                        outStream.WriteBytes(Settings.HeartBeatParams.ToString().Replace("\r", "").Replace("\n", "").Replace(" ", ""));  // 将数据写入输出流
                        Settings.HeartBeatParams.Remove("lastCmd");
                        Settings.HeartBeatParams.Remove("errMsg");
                        outStream.Flush();  // 立刻输出缓存数据
                    }

                    // 判断是否响应成功
                    if (httpConn.ResponseCode == HttpStatus.Ok)
                    {
                        using (InputStreamReader inStream = new InputStreamReader(httpConn.InputStream))  // 获取输入流
                        using (BufferedReader buffer = new BufferedReader(inStream))  // 获取输入流读取器
                        {
                            string inputLine = null, heartBeatResult = null;
                            while ((inputLine = buffer.ReadLine()) != null)
                            {
                                heartBeatResult += inputLine + "\n";
                            }

                            // 解析心跳返回数据
                            ParseHeartBeatResult(heartBeatResult);
                        }
                    }
                    else
                    {
                        long Code = (long)httpConn.ResponseCode;

                        // HTTP error
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "心跳线程: HTTP error code " + Code, ToastLength.Long).Show();
                        });
                        Settings.RuntimeLog.Info("心跳线程: HTTP error code " + Code);
                    }

                    httpConn.Disconnect();  // 断开HTTP连接
                }
            }
            catch (Exception e)
            {
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "HeartBeat Exception: " + e.Message, ToastLength.Long).Show();
                });
                System.Console.WriteLine("HeartBeat Exception: " + e.Message);
                Settings.RuntimeLog.Severe("HeartBeat Exception: " + e.ToString());
            }
        }

        /// <summary>
        /// WebSocket心跳线程
        /// </summary>
        public Java.Lang.Thread WebSocketThread = null;

        /// <summary>
        /// WebSocket客户端
        /// </summary>
        public System.Net.WebSockets.ClientWebSocket wsClient = null;

        /// <summary>
        /// WebSocket线程控制
        /// </summary>
        public bool bIsWebSocketHandlerOn = false;

        /// <summary>
        /// WebSocket工作
        /// </summary>
        public void WebSocketHandler()
        {
            int nMaxSize = 4096;
            Uri uri = new Uri(Settings.wsUri);
            wsClient = new System.Net.WebSockets.ClientWebSocket();

            bIsWebSocketHandlerOn = true;
            while (bIsWebSocketHandlerOn)
            {
                try
                {
                    // WebSocket连接
                    wsClient.ConnectAsync(uri, System.Threading.CancellationToken.None).Wait();
                    while (wsClient.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        // 格式化心跳参数
                        if (!Settings.HeartBeatParams.Has("action"))
                        {
                            Settings.HeartBeatParams.Put("action", "AdSubAppHeartBeat");
                        }
                        if (!Settings.HeartBeatParams.Has("cpuId"))
                        {
                            //Settings.HeartBeatParams.Put("cpuId", Settings.CpuId);
                            Settings.HeartBeatParams.Put("cpuId", "666999");
                        }
                        if (!Settings.HeartBeatParams.Has("version"))
                        {
                            Settings.HeartBeatParams.Put("version", Settings.Version);
                        }

                        // 准备发送
                        byte[] tmpSendBuf = System.Text.Encoding.Default.GetBytes(Settings.HeartBeatParams.ToString().Replace("\r", "").Replace("\n", "").Replace(" ", ""));
                        var sendBuf = new ArraySegment<byte>(tmpSendBuf);  // 发送缓冲区
                        wsClient.SendAsync(sendBuf, System.Net.WebSockets.WebSocketMessageType.Text, true, System.Threading.CancellationToken.None).Wait();
                        Settings.HeartBeatParams.Remove("lastCmd");
                        Settings.HeartBeatParams.Remove("errMsg");
                        tmpSendBuf = null;

                        // 准备接收
                        byte[] tmpRecvBuf = new byte[nMaxSize];
                        var recvBuf = new ArraySegment<byte>(tmpRecvBuf);  // 接收缓冲区
                        wsClient.ReceiveAsync(recvBuf, System.Threading.CancellationToken.None).Wait();

                        // 解析数据
                        string recvMsg = System.Text.Encoding.Default.GetString(recvBuf.Array);
                        recvMsg = recvMsg.Substring(0, recvMsg.IndexOf('\0'));
                        tmpRecvBuf = null;

                        // 解析心跳返回数据
                        //ParseHeartBeatResult(recvMsg);

                        System.Threading.Thread.Sleep(Settings.HeartBeatRate);
                        //MemoryCheck();
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("WebSocket Exception: " + e.Message);
                    Settings.RuntimeLog.Severe("WebSocket Exception: " + e.ToString());
                }
            }
        }

        /// <summary>
        /// 解析心跳返回数据
        /// </summary>
        /// <param name="heartBeatResult">心跳返回数据</param>  
        public void ParseHeartBeatResult(string heartBeatResult)
        {
            try
            {
                using (JSONObject jsonResult = new JSONObject(heartBeatResult))
                {
                    // 解析结果
                    string result = jsonResult.OptString("result");

                    if (result == "Success")
                    {
                        // 解析命令
                        string cmd = jsonResult.OptString("cmd");
                        if (!string.IsNullOrEmpty(cmd))
                        {
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, "cmd: " + cmd, ToastLength.Short).Show();
                            });
                            if (Settings.RuntimeLog.IsLoggable(Level.Fine))
                            {
                                Settings.RuntimeLog.Fine("cmd: " + cmd);
                            }

                            // 解析参数
                            using (JSONObject cmdArg = jsonResult.OptJSONObject("cmdArg"))
                            {
                                if (cmd == "updateConfig")
                                {
                                    string user = cmdArg.OptString("user");
                                    string pwd = cmdArg.OptString("pwd");
                                    string path = cmdArg.OptString("path");

                                    lock (ConfigLocker)
                                    {
                                        // 备份配置文件
                                        Settings.RuntimeLog.Info("备份配置文件");
                                        string configPath = Settings.AppPath + "/" + GetString(Resource.String.configFile);
                                        string bakConfigPath = configPath + ".bak";
                                        File bakConfigFile = new File(bakConfigPath);
                                        File configFile = new File(configPath);
                                        configFile.RenameTo(bakConfigFile);

                                        // 下载新配置文件
                                        bool bSussess = FtpDownload(user, pwd, path, configPath);
                                        if (bSussess)
                                        {
                                            // 更新节目
                                            // 读取配置文件
                                            using (JSONObject jsonConfig = ReadConfig())
                                            {
                                                string ftpUser = cmdArg.OptString("user");
                                                string ftpPwd = cmdArg.OptString("pwd");
                                                using (JSONObject program = jsonConfig.OptJSONObject("program"))
                                                using (JSONArray resources = program.OptJSONArray("resources"))
                                                {
                                                    int i = 0, nLength = resources.Length();
                                                    for (i = 0; i < nLength; i++)  // 资源循环
                                                    {
                                                        string name, type, md5, url;
                                                        using (JSONObject resource = resources.GetJSONObject(i))
                                                        {
                                                            name = resource.OptString("name");
                                                            type = resource.OptString("type");
                                                            md5 = resource.OptString("md5");
                                                            url = resource.OptString("url");
                                                        }
                                                        string localpath = Settings.AppPath + "/" + type + "/" + name;
                                                        using (File localFile = new File(localpath))
                                                        {
                                                            if (localFile.Exists())
                                                            {
                                                                RunOnUiThread(() =>
                                                                {
                                                                    Toast.MakeText(this, localFile.Name + "文件已存在", ToastLength.Long).Show();
                                                                });
                                                                Settings.RuntimeLog.Info(localFile.Name + "文件已存在");
                                                                continue;
                                                            }
                                                            else
                                                            {
                                                                using (File typePath = new File(Settings.AppPath + "/" + type))
                                                                {
                                                                    if (!typePath.Exists())
                                                                    {
                                                                        typePath.Mkdirs();
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        // 下载资源
                                                        if (FtpDownload(ftpUser, ftpPwd, url, localpath))
                                                        {
                                                            // 下载资源成功
                                                            RunOnUiThread(() =>
                                                            {
                                                                Toast.MakeText(this, "下载资源" + name + "成功", ToastLength.Long).Show();
                                                            });
                                                            Settings.RuntimeLog.Info("下载资源" + name + "成功");

                                                            //// 匹配MD5值
                                                            //if (!MatchMd5(localpath, md5))
                                                            //{
                                                            //    // 匹配MD5值失败
                                                            //    RunOnUiThread(() =>
                                                            //    {
                                                            //        Toast.MakeText(this, "匹配MD5值失败", ToastLength.Long).Show();
                                                            //    });
                                                            //    Settings.RuntimeLog.Warning("匹配MD5值失败");
                                                            //    Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                                            //    Settings.HeartBeatParams.Put("errMsg", "Failed to match " + name + " MD5");

                                                            //    // 恢复备份文件
                                                            //    configFile.Delete();
                                                            //    bakConfigFile.RenameTo(configFile);
                                                            //    break;
                                                            //}
                                                        }
                                                        else
                                                        {
                                                            // 下载资源失败
                                                            RunOnUiThread(() =>
                                                            {
                                                                Toast.MakeText(this, "下载资源" + name + "失败", ToastLength.Long).Show();
                                                            });
                                                            Settings.RuntimeLog.Warning("下载资源" + name + "失败");
                                                            Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                                            Settings.HeartBeatParams.Put("errMsg", "Failed to download " + name);

                                                            // 恢复备份文件
                                                            configFile.Delete();
                                                            bakConfigFile.RenameTo(configFile);
                                                            Settings.RuntimeLog.Info("恢复备份文件");
                                                            break;
                                                        }
                                                    }
                                                    if (i >= nLength)
                                                    {
                                                        // 所有资源下载成功
                                                        RunOnUiThread(() =>
                                                        {
                                                            Toast.MakeText(this, "所有资源下载成功", ToastLength.Long).Show();
                                                        });
                                                        Settings.RuntimeLog.Info("所有资源下载成功");
                                                        Settings.HeartBeatParams.Put("lastCmd", cmd + "_Success");

                                                        // 重新载入节目
                                                        bIsProgramLoop = false;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // 下载配置失败
                                            RunOnUiThread(() =>
                                            {
                                                Toast.MakeText(this, "下载配置失败", ToastLength.Long).Show();
                                            });
                                            Settings.RuntimeLog.Warning("下载配置失败");
                                            Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                            Settings.HeartBeatParams.Put("errMsg", "Failed to download Config");

                                            // 恢复备份文件
                                            bakConfigFile.RenameTo(configFile);
                                            Settings.RuntimeLog.Info("恢复备份文件");
                                        }

                                        // 删除备份配置文件
                                        bakConfigFile.Delete();
                                        bakConfigFile.Dispose();
                                        bakConfigFile = null;
                                        configFile.Dispose();
                                        configFile = null;
                                    }
                                }
                                else if(cmd == "update")
                                {
                                    string user = cmdArg.OptString("user");
                                    string pwd = cmdArg.OptString("pwd");
                                    string path = cmdArg.OptString("path");
                                    string localPath = Settings.AppPath + "/" + GetString(Resource.String.ApplicationName) + ".apk";

                                    // 下载新版本
                                    bool bSussess = FtpDownload(user, pwd, path, localPath);
                                    if (bSussess)
                                    {
                                        // 下载新版本成功
                                        RunOnUiThread(() =>
                                        {
                                            Toast.MakeText(this, "下载新版本成功", ToastLength.Long).Show();
                                        });
                                        Settings.RuntimeLog.Info("下载新版本成功");
                                        Settings.HeartBeatParams.Put("lastCmd", cmd + "_Succeed");

                                        // 安装
                                        InstallApp(localPath);
                                    }
                                    else
                                    {
                                        // 下载新版本失败
                                        RunOnUiThread(() =>
                                        {
                                            Toast.MakeText(this, "下载新版本失败", ToastLength.Long).Show();
                                        });
                                        Settings.RuntimeLog.Warning("下载新版本失败");
                                        Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                        Settings.HeartBeatParams.Put("errMsg", "Failed to download new version");
                                    }
                                }
                                else if (cmd == "restart")
                                {
                                    Settings.HeartBeatParams.Put("lastCmd", cmd + "_Success");
                                    RestartApp(this.PackageName);
                                    Finish();
                                }
                                else if (cmd == "stop")
                                {
                                    Settings.HeartBeatParams.Put("lastCmd", cmd + "_Success");
                                    Daemon();  // 关闭前，查看守护进程
                                    Finish();
                                    System.Threading.Thread.Sleep(8);
                                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                                }
                                else
                                {
                                    // other cmd
                                    Settings.RuntimeLog.Warning("Unknown cmd: " + cmd);
                                    Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                    Settings.HeartBeatParams.Put("errMsg", "Unknown cmd: " + cmd);
                                }
                            }
                        }
                        else
                        {
                            string timeStamp = jsonResult.OptString("timeStamp");

                            RunOnUiThread(() =>
                            {
                                using (Toast toast = Toast.MakeText(this, "心跳线程: " + timeStamp, ToastLength.Short))
                                {
                                    toast.SetGravity(GravityFlags.AxisSpecified | GravityFlags.Bottom, 666, 48);
                                    toast.Show();
                                }
                            });
                            Settings.RuntimeLog.Info("心跳线程: " + timeStamp);
                        }
                    }
                    else  // Failure
                    {
                        string ErrMsg = jsonResult.OptString("ErrMsg");

                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "心跳线程: " + ErrMsg, ToastLength.Long).Show();
                        });
                        Settings.RuntimeLog.Warning("心跳线程: " + ErrMsg);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("ParseHttpResult Exception: " + e.Message);
                Settings.RuntimeLog.Severe("ParseHttpResult Exception: " + e.ToString());
            }
        }

        /// <summary>
        /// 下载进度
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="max">最大进度</param>
        /// <param name="progress">当前进度</param>
        public void DownloadProgress(string title, int max, int progress)
        {
            // 获取系统全局Notification管理器
            if (Settings.nm == null)
            {
                Settings.nm = this.GetSystemService(Context.NotificationService) as NotificationManager;
            }

            if (progress > max)
            {
                Settings.nm.Cancel(0);  // 下载完毕，移除通知栏
            }
            else
            {
                int requestCode = 123456 + System.DateTime.Now.Millisecond;
                using (Intent intent = new Intent())
                {
                    using (PendingIntent operation = PendingIntent.GetActivity(this, requestCode, intent, PendingIntentFlags.OneShot))
                    {
                        Notification notification = new Notification.Builder(this)
                            .SetSmallIcon(Resource.Drawable.Icon)  // App小图标
                            .SetLargeIcon(null)  // App大图标
                            .SetContentTitle(title)  // 设置通知信息
                            .SetContentIntent(operation)
                            .SetWhen(DateTime.Now.Ticks)
                            .SetContentText("当前下载进度: " + progress / max + "%")
                            .SetAutoCancel(true)  // 用户点击后自动删除
                            .SetDefaults(NotificationDefaults.Lights)  // 灯光
                            .SetPriority(0)  // 设置优先级
                            .SetOngoing(true)
                            .SetProgress(max, progress, false)  // max最大进度 progress当前进度
                            .Build();

                        Settings.nm.Notify(0, notification);
                        notification.Dispose();
                        notification = null;
                    }
                }
            }
        }

        /// <summary>
        /// FTP下载
        /// </summary>
        /// <param name="userName">用户名</param>  
        /// <param name="password">密码</param>  
        /// <param name="remotepath">远程路径</param>  
        /// <param name="localpath">本地路径</param> 
        public bool FtpDownload(string userName, string password, string remotepath, string localpath)
        {
            bool bRet = true;
            var request = System.Net.WebRequest.Create(remotepath) as System.Net.FtpWebRequest;
            request.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new System.Net.NetworkCredential(userName, password);
            request.KeepAlive = false;
            request.UseBinary = true;

            System.Net.FtpWebResponse response = null;
            System.IO.Stream resStream = null;
            System.IO.FileStream outStream = null;
            try
            {
                response = request.GetResponse() as System.Net.FtpWebResponse;
                resStream = response.GetResponseStream();

                outStream = new System.IO.FileStream(localpath, System.IO.FileMode.Create);

                byte[] tmpBuf = new byte[2048];
                int nReadCount = resStream.Read(tmpBuf, 0, tmpBuf.Length);
                while (nReadCount > 0)
                {
                    outStream.Write(tmpBuf, 0, nReadCount);
                    nReadCount = resStream.Read(tmpBuf, 0, tmpBuf.Length);
                }
                tmpBuf = null;
            }
            catch (Exception e)
            {
                bRet = false;
                System.Console.WriteLine("FtpDownload Exception: " + e.Message);
                Settings.RuntimeLog.Severe("FtpDownload Exception: " + e.ToString());
            }
            finally
            {
                if (outStream != null)
                {
                    outStream.Flush();
                    outStream.Close();
                    outStream.Dispose();
                    outStream = null;
                }
                if (resStream != null)
                {
                    resStream.Close();
                    resStream.Dispose();
                    resStream = null;
                }
                if (response != null)
                {
                    response.Close();
                    response.Dispose();
                    response = null;
                }
            }
            return bRet;
        }

        /// <summary>
        /// 匹配MD5值
        /// </summary>
        /// <param name="matchFile">匹配文件</param>  
        /// <param name="realMd5Value">真实MD5值</param>  
        public bool MatchMd5(string matchFile, string realMd5Value)
        {
            bool bRet = false;
            Md5Encrypt encrypt = new Md5Encrypt();
            try
            {
                using (var stream = new System.IO.StreamReader(matchFile))
                {
                    string inputString = stream.ReadToEnd();
                    string md5Value = encrypt.Output(inputString, System.Text.Encoding.Default).ToUpper();
                    if (md5Value == realMd5Value.ToUpper())
                    {
                        // 匹配正确
                        bRet = true;
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("MatchMd5 Exception: " + e.Message);
                Settings.RuntimeLog.Severe("MatchMd5 Exception: " + e.ToString());
            }
            encrypt = null;
            return bRet;
        }

        /// <summary>
        /// 安装应用
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="triggerAtMillis">延时安装时间（默认3000ms）</param>
        public void InstallApp(string filePath, long triggerAtMillis = 3000)
        {
            int requestCode = 123456 + System.DateTime.Now.Millisecond;
            using (Intent intent = new Intent(Intent.ActionView))
            {
                intent.SetDataAndType(Android.Net.Uri.Parse("file://" + filePath), 
                    "application/vnd.android.package-archive");
                intent.AddFlags(ActivityFlags.NewTask);
                using (PendingIntent operation = PendingIntent.GetActivity(this, requestCode, intent, PendingIntentFlags.OneShot))
                using (AlarmManager am = GetSystemService(Context.AlarmService) as AlarmManager)
                {
                    am.Set(AlarmType.Rtc, triggerAtMillis, operation);
                }
            }
        }
    }
}

