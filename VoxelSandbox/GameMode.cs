namespace VoxelSandbox;

/// <summary>How the player plays, as Minecraft's modes are, in the order its switcher steps through them.</summary>
public enum GameMode
{
    /// <summary>Every block to place without end, blocks broken at once and dropping nothing, and flight, for building a test of the light.</summary>
    Creative,

    /// <summary>Blocks that take time to break and drop what they are made of, an inventory that placing uses up, and no flight.</summary>
    Survival,

    /// <summary>Survival's health, hunger and inventory with no block placed or broken, the doors, tables and furnaces opened and the mobs hit, for playing through a world built for it.</summary>
    Adventure,

    /// <summary>Flight through the blocks with no body, nothing held and nothing touched, unseen by the mobs, for looking around a world.</summary>
    Spectator,
}

/// <summary>What each mode lets the player do, and its name as the switcher shows it.</summary>
public static class GameModes
{
    /// <summary>
    /// Whether the player has health and hunger, takes harm, is hunted and sees monsters come at
    /// night, digs blocks over time and uses up what it places, as in survival and adventure.
    /// </summary>
    public static bool Lives(this GameMode mode) => mode is GameMode.Survival or GameMode.Adventure;

    /// <summary>Whether the player places and breaks blocks, as in creative and survival.</summary>
    public static bool Builds(this GameMode mode) => mode is GameMode.Creative or GameMode.Survival;

    /// <summary>Whether the player holds items, opens doors, tables and furnaces, hits mobs and picks up what lies on the ground, as everywhere but in spectator.</summary>
    public static bool Touches(this GameMode mode) => mode != GameMode.Spectator;

    /// <summary>The mode's name in the switcher.</summary>
    public static string Title(this GameMode mode) => mode switch
    {
        GameMode.Creative => "Creative Mode",
        GameMode.Survival => "Survival Mode",
        GameMode.Adventure => "Adventure Mode",
        _ => "Spectator Mode",
    };

    /// <summary>The mode after this one in the switcher, back to creative after spectator.</summary>
    public static GameMode Next(this GameMode mode) => (GameMode)(((int)mode + 1) % 4);
}
