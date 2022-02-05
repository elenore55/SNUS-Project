using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoreWCFService
{
    [DataContract]
    public class RealTimeDriver : Driver
    {
        public static Dictionary<string, double> valuesOnAddresses = new Dictionary<string, double>();

        public override double ReturnValue(string address)
        {
            if (valuesOnAddresses.ContainsKey(address))
                return valuesOnAddresses[address];
            return -1;
        }

        public static void MessageArrived(string message)
        {
            string[] tokens = message.Split(',');
            double value = double.Parse(tokens[1].Split(':')[1]);
            string address = tokens[2].Split(':')[1];
            valuesOnAddresses[address] = value;
        }
    }
}