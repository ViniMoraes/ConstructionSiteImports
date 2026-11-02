using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using ConstructionSiteImports;

var passed = 0;
var failed = 0;

void Check(string name, Action test)
{
    try
    {
        test();
        passed++;
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception error)
    {
        failed++;
        Console.WriteLine($"FAIL {name}: {error.GetBaseException().Message}");
    }
}

void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected {expected}, got {actual}.");
    }
}

Check("Paper20WithoutWarehouse", () => Equal(20, ConstructionImportMath.ExportAmount(0, false,
    ConstructionImportMath.MissingAmount(20, 0), ConstructionImportMath.ExportableAmount(true, 100, 100, 80))));
Check("LocalStockReducesDeficit", () => Equal(13, ConstructionImportMath.MissingAmount(20, 7)));
Check("PartialIncomingReservation", () => Equal(8, ConstructionImportMath.MissingAmount(20, 12)));
Check("FullIncomingReservation", () => Equal(0, ConstructionImportMath.MissingAmount(20, 20)));
Check("OverfullIncomingReservation", () => Equal(0, ConstructionImportMath.MissingAmount(20, 30)));
Check("ConstructionReservationAlreadyRemoved", () => Equal(3, ConstructionImportMath.MissingAmount(8, 5)));
Check("NegativeStockCannotIncreaseDemand", () => Equal(20, ConstructionImportMath.MissingAmount(20, -5)));
Check("NoRemainingConstruction", () => Equal(0, ConstructionImportMath.MissingAmount(0, 20)));
Check("ExportDisabled", () => Equal(0, ConstructionImportMath.ExportableAmount(false, 100, 100, 80)));
Check("ExportThreshold", () => Equal(30, ConstructionImportMath.ExportableAmount(true, 100, 100, 30)));
Check("FractionalThresholdFloors", () => Equal(3, ConstructionImportMath.ExportableAmount(true, 100, 100, 3.9f)));
Check("ThresholdCannotExceedStock", () => Equal(4, ConstructionImportMath.ExportableAmount(true, 4, 100, 30)));
Check("ThresholdZero", () => Equal(0, ConstructionImportMath.ExportableAmount(true, 100, 100, 0)));
Check("ThresholdNegative", () => Equal(0, ConstructionImportMath.ExportableAmount(true, 100, 100, -3)));
Check("ZeroCapacityCrossingCanSupply", () => Equal(12, ConstructionImportMath.ExportableAmount(true, 12, 0, 0)));
Check("ZeroCapacityStillHonorsExportDisabled", () => Equal(0, ConstructionImportMath.ExportableAmount(false, 12, 0, 0)));
Check("NegativeStockCannotExport", () => Equal(0, ConstructionImportMath.ExportableAmount(true, -1, 0, 0)));
Check("FinalItemOverridesVanillaFloor", () => Equal(1, ConstructionImportMath.ExportAmount(0, false, 1, 100)));
Check("BoundedByRemainingDemand", () => Equal(20, ConstructionImportMath.ExportAmount(50, false, 20, 100)));
Check("BoundedByExporterSupply", () => Equal(4, ConstructionImportMath.ExportAmount(50, false, 20, 4)));
Check("StaleDemandClampsToZero", () => Equal(0, ConstructionImportMath.ExportAmount(50, false, 0, 100)));
Check("NegativeStaleDemandClampsToZero", () => Equal(0, ConstructionImportMath.ExportAmount(50, false, -10, 100)));
Check("NativeWarehouseImportPreserved", () => Equal(50, ConstructionImportMath.ExportAmount(50, true, 20, 100)));
Check("NativeImportAndLargerConstruction", () => Equal(20, ConstructionImportMath.ExportAmount(5, true, 20, 100)));
Check("NativeImportPreservedAfterConstruction", () => Equal(50, ConstructionImportMath.ExportAmount(50, true, 0, 100)));
Check("NoSupplyNoConstructionImport", () => Equal(0, ConstructionImportMath.ExportAmount(0, false, 20, 0)));
Check("RetainAllImportedConstructionStock", () => Equal(0, ConstructionImportMath.StockAvailableForExport(20, 20)));
Check("RetainPartialImportedStock", () => Equal(0, ConstructionImportMath.StockAvailableForExport(8, 20)));
Check("ExportConstructionSurplus", () => Equal(5, ConstructionImportMath.StockAvailableForExport(25, 20)));
Check("ReleaseStockAfterConstruction", () => Equal(20, ConstructionImportMath.StockAvailableForExport(20, 0)));
Check("CapZeroSourceClampedToSurplus", () => Equal(5, ConstructionImportMath.LimitSourceExport(1000, true, 5)));
Check("CapZeroSourceFullyRetained", () => Equal(0, ConstructionImportMath.LimitSourceExport(1000, true, 0)));
Check("SourceClampKeepsSmallerRequest", () => Equal(3, ConstructionImportMath.LimitSourceExport(3, true, 5)));
Check("SourceWithoutConstructionKeepsVanilla", () => Equal(1000, ConstructionImportMath.LimitSourceExport(1000, false, 5)));

