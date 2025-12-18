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
    /// <summary>
    /// Death screen displayed when the player dies
    /// </summary>
    public class DeathScreen : IScreen
    {
        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly Texture2D background;
        private readonly SpriteFont font;
        private readonly Texture2D selectionHighlight;

        private float fadeTimer = 0f;
        private const float FadeInDuration = 2f;
        private bool fadeComplete = false;

        private enum MenuSelection
        {
            Retry,
            Menu
        }

        private MenuSelection currentSelection = MenuSelection.Retry;

        #endregion

        /// <summary>
        /// Event triggered when player selects Retry
        /// </summary>
        public event EventHandler Retry;

        /// <summary>
        /// Event triggered when player selects Menu
        /// </summary>
        public event EventHandler BackToMenu;

        public DeathScreen(
            GraphicsDevice graphicsDevice,
            SpriteBatch spriteBatch,
            ContentManager content)
        {
            this.graphicsDevice = graphicsDevice;
            this.spriteBatch = spriteBatch;

            background = content.Load<Texture2D>("Images/DeathScreen");
            font = content.Load<SpriteFont>("Fonts/Menu");

            selectionHighlight = new Texture2D(graphicsDevice, 1, 1);
            selectionHighlight.SetData(new[] { Color.White });
        }

        public void Draw()
        {
            float opacity = MathHelper.Clamp(fadeTimer / FadeInDuration, 0f, 1f);
            Color fadeColor = Color.White * opacity;

            spriteBatch.Draw(background,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                fadeColor);

            if (fadeComplete)
            {
                DrawMenu();
            }
        }

        private void DrawMenu()
        {
            int menuX = graphicsDevice.Viewport.Width / 2 - 80;
            int menuY = graphicsDevice.Viewport.Height - 150;

            int ySelection = menuY + (int)currentSelection * 50;
            spriteBatch.Draw(selectionHighlight,
                new Rectangle(menuX - 10, ySelection, 180, 45),
                new Color(232, 100, 50) * 0.5f);

            spriteBatch.DrawString(font, "Retry", new Vector2(menuX, menuY), Color.WhiteSmoke);
            spriteBatch.DrawString(font, "Menu", new Vector2(menuX, menuY + 50), Color.WhiteSmoke);
        }

        public void Hide()
        {
            AudioManager.PlayMusic(string.Empty);
            currentSelection = MenuSelection.Retry;
            fadeTimer = 0f;
            fadeComplete = false;
        }

        public void Show()
        {
            AudioManager.PlayMusic("Death");
            currentSelection = MenuSelection.Retry;
            fadeTimer = 0f;
            fadeComplete = false;
        }

        public void Update(GameTime gameTime)
        {
            if (!fadeComplete)
            {
                fadeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (fadeTimer >= FadeInDuration)
                {
                    fadeComplete = true;
                }
                return;
            }

            KeyboardInput.GetInput();

            if (KeyboardInput.HasBeenPressed(Keys.Space) || KeyboardInput.HasBeenPressed(Keys.Enter))
            {
                AudioManager.PlaySound("AppClick");

                if (currentSelection == MenuSelection.Retry)
                {
                    Retry?.Invoke(this, EventArgs.Empty);
                }
                else if (currentSelection == MenuSelection.Menu)
                {
                    BackToMenu?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (KeyboardInput.HasBeenPressed(Keys.Down) || KeyboardInput.HasBeenPressed(Keys.S))
            {
                currentSelection = (MenuSelection)(((int)currentSelection + 1) % 2);
                AudioManager.PlaySound("RetroClick");
            }
            else if (KeyboardInput.HasBeenPressed(Keys.Up) || KeyboardInput.HasBeenPressed(Keys.W))
            {
                currentSelection = (MenuSelection)(((int)currentSelection + 1) % 2);
                AudioManager.PlaySound("RetroClick");
            }
        }
    }
}
