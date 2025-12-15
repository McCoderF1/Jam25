using HDT.Gaming.Audio;
using Jam25.Graphics;
using Jam25.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace Jam25
{
    public class Game1 : Game
    {
        public const string TITLE = "Last Ember";

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private AudioController audioController;
        private ScreenManager screenManager;
        private GameContent content;


        public Game1()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler((_, _) => Exit());
            graphics = new GraphicsDeviceManager(this);

            //TODO: use saved settings <see cref="SettingsScreen"/>
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            content = new GameContent(Content);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = TITLE;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            content.LoadFont(FontID.Title, "Fonts/Title");
            content.LoadFont(FontID.Heading, "Fonts/GameState");
            content.LoadFont(FontID.Body, "Fonts/Score");

            audioController = new AudioController();
            audioController.InstallMusic("The Flickering Flame", Content.Load<Song>("Sound/Music/The Flickering Flame"));
            //add audio here

            AudioManager.InstallController(audioController);

            var startScreen = new StartScreen(graphics.GraphicsDevice, spriteBatch, Content, audioController, this);
            var settingScreen = new SettingsScreen(spriteBatch, graphics, content, Content, audioController);
            var gameScreen = new GameScreen(graphics.GraphicsDevice, spriteBatch, Content, audioController, this);

            screenManager = new ScreenManager(startScreen, settingScreen, gameScreen);
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

        public void Exit()
        {
            base.Exit();
        }

    }
}
