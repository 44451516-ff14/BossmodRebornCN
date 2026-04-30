using System.IO;
using System.Reflection;
using System.Threading;

namespace BossMod.Pathfinding;

[ConfigDisplay(Name = "Developer settings", Order = 9)]
public sealed class DeveloperConfig : ConfigNode
{
    [PropertyDisplay("Module packs: source directory")]
    public string ModulePackDirectory = "";

    [PropertyDisplay("Obstacle maps: load from source")]
    public bool MapLoadFromSource;

    [PropertyDisplay("Obstacle maps: source path", tooltip: "Should be <repo root>/BossModReborn/Pathfinding/ObstacleMaps/maplist.json")]
    public string MapSourcePath = "";
}

public sealed class ObstacleMapManager : IDisposable
{
    public readonly WorldState World;
    public readonly ObstacleMapDatabase Database = new();
    public string RootPath { get; private set; } = ""; // empty or ends with slash
    private readonly DeveloperConfig _config = Service.Config.Get<DeveloperConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<(ObstacleMapDatabase.Entry entry, Bitmap data)> _entries = [];
    private readonly Lock _tempMapLock = new();
    private (ObstacleMapDatabase.Entry entry, Bitmap data)? _tempMap;
    private Task? _generationTask;

    public bool LoadFromSource => _config.MapLoadFromSource;

    public ObstacleMapManager(WorldState ws)
    {
        World = ws;
        _subscriptions = new(
            _config.Modified.Subscribe(ReloadDatabase),
            ws.CurrentZoneChanged.Subscribe(op => LoadMaps(op.Zone, op.CFCID))
        );
        ReloadDatabase();
    }

    public void Dispose()
    {
        ClearTempMap();
        _subscriptions.Dispose();
    }

    public (ObstacleMapDatabase.Entry? entry, Bitmap? data) Find(Vector3 pos)
    {
        lock (_tempMapLock)
        {
            if (_tempMap is { } temp && temp.entry.Contains(pos))
                return temp;
        }
        return _entries.FirstOrDefault(e => e.entry.Contains(pos));
    }
    public bool CanEditDatabase() => _config.MapLoadFromSource;
    public uint ZoneKey(ushort zoneId, ushort cfcId) => ((uint)zoneId << 16) | cfcId;
    public uint CurrentKey() => ZoneKey(World.CurrentZone, World.CurrentCFCID);
    public TaskStatus GenerationStatus => _generationTask?.Status ?? TaskStatus.RanToCompletion;
    public bool HasTempMap()
    {
        lock (_tempMapLock)
            return _tempMap != null;
    }

    public (string Filename, int Width, int Height)? TempMapMeta
    {
        get
        {
            lock (_tempMapLock)
                return _tempMap is { } t ? (t.entry.Filename, t.data.Width, t.data.Height) : null;
        }
    }

    public bool TryCloneTempMap(out ObstacleMapDatabase.Entry entry, out Bitmap bitmap)
    {
        lock (_tempMapLock)
        {
            if (_tempMap is not { } t)
            {
                entry = null!;
                bitmap = null!;
                return false;
            }

            entry = new ObstacleMapDatabase.Entry(t.entry.MinBounds, t.entry.MaxBounds, t.entry.Origin, t.entry.ViewWidth, t.entry.ViewHeight, t.entry.Filename);
            bitmap = t.data.Clone();
            return true;
        }
    }

    public void ReloadDatabase()
    {
        Service.Log($"Loading obstacle database from {(_config.MapLoadFromSource ? _config.MapSourcePath : "<embedded>")}");
        RootPath = _config.MapLoadFromSource ? _config.MapSourcePath[..(_config.MapSourcePath.LastIndexOfAny(['\\', '/']) + 1)] : "";

        try
        {
            using var dbStream = _config.MapLoadFromSource ? File.OpenRead(_config.MapSourcePath) : GetEmbeddedResource("maplist.json");
            Database.Load(dbStream);
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to load obstacle database: {ex}");
            Database.Entries.Clear();
        }

        LoadMaps(World.CurrentZone, World.CurrentCFCID);
    }

