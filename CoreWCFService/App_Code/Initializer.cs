namespace CoreWCFService.App_Code
{
    public class Initializer
    {
        public static void AppInitialize()
        {
            TagProcessing.LoadConfiguration();
            TagProcessing.Simulate();
        }
    }
}