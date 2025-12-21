using Jam25.Entities;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Screens
{
    /// <summary>
    /// Provides a system for managing dynamic torch-based lighting, tile visibility, and shadow effects in a tile-based
    /// game environment.
    /// </summary>
    /// <remarks>The TorchLightingSystem tracks which tiles are visible or have been visited by the player,
    /// and renders lighting and shadow overlays based on the player's torch and map layout. It is designed to be
    /// updated and drawn once per frame, and supports features such as torch flicker, fade-in effects, and debug
    /// lighting control. This class is intended for internal use within the game's rendering and logic
    /// systems.</remarks>
    internal sealed class TorchLightingSystem
    {
        #region Private Members

        private const float TORCH_FADE_IN_SPEED = 2f;

        private const float SHADOW_ALPHA_CHANGE_SPEED = 5f;

        private const float SHADOW_CULL_RADIUS_PADDING = 64f;

        private readonly GraphicsDevice graphicsDevice;
        private readonly int mapWidth;
        private readonly int mapHeight;
        private readonly int tileSize;
       
        // lighting
        private Texture2D lightMask;
        private Texture2D tileShadowMask;
        private int lightMaskSize = 1024;
        

        // Torch flicker
        private readonly Random flickerRandom = new Random();
        private float flickerTimer;
        private float currentFlicker = 1f;
        private const float FlickerFrequency = 1.5f;
        private const float FlickerStrength = 0.05f;

        private float torchFadeIn;

        private float[,] tileShadowTransparency; // 0 = full shadow, 1 = no shadow
        private int rayCount = 360;
        private float rayStep = 8f;

        private readonly Texture2D UiWhitePixel;

        #endregion Private Members

        /// <summary>
        /// Gets the current visibility state of each tile in the grid.
        /// </summary>
        /// <remarks>Each element in the two-dimensional array represents the visibility of a specific
        /// tile, where the first and second indices correspond to the tile's position in the grid. The array dimensions
        /// match the grid's width and height.</remarks>
        public TileVisibility[,] VisibleTiles { get; }

        /// <summary>
        /// Gets a two-dimensional array indicating which tiles have been visited.
        /// </summary>
        public bool[,] VisitedTiles { get; }

        /// <summary>
        /// Gets or sets a value indicating whether debug lighting is disabled.
        /// </summary>
        public bool DebugLightingDisabled { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the TorchLightingSystem class for managing tile-based lighting and visibility
        /// effects.
        /// </summary>
        /// <remarks>This constructor sets up the internal data structures required for lighting
        /// calculations and tile visibility tracking. The provided parameters determine the dimensions and granularity
        /// of the lighting system.</remarks>
        /// <param name="graphicsDevice">The graphics device used to create textures and render lighting effects. Cannot be null.</param>
        /// <param name="mapWidth">The number of tiles in the horizontal direction of the map. Must be greater than zero.</param>
        /// <param name="mapHeight">The number of tiles in the vertical direction of the map. Must be greater than zero.</param>
        /// <param name="tileSize">The size, in pixels, of each tile. Must be greater than zero.</param>
        public TorchLightingSystem(
            GraphicsDevice graphicsDevice,
            int mapWidth,
            int mapHeight,
            int tileSize)
        {
            this.graphicsDevice = graphicsDevice;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.tileSize = tileSize;

            VisibleTiles = new TileVisibility[mapWidth, mapHeight];
            tileShadowTransparency = new float[mapWidth, mapHeight];
            VisitedTiles = new bool[mapWidth, mapHeight];

            lightMask = LightMaskFactory.CreateRadialMask(graphicsDevice, lightMaskSize);
            tileShadowMask = LightMaskFactory.CreateTileShadowMask(graphicsDevice, 64);

            UiWhitePixel = new Texture2D(graphicsDevice, 1, 1);
            UiWhitePixel.SetData(new[] { Color.White });
        }

        /// <summary>
        /// Updates the lighting and visibility state based on the player's position, torch properties, and current game
        /// map.
        /// </summary>
        /// <remarks>This method should be called once per frame to ensure that lighting and visibility
        /// remain consistent with player actions and environmental changes. It also updates which tiles have been
        /// visited for purposes such as minimap display.</remarks>
        /// <param name="gameTime">The elapsed game time since the last update. Used to calculate time-dependent effects such as flickering and
        /// fading.</param>
        /// <param name="playerPos">The current position of the player in world coordinates. Determines the center of the light source for
        /// visibility calculations.</param>
        /// <param name="torch">The torch carried by the player. Its properties affect the radius and intensity of the light.</param>
        /// <param name="map">The current game map containing tile information used for visibility and shadow calculations.</param>
        /// <param name="player">The player entity. Used to determine special vision states, such as the ability to see through walls.</param>
        public void Update(GameTime gameTime, Vector2 playerPos, Torch torch, GameMap map, Player player)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            flickerTimer += dt;

            float sine = (float)Math.Sin(flickerTimer * MathHelper.TwoPi * FlickerFrequency);
            float noise = (float)(flickerRandom.NextDouble() * 2.0 - 1.0);
            float combined = sine * 0.7f + noise * 0.3f;
            float raw = 1f + combined * FlickerStrength;
            currentFlicker = MathHelper.Clamp(raw, 1f - FlickerStrength, 1f + FlickerStrength);

            torchFadeIn = Math.Min(torchFadeIn + TORCH_FADE_IN_SPEED * dt, 1f);

            Array.Clear(VisibleTiles, 0, VisibleTiles.Length);

            float radius = GetTorchRadius(torch);
            Vector2 lightCenter = playerPos;

            float tileRadius = radius / tileSize + SHADOW_CULL_RADIUS_PADDING;
            float maxDistanceSq = tileRadius * tileRadius;
            Vector2 playerTile = lightCenter / tileSize;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.ToRadians(i * (360f / rayCount));
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                Vector2 pos = lightCenter;
                float traveled = 0f;

                var rayVisibility = TileVisibility.Full;

                while (traveled <= radius)
                {
                    pos += dir * rayStep;
                    traveled += rayStep;

                    if (!TryGetTileCoords(pos, out int tx, out int ty))
                        break;

                    Vector2 tileCenter = new Vector2(tx + 0.5f, ty + 0.5f);
                    if (Vector2.DistanceSquared(tileCenter, playerTile) > maxDistanceSq)
                        break;

                    TileType tile = map.tiles[tx, ty].Type;

                    if (tile == TileType.Floor)
                    {
                        SetTileVisibility(tx, ty, rayVisibility);

                        // Check tiles around floor for walls/doors to mark as visible.
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                int checkX = tx + ox;
                                int checkY = ty + oy;
                                if (checkX < 0 || checkX >= mapWidth || checkY < 0 || checkY >= mapHeight)
                                    continue;
                                TileType adjacentTile = map.tiles[checkX, checkY].Type;
                                SetTileVisibility(checkX, checkY, rayVisibility);
                            }
                        }
                    }

                    if (tile == TileType.Wall1 || tile == TileType.Door)
                    {
                        SetTileVisibility(tx, ty, rayVisibility);

                        if (player.SeeThroughWallsTimer > 0f)
                        {
                            // Continue raymarch with partial visibility
                            rayVisibility = TileVisibility.Partial;
                        }
                        else
                        {
                            // Finish raymarch
                            break;
                        }
                    }
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float targetTransparency = VisibleTiles[x, y] switch
                    {
                        TileVisibility.Hidden => 0f,
                        TileVisibility.Partial => 0.9f,
                        TileVisibility.Full => 1f,
                    };

                    float changeSpeed = SHADOW_ALPHA_CHANGE_SPEED * dt;
                    if (player.SeeThroughWallsTimer > 0f)
                    {
                        changeSpeed *= 0.2f;
                    }

                    tileShadowTransparency[x, y] = StepTowards(tileShadowTransparency[x, y], targetTransparency, changeSpeed);
                }
            }

            // Mark tiles we can currently see as permanently visited (for minimap)
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    if (VisibleTiles[x, y] != TileVisibility.Hidden)
                    {
                        VisitedTiles[x, y] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Moves from fromValue towards toValue by step, without overshooting.
        /// </summary>
        /// <param name="fromValue"></param>
        /// <param name="toValue"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public float StepTowards(float fromValue, float toValue, float step)
        {
            if (fromValue < toValue - step)
                return fromValue + step;
            if (fromValue > toValue + step)
                return fromValue - step;
            return toValue;
        }

        /// <summary>
        /// Draws the dynamic lighting and shadow effects for the current game scene, including the player's torch light
        /// and tile-based shadows.
        /// </summary>
        /// <remarks>This method should be called after the main scene rendering to overlay lighting and
        /// shadow effects. If lighting is disabled for debugging, or if required resources are unavailable, the method
        /// will exit without drawing. The method modifies the SpriteBatch state; callers should ensure SpriteBatch is
        /// in a valid state before and after calling this method.</remarks>
        /// <param name="spriteBatch">The SpriteBatch used to issue draw calls for rendering lighting and shadow overlays.</param>
        /// <param name="cameraPosition">The world-space position of the camera's top-left corner, used to align lighting effects with the visible
        /// viewport.</param>
        /// <param name="playerPosition">The world-space position of the player, used as the center point for the torch light effect.</param>
        /// <param name="map">The current game map containing tile data used to determine shadow placement and occlusion.</param>
        /// <param name="torch">The torch object representing the player's light source. Determines the radius and intensity of the lighting
        /// effect. Cannot be null.</param>
        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition, Vector2 playerPosition, GameMap map, Torch torch)
        {
            if (DebugLightingDisabled)
            {
                return;
            }

            if (lightMask == null || torch == null || UiWhitePixel == null)
            {
                return;
            }

            var viewport = graphicsDevice.Viewport;
            float radius = GetTorchRadius(torch);

            var screenInWorld = new Rectangle(
                (int)cameraPosition.X,
                (int)cameraPosition.Y,
                viewport.Width,
                viewport.Height);

            Vector2 lightCenter = playerPosition;

            if (radius <= 0f)
            {
                spriteBatch.Draw(UiWhitePixel, screenInWorld, Color.Black * 0.99f);
                return;
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-cameraPosition, 0f)));

            float baseRadius = lightMaskSize / 2f;
            float scale = radius / baseRadius;
            int maskSize = (int)(lightMaskSize * scale);

            var destRect = new Rectangle(
                (int)(lightCenter.X - maskSize / 2f),
                (int)(lightCenter.Y - maskSize / 2f),
                maskSize,
                maskSize);

            spriteBatch.Draw(lightMask, destRect, Color.White);

            if (destRect.Top > screenInWorld.Top)
                spriteBatch.Draw(UiWhitePixel, new Rectangle(screenInWorld.X, screenInWorld.Y, screenInWorld.Width, destRect.Top - screenInWorld.Top), Color.Black);
            if (destRect.Bottom < screenInWorld.Bottom)
                spriteBatch.Draw(UiWhitePixel, new Rectangle(screenInWorld.X, destRect.Bottom, screenInWorld.Width, screenInWorld.Bottom - destRect.Bottom), Color.Black);
            if (destRect.Left > screenInWorld.Left)
                spriteBatch.Draw(UiWhitePixel, new Rectangle(screenInWorld.X, destRect.Top, destRect.Left - screenInWorld.Left, destRect.Height), Color.Black);
            if (destRect.Right < screenInWorld.Right)
                spriteBatch.Draw(UiWhitePixel, new Rectangle(destRect.Right, destRect.Top, screenInWorld.Right - destRect.Right, destRect.Height), Color.Black);

            int shadowSize = (int)(tileSize * 0.8f);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var tileWorldRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    if (!destRect.Intersects(tileWorldRect))
                        continue;

                    TileType tileType = map.tiles[x, y].Type;
                    //if (tileType == TileType.Wall1 || tileType == TileType.Door)
                    //    continue;

                    Vector2 tileCenterWorld = new Vector2(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f);
                    float distToLight = Vector2.Distance(tileCenterWorld, lightCenter);

                    if (distToLight > radius + SHADOW_CULL_RADIUS_PADDING)
                        continue;

                    int circlesPerRow = 3;
                    float spacing = tileSize / (float)circlesPerRow;

                    for (int cx = 0; cx < circlesPerRow; cx++)
                    {
                        for (int cy = 0; cy < circlesPerRow; cy++)
                        {
                            int drawX = (int)(x * tileSize + cx * spacing + spacing / 2 - shadowSize / 2);
                            int drawY = (int)(y * tileSize + cy * spacing + spacing / 2 - shadowSize / 2);

                            var shadowRect = new Rectangle(drawX, drawY, shadowSize, shadowSize);
                            float alpha = 1f - tileShadowTransparency[x, y];
                            spriteBatch.Draw(tileShadowMask, shadowRect, Color.White * alpha);
                        }
                    }
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-cameraPosition, 0f)));
        }

        /// <summary>
        /// Resets the state of the tile visibility and shadow data to their initial values.
        /// </summary>
        /// <remarks>Call this method to clear all visibility, shadow transparency, and visitation
        /// information, typically when starting a new session or reinitializing the map. After calling this method, all
        /// related data structures will be empty or set to their default state.</remarks>
        public void Reset()
        {
            torchFadeIn = 0f;

            Array.Clear(VisibleTiles, 0, VisibleTiles.Length);
            Array.Clear(tileShadowTransparency, 0, tileShadowTransparency.Length);
            Array.Clear(VisitedTiles, 0, VisitedTiles.Length);
        }

        #region Private Methods

        private float GetTorchRadius(Torch torch)
        {
            return torch.CurrentRadius * currentFlicker * torchFadeIn;
        }

        private bool TryGetTileCoords(Vector2 worldPos, out int tileX, out int tileY)
        {
            tileX = (int)(worldPos.X / tileSize);
            tileY = (int)(worldPos.Y / tileSize);

            if (tileX < 0 || tileX >= mapWidth || tileY < 0 || tileY >= mapHeight)
                return false;

            return true;
        }

        private void SetTileVisibility(int tileX, int tileY, TileVisibility visibility)
        {
            // Only update if new visibility is greater than existing
            if ((int)visibility > (int)VisibleTiles[tileX, tileY])
            {
                VisibleTiles[tileX, tileY] = visibility;
            }
        }

        #endregion 
    }
}
