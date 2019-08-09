using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny;
using Unity.Tiny.Core2D;


/// <summary>
///  The TileMap module implements efficient rendering of tile maps drawn with the
///  Unity editor tilemap tool.
///  @module
/// </summary>
namespace Unity.Tiny.Tilemap2D
{

    /// <summary>
    ///  A structure that describes data per tile instance in a tilemap.
    /// </summary>
    public struct TileData
    {
        /// <summary> Tile position.</summary>
        public float2 position;

        /// <summary>
        ///  The tile to display. Must point to an entity with the Tile component on it.
        /// </summary>
        public Entity tile;
    }

    /// <summary>
    ///  A component that describes tiles in a grid layout.
    /// </summary>
    public struct Tilemap : IComponentData
    {
        // note: tint color of entire map is on renderer

        /// <summary> The anchor point of each tile within its grid cell.</summary>
        public float3 anchor;

        /// <summary> The position of each tile in its grid cell relative to anchor.</summary>
        public float3 position;

        /// <summary> The rotation of each tile in its grid cell relative to anchor </summary>
        public quaternion rotation;

        /// <summary> The scale of each tile in its grid cell relative to anchor. </summary>
        public float3 scale;

        /// <summary> The size of grid cells, in pixels. </summary>
        public float3 cellSize;

        /// <summary> The gap between grid cells, in pixels. </summary>
        public float3 cellGap;

        /// <summary> The list of tiles to draw. </summary>
        //public DynamicArray<TileData> tiles;
    }

    /// <summary> An enum that describes the different collider types for tiles. </summary>
    public enum TileColliderType
    {
        /// <summary> No collider. </summary>
        None,

        /// <summary>
        ///  Sprite collider. Uses the sprite outline as the collider shape for the tile.
        /// </summary>
        Sprite,

        /// <summary>
        ///  Grid collider. Uses the grid layout boundary outline as the collider
        ///  shape for the tile.
        /// </summary>
        Grid
    }

    /// <summary> A component describing properties of a tile used by tilemaps. </summary>
    public struct Tile : IComponentData
    {
        /// <summary> The Color used to tint the material. </summary>
        public Color color;

        /// <summary> The Sprite entity to draw. </summary>
        public Entity sprite;

        /// <summary>
        ///  Collider type, as defined in the TileColliderType enum.
        ///  Options are: None, Sprite, Grid. Defaults to None.
        /// </summary>
        public TileColliderType colliderType;
    }

    /// <summary>
    ///   A component for tilemap rendering.  Specifies an {@link Tilemap} to draw,
    ///   as well as and rendering modifiers, such as a color tint.
    /// </summary>
    public struct TilemapRenderer : IComponentData
    {
        /// <summary>
        ///   Specifies the entity with the {@link Unity.Tiny.Tilemap Tilemap} component that describes
        ///   the shape to render.
        ///   If null, looks for the {@link Tilemap} on the same entity as the {@link Tilemap2DRenderer}.
        /// </summary>
        public Entity tilemap;

        /// <summary>
        ///   A color tint to apply to the sprite image. For normal rendering, this should be opaque
        ///   white (1, 1, 1, 1).
        /// </summary>
        public Color color;

        /// <summary>
        ///   Blending mode for rendering the sprite. The default and regular mode is Alpha.
        /// </summary>
        public BlendOp blending;
    }

    /// <summary>
    ///   Flag component that you apply to the same entity as a TileMap component
    ///   to indicate that the tilemap needs re-chunking.
    ///   Add this component whenever a tile changes.
    /// </summary>
    public struct TilemapRechunk : IComponentData
    {
    }

        /// <summary>
        ///  System that chunks tilemaps and prepares them for rendering.
        ///  This system must run before the DisplayList system and before the map renders.
        ///  It will perform dirty checking on the TileMap, and create private components
        ///  on the entity with the TileMap component.
        /// </summary>
    // FIXME UpdateInGroup
    class TilemapChunkingSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }


}
