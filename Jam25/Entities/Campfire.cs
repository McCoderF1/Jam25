using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Entities
{
    /// <summary>
    /// Safe campfire that emits an orange glow
    /// </summary>
    public class Campfire
    {
        private readonly Texture2D texture;
        private readonly Texture2D roomGlowTexture;
        private readonly Texture2D innerGlowTexture;
        private readonly Texture2D outerGlowTexture;
        private readonly int frameWidth;
        private readonly int frameHeight;
        private readonly int columns;
        private readonly int rows;
        private readonly int totalFrames;
        private readonly float frameTime;
        
        private float animationTimer;
        private int currentFrame;
        
        // Glow effect
        private readonly Random flickerRandom;
        private float flickerTimer;
        private float innerFlicker;
        private float outerFlicker;
        
        public Vector2 Position { get; set; }
        
        public int DrawSize { get; set; } = 96;

        public Campfire(ContentManager content, GraphicsDevice graphicsDevice, Vector2 position)
        {
            texture = content.Load<Texture2D>("Images/Campfire");
            Position = position;
            columns = 4;
            rows = 2;
            totalFrames = columns * rows;
            frameWidth = texture.Width / columns;
            frameHeight = texture.Height / rows;
            frameTime = 0.12f; 
            animationTimer = 0f;
            currentFrame = 0;
            flickerRandom = new Random();
            flickerTimer = 0f;
            innerFlicker = 1f;
            outerFlicker = 1f;

            roomGlowTexture = CreateRoomGlowTexture(graphicsDevice, 800);
            innerGlowTexture = CreateCircularGlowTexture(graphicsDevice, 400, 0.5f);
            outerGlowTexture = CreateCircularGlowTexture(graphicsDevice, 600, 0.85f); 
        }

        private Texture2D CreateRoomGlowTexture(GraphicsDevice graphicsDevice, int size)
        {
            var texture = new Texture2D(graphicsDevice, size, size);
            var data = new Color[size * size];
            float center = size / 2f;
            float maxDist = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    float t = Math.Min(dist / maxDist, 1f);
                    float alpha = 1f - t;
                    alpha = alpha * alpha * alpha * alpha * alpha;
                    alpha *= 0.32f; 
                    float r = MathHelper.Clamp(MathHelper.Lerp(2.0f, 1.0f, t) * alpha, 0f, 1f);
                    float g = MathHelper.Clamp(MathHelper.Lerp(1.5f, 1.0f, t) * alpha, 0f, 1f);
                    float b = MathHelper.Clamp(MathHelper.Lerp(0.5f, 0.1f, t) * alpha, 0f, 1f);
                    data[y * size + x] = new Color(r, g, b, MathHelper.Clamp(alpha, 0f, 1f));
                }
            }
            texture.SetData(data);
            return texture;
        }

        private Texture2D CreateCircularGlowTexture(GraphicsDevice graphicsDevice, int size, float maxOpacity)
        {
            var texture = new Texture2D(graphicsDevice, size, size);
            var data = new Color[size * size];
            float center = size / 2f;
            float radius = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist > radius)
                    {
                        data[y * size + x] = Color.Transparent;
                        continue;
                    }
                    float t = dist / radius;
                    float alpha = 1f - t;
                    alpha = alpha * alpha * alpha;
                    alpha *= maxOpacity;
                    float r = MathHelper.Clamp(MathHelper.Lerp(2.0f, 1.0f, t) * alpha, 0f, 1f);
                    float g = MathHelper.Clamp(MathHelper.Lerp(1.5f, 1.0f, t) * alpha, 0f, 1f);
                    float b = MathHelper.Clamp(MathHelper.Lerp(0.7f, 0.4f, t) * alpha, 0f, 1f);
                    data[y * size + x] = new Color(r, g, b, MathHelper.Clamp(alpha, 0f, 1f));
                }
            }
            texture.SetData(data);
            return texture;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            animationTimer += dt;
            if (animationTimer >= frameTime)
            {
                animationTimer -= frameTime;
                currentFrame = (currentFrame + 1) % totalFrames;
            }
            
            flickerTimer += dt;
            if (flickerTimer >= 0.08f)
            {
                flickerTimer = 0f;
                float innerNoise = (float)(flickerRandom.NextDouble() * 0.15 - 0.075);
                innerFlicker = 1f + innerNoise;
                
                float outerNoise = (float)(flickerRandom.NextDouble() * 0.08 - 0.04);
                outerFlicker = 1f + outerNoise;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int frameX = currentFrame % columns;
            int frameY = currentFrame / columns;
            var sourceRect = new Rectangle(
                frameX * frameWidth,
                frameY * frameHeight,
                frameWidth,
                frameHeight);


            float roomGlowSize = 800f;
            var roomGlowRect = new Rectangle(
                (int)Math.Floor(Position.X - roomGlowSize / 2f),
                (int)Math.Floor(Position.Y - roomGlowSize / 2f),
                (int)Math.Floor(roomGlowSize),
                (int)Math.Floor(roomGlowSize));
            spriteBatch.Draw(roomGlowTexture, roomGlowRect, Color.White);

            float outerSize = 320f * outerFlicker;
            var outerGlowRect = new Rectangle(
                (int)Math.Floor(Position.X - outerSize / 2f),
                (int)Math.Floor(Position.Y - outerSize / 2f),
                (int)Math.Floor(outerSize),
                (int)Math.Floor(outerSize));
            spriteBatch.Draw(outerGlowTexture, outerGlowRect, Color.White);


            float innerSize = 200f * innerFlicker;
            var innerGlowRect = new Rectangle(
                (int)Math.Floor(Position.X - innerSize / 2f),
                (int)Math.Floor(Position.Y - innerSize / 2f),
                (int)Math.Floor(innerSize),
                (int)Math.Floor(innerSize));
            spriteBatch.Draw(innerGlowTexture, innerGlowRect, Color.White);

            int evenDrawSize = (DrawSize % 2 == 0) ? DrawSize : DrawSize + 1;
            var destRect = new Rectangle(
                (int)Math.Floor(Position.X - evenDrawSize / 2f),
                (int)Math.Floor(Position.Y - evenDrawSize / 2f),
                evenDrawSize,
                evenDrawSize);
            spriteBatch.Draw(texture, destRect, sourceRect, Color.White);
        }
    }
}
