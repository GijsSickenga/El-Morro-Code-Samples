// (c) Gijs Sickenga, 2018 //

/// <summary>
/// Used to convert from named collision layers to layer integer id's.
/// </summary>
public static class Layers
{
    public enum Names
    {
        // Unity's built-in layers.
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,

        // Custom layers.
        PostProcessing = 8,
        PlayerShip = 10,
        AlliedShips = 11,
        EnemyShips = 12,
        PlayerProjectiles = 13,
        AlliedProjectiles = 14,
        EnemyProjectiles = 15
    }
}