// Use an isolated resolver so the default mode cannot accidentally satisfy a
// ConstructionSiteHauler reference from the test host or Workshop installation.
// Only plain managed distribution records are instantiated below; no Unity
// components, Harmony patch application, game simulation or native APIs run.
var taskWork = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../.."));
var withHauler = args.Contains("--with-hauler", StringComparer.OrdinalIgnoreCase);
var steamApps = args.FirstOrDefault(argument => !argument.StartsWith("--")) ?? @"D:\Jogos\Steam\steamapps";
var managed = Path.Combine(steamApps, "common/Timberborn/Timberborn_Data/Managed");
var workshop = Path.Combine(steamApps, "workshop/content/1062090");
var addonPath = Path.Combine(taskWork, "staging/ConstructionSiteImports/version-1.1/ConstructionSiteImports.dll");
var haulerPath = Path.Combine(workshop, "3779332547/version-1.1/ConstructionSiteHauler.dll");
var libraryPaths = new List<string>
{
    managed,
    Path.Combine(workshop, "3284904751"),
    Path.Combine(workshop, "3578009707/version-1.1"),
    Path.Combine(workshop, "3430626620/version-1.1")
};
if (withHauler)
{
    libraryPaths.Add(Path.GetDirectoryName(haulerPath)!);
}
var libraries = libraryPaths.SelectMany(path => Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories))
    .GroupBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
    .ToDictionary(group => group.Key!, group => group.First(), StringComparer.OrdinalIgnoreCase);

var loadContext = new AddonLoadContext(libraries, allowHauler: withHauler);
var addon = loadContext.LoadFromAssemblyPath(addonPath);
var patchClass = addon.GetType("ConstructionSiteImports.ConstructionImportPatches", throwOnError: true)!;
var allMethods = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
var expectedNames = new[]
{
    "AddConstructionDemand", "KeepMaterialsForLocalConstruction",
    "CanExportForConstruction", "LimitConstructionImport"
};

