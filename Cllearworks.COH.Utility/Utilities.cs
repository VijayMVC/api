namespace Cllearworks.COH.Utility
{
    public static class Utilities
    {
        public static string GetImageBasePath() {
            var ImageBasePath = "http://cohapi.azurewebsites.net/Images/";

#if DEBUG
            ImageBasePath = "http://localhost:21388/Images/";
#endif
            return ImageBasePath;
        }
    }
}
