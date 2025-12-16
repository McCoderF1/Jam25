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

        public Vector2 CameraPosition;
        public Vector2 PlayerPosition; // Update this in your game loop
        public Rectangle WorldBounds = new Rectangle(0, 0, 2000, 1500); // Example map size in pixels


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
            var playerScreen = new PlayerScreen(spriteBatch, graphics, content, Content, audioController);

            screenManager = new ScreenManager(startScreen, settingScreen, gameScreen, playerScreen);
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
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            screenManager.Draw();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Exit()
        {
            base.Exit();
        }

    }
}
