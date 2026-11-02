namespace ConstructionSiteImports;

/// <summary>
/// Assigns one importer to each reachable native construction site. Materials use
/// vanilla district crossings and are collected by builders or compatible haulers.
/// </summary>
[BindSingleton]
public class ConstructionImportService(
    DefaultEntityTracker<ConstructionImportSite> sites,
    HaulingTargetHelper haulingTargetHelper,
    EventBus eventBus
) : ILoadableSingleton, IUnloadableSingleton, ISingletonInstantNavMeshListener
{
    public static ConstructionImportService? Instance { get; private set; }

    readonly Dictionary<ConstructionImportSite, DistrictCenter> importDistricts = [];
    readonly Dictionary<DistrictCenter, HashSet<ConstructionImportSite>> sitesByDistrict = [];
    readonly Dictionary<DistrictCenter, HashSet<string>> constructionGoods = [];

    public void Load()
    {
        Instance = this;
        eventBus.Register(this);
        RefreshAll();
    }

    public void OnInstantNavMeshUpdated(NavMeshUpdate navMeshUpdate) => RefreshAll();

    [OnEvent]
    public void OnDistrictCenterRegistryChanged(DistrictCenterRegistryChangedEvent e) => RefreshAll();

    public void RefreshAll()
    {
        foreach (var site in sites.Entities)
        {
            Refresh(site);
        }
    }

    public void Unload()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        importDistricts.Clear();
        sitesByDistrict.Clear();
        constructionGoods.Clear();
    }

    public void Refresh(ConstructionImportSite site)
    {
        Unregister(site);
        if (!site || !site.IsUnfinished || !site.Inventory
            || !site.Inventory.Enabled || site.Inventory.IsFull
            || !site.ConstructionSiteAccessible)
        {
            return;
        }

        var accessible = site.ConstructionSiteAccessible!.Accessible;
        if (!accessible)
        {
            return;
        }

        // Match builder road access, including sites without a building entrance.
        // Query the shared helper directly; no hauling registration is required.
        var districts = haulingTargetHelper.FindDistrictsFor(accessible, ignoreEntrance: true).ToArray();
        var district = FindImportDistrict(site, districts);
        if (!district)
        {
            return;
        }

        importDistricts[site] = district!;
        if (!sitesByDistrict.TryGetValue(district!, out var districtSites))
        {
            districtSites = [];
            sitesByDistrict[district!] = districtSites;
            constructionGoods[district!] = [];
        }

        districtSites.Add(site);
        // Keep seen goods until unload: a stale crossing job must be limited to
        // zero after its final construction site is paused, removed or completed.
        foreach (var goodId in site.Inventory.InputGoods)
        {
            constructionGoods[district!].Add(goodId);
        }
    }

    public void Unregister(ConstructionImportSite site)
    {
        if (!importDistricts.TryGetValue(site, out var district))
        {
            return;
        }

        importDistricts.Remove(site);
        sitesByDistrict[district].Remove(site);
    }

    public bool TryGetDemand(DistrictDistributableGoodProvider provider, string goodId, out ConstructionImportDemand demand)
    {
        demand = default;
        if (!provider || provider.GetGoodImportOption(goodId) != ImportOption.Auto)
        {
            return false;
        }

        var district = provider.GetComponent<DistrictCenter>();
        if (!district || !constructionGoods.TryGetValue(district, out var goods) || !goods.Contains(goodId))
        {
            return false;
        }

        var remaining = 0;
        foreach (var site in sitesByDistrict[district])
        {
            var inventory = site.Inventory;
            if (site && site.IsUnfinished && inventory && inventory.Enabled && inventory.IsUnblocked
                && !site.IsPaused && site.ConstructionSite && site.ConstructionSite.IsOn
                && inventory.InputGoods.Contains(goodId))
            {
                remaining += inventory.UnreservedCapacity(goodId);
            }
        }

        // Vanilla includes crossing reservations and incoming warehouse deliveries.
        // Construction reservations have already been removed from 'remaining'.
        var stock = provider.GetStock(goodId, withDistrictCrossingIncomingStock: true);
        var setting = provider._districtDistributionSetting.GetGoodDistributionSetting(goodId);
        demand = new(
            setting,
            stock,
            remaining,
            ConstructionImportMath.MissingAmount(remaining, stock),
            provider.HasUnreservedCapacity(setting, out _));
        return true;
    }

    static DistrictCenter? FindImportDistrict(ConstructionImportSite site, IReadOnlyList<DistrictCenter> districts)
    {
        var assigned = site.DistrictBuilding
            ? site.DistrictBuilding!.GetDistrictOrConstructionDistrict()
            : null;
        if (assigned && districts.Contains(assigned))
        {
            return assigned;
        }

        var coordinates = site.BlockObject.Coordinates;
        return districts
            .Where(d => d)
            .OrderBy(d => (d.GetComponent<BlockObject>().Coordinates - coordinates).sqrMagnitude)
            .ThenBy(d => d.GetComponent<BlockObject>().Coordinates.x)
            .ThenBy(d => d.GetComponent<BlockObject>().Coordinates.y)
            .ThenBy(d => d.GetComponent<BlockObject>().Coordinates.z)
            .FirstOrDefault();
    }
}

public readonly record struct ConstructionImportDemand(
    GoodDistributionSetting Setting,
    int Stock,
    int Remaining,
    int Missing,
    bool HasNativeDemand);
