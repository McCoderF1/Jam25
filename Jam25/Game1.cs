using System;
using System.Net;
using HDT.Gaming.Audio;
using HDT.Gaming.Networking.LibCopy;
using Jam25.Graphics;
using Jam25.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Jam25
{
    public class Game1 : Game
    {
        public const string TITLE = "Last Ember";


        private AudioController audioController;
        private ScreenManager screenManager;
        private GameContent content;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        Player player;

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

            player = new();
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = TITLE;
            player.Initalise(Content, GraphicsDevice);

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 20d);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            content.LoadFont(FontID.Title, "Fonts/Title");
            content.LoadFont(FontID.Heading, "Fonts/GameState");
            content.LoadFont(FontID.Body, "Fonts/Score");

            var startScreen = new StartScreen(graphics.GraphicsDevice, spriteBatch, Content, audioController, this);

            screenManager = new ScreenManager(startScreen);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            player.Update(Keyboard.GetState());

            screenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            screenManager.Draw();
            spriteBatch.End();

            player.Draw();

            base.Draw(gameTime);
        }

        public void Exit()
        {
            base.Exit();
        }

    }
}
