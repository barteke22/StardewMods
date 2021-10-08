using StardewModdingAPI;

namespace FishingMinigames
{
    class Log
    {
        public static IMonitor Monitor;

        public static void Error(params object[] text)
        {
            string s = "";
            if (text.Length == 1) s += text[0];
            else
            {
                foreach (var item in text)
                {
                    s += ", " + item;
                }
            }
            Monitor.Log(s, LogLevel.Error);
        }
    }
}
