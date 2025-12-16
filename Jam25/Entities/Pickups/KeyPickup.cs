using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Entities.Pickups
{
    public class KeyPickup : IPickup
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public bool Consumed { get; set; }

        public KeyPickup(Vector2 position, ContentManager content)
        {
            Texture = content.Load<Texture2D>("Images/key32");
            Consumed = false;
        }
        
        public void Collect(Player player)
        {
            Consumed = true;
            Console.WriteLine("Collected Key!");
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            if (Consumed) return;

            Rectangle rect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                tileSize,
                tileSize
            );

            spriteBatch.Draw(Texture, rect, null, Color.AliceBlue);
        }
    }
}
