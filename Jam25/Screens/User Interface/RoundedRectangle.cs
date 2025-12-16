using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Screens.UserInterface
{
    public sealed class RoundedRectangle
    {
        private readonly SpriteBatch spriteBatch;
        private readonly Texture2D pixel;

        public RoundedRectangle(SpriteBatch spriteBatch, Texture2D pixel)
        {
            this.spriteBatch = spriteBatch;
            this.pixel = pixel;
        }

        public void Draw(Rectangle rect, int radius, Color color)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return;
            }

            radius = Math.Min(radius, Math.Min(rect.Width / 2, rect.Height / 2));
            if (radius <= 0)
            {
                spriteBatch.Draw(pixel, rect, color);
                return;
            }

            // Horizontal center
            var centerRect = new Rectangle(
                rect.X + radius,
                rect.Y,
                rect.Width - radius * 2,
                rect.Height);

            // Vertical center
            var verticalRect = new Rectangle(
                rect.X,
                rect.Y + radius,
                rect.Width,
                rect.Height - radius * 2);

            spriteBatch.Draw(pixel, centerRect, color);
            spriteBatch.Draw(pixel, verticalRect, color);

            // Corners
            DrawCornerCircle(rect.X + radius, rect.Y + radius, radius, color, Corner.TopLeft);
            DrawCornerCircle(rect.Right - radius, rect.Y + radius, radius, color, Corner.TopRight);
            DrawCornerCircle(rect.X + radius, rect.Bottom - radius, radius, color, Corner.BottomLeft);
            DrawCornerCircle(rect.Right - radius, rect.Bottom - radius, radius, color, Corner.BottomRight);
        }

        private enum Corner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        private void DrawCornerCircle(int centerX, int centerY, int radius, Color color, Corner corner)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radius * radius)
                    {
                        continue;
                    }

                    bool inCorner = corner switch
                    {
                        Corner.TopLeft => x <= 0 && y <= 0,
                        Corner.TopRight => x >= 0 && y <= 0,
                        Corner.BottomLeft => x <= 0 && y >= 0,
                        Corner.BottomRight => x >= 0 && y >= 0,
                        _ => false
                    };

                    if (!inCorner)
                    {
                        continue;
                    }

                    spriteBatch.Draw(pixel, new Rectangle(centerX + x, centerY + y, 1, 1), color);
                }
            }
        }
    }
}