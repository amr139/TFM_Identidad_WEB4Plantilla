using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAgent.Models
{
    
    public class CredentialModel
    {
        public string ID { get; set; }
        public string NombreCompleto { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public string FechaSalida { get; set; }
        public string Asiento { get; set; }
        public string Clase { get; set; }

        public List<CredentialPreviewAttribute> ToCredentialList()
        {
            List<CredentialPreviewAttribute> lista = new List<CredentialPreviewAttribute>();
            foreach (var t in GetType().GetProperties())
            {
                lista.Add(new CredentialPreviewAttribute()
                {
                    Name = t.Name,
                    Value = t.GetValue(this).ToString(),
                    MimeType = CredentialMimeTypes.ApplicationJsonMimeType
                });
            }
            return lista;
        }

        public string[] GetProperties()
        {
            string[] listado = new string[GetType().GetProperties().Length];
            for(int i=0;i<listado.Length;i++)
            {
                listado[i] = GetType().GetProperties()[i].Name;
            }
            return listado;
        }

        public Dictionary<string, ProofAttributeInfo> ToProof(string credDef)
        {
            Dictionary<string, ProofAttributeInfo> proof = new Dictionary<string, ProofAttributeInfo>();

            foreach (var t in GetType().GetProperties())
            {
                proof.Add(t.Name + "-requeriment", new ProofAttributeInfo()
                {
                    Name = t.Name,
                    Restrictions = new List<AttributeFilter>() {
                        new AttributeFilter() {
                            CredentialDefinitionId = credDef,
                        }
                    }
                });
            }
            return proof;
        }

    }
}
