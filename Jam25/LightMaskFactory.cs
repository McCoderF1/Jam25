using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Graphics
{
    internal static class LightMaskFactory
    {
        /// <summary>
        /// Creates a shadow mask with a transparent center
        /// </summary>
        public static Texture2D CreateRadialMask(GraphicsDevice graphicsDevice, int size)
        {
            var texture = new Texture2D(graphicsDevice, size, size);
            var data = new Color[size * size];

            float radius = size / 2f;
            float center = radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    float t = MathHelper.Clamp(dist / radius, 0f, 1f);

                    // Smooth falloff
                    float alpha = t * t * t;

                    data[y * size + x] = new Color(0, 0, 0, alpha);
                }
            }

            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateTileShadowMask(GraphicsDevice device, int size)
        {
            var texture = new Texture2D(device, size, size);
            var data = new Color[size * size];

            float radius = size / 2f;
            float center = radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    float t = MathHelper.Clamp(dist / radius, 0f, 1f);

                    float alpha = t < 0.85f ? 1f : 1f - ((t - 0.85f) / 0.15f);

                    data[y * size + x] = new Color(0, 0, 0, alpha);
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}