using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

            if(!hasStarted && currentFrame != 0)
                hasStarted = true;

            if(!hasCompleted && hasStarted && currentFrame == 0)
                hasCompleted = true;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
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
        }
    }
}
