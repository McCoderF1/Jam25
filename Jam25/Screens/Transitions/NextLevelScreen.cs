using HDT.Gaming.Audio;
using HDT.Gaming.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Screens.Transitions
{
    public class NextLevelScreen : TransitionBase
    {
        public event EventHandler IntoNextArea;

        public NextLevelScreen(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, ContentManager content, AudioController audioController) : base(spriteBatch, graphics, content, audioController) 
        {
            background = content.Load<Texture2D>("Images/Transitions/NextLevel");
            titleText = "Next Level";
            messageText = "You have reached the next level. Prepare for new challenges ahead.";
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
