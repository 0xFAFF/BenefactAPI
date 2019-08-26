using BenefactAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace BenefactAPI.DataAccess
{
    public class AuthRequiredAttribute : RPCMiddlewareAttribute
    {
        public bool RequireVerified = true;
        public Privilege RequirePrivilege = 0;
        public override Task Run(HttpContext services)
        {
            Auth.ThrowIfUnauthorized(RequireVerified, RequirePrivilege);
            return Task.FromResult(true);
        }
    }

    public static class Auth
    {
        private static AsyncLocal<UserData> _currentUser = new AsyncLocal<UserData>();
        public static UserData CurrentUser { get => _currentUser.Value; set => _currentUser.Value = value; }
        private static AsyncLocal<UserRole> _currentRole = new AsyncLocal<UserRole>();
        public static UserRole CurrentRole { get => _currentRole.Value; set => _currentRole.Value = value; }

        static readonly byte[] key = Convert.FromBase64String("ufbSRUHVCGWsWa1Ny+7oS8Wj9BB2n8m+DqBnLz8PreKH+ykeStpNLo621d3NnvzJRNJjY5yMPTlTkFpZzmmtpg==");
        public static string GenerateToken(UserData user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("name", user.Name),
                    new Claim("id", user.Id.ToString(), ClaimValueTypes.Integer64),
                }),
                Expires = DateTime.UtcNow.AddDays(28),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }
        public static string ValidateUserEmail(string token) => ValidateToken(token)?.FindFirstValue(ClaimTypes.Email);
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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
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
        public static async Task<UserData> Authenticate(HttpRequest request, IServiceProvider services)
        {
            string token = null;
            if (request.Headers.ContainsKey("Authorization"))
            {
                var bearer = request.Headers["Authorization"].FirstOrDefault(h => h.Substring(0, 6) == "Bearer");
                if (bearer != null && bearer.Length > 7)
                    token = bearer.Substring(7, bearer.Length - 7);
            }
            if (token == null)
                request.Cookies.TryGetValue("token", out token);
            if (token != null)
            {
                var email = ValidateUserEmail(token);
                if (email != null)
                    return await GetUser(services, email);
            }
            return null;
        }
        public static async Task<UserData> GetUser(IServiceProvider services, string email)
        {
            return await services.DoWithDB(async db => await db.Users
                // TODO: Doing this include might be expensive?
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Email == email));
        }
        public static void VerifyPrivilege(Privilege privilege)
        {
            if (BoardExtensions.Board == null)
                throw new InvalidOperationException("Cannot check privileges without board being set");
            var userPrivilege = CurrentUser.Roles.FirstOrDefault(up => up.BoardId == BoardExtensions.Board.Id)?.Privilege ?? Privilege.None;
            if((userPrivilege & (privilege | Privilege.Admin)) == 0)
                throw new HTTPError("Insufficient privilege", 403);
        }
        public static void ThrowIfUnauthorized(bool requireVerified = true, Privilege privilege = 0)
        {
            if (CurrentUser == null)
                throw new HTTPError("Unauthorized", 401);
            if (requireVerified && !CurrentUser.EmailVerified)
                throw new HTTPError("Email address unverified", 403);
            if (privilege != 0)
                VerifyPrivilege(privilege);
        }
        public static IApplicationBuilder UseAuthn(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                _currentUser.Value = await Authenticate(context.Request, context.RequestServices);
                await next();
            });
        }
    }
}
