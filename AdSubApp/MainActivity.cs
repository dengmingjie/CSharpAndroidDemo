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

                // 重启后亦不生效，自杀后，通过守护进程开启
                Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                return;
            }

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

            // 节目控制线程
            if (ProgramControlThread == null)
            {
                ProgramControlThread = new Java.Lang.Thread(() =>
                {
                    bIsProgramControlThreadOn = true;
                    while (bIsProgramControlThreadOn)
                    {
                        System.Threading.Thread.Sleep(1000);
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
            if (am != null)
            {
                am.Dispose();
                am = null;
            }

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
            base.OnDestroy();
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
                    // 建议的方法：
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "开机启动", ToastLength.Short).Show();
                    });
                    Settings.HeartBeatParams.Put("lastCmd", mode + "_Success");
                    nMode = 1;
                }
                else if (mode == "restart")
                {
                    // 重启应用
                    // 建议的方法：
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "重启应用", ToastLength.Short).Show();
                    });
                    Settings.HeartBeatParams.Put("lastCmd", mode + "_Success");
                    nMode = 2;
                }
                else
                {
                    // 未知的启动方式
                    // 建议的方法：
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "未知的启动方式", ToastLength.Short).Show();
                    });
                    nMode = 3;
                }
            }
            else
            {
                // 手动启动
                // 建议的方法：
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "手动启动", ToastLength.Short).Show();
                });
                Settings.HeartBeatParams.Put("lastCmd", "start_Success");
                nMode = 0;
            }
            return nMode;
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
                // 建议的方法：
                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, "当前进程navtive堆内存不足", ToastLength.Long).Show();
                });
                System.GC.Collect();  // 垃圾回收
            }
            else
            {
                if (Settings.lNativeHeapSize != 0 && Settings.lNativeHeapSize != lNativeHeapSize)
                {
                    // 建议的方法：
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "当前进程navtive堆内存重新分配", ToastLength.Long).Show();
                    });
                    System.GC.Collect();  // 垃圾回收
                }
                Settings.lNativeHeapSize = lNativeHeapSize;
            }
        }

        /// <summary>
        /// 系统全局Activity管理器
        /// </summary>
        public ActivityManager am = null;

        /// <summary>
        /// 守护
        /// </summary>
        public void Daemon()
        {
            // 获取系统全局Activity管理器
            if (am == null)
            {
                am = (ActivityManager)this.GetSystemService(Context.ActivityService);
            }
            // 获取当前运行中进程
            var runningAppProcesses = am.RunningAppProcesses;
            bool bIsRunning = false;  // 被守护程序是否在运行中
            foreach (var one in runningAppProcesses)
            {
                var importance = one.Importance;  // 当前状态（前/后台）
                string processName = one.ProcessName;
                if (processName == Settings.PeerName)
                {
                    // 运行中
                    bIsRunning = true;
                    break;
                }
            }
            if (!bIsRunning)
            {
                // 未运行，则重启之！
                RestartApp(Settings.PeerName);
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
            Intent iStartActivity = PackageManager.GetLaunchIntentForPackage(packageName);
            iStartActivity.AddCategory(Intent.CategoryLauncher);
            iStartActivity.SetAction(Intent.ActionMain);
            iStartActivity.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            iStartActivity.PutExtra("mode", "restart");
            PendingIntent operation = PendingIntent.GetActivity(this, requestCode, iStartActivity, PendingIntentFlags.OneShot);
            AlarmManager am = (AlarmManager)GetSystemService(Context.AlarmService);
            am.Set(AlarmType.Rtc, triggerAtMillis, operation);
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
                        // 建议的方法：
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, GetString(Resource.String.configFile), ToastLength.Short).Show();
                        });

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
                            }
                        }
                        catch (IOException e)
                        {
                            jsonObject = null;
                            e.PrintStackTrace();
                            System.Console.WriteLine("Parse Config IOException: " + e.Message);
                        }
                    }
                    else
                    {
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
        bool bIsProgramControlThreadOn = false;

        /// <summary>
        /// 节目循环
        /// </summary>
        bool bIsProgramLoop = false;

        /// <summary>
        /// 节目控制
        /// </summary>
        public void ProgramControl()
        {
            // 读取配置文件
            JSONObject jsonConfig = ReadConfig();

            if (jsonConfig != null)
            {
                bIsProgramLoop = true;
            }

            // 节目循环
            while (bIsProgramLoop)
            {
                using (JSONObject program = jsonConfig.OptJSONObject("program"))
                using (JSONArray resources = program.OptJSONArray("resources"))
                {
                    int nLength = resources.Length();
                    for (int i = 0; i < nLength; i++)  // 资源循环
                    {
                        CActivityManager.GetInstence().FinishSingleActivityByType(typeof(VideoViewActivity));  // 关闭视频播放
                        CActivityManager.GetInstence().FinishSingleActivityByType(typeof(ImageViewActivity));  // 关闭图片播放
                        if (!bIsProgramLoop)
                        {
                            // 重新载入节目
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
                            // 建议的方法：
                            RunOnUiThread(() =>
                            {
                                // 播放视频
                                Intent intentVideo = new Intent(this, typeof(VideoViewActivity));
                                intentVideo.PutExtra("uri", Settings.AppPath + "/" + type + "/" + name);
                                //intentVideo.PutExtra("bIsLoop", "true");
                                StartActivity(intentVideo);
                            });
                            Settings.semVideoCompleted.WaitOne();  // 等待视频结束
                        }
                        else if (type == "image")
                        {
                            int nDuration = int.Parse(duration) + 1;  // 图片播放时间
                            // 建议的方法：
                            RunOnUiThread(() =>
                            {
                                // 播放图片
                                Intent intentImage = new Intent(this, typeof(SrcActivity.ImageViewActivity));
                                intentImage.PutExtra("uri", Settings.AppPath + "/" + type + "/" + name);
                                StartActivity(intentImage);
                            });
                            System.Threading.Thread.Sleep(nDuration * 1000);
                        }
                        else
                        {
                            // other type
                        }
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
        bool bIsHeartBeatThreadOn = false;

        /// <summary>
        /// 心跳请求
        /// </summary>
        public void HeartBeat()
        {
            try
            {
                HttpURLConnection urlConn = (HttpURLConnection)Settings.url.OpenConnection();  // 创建HTTP连接
                urlConn.RequestMethod = "POST";  // 启用POST方式
                //urlConn.RequestMethod = "Get";  // 启用Get方式
                urlConn.UseCaches = false;  // 不启用缓存
                urlConn.DoOutput = true;  // 启用输出流
                urlConn.DoInput = true;  // 启用输入流
                urlConn.InstanceFollowRedirects = true;  // 启用HTTP重定向
                //urlConn.SetRequestProperty("Content-Type", "application/x-www-form-urlencoded");  // 设置请求类型
                urlConn.SetRequestProperty("Content-Type", "application/json");  // 设置请求类型

                DataOutputStream outStream = new DataOutputStream(urlConn.OutputStream);  // 获取输出流
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
                outStream.Close();  // 关闭输出流
                outStream.Dispose();  // 释放输出流
                outStream = null;

                // 判断是否响应成功
                if (urlConn.ResponseCode == HttpStatus.Ok)
                {
                    InputStreamReader inStream = new InputStreamReader(urlConn.InputStream);  // 获取输入流
                    BufferedReader buffer = new BufferedReader(inStream);  // 获取输入流读取器
                    string inputLine = null, heartBeatResult = null;
                    while ((inputLine = buffer.ReadLine()) != null)
                    {
                        heartBeatResult += inputLine + "\n";
                    }
                    buffer.Close();  // 关闭输入流读取器
                    buffer.Dispose();  // 释放输入流读取器
                    buffer = null;
                    inStream.Close();  // 关闭输入流
                    inStream.Dispose();  // 释放输入流
                    inStream = null;

                    // 解析心跳返回数据
                    ParseHeartBeatResult(heartBeatResult);
                }
                else
                {
                    long Code = (long)urlConn.ResponseCode;

                    // 建议的方法：
                    RunOnUiThread(() =>
                    {
                        Toast.MakeText(this, "心跳线程: HTTP error code " + Code, ToastLength.Long).Show();
                    });
                }

                urlConn.Disconnect();  // 断开HTTP连接
                urlConn.Dispose();  // 释放HTTP连接
                urlConn = null;
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
                System.Console.WriteLine("HeartBeat IOException: " + e.Message);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("HeartBeat Exception: " + e.Message);
            }
        }

        /// <summary>
        /// WebSocket心跳线程
        /// </summary>
        public Java.Lang.Thread WebSocketThread = null;

        /// <summary>
        /// WebSocket客户端
        /// </summary>
        System.Net.WebSockets.ClientWebSocket wsClient = null;

        /// <summary>
        /// WebSocket线程控制
        /// </summary>
        bool bIsWebSocketHandlerOn = false;

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
                        tmpRecvBuf = null;

                        // 解析数据
                        string recvMsg = System.Text.Encoding.Default.GetString(recvBuf.Array);
                        recvMsg = recvMsg.Substring(0, recvMsg.IndexOf('\0'));

                        // 解析心跳返回数据
                        //ParseHeartBeatResult(recvMsg);

                        System.Threading.Thread.Sleep(Settings.HeartBeatRate);
                        MemoryCheck();
                    }
                }
                catch (Exception e)
                {
                    string errStackTrace = e.StackTrace;
                    string errMessage = e.Message;
                    System.Console.WriteLine("WebSocket Exception: " + errMessage);
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
                            // 建议的方法：
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, "cmd: " + cmd, ToastLength.Long).Show();
                            });

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
                                            JSONObject jsonConfig = ReadConfig();
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
                                                        //// 匹配MD5值
                                                        //if (!MatchMd5(localpath, md5))
                                                        //{
                                                        //    // 匹配MD5值失败
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
                                                        Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                                        Settings.HeartBeatParams.Put("errMsg", "Failed to download " + name);

                                                        // 恢复备份文件
                                                        configFile.Delete();
                                                        bakConfigFile.RenameTo(configFile);
                                                        break;
                                                    }
                                                }
                                                if (i >= nLength)
                                                {
                                                    // 所有资源下载成功
                                                    Settings.HeartBeatParams.Put("lastCmd", cmd + "_Success");

                                                    // 重新载入节目
                                                    bIsProgramLoop = false;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // 下载配置失败
                                            Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                            Settings.HeartBeatParams.Put("errMsg", "Failed to download Config");

                                            // 恢复备份文件
                                            bakConfigFile.RenameTo(configFile);
                                        }

                                        // 删除备份配置文件
                                        bakConfigFile.Delete();
                                        bakConfigFile.Dispose();
                                        bakConfigFile = null;
                                        configFile.Dispose();
                                        configFile = null;
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
                                    Settings.HeartBeatParams.Put("lastCmd", cmd + "_Failure");
                                    Settings.HeartBeatParams.Put("errMsg", "Unknown cmd: " + cmd);
                                }
                            }
                        }
                        else
                        {
                            string timeStamp = jsonResult.OptString("timeStamp");

                            // 建议的方法：
                            RunOnUiThread(() =>
                            {
                                Toast.MakeText(this, "心跳线程: " + timeStamp, ToastLength.Long).Show();
                            });
                        }
                    }
                    else  // Failure
                    {
                        string ErrMsg = jsonResult.OptString("ErrMsg");

                        // 建议的方法：
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, "心跳线程: " + ErrMsg, ToastLength.Long).Show();
                        });
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("ParseHttpResult Exception: " + e.Message);
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
            var request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(remotepath);
            request.Method = System.Net.WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new System.Net.NetworkCredential(userName, password);
            request.KeepAlive = false;
            request.UseBinary = true;

            System.Net.FtpWebResponse response = null;
            System.IO.Stream resStream = null;
            System.IO.FileStream outStream = null;
            try
            {
                response = (System.Net.FtpWebResponse)request.GetResponse();
                resStream = response.GetResponseStream();

                outStream = new System.IO.FileStream(localpath, System.IO.FileMode.Create);

                byte[] tmpBuf = new byte[2048];
                int nReadCount = resStream.Read(tmpBuf, 0, tmpBuf.Length);
                while (nReadCount > 0)
                {
                    outStream.Write(tmpBuf, 0, nReadCount);
                    nReadCount = resStream.Read(tmpBuf, 0, tmpBuf.Length);
                }
            }
            catch (IOException e)
            {
                bRet = false;
                e.PrintStackTrace();
                System.Console.WriteLine("FtpDownload IOException: " + e.Message);
            }
            catch (Exception e)
            {
                bRet = false;
                System.Console.WriteLine("FtpDownload Exception: " + e.Message);
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
            }
            encrypt = null;
            return bRet;
        }
    }
}

