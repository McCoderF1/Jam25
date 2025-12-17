using HDT.Gaming.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Screens.Transitions
{
    public class BossScreen : TransitionBase
    {
        public event EventHandler IntoNextArea;

        public BossScreen(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager content, AudioController audioController) : base(spriteBatch, graphics, content, audioController) 
        {
            background = content.Load<Texture2D>("Images/Transitions/Boss");
            titleText = "Boss!";
            messageText = "You have reached a boss area. Prepare for a tough battle ahead.";
        }

        public override void Hide()
        {

        }

        public override void Show()
        {

        }

        public override void Update(GameTime gameTime)
        {

        }
    }
}
