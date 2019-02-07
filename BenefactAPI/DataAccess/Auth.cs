using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class AuthRequiredAttribute : Attribute { }

    public class Auth
    {
        public static AsyncLocal<UserData> CurrentUser = new AsyncLocal<UserData>();
        static readonly byte[] key = Convert.FromBase64String("ufbSRUHVCGWsWa1Ny+7oS8Wj9BB2n8m+DqBnLz8PreKH+ykeStpNLo621d3NnvzJRNJjY5yMPTlTkFpZzmmtpg==");
        public static string GenerateToken(UserData user, int expireMinutes = 20)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireMinutes)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }
        public static string ValidateUserEmail(string token) => ValidateToken(token)?.FindFirst(ClaimTypes.Email)?.Value;
        public static ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
        public static string ValidateUserEmail(HttpRequest request)
        {
            if (request.Headers.ContainsKey("Authorization"))
            {
                var bearer = request.Headers["Authorization"].FirstOrDefault(h => h.Substring(0, 6) == "Bearer");
                if (bearer != null && bearer.Length > 7)
                {
                    var token = bearer.Substring(7, bearer.Length - 7);
                    return ValidateUserEmail(token);
                }
            }
            return null;
        }
    }
}
