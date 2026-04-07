namespace Bookify.Models.Constants
{
    /// <summary>
    /// Centralized magic strings for subscription statuses, plan codes, and roles.
    /// Use these constants instead of raw string literals throughout the codebase.
    /// </summary>
    public static class AppConstants
    {
        public static class SubscriptionStatus
        {
            public const string Active = "active";
            public const string Expired = "expired";
            public const string Paused = "paused";
            public const string Cancelled = "cancelled";
        }

        public static class PlanCode
        {
            public const string FreeMonthly = "FreeMonthly";
            public const string PremiumMonthly = "PremiumMonthly";
        }

        public static class PaymentStatus
        {
            public const string Paid = "paid";
            public const string Pending = "pending";
            public const string Failed = "failed";
        }

        public static class PaymentMethod
        {
            public const string Stripe = "stripe";
        }

        public static class Currency
        {
            public const string USD = "USD";
        }

        public static class FileFormat
        {
            public const string Pdf = "pdf";
            public const string Mp3 = "mp3";
        }

        public static class ChangedBy
        {
            public const string User = "User";
            public const string System = "System";
        }
    }
}
