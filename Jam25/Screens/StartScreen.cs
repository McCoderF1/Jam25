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
            Quit
        }

        private MenuSelection currentSelection = MenuSelection.Quit;

        #endregion

        public event EventHandler Exit;

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
            spriteBatch.Draw(title, new Microsoft.Xna.Framework.Vector2(0, 0),
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White,
                0f,
                new Microsoft.Xna.Framework.Vector2(0, 0),
                1f,
                SpriteEffects.None,
                0);

            DrawMenu();
        }

        private void DrawMenu()
        {
            int ySelection = 750 + (int)currentSelection * 50;
            spriteBatch.Draw(whiteRectangle, new Microsoft.Xna.Framework.Vector2(100, ySelection),
                new Rectangle(0, 0, 210, 50),
                Color.Black * 0.5f,
                0f,
                new Microsoft.Xna.Framework.Vector2(0, 0),
                1f,
                SpriteEffects.None,
                0);

            spriteBatch.DrawString(font, "Quit", new Microsoft.Xna.Framework.Vector2(110, 1015), Color.WhiteSmoke);
        }

        public void Hide()
        {
        }

        public void Show()
        {
            //AudioManager.PlayMusic("MainTheme");
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();


            if (KeyboardInput.HasBeenPressed(Keys.Space) || KeyboardInput.HasBeenPressed(Keys.Enter))
            {
                if (currentSelection == MenuSelection.Quit)
                    game.Exit();
            }
            else if (KeyboardInput.HasBeenPressed(Keys.M))
            {
                AudioManager.ToggleMute();
            }
            else if (KeyboardInput.HasBeenPressed(Keys.Down))
            {
                currentSelection = (MenuSelection)(((int)currentSelection + 1) % 6);
            }
            else if (KeyboardInput.HasBeenPressed(Keys.Up))
            {
                currentSelection = (MenuSelection)(((int)currentSelection + 5) % 6);
            }
        }

    }

    #region private methods


    #endregion

}
