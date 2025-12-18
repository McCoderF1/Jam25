using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Jam25.Screens
{
    public enum LevelType
    {
        Dungeon,
        Lava
    }
    public class GameScreen : IScreen
    {
        #region private members

        private const float TORCH_FADE_IN_SPEED = 2f;

        private const float SHADOW_ALPHA_CHANGE_SPEED = 5f;

        private const float SHADOW_CULL_RADIUS_PADDING = 64f;

        private static readonly TileColors[] levelTileColors = new TileColors[]
        {
            TileColors.Default,
            new(new Color(160, 255, 180, 255), new Color(130, 200, 210, 255)),
            new(new Color(220, 140, 230, 255), new Color(240, 130, 160, 255)),
            new(new Color(240, 40, 20), new Color(240, 100, 30)),
        };

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

        private float torchFadeIn;

        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 10;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int playerAttackState = 0;
        private int healthPickupCount = 20;
        private int coalPickupCount = 15;

        private const int tileSize = 32;
        private PhysicsWorld physicsWorld;

        private bool[,] visibleTiles;
        private float[,] tileShadowTransparency; // 0 = full shadow, 1 = no shadow
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
        private bool currentLevelCompleted = false;

        private readonly Texture2D debugPixel;
        private readonly bool debugDrawCollision = false;

        private const int PlayerColliderHalfWidth = 9;
        private const int PlayerColliderHalfHeight = 1;
        private const int WallOverlapHeight = 8;

        private Boss boss;

        private Dictionary<int, EnemySpawner> enemySpawners;

        private LevelType CurrentLevelType = LevelType.Lava;

        #endregion

        public EventHandler LevelCompleted { get; set; }
        public EventHandler PlayerDied { get; set; }
        public EventHandler TransitionScreen { get; set; }

        public event EventHandler WinScreenTransition;

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

            EnemyFactory enemyFactory = new(game.Content, audioController);

            enemySpawners = new Dictionary<int, EnemySpawner>
            {
                [1] = new(
                    maxEnemies: 15,
                    minSpawnDistanceFromPlayer: 200,
                    PointWithinWalls,
                    [
                        (_ => enemyFactory.CreateSlimeEnemy(_), 0.6),
                        (_ => enemyFactory.CreateLavaSlimeEnemy(_), 0.2),
                        (_ => enemyFactory.CreateVampireEnemy(_), 0.2),
                    ]),
                [2] = new(
                    maxEnemies: 25,
                    minSpawnDistanceFromPlayer: 200,
                    PointWithinWalls,
                    [
                        (_ => enemyFactory.CreateSlimeEnemy(_), 0.4),
                        (_ => enemyFactory.CreateLavaSlimeEnemy(_), 0.3),
                        (_ => enemyFactory.CreateVampireEnemy(_), 0.3),
                    ]),
                [3] = new(
                    maxEnemies: 30,
                    minSpawnDistanceFromPlayer: 200,
                    PointWithinWalls,
                    [
                        (_ => enemyFactory.CreateSlimeEnemy(_), 0.2),
                        (_ => enemyFactory.CreateLavaSlimeEnemy(_), 0.4),
                        (_ => enemyFactory.CreateVampireEnemy(_), 0.4),
                    ]),
                [4] = new(
                    maxEnemies: 50,
                    minSpawnDistanceFromPlayer: 200,
                    PointWithinWalls,
                    [
                        (_ => enemyFactory.CreateSlimeEnemy(_), 0.2),
                        (_ => enemyFactory.CreateLavaSlimeEnemy(_), 0.4),
                        (_ => enemyFactory.CreateVampireEnemy(_), 0.4),
                    ])
            };

            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");
            doorsTexture = game.Content.Load<Texture2D>("Images/doors_lever_chest_animation");
            whitePixelTexture = game.Content.Load<Texture2D>("Textures/WhiteRectangle");
            objectSpriteSheet = game.Content.Load<Texture2D>("Images/supplies_objects");
            lavaSpriteSheet = game.Content.Load<Texture2D>("Images/spritesheet-lavaland");
            keyTexture = content.Load<Texture2D>("Images/key32");

            player = new Player(spriteBatch);
            player.Initalise(content, graphicsDevice);

            visibleTiles = new bool[mapWidth, mapHeight];
            tileShadowTransparency = new float[mapWidth, mapHeight];

            key = new KeyPickup(keyTexture);
            key.PickedUp += (_, _) => gameUI.CollectedItems.Add(new CollectedItem(key.Sprite.Texture, "Key"));

            var dungeonLevel = new Dungeon(mapWidth, mapHeight, player, key);
            gameScene = new GameScene(dungeonLevel.Map, player);
            gameScene.Pickups.Add(key);

            gameUI = new GameUserInterface(spriteBatch, gfxDevice, gameContent, content, audioController, player, gameScene);

            lightMask = LightMaskFactory.CreateRadialMask(graphicsDevice, lightMaskSize);
            tileShadowMask = LightMaskFactory.CreateTileShadowMask(graphicsDevice, 64);

            this.LevelCompleted += (_, _) =>
            {
                currentLevelCompleted = true;

                if (gameScene.GameLevel > 3)
                    CurrentLevelType = LevelType.Lava;
            };


            debugPixel = new Texture2D(game.GraphicsDevice, 1, 1);
            debugPixel.SetData(new[] { Color.White });

            boss = new Boss(content);
        }

        public void Draw()
        {
            if (!draw)
                return;

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                null,
                null,
                null,
                Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));

            DrawDungeon(backgroundOnly: true);

            foreach (IPickup pickup in gameScene.Pickups)
            {
                pickup.Draw(spriteBatch, tileSize);
            }

            var sortedEnemies = gameScene.Enemies
                .Where(enemy => enemy.CurrentState != Enemy.EnemyState.Dead)
                .OrderBy(enemy => enemy.Body.Position.Y)
                .ToList();
            bool playerDrawn = false; // Used to sort player sprite among enemies
            for (int i = 0; i < sortedEnemies.Count; i++)
            {
                var enemy = sortedEnemies[i];

                if (!playerDrawn && enemy.Body.Position.Y > player.Body.Position.Y)
                {
                    player.Draw();
                    playerDrawn = true;
                }

                enemy.CurrentSprite.Draw(spriteBatch, enemy.Body.Position);

                foreach (Projectile p in enemy.Projectiles)
                {
                    p.Draw(spriteBatch);
                }
            }
            if (!playerDrawn)
            {
                player.Draw();
            }

            //DrawLighting();
            DrawDungeon(backgroundOnly: false);

            DrawDebugCollision();

            DrawLighting();

            // Draw enemy health bars
            for (int i = 0; i < sortedEnemies.Count; i++)
            {
                var enemy = sortedEnemies[i];
                if (TryGetTileCoords(enemy.Body.Position, out int tileX, out int tileY) && visibleTiles[tileX, tileY])
                {
                    enemy.CurrentSprite.DrawHealthBar(spriteBatch, enemy.Body.Position, whitePixelTexture, enemy.Health);
                }
            }

            if (CurrentLevelType == LevelType.Lava)
            {
                boss.Draw(spriteBatch);
                foreach (Projectile p in boss.Projectiles)
                {
                    p.Draw(spriteBatch);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if (CurrentLevelType == LevelType.Lava)
            {
                boss.DrawHealthBar(spriteBatch, game.GraphicsDevice.Viewport.Width);
            }

            gameUI?.Draw();
        }

        public void Hide()
        {
            AudioManager.PlayMusic(string.Empty);

            gameUI?.Hide();
        }

        public void Show()
        {
            ResetWorld();

            // Reset death state
            playerDied = false;
            deathTimer = 0f;
            deathTorchEnergyAtDeath = 0f;

            // Reset player state
            player.Health.Heal(player.Health.Max);
            player.LastState = Player.PlayerState.Idle;
            player.MoveSpeed = 1.0f;

            Random r = new Random();
            AudioManager.PlayMusic($"Game{r.Next(1, 4)}");

            BuildWorld(CurrentLevelType);

            debugLightingDisabled = (CurrentLevelType == LevelType.Lava);

            if (gameUI is GameUserInterface gui)
            {
                gui.SetTorch(game.Torch);
            }

            if (CurrentLevelType == LevelType.Lava)
            {
                boss.Position = PointWithinWalls();
            }

            gameUI?.Show();
        }

        public void Update(GameTime gameTime)
        {
            if (currentLevelCompleted)
            {
                gameScene.GameLevel++;
                currentLevelCompleted = false;

                TransitionScreen?.Invoke("nextlevel", EventArgs.Empty);

                Transition(CurrentLevelType);
                gameUI.SkillsAndAbilitiesTrigger();

                return;
            }

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





            if (CurrentLevelType == LevelType.Lava)
            {
                boss.Update(gameTime, player);


                if (boss.CurrentStage == Boss.Stage.Dead)
                {
                    WinScreenTransition?.Invoke(this, EventArgs.Empty);
                }



                if (Vector2.Distance(boss.Position, player.Body.Position) < 150 && player.IsAttacking != playerAttackState)
                {
                    playerAttackState = player.IsAttacking;

                    if (playerAttackState > 0)
                    {
                        boss.TakeDamage(20);
                    }
                }

                List<Projectile> toRemove = new();
                foreach (Projectile p in boss.Projectiles)
                {
                    p.Update(gameTime.ElapsedGameTime.Milliseconds);
                    if (Vector2.Distance(p.Position, player.Body.Position) < 20 && p.CurrentState == Projectile.ProjectileState.Alive)
                    {
                        p.HitSomething();
                        player.TakeDamage(p.Damage);
                    }
                    if (gameScene.GameMap.tiles[(int)p.Position.X / 32, (int)p.Position.Y / 32].Type != TileType.Floor)
                    {
                        p.HitSomething();
                    }
                    if (p.CurrentState == Projectile.ProjectileState.Dead)
                    {
                        toRemove.Add(p);
                    }
                }
                boss.Projectiles.RemoveAll(i => toRemove.Contains(i));
            }






            gameUI.UpdateWithVector(gameTime, CameraPosition);

            torchFadeIn = Math.Min(torchFadeIn + TORCH_FADE_IN_SPEED * dt, 1f);

            UpdateLighting(dt);
        }

        #region private methods

        private void DrawDebugRect(Rectangle rect, Color color, float thickness = 1f)
        {
            // Left
            spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, (int)thickness, rect.Height), color);
            // Right
            spriteBatch.Draw(debugPixel, new Rectangle(rect.Right, rect.Y, (int)thickness, rect.Height), color);
            // Top
            spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Y, rect.Width, (int)thickness), color);
            // Bottom
            spriteBatch.Draw(debugPixel, new Rectangle(rect.X, rect.Bottom, rect.Width, (int)thickness), color);
        }

        private void DrawDebugCollision()
        {
            if (!debugDrawCollision)
            {
                return;
            }

            // 1) Draw tile grid (optional)
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Rectangle tileRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                    // Thin gray grid
                    DrawDebugRect(tileRect, new Color(80, 80, 80, 120), 1f);

                    // 2) Highlight collidable tiles (walls, doors)
                    var tile = gameScene.GameMap.tiles[x, y];
                    if (tile.Type == TileType.Wall1)
                    {
                        DrawDebugRect(tileRect, Color.Red, 2f);
                    }
                    else if (tile.Type == TileType.Door)
                    {
                        DrawDebugRect(tileRect, Color.Orange, 2f);
                    }
                }
            }

            // 3) Draw player body (assuming Body.Position is center and radius-like AABB)
            const int playerHalfSize = 16; // adjust to your actual collider
            var playerRect = new Rectangle(
                (int)(player.Body.Position.X - playerHalfSize),
                (int)(player.Body.Position.Y - playerHalfSize),
                playerHalfSize * 2,
                playerHalfSize * 2);
            DrawDebugRect(playerRect, Color.Lime, 2f);

            // 4) Draw enemies' bodies similarly
            foreach (var enemy in gameScene.Enemies)
            {
                const int enemyHalfSize = 16; // adjust to actual
                var enemyRect = new Rectangle(
                    (int)(enemy.Body.Position.X - enemyHalfSize),
                    (int)(enemy.Body.Position.Y - enemyHalfSize),
                    enemyHalfSize * 2,
                    enemyHalfSize * 2);
                DrawDebugRect(enemyRect, Color.Cyan, 2f);
            }
        }

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
                Vector2 currentPos = player.Body.Position;
                Vector2 targetPos = probableTargetPosition.Value;
                Vector2 delta = targetPos - currentPos;

                // 1) Try move along X
                if (delta.X != 0f)
                {
                    Vector2 testPosX = currentPos + new Vector2(delta.X, 0f);

                    Rectangle rectX = new Rectangle(
                        (int)(testPosX.X - PlayerColliderHalfWidth),
                        (int)(testPosX.Y - PlayerColliderHalfHeight),
                        PlayerColliderHalfWidth * 2,
                        PlayerColliderHalfHeight * 2);

                    if (CanMoveTo(rectX))
                    {
                        currentPos = testPosX;
                    }
                }

                // 2) Then try move along Y from updated X position
                if (delta.Y != 0f)
                {
                    Vector2 testPosY = currentPos + new Vector2(0f, delta.Y);

                    Rectangle rectY = new Rectangle(
                        (int)(testPosY.X - PlayerColliderHalfWidth),
                        (int)(testPosY.Y - PlayerColliderHalfHeight),
                        PlayerColliderHalfWidth * 2,
                        PlayerColliderHalfHeight * 2);

                    if (CanMoveTo(rectY))
                    {
                        currentPos = testPosY;
                    }
                }

                // After axis-resolved movement, did we move at all?
                if (currentPos != player.Body.Position)
                {
                    // After axis-resolved movement, did we move at all?
                    if (currentPos != player.Body.Position)
                    {
                        // First resolve soft collisions with enemies
                        Vector2 resolvedPos = ResolvePlayerEnemyCollisions(currentPos);
                        player.Body.Position = resolvedPos;

                        Rectangle playerRect = new Rectangle(
                            (int)(resolvedPos.X - PlayerColliderHalfWidth),
                            (int)(resolvedPos.Y - PlayerColliderHalfHeight),
                            PlayerColliderHalfWidth * 2,
                            PlayerColliderHalfHeight * 2);

                        CollectedItem key = gameUI.CollectedItems.Where(item => item.Name == "Key").FirstOrDefault();

                        if (IsOverDoor(playerRect) && key.Name != null)
                        {
                            if (gameUI.CollectedItems.Contains(key))
                                gameUI.CollectedItems.Remove(key);

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
        }

        private bool AnyKey()
        {
            return gameUI.CollectedItems.Where(item => item.Name == "Key").Any();
        }

        private bool CanMoveTo(Rectangle playerRect)
        {
            // Convert world rect to tile indices
            int minTileX = Math.Max(0, playerRect.Left / tileSize);
            int maxTileX = Math.Min(mapWidth - 1, playerRect.Right / tileSize);
            int minTileY = Math.Max(0, playerRect.Top / tileSize);
            int maxTileY = Math.Min(mapHeight - 1, playerRect.Bottom / tileSize);

            for (int tx = minTileX; tx <= maxTileX; tx++)
            {
                for (int ty = minTileY; ty <= maxTileY; ty++)
                {
                    TileType type = gameScene.GameMap.tiles[tx, ty].Type;

                    // Block movement on walls always
                    if (type == TileType.Wall1 || type == TileType.Empty)
                    {
                        return false;
                    }

                    // Doors only block if you don't have the key; allow floor always
                    if (type == TileType.Door && !AnyKey())
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        private const int EnemyColliderHalfWidth = 8;
        private const int EnemyColliderHalfHeight = 8;

        /// <summary>
        /// Resolves collisions between the player and enemies without pushing:
        /// - If the desired position would overlap an enemy, the move is rejected.
        /// - Enemies are never moved.
        /// </summary>
        private Vector2 ResolvePlayerEnemyCollisions(Vector2 desiredPlayerPos)
        {
            // Rectangle at the desired position
            Rectangle desiredPlayerRect = new Rectangle(
                (int)(desiredPlayerPos.X - PlayerColliderHalfWidth),
                (int)(desiredPlayerPos.Y - PlayerColliderHalfHeight),
                PlayerColliderHalfWidth * 2,
                PlayerColliderHalfHeight * 2);

            foreach (var enemy in gameScene.Enemies)
            {
                Rectangle enemyRect = new Rectangle(
                    (int)(enemy.Body.Position.X - EnemyColliderHalfWidth),
                    (int)(enemy.Body.Position.Y - EnemyColliderHalfHeight),
                    EnemyColliderHalfWidth * 2,
                    EnemyColliderHalfHeight * 2);

                if (desiredPlayerRect.Intersects(enemyRect))
                {
                    // Reject movement into enemy: stay at current position
                    return player.Body.Position;
                }
            }

            // No enemy collision: accept desired position
            return desiredPlayerPos;
        }


        private bool IsOverDoor(Rectangle playerRect)
        {
            int minTileX = Math.Max(0, playerRect.Left / tileSize);
            int maxTileX = Math.Min(mapWidth - 1, playerRect.Right / tileSize);
            int minTileY = Math.Max(0, playerRect.Top / tileSize);
            int maxTileY = Math.Min(mapHeight - 1, playerRect.Bottom / tileSize);

            for (int tx = minTileX; tx <= maxTileX; tx++)
            {
                for (int ty = minTileY; ty <= maxTileY; ty++)
                {
                    if (gameScene.GameMap.tiles[tx, ty].Type == TileType.Door)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void DrawDungeon(bool backgroundOnly)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var tile = gameScene.GameMap.tiles[x, y];
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

                    Rectangle fullSourceRect = gameScene.GameMap.tiles[x, y].Theme switch
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

        private void UpdateLighting(float dt)
        {
            Array.Clear(visibleTiles, 0, visibleTiles.Length);

            float radius = GetTorchRadius();
            Vector2 lightCenter = player.Body.Position;

            float tileRadius = radius / tileSize + SHADOW_CULL_RADIUS_PADDING;
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
                    {
                        visibleTiles[tx, ty] = true;

                        // Check tiles around floor for walls/doors to mark as visible.
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                int checkX = tx + ox;
                                int checkY = ty + oy;
                                if (checkX < 0 || checkX >= mapWidth || checkY < 0 || checkY >= mapHeight)
                                    continue;
                                TileType adjacentTile = gameScene.GameMap.tiles[checkX, checkY].Type;
                                visibleTiles[checkX, checkY] = true;
                            }
                        }
                    }

                    if (tile == TileType.Wall1 || tile == TileType.Door)
                    {
                        visibleTiles[tx, ty] = true;
                        break;
                    }
                }
            }

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float changeDirection = visibleTiles[x, y] ? 1f : -1f;
                    tileShadowTransparency[x, y] = MathHelper.Clamp(tileShadowTransparency[x, y] + changeDirection * SHADOW_ALPHA_CHANGE_SPEED * dt, 0f, 1f);
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
            float radius = GetTorchRadius();

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

                    TileType tileType = gameScene.GameMap.tiles[x, y].Type;
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));
        }

        private float GetTorchRadius()
        {
            return game.Torch.CurrentRadius * currentFlicker * torchFadeIn;
        }

        private bool TryGetTileCoords(Vector2 worldPos, out int tileX, out int tileY)
        {
            tileX = (int)(worldPos.X / tileSize);
            tileY = (int)(worldPos.Y / tileSize);

            if (tileX < 0 || tileX >= mapWidth || tileY < 0 || tileY >= mapHeight)
                return false;

            return true;
        }

        private void Transition(LevelType levelType)
        {
            ResetWorld();
            BuildWorld(levelType);

            debugLightingDisabled = (levelType == LevelType.Lava);

            return;
        }

        private void ResetWorld()
        {
            if (playerDied)
                gameScene.GameLevel = 1;

            gameScene.Reset();
            key.Reset();
            // Reset death state
            playerDied = false;
            deathTimer = 0f;
            deathTorchEnergyAtDeath = 0f;

            // Reset player state
            player.Health.Heal(player.Health.Max);
            player.LastState = Player.PlayerState.Idle;
            player.MoveSpeed = 1.0f;

            game.Torch.Reset();

            torchFadeIn = 0f;

            Array.Clear(visibleTiles, 0, visibleTiles.Length);
            Array.Clear(tileShadowTransparency, 0, tileShadowTransparency.Length);
        }

        private void BuildWorld(LevelType levelType)
        {
            if (levelType == LevelType.Dungeon)
            {
                var tileColors = TileColors.Default;
                int levelIndex = gameScene.GameLevel - 1;
                if (levelIndex >= 0 && levelIndex < levelTileColors.Length)
                {
                    tileColors = levelTileColors[levelIndex];
                }

                var bossLevel = new Dungeon(mapWidth, mapHeight, player, key, tileColors: tileColors);
                gameScene.GameMap = bossLevel.Map;

                InitialiseHealthPickups();
                InitialiseCoalPickups();

                key.Reset();
                gameScene.Pickups.Add(key);

                if (enemySpawners.TryGetValue(gameScene.GameLevel, out EnemySpawner enemySpawner))
                {
                    gameScene.EnemySpawner = enemySpawner;
                }
                else
                {
                    gameScene.EnemySpawner = null;
                }

                gameScene.EnemySpawner = enemySpawner;
            }
            else if (levelType == LevelType.Lava)
            {
                var bossLevel = new BossLevel(mapWidth, mapHeight, player);
                gameScene.GameMap = bossLevel.Map;
            }
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

        #endregion
    }
}
