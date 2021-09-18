using Hyperledger.Aries.Features.PresentProof;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAgent.Models;

namespace WebAgent.Models
{
    public class ComprobarBilleteModel
    {
        public Dictionary<string, ProofAttributeInfo> ToProof(string renfeCredDef)
        {
            Dictionary<string, string[]> dni_model = new Dictionary<string, string[]>() { { "Mf4XeAEt1j7NRpQBC5us2h:3:CL:6138:Default", new string[] { "NIF", "Nombre", "Apellidos" } } };
            Dictionary<string, string[]> covid_model = new Dictionary<string, string[]>() { { "84Lc1u4ZfrAbGXYZeh1Y9W:3:CL:6158:Default", new string[] { "ID", "Country", "Vaccined", "CovidIllness", "Issued" } } };

            Dictionary<string, ProofAttributeInfo> proof = new Dictionary<string, ProofAttributeInfo>();
            
            foreach (var t in covid_model.Values.First())
            {
                {
                    proof.Add(t + "-COVIDrequeriment", new ProofAttributeInfo()
                    {
                        Name = t,
                        Restrictions = new List<AttributeFilter>() {
                        new AttributeFilter() {
                            CredentialDefinitionId = covid_model.Keys.First(),
                        }
                    }
                    });
                }
            }

            return proof.Union(new CredentialModel().ToProof(renfeCredDef)).ToDictionary(k => k.Key, v => v.Value);
        }

    }
        
}
