using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Graphics
{
    public record Sprite
    {
        public Sprite(Texture2D Texture, Vector2 Origin)
        {
            this.Texture = Texture;
            this.Origin = Origin;
        }

        public Sprite()
        {
        }

        public Texture2D Texture { get; set; }

        public Vector2 Origin { get; set; }
        public Vector2 Position { get; set; }

        public float Scale { get; set; } = 1f;
        public bool IsFacingRight { get; set; }
    }
}
