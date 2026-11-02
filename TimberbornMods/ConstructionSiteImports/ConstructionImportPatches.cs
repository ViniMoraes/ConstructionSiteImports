namespace ConstructionSiteImports;

[HarmonyPatch]
public static class ConstructionImportPatches
{
    [HarmonyPostfix, HarmonyPatch(typeof(ConstructionSiteHaulerService), nameof(ConstructionSiteHaulerService.Refresh))]
    public static void RefreshSite(ConstructionSiteHaulerComponent site)
        => ConstructionImportService.Instance?.Refresh(site);

    [HarmonyPostfix, HarmonyPatch(typeof(ConstructionSiteHaulerService), nameof(ConstructionSiteHaulerService.Unregister))]
    public static void UnregisterSite(ConstructionSiteHaulerComponent site)
        => ConstructionImportService.Instance?.Unregister(site);

    // Apply after the original cache lookup, without caching construction demand.
    // New/pause/cancel/reservation changes therefore take effect on the next query.
    [HarmonyPostfix, HarmonyPatch(typeof(DistrictDistributableGoodProvider), nameof(DistrictDistributableGoodProvider.GetImportableGood))]
    public static void AddConstructionDemand(DistrictDistributableGoodProvider __instance, string goodId, ref ImportableGood __result)
    {
        if (ConstructionImportService.Instance is not { } service
            || !service.TryGetDemand(__instance, goodId, out var demand)
            || demand.Missing <= 0)
        {
            return;
        }

        var capacity = Math.Max(__instance.GetCapacity(demand.Setting), demand.Stock + demand.Missing);
        __result = ImportableGood.CreateImportableWithCapacity(new DistributableGood(demand.Stock, capacity, demand.Setting));
    }

    [HarmonyPostfix, HarmonyPatch(typeof(DistrictDistributableGoodProvider), nameof(DistrictDistributableGoodProvider.GetDistributableGoodForExport))]
    public static void KeepMaterialsForLocalConstruction(DistrictDistributableGoodProvider __instance, string goodId, ref DistributableGood __result)
    {
        if (ConstructionImportService.Instance is not { } service
            || !service.TryGetDemand(__instance, goodId, out var demand)
            || demand.Remaining <= 0)
        {
            return;
        }

        // Crossing inventories are exportable too. Without protecting this stock,
        // a warehouse-less district can send its newly imported materials back
        // before a local hauler has time to reserve them for the construction site.
        var stock = ConstructionImportMath.StockAvailableForExport(__result.Stock, demand.Remaining);
        __result = new DistributableGood(stock, __result.Capacity, demand.Setting);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(DistrictCrossing), nameof(DistrictCrossing.CanExportGood))]
    public static void CanExportForConstruction(
        DistrictCrossing __instance,
        DistributableGood myDistributableGood,
        DistributableGood linkedDistributableGood,
        ref bool __result)
    {
        if (!TryGetDemand(__instance, linkedDistributableGood.GoodId, out var demand))
        {
            return;
        }

        var canSupply = demand.Missing > 0 && ExportableAmount(myDistributableGood) > 0;
        __result = demand.HasNativeDemand ? __result || canSupply : canSupply;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(DistrictCrossing), nameof(DistrictCrossing.GetAmountToExport))]
    public static void LimitConstructionImport(
        DistrictCrossing __instance,
        DistributableGood myDistributableGood,
        DistributableGood linkedDistributableGood,
        ref int __result)
    {
        var exportable = ExportableAmount(myDistributableGood);
        if (TryGetDemand(__instance, linkedDistributableGood.GoodId, out var demand))
        {
            // Do not use fill-rate equalization for construction-only demand:
            // flooring can otherwise leave the final item missing forever.
            // Read live demand to account for other crossings' reservations.
            __result = ConstructionImportMath.ExportAmount(
                __result, demand.HasNativeDemand, demand.Missing, exportable);
        }

        if (ConstructionImportService.Instance is { } service
            && service.TryGetDemand(__instance.DistrictDistributableGoodProvider, myDistributableGood.GoodId, out var ownDemand))
        {
            // With no source warehouse, vanilla ignores source Stock and asks
            // for the receiver's entire free capacity. Apply the protected source
            // stock limit even if the receiver has only ordinary warehouse jobs.
            __result = ConstructionImportMath.LimitSourceExport(__result, ownDemand.Remaining > 0, exportable);
        }
    }

    static int ExportableAmount(DistributableGood good)
        => ConstructionImportMath.ExportableAmount(good.CanExport, good.Stock, good.Capacity, good.MaxExportAmount);

    static bool TryGetDemand(DistrictCrossing crossing, string goodId, out ConstructionImportDemand demand)
    {
        demand = default;
        return crossing._linked
            && ConstructionImportService.Instance is { } service
            && service.TryGetDemand(crossing._linked.DistrictDistributableGoodProvider, goodId, out demand);
    }
}
