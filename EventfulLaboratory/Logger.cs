using Exiled.API.Features;

namespace EventfulLaboratory
{
    public class Logger
    {
        public static void Info(string text)
        {
            Log.Info(Constant.SHORTFORM + "#" + text);
        }

        public static void Error(string text)
        {
            Log.Error(Constant.SHORTFORM + "#" + text);
        }
    }
}