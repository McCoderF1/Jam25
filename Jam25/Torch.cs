using Microsoft.Xna.Framework;

namespace Jam25
{
    /// <summary>
    /// Represents the player's torch/light source.
    /// </summary>
    public class Torch
    {
        public float MaxEnergy { get; }
        public float Energy { get; private set; }

        /// <summary>
        /// How fast the torch drains per second
        /// </summary>
        public float DrainPerSecond { get; set; }

        /// <summary>
        /// Light radius when the torch is at full energy
        /// </summary>
        public float MaxRadius { get; set; }

        /// <summary>
        /// Minimum radius when the torch is almost out but not fully zero
        /// </summary>
        public float MinRadius { get; set; }

        public bool IsOut => Energy <= 0f;

        public float NormalizedEnergy => MaxEnergy <= 0f ? 0f : Energy / MaxEnergy;

        public float CurrentRadius
        {
            get
            {
                var t = NormalizedEnergy;
                if (t <= 0f)
                {
                    return 0f;
                }

                return MathHelper.Lerp(MinRadius, MaxRadius, t);
            }
        }

        public Torch(float maxEnergy, float drainPerSecond, float maxRadius, float minRadius)
        {
            MaxEnergy = maxEnergy;
            Energy = maxEnergy;
            DrainPerSecond = drainPerSecond;
            MaxRadius = maxRadius;
            MinRadius = minRadius;
        }

        public void Update(GameTime gameTime)
        {
            if (IsOut)
            {
                return;
            }

            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Energy -= DrainPerSecond * dt;

            if (Energy < 0f)
            {
                Energy = 0f;
            }
        }

        public void AddEnergy(float amount)
        {
            Energy += amount;

            if (Energy > MaxEnergy)
            {
                Energy = MaxEnergy;
            }
        }

        public void SetEmpty()
        {
            Energy = 0f;
        }

        public void Reset()
        {
            Energy = MaxEnergy;
        }
    }
}
