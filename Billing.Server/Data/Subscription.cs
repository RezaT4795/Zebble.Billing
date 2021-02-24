﻿namespace Zebble.Billing
{
    using System;

    public partial class Subscription
    {
        public string SubscriptionId { get; set; }

        public string UserId { get; set; }

        public string Platform { get; set; }

        public string TransactionId { get; set; }
        public string ReceiptData { get; set; }
        public string PurchaseToken { get; set; }

        public DateTime? LastUpdate { get; set; }

        public bool? AutoRenews { get; set; }
    }
}
