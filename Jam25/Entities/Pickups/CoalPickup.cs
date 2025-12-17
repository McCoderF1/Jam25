using HDT.Gaming.Audio;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Entities.Pickups
{
    public enum CoalSize
    {
        Small,
        Medium,
        Large,
        Huge
    }

    public class CoalPickup : IPickup
    {
        public Sprite Sprite { get; set; }
        public bool Consumed { get; set; }
        public CoalSize Size { get; }
        public float EnergyAmount { get; }
        
        // Store torch reference to add energy on collect
        public Torch TargetTorch { get; set; }

        private readonly Texture2D spriteSheet;
        private readonly int spriteY = 290;

        public CoalPickup(Vector2 position, CoalSize size, ContentManager content)
        {
            Size = size;
            spriteSheet = content.Load<Texture2D>("Images/supplies_objects");

            EnergyAmount = size switch
            {
                CoalSize.Small => 10f,
                CoalSize.Medium => 20f,
                CoalSize.Large => 35f,
                CoalSize.Huge => 60f,
                _ => 10f
            };

            Sprite = new Sprite
            {
                Position = position,
                Texture = spriteSheet,
                Scale = 2f
            };
        }

        public void Collect(Player player)
        {
            if (Consumed) return;

            // Add energy to torch if it's been set
            if (TargetTorch != null)
            {
                TargetTorch.AddEnergy(EnergyAmount);
            }

            Consumed = true;
            AudioManager.PlaySound("TakeItem");
        }

        public void Draw(SpriteBatch spriteBatch, int tileSize)
        {
            if (Consumed) return;

            int spriteX = (int)Size * 16;
            Rectangle sourceRect = new Rectangle(spriteX, spriteY, 16, 16);

            spriteBatch.Draw(
                spriteSheet,
                Sprite.Position,
                sourceRect,
                Color.White,
                0f,
                new Vector2(8, 8),
                2f,
                SpriteEffects.None,
                0f);
        }
    }
}
