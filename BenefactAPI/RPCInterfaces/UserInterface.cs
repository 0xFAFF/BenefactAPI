using BenefactAPI.Controllers;
using BenefactAPI.DataAccess;
using BenefactAPI.RPCInterfaces.Board;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using PasswordSecurity;
using Replicate;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BenefactAPI.RPCInterfaces
{
    [ReplicateType]
    public class UserResponse
    {
        public UserData User;
        public List<BoardResponse> Boards;
        public List<CardData> CreatedCards;
        public List<CardData> AssignedCards;
        public List<ActivityData> Activity;
    }
    [ReplicateType]
    [ReplicateRoute(Route = "users")]
    public class UserInterface
    {
        IServiceProvider Services;
        EmailService Email;
        public UserInterface(IServiceProvider services)
        {
            Services = services;
            Email = services.GetService<EmailService>();
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
        public async Task<string> Add(UserCreateRequest create, bool sendVerification = true)
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
            }).HandleDuplicate("ak_users_email", "User already exists");
            // TODO: Handle this failing and roll back the user add, putting this inside recurses the DB instance
            if (sendVerification)
                await _sendVerification(user).ConfigureAwait(false);
            return Auth.GenerateToken(user);
        }
        private async Task<Guid> updateNonce(UserData user)
        {
            return await Services.DoWithDB(db =>
            {
                db.Attach(user);
                user.Nonce = Guid.NewGuid();
                return Task.FromResult(user.Nonce.Value);
            });
        }
        private async Task _sendVerification(UserData user)
        {
            var nonce = await updateNonce(user);
            await Email.SendEmail(user.Email, "Verify your email address", "verification.html",
                new Dictionary<string, string> { { "link_target", $"{{{{baseURL}}}}/login?verify={nonce}" } });
        }
        [AuthRequired(RequireVerified = false)]
        public async Task SendVerification()
        {
            await _sendVerification(Auth.CurrentUser).ConfigureAwait(false);
        }
        [AuthRequired(RequireVerified = false)]
        public async Task<bool> Verify(UserVerificationRequest request)
        {
            if (!Guid.TryParse(request.Nonce, out var nonce))
                throw new HTTPError("Failed to parse guid", 400);
            return await Services.DoWithDB(async db =>
            {
                var user = await db.Users.Where(u => u.Id == Auth.CurrentUser.Id).FirstOrDefaultAsync();
                if (user?.EmailVerified ?? false) return false;
                if (user == null || user.Nonce != nonce) throw new HTTPError("Email verification failed", 401);
                user.EmailVerified = true;
                user.Nonce = null;
                return true;
            });
        }
        [AuthRequired]
        public async Task SendPasswordReset(UserAuthRequest request)
        {
            var user = await Services.DoWithDB(db => db.Users.Where(u => u.Email == request.Email).FirstOr404());
            var nonce = await updateNonce(user);
            await Email.SendEmail(user.Email, "Reset your password", "password_reset.html",
                new Dictionary<string, string> { { "link_target", $"{{{{baseURL}}}}/login?reset={nonce}" } });
        }
        public async Task ChangePassword(ChangePasswordRequest request)
        {
            if (Auth.CurrentUser == null)
            {
                if (!Guid.TryParse(request.Nonce, out var nonce))
                    throw new HTTPError("Failed to parse guid", 400);
                Auth.CurrentUser = await Services.DoWithDB(db => db.Users.Where(u => u.Nonce == nonce).FirstOrError("Unauthorized", 401));
            }
            await Services.DoWithDB(db =>
            {
                db.Attach(Auth.CurrentUser);
                Auth.CurrentUser.Hash = PasswordStorage.CreateHash(request.Password);
                Auth.CurrentUser.Nonce = null;
                return Task.FromResult(true);
            });
        }
        [AuthRequired]
        public async Task<UserResponse> Current()
        {
            return await Services.DoWithDB(async db =>
            {
                var boards = (await db.Roles
                    .Include(r => r.Board)
                    .Include(r => r.Board.Columns)
                    .Include(r => r.Board.Tags)
                    .Where(r => r.UserId == Auth.CurrentUser.Id).ToListAsync())
                    .Select(r =>
                            TypeUtil.CopyFrom(new BoardResponse()
                            {
                                UserPrivilege = r.Privilege,
                                Columns = r.Board.Columns,
                                Tags = r.Board.Tags,
                            }, r.Board))
                    .ToList();
                return new UserResponse()
                {
                    User = Auth.CurrentUser,
                    Boards = boards,
                    CreatedCards = await db.Cards.Where(c => c.AuthorId == Auth.CurrentUser.Id).ToListAsync(),
                    Activity = await db.Activity
                        .OrderByDescending(a => a.Time)
                        .Include(a => a.Comment)
                        .Include(a => a.Card)
                        .Where(a => a.UserId == Auth.CurrentUser.Id).ToListAsync(),
                };
            });
        }
    }
}
