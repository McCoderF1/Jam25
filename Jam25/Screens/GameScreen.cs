using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Physics;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Entities.Pickups;
using Jam25.Entities.Enemies;
using Jam25.Graphics;
using Jam25.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata;
using Jam25.Screens.UserInterface;

namespace Jam25.Screens
{
    public class GameScreen : IScreen
    {
        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly AudioController audioController;
        private readonly Game1 game;
        private readonly Scene gameScene;

        private Texture2D wallsFloor;
        private GameMap gameMap;
        private KeyPickup key;

        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 30;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int healthPickupCount = 20;

        private int tileSize = 32;
        private Player player;
        private PhysicsWorld physicsWorld;

        public Vector2 CameraPosition;
        public Rectangle WorldBounds;

        public List<IPickup> pickups;
        private IScreen gameUI;

        #endregion

        public GameScreen(
            GraphicsDevice gfxDevice,
            SpriteBatch spriteBatch,
            ContentManager content,
            AudioController audioController,
            Game1 game)
        {
            this.graphicsDevice = gfxDevice;
            this.spriteBatch = spriteBatch;
            this.audioController = audioController;
            this.game = game;

            pickups = new();

            player = new Player(spriteBatch);
            player.Initalise(content, graphicsDevice);


            key = new KeyPickup(Vector2.Zero, game.Content);
            gameMap = new GameMap(mapWidth, mapHeight);

            gameScene = new(gameMap, player);

            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, player, key);
            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");


            EnemyFactory enemyFactory = new (game.Content, audioController);

            gameScene.Enemies.Add(enemyFactory.CreateSlimeEnemy(new(200, 200)));
        }

        public void Draw()
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));

            DrawDungeon();
            player.Draw();

            foreach (IPickup pickup in pickups)
            {
                pickup.Draw(spriteBatch, tileSize);
            }
            //spriteBatch.Draw(key.Sprite.Texture, key.Sprite.Position, Color.White);

            for (int i = 0; i < gameScene.Enemies.Count; i++)
            {
                gameScene.Enemies[i].CurrentSprite.Draw(spriteBatch, gameScene.Enemies[i].Body.Position);
            }

            gameUI.Draw();
        }

        public void Hide()
        {
            gameUI.Hide();
        }

        public void Show()
        {
            Texture2D playerTexture = game.Content.Load<Texture2D>("PlayerSprite/lvl1/Swordsman_lvl1_Idle_with_shadow");
            player = new Player(spriteBatch)
            {
                Sprite = new Graphics.Sprite(playerTexture, new Vector2(playerTexture.Width * 0.5f, playerTexture.Height))
            };

            player.Initalise(game.Content, game.GraphicsDevice);

            gameMap = new GameMap(mapWidth, mapHeight);

            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, player, key);


            // Add the pickups
            for (int i = 0; i < healthPickupCount; i++)
            {
                pickups.Add(new HealthPack(PointWithinWalls(), game.Content));
            }
            pickups.Add(key);

            WorldBounds = new Rectangle(0, 0, mapWidth * tileSize, mapHeight * tileSize);

            gameUI.Show();
        }

        private Vector2 PointWithinWalls()
        {
            Random rnd = new();
            Vector2 pos;
            do
            {
                pos = new Vector2(rnd.Next(mapWidth), rnd.Next(mapHeight));
            }
            while (gameMap.tiles[(int)pos.X, (int)pos.Y] == TileType.Wall);
            return Vector2.Multiply(pos, tileSize);
        }

        public void Update(GameTime gameTime)
        {
            MovePlayer(gameTime);
            gameScene.Update(gameTime);

            Vector2 targetCameraPosition = player.Body.Position - new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            float cameraMinX = WorldBounds.X;
            float cameraMaxX = WorldBounds.Right - game.GraphicsDevice.Viewport.Width;
            float cameraMinY = WorldBounds.Y;
            float cameraMaxY = WorldBounds.Bottom - game.GraphicsDevice.Viewport.Height;

            CameraPosition.X = MathHelper.Clamp(targetCameraPosition.X, cameraMinX, cameraMaxX);
            CameraPosition.Y = MathHelper.Clamp(targetCameraPosition.Y, cameraMinY, cameraMaxY);

            gameUI.Update(gameTime);
        }

        public void InstallUI(IScreen userInterface)
        {
            gameUI = userInterface;
        }

        #region private methods

        private void MovePlayer(GameTime gameTime)
        {
            // move based on keyboard input
            KeyboardInput.GetInput();
            Vector2 playerMovement = Vector2.Zero;
            KeyboardState keyboardState = Keyboard.GetState();
            Vector2? probableTargetPosition = player.Update(gameTime, keyboardState);

            // the player is moving
            if (probableTargetPosition is not null)
            {
                // Top left coordinate of where the player is moving to.
                Vector2 targetPosition = (Vector2)probableTargetPosition;

                float buffer = 8; // to make going through thin corridors easier

                // check each corner of the player box
                bool canMove = !(IsWallTile(targetPosition.X - buffer, targetPosition.Y - buffer)
                    || IsWallTile(targetPosition.X - tileSize + buffer, targetPosition.Y - buffer)
                    || IsWallTile(targetPosition.X - tileSize + buffer, targetPosition.Y - tileSize + buffer)
                    || IsWallTile(targetPosition.X - buffer, targetPosition.Y - tileSize + buffer));

                if (canMove)
                {
                    player.Body.Position = targetPosition;

                    foreach (IPickup pickup in pickups)
                    {
                        if (Vector2.Distance(pickup.Position, Vector2.Subtract(player.Body.Position, new Vector2(tileSize / 2, tileSize / 2))) < tileSize)
                        {
                            pickup.Collect(player);
                        }
                    }
                }
                
            }
        }

        private bool IsWallTile(float x, float y)
        {
            int xProj = Convert.ToInt32(x / tileSize);
            int yProj = Convert.ToInt32(y / tileSize);

            return gameMap.tiles[xProj, yProj] == TileType.Wall;
        }

        private void DrawDungeon()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Texture2D texture = gameMap.tiles[x, y] switch
                    {
                        TileType.Floor => wallsFloor,
                        TileType.Wall => wallsFloor,
                        _ => null,
                    };

                    if (texture != null)
                    {
                        Rectangle sourceRect = gameMap.tiles[x, y] switch
                        {
                            TileType.Floor => new Rectangle(8, 86, 32, 32),
                            TileType.Wall => new Rectangle(8, 16, 32, 12),
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

    }


    #endregion

}
