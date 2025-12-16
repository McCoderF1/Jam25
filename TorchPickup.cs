using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25
{
    /// <summary>
    /// Pickup that refuels the player torch when collected, player movement and collision detection elsewhere
    /// </summary>
    public class TorchPickup
    {
        public Vector2 Position { get; }
        public float Radius { get; }
        public float EnergyAmount { get; }

        private readonly Texture2D texture;

        public bool IsCollected { get; private set; }

        public TorchPickup(Vector2 position, float radius, float energyAmount, Texture2D texture)
        {
            Position = position;
            Radius = radius;
            EnergyAmount = energyAmount;
            this.texture = texture;
        }

        /// <summary>
        /// Called by external collision logic when the player picks this up
        /// </summary>
        public void Collect(Torch torch)
        {
            if (IsCollected)
            {
                return;
            }

            torch.AddEnergy(EnergyAmount);
            IsCollected = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsCollected)
            {
                return;
            }

            var origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            spriteBatch.Draw(texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}