﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RealTimeUnit.ServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IRTUService")]
    public interface IRTUService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRTUService/SendMessage", ReplyAction="http://tempuri.org/IRTUService/SendMessageResponse")]
        bool SendMessage(string message, byte[] signature);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRTUService/SendMessage", ReplyAction="http://tempuri.org/IRTUService/SendMessageResponse")]
        System.Threading.Tasks.Task<bool> SendMessageAsync(string message, byte[] signature);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRTUService/IsAddressTaken", ReplyAction="http://tempuri.org/IRTUService/IsAddressTakenResponse")]
        bool IsAddressTaken(string address);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRTUService/IsAddressTaken", ReplyAction="http://tempuri.org/IRTUService/IsAddressTakenResponse")]
        System.Threading.Tasks.Task<bool> IsAddressTakenAsync(string address);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRTUService/IsIdTaken", ReplyAction="http://tempuri.org/IRTUService/IsIdTakenResponse")]
        bool IsIdTaken(string id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IRTUService/IsIdTaken", ReplyAction="http://tempuri.org/IRTUService/IsIdTakenResponse")]
        System.Threading.Tasks.Task<bool> IsIdTakenAsync(string id);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IRTUServiceChannel : RealTimeUnit.ServiceReference.IRTUService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class RTUServiceClient : System.ServiceModel.ClientBase<RealTimeUnit.ServiceReference.IRTUService>, RealTimeUnit.ServiceReference.IRTUService {
        
        public RTUServiceClient() {
        }
        
        public RTUServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public RTUServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RTUServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public RTUServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool SendMessage(string message, byte[] signature) {
            return base.Channel.SendMessage(message, signature);
        }
        
        public System.Threading.Tasks.Task<bool> SendMessageAsync(string message, byte[] signature) {
            return base.Channel.SendMessageAsync(message, signature);
        }
        
        public bool IsAddressTaken(string address) {
            return base.Channel.IsAddressTaken(address);
        }
        
        public System.Threading.Tasks.Task<bool> IsAddressTakenAsync(string address) {
            return base.Channel.IsAddressTakenAsync(address);
        }
        
        public bool IsIdTaken(string id) {
            return base.Channel.IsIdTaken(id);
        }
        
        public System.Threading.Tasks.Task<bool> IsIdTakenAsync(string id) {
            return base.Channel.IsIdTakenAsync(id);
        }
    }
}
