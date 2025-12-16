using HDT.Gaming.Audio;
using HDT.Gaming.Screens;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Screens.UserInterface
{
    /// <summary>
    /// User interface overall during the main game loop
    /// </summary>
    public class GameUserInterface : IScreenUI
    {
        #region private members

        private readonly SpriteBatch spriteBatch;
        private readonly GameContent game;
        private readonly GraphicsDevice graphicsDevice;
        private readonly ContentManager content;
        private readonly AudioController audioController;
        private readonly Player player;
        private readonly Texture2D UIBase;
        private readonly SpriteFont font;
        private readonly Texture2D whitePixel;
        private readonly RoundedRectangle roundedRectangle;

        private Vector2 currentCameraPosition = Vector2.Zero;

        #endregion

        /// <summary>
        /// Game User Interface constructor
        /// </summary>
        public GameUserInterface(SpriteBatch spriteBatch, GraphicsDevice graphics, GameContent game, ContentManager content, AudioController audioController, Player player)
        {
            this.spriteBatch = spriteBatch;
            this.game = game;
            this.graphicsDevice = graphics;
            this.content = content;
            this.audioController = audioController;
            this.player = player; 

            UIBase = content.Load<Texture2D>("Images/UI/UIBase");
            font = content.Load<SpriteFont>("Fonts/Menu");
            whitePixel = new Texture2D(graphics, 1, 1);
            whitePixel.SetData(new[] { Color.White });
            roundedRectangle = new RoundedRectangle(spriteBatch, whitePixel);
        }

        ///<inheritdoc/>
        public void Draw()
        {
            DrawPlayerStatusBars();
            spriteBatch.Draw(UIBase,
                new Rectangle((int)currentCameraPosition.X, (int)currentCameraPosition.Y, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

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
        public void UpdateWithVector(GameTime gameTime, Vector2 cameraPosition)
        {
            currentCameraPosition = cameraPosition;
            Update(gameTime);
        }

        public void Update(GameTime gameTime)
        {

        }

        #region private methods

        private void DrawPlayerStatusBars()
        {
            const int maxBarWidth = 130;
            const int barHeight = 15;
            const int margin = 20;
            const int cornerRadius = 4;

            int x = (int)currentCameraPosition.X + 95 + margin;
            int y = (int)currentCameraPosition.Y + 110 - barHeight - margin;

            float staminaPercent = 0f;
            if (player?.Stamina != null && player.Stamina.Max > 0)
            {
                staminaPercent = (float)player.Stamina.Current / player.Stamina.Max;
                staminaPercent = MathHelper.Clamp(staminaPercent, 0f, 1f);
            }

            // Draw full-size background as a frame
            var backgroundRect = new Rectangle(x, y, maxBarWidth, barHeight);
            roundedRectangle.Draw(backgroundRect, cornerRadius, Color.DarkGray);

            // Scale the visible bar width by remaining stamina
            int currentBarWidth = (int)(maxBarWidth * staminaPercent);
            if (currentBarWidth > 0)
            {
                var staminaRect = new Rectangle(x, y, currentBarWidth, barHeight);
                roundedRectangle.Draw(staminaRect, cornerRadius, Color.DarkGoldenrod);
            }
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
