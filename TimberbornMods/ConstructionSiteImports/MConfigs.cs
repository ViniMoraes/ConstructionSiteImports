namespace ConstructionSiteImports;

public class MConfigs : BaseModdableTimberbornAttributeConfiguration, IHarmonyPatchAll
{
    public override ConfigurationContext AvailableContexts => ConfigurationContext.Game;
}
