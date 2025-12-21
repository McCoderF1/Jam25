using Jam25.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Screens
{
    /// <summary>
    /// Provides functionality for rendering dungeon maps by drawing background and foreground tile elements to a sprite
    /// batch. This class handles the visual layering of floors, walls, and doors for correct display in a 2D tile-based
    /// game.
    /// </summary>
    /// <remarks>DungeonRenderer separates the rendering of background and foreground elements to ensure
    /// proper visual stacking of tiles, such as overlapping walls and doors. It requires specific tile textures and a
    /// tile size to be provided at construction. This class is sealed and cannot be inherited.</remarks>
    public sealed class DungeonRenderer : IDungeonRenderer
    {
        #region Private Members

        private readonly Texture2D wallsFloor;
        private readonly Texture2D doorsTexture;
        private readonly Texture2D lavaSpriteSheet;
        private readonly int tileSize;
        private const int WallOverlapHeight = 8;

        #endregion

        public DungeonRenderer(Texture2D wallsFloor, Texture2D doorsTexture, Texture2D lavaSpriteSheet, int tileSize)
        {
            this.wallsFloor = wallsFloor;
            this.doorsTexture = doorsTexture;
            this.lavaSpriteSheet = lavaSpriteSheet;
            this.tileSize = tileSize;
        }

        /// <summary>
        /// Draws the visible tiles of the specified game map to the provided sprite batch, rendering either only
        /// background elements or both background and foreground elements based on the specified mode.
        /// </summary>
        /// <remarks>Call this method twice per frame—once with backgroundOnly set to true and once with
        /// it set to false—to ensure correct layering of background and foreground tile elements. This separation
        /// allows for proper rendering of overlapping wall and door graphics. The method does not modify the map or
        /// sprite batch state.</remarks>
        /// <param name="spriteBatch">The sprite batch used to draw tile textures to the screen. Must not be null.</param>
        /// <param name="map">The game map containing the tile data to render. Must not be null.</param>
        /// <param name="backgroundOnly">true to draw only background elements (such as floors and the background portions of walls and doors); false
        /// to draw foreground elements (such as the foreground portions of walls and doors).</param>
        public void Draw(SpriteBatch spriteBatch, GameMap map, bool backgroundOnly)
        {
            var mapWidth = map.tiles.GetLength(0);
            var mapHeight = map.tiles.GetLength(1);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var tile = map.tiles[x, y];
                    TileType tileType = tile.Type;

                    // Floors and doors should be drawn fully in the background pass only
                    if (tileType == TileType.Floor)
                    {
                        if (!backgroundOnly)
                        {
                            continue;
                        }
                    }
                    else if (tileType == TileType.Wall1 || tileType == TileType.Door)
                    {
                        // Walls are split: upper in background pass, lower in foreground pass
                        // so in background pass we draw the wall minus a bottom strip
                        // in foreground pass we draw only that bottom strip.
                    }
                    else
                    {
                        continue;
                    }

                    Texture2D texture = tile.Theme switch
                    {
                        TileTheme.Dungeon => tile.Type switch
                        {
                            TileType.Floor => wallsFloor,
                            TileType.Wall1 => wallsFloor,
                            TileType.Door => doorsTexture,
                            _ => null
                        },
                        TileTheme.Lava => lavaSpriteSheet,
                        _ => null
                    };

                    if (texture == null)
                    {
                        continue;
                    }

                    Rectangle fullSourceRect = map.tiles[x, y].Theme switch
                    {
                        TileTheme.Dungeon => map.tiles[x, y].Type switch
                        {
                            TileType.Floor => new Rectangle(8, 86, 32, 32),
                            TileType.Wall1 => map.tiles[x, y].DirectionMask switch
                            {
                                DirectionMask.North => new Rectangle(8, 0, 30, 24),
                                DirectionMask.South => new Rectangle(8, 32, 32, 32),
                                DirectionMask.West => new Rectangle(2, 8, 32, 24),
                                DirectionMask.East => new Rectangle(14, 8, 32, 24),
                                _ => map.tiles[x, y].TileShape switch
                                {
                                    TileShape.InnerCornerNW => new Rectangle(2, 0, 32, 32),
                                    TileShape.InnerCornerNE => new Rectangle(16, 0, 30, 32),
                                    TileShape.InnerCornerSW => new Rectangle(2, 32, 32, 32),
                                    TileShape.InnerCornerSE => new Rectangle(14, 32, 32, 32),
                                    TileShape.OuterCornerSE => new Rectangle(64, 32, 32, 32),
                                    TileShape.StraightHorizontal => new Rectangle(2, 7, 44, 30),
                                    TileShape.StraightVertical => new Rectangle(9, 0, 30, 78),
                                    _ => Rectangle.Empty
                                }
                            },
                            TileType.Door => new Rectangle(1, 32, 32, 32),
                            _ => Rectangle.Empty
                        },
                        TileTheme.Lava => map.tiles[x, y].Type switch
                        {
                            TileType.Floor => map.tiles[x, y].DirectionMask switch
                            {
                                DirectionMask.North => new Rectangle(42, 6, 32, 32),
                                DirectionMask.South => new Rectangle(42, 79, 32, 32),
                                DirectionMask.West => new Rectangle(4, 49, 32, 32),
                                DirectionMask.East => new Rectangle(66, 49, 32, 32),
                                _ => map.tiles[x, y].TileShape switch
                                {
                                    TileShape.InnerCornerNW => new Rectangle(8, 6, 32, 32),
                                    TileShape.InnerCornerNE => new Rectangle(75, 6, 32, 32),
                                    TileShape.InnerCornerSW => new Rectangle(4, 74, 32, 32),
                                    TileShape.InnerCornerSE => new Rectangle(71, 74, 32, 32),
                                    _ => new Rectangle(32, 32, 32, 32)
                                }
                            },
                            TileType.Wall1 => map.tiles[x, y].DirectionMask switch
                            {
                                DirectionMask.South => new Rectangle(42, 127, 32, 32),
                                _ => map.tiles[x, y].TileShape switch
                                {
                                    TileShape.InnerCornerSW => new Rectangle(4, 127, 32, 32),
                                    TileShape.InnerCornerSE => new Rectangle(71, 127, 32, 32),
                                    _ => Rectangle.Empty
                                }
                            },
                            _ => Rectangle.Empty
                        },
                        _ => Rectangle.Empty
                    };

                    if (fullSourceRect == Rectangle.Empty)
                    {
                        continue;
                    }

                    Rectangle destRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                    if (tileType == TileType.Wall1 || tileType == TileType.Door)
                    {
                        // Map “world” overlap height into texture space
                        int overlapWorld = WallOverlapHeight;
                        int overlapSource = (int)(overlapWorld * (fullSourceRect.Height / (float)tileSize));

                        if (overlapSource <= 0 || overlapSource >= fullSourceRect.Height)
                        {
                            // Fallback: draw full wall in background
                            if (backgroundOnly)
                            {
                                spriteBatch.Draw(
                                    texture,
                                    destRect,
                                    fullSourceRect,
                                    tile.Colors.WallTint,
                                    0f,
                                    Vector2.Zero,
                                    SpriteEffects.None,
                                    0f);
                            }

                            continue;
                        }

                        if (!backgroundOnly)
                        {
                            // Upper part: full wall minus bottom overlap strip
                            Rectangle upperSource = new Rectangle(
                                fullSourceRect.X,
                                fullSourceRect.Y,
                                fullSourceRect.Width,
                                fullSourceRect.Height - overlapSource);

                            Rectangle upperDest = new Rectangle(
                                destRect.X,
                                destRect.Y,
                                destRect.Width,
                                destRect.Height - overlapWorld);

                            spriteBatch.Draw(
                                texture,
                                upperDest,
                                upperSource,
                                tile.Colors.WallTint,
                                0f,
                                Vector2.Zero,
                                SpriteEffects.None,
                                0f);
                        }
                        else
                        {
                            // Foreground strip: only the bottom overlap strip
                            Rectangle lowerSource = new Rectangle(
                                fullSourceRect.X,
                                fullSourceRect.Bottom - overlapSource,
                                fullSourceRect.Width,
                                overlapSource);

                            Rectangle lowerDest = new Rectangle(
                                destRect.X,
                                destRect.Bottom - overlapWorld,
                                destRect.Width,
                                overlapWorld);

                            spriteBatch.Draw(
                                texture,
                                lowerDest,
                                lowerSource,
                                tile.Colors.WallTint,
                                0f,
                                Vector2.Zero,
                                SpriteEffects.None,
                                0f);
                        }

                        continue;
                    }

                    // Floors and doors: draw once in background pass
                    if (backgroundOnly)
                    {
                        spriteBatch.Draw(
                            texture,
                            destRect,
                            fullSourceRect,
                            tile.Colors.FloorTint,
                            0f,
                            Vector2.Zero,
                            SpriteEffects.None,
                            0f);
                    }
                }
            }
        }
    }
}
