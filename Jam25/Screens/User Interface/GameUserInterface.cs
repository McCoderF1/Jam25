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

        private Torch torch;

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

        /// <summary>
        /// Set the torch reference for drawing the torch bar
        /// </summary>
        public void SetTorch(Torch torch)
        {
            this.torch = torch;
        }

        ///<inheritdoc/>
        public void Draw()
        {
            spriteBatch.Draw(UIBase,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            DrawPlayerStatusBars();
            DrawTorchBar();
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

            int x = 95 + margin;
            int yStamina = 110 - barHeight - margin;

            // Draw Stamina Bar
            float staminaPercent = 0f;
            if (player?.Stamina != null && player.Stamina.Max > 0)
            {
                staminaPercent = (float)player.Stamina.Current / player.Stamina.Max;
                staminaPercent = MathHelper.Clamp(staminaPercent, 0f, 1f);
            }

            var staminaBackgroundRect = new Rectangle(x, yStamina, maxBarWidth, barHeight);
            roundedRectangle.Draw(staminaBackgroundRect, cornerRadius, Color.DarkGray);

            int currentStaminaWidth = (int)(maxBarWidth * staminaPercent);
            if (currentStaminaWidth > 0)
            {
                var staminaRect = new Rectangle(x, yStamina, currentStaminaWidth, barHeight);
                roundedRectangle.Draw(staminaRect, cornerRadius, Color.DarkGoldenrod);
            }

            int yHealth = yStamina - barHeight - 5;

            // Draw Health Bar
            float healthPercent = 0f;
            if (player?.Health != null && player.Health.Max > 0)
            {
                healthPercent = (float)player.Health.Current / player.Health.Max;
                healthPercent = MathHelper.Clamp(healthPercent, 0f, 1f);
            }

            var healthBackgroundRect = new Rectangle(x, yHealth, maxBarWidth, barHeight);
            roundedRectangle.Draw(healthBackgroundRect, cornerRadius, Color.DarkGray);

            int currentHealthWidth = (int)(maxBarWidth * healthPercent);
            if (currentHealthWidth > 0)
            {
                var healthRect = new Rectangle(x, yHealth, currentHealthWidth, barHeight);
                roundedRectangle.Draw(healthRect, cornerRadius, Color.DarkRed);
            }
        }

        private void DrawTorchBar()
        {
            if (torch == null) return;

            const int maxBarWidth = 130;
            const int barHeight = 15;
            const int margin = 20;
            const int cornerRadius = 4;

            int x = 95 + margin;
            int yTorch = 106 - (barHeight * 2) - margin + barHeight + 5 + barHeight + 5;

            var torchBackgroundRect = new Rectangle(x, yTorch, maxBarWidth, barHeight);
            roundedRectangle.Draw(torchBackgroundRect, cornerRadius, Color.DarkGray);

            float torchPercent = torch.NormalizedEnergy;
            int currentTorchWidth = (int)(maxBarWidth * torchPercent);
            if (currentTorchWidth > 0)
            {
                Color torchColor = Color.Lerp(Color.Red, Color.Orange, torchPercent);
                var torchRect = new Rectangle(x, yTorch, currentTorchWidth, barHeight);
                roundedRectangle.Draw(torchRect, cornerRadius, torchColor);
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
