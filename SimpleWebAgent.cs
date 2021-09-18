using System;
using Hyperledger.Aries.Agents;

namespace WebAgent
{
    public class SimpleWebAgent : AgentBase
    {
        public SimpleWebAgent(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {

        }

        protected override void ConfigureHandlers()
        {
         
            AddConnectionHandler();
            AddForwardHandler();
            AddDiscoveryHandler();
            AddTrustPingHandler();
            AddCredentialHandler();
            AddProofHandler();
            
        }
    }
}
