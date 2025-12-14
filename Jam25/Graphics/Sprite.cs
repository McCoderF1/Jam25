using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam24.Graphics
{
    public record Sprite(Texture2D Texture, Vector2 Origin)
    {
        public Texture2D Texture { get; set; } = Texture;

        public Vector2 Origin { get; set; } = Origin;
        public Vector2 Position { get; set; }

        public float Scale { get; set; } = 1f;
        public bool IsFacingRight { get; set; }
    }
}
