using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebAgent.Utils
{
    public class JWTHelper
    {

        public static string GenerateJWT(Dictionary<string, Hyperledger.Aries.Features.PresentProof.ProofAttribute> listado)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"));

            ClaimsIdentity temporal = new  ClaimsIdentity();

            foreach(var atributo in listado)
            {
                temporal.AddClaim(new Claim(atributo.Key, atributo.Value.Raw));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = temporal,
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);

        }
    }
}
