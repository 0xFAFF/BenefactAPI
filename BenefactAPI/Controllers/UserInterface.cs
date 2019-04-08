using BenefactAPI.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PasswordSecurity;
using Replicate;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType]
    [ReplicateRoute(Route = "users")]
    public class UserInterface
    {
        IServiceProvider Services;
        string baseURL;
        string sendKey;
        ILogger logger;
        public UserInterface(IServiceProvider services)
        {
            Services = services;
            logger = services.GetRequiredService<ILogger<UserInterface>>();
            var config = services.GetService<IConfiguration>();
            baseURL = config.GetValue<string>("BaseURL");
            sendKey = config.GetValue<string>("SendKey");
        }
        public Task<string> auth(UserAuthRequest auth)
        {
            if (auth?.Email == null || auth?.Password == null) return null;
            return Services.DoWithDB(async db =>
            {
                var user = await db.Users.Where(u => u.Email == auth.Email).FirstOrDefaultAsync();
                if (user == null || !PasswordStorage.VerifyPassword(auth.Password, user.Hash))
                    throw new HTTPError("Invalid user/pass", 401);
                return Auth.GenerateToken(user);
            });
        }
        [ReplicateIgnore]
        public async Task<string> Add(UserCreateRequest create, bool sendVerification)
        {
            if (create?.Email == null || create?.Password == null) throw new HTTPError("Invalid request", 400);
            var user = await Services.DoWithDB(async db =>
            {
                var _user = (await db.Users.AddAsync(new UserData()
                {
                    Id = 0,
                    Email = create.Email,
                    Name = create.Name,
                    Hash = PasswordStorage.CreateHash(create.Password),
                })).Entity;
                return _user;
            });
            // TODO: Handle this failing and roll back the user add, putting this inside recurses the DB instance
            if (sendVerification)
                await _sendVerification(user).ConfigureAwait(false);
            return Auth.GenerateToken(user);
        }
        public Task<string> Add(UserCreateRequest create) => Add(create, true);
        private async Task _sendVerification(UserData user)
        {
            await Services.DoWithDB(db =>
            {
                db.Attach(user);
                if (user == null) throw new HTTPError("Authentication error", 401);
                user.Nonce = Guid.NewGuid();
                return Task.FromResult(user);
            });
            var client = new SendGridClient(sendKey);
            var from = new EmailAddress($"noreply@{baseURL}", "Benefact - No Reply");
            var subject = "Verify your email address";
            var to = new EmailAddress(user.Email, "Benefact User");
            var htmlContent = await File.ReadAllTextAsync(Path.Combine("Content", "verification.html"));
            htmlContent = htmlContent.Replace("{{link_target}}", $"https://{baseURL}/login?nonce={user.Nonce}");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            msg.AddAttachment(new SendGrid.Helpers.Mail.Attachment()
            {
                Content = "iVBORw0KGgoAAAANSUhEUgAAADwAAABQCAYAAABFyhZTAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH4wMdEg8Xoj7YCAAAAB1pVFh0Q29tbWVudAAAAAAAQ3JlYXRlZCB3aXRoIEdJTVBkLmUHAAACtElEQVR42uXazWpTQRQH8P+cBpS4EFzoyvoEBXcREXwId32FFkHXdudehPoGEnwLu5O+gLhRRKRSpBbUmDRtM11cPxKTe+98nJk5M/fAJcmdcMgvM/frzCg8v61x/TKgNbxjpoH1fvXqG2sKePsd2Lj6DHdfPwZTEL5NgcMJoBTExJoC3v0APv0CZvoR9u7v8oEVgGNB6D/Yz+P537PFhSYAEINewC61sqDp77vU6GYsG5oWPqVCm2FZ0LS0JzbaDuuNppV7Y6HdsF5oqm0JjfbDOqOpsTUUmgfrhKbWb3CjebHWaDJKx4UOg7VCk3E6X3RYrDGarNK5ouNgjdBknc4WHRfbiiandKboNNhGNDmna0OnxdaiyStdHVoGdiWavNPNo0lVGeVgl9A9lnIMABydVKWd0Zk07Dy6r/DqDo+YAByMgZv9p3jwZgdCgzDT8N40gIMJcDQFTvUTDAe7csHex7Cqjt/j6fww3pKKpgBY0WgKhBWLpoBYkWgKjBWHpghYUWiKhBWDpohYEWiKjE2OpgTYpGhKhE2GpoTYJGhKjI2OJgHYqGgSgo2GJkHYKGgShg2OJnwZA18nwLkGzjy3U62Z0S+5wT3c6gODazzrtACFe3u842Q4UNjc13zg9z+r3r1xqapNSQtGbDWkSQEfR8DhicTSaqCzdIfQ/67DHUEv3ml1AL18L104evXTUsHo+ufhQtHNFY8C0e01rcLQZlXLgtDmdelC0HYzDwWg7eeWMke7zR5mjHafH84U7bcCIEN0z/8v+40GqiKCeDDXOq0Po0x6eP0KX7ZzbHenh4FtbO6/KPuklRmWC5wNlgOcFdYXnB3WB5wl1hWcLdYFnDXWFpw91gZcBNYUXAzWBFwUtg1cHLYJXCS2DlwsdhW4aOz/4OKx8+BOYKsYDh6iQ3EBmYL5eYOwqDYAAAAASUVORK5CYII=",
                ContentId = "logo",
                Disposition = "inline",
                Filename = "logo.png",
                Type = "image/png",
            });
            try
            {
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
                if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted))
                    throw new HTTPError($"Failed to send verification email\n{await response.Body.ReadAsStringAsync()}");
            }
            catch (HttpRequestException)
            {
                throw new HTTPError("Failed to send verification");
            }
        }
        [AuthRequired(RequireVerified = false)]
        public async Task SendVerification()
        {
            await _sendVerification(Auth.CurrentUser).ConfigureAwait(false);
        }
        public async Task<bool> Verify(UserVerificationRequest request)
        {
            if (!Guid.TryParse(request.Nonce, out var nonce))
                throw new HTTPError("Failed to parse guid", 400);
            return await Services.DoWithDB(async db =>
            {
                var user = await db.Users.Where(u => u.Id == request.UserId).FirstOrDefaultAsync();
                if (user?.EmailVerified ?? false) return false;
                if (user == null || user.Nonce != nonce) throw new HTTPError("Authentication error", 401);
                user.EmailVerified = true;
                user.Nonce = null;
                return true;
            });
        }
        [AuthRequired]
        public UserData Current()
        {
            return Auth.CurrentUser;
        }
    }
}
