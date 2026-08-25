namespace SmartStock.Shared.Helpers;

public static class NumberSeriesHelper
{
    public static string Generate(string prefix, int year, int sequence)
    {
        return $"{prefix}-{year}-{sequence:D4}";
    }
}
