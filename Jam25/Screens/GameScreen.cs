using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Physics;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Entities.Levels;
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
using System.Threading.Tasks;

namespace Jam25.Screens
{
    public class GameScreen : IScreen
    {
        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly AudioController audioController;
        private readonly Game1 game;
        private GameScene gameScene;

        // Textures
        private Texture2D wallsFloor;
        private Texture2D doorsTexture;
        private Texture2D lavaSpriteSheet;
        private Texture2D objectSpriteSheet;
        private Texture2D keyTexture;

        // Entities
        private KeyPickup key;
        private Player player;

        // lighting
        private Texture2D lightMask;
        private Texture2D tileShadowMask;
        private int lightMaskSize = 1024;
        private bool debugLightingDisabled = false;

        // Torch flicker
        private readonly Random flickerRandom = new Random();
        private float flickerTimer;
        private float currentFlicker = 1f;
        private const float FlickerFrequency = 1.5f;
        private const float FlickerStrength = 0.05f;

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

        private GameUserInterface gameUI;
        private readonly Random spawnRandom = new Random();
        private Texture2D whitePixelTexture;

        // Death handling
        private bool playerDied = false;
        private float deathTimer = 0f;
        private const float DeathDelay = 5f;
        private const float DeathShrinkDuration = 3.5f;
        private const float DeathFadeDuration = 1.5f;
        private float deathTorchEnergyAtDeath = 0f;
        // Camera
        private Vector2 CameraPosition;
        private Rectangle WorldBounds => new Rectangle(0, 0, mapWidth * tileSize, mapHeight * tileSize);
        private bool draw = true;

        #endregion

        public EventHandler LevelCompleted { get; set; }
        public EventHandler PlayerDied { get; set; }

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

            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");
            doorsTexture = game.Content.Load<Texture2D>("Images/doors_lever_chest_animation");
            whitePixelTexture = game.Content.Load<Texture2D>("Textures/WhiteRectangle");
            objectSpriteSheet = game.Content.Load<Texture2D>("Images/supplies_objects");
            lavaSpriteSheet = game.Content.Load<Texture2D>("Images/spritesheet-lavaland");
            keyTexture = content.Load<Texture2D>("Images/key32");

            player = new Player(spriteBatch);
            player.Initalise(content, graphicsDevice);

            visibleTiles = new bool[mapWidth, mapHeight];

            gameUI = new GameUserInterface(spriteBatch, gfxDevice, gameContent, content, audioController, player);

            lightMask = LightMaskFactory.CreateRadialMask(graphicsDevice, lightMaskSize);
            tileShadowMask = LightMaskFactory.CreateTileShadowMask(graphicsDevice, 64);

            this.LevelCompleted += (_, _) => Task.Delay(1000).ContinueWith(_ => Transition());
        }

