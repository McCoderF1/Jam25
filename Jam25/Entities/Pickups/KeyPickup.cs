using System;
using HDT.Gaming.Audio;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Entities.Pickups
{
    public class KeyPickup : IPickup
    {
        public Sprite Sprite { get; set; }

        public bool Consumed { get; set; }

        public EventHandler PickedUp { get; set; }

        public KeyPickup(Texture2D texture)
        {
            Sprite = new Sprite()
            {
                Texture = texture
            };

            Consumed = false;
        }

        public void Collect(Player player)
        {
            if (Consumed) return;

            Consumed = true;
            player.HasKey = true;
            PickedUp.Invoke(this, EventArgs.Empty);
            AudioManager.PlaySound("GetKey");
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

        public void Reset()
        {
            Consumed = false;
        }
    }
}
