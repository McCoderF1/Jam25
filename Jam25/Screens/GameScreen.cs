using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Physics;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Entities.Levels;
using Jam25.Entities.Pickups;
using Jam25.Models;
using Jam25.Scenes;
using Jam25.Screens.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jam25.Screens
{
    public enum LevelType
    {
        Dungeon,
        Lava
    }

    public enum TileVisibility
    {
        Hidden,
        Partial,
        Full,
    }

    public class GameScreen : IScreen
    {
        #region private members

        private const float MUSIC_STANDARD_VOLUME = 1f;
        private const float MUSIC_QUIET_VOLUME = 0.5f;
        private const float MUSIC_CHANGE_SPEED = 1f;

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

        private Texture2D objectSpriteSheet;
        private Texture2D keyTexture;

        // Entities
        private KeyPickup key;
        private Player player;


        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 10;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int playerAttackState = 0;
        private int healthPickupCount = 20;
        private int coalPickupCount = 15;
        private int eyePickupCount = 1;

        private const int tileSize = 32;
        private PhysicsWorld physicsWorld;

        private SpriteFont font;
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

        private Boss boss;

        private Dictionary<int, EnemySpawner> enemySpawners;

        private LevelType CurrentLevelType = LevelType.Dungeon;

        private readonly IDungeonRenderer dungeonRenderer;
        private readonly NavigationOverlay navigationOverlay;
        private readonly TorchLightingSystem torchLightingSystem;
        private readonly PlayerMovementController playerMovementController;

        #endregion

        public EventHandler LevelCompleted { get; set; }
        public EventHandler PlayerDied { get; set; }
        public EventHandler TransitionScreen { get; set; }

        public event EventHandler WinScreenTransition;

        private List<Projectile> playerProjectiles = new();
        private bool fireballReady = false;
        private float FireballTorchCost;

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
                        //(_ => enemyFactory.CreateLavaSlimeEnemy(_), 0.2),
                        //(_ => enemyFactory.CreateVampireEnemy(_), 0.2),
                    ]),
                [2] = new(
                    maxEnemies: 25,
                    minSpawnDistanceFromPlayer: 200,
                    PointWithinWalls,
                    [
                        (_ => enemyFactory.CreateSlimeEnemy(_), 0.4),
                        (_ => enemyFactory.CreateLavaSlimeEnemy(_), 0.3),
                        //(_ => enemyFactory.CreateVampireEnemy(_), 0.3),
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

            dungeonRenderer = new DungeonRenderer(
                game.Content.Load<Texture2D>("Images/walls_floor"),
                game.Content.Load<Texture2D>("Images/doors_lever_chest_animation"),
                game.Content.Load<Texture2D>("Images/spritesheet-lavaland"),
                tileSize);

            whitePixelTexture = game.Content.Load<Texture2D>("Textures/WhiteRectangle");
            objectSpriteSheet = game.Content.Load<Texture2D>("Images/supplies_objects");
            keyTexture = content.Load<Texture2D>("Images/key32");

            player = new Player(spriteBatch);
            player.Initalise(content, graphicsDevice);

            key = new KeyPickup(keyTexture);

            var dungeonLevel = new Dungeon(mapWidth, mapHeight, player, key);
            gameScene = new GameScene(dungeonLevel.Map, player);
            gameScene.Pickups.Add(key);

            font = content.Load<SpriteFont>("Fonts/Menu");

            gameUI = new GameUserInterface(spriteBatch, gfxDevice, gameContent, content, audioController, player, gameScene);

            key.PickedUp += (_, _) => gameUI.CollectedItems.Add(new CollectedItem(key.Sprite.Texture, "Key"));

            torchLightingSystem = new TorchLightingSystem(
                graphicsDevice,
                mapWidth,
                mapHeight,
                tileSize);

            this.LevelCompleted += (_, _) =>
            {
                currentLevelCompleted = true;

                if (gameScene.GameLevel > 3)
                    CurrentLevelType = LevelType.Lava;
            };


            debugPixel = new Texture2D(game.GraphicsDevice, 1, 1);
            debugPixel.SetData(new[] { Color.White });

            boss = new Boss(content);
            player.UpdateFromStores();
            navigationOverlay = new NavigationOverlay(
                graphicsDevice,
                whitePixelTexture,
                font,
                mapWidth,
                mapHeight,
                tileSize);

            playerMovementController = new PlayerMovementController(
                gameScene,
                gameUI,
                key,
                mapWidth,
                mapHeight,
                tileSize);
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

            dungeonRenderer.Draw(spriteBatch, gameScene.GameMap, backgroundOnly: true);

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

                enemy.CurrentSprite.Draw(spriteBatch, enemy.Body.Position, enemy.IsHitFlashing ? Color.Red : Color.White);

                foreach (Projectile p in enemy.Projectiles)
                {
                    p.Draw(spriteBatch);
                }
            }
            if (!playerDrawn)
            {
                player.Draw();
            }

            foreach (var proj in playerProjectiles)
            {
                proj.Draw(spriteBatch);
            }

            //DrawLighting();
            dungeonRenderer.Draw(spriteBatch, gameScene.GameMap, backgroundOnly: false);

            DrawDebugCollision();

            torchLightingSystem.Draw(spriteBatch, CameraPosition, player.Body.Position, gameScene.GameMap, game.Torch);

            // Draw enemy health bars
            for (int i = 0; i < sortedEnemies.Count; i++)
            {
                var enemy = sortedEnemies[i];
                if (TryGetTileCoords(enemy.Body.Position, out int tileX, out int tileY) && torchLightingSystem.VisibleTiles[tileX, tileY] != TileVisibility.Hidden)
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

            navigationOverlay.DrawDirectionIndicator(spriteBatch, gameScene.GameMap, CameraPosition, player.Body.Position, key);

            gameUI?.Draw();
            navigationOverlay.DrawMiniMap(spriteBatch, gameScene.GameMap, torchLightingSystem.VisitedTiles, player.Body.Position, key);
        }

        public void Hide()
        {
            AudioManager.PlayMusic(string.Empty);

            MediaPlayer.Volume = MUSIC_STANDARD_VOLUME;

            gameUI?.Hide();
        }

        public void Show()
        {
            FireballTorchCost = game.Torch.MaxEnergy * 0.24f;

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

            torchLightingSystem.DebugLightingDisabled = (CurrentLevelType == LevelType.Lava);

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
            KeyboardState keyboardState = Keyboard.GetState();

            // Fireball input: Press B to fire at nearest enemy (not boss)
            if (keyboardState.IsKeyDown(Keys.B) && !fireballReady)
            {
                fireballReady = true;
                var nearestEnemy = gameScene.Enemies
                    .Where(e => e.CurrentState != Enemy.EnemyState.Dead && !(e is Jam25.Entities.Enemies.Boss))
                    .OrderBy(e => Vector2.Distance(e.Body.Position, player.Body.Position))
                    .FirstOrDefault();
                if (nearestEnemy != null)
                {
                    Vector2 fireDirection = nearestEnemy.Body.Position - player.Body.Position;
                    if (fireDirection != Vector2.Zero && game.Torch.NormalizedEnergy * game.Torch.MaxEnergy >= FireballTorchCost)
                    {
                        Vector2 toEnemy = nearestEnemy.Body.Position - player.Body.Position;
                        if (toEnemy.LengthSquared() > 0 && game.Torch.NormalizedEnergy * game.Torch.MaxEnergy >= FireballTorchCost)
                        {
                            var fireball = new Projectile();
                            fireball.Position = player.Body.Position;
                            fireball.Direction = Math.Atan2(fireDirection.Y, fireDirection.X);
                            fireball.Velocity = 300;
                            fireball.Direction = Math.Atan2(toEnemy.Y, toEnemy.X);
                            fireball.Velocity = 500;
                            fireball.Texture = game.Content.Load<Texture2D>("Images/projectile");
                            fireball.Damage = 10;
                            fireball.Lifespan = 1200;
                            fireball.Damage = 99999; //instant kill
                            fireball.Lifespan = 3000;
                            fireball.Target = nearestEnemy;
                            playerProjectiles.Add(fireball);
                            game.Torch.AddEnergy(-FireballTorchCost);
                        }
                    }
                }
            }
            if (!keyboardState.IsKeyDown(Keys.B))
            {
                fireballReady = false;
            }

            // Update player projectiles
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = playerProjectiles[i];
                if (proj.Target != null && proj.Target.CurrentState != Enemy.EnemyState.Dead)
                {
                    Vector2 toTarget = proj.Target.Body.Position - proj.Position;
                    if (toTarget.LengthSquared() > 0.1f)
                    {
                        proj.Direction = Math.Atan2(toTarget.Y, toTarget.X);
                    }
                    if (Vector2.Distance(proj.Position, proj.Target.Body.Position) < 24)
                    {
                        proj.Target.Health.TakeDamage(proj.Damage);
                        proj.Target.CurrentState = Enemy.EnemyState.Dying;
                        proj.Lifespan = 0;
                    }
                }
                proj.Update(gameTime.ElapsedGameTime.Milliseconds);
                if (proj.CurrentState == Projectile.ProjectileState.Dead)
                    playerProjectiles.RemoveAt(i);
            }

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
                torchLightingSystem.DebugLightingDisabled = Player.DebugInvincibleMode;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool levelComplete = playerMovementController.MovePlayer(gameTime, player);
            if(levelComplete)
                LevelCompleted?.Invoke(this, EventArgs.Empty);

            gameScene.Update(gameTime);

            Vector2 targetCameraPosition = player.Body.Position - new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            float cameraMinX = WorldBounds.X;
            float cameraMaxX = WorldBounds.Right - game.GraphicsDevice.Viewport.Width;
            float cameraMinY = WorldBounds.Y;
            float cameraMaxY = WorldBounds.Bottom - game.GraphicsDevice.Viewport.Height;

            CameraPosition.X = MathHelper.Clamp(targetCameraPosition.X, cameraMinX, cameraMaxX);
            CameraPosition.Y = MathHelper.Clamp(targetCameraPosition.Y, cameraMinY, cameraMaxY);

            if (CurrentLevelType == LevelType.Lava)
            {
                boss.Update(gameTime, player);

                if (boss.CurrentStage == Boss.Stage.Dead)
                {
                    WinScreenTransition?.Invoke(this, EventArgs.Empty);
                }

                if (Vector2.Distance(boss.Position, player.Body.Position) < 200 && player.IsAttacking != playerAttackState)
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

            torchLightingSystem.Update(gameTime, player.Body.Position, game.Torch, gameScene.GameMap, player);


            float targetMusicVolume = MUSIC_STANDARD_VOLUME;
            // Make music quieter when buffs are active
            if (player.SeeThroughWallsTimer > 0f)
            {
                targetMusicVolume = MUSIC_QUIET_VOLUME;
            }
            MediaPlayer.Volume = torchLightingSystem.StepTowards(MediaPlayer.Volume, targetMusicVolume, MUSIC_CHANGE_SPEED * dt);
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

            torchLightingSystem.DebugLightingDisabled = (levelType == LevelType.Lava);

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

            torchLightingSystem.Reset();

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
                InitialiseEyePickups();

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

        private void InitialiseEyePickups()
        {
            for (int i = 0; i < eyePickupCount; i++)
            {
                gameScene.Pickups.Add(new EyePickup(PointWithinWalls(), game.Content));
            }
        }

        #endregion
    }
}
