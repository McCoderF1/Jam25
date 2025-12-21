using Jam25.Entities;
using Jam25.Entities.Pickups;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Screens
{
    /// <summary>
    /// Provides functionality for rendering a minimap overlay and directional guidance indicators on the game screen to
    /// assist player navigation.
    /// </summary>
    /// <remarks>The NavigationOverlay class manages the display of a minimap, including explored areas,
    /// player and key locations, and visual guidance cues when targets are off-screen. It is intended for use within
    /// the game's user interface layer and requires valid graphics resources for rendering. This class is not
    /// thread-safe.</remarks>
    internal sealed class NavigationOverlay
    {
        #region Private Members

        private readonly int miniMapWidth = 200;
        private readonly int miniMapHeight = 120;
        private readonly int miniMapPadding = 8;
        private readonly Texture2D whitePixelTexture;
        private readonly SpriteFont font;
        private readonly int mapWidth;
        private readonly int mapHeight;
        private readonly int tileSize;
        private readonly int viewportWidth;
        private readonly int viewportHeight;
        private readonly Texture2D mapBox;
        private Rectangle miniMapRect;
        private Vector2 screenCenter;
        private Rectangle screenRect;
        private readonly int mapX;
        private readonly int mapY;

        #endregion 

        /// <summary>
        /// Initializes a new instance of the NavigationOverlay class for rendering a minimap overlay on the screen.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used for rendering the overlay and creating textures.</param>
        /// <param name="whitePixelTexture">A 1x1 white pixel texture used for drawing overlay elements.</param>
        /// <param name="font">The sprite font used to render text within the overlay.</param>
        /// <param name="mapWidth">The width of the map, in tiles, to be represented on the minimap.</param>
        /// <param name="mapHeight">The height of the map, in tiles, to be represented on the minimap.</param>
        /// <param name="tileSize">The size, in pixels, of each map tile.</param>
        public NavigationOverlay(GraphicsDevice graphicsDevice,
            Texture2D whitePixelTexture,
            SpriteFont font,
            int mapWidth,
            int mapHeight,
            int tileSize)
        {
            this.whitePixelTexture = whitePixelTexture;
            this.font = font;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.tileSize = tileSize;

            viewportWidth = graphicsDevice.Viewport.Width;
            viewportHeight = graphicsDevice.Viewport.Height;

            mapBox = new Texture2D(graphicsDevice, 1, 1);
            mapBox.SetData(new[] { Color.White });

            // Bottom-right corner with padding
            mapX = viewportWidth - miniMapWidth - miniMapPadding;
            mapY = viewportHeight - miniMapHeight - miniMapPadding - 50;

            miniMapRect = new Rectangle(mapX, mapY, miniMapWidth, miniMapHeight);
            screenCenter = new Vector2(viewportWidth / 2f, viewportHeight / 2f);
            screenRect = new Rectangle(0, 0, viewportWidth, viewportHeight);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the mini map is displayed in the user interface.
        /// </summary>
        public bool ShowMiniMap { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the guidance indicator is displayed.s
        /// </summary>
        public bool ShowGuidanceIndicator { get; set; } = false;

        /// <summary>
        /// Draws a directional indicator on the screen to guide the player toward a target location, such as a key or
        /// door, when the target is off-screen.
        /// </summary>
        /// <remarks>The indicator is only drawn if the target is off-screen and guidance is enabled. If a
        /// font is unavailable, a colored square is drawn as a fallback. The indicator uses different colors and
        /// letters to distinguish between guiding to a key or a door.</remarks>
        /// <param name="spriteBatch">The sprite batch used to draw the indicator and any associated graphics.</param>
        /// <param name="map">The current game map containing tile and world information used to determine the target location.</param>
        /// <param name="cameraPosition">The world-space position of the camera, used to convert world coordinates to screen coordinates.</param>
        /// <param name="playerPosition">The current world-space position of the player character.</param>
        /// <param name="key">The key pickup object that may serve as the guidance target. The indicator will point to the key if it has
        /// not been collected; otherwise, it may point to another relevant target.</param>
        public void DrawDirectionIndicator(SpriteBatch spriteBatch, GameMap map, Vector2 cameraPosition, Vector2 playerPosition, KeyPickup key)
        {
            if (whitePixelTexture == null || map.tiles == null || !ShowGuidanceIndicator)
            {
                return;
            }

            Vector2? targetWorld = GetGuidanceTarget(map, playerPosition, key);
            if (targetWorld is null)
            {
                return;
            }

            // Convert world position to screen-space (UI space)
            Vector2 targetScreen = targetWorld.Value - cameraPosition;

            // If target is on screen already, skip (optional)
            
            if (screenRect.Contains(targetScreen))
            {
                return;
            }

            Vector2 dir = targetScreen - screenCenter;
            if (dir.LengthSquared() < 0.0001f)
            {
                return;
            }
            dir.Normalize();

            // Clamp marker to just inside the screen bounds
            float edgePadding = 24f;
            float halfW = viewportWidth / 2f - edgePadding;
            float halfH = viewportHeight / 2f - edgePadding;

            float maxDistX = dir.X != 0f ? halfW / Math.Abs(dir.X) : float.MaxValue;
            float maxDistY = dir.Y != 0f ? halfH / Math.Abs(dir.Y) : float.MaxValue;
            float maxDist = Math.Min(maxDistX, maxDistY);

            if (float.IsInfinity(maxDist) || maxDist <= 0f)
            {
                return;
            }

            Vector2 markerPos = screenCenter + dir * maxDist;

            // Decide which letter and color to use
            bool guidingToKey = !key.Consumed;
            char letter = guidingToKey ? 'K' : 'D';
            Color color = guidingToKey ? Color.Cyan : Color.Gold;

            if (font == null)
            {
                // Fallback: small colored square if no font is available
                float size = 10f;
                var rect = new Rectangle(
                    (int)(markerPos.X - size / 2f),
                    (int)(markerPos.Y - size / 2f),
                    (int)size,
                    (int)size);

                spriteBatch.Draw(
                    whitePixelTexture,
                    rect,
                    color * 0.9f);

                return;
            }

            string text = letter.ToString();

            Vector2 textSize = font.MeasureString(text);
            Vector2 textOrigin = textSize / 2f;

            // Optional subtle background for readability
            float bgPadding = 4f;
            var bgRect = new Rectangle(
                (int)(markerPos.X - textSize.X / 2f - bgPadding),
                (int)(markerPos.Y - textSize.Y / 2f - bgPadding),
                (int)(textSize.X + bgPadding * 2f),
                (int)(textSize.Y + bgPadding * 2f));

            spriteBatch.Draw(
                whitePixelTexture,
                bgRect,
                Color.Black * 0.6f);

            spriteBatch.DrawString(
                font,
                text,
                markerPos,
                color,
                0f,
                textOrigin,
                1f,
                SpriteEffects.None,
                0f);
        }

        /// <summary>
        /// Renders the minimap overlay, displaying explored tiles, the player's position, and the key location if it
        /// has not been collected.
        /// </summary>
        /// <remarks>The minimap is only drawn if minimap display is enabled and required resources are
        /// available. Unvisited tiles are hidden to preserve fog of war. The minimap includes visual markers for the
        /// player and the key (if present), aiding navigation. This method does not modify game state.</remarks>
        /// <param name="spriteBatch">The sprite batch used to draw minimap elements to the screen. Must not be null.</param>
        /// <param name="map">The current game map containing tile data to be visualized on the minimap. Must not be null and must have
        /// initialized tile data.</param>
        /// <param name="visitedTiles">A two-dimensional Boolean array indicating which map tiles have been visited by the player. Only visited
        /// tiles are shown on the minimap. The array dimensions must match the map's width and height.</param>
        /// <param name="playerPosition">The player's current position in world coordinates. Used to display the player's marker on the minimap.</param>
        /// <param name="key">The key object to display on the minimap if it has not been picked up. The key's position is shown only if
        /// it is uncollected and its tile has been visited.</param>
        public void DrawMiniMap(SpriteBatch spriteBatch, GameMap map, bool[,] visitedTiles, Vector2 playerPosition, KeyPickup key)
        {
            if (!ShowMiniMap || map.tiles == null || whitePixelTexture == null)
            {
                return;
            }

            // Background (semi-transparent)
            spriteBatch.Draw(
                whitePixelTexture,
                miniMapRect,
                Color.Black * 0.6f);

            // Calculate tile → minimap pixel scaling
            float scaleX = miniMapWidth / (float)mapWidth;
            float scaleY = miniMapHeight / (float)mapHeight;

            // Draw tiles
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    // Skip tiles never visited (fog of war)
                    if (!visitedTiles[x, y])
                    {
                        continue;
                    }

                    var tile = map.tiles[x, y];

                    Color color;
                    switch (tile.Type)
                    {
                        case TileType.Wall1:
                            color = new Color(200, 200, 200, 255); // light wall
                            break;
                        case TileType.Floor:
                            color = new Color(60, 60, 60, 255); // dark floor
                            break;
                        case TileType.Door:
                            color = Color.Gold; // door highlight
                            break;
                        default:
                            continue;
                    }

                    // Convert tile index to minimap pixel rect
                    int px = mapX + (int)(x * scaleX);
                    int py = mapY + (int)(y * scaleY);
                    int pw = Math.Max(1, (int)Math.Ceiling(scaleX));
                    int ph = Math.Max(1, (int)Math.Ceiling(scaleY));

                    Rectangle tileRect = new Rectangle(px, py, pw, ph);

                    spriteBatch.Draw(
                        whitePixelTexture,
                        tileRect,
                        color);
                }
            }

            // Draw key marker (if not picked up)
            if (!key.Consumed)
            {
                // Key.Position is in world space (top-left of sprite)
                Vector2 keyCenter = key.Sprite.Position + new Vector2(tileSize / 2f, tileSize / 2f);

                int keyTileX = (int)(keyCenter.X / tileSize);
                int keyTileY = (int)(keyCenter.Y / tileSize);

                if (keyTileX >= 0 && keyTileX < mapWidth &&
                    keyTileY >= 0 && keyTileY < mapHeight &&
                    visitedTiles[keyTileX, keyTileY])
                {
                    int kx = mapX + (int)(keyTileX * scaleX);
                    int ky = mapY + (int)(keyTileY * scaleY);

                    Rectangle keyRect = new Rectangle(kx - 2, ky - 2, 4, 4);
                    spriteBatch.Draw(whitePixelTexture, keyRect, Color.Cyan);
                }
            }

            // Draw player marker
            {
                Vector2 playerCenter = playerPosition;

                float playerTileX = playerCenter.X / tileSize;
                float playerTileY = playerCenter.Y / tileSize;

                int px = mapX + (int)(playerTileX * scaleX);
                int py = mapY + (int)(playerTileY * scaleY);

                Rectangle playerRect = new Rectangle(px - 2, py - 2, 4, 4);
                spriteBatch.Draw(whitePixelTexture, playerRect, Color.White);
            }

            DrawMinimapBox(spriteBatch, miniMapRect, Color.White, 2f);
        }

        private void DrawMinimapBox(SpriteBatch spriteBatch, Rectangle rect, Color color, float thickness = 1f)
        {
            // Left
            spriteBatch.Draw(mapBox, new Rectangle(rect.X, rect.Y, (int)thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(mapBox, new Rectangle(rect.Right, rect.Y, (int)thickness, rect.Height), color);
            // Top
            spriteBatch.Draw(mapBox, new Rectangle(rect.X, rect.Y, rect.Width, (int)thickness), color);
            // Bottom
            spriteBatch.Draw(mapBox, new Rectangle(rect.X, rect.Bottom, rect.Width, (int)thickness), color);
        }

        private Vector2? GetGuidanceTarget(GameMap map, Vector2 playerPos, KeyPickup key)
        {
            if (!key.Consumed)
            {
                return key.Sprite.Position + new Vector2(tileSize / 2f, tileSize / 2f);
            }

            Vector2? closestDoorCenter = null;
            float closestDistSq = float.MaxValue;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (map.tiles[x, y].Type != TileType.Door)
                    {
                        continue;
                    }

                    Vector2 doorCenter = new Vector2(
                        x * tileSize + tileSize / 2f,
                        y * tileSize + tileSize / 2f);

                    float distSq = Vector2.DistanceSquared(playerPos, doorCenter);
                    if (distSq < closestDistSq)
                    {
                        closestDistSq = distSq;
                        closestDoorCenter = doorCenter;
                    }
                }
            }

            return closestDoorCenter;
        }

    }
}
