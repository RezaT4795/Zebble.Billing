﻿namespace Zebble.Billing
{
    using System;

    public class SubscriptionInfo
    {
        public string UserId { get; set; }
        public string TransactionId { get; set; }
        public DateTime? SubscriptionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? CancellationDate { get; set; }
        public bool AutoRenews { get; set; }
    }
}
