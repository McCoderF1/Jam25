using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Jam25.Graphics;
using Jam25.Stores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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
        private readonly Texture2D levelBar;
        private readonly Texture2D tab;
        private readonly Texture2D levelMark;
        private readonly SpriteFont font;

        private readonly List<int> levelMarkerLocations = new() { 248, 292, 333, 376, 420 };

        private AnimatedSprite idleLvl1;
        private AnimatedTexture animatedIdleLvl1;

        private AnimatedSprite idleLvl2;
        private AnimatedTexture animatedIdleLvl2;

        private AnimatedSprite idleLvl3;
        private AnimatedTexture animatedIdleLvl3;

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
            levelBar = content.Load<Texture2D>("Textures/WhiteRectangle");

            font = content.Load<SpriteFont>("Fonts/Menu");

            if (game.TryGetSprite(SpriteID.PlayerLvl1, out AnimatedTexture animatedIdleLvl1))
            {
                this.animatedIdleLvl1 = animatedIdleLvl1;
                idleLvl1 = new AnimatedSprite() { SpriteId = SpriteID.PlayerLvl1, ScaleX = 6, ScaleY = 6 };
            }

            if (game.TryGetSprite(SpriteID.PlayerLvl2, out AnimatedTexture animatedIdleLvl2))
            {
                this.animatedIdleLvl2 = animatedIdleLvl2;
                idleLvl2 = new AnimatedSprite() { SpriteId = SpriteID.PlayerLvl2, ScaleX = 6, ScaleY = 6 };
            }

            if (game.TryGetSprite(SpriteID.PlayerLvl3, out AnimatedTexture animatedIdleLvl3))
            {
                this.animatedIdleLvl3 = animatedIdleLvl3;
                idleLvl3 = new AnimatedSprite() { SpriteId = SpriteID.PlayerLvl3, ScaleX = 6, ScaleY = 6 };
            }
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

            var totalLvl = PlayerTracker.PlayerStats.TotalLevel;

            if (totalLvl >= 5 && totalLvl < 10)
                animatedIdleLvl2.DrawFrame(spriteBatch, idleLvl2.Frame, new Vector2(835, 555), idleLvl2);
            else if (totalLvl >= 10)
                animatedIdleLvl3.DrawFrame(spriteBatch, idleLvl3.Frame, new Vector2(835, 555), idleLvl3);
            else
                animatedIdleLvl1.DrawFrame(spriteBatch, idleLvl1.Frame, new Vector2(835, 555), idleLvl1);

            DrawLevelMarkings();
            DrawXPBar();
            DrawPlayerStats();

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

            UpdateSprite(idleLvl1, (float)gameTime.ElapsedGameTime.TotalSeconds);
            UpdateSprite(idleLvl2, (float)gameTime.ElapsedGameTime.TotalSeconds);
            UpdateSprite(idleLvl3, (float)gameTime.ElapsedGameTime.TotalSeconds);

            if (KeyboardInput.HasBeenPressed(Keys.Back))
                BackToMainMenu.Invoke(this, EventArgs.Empty);
        }

        #region private methods

        private void DrawLevelMarkings()
        {
            //Default level markings
            spriteBatch.Draw(levelMark, new Rectangle(204, 331, 25, 36), Color.White);
            spriteBatch.Draw(levelMark, new Rectangle(204, 414, 25, 36), Color.White);
            spriteBatch.Draw(levelMark, new Rectangle(204, 498, 25, 36), Color.White);

            //Health
            for (int i = 0; i < PlayerTracker.PlayerStats.HealthLevel; i++)
                spriteBatch.Draw(levelMark, new Rectangle(levelMarkerLocations[i], 331, 25, 36), Color.White);

            //Torch
            for (int i = 0; i < PlayerTracker.PlayerStats.TorchLevel; i++)
                spriteBatch.Draw(levelMark, new Rectangle(levelMarkerLocations[i], 414, 25, 36), Color.White);

            //Speed
            for (int i = 0; i < PlayerTracker.PlayerStats.SpeedLevel; i++)
                spriteBatch.Draw(levelMark, new Rectangle(levelMarkerLocations[i], 498, 25, 36), Color.White);
        }

        private void DrawXPBar()
        {
            var levelCompletePercentage = (double)PlayerTracker.PlayerStats.EmbersCollected / (double)PlayerTracker.EmbersPerLevel[PlayerTracker.PlayerStats.TotalLevel];
            int barLength = (int)(250 * levelCompletePercentage);

            spriteBatch.Draw(levelBar, new Rectangle(499, 540, barLength, 28), Color.Green);

            spriteBatch.Draw(levelBar, new Rectangle(749, 540, 2, 28), Color.Black);
            spriteBatch.DrawString(font, (PlayerTracker.PlayerStats.TotalLevel + 1).ToString(), new Microsoft.Xna.Framework.Vector2(784, 534), Color.Black);
            spriteBatch.DrawString(font, PlayerTracker.PlayerStats.EmbersCollected + " / " + PlayerTracker.EmbersPerLevel[PlayerTracker.PlayerStats.TotalLevel], new Microsoft.Xna.Framework.Vector2(600, 542), Color.Black, 0, new Microsoft.Xna.Framework.Vector2(0, 0), 0.6f, SpriteEffects.None, 0);
        }

        private void DrawPlayerStats()
        {
            spriteBatch.DrawString(font, "Kills: " + PlayerTracker.PlayerStats.Kills, new Microsoft.Xna.Framework.Vector2(850, 311), Color.White, 0, new Microsoft.Xna.Framework.Vector2(0, 0), 0.6f, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, "Deaths: " + PlayerTracker.PlayerStats.Deaths, new Microsoft.Xna.Framework.Vector2(850, 414 - 20), Color.White, 0, new Microsoft.Xna.Framework.Vector2(0, 0), 0.6f, SpriteEffects.None, 0);

            spriteBatch.DrawString(font, "Rounds Played: " + PlayerTracker.PlayerStats.RoundsPlayed, new Microsoft.Xna.Framework.Vector2(850, 498 - 20), Color.White, 0, new Microsoft.Xna.Framework.Vector2(0, 0), 0.6f, SpriteEffects.None, 0);

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
