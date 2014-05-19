﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SaasEcom.Data.DataServices.Interfaces;
using SaasEcom.Data.Models;

namespace SaasEcom.Data.DataServices.Storage
{
    public class AccountDataService : IAccountDataService
    {
        private ApplicationDbContext DbContext { get; set; }

        public AccountDataService(ApplicationDbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        public async Task<ApplicationUser> GetUserAsync(string userId)
        {
            return await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<StripeAccount> GetStripeAccountAsync(string userId)
        {
            return await DbContext.StripeAccounts.FirstOrDefaultAsync(
                stripeAccount => stripeAccount.ApplicationUser.Id == userId);
        }

        public async Task AddOrUpdateStripeAccountAsync(StripeAccount stripeAccount)
        {
            StripeAccount sa = await DbContext.Users.Include(u => u.StripeAccount)
                .Where(u => u.Id == stripeAccount.ApplicationUser.Id).Select(u => u.StripeAccount).FirstOrDefaultAsync();

            if (sa == null)
            {
                var user = await DbContext.Users.FirstAsync(u => u.Id == stripeAccount.ApplicationUser.Id);
                user.StripeAccount = stripeAccount;
            }
            else
            {
                sa.LiveMode = stripeAccount.LiveMode;
                sa.StripeLivePublicApiKey = stripeAccount.StripeLivePublicApiKey;
                sa.StripeLiveSecretApiKey = stripeAccount.StripeLiveSecretApiKey;
                sa.StripeTestPublicApiKey = stripeAccount.StripeTestPublicApiKey;
                sa.StripeTestSecretApiKey = stripeAccount.StripeTestSecretApiKey;
            }

            await DbContext.SaveChangesAsync();
        }
    }
}
