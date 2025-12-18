using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Screens
{
    public class CasinoScreen : IScreen
    {
        #region private members

        private readonly SpriteBatch spriteBatch;
        private readonly GameContent game;
        private readonly GraphicsDeviceManager graphics;
        private readonly GraphicsDevice graphicsDevice;
        private readonly ContentManager content;
        private readonly AudioController audioController;

        private readonly Texture2D background;
        private readonly Texture2D slotMachine;
        private readonly Texture2D bar;
        private readonly Texture2D seven;
        private readonly Texture2D bell;
        private readonly Texture2D cherry;
        private readonly Texture2D handleUp;
        private readonly Texture2D handleDown;

        private readonly SpriteFont font;

        #endregion

        public event EventHandler BackToMainMenu;

        public CasinoScreen(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, GameContent game, ContentManager content, AudioController audioController)
        {
            this.spriteBatch = spriteBatch;
            this.game = game;
            this.graphics = graphics;
            this.graphicsDevice = graphics.GraphicsDevice;
            this.content = content;
            this.audioController = audioController;

            background = content.Load<Texture2D>("Images/Casino/Background");
            slotMachine = content.Load<Texture2D>("Images/Casino/SlotMachine");
            bell = content.Load<Texture2D>("Images/Casino/Bell");
            cherry = content.Load<Texture2D>("Images/Casino/Cherry");
            bar = content.Load<Texture2D>("Images/Casino/Bar");
            seven = content.Load<Texture2D>("Images/Casino/Seven");
            handleUp = content.Load<Texture2D>("Images/Casino/HandleUp");
            handleDown = content.Load<Texture2D>("Images/Casino/HandleDown");

            font = content.Load<SpriteFont>("Fonts/Menu");
        }

        public void Draw()
        {
            spriteBatch.Draw(background,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            spriteBatch.Draw(slotMachine,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), 
                Color.White);

            spriteBatch.DrawString(font, "Menu (BackSpace)", new Microsoft.Xna.Framework.Vector2(graphicsDevice.Viewport.Width - 420, graphicsDevice.Viewport.Height - 100), Color.WhiteSmoke);
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

            if (KeyboardInput.HasBeenPressed(Keys.Back))
                BackToMainMenu.Invoke(this, EventArgs.Empty);
        }
    }
}
