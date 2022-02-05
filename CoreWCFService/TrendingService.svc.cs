using System.ServiceModel;

namespace CoreWCFService
{
    public class TrendingService : ITrendingService
    {
        public void InitializeTrending()
        {
            TagProcessing.OnTagValueChanged += OperationContext.Current.GetCallbackChannel<ITrendingCallback>().TagValueChanged;
        }   
    }
}
