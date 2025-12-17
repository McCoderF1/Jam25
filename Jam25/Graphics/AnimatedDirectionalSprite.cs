using HDT.Gaming.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Graphics
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public class AnimatedDirectionalSprite
    {
        private readonly int frameWidth;
        private readonly Direction[] directionsOrder;
        private readonly TimeSpan frameTime;
        private readonly int framesPerDirection;
        private TimeSpan accumulatedTime = TimeSpan.Zero;
        private int currentFrame = 0;
        private bool hasCompleted = false;
        private bool hasStarted = false;
        private Direction currentDirection;

        public Texture2D Texture { get; }

        /// <summary>
        /// Gets a value indicating whether the animation loop has completed execution at least once.
        /// </summary>
        public bool LoopCompleted => hasCompleted;

        public AnimatedDirectionalSprite(Texture2D texture, int frameWidth, Direction[] directionsOrder, TimeSpan frameTime)
        {
            Texture = texture;
            this.frameWidth = frameWidth;
            this.directionsOrder = directionsOrder;
            this.frameTime = frameTime;
            framesPerDirection = Texture.Width / frameWidth;

            if (Texture.Width % frameWidth != 0)
                throw new ArgumentException("Texture width must be a multiple of frame width.");

            if (Texture.Height % frameWidth != 0)
                throw new ArgumentException("Texture height must be a multiple of frame width.");

            if (directionsOrder.Length == 0)
                throw new ArgumentException("Directions order must contain at least one direction.");

            if (directionsOrder.Length * frameWidth != Texture.Height)
                throw new ArgumentException("Directions order length must match the number of rows in the texture.");

            currentDirection = directionsOrder[0];
        }

        public void Update(Direction direction, GameTime gameTime)
        {
            if (direction != currentDirection)
            {
                currentDirection = direction;
                currentFrame = 0;
                accumulatedTime = TimeSpan.Zero;
                hasCompleted = false;
                hasStarted = false;
            }

            accumulatedTime += gameTime.ElapsedGameTime;

            while (accumulatedTime >= frameTime)
            {
                accumulatedTime -= frameTime;
                currentFrame = (currentFrame + 1) % framesPerDirection;
            }

            if (!hasStarted && currentFrame != 0)
                hasStarted = true;

            if (!hasCompleted && hasStarted && currentFrame == 0)
                hasCompleted = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, Texture2D? whitePixel = null, Health? health = null)
        {
            if (spriteBatch == null)
                throw new ArgumentNullException(nameof(spriteBatch));

            var directionIndex = Array.IndexOf(directionsOrder, currentDirection);
            if (directionIndex < 0)
                throw new InvalidOperationException("Current direction is not present in the directions order.");

            var sourceRectangle = new Rectangle(
                currentFrame * frameWidth,
                directionIndex * frameWidth,
                frameWidth,
                frameWidth);

            spriteBatch.Draw(
                Texture,
                position,
                sourceRectangle,
                Color.White,
                0f,
                new Vector2(frameWidth / 2, frameWidth / 2),
                Vector2.One,
                SpriteEffects.None,
                layerDepth: 1f);


            // draw the health bar
            if (health is not null && whitePixel is not null)
            {
                float healthPercent = Math.Clamp(health.Current / health.Max, 0f, 1f);
                int barWidth = frameWidth; // match sprite width
                int barHeight = Math.Max(4, frameWidth / 16); // small bar height
                float topOfSpriteY = position.Y - (frameWidth / 2f);
                int barX = (int)(position.X - (barWidth / 2f));
                int barY = (int)(topOfSpriteY - 8 - barHeight); // 8px gap above sprite

                // Background
                var bgRect = new Rectangle(barX, barY, barWidth, barHeight);
                spriteBatch.Draw(whitePixel, bgRect, Color.Black * 0.75f);

                // Fill
                int fillWidth = (int)(barWidth * healthPercent);
                if (fillWidth > 0)
                {
                    var fillRect = new Rectangle(barX + 1, barY + 1, Math.Max(0, fillWidth - 2), barHeight - 2);
                    // color: red -> green
                    var fillColor = Color.Lerp(Color.Red, Color.Green, healthPercent);
                    spriteBatch.Draw(whitePixel, fillRect, fillColor);
                }

                // 1px border
                var borderColor = Color.Black * 0.9f;
                spriteBatch.Draw(whitePixel, new Rectangle(barX, barY, 1, barHeight), borderColor);
                spriteBatch.Draw(whitePixel, new Rectangle(barX + barWidth - 1, barY, 1, barHeight), borderColor);
                spriteBatch.Draw(whitePixel, new Rectangle(barX, barY, barWidth, 1), borderColor);
                spriteBatch.Draw(whitePixel, new Rectangle(barX, barY + barHeight - 1, barWidth, 1), borderColor);
            }
        }
    }
}
