using System;

namespace StageGame
{
    public enum PurchaseResult { Completed, Cancelled, Failed, Pending, NothingToRestore }

    // Platform adapters must expose only server-verified entitlement. A purchase callback alone
    // never grants ownership. Product readiness includes current price and permission to sell.
    public interface IAdRemovalPurchases
    {
        bool ProductReady { get; }
        bool EntitlementChecked { get; }
        bool OwnsAdRemoval { get; }
        string DisplayPrice { get; }
        void Purchase(Action<PurchaseResult> callback);
        void Restore(Action<PurchaseResult> callback);
    }

    // Pages and the ad-free first release have no live IAP integration.
    public sealed class UnavailablePurchases : IAdRemovalPurchases
    {
        public bool ProductReady => false;
        public bool EntitlementChecked => false;
        public bool OwnsAdRemoval => false;
        public string DisplayPrice => "";
        public void Purchase(Action<PurchaseResult> callback) => callback(PurchaseResult.Failed);
        public void Restore(Action<PurchaseResult> callback) => callback(PurchaseResult.Failed);
    }
}
