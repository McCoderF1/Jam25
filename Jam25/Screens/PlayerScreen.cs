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
    /// Player stats screen
    /// </summary>
    public class PlayerScreen : IScreen
    {
        #region private members

        private readonly SpriteBatch spriteBatch;
        private readonly GameContent game;
        private readonly GraphicsDeviceManager graphics;
        private readonly GraphicsDevice graphicsDevice;
        private readonly ContentManager content;
        private readonly AudioController audioController;

        private readonly Texture2D background;
        private readonly Texture2D tab;
        private readonly Texture2D levelMark;
        private readonly SpriteFont font;

        #endregion

        /// <summary>
        /// Back to the menu main screen
        /// </summary>
        public event EventHandler BackToMainMenu;

        /// <summary>
        /// Player Screen constructor
        /// </summary>
        public PlayerScreen(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, GameContent game, ContentManager content, AudioController audioController)
        {
            this.spriteBatch = spriteBatch;
            this.game = game;
            this.graphics = graphics;
            this.graphicsDevice = graphics.GraphicsDevice;
            this.content = content;
            this.audioController = audioController;

            background = content.Load<Texture2D>("Images/SettingsMenu");
            tab = content.Load<Texture2D>("Images/StatsPage/PlayerTab");
            levelMark = content.Load<Texture2D>("Images/StatsPage/LevelMark");

            font = content.Load<SpriteFont>("Fonts/Menu");
        }

        ///<inheritdoc/>
        public void Draw()
        {
            spriteBatch.Draw(background,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            spriteBatch.Draw(tab,
                new Rectangle(100, 100, graphicsDevice.Viewport.Width - 200, graphicsDevice.Viewport.Height - 200),
                Color.White);

            spriteBatch.DrawString(font, "Menu (BackSpace)", new Microsoft.Xna.Framework.Vector2(graphicsDevice.Viewport.Width - 420, graphicsDevice.Viewport.Height - 100), Color.WhiteSmoke);
        }

        ///<inheritdoc/>
        public void Hide()
        {

        }

        ///<inheritdoc/>
        public void Show()
        {

        }

        ///<inheritdoc/>
        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();

            if (KeyboardInput.HasBeenPressed(Keys.Back))
                BackToMainMenu.Invoke(this, EventArgs.Empty);
        }

        #region private methods

        private void DrawLevelMarkings()
        {

        }

        private void DrawXPBar()
        {

        }

        private void DrawPlayerStats()
        {

        }

        #endregion
    }
}
