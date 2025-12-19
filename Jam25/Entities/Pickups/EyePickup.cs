using HDT.Gaming.Audio;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Jam25.Entities.Pickups
{
    public class EyePickup : IPickup
    {
        public const float DURATION = 20f;

        public Sprite Sprite { get; set; }

        public bool Consumed { get; set; }

        public EventHandler PickedUp { get; set; }

        public EyePickup(Vector2 position, ContentManager content)
        {
            Sprite = new Sprite()
            {
                Position = position,
                Texture = content.Load<Texture2D>("Images/EyePickup"),
            };

            Consumed = false;
        }

        public void Collect(Player player)
        {
            if (Consumed) return;

            player.SeeThroughWallsTimer = DURATION;

            Consumed = true;
            PickedUp?.Invoke(this, EventArgs.Empty);
            AudioManager.PlaySound("TakeItem");
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
