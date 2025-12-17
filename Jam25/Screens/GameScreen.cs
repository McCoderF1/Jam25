using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Physics;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Entities.Pickups;
using Jam25.Graphics;
using Jam25.Models;
using Jam25.Scenes;
using Jam25.Screens.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Jam25.Screens
{
    public class GameScreen : IScreen
    {
        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly AudioController audioController;
        private readonly Game1 game;
        private readonly GameScene gameScene;
        private Texture2D wallsFloor;
        private Texture2D objectSpriteSheet;
        private GameMap gameMap;
        private KeyPickup key;

        private Player player;

        // lighting
        private Texture2D lightMask;
        private Texture2D tileShadowMask;
        private int lightMaskSize = 1024;

        // Torch flicker
        private readonly Random flickerRandom = new Random();
        private float flickerTimer;
        private float currentFlicker = 1f;
        private const float FlickerFrequency = 3f;
        private const float FlickerStrength = 0f;

        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 10;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int healthPickupCount = 20;
        private int coalPickupCount = 15;

        private const int tileSize = 32;
        private PhysicsWorld physicsWorld;

        private bool[,] visibleTiles;
        private int rayCount = 360;
        private float rayStep = 8f;

        public List<IPickup> pickups;
        private GameUserInterface gameUI;
        private readonly Random spawnRandom = new Random();
        private Texture2D whitePixelTexture;

        // Camera
        private Vector2 CameraPosition;
        private Rectangle WorldBounds => new Rectangle(0, 0, mapWidth * tileSize, mapHeight * tileSize);

        #endregion

        public EventHandler LevelCompleted { get; set; }

        public GameScreen(
            GraphicsDevice gfxDevice,
            SpriteBatch spriteBatch,
            GameContent gameContent,
            ContentManager content,
            AudioController audioController,
            Game1 game)
        {
            this.graphicsDevice = gfxDevice;
            this.spriteBatch = spriteBatch;
            this.audioController = audioController;
            this.game = game;

            pickups = new();

            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");
            whitePixelTexture = game.Content.Load<Texture2D>("Textures/WhiteRectangle");

            player = new Player(spriteBatch);
            player.Initalise(content, graphicsDevice);

            key = new KeyPickup(game.Content);
            pickups.Add(key);

            gameMap = new GameMap(mapWidth, mapHeight);

            EnemyFactory enemyFactory = new(game.Content, audioController);

            EnemySpawner enemySpawner = new(
                maxEnemies: 50,
                minSpawnDistanceFromPlayer: 200,
                PointWithinWalls,
                enemyFactory);

            gameScene = new(gameMap, player, enemySpawner);

            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, gameScene, key);

            visibleTiles = new bool[mapWidth, mapHeight];

            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");
            gameUI = new GameUserInterface(spriteBatch, gfxDevice, gameContent, content, audioController, player);

            key.PickedUp += (_, _) => gameUI.SetKey(key);

            objectSpriteSheet = game.Content.Load<Texture2D>("Images/supplies_objects");

            lightMask = LightMaskFactory.CreateRadialMask(graphicsDevice, lightMaskSize);
            tileShadowMask = LightMaskFactory.CreateTileShadowMask(graphicsDevice, 64);

            gameScene.Enemies.Add(enemyFactory.CreateSlimeEnemy(new(200, 200)));
        }

        public void Draw()
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));

            DrawDungeon();

            foreach (IPickup pickup in pickups)
            {
                pickup.Draw(spriteBatch, tileSize);
            }

            player.Draw();

            for (int i = 0; i < gameScene.Enemies.Count; i++)
            {
                gameScene.Enemies[i].CurrentSprite.Draw(spriteBatch, gameScene.Enemies[i].Body.Position, whitePixelTexture, gameScene.Enemies[i].Health);
            }

            DrawLighting();

            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            gameUI?.Draw();
        }

        public void Hide()
        {
            gameUI?.Hide();
        }

        public void Show()
        {
            pickups.Clear();
            
            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, gameScene, key);

            for (int i = 0; i < healthPickupCount; i++)
            {
                pickups.Add(new HealthPack(PointWithinWalls(), game.Content));
            }

            game.Torch = new Torch(maxEnergy: 100f, drainPerSecond: 0.1f, maxRadius: 250f, minRadius: 60f);

            if (gameUI is GameUserInterface gui)
            {
                gui.SetTorch(game.Torch);
                
                // Set UI reference on key so it can display when collected
                key.GameUI = gui;
            }

            for (int i = 0; i < coalPickupCount; i++)
            {
                CoalSize size = (CoalSize)spawnRandom.Next(0, 4);
                var coal = new CoalPickup(PointWithinWalls(), size, game.Content);
                coal.TargetTorch = game.Torch;
                pickups.Add(coal);
            }

            pickups.Add(key);

            gameUI?.Show();
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();
            game.Torch.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            flickerTimer += dt;

            MovePlayer(gameTime);
            gameScene.Update(gameTime);

            Vector2 targetCameraPosition = player.Body.Position - new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            float cameraMinX = WorldBounds.X;
            float cameraMaxX = WorldBounds.Right - game.GraphicsDevice.Viewport.Width;
            float cameraMinY = WorldBounds.Y;
            float cameraMaxY = WorldBounds.Bottom - game.GraphicsDevice.Viewport.Height;

            CameraPosition.X = MathHelper.Clamp(targetCameraPosition.X, cameraMinX, cameraMaxX);
            CameraPosition.Y = MathHelper.Clamp(targetCameraPosition.Y, cameraMinY, cameraMaxY);

            float sine = (float)Math.Sin(flickerTimer * MathHelper.TwoPi * FlickerFrequency);
            float noise = (float)(flickerRandom.NextDouble() * 2.0 - 1.0);
            float combined = sine * 0.7f + noise * 0.3f;
            float raw = 1f + combined * FlickerStrength;
            currentFlicker = MathHelper.Clamp(raw, 1f - FlickerStrength, 1f + FlickerStrength);

            gameUI.UpdateWithVector(gameTime, CameraPosition);
        }

        #region private methods

        private Vector2 PointWithinWalls()
        {
            Vector2 pos;
            do
            {
                pos = new Vector2(spawnRandom.Next(mapWidth), spawnRandom.Next(mapHeight));
            }
            while (gameMap.tiles[(int)pos.X, (int)pos.Y].Type != TileType.Floor);

            return Vector2.Multiply(pos, tileSize);
        }

        private void MovePlayer(GameTime gameTime)
        {
            KeyboardInput.GetInput();
            KeyboardState keyboardState = Keyboard.GetState();
            Vector2? probableTargetPosition = player.Update(gameTime, keyboardState);

            if (probableTargetPosition is not null)
            {
                Vector2 targetPosition = (Vector2)probableTargetPosition;
                float buffer = 8;

                bool canMove = IsTileType(TileType.Floor, targetPosition.X - buffer, targetPosition.Y)
                    || (player.HasKey && (IsTileType(TileType.Door, targetPosition.X, targetPosition.Y)));

                if (canMove)
                {
                    player.Body.Position = targetPosition;

                    if (IsTileType(TileType.Door, targetPosition.X, targetPosition.Y))
                    {
                        player.MoveSpeed = 0f;

                        LevelCompleted?.Invoke(this, EventArgs.Empty);
                    }

                    foreach (IPickup pickup in pickups)
                    {
                        if (Vector2.Distance(pickup.Sprite.Position, Vector2.Subtract(player.Body.Position, new Vector2(tileSize / 2, tileSize / 2))) < tileSize)
                        {
                            pickup.Collect(player);
                        }
                    }
                }
            }
        }

        private bool IsTileType(TileType type, float x, float y)
        {
            int xProj = Convert.ToInt32(Math.Round(x / tileSize, MidpointRounding.ToZero));
            int yProj = Convert.ToInt32(Math.Round(y / tileSize, MidpointRounding.ToZero));

            return gameMap.tiles[xProj, yProj].Type == type;
        }

        private void DrawDungeon()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Texture2D texture = gameMap.tiles[x, y].Type switch
                    {
                        TileType.Floor => wallsFloor,
                        TileType.Wall => wallsFloor,
                        _ => null,
                    };

                    if (texture != null)
                    {
                        Rectangle sourceRect = gameMap.tiles[x, y].Type switch
                        {
                            TileType.Floor => new Rectangle(8, 86, 32, 32),
                            TileType.Wall => gameMap.tiles[x, y].WallMask switch
                            {
                                WallMask.North => new Rectangle(8, 0, 30, 24),
                                WallMask.South => new Rectangle(8, 14, 32, 64),
                                WallMask.West => new Rectangle(2, 8, 32, 24),
                                WallMask.East => new Rectangle(14, 8, 32, 24),
                                _ => Rectangle.Empty
                            },
                            _ => Rectangle.Empty,
                        };

                        spriteBatch.Draw(
                            texture,
                            new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize),
                            sourceRect,
                            Color.White,
                            0f, Vector2.Zero, SpriteEffects.None, 0f);
                    }
                }
            }
        }

        private void DrawLighting()
        {
            if (lightMask == null || game.Torch == null || game.UiWhitePixel == null)
            {
                return;
            }

            var viewport = graphicsDevice.Viewport;
            float radius = game.Torch.CurrentRadius * currentFlicker;

            var screenInWorld = new Rectangle(
                (int)CameraPosition.X,
                (int)CameraPosition.Y,
                viewport.Width,
                viewport.Height);

            Vector2 lightCenter = player.Body.Position;

            if (radius <= 0f)
            {
                spriteBatch.Draw(game.UiWhitePixel, screenInWorld, Color.Black * 0.99f);
                return;
            }

            Array.Clear(visibleTiles, 0, visibleTiles.Length);

            float tileRadius = radius / tileSize;
            float maxDistanceSq = tileRadius * tileRadius;
            Vector2 playerTile = lightCenter / tileSize;

            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.ToRadians(i * (360f / rayCount));
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                Vector2 pos = lightCenter;
                float traveled = 0f;

                while (traveled <= radius)
                {
                    pos += dir * rayStep;
                    traveled += rayStep;

                    if (!TryGetTileCoords(pos, out int tx, out int ty))
                        break;

                    Vector2 tileCenter = new Vector2(tx + 0.5f, ty + 0.5f);
                    if (Vector2.DistanceSquared(tileCenter, playerTile) > maxDistanceSq)
                        break;

                    TileType tile = gameMap.tiles[tx, ty].Type;

                    if (tile == TileType.Floor)
                        visibleTiles[tx, ty] = true;

                    if (tile == TileType.Wall)
                    {
                        visibleTiles[tx, ty] = true;
                        break;
                    }
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));

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
                spriteBatch.Draw(game.UiWhitePixel, new Rectangle(screenInWorld.X, screenInWorld.Y, screenInWorld.Width, destRect.Top - screenInWorld.Top), Color.Black);
            if (destRect.Bottom < screenInWorld.Bottom)
                spriteBatch.Draw(game.UiWhitePixel, new Rectangle(screenInWorld.X, destRect.Bottom, screenInWorld.Width, screenInWorld.Bottom - destRect.Bottom), Color.Black);
            if (destRect.Left > screenInWorld.Left)
                spriteBatch.Draw(game.UiWhitePixel, new Rectangle(screenInWorld.X, destRect.Top, destRect.Left - screenInWorld.Left, destRect.Height), Color.Black);
            if (destRect.Right < screenInWorld.Right)
                spriteBatch.Draw(game.UiWhitePixel, new Rectangle(destRect.Right, destRect.Top, screenInWorld.Right - destRect.Right, destRect.Height), Color.Black);

            int shadowSize = (int)(tileSize * 0.8f);

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var tileWorldRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    if (!destRect.Intersects(tileWorldRect))
                        continue;

                    if (visibleTiles[x, y])
                        continue;

                    Vector2 tileCenterWorld = new Vector2(x * tileSize + tileSize / 2f, y * tileSize + tileSize / 2f);
                    float distToLight = Vector2.Distance(tileCenterWorld, lightCenter);

                    if (distToLight > radius)
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
                            spriteBatch.Draw(tileShadowMask, shadowRect, Color.White);
                        }
                    }
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));
        }

        private bool TryGetTileCoords(Vector2 worldPos, out int tileX, out int tileY)
        {
            tileX = (int)(worldPos.X / tileSize);
            tileY = (int)(worldPos.Y / tileSize);

            if (tileX < 0 || tileX >= mapWidth || tileY < 0 || tileY >= mapHeight)
                return false;

            return true;
        }

        #endregion
    }
}
