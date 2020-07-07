using System.ComponentModel.Composition.Hosting;
using EXILED;

namespace EventfulLaboratory
{
    public class Logger
    {
        public static void Info(string text)
        {
            Log.Warn(Constant.SHORTFORM + "#" + text);
        }

        public static void Error(string text)
        {
            Log.Error(Constant.SHORTFORM + "#" + text);
        }
    }
}