using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Jam25.Stores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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

        private readonly Texture2D winText;
        private readonly Texture2D bigWinText;
        private readonly Texture2D superWinText;
        private readonly Texture2D megaWinText;

        private readonly SpriteFont font;
        private readonly Random random = new Random();

        private bool spinning = false;

        private reelResults reel1 = reelResults.Blank;
        private reelResults reel2 = reelResults.Blank;
        private reelResults reel3 = reelResults.Blank;

        private winTypes win = winTypes.NoWin;

        private enum reelResults
        {
            Blank = 0,
            Cherry = 1,
            Seven = 2,
            Bar = 3,
            Bell = 4,
        }

        private enum winTypes
        {
            NoWin = 0,
            Win = 1,
            BigWin = 2,
            SuperWin = 3,
            MegaWin = 4,
        }

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

            winText = content.Load<Texture2D>("Images/Casino/WIN");
            bigWinText = content.Load<Texture2D>("Images/Casino/BigWin");
            superWinText = content.Load<Texture2D>("Images/Casino/SuperWin");
            megaWinText = content.Load<Texture2D>("Images/Casino/MegaWin");

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

            if (!spinning)
                spriteBatch.Draw(handleUp, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.WhiteSmoke);
            else
                spriteBatch.Draw(handleDown, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.WhiteSmoke);

            DrawReel(reel1, 395);
            DrawReel(reel2, 595);
            DrawReel(reel3, 795);

            DrawWin(win);

            spriteBatch.DrawString(font, "Spin (Space)", new Microsoft.Xna.Framework.Vector2(graphicsDevice.Viewport.Width / 2 - 105, graphicsDevice.Viewport.Height - 70), Color.WhiteSmoke);
            spriteBatch.DrawString(font, "Menu (BackSpace)", new Microsoft.Xna.Framework.Vector2(graphicsDevice.Viewport.Width / 2 - 145, graphicsDevice.Viewport.Height - 35), Color.WhiteSmoke);
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

            if (KeyboardInput.HasBeenPressed(Keys.Space))
            {
                if (!spinning)
                {
                    spinning = true;
                    Spin();
                    CheckWin();
                    Task.Delay(2000).ContinueWith(_ => { spinning = false; ClearReels(); win = winTypes.NoWin; });
                }
            }
        }

        private void Spin()
        {
            spinning = true;
            var numbers = new int[]
            {
                random.Next(1, 5),
                random.Next(1, 5),
                random.Next(1, 5)
            };

            reel1 = (reelResults)numbers[0];
            reel2 = (reelResults)numbers[1];
            reel3 = (reelResults)numbers[2];
        }

        private void DrawReel(reelResults result, int slotX)
        {
            switch (result)
            {
                case reelResults.Blank:
                    break;
                case reelResults.Cherry:
                    spriteBatch.Draw(cherry, new Rectangle(slotX, 360, 96, 96), Color.White);
                    break;
                case reelResults.Seven:
                    spriteBatch.Draw(seven, new Rectangle(slotX, 360, 96, 96), Color.White);
                    break;
                case reelResults.Bar:
                    spriteBatch.Draw(bar, new Rectangle(slotX, 360, 96, 96), Color.White);
                    break;
                case reelResults.Bell:
                    spriteBatch.Draw(bell, new Rectangle(slotX, 360, 96, 96), Color.White);
                    break;
            }
        }

        private void DrawWin(winTypes win)
        {
            switch (win)
            {
                case winTypes.NoWin:
                    break;
                case winTypes.Win:
                    spriteBatch.Draw(winText, new Rectangle(0, 0, graphicsDevice.Viewport.Width, 83), Color.White);
                    break;
                case winTypes.BigWin:
                    spriteBatch.Draw(bigWinText, new Rectangle(0, 0, graphicsDevice.Viewport.Width, 83), Color.White);
                    break;
                case winTypes.SuperWin:
                    spriteBatch.Draw(superWinText, new Rectangle(0, 0, graphicsDevice.Viewport.Width, 83), Color.White);
                    break;
                case winTypes.MegaWin:
                    spriteBatch.Draw(megaWinText, new Rectangle(0, 0, graphicsDevice.Viewport.Width, 83), Color.White);
                    break;
            }
        }

        private void ClearReels()
        {
            reel1 = reelResults.Blank;
            reel2 = reelResults.Blank;
            reel3 = reelResults.Blank;
        }

        private void CheckWin()
        {
            if (reel1 == reel2 && reel2 == reel3)
            {
                if (reel1 is reelResults.Cherry)
                {
                    //10
                    win = winTypes.Win;
                    for (int i = 0; i < 10; i++) { PlayerTracker.CollectEmber(); }
                }
                else if (reel1 is reelResults.Seven)
                {
                    //50
                    win = winTypes.BigWin;
                    for (int i = 0; i < 50; i++) { PlayerTracker.CollectEmber(); }
                }
                else if (reel1 is reelResults.Bell)
                {
                    //100
                    win = winTypes.SuperWin;
                    for (int i = 0; i < 100; i++) { PlayerTracker.CollectEmber(); }
                }
                else if (reel1 is reelResults.Bar)
                {
                    //250
                    win = winTypes.MegaWin;
                    for (int i = 0; i < 250; i++) { PlayerTracker.CollectEmber(); }
                }

                PlayerTracker.SavePlayerProgress();
            }
        }
    }
}
