using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Jam25.Stores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Screens.Transitions
{
    public abstract class TransitionBase : IScreen
    {
        #region private/protected members

        protected readonly SpriteBatch spriteBatch;
        protected readonly GraphicsDeviceManager graphics;
        protected readonly GraphicsDevice graphicsDevice;
        protected readonly ContentManager content;
        protected readonly AudioController audioController;

        protected readonly SpriteFont font;
        protected Texture2D background;
        protected string titleText;
        protected string messageText;

        #endregion

        /// <summary>
        /// Move to the next location after transition
        /// </summary>
        public event EventHandler MovePassTransition;

        public TransitionBase(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager content, AudioController audioController)
        {
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            this.graphicsDevice = graphics.GraphicsDevice;
            this.content = content;
            this.audioController = audioController;

            font = content.Load<SpriteFont>("Fonts/Menu");
        }

        public void Draw()
        {
            spriteBatch.Draw(background,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            spriteBatch.DrawString(font, titleText, new Microsoft.Xna.Framework.Vector2(graphicsDevice.Viewport.Width / 2 - 60, graphicsDevice.Viewport.Height / 8), Color.WhiteSmoke);
            spriteBatch.DrawString(font, messageText, new Microsoft.Xna.Framework.Vector2(200, 542), Color.WhiteSmoke, 0, new Microsoft.Xna.Framework.Vector2(0, 0), 0.6f, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, "Continue (Enter)", new Microsoft.Xna.Framework.Vector2(graphicsDevice.Viewport.Width - 300, graphicsDevice.Viewport.Height - 100), Color.WhiteSmoke);
        }

        public abstract void Hide();

        public abstract void Show();

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();

            if (KeyboardInput.HasBeenPressed(Keys.Enter))
                MovePassTransition.Invoke(this, EventArgs.Empty);
        }
    }
}
