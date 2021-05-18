namespace EventfulLaboratory.Extension
{
    public static class CommonExtension
    {
        public static string ToBase64(this string str) =>
            System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        
        public static string FromBase64(this string str) =>
            System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(str));
    }
}