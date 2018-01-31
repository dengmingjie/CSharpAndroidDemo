using Java.Util.Logging;

namespace AdSubApp.Util
{
    public class LogFormatter : Formatter
    {
        // 自定义日志输出格式
        public override string Format(LogRecord r)
        {
            string format = System.DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss.fff]");
            if (r.LoggerName != null)
            {
                format += "[" + r.LoggerName;
            }
            if (r.Level != null)
            {
                format += "-" + r.Level;
            }
            if (r.ThreadID >= 0)
            {
                format += "-" + r.ThreadID;
            }
            format += "]" + r.Message + "\n\n";
            return format;
        }
    }
}