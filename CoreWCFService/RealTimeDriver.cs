using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CoreWCFService
{
    [DataContract]
    public class RealTimeDriver : Driver
    {
        public static Dictionary<string, double> valuesOnAddresses = new Dictionary<string, double>();
        public static Dictionary<string, string> RTUs = new Dictionary<string, string>();

        public override double ReturnValue(string address)
        {
            if (valuesOnAddresses.ContainsKey(address))
                return valuesOnAddresses[address];
            return -1;
        }

        public static void MessageArrived(string message)
        {
            string[] tokens = message.Split(',');
            string id = tokens[0].Split(':')[1];
            double value = double.Parse(tokens[1].Split(':')[1]);
            string address = tokens[2].Split(':')[1];
            valuesOnAddresses[address] = value;
            if (!RTUs.ContainsKey(id))
                RTUs[id] = address;
        }

        public static bool IsAddressTaken(string address)
        {
            return RTUs.ContainsValue(address);
        }

        public static bool IsIdTaken(string id)
        {
            return RTUs.ContainsKey(id);
        }
    }
}