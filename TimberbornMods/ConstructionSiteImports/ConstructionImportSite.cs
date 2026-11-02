namespace ConstructionSiteImports;

/// <summary>
/// Observes the native construction inventory. It does not create hauling jobs or
/// register extra hauling targets, so other mods can handle that independently.
/// </summary>
[AddTemplateModule2(typeof(ConstructionSite))]
public class ConstructionImportSite(ConstructionImportService service)
    : BaseComponent, IAwakableComponent, IInitializableEntity, IDeletableEntity, IUnfinishedStateListener
{
    public ConstructionSite ConstructionSite { get; private set; } = null!;
    public Inventory Inventory { get; private set; } = null!;
    public BlockObject BlockObject { get; private set; } = null!;
    public DistrictBuilding? DistrictBuilding { get; private set; }
    public ConstructionSiteAccessible? ConstructionSiteAccessible { get; private set; }
    public PausableBuilding? PausableBuilding { get; private set; }
    public bool IsUnfinished { get; private set; }

    public bool IsPaused => PausableBuilding && PausableBuilding!.Paused;

    public void Awake()
    {
        ConstructionSite = GetComponent<ConstructionSite>();
        BlockObject = GetComponent<BlockObject>();
        DistrictBuilding = GetComponent<DistrictBuilding>();
        ConstructionSiteAccessible = GetComponent<ConstructionSiteAccessible>();
        PausableBuilding = GetComponent<PausableBuilding>();
    }

    public void InitializeEntity()
    {
        Inventory = ConstructionSite.Inventory;
        IsUnfinished = BlockObject.IsUnfinished;

        Inventory.InventoryChanged += OnInventoryChanged;
        Inventory.InventoryEnabled += OnInventoryEnabledChanged;
        Inventory.InventoryDisabled += OnInventoryEnabledChanged;
        if (DistrictBuilding)
        {
            DistrictBuilding!.ReassignedConstructionDistrict += OnDistrictChanged;
            DistrictBuilding!.ReassignedDistrict += OnDistrictChanged;
        }

        if (PausableBuilding)
        {
            PausableBuilding!.PausedChanged += OnPausedChanged;
        }

        service.Refresh(this);
    }

    public void OnEnterUnfinishedState()
    {
        IsUnfinished = true;
        service.Refresh(this);
    }

    public void OnExitUnfinishedState()
    {
        IsUnfinished = false;
        service.Unregister(this);
    }

    public void DeleteEntity()
    {
        IsUnfinished = false;
        if (Inventory)
        {
            Inventory.InventoryChanged -= OnInventoryChanged;
            Inventory.InventoryEnabled -= OnInventoryEnabledChanged;
            Inventory.InventoryDisabled -= OnInventoryEnabledChanged;
        }

        if (DistrictBuilding)
        {
            DistrictBuilding!.ReassignedConstructionDistrict -= OnDistrictChanged;
            DistrictBuilding.ReassignedDistrict -= OnDistrictChanged;
        }

        if (PausableBuilding)
        {
            PausableBuilding!.PausedChanged -= OnPausedChanged;
        }

        service.Unregister(this);
    }

    void OnInventoryChanged(object sender, InventoryChangedEventArgs e) => service.Refresh(this);
    void OnInventoryEnabledChanged(object sender, EventArgs e) => service.Refresh(this);
    void OnDistrictChanged(object sender, EventArgs e) => service.Refresh(this);
    void OnPausedChanged(object sender, EventArgs e) => service.Refresh(this);
}
