﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Olive;

    public class SubscriptionManager
    {
        readonly ISubscriptionRepository repository;
        readonly IPlatformProvider platformProvider;

        public SubscriptionManager(ISubscriptionRepository repository, IPlatformProvider platformProvider)
        {
            this.repository = repository;
            this.platformProvider = platformProvider;
        }

        public async Task InitiatePurchase(string productId, string userId, string platform, string purchaseToken)
        {
            var subscription = await repository.GetByPurchaseToken(purchaseToken);

            if (subscription != null)
            {
                if (subscription.ProductId != productId)
                    throw new Exception("Provided purchase token is associated with another product!");

                if (subscription.UserId != userId)
                    throw new Exception("Provided purchase token is associated with another user!");

                if (subscription.Platform != platform)
                    throw new Exception("Provided purchase token is associated with another platform!");

                return;
            }

            await repository.AddSubscription(new Subscription
            {
                SubscriptionId = Guid.NewGuid().ToString(),
                ProductId = productId,
                UserId = userId,
                Platform = platform,
                PurchaseToken = purchaseToken,
                LastUpdated = LocalTime.Now
            });
        }

        public async Task<Subscription> GetSubscriptionStatus(string userId)
        {
            var subscription = await repository.GetMostUpdatedByUserId(userId);

            await TryUpdateSubscription(subscription);

            return subscription;
        }

        async Task TryUpdateSubscription(Subscription subscription)
        {
            var updatedSubscription = await platformProvider.GetUpToDateInfo(subscription.ProductId, subscription.PurchaseToken);

            if (updatedSubscription == null) return;

            subscription.ExpiryDate = updatedSubscription.ExpiryDate;
            subscription.CancellationDate = updatedSubscription.CancellationDate;
            subscription.AutoRenews = updatedSubscription.AutoRenews;

            await repository.UpdateSubscription(subscription);
        }
    }
}
