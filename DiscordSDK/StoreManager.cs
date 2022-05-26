namespace DiscordSDK;

public partial class StoreManager {
    public IEnumerable<Entitlement> GetEntitlements() {
        int               count        = this.CountEntitlements();
        List<Entitlement> entitlements = new();
        for (int i = 0; i < count; i++)
            entitlements.Add(this.GetEntitlementAt(i));
        return entitlements;
    }

    public IEnumerable<Sku> GetSkus() {
        int       count = this.CountSkus();
        List<Sku> skus  = new();
        for (int i = 0; i < count; i++)
            skus.Add(this.GetSkuAt(i));
        return skus;
    }
}
