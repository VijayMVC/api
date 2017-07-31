using Cllearworks.COH.Utility;
using Microsoft.AspNet.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cllearworks.COH.Web.Utility.Auth
{
    public class COHUserManager : UserManager<COHApplicationUser, string>
    {
        public COHUserManager()
			: this( new COHUserStore() )
		{
        }

        public COHUserManager(IUserStore<COHApplicationUser, string> store)
			: base( store )
		{
            if (store == null)
                throw new ArgumentNullException("store");

            UserValidator = new UserValidator<COHApplicationUser>(this);
            PasswordValidator = new MinimumLengthValidator(6);
            PasswordHasher = new PasswordHasher();
            ClaimsIdentityFactory = new COHClaimsIdentityFactory();
        }

        public override async Task<COHApplicationUser> FindAsync(string userName, string password)
        {
            var passwordStore = GetPasswordStore();

            if (userName == null)
                throw new ArgumentNullException("userName");

            var user = await FindByNameAsync(userName);
            COHApplicationUser user1;

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.Salt))
                user1 = default(COHApplicationUser);
            else
                user1 = await VerifyPassword(passwordStore, user, password) ? user : default(COHApplicationUser);

            return user1;
        }


        private COHUserStore GetPasswordStore()
        {
            var userPasswordStore = Store as COHUserStore;

            if (userPasswordStore == null)
                throw new NotSupportedException("");
            else
                return userPasswordStore;
        }

        internal async Task<bool> VerifyPassword(COHUserStore store, COHApplicationUser user, string password)
        {
            var hash = await store.GetPasswordHashAsync(user);
            var verify = PasswordHelpers.GenerateHashForSaltAndPassword(user.Salt, password);
            return hash == verify;
        }

        public override Task<ClaimsIdentity> CreateIdentityAsync(COHApplicationUser user, string authenticationType)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            else
                return ClaimsIdentityFactory.CreateAsync(this, user, authenticationType);
        }
    }
}
