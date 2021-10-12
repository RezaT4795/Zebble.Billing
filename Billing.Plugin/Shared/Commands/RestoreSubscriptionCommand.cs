﻿namespace Zebble.Billing
{
    using System;
    using System.Threading.Tasks;
    using Plugin.InAppBilling;
    using Olive;

    class RestoreSubscriptionCommand : StoreCommandBase<bool>
    {
        protected override async Task<bool> DoExecute()
        {
            return await Fetch(ItemType.Subscription) | await Fetch(ItemType.InAppPurchase);
        }

        async Task<bool> Fetch(ItemType type)
        {
            try
            {
                var purchases = await Billing.GetPurchasesAsync(type)
                                             .Where(x => x.State == PurchaseState.Purchased)
                                             .Distinct(x => x.Id).ToList();

                if (purchases.None()) return false;

                await purchases.ForEachAsync(Environment.ProcessorCount*2,async purchase=>
                {
                    var (result, _) = await BillingContext.Current.ProcessPurchase(purchase);

#if !(CAFEBAZAAR && ANDROID)
                    if (result != PurchaseResult.Succeeded) return;
                    await Billing.AcknowledgePurchaseAsync(purchase.PurchaseToken);
#endif
                });


                return true;
            }
            catch (Exception ex)
            {
                Log.For(this).Error(ex);
                return false;
            }
        }
    }
}