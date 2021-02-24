﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Olive;

    partial class BillingContext
    {
        public async Task<string> PurchaseSubscription(string productId)
        {
            var product = await ProductProvider.GetById(productId) ?? throw new Exception($"Product with id '{productId}' not found.");
            return await new PurchaseSubscriptionCommand(product).Execute()
                 ?? "Failed to connect to the store. Are you connected to the network? If so, try 'Pay with Card'.";
        }

        public async Task<bool> RestoreSubscription(bool userRequest = false)
        {
            var errorMessage = "";
            try { await new RestoreSubscriptionCommand().Execute(); }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Log.For(typeof(BillingContext)).Error(ex);
            }

            var successful = false;
            try
            {
                await Refresh();
                successful = IsSubscribed;
            }
            catch (Exception ex)
            {
                if (errorMessage.IsEmpty()) errorMessage = ex.Message;
                Log.For(typeof(BillingContext)).Error(ex);
            }

            if (!successful && userRequest)
            {
                if (errorMessage.IsEmpty()) errorMessage = "Unable to find an active subscription.";
                await Alert.Show(errorMessage);
            }

            return successful;
        }

        internal async Task<bool> VerifyPurchase(VerifyPurchaseEventArgs args)
        {
            if (await UIContext.IsOffline())
            {
                await Alert.Show("Network connection is not available.");
                return false;
            }

            if (User == null)
            {
                await Alert.Show("User is not available.");
                return false;
            }

            try
            {
                var url = new Uri(Options.BaseUri, Options.VerifyPurchasePath).ToString();
                var @params = new { User.Ticket, User.UserId, Platform = PaymentAuthority, args.ProductId, args.TransactionId, args.ReceiptData };
                var result = await BaseApi.Post(url, @params, OnError.Ignore, showWaiting: false);

                return result;
            }
            catch { return false; }
        }

        internal async Task PurchaseAttempt(SubscriptionPurchasedEventArgs args)
        {
            if (await UIContext.IsOffline())
            {
                await Alert.Show("Network connection is not available.");
                return;
            }

            if (User == null)
            {
                await Alert.Show("User is not available.");
                return;
            }

            try
            {
                var url = new Uri(Options.BaseUri, Options.PurchaseAttemptPath).ToString();
                var @params = new { User.Ticket, User.UserId, Platform = PaymentAuthority, args.ProductId, args.TransactionId, args.TransactionDateUtc, args.PurchaseToken };
                await BaseApi.Post(url, @params, OnError.Ignore, showWaiting: false);

                await SubscriptionPurchased.Raise(args);
            }
            catch { }
        }
    }
}
