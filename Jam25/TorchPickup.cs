using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25
{
    public enum TorchPickupSize
    {
        Small,
        Medium,
        Large,
        Huge
    }

    /// <summary>
    /// Pickup that refuels the players torch when collected
    /// </summary>
    public sealed class TorchPickup
    {
        public Vector2 Position { get; }
        public float Radius { get; }
        public float EnergyAmount { get; }
        public TorchPickupSize Size { get; }

        private readonly Sprite sprite;

        public bool IsCollected { get; private set; }

        public TorchPickup(Vector2 position, float radius, float energyAmount, Sprite sprite, TorchPickupSize size)
        {
            Position = position;
            Radius = radius;
            EnergyAmount = energyAmount;
            this.sprite = sprite;
            Size = size;
        }

        /// <summary>
        /// Called by external collision logic when the player picks this up.
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


            spriteBatch.Draw(
                sprite.Texture,
                Position,
                null,
                Color.White,
                0f,
                sprite.Origin,
                sprite.Scale,
                sprite.IsFacingRight ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0f);
        }
    }
}
