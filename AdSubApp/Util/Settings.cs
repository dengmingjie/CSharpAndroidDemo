using System;
using System.Text;
using System.Threading;

using Java.IO;
using Java.Net;

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
        }

        /// <summary>
        /// 软件版本
        /// </summary>
        public static string Version = "0.0";

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
        public static Org.Json.JSONObject HeartBeatParams = new Org.Json.JSONObject();

        /// <summary>
        /// 被守护程序名
        /// </summary>
        public static string PeerName = "RestartAppService.RestartAppService";

        /// <summary>
        /// APP目录
        /// </summary>
        public static string AppPath;

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