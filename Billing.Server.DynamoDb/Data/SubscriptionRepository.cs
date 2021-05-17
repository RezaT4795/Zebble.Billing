﻿namespace Zebble.Billing
{
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;
    using Olive;

    class SubscriptionRepository : ISubscriptionRepository
    {
        readonly SubscriptionDbContext Context;

        public SubscriptionRepository(SubscriptionDbContext context) => Context = context;

        public async Task<Subscription> GetByTransactionId(string transactionId)
        {
            return await Context.SubscriptionTransactions.FirstOrDefault(transactionId);
        }

        public async Task<Subscription> GetByPurchaseToken(string purchaseToken)
        {
            var hash = purchaseToken.ToSimplifiedSHA1Hash();
            return await Context.SubscriptionPurchaseTokenHashes.FirstOrDefault(hash);
        }

        public async Task<Subscription[]> GetAll(string userId)
        {
            return await Context.SubscriptionUsers.All(userId);
        }

        public async Task<Subscription> AddSubscription(Subscription subscription)
        {
            await Context.Subscriptions.AddAsync(new SubscriptionProxy(subscription));

            return subscription;
        }

        public Task UpdateSubscription(Subscription subscription)
        {
            return Context.Subscriptions.UpdateAsync(x => x.Id, new SubscriptionProxy(subscription));
        }

        public async Task<Transaction> AddTransaction(Transaction transaction)
        {
            await Context.Transactions.AddAsync(new TransactionProxy(transaction));

            return transaction;
        }

        public async Task<string> GetOriginUserOfTransactionIds(string[] transactionIds)
        {
            var conditions = transactionIds.Select(x => new ScanCondition(nameof(Subscription.TransactionId), ScanOperator.Equal, x)).ToArray();
            var subscriptions = (await Context.Subscriptions.All(conditions)).OrderBy(x => x.ExpirationDate);

            return subscriptions.Where(x => x.IsStarted())
                                .Where(x => !x.IsCanceled())
                                .Where(x => !x.IsExpired())
                                .Select(x => x.UserId)
                                .FirstOrDefault(x => x.HasValue());
        }
    }
}
