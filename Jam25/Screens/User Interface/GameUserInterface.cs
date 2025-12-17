using HDT.Gaming.Audio;
using HDT.Gaming.Screens;
using Jam25.Entities.Pickups;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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

        private KeyPickup keyPickup;
        private Torch torch;

        private AnimatedSprite playerIcon;
        private AnimatedTexture animatedPlayerIcon;

        private Vector2 currentCameraPosition = Vector2.Zero;

        private const int maxBarWidth = 130;

        #endregion

        /// <summary>
        /// Item collected by the player for UI display
        /// </summary>
        private struct CollectedItem
        {
            public Texture2D Texture;
            public string Name;
        }

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
            keyTexture = content.Load<Texture2D>("Images/key32");

            // Try to load player icon sprite
            if (game.TryGetSprite(SpriteID.PlayerLvl1, out AnimatedTexture animatedIcon))
            {
                this.animatedPlayerIcon = animatedIcon;
                playerIcon = new AnimatedSprite() { SpriteId = SpriteID.PlayerLvl1, ScaleX = 4, ScaleY = 4 };
            }

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

        public void SetKey(KeyPickup key)
        {
            this.keyPickup = key;
        }

        ///<inheritdoc/>
        public void Draw()
        {
            // Draw UI at fixed screen position (0,0) - static, not moving with camera
            spriteBatch.Draw(UIBase,
                new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height),
                Color.White);

            DrawPlayerStatusBars();
            DrawTorchBar();
            DrawTimer();

            if (keyPickup != null)
            {
                //var testKey = new KeyPickup(content);
                spriteBatch.Draw(keyPickup.Sprite.Texture,
                    new Rectangle(maxBarWidth - 20, 125, 32, 32),
                    null,
                    Color.White);
            }
            DrawInformation();
            DrawCollectedItems();
        }

        ///<inheritdoc/>
        public void Hide()
        {

        }

        ///<inheritdoc/>
        public void Show()
        {
            ClearItems();
        }

        ///<inheritdoc/>
        public void UpdateWithVector(GameTime gameTime, Vector2 cameraPosition)
        {
            currentCameraPosition = cameraPosition;
            Update(gameTime);
        }

        public void Update(GameTime gameTime)
        {
            if (playerIcon != null)
                UpdateSprite(playerIcon, (float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        #region private methods

        /// <summary>
        /// Displays the player status bars (health, stamina)
        /// </summary>
        private void DrawPlayerStatusBars()
        {
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

        /// <summary>
        /// Displays the torch energy bar
        /// </summary>
        private void DrawTorchBar()
        {
            if (torch == null) return;

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
            // Timer placeholder
        }

        private void DrawInformation()
        {
            spriteBatch.DrawString(font, "Floor 1", new Vector2(1127, 60), Color.White);
        }

        /// <summary>
        /// Draws collected items in the UI
        /// </summary>
        private void DrawCollectedItems()
        {
            const int slotSize = 35;
            const int slotSpacing = 2;
            const int margin =11;
            const int maxSlots = 4;

            int startX = graphicsDevice.Viewport.Width - margin - (slotSize * maxSlots) - (slotSpacing * (maxSlots - 1));
            int y = graphicsDevice.Viewport.Height - margin - slotSize;

            for (int i = 0; i < maxSlots; i++)
            {
                int slotX = startX + (i * (slotSize + slotSpacing));

                var slotRect = new Rectangle(slotX, y, slotSize, slotSize);
                roundedRectangle.Draw(slotRect, 4, Color.Black * 0.5f);

                if (i < collectedItems.Count)
                {
                    var item = collectedItems[i];
                    int itemSize = slotSize - 8;
                    var itemRect = new Rectangle(slotX + 4, y + 4, itemSize, itemSize);
                    spriteBatch.Draw(item.Texture, itemRect, Color.White);
                }
            }
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
