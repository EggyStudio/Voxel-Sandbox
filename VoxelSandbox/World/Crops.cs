using Engine;

namespace VoxelSandbox;

/// <summary>A young wheat growing in a cell, an entity of the engine's ECS, with the seconds it has grown.</summary>
public struct Crop
{
    public int X, Y, Z;

    /// <summary>The seconds it has grown on a loaded column.</summary>
    public float Age;
}

/// <summary>
/// The young wheat the seeds planted, each growing while its column is loaded and the world's time
/// runs, which ripens into wheat once it has grown for <see cref="RipeSeconds"/>.
/// </summary>
/// <remarks>
/// The world names each young wheat set, by an edit or as a column holding some arrived, and a save
/// keeps each one's age with the things under way, so a crop grows on from its age after a load and
/// one whose age no save kept begins anew. The wheat the land placed is ripe and grows no more.
/// </remarks>
public sealed class Crops(EcsWorld ecs)
{
    /// <summary>The seconds a young wheat grows before it ripens.</summary>
    public const float RipeSeconds = 300;

    /// <summary>Starts a young wheat growing in a cell from an age, unless one grows there.</summary>
    public void Spawn(int x, int y, int z, float age)
    {
        foreach (var (_, crop) in ecs.Query<Crop>())
            if ((crop.X, crop.Y, crop.Z) == (x, y, z)) return;
        ecs.Add(ecs.Spawn(), new Crop { X = x, Y = y, Z = z, Age = age });
    }

    /// <summary>The age of the young wheat growing in a cell, or null where none grows.</summary>
    public float? Age(int x, int y, int z)
    {
        foreach (var (_, crop) in ecs.Query<Crop>())
            if ((crop.X, crop.Y, crop.Z) == (x, y, z)) return crop.Age;
        return null;
    }

    /// <summary>How many young wheat grow.</summary>
    public int Count => ecs.Count<Crop>();

    /// <summary>
    /// Takes the young wheat the world set to grow from naught, then grows each by the seconds,
    /// ripening those that have grown long enough and letting go of those no longer standing.
    /// </summary>
    /// <returns>How many ripened.</returns>
    public int Update(VoxelWorld world, float seconds)
    {
        if (world.Sown.Count > 0)
        {
            var growing = new HashSet<(int X, int Y, int Z)>();
            foreach (var (_, crop) in ecs.Query<Crop>()) growing.Add((crop.X, crop.Y, crop.Z));
            foreach (var at in world.Sown)
                if (growing.Add(at) && world.GetBlock(at.X, at.Y, at.Z) == BlockId.YoungWheat) ecs.Add(ecs.Spawn(), new Crop { X = at.X, Y = at.Y, Z = at.Z });
            world.Sown.Clear();
        }
        var (gone, ripened) = (new List<int>(), 0);
        foreach (var (id, crop) in ecs.Query<Crop>())
        {
            if (!world.HasColumn(VoxelWorld.ColumnOf(crop.X), VoxelWorld.ColumnOf(crop.Z))) continue;
            if (world.GetBlock(crop.X, crop.Y, crop.Z) != BlockId.YoungWheat) gone.Add(id);
            else if (crop.Age + seconds < RipeSeconds) ecs.GetRef<Crop>(id).Age += seconds;
            else
            {
                world.SetBlock(crop.X, crop.Y, crop.Z, BlockId.Wheat);
                gone.Add(id);
                ripened++;
            }
        }
        foreach (var id in gone) ecs.Despawn(id);
        return ripened;
    }

    /// <summary>Lets go of every young wheat, as a world closes.</summary>
    public void Clear()
    {
        var all = new List<int>();
        foreach (var (id, _) in ecs.Query<Crop>()) all.Add(id);
        foreach (var id in all) ecs.Despawn(id);
    }
}
