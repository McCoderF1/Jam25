using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Jam25
{
    public class GameContent
    {
        private readonly ContentManager contentManager;

        public readonly Dictionary<SpriteID, AnimatedTexture> sprites = new();
        private readonly Dictionary<FontID, SpriteFont> fonts = new();

        public GameContent(ContentManager contentManager)
        {
            this.contentManager = contentManager;
        }

        public void LoadSprite(SpriteID id, string path, int frames, int fps, Vector2? origin = null)
        {
            AnimatedTexture animation = new(origin ?? Vector2.Zero);
            animation.Load(contentManager, path, frames, fps);

            sprites.Add(id, animation);
        }

        public void LoadSprite(SpriteID id, string path, Vector2? origin = null)
        {
            AnimatedTexture animation = new(origin ?? Vector2.Zero);
            animation.Load(contentManager, path, 1, 1);

            sprites.Add(id, animation);
        }

        public void LoadFont(FontID id, string path)
        {
            SpriteFont font = contentManager.Load<SpriteFont>(path);
            fonts.Add(id, font);
        }

        public bool TryGetSprite(SpriteID id, out AnimatedTexture sprite)
        {
            return sprites.TryGetValue(id, out sprite);
        }

        public bool TryGetFont(FontID id, out SpriteFont font)
        {
            return fonts.TryGetValue(id, out font);
        }
    }
}
