using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam24.Graphics
{
    public class GameDrawer
    {
        private readonly GameContent content;
        private readonly SpriteBatch spriteBatch;

        public GameDrawer(GameContent content, SpriteBatch spriteBatch)
        {
            this.content = content;
            this.spriteBatch = spriteBatch;
        }

        public void DrawText(FontID id, string text, Vector2 position, Color color)
        {
            if (content.TryGetFont(id, out SpriteFont font))
            {
                spriteBatch.DrawString(font, text, position, color);
            }
        }

        public Vector2 MeasureText(FontID id, string text)
        {
            if (content.TryGetFont(id, out SpriteFont font))
            {
                return font.MeasureString(text);
            }

            return Vector2.Zero;
        }

        public void DrawSprite(SpriteID id, Vector2 position, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0f, Vector2? scale = null)
        {
            if (content.TryGetSprite(id, out AnimatedTexture sprite))
            {
                spriteBatch.Draw(sprite.myTexture, position, sourceRectangle, color ?? Color.White, rotation, sprite.Origin, scale ?? Vector2.One, SpriteEffects.None, 0f);
            }
        }

        public void DrawSprite(AnimatedSprite sprite, Vector2 position)
        {
            if (content.TryGetSprite(sprite.SpriteId, out AnimatedTexture texture))
            {
                texture.DrawFrame(spriteBatch, sprite.Frame, position, sprite);
            }
        }

        public void DrawSprite(SpriteID id, AnimatedSprite sprite, Vector2 position, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0f)
        {
            if (content.TryGetSprite(id, out AnimatedTexture texture))
            {
                texture.DrawFrame(spriteBatch, sprite.Frame, position, sprite);
            }
        }
    }
}
