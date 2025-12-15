using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25
{
    /// <summary>
    /// A helper class for handling animated textures.
    /// </summary>
    public class AnimatedTexture
    {
        public int frameCount;
        public Texture2D myTexture;
        public float timePerFrame;
        public Vector2 Origin;

        public AnimatedTexture(Vector2 position)
        {
            this.Origin = position;
        }

        public void Load(ContentManager content, string asset, int frameCount, int framesPerSec)
        {
            this.frameCount = frameCount;
            myTexture = content.Load<Texture2D>(asset);

            timePerFrame = (float)1 / framesPerSec;
        }

        public void DrawFrame(SpriteBatch batch, int frame, Vector2 screenPos, AnimatedSprite animation)
        {
            int frameWidth = myTexture.Width / frameCount;
            Rectangle sourcerect = new Rectangle(frameWidth * frame, 0, frameWidth, myTexture.Height);

            batch.Draw(myTexture, screenPos, sourcerect, Color.White * animation.Opacity,
                animation.Rotation, Origin, new Vector2(animation.ScaleX, animation.ScaleY),
                !animation.IsFacingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                animation.Depth);
        }
    }
}