namespace SmartStock.Shared.Constants;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string StoreManager = "StoreManager";
        public const string Customer = "Customer";
    }

    public static class Series
    {
        public const string InvoicePrefix = "INV";
        public const string OrderPrefix = "ORD";
    }

    public const decimal DefaultTaxRate = 0.18m;
}
