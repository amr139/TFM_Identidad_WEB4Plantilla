using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.PresentProof;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAgent.Hubs;

namespace WebAgent.Utils
{
    public class WSocketMiddleware : IAgentMiddleware
    {
        private readonly IHubContext<TestHub> _hubContext;

        public WSocketMiddleware(IHubContext<TestHub> hubContext)
        {
            _hubContext = hubContext;

        }

        public Task OnMessageAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            switch (messageContext.GetMessageType())
            {
                case MessageTypes.PresentProofNames.Presentation:
                case MessageTypesHttps.PresentProofNames.Presentation:
                    ProofRecord registro = (ProofRecord)messageContext.ContextRecord;
                    PartialProof proofJson = JsonConvert.DeserializeObject<PartialProof>(registro.ProofJson);
                    ProofRequest requestJson = JsonConvert.DeserializeObject<ProofRequest>(registro.RequestJson);

                    var elementos = proofJson.RequestedProof.RevealedAttributes;

                    if(registro.State == ProofState.Accepted)
                    {
                        ShorterData d = MemoryDatabase.GetRegisterByNonce(requestJson.Nonce);
                        if (d != null)
                        {
                            if (d.NumClients == 1)
                            {
                                return _hubContext.Clients.Groups(d.WSocketID).SendAsync("/room/" + d.WSocketID + "/NewMessage", JWTHelper.GenerateJWT(elementos));
                            }
                        }
                    }
                    break;
            }
            return Task.CompletedTask;
        }
    }
}

//158044915114739742005559
