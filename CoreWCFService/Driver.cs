using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreWCFService
{
    [DataContract]
    [KnownType(typeof(SimulationDriver))]
    [KnownType(typeof(RealTimeDriver))]
    public abstract class Driver
    {
        public abstract double ReturnValue(string address);
    }
}
