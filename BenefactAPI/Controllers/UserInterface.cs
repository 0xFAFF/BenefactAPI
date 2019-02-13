using BenefactAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using PasswordSecurity;
using Replicate;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI.Controllers
{
    [ReplicateType(AutoMethods = AutoAdd.AllPublic)]
    public class UserInterface
    {
        IServiceProvider Services;
        public UserInterface(IServiceProvider services)
        {
            Services = services;
        }
        public Task<string> AuthUser(UserAuthRequest auth)
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
        public Task<bool> CreateUser(UserCreateRequest create)
        {
            if (create?.Email == null || create?.Password == null) return null;
            return Services.DoWithDB(async db =>
            {
                await db.Users.AddAsync(new UserData()
                {
                    Email = create.Email,
                    Name = create.Name,
                    Hash = PasswordStorage.CreateHash(create.Password),
                });
                await db.SaveChangesAsync();
                return true;
            });
        }
        [AuthRequired]
        public UserData CurrentUser()
        {
            return Auth.CurrentUser.Value;
        }
    }
}
