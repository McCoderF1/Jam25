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
    public class GameScreen : IScreen
    {

        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly AudioController audioController;
        private readonly Game1 game;

        #endregion


        public GameScreen(
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

        }

        public void Draw()
        {

        }

        public void Hide()
        {
        }

        public void Show()
        {
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();

        }

    }

    #region private methods


    #endregion

}
