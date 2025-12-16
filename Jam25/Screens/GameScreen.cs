using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Physics;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Entities.Pickups;
using Jam25.Scenes;
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
        private Scene gameScene;
        private Texture2D wallsFloor;
        private GameMap gameMap;
        private KeyPickup key;

        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 10;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int healthPickupCount = 20;

        private int tileSize = 32;
        private Player player;
        private PhysicsWorld physicsWorld;

        public Vector2 CameraPosition;
        public Rectangle WorldBounds;

        public List<IPickup> pickups;

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

            for (int i = 0; i < gameScene.Enemies.Count; i++)
            {
                gameScene.Enemies[i].CurrentSprite.Draw(spriteBatch, gameScene.Enemies[i].Body.Position);
            }
        }

        public void Hide()
        {
        }

        public void Show()
        {
            player = new Player(spriteBatch);
            player.Initalise(game.Content, game.GraphicsDevice);

            key = new KeyPickup(Vector2.Zero, game.Content);

            gameScene = new(gameMap, player);
            gameMap = new GameMap(mapWidth, mapHeight);
            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, gameScene, key);
            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");

            // Add the pickups
            pickups = new();
            for (int i = 0; i < healthPickupCount; i++)
            {
                pickups.Add(new HealthPack(PointWithinWalls(), game.Content));
            }
            key.Sprite.Position = PointWithinWalls();    // I think gamemap should over write this, and place it in a far away room.
            pickups.Add(key);


            EnemyFactory enemyFactory = new(game.Content, audioController);
            gameScene.Enemies.Add(enemyFactory.CreateSlimeEnemy(new(200, 200)));

            WorldBounds = new Rectangle(0, 0, mapWidth * tileSize, mapHeight * tileSize);
        }

        private Vector2 PointWithinWalls()
        {

            Random rnd = new();

            Vector2 pos;

            do
            {
                pos = new Vector2(rnd.Next(mapWidth), rnd.Next(mapHeight));
            }
            while (gameMap.tiles[(int)pos.X, (int)pos.Y].Type != TileType.Floor);

            return Vector2.Multiply(pos, tileSize);

        }


        public void Update(GameTime gameTime)
        {
            MovePlayer(gameTime);
            //gameScene.Update(gameTime);

            Vector2 targetCameraPosition = player.Body.Position - new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            float cameraMinX = WorldBounds.X;
            float cameraMaxX = WorldBounds.Right - game.GraphicsDevice.Viewport.Width;
            float cameraMinY = WorldBounds.Y;
            float cameraMaxY = WorldBounds.Bottom - game.GraphicsDevice.Viewport.Height;

            CameraPosition.X = MathHelper.Clamp(targetCameraPosition.X, cameraMinX, cameraMaxX);
            CameraPosition.Y = MathHelper.Clamp(targetCameraPosition.Y, cameraMinY, cameraMaxY);
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
                        if (Vector2.Distance(pickup.Sprite.Position, Vector2.Subtract(player.Body.Position, new Vector2(tileSize / 2, tileSize / 2))) < tileSize)
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

            return gameMap.tiles[xProj, yProj].Type == TileType.Wall;
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

    }


    #endregion

}
