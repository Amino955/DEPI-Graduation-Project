namespace TellaStore.Settings;

public class AppSettings
{
    public string StoreName { get; set; } = "طِلّة";
    public string StoreEmail { get; set; } = string.Empty;
    public int LowStockThreshold { get; set; } = 5;
    public int DeliveryCodeLength { get; set; } = 4;
    public int MaxAddressesPerUser { get; set; } = 3;
    public int MaxProductImages { get; set; } = 6;
}
