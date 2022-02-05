using System.ServiceModel;

namespace CoreWCFService
{
    [ServiceContract (CallbackContract = typeof(ITrendingCallback))]
    public interface ITrendingService
    {
        [OperationContract (IsOneWay = true)]
        void InitializeTrending();
    }

    public interface ITrendingCallback
    {
        [OperationContract (IsOneWay = true)]
        void TagValueChanged(InputTag tag, double value);
    }
}
