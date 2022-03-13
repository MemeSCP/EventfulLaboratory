using Exiled.API.Features;

namespace EventfulLaboratory
{
    public static class Logger
    {
        public static void Info(string text)
        {
            Log.Info($"{Constant.SHORTFORM}#{text}");
        }

        public static void Error(string text)
        {
            Log.Error($"{Constant.SHORTFORM}#{text}");
        }

        public static void Debug(params string[] args)
        {
            Log.Debug($"{Constant.SHORTFORM}#{string.Join(" ", args)}", EventfulLab.Instance.Config.DevelopmentMode);
        }
    }
}