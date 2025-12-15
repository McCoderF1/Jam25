using HDT.Gaming.Audio;
using Jam25.Graphics;
using Jam25.NewFolder;
using Jam25.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Jam25
{
    public class Game1 : Game
    {
        public const string TITLE = "WORKING TITLE";

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private AudioController audioController;
        private ScreenManager screenManager;
        private GameContent content;
        private Texture2D wallsFloor;
        private GameMap gameMap;


        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 30;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int tileSize = 32;

        public Game1()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler((_, _) => Exit());
            graphics = new GraphicsDeviceManager(this);

            //TODO: use saved settings <see cref="SettingsScreen"/>
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;

            content = new GameContent(Content);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = TITLE;

            gameMap = new GameMap(mapWidth, mapHeight);
            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            content.LoadFont(FontID.Title, "Fonts/Title");
            content.LoadFont(FontID.Heading, "Fonts/GameState");
            content.LoadFont(FontID.Body, "Fonts/Score");

            wallsFloor = Content.Load<Texture2D>("Images/walls_floor");

            var startScreen = new StartScreen(graphics.GraphicsDevice, spriteBatch, Content, audioController, this);

            screenManager = new ScreenManager(startScreen);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            screenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            DrawDungeon();

            screenManager.Draw();

            // Floor horizontal
            //spriteBatch.Draw(wallsFloor, new Rectangle(0, 0, 32, 32), new Rectangle(8, 80, 32, 64), Color.White);

            // Floor Vertical
            //spriteBatch.Draw(wallsFloor, new Rectangle(0, 32, 32, 32), new Rectangle(0, 86, 48, 32), Color.White);

            // Floor plain
            //spriteBatch.Draw(wallsFloor, new Rectangle(0, 64, 32, 32), new Rectangle(8, 86, 32, 32), Color.White);

            // Wall horizontal
            //spriteBatch.Draw(wallsFloor, new Rectangle(0, 0, 32, 32), new Rectangle(8, 0, 32, 64), Color.White);

            // Wall vertical
            //spriteBatch.Draw(wallsFloor, new Rectangle(0, 0, 32, 32), new Rectangle(0, 8, 48, 24), Color.White);

            // Wall plain
            //spriteBatch.Draw(wallsFloor, new Rectangle(0, 0, 32, 32), new Rectangle(8, 16, 32, 12), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
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
                            Color.White);
                    }
                }
            }
        }

        public void Exit()
        {
            base.Exit();
        }

    }
}
