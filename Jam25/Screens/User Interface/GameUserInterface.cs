using HDT.Gaming.Audio;
using HDT.Gaming.Screens;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Screens.UserInterface
{
    /// <summary>
    /// User interface overall during the main game loop
    /// </summary>
    public class GameUserInterface : IScreen
    {
        #region private members

        private readonly SpriteBatch spriteBatch;
        private readonly GameContent game;
        private readonly GraphicsDeviceManager graphics;
        private readonly GraphicsDevice graphicsDevice;
        private readonly ContentManager content;
        private readonly AudioController audioController;

        private readonly Texture2D UIBase;
        private readonly SpriteFont font;

        #endregion

        /// <summary>
        /// Game User Interface constructor
        /// </summary>
        public GameUserInterface(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, GameContent game, ContentManager content, AudioController audioController)
        {
            this.spriteBatch = spriteBatch;
            this.game = game;
            this.graphics = graphics;
            this.graphicsDevice = graphics.GraphicsDevice;
            this.content = content;
            this.audioController = audioController;

            UIBase = content.Load<Texture2D>("Images/UI/UIBase");
            font = content.Load<SpriteFont>("Fonts/Menu");
        }

        ///<inheritdoc/>
        public void Draw()
        {
            spriteBatch.Draw(UIBase,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            DrawPlayerStatusBars();
            DrawTimer();
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

        }

        #region private methods

        private void DrawPlayerStatusBars()
        {

        }

        private void DrawTimer()
        {

        }

        private void UpdateSprite(AnimatedSprite sprite, float elapsed)
        {
            if (sprite.IsPaused)
                return;

            if (game.TryGetSprite(sprite.SpriteId, out AnimatedTexture texture))
            {
                sprite.TotalElapsed += elapsed;
                if (sprite.TotalElapsed > texture.timePerFrame)
                {
                    sprite.Frame++;
                    // Keep the Frame between 0 and the total frames, minus one.
                    sprite.Frame %= texture.frameCount;
                    sprite.TotalElapsed -= texture.timePerFrame;
                }
            }
        }

        #endregion
    }
}
