using Jam25.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Entities.Pickups
{
    public interface IPickup
    {
        Sprite Sprite { get; set; }

        public bool Consumed { get; set; }

        public void Collect(Player player);
        public void Draw(SpriteBatch spriteBatch, int tileSize);
    }
}
