using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Entities.Pickups
{
    public class HealthPack : IPickup
    {
        public Sprite Sprite { get; set; }

        public bool Consumed { get; set; }

        private int animationStep;
        private int frameInAnimation;
        private int framesPerAnimation;

        public HealthPack(Vector2 position, ContentManager content)
        {
            animationStep = 0;
            frameInAnimation = 0;
            framesPerAnimation = 20;
            Sprite = new Sprite()
            {

                Position = position,
                Texture = content.Load<Texture2D>("Images/HealthPickup")
            };
        }

        public void Collect(Player player)
        {
            player.Health.Heal(100);
            Consumed = true;
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            if (Consumed) return;

            frameInAnimation++;
            if (frameInAnimation >= framesPerAnimation)
            {
                animationStep++;
                frameInAnimation = 0;
            }

            Rectangle sourceRect = new Rectangle(tileSize * (animationStep % 4), 0, tileSize, tileSize);

            Rectangle rect = new Rectangle(
                (int)Sprite.Position.X,
                (int)Sprite.Position.Y,
                tileSize,
                tileSize
            );

            spriteBatch.Draw(Sprite.Texture, rect, sourceRect, Color.AliceBlue);
        }
    }
}
