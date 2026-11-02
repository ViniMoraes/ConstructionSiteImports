namespace ConstructionSiteImports;

public static class ConstructionImportMath
{
    public static int MissingAmount(int unreservedConstructionCapacity, int localAndIncomingStock)
        => Math.Max(0, unreservedConstructionCapacity - Math.Max(0, localAndIncomingStock));

    public static int StockAvailableForExport(int stock, int unreservedConstructionCapacity)
        => Math.Max(0, stock - unreservedConstructionCapacity);

    public static int ExportableAmount(bool canExport, int stock, int capacity, float maxExportAmount)
    {
        if (!canExport)
        {
            return 0;
        }

        return capacity > 0
            ? Math.Max(0, Math.Min(stock, (int)Math.Floor(maxExportAmount)))
            : Math.Max(0, stock);
    }

    public static int ExportAmount(int vanillaAmount, bool hasNativeDemand, int missing, int exportable)
    {
        var constructionAmount = Math.Max(0, Math.Min(missing, exportable));
        return hasNativeDemand ? Math.Max(vanillaAmount, constructionAmount) : constructionAmount;
    }

    public static int LimitSourceExport(int amount, bool hasLocalConstruction, int exportable)
        => hasLocalConstruction ? Math.Max(0, Math.Min(amount, exportable)) : amount;
}