    public void SaveDatabase()
    {
        if (!_config.MapLoadFromSource)
            return;
        Database.Save(_config.MapSourcePath);
        LoadMaps(World.CurrentZone, World.CurrentCFCID);
    }

    private void LoadMaps(ushort zoneId, ushort cfcId)
    {
        ClearTempMap();
        _entries.Clear();
        if (Database.Entries.TryGetValue(ZoneKey(zoneId, cfcId), out var entries))
        {
            foreach (var e in entries)
            {
                try
                {
                    using var eStream = _config.MapLoadFromSource ? File.OpenRead(RootPath + e.Filename) : GetEmbeddedResource(e.Filename);
                    var bitmap = new Bitmap(eStream);
                    _entries.Add((e, bitmap));
                }
                catch (Exception ex)
                {
                    Service.Log($"Failed to load map {e.Filename} from {(_config.MapLoadFromSource ? RootPath : "<embedded>")} for {zoneId}.{cfcId}: {ex}");
                }
            }
        }
    }

    public bool GenerateMap(Vector3 centerWorld, float radius, bool writeToFile)
    {
        if (_generationTask is { IsCompleted: false })
            return false;
        if (writeToFile && !CanEditDatabase())
            return false;

        _generationTask = Service.Framework.RunOnTick(() =>
        {
            var minBounds = new Vector3(centerWorld.X - radius, -1024, centerWorld.Z - radius);
            var maxBounds = new Vector3(centerWorld.X + radius, 1024, centerWorld.Z + radius);
            var pixelSize = 0.5f;
            var filename = writeToFile ? GeneratePersistentMapName() : Path.GetRandomFileName() + ".bmp";
            var fullPath = writeToFile ? RootPath + filename : Path.Combine(Path.GetTempPath(), filename);

            try
            {
                var build = Service.PluginInterface.GetIpcSubscriber<Vector3, string, float, Vector3, Vector3, (Vector3, Vector3)>("vnavmesh.Nav.BuildBitmapBounded");
                var (actualMin, actualMax) = build.InvokeFunc(centerWorld, fullPath, pixelSize, minBounds, maxBounds);

                using var stream = File.OpenRead(fullPath);
                // keep x/z bounds as-is, but make y bounds permissive
                var min = new Vector3(actualMin.X, -1024, actualMin.Z);
                var max = new Vector3(actualMax.X, 1024, actualMax.Z);
                var entry = new ObstacleMapDatabase.Entry(min, max, new(actualMin.XZ()), 60, 60, filename);
                var bitmap = new Bitmap(stream);
                if (writeToFile)
                {
                    Database.Entries.GetOrAdd(CurrentKey()).Add(entry);
                    SaveDatabase();
                }
                else
                {
                    lock (_tempMapLock)
                    {
                        _tempMap = (entry, bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                Service.Log($"Obstacle map generation failed: {ex}");
                throw;
            }
            finally
            {
                if (!writeToFile)
                {
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Service.Log($"Failed to delete temporary obstacle map '{fullPath}': {ex}");
                    }
                }
            }
        }, delayTicks: 1);
        return true;
    }

    private void ClearTempMap()
    {
        lock (_tempMapLock)
            _tempMap = null;
    }

    private string GeneratePersistentMapName()
    {
        for (var i = 1; ; ++i)
        {
            var name = $"{World.CurrentZone}.{World.CurrentCFCID}.auto.{i}.bmp";
            if (!new FileInfo(RootPath + name).Exists)
                return name;
        }
    }

    private Stream GetEmbeddedResource(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream($"BossModReborn.Pathfinding.ObstacleMaps.{name}") ?? throw new InvalidDataException($"Missing embedded resource {name}");
}
