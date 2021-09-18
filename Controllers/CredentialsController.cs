using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage;
using Hyperledger.Indy.AnonCredsApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebAgent.Models;
using WebAgent.Utils;
using WebAgent.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebAgent.Controllers
{
    public class CredentialsController : Controller
    {
        
        private string mySchema = "MBvzXv85ALtZSSJnx3XWby:2:Billete Airberia SSI:1.0";
        private string myCredDef = "MBvzXv85ALtZSSJnx3XWby:3:CL:6298:Default";
        private string dniCredDef = "Mf4XeAEt1j7NRpQBC5us2h:3:CL:6138:Default";
        private string covidCredDef = "84Lc1u4ZfrAbGXYZeh1Y9W:3:CL:6158:Default";
        private string DomainURL = "http://374e-95-23-238-98.ngrok.io";

        private readonly IAgentProvider _agentContextProvider;
        private readonly IProvisioningService _provisionService;
        private readonly IWalletService _walletService;
        private readonly IConnectionService _connectionService;
        private readonly ICredentialService _credentialService;
        private readonly ISchemaService _schemaService;
        private readonly IMessageService _messageService;
        private readonly IProofService _proofService;
        private readonly IHubContext<TestHub> _hubContext;

        //Constructor con inyeccion de dependencias
        public CredentialsController(
            IAgentProvider agentContextProvider,
            IProvisioningService provisionService,
            IWalletService walletService,
            IConnectionService connectionService,
            ICredentialService credentialService,
            ISchemaService schemaService,
            IMessageService messageService,
            IProofService proofService,
            IHubContext<TestHub> hubContext
            )
        {
            _agentContextProvider = agentContextProvider;
            _provisionService = provisionService;
            _walletService = walletService;
            _connectionService = connectionService;
            _credentialService = credentialService;
            _schemaService = schemaService;
            _messageService = messageService;
            _proofService = proofService;
            _hubContext = hubContext;
        }

        //Pinta la portada de la web
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var issuer = await _provisionService.GetProvisioningAsync(context.Wallet);
            return View("Index");
        }

        //Pinta el formulario para crear las credenciales
        [Authorize]
        [HttpGet]
        public IActionResult Registro()
        {
            return View("RegistroForm");
        }

        //Recibe el form y expide una credencial
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Registro(CredentialModel modelo)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            var context = await _agentContextProvider.GetContextAsync();
            var issuer = await _provisionService.GetProvisioningAsync(context.Wallet);
            
            if(identity.FindFirst("NIF-requeriment")!=null){
                string nif = identity.FindFirst("NIF-requeriment").Value;
                if (identity.FindFirst("Nombre-requeriment") != null)
                {
                    string nombre = identity.FindFirst("Nombre-requeriment").Value;

                    if (identity.FindFirst("Apellidos-requeriment") != null)
                    {
                        string apellidos = identity.FindFirst("Apellidos-requeriment").Value;
                        modelo.NombreCompleto = String.Format("{0} | {1} {2}", nif, nombre, apellidos);
                        modelo.ID = MemoryDatabase.GenerateRandomString(16);
                        var values = modelo.ToCredentialList();

                        var (offer, _) = await _credentialService.CreateOfferAsync(context, new OfferConfiguration
                        {
                            CredentialDefinitionId = myCredDef,
                            IssuerDid = issuer.IssuerDid,
                            CredentialAttributeValues = values,
                        });

                        var offerJSON = JObject.Parse(offer.ToJson());
                        offerJSON["~service"]["serviceEndpoint"] = DomainURL;

                        var encoded = offerJSON.ToString(Formatting.None).ToBase64();
                        MemoryDatabase.AddRegister(offer.Id, encoded, true);

                        return View("RegistroResult", String.Format("{0}/Link?data={1}", DomainURL, offer.Id));
                    }
                }
            }
            
            return View("Error");
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var issuer = await _provisionService.GetProvisioningAsync(context.Wallet);

            var proof = new ProofRequest()
            {
                Name = "Solicitud",
                Version = "1.0",
                Nonce = await AnonCreds.GenerateNonceAsync(),
                RequestedAttributes = new DNIModel().ToProof(dniCredDef)
            };

            var (offer, _) = await _proofService.CreateRequestAsync(context, proof);

            var offerJSON = JObject.Parse(offer.ToJson());
            offerJSON["~service"]["serviceEndpoint"] = DomainURL;

            var encoded = offerJSON.ToString(Formatting.None).ToBase64();
            MemoryDatabase.AddRegister(proof.Nonce, encoded);

            return View("Login", new LoginProofModel() { 
                URL = String.Format(String.Format("{0}/Link?data={1}", DomainURL, proof.Nonce)),
                WSID = MemoryDatabase.GetRegisterByNonce(proof.Nonce).WSocketID
            });
        }

        public async Task<IActionResult> ComprobarBillete()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var issuer = await _provisionService.GetProvisioningAsync(context.Wallet);

            var proof = new ProofRequest()
            {
                Name = "Solicitud Billete Airberia SSI",
                Version = "1.0",
                Nonce = await AnonCreds.GenerateNonceAsync(),
                RequestedAttributes = new ComprobarBilleteModel().ToProof(myCredDef)
            };

            var (offer, _) = await _proofService.CreateRequestAsync(context, proof);

            var offerJSON = JObject.Parse(offer.ToJson());
            offerJSON["~service"]["serviceEndpoint"] = DomainURL;

            var encoded = offerJSON.ToString(Formatting.None).ToBase64();
            MemoryDatabase.AddRegister(proof.Nonce, encoded);

            return View("LoginBillete", new LoginProofModel()
            {
                URL = String.Format(String.Format("{0}/Link?data={1}", DomainURL, proof.Nonce)),
                WSID = MemoryDatabase.GetRegisterByNonce(proof.Nonce).WSocketID
            });
        }

        //Metodoo
        [HttpGet]
        public IActionResult Link(string data)
        {
            ShorterData registro = MemoryDatabase.GetRegisterByNonce(data);
            if (registro != null)
            {
                if (registro.IssueCredential == true)
                {
                    MemoryDatabase.RemoveRegister(data);
                }
                return Redirect(String.Format("https://trinsic.studio/link/?d_m={0}", registro.MessageData));
            }
            return BadRequest();
        }

         
        [HttpGet]
        public async Task<IActionResult> RegisterSchema()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var issuer = await _provisionService.GetProvisioningAsync(context.Wallet);

            var schemaId = await _schemaService.CreateSchemaAsync(
                context: context,
                issuerDid: issuer.IssuerDid,
                name: "Billete Airberia SSI",
                version: "1.0",
                attributeNames: new CredentialModel().GetProperties());


            var cred2 = await _schemaService.CreateCredentialDefinitionAsync(context, new CredentialDefinitionConfiguration
            {

                SchemaId = schemaId,
                Tag = "Default",
                EnableRevocation = false,
                RevocationRegistrySize = 0,
                RevocationRegistryBaseUri = "",
                RevocationRegistryAutoScale = false,
                IssuerDid = issuer.IssuerDid
            });

            Console.WriteLine("schemaid: " + schemaId);
            Console.WriteLine("credDef: " + cred2);
            return Ok("FIN");
        }
        

    }
}