Check("OnlyFourNativeDistributionPatchesPresent", () => Equal(4, patchClass.GetMethods(allMethods)
    .Count(method => method.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPostfix"))));

Check("NoConstructionSiteHaulerAssemblyDependency", () => Equal(false, addon.GetReferencedAssemblies()
    .Any(reference => reference.Name!.StartsWith("ConstructionSiteHauler", StringComparison.OrdinalIgnoreCase))));

foreach (var name in expectedNames)
{
    Check($"RealTargetAndParameters_{name}", () =>
    {
        var patch = patchClass.GetMethod(name, allMethods) ?? throw new MissingMethodException(name);
        var targetAttribute = patch.CustomAttributes.Single(attribute => attribute.AttributeType.FullName == "HarmonyLib.HarmonyPatch");
        var targetType = (Type)targetAttribute.ConstructorArguments[0].Value!;
        Equal(true, targetType.Assembly.GetName().Name!.StartsWith("Timberborn.", StringComparison.Ordinal));
        var targetName = (string)targetAttribute.ConstructorArguments[1].Value!;
        var targets = targetType.GetMethods(allMethods).Where(method => method.Name == targetName).ToArray();
        Equal(1, targets.Length);
        var target = targets.Single();
        foreach (var parameter in patch.GetParameters())
        {
            if (parameter.Name == "__instance")
            {
                Equal(target.DeclaringType, parameter.ParameterType);
                Equal(false, target.IsStatic);
            }
            else if (parameter.Name == "__result")
            {
                Equal(true, parameter.ParameterType.IsByRef);
                Equal(target.ReturnType, parameter.ParameterType.GetElementType());
            }
            else
            {
                var original = target.GetParameters().Single(candidate => candidate.Name == parameter.Name);
                Equal(original.ParameterType, parameter.ParameterType);
            }
        }
        Equal(typeof(void), patch.ReturnType);
        Console.WriteLine($"  {target.DeclaringType!.FullName}.{target.Name}({string.Join(", ", target.GetParameters().Select(parameter => parameter.ParameterType.Name + " " + parameter.Name))}) -> {target.ReturnType.Name}");
    });
}

Check("RealAssembliesResolveAllAddonTypes", () =>
{
    var types = addon.GetTypes();
    Equal(true, types.Any(type => type.Name == "ConstructionImportService"));
    Equal(true, types.Any(type => type.Name == "MConfigs"));
    foreach (var type in types)
    {
        foreach (var method in type.GetMethods(allMethods | BindingFlags.DeclaredOnly))
        {
            _ = method.ReturnType;
            _ = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();
        }
        _ = type.BaseType;
        _ = type.GetInterfaces();
    }
});

var siteType = addon.GetType("ConstructionSiteImports.ConstructionImportSite", throwOnError: true)!;
Check("NativeSiteLifecycleInterfaces", () =>
{
    var interfaces = siteType.GetInterfaces().Select(type => type.Name).ToHashSet();
    foreach (var expected in new[] { "IInitializableEntity", "IDeletableEntity", "IUnfinishedStateListener" })
    {
        Equal(true, interfaces.Contains(expected));
    }
});

Check("OwnComponentDecoratesNativeConstructionSites", () =>
{
    var decoration = siteType.CustomAttributes.Single(attribute => attribute.AttributeType.Name == "AddTemplateModule2Attribute");
    var decorated = (Type)decoration.ConstructorArguments[0].Value!;
    Equal("ConstructionSite", decorated.Name);
    Equal(true, decorated.Assembly.GetName().Name!.StartsWith("Timberborn.", StringComparison.Ordinal));
});

var config = addon.GetType("ConstructionSiteImports.MConfigs", throwOnError: true)!;
Check("OwnSiteTrackerIsRegistered", () =>
{
    var registryCalls = CalledMethods(config.GetMethod("ConfigureRegistry", allMethods)!).OfType<MethodInfo>().ToArray();
    Equal(true, registryCalls.Any(method => method.Name == "UseEntityTracker"));
    Equal(true, registryCalls.Any(method => method.Name == "TryTrack"
        && method.IsGenericMethod && method.GetGenericArguments().SequenceEqual(new[] { siteType })));
});

Check("AddonDoesNotRegisterOrReplaceHaulingTargets", () =>
{
    var calls = addon.GetTypes().SelectMany(type => type.GetMethods(allMethods | BindingFlags.DeclaredOnly))
        .SelectMany(CalledMethods).ToArray();
    Equal(false, calls.Any(method => method.Name == "UseExtraHaulerTargets"
        || method.DeclaringType?.Namespace?.StartsWith("ConstructionSiteHauler", StringComparison.Ordinal) == true
        || method.DeclaringType?.FullName == "ModdableTimberborn.HaulingSystem.ExtraHaulerTargetService"));
});

Check("AddonLoadsWithoutResolvingConstructionSiteHauler", () =>
{
    Equal(0, loadContext.HaulerResolutionRequests);
    Equal(false, loadContext.Assemblies.Any(assembly => assembly.GetName().Name == "ConstructionSiteHauler"));
    if (!withHauler)
    {
        // The resolver actively refuses this assembly even though it is installed.
        // A future hard dependency therefore cannot be hidden by the local game.
        try
        {
            loadContext.LoadFromAssemblyName(new AssemblyName("ConstructionSiteHauler"));
            throw new InvalidOperationException("The standalone resolver accepted ConstructionSiteHauler.");
        }
        catch (FileNotFoundException)
        {
            Equal(1, loadContext.HaulerResolutionRequests);
        }
    }
});

Check(withHauler ? "OriginalHaulerCanCoexistInSameLoadContext" : "StandaloneResolverHasNoHaulerLibrary", () =>
{
    if (withHauler)
    {
        var original = loadContext.LoadFromAssemblyPath(haulerPath);
        Equal("ConstructionSiteHauler", original.GetName().Name);
        Equal(true, original.GetTypes().Any(type => type.Name == "ConstructionSiteHaulerComponent"));
        Equal(true, addon.GetTypes().Contains(siteType));
        Equal(0, loadContext.HaulerResolutionRequests);
    }
    else
    {
        Equal(false, libraries.ContainsKey("ConstructionSiteHauler"));
    }
});

Check("RealCapacityZeroSourceOnlyExportsSurplus", () =>
{
    var distribution = loadContext.LoadFromAssemblyPath(libraries["Timberborn.DistributionSystem"]);
    var settingType = distribution.GetType("Timberborn.DistributionSystem.GoodDistributionSetting", true)!;
    var optionType = distribution.GetType("Timberborn.DistributionSystem.ImportOption", true)!;
    var goodType = distribution.GetType("Timberborn.DistributionSystem.DistributableGood", true)!;
    // Create() only stores these values. GoodId/GoodSpec are never accessed.
    var setting = settingType.GetMethod("Create", allMethods)!.Invoke(null,
        [null, 0f, Enum.Parse(optionType, "Auto"), 0f]);
    var protectedStock = ConstructionImportMath.StockAvailableForExport(25, 20);
    var source = Activator.CreateInstance(goodType, protectedStock, 0, setting)!;
    // Actual game record reports CanExport=true, but MaxExportAmount=0 at cap0.
    Equal(true, (bool)goodType.GetProperty("CanExport")!.GetValue(source)!);
    Equal(0f, (float)goodType.GetProperty("MaxExportAmount")!.GetValue(source)!);
    var exportable = (int)patchClass.GetMethod("ExportableAmount", allMethods)!.Invoke(null, [source])!;
    Equal(5, exportable);
    // DistrictCrossing.GetAmountToExport returns destination FreeCapacity at
    // cap0 (source is available in ModdingAssetUpdater/out). Its 1000-item
    // request must still be clamped when the destination has no tracked site.
    var destination = Activator.CreateInstance(goodType, 0, 1000, setting)!;
    var vanillaAmount = (int)goodType.GetProperty("FreeCapacity")!.GetValue(destination)!;
    Equal(1000, vanillaAmount);
    Equal(5, ConstructionImportMath.LimitSourceExport(vanillaAmount, true, exportable));
});

Console.WriteLine($"RESULT: {passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;

// Read call operands instead of invoking registry configuration: invoking it
// would initialize ModdableTimberborn's static Harmony/game registry.
IEnumerable<MethodBase> CalledMethods(MethodBase method)
{
    var bytes = method.GetMethodBody()?.GetILAsByteArray();
    if (bytes is null)
    {
        yield break;
    }

    var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(opcode => unchecked((ushort)opcode.Value));
    var position = 0;
    while (position < bytes.Length)
    {
        ushort value = bytes[position++];
        if (value == 0xFE)
        {
            value = (ushort)(0xFE00 | bytes[position++]);
        }
        var opcode = opcodes[value];
        if (opcode.OperandType == OperandType.InlineMethod)
        {
            yield return method.Module.ResolveMethod(BitConverter.ToInt32(bytes, position),
                method.DeclaringType?.GetGenericArguments(), method.IsGenericMethod ? method.GetGenericArguments() : null)!;
        }
        position += opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(bytes, position),
            _ => 4
        };
    }
}

sealed class AddonLoadContext(IReadOnlyDictionary<string, string> libraries, bool allowHauler)
    : AssemblyLoadContext("ConstructionSiteImportsVerification", isCollectible: true)
{
    readonly HashSet<string> frameworkNames = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator).Select(path => Path.GetFileNameWithoutExtension(path))
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public int HaulerResolutionRequests { get; private set; }

    protected override Assembly? Load(AssemblyName name)
    {
        if (name.Name!.StartsWith("ConstructionSiteHauler", StringComparison.OrdinalIgnoreCase))
        {
            HaulerResolutionRequests++;
            if (!allowHauler)
            {
                throw new FileNotFoundException("ConstructionSiteHauler is unavailable in standalone verification.", name.Name);
            }
        }
        // Unity ships its own framework facades; use the host .NET runtime for
        // those, as loading Unity's mscorlib creates an incompatible object type.
        if (frameworkNames.Contains(name.Name) || name.Name is "mscorlib" or "netstandard")
        {
            return null;
        }
        return libraries.TryGetValue(name.Name, out var path) ? LoadFromAssemblyPath(path) : null;
    }
}
