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
            base.Initialize(); ;
            Window.Title = TITLE;
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

            screenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

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
