using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Jam25.Screens
{
    public class StartScreen : IScreen
    {

        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly AudioController audioController;
        private readonly Game1 game;
        private readonly Texture2D title;
        private readonly SpriteFont font;
        private readonly Texture2D shopIcon;
        private readonly Texture2D whiteRectangle;
        private readonly Texture2D nameInput;

        private string nameBuilder = "";

        private enum MenuSelection
        {
            Start,
            Player,
            Settings,
            Casino,
            Quit
        }

        private MenuSelection currentSelection = MenuSelection.Start;

        #endregion

        public event EventHandler Exit;
        public event EventHandler Settings;
        public event EventHandler Start;
        public event EventHandler Player;
        public event EventHandler Casino;

        public StartScreen(
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

            title = content.Load<Texture2D>("Images/StartMenu");
            font = content.Load<SpriteFont>("Fonts/Menu");

            whiteRectangle = new Texture2D(graphicsDevice, 10, 50);
            Color[] data = new Color[50 * 10];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
            whiteRectangle.SetData(data);
        }

        public void Draw()
        {
            spriteBatch.Draw(title,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            DrawMenu();
        }

        private void DrawMenu()
        {
            int ySelection = 455 + (int)currentSelection * 50;
            spriteBatch.Draw(whiteRectangle, new Microsoft.Xna.Framework.Vector2(100, ySelection),
                new Rectangle(0, 0, 160, 50),
                new Color(232, 190, 84) * 0.5f,
                0f,
                new Microsoft.Xna.Framework.Vector2(0, 0),
                1f,
                SpriteEffects.None,
                0);

            spriteBatch.DrawString(font, "Start", new Microsoft.Xna.Framework.Vector2(110, 455), Color.WhiteSmoke);
            spriteBatch.DrawString(font, "Player", new Microsoft.Xna.Framework.Vector2(110, 505), Color.WhiteSmoke);
            spriteBatch.DrawString(font, "Settings", new Microsoft.Xna.Framework.Vector2(110, 555), Color.WhiteSmoke);
            spriteBatch.DrawString(font, "Casino", new Microsoft.Xna.Framework.Vector2(110, 605), Color.WhiteSmoke);
            spriteBatch.DrawString(font, "Quit", new Microsoft.Xna.Framework.Vector2(110, 655), Color.WhiteSmoke);
        }

        public void Hide()
        {
            AudioManager.PlayMusic(string.Empty);
        }

        public void Show()
        {
            AudioManager.PlayMusic("The Flickering Flame");
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();

            if (KeyboardInput.HasBeenPressed(Keys.Space) || KeyboardInput.HasBeenPressed(Keys.Enter))
            {
                AudioManager.PlaySound("AppClick");
                if (currentSelection == MenuSelection.Quit)
                    game.Exit();

                if (currentSelection == MenuSelection.Settings)
                    Settings.Invoke(this, EventArgs.Empty);

                if (currentSelection == MenuSelection.Start)
                    Start.Invoke(this, EventArgs.Empty);

                if (currentSelection == MenuSelection.Player)
                    Player.Invoke(this, EventArgs.Empty);

                if (currentSelection == MenuSelection.Casino)
                    Casino.Invoke(this, EventArgs.Empty);
            }
            else if (KeyboardInput.HasBeenPressed(Keys.M))
            {
                AudioManager.ToggleMute();
            }
            else if (KeyboardInput.HasBeenPressed(Keys.Down) || KeyboardInput.HasBeenPressed(Keys.S))
            {
                currentSelection = (MenuSelection)(((int)currentSelection + 1) % 5);
                AudioManager.PlaySound("RetroClick");
            }
            else if (KeyboardInput.HasBeenPressed(Keys.Up) || KeyboardInput.HasBeenPressed(Keys.W))
            {
                currentSelection = (MenuSelection)(((int)currentSelection + 4) % 5);
                AudioManager.PlaySound("RetroClick");
            }
        }

    }

    #region private methods


    #endregion

}
