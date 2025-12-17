using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Entities.Pickups
{
    public class KeyPickup : IPickup
    {
        public Sprite Sprite { get; set; }

        public bool Consumed { get; set; }

        public EventHandler PickedUp { get; set; }

        public KeyPickup(ContentManager content)
        {
            Sprite = new Sprite()
            {
                Texture = content.Load<Texture2D>("Images/key32")
            };

            Consumed = false;
        }

        public void Collect(Player player)
        {
            if (Consumed) return;

            Consumed = true;
            player.HasKey = true;
            PickedUp.Invoke(this, EventArgs.Empty);

            Console.WriteLine("Collected Key!");
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            if (Consumed) return;

            Rectangle rect = new Rectangle(
                (int)Sprite.Position.X,
                (int)Sprite.Position.Y,
                tileSize,
                tileSize
            );

            spriteBatch.Draw(Sprite.Texture, rect, null, Color.AliceBlue);
        }
    }
}
