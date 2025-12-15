using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 30;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int tileSize = 32;
        private Player player;

        public Vector2 CameraPosition;
        public Rectangle WorldBounds;

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




            player = new Player(spriteBatch)
            {

            };
            player.Initalise(content, graphicsDevice);

            gameMap = new GameMap(mapWidth, mapHeight);
            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, player);
            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");


            gameScene = new(gameMap, player);
        }

        public void Draw()
        {
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateTranslation(new Vector3(-CameraPosition, 0f)));

            DrawDungeon();
            player.Draw();
        }

        public void Hide()
        {
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
            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight, player);

            WorldBounds = new Rectangle(0, 0, mapWidth * tileSize, mapHeight * tileSize);
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();
            Vector2 playerMovement = Vector2.Zero;

            KeyboardState keyboardState = Keyboard.GetState();

            //if (keyboardState.IsKeyDown(Keys.D) || keyboardState.IsKeyDown(Keys.Right))
            //{
            //    player.Sprite.IsFacingRight = true;
            //    playerMovement += new Vector2(1f, 0);
            //}
            //if (keyboardState.IsKeyDown(Keys.A) || keyboardState.IsKeyDown(Keys.Left))
            //{
            //    player.Sprite.IsFacingRight = false;
            //    playerMovement += new Vector2(-1f, 0);
            //}
            //if (keyboardState.IsKeyDown(Keys.W) || keyboardState.IsKeyDown(Keys.Up))
            //    playerMovement += new Vector2(0, -1f);
            //if (keyboardState.IsKeyDown(Keys.S) || keyboardState.IsKeyDown(Keys.Down))
            //    playerMovement += new Vector2(0, 1f);

            //if (playerMovement != Vector2.Zero)
            //{
            //    player.Body.Velocity = playerMovement * player.MovementSpeed;
            //}
            //else
            //{
            //    player.Body.Velocity = Vector2.Zero;
            //}

            player.Update(keyboardState);
            Vector2 targetCameraPosition = player.Body.Position - new Vector2(game.GraphicsDevice.Viewport.Width / 2, game.GraphicsDevice.Viewport.Height / 2);

            float cameraMinX = WorldBounds.X;
            float cameraMaxX = WorldBounds.Right - game.GraphicsDevice.Viewport.Width;
            float cameraMinY = WorldBounds.Y;
            float cameraMaxY = WorldBounds.Bottom - game.GraphicsDevice.Viewport.Height;

            CameraPosition.X = MathHelper.Clamp(targetCameraPosition.X, cameraMinX, cameraMaxX);
            CameraPosition.Y = MathHelper.Clamp(targetCameraPosition.Y, cameraMinY, cameraMaxY);
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

    #region private methods


    #endregion

}
