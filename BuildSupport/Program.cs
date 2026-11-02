using System.Security.Cryptography;
using System.Text.Json;
using BepInEx.AssemblyPublicizer;

if (args.Length < 2 || args.Any(arg => arg is "--help" or "-h"))
{
    Console.WriteLine("Usage: BuildSupport <game-managed-directory> <output-directory> [additional-dll-directory ...]");
    Console.WriteLine("Creates publicized Bindito.*, Timberborn.*, Unity*.dll copies and all DLLs in additional directories.");
    Console.WriteLine("Source files are never overwritten; only this helper's generated output files may be replaced.");
    return args.Length < 2 ? 2 : 0;
}

try
{
    var inputDirectories = args.Where((_, index) => index != 1).Select(Path.GetFullPath).ToArray();
    var outputDirectory = Path.GetFullPath(args[1]);
    foreach (var inputDirectory in inputDirectories)
    {
        if (!Directory.Exists(inputDirectory))
            throw new DirectoryNotFoundException(inputDirectory);
        if (IsWithin(outputDirectory, inputDirectory) || IsWithin(inputDirectory, outputDirectory))
            throw new InvalidOperationException("Input and output directories must be separate and cannot contain each other.");
    }

    var files = Directory.EnumerateFiles(inputDirectories[0], "*.dll", SearchOption.AllDirectories)
        .Where(path => new[] { "Bindito.", "Timberborn.", "Unity" }
            .Any(prefix => Path.GetFileName(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        .Concat(inputDirectories.Skip(1).SelectMany(path => Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories)))
        .Select(Path.GetFullPath)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
        .ToArray();
    if (files.Length == 0)
        throw new InvalidOperationException("No matching input DLLs were found.");
    var duplicates = files.GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1).ToArray();
    if (duplicates.Length > 0)
        throw new InvalidOperationException($"Duplicate DLL output names: {string.Join(", ", duplicates.Select(group => group.Key))}");

    var manifestPath = Path.Combine(outputDirectory, "build-support-manifest.json");
    var previousManifest = File.Exists(manifestPath)
        ? JsonSerializer.Deserialize<BuildManifest>(File.ReadAllText(manifestPath))
        : null;
    var ownedFiles = new HashSet<string>(previousManifest?.Files.Select(file => file.FileName) ?? [], StringComparer.OrdinalIgnoreCase);
    foreach (var input in files)
    {
        var fileName = Path.GetFileName(input);
        var output = Path.Combine(outputDirectory, fileName);
        if (File.Exists(output) && !ownedFiles.Contains(fileName))
            throw new InvalidOperationException($"Refusing to overwrite an output file not owned by this helper: {output}");
    }

    Directory.CreateDirectory(outputDirectory);
    var manifest = new BuildManifest("BepInEx.AssemblyPublicizer 0.4.3", DateTimeOffset.UtcNow, []);
    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    // Save ownership before writes so an interrupted run can safely replace its own partial copies.
    foreach (var input in files)
        manifest.Files.Add(new GeneratedFile(Path.GetFileName(input), input, ComputeHash(input)));
    File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, jsonOptions));

    foreach (var entry in manifest.Files)
    {
        var output = Path.Combine(outputDirectory, entry.FileName);
        AssemblyPublicizer.Publicize(entry.Source, output, new()
        {
            IncludeOriginalAttributesAttribute = true,
            Strip = false,
        });
        entry.OutputSha256 = ComputeHash(output);
        if (ComputeHash(entry.Source) != entry.SourceSha256)
            throw new InvalidOperationException($"Source hash changed while processing: {entry.Source}");
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, jsonOptions));
    }

    Console.WriteLine($"Publicized {manifest.Files.Count} DLL copies into {outputDirectory}");
    Console.WriteLine($"Verified {manifest.Files.Count} source SHA-256 hashes unchanged.");
    foreach (var group in manifest.Files.GroupBy(file => Path.GetDirectoryName(file.Source)))
        Console.WriteLine($"  {group.Count()} from {group.Key}");
    Console.WriteLine($"Manifest: {manifestPath}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}

static bool IsWithin(string candidate, string directory)
{
    var relative = Path.GetRelativePath(directory, candidate);
    return !Path.IsPathRooted(relative)
        && relative != ".."
        && !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
}

static string ComputeHash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream));
}

sealed record BuildManifest(string Tool, DateTimeOffset GeneratedAt, List<GeneratedFile> Files);

sealed record GeneratedFile(string FileName, string Source, string SourceSha256)
{
    public string? OutputSha256 { get; set; }
}