        public void Draw()
        {
            if (!draw)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));

            DrawDungeon();

            foreach (IPickup pickup in gameScene.Pickups)
            {
                pickup.Draw(spriteBatch, tileSize);
            }

            player.Draw();

            for (int i = 0; i < gameScene.Enemies.Count; i++)
            {
                if (gameScene.Enemies[i].CurrentState != Enemy.EnemyState.Dead)
                {
                    gameScene.Enemies[i].CurrentSprite.Draw(spriteBatch, gameScene.Enemies[i].Body.Position, whitePixelTexture, gameScene.Enemies[i].Health);

                    foreach (Projectile p in gameScene.Enemies[i].Projectiles)
                    {
                        p.Draw(spriteBatch);
                    }
                }
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

            // Reset death state
            playerDied = false;
            deathTimer = 0f;
            deathTorchEnergyAtDeath = 0f;

            // Reset player state
            player.Health.Heal(player.Health.Max);
            player.LastState = Player.PlayerState.Idle;
            player.HasKey = false;
            player.MoveSpeed = 1.0f;

            key = new KeyPickup(keyTexture);
            key.PickedUp += (_, _) => gameUI.CollectedItems.Add(new CollectedItem(key.Sprite.Texture, "Key"));

            var dungeonLevel = new Dungeon(mapWidth, mapHeight, player, key);
            gameScene = new GameScene(dungeonLevel.Map, player);
            gameScene.Pickups.Add(key);

            InitialiseHealthPickups();
            InitialiseCoalPickups();

            game.Torch = new Torch(maxEnergy: 100f, drainPerSecond: 0.4f, maxRadius: 250f, minRadius: 60f);

            gameScene.EnemySpawner = new EnemySpawner(
                maxEnemies: 50,
                minSpawnDistanceFromPlayer: 200,
                PointWithinWalls,
                new EnemyFactory(game.Content, audioController));

            if (gameUI is GameUserInterface gui)
            {
                gui.SetTorch(game.Torch);
            }

            gameUI?.Show();
        }

        private void InitialiseCoalPickups()
        {
            for (int i = 0; i < coalPickupCount; i++)
            {
                CoalSize size = (CoalSize)spawnRandom.Next(0, 4);
                var coal = new CoalPickup(PointWithinWalls(), size, game.Content);
                coal.TargetTorch = game.Torch;
                gameScene.Pickups.Add(coal);
            }
        }

        private void InitialiseHealthPickups()
        {
            for (int i = 0; i < healthPickupCount; i++)
            {
                gameScene.Pickups.Add(new HealthPack(PointWithinWalls(), game.Content));
            }
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();

            if (player.LastState == Player.PlayerState.Dying && !playerDied)
            {
                playerDied = true;
                deathTimer = 0f;
                deathTorchEnergyAtDeath = game.Torch.NormalizedEnergy;
            }

            // Death delay with torch fade effect
            if (playerDied)
            {
                deathTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (deathTimer < DeathShrinkDuration)
                {
                    float shrinkProgress = deathTimer / DeathShrinkDuration;

                    float targetEnergy = MathHelper.Lerp(deathTorchEnergyAtDeath * game.Torch.MaxEnergy, 1f, shrinkProgress);

                    float currentTarget = MathHelper.Lerp(game.Torch.MaxEnergy * deathTorchEnergyAtDeath, 1f, shrinkProgress);

                    game.Torch.SetEmpty();

                    game.Torch.AddEnergy(currentTarget);
                }
                else if (deathTimer < DeathShrinkDuration + DeathFadeDuration)
                {
                    game.Torch.SetEmpty();
                }

                if (deathTimer >= DeathDelay)
                {
                    PlayerDied?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }

            game.Torch.Update(gameTime);

            // Debug: P (unlimited stamina, health, one shot enemies, no lighting feature)
            if (KeyboardInput.HasBeenPressed(Keys.P))
            {
                Player.DebugInvincibleMode = !Player.DebugInvincibleMode;
                debugLightingDisabled = Player.DebugInvincibleMode;
            }

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
            while (gameScene.GameMap.tiles[(int)pos.X, (int)pos.Y].Type != TileType.Floor);

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

                    foreach (IPickup pickup in gameScene.Pickups)
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

            return gameScene.GameMap.tiles[xProj, yProj].Type == type;
        }

        private void DrawDungeon()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Texture2D texture = gameScene.GameMap.tiles[x, y].Theme switch
                    {
                        TileTheme.Dungeon => gameScene.GameMap.tiles[x, y].Type switch
                        {
                            TileType.Floor => wallsFloor,
                            TileType.Wall1 => wallsFloor,
                            TileType.Door => doorsTexture,
                            _ => null
                        },
                        TileTheme.Lava => lavaSpriteSheet
                    };

                    if (texture != null)
                    {
                        Rectangle sourceRect = gameScene.GameMap.tiles[x, y].Theme switch
                        {
                            TileTheme.Dungeon => gameScene.GameMap.tiles[x, y].Type switch
                            {
                                TileType.Floor => new Rectangle(8, 86, 32, 32),
                                TileType.Wall1 => gameScene.GameMap.tiles[x, y].DirectionMask switch
                                {
                                    DirectionMask.North => new Rectangle(8, 0, 30, 24),
                                    DirectionMask.South => new Rectangle(8, 32, 32, 32),
                                    DirectionMask.West => new Rectangle(2, 8, 32, 24),
                                    DirectionMask.East => new Rectangle(14, 8, 32, 24),
                                    _ => gameScene.GameMap.tiles[x, y].TileShape switch
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
                            TileTheme.Lava => gameScene.GameMap.tiles[x, y].Type switch
                            {
                                TileType.Floor => gameScene.GameMap.tiles[x, y].DirectionMask switch
                                {
                                    DirectionMask.North => new Rectangle(42, 6, 32, 32),
                                    DirectionMask.South => new Rectangle(42, 79, 32, 32),
                                    DirectionMask.West => new Rectangle(4, 49, 32, 32),
                                    DirectionMask.East => new Rectangle(66, 49, 32, 32),
                                    _ => gameScene.GameMap.tiles[x, y].TileShape switch
                                    {
                                        TileShape.InnerCornerNW => new Rectangle(8, 6, 32, 32),
                                        TileShape.InnerCornerNE => new Rectangle(75, 6, 32, 32),
                                        TileShape.InnerCornerSW => new Rectangle(4, 74, 32, 32),
                                        TileShape.InnerCornerSE => new Rectangle(71, 74, 32, 32),
                                        _ => new Rectangle(32, 32, 32, 32)
                                    }
                                },
                                TileType.Wall1 => gameScene.GameMap.tiles[x, y].DirectionMask switch
                                {
                                    DirectionMask.South => new Rectangle(42, 127, 32, 32),
                                    _ => gameScene.GameMap.tiles[x, y].TileShape switch
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
            if (debugLightingDisabled)
            {
                return;
            }

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

                    TileType tile = gameScene.GameMap.tiles[tx, ty].Type;

                    if (tile == TileType.Floor)
                        visibleTiles[tx, ty] = true;

                    if (tile == TileType.Wall1 || tile == TileType.Door)
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

                    TileType tileType = gameScene.GameMap.tiles[x, y].Type;
                    if (tileType == TileType.Wall1 || tileType == TileType.Door)
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

        private void Transition()
        {
            GameMap checkPoint = new GameMap(mapWidth, mapHeight);
            checkPoint.MakeMap(1, 10, 10, mapWidth, mapHeight);
            checkPoint.AddWalls();

            checkPoint.AddPlayer(gameScene.Player);
            gameScene.Enemies.Clear();
            gameScene.Pickups.Clear();
            gameScene.EnemySpawner = null;
            gameScene.Player.MoveSpeed = 1.0f;
            gameScene.Player.HasKey = false;
            gameUI.CollectedItems.Clear();

            gameScene.GameMap = checkPoint;
            draw = false;
            Task.Delay(1000).ContinueWith(_ => draw = true);
        }

        #endregion
    }
}
