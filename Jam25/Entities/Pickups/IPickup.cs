using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Entities.Pickups
{
    public interface IPickup
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public bool Consumed { get; set; }

        public void Collect(Player player);
        public void Draw(SpriteBatch spriteBatch, int tileSize);
    }
}
