namespace ConstructionSiteImports;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IHarmonyPatchAll
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;

    protected override void ConfigureRegistry(ModdableTimberbornRegistry registry)
        => registry
            .UseEntityTracker()
            .TryTrack<ConstructionImportSite>();
}
