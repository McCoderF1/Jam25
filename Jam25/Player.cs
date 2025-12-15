using HDT.Gaming.Physics;
using Jam25.Graphics;
using Jam25.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Jam25
{
    public class Player
    {
        public struct PlayerTexture
        {
            public Texture2D texture;
            public int cols;

            public PlayerTexture(Texture2D texture, int cellSize)
            {
                this.texture = texture;
                cols = texture.Width / cellSize;
            }
        }


        enum Direction { Up, Right, Down, Left }
        private Direction lastDir;

        public enum PlayerState { Idle, Running, Attacking, Hurt, Dying }  // There are textures for walking and run w/ attack too, can include later
        private PlayerState lastState;

        Dictionary<PlayerState, PlayerTexture>[] textures;
        PlayerTexture currentTexture { get => textures[Level - 1][lastState]; }

        // These need to be split into public/private
        private int cellSize;
        private int animationStage;
        private int framesPerAnimation;
        private int frameInAnimation;
        private int textureScale;
        private readonly SpriteBatch spriteBatch;

        public Sprite Sprite { get; set; }

        public Body Body { get; set; }

        public int MovementSpeed { get; set; }

        public Health Health { get; set; }

        public int Level { get; set; }

        public Player(SpriteBatch spriteBatch)
        {
            lastDir = Direction.Down;
            cellSize = 64;
            Health = new(100);
            Level = 1;  // NOTE: level is from 1-3, while level index in texture array is 0-2.
            textureScale = 5;
            textures = new Dictionary<PlayerState, PlayerTexture>[3];

            Body = new Body()
            {
                Owner = this
            };
            this.spriteBatch = spriteBatch;
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            for (int level = 1; level <= 3; level++)
            {
                string prefix = $"PlayerSprite/lvl{level}/";
                var newTextureSet = new Dictionary<PlayerState, PlayerTexture>();
                newTextureSet.Add(PlayerState.Idle, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_Idle_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Running, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_run_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Attacking, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_attack_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Hurt, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_Hurt_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Dying, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_Death_with_shadow"), cellSize));
                //textures.Add(null, new PlayerTexture(content.Load<Texture2D>(prefix + "Images/Swordsman_lvl1_Run_Attack_with_shadow"), cellSize));
                //textures.Add(null, new PlayerTexture(content.Load<Texture2D>(prefix + "Images/Swordsman_lvl1_Walk_with_shadow"), cellSize));
                //textures.Add(null, new PlayerTexture(content.Load<Texture2D>(prefix + "Images/Swordsman_lvl1_Walk_Attack_with_shadow"), cellSize));
                textures[level - 1] = newTextureSet;
            }

            animationStage = 0;
            lastState = PlayerState.Idle;
        }

        private void IncrementAnimation()
        {
            if (frameInAnimation++ >= framesPerAnimation)
            {
                animationStage = (animationStage + 1) % currentTexture.cols;
                frameInAnimation = 0;
            }
        }
        private void ResetAnimation()
        {
            animationStage = 0;
            frameInAnimation = 0;
        }

        public void Update(KeyboardState keyboardState)
        {
            // Placeholder
            if (keyboardState.IsKeyDown(Keys.T))
            {
                TakeDamage(10);
            }
            if (keyboardState.IsKeyDown(Keys.L))
            {
                Level++;
                if (Level == 4)
                    Level = 1;
            }

            // Update based off the current state
            switch (lastState)
            {
                case PlayerState.Idle:
                    IncrementAnimation();

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D)
                    );

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        ResetAnimation();
                        lastState = PlayerState.Attacking;
                    }
                    break;

                case PlayerState.Running:
                    IncrementAnimation();

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D)
                    );
                    //MovePlayer();

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        ResetAnimation();
                        lastState = PlayerState.Attacking;
                    }
                    break;

                case PlayerState.Attacking:
                    IncrementAnimation();
                    if (animationStage == currentTexture.cols - 1)
                    {
                        lastState = PlayerState.Idle;
                    }
                    break;

                case PlayerState.Hurt:
                    IncrementAnimation();
                    if (animationStage == 0)
                    {
                        lastState = PlayerState.Idle;
                    }
                    break;
                case PlayerState.Dying:
                    if (animationStage != currentTexture.cols - 1)
                    {
                        IncrementAnimation();
                    }
                    break;
            }
        }

        public void TakeDamage(int damage)
        {
            if (lastState != PlayerState.Hurt && lastState != PlayerState.Dying)
            {
                Health.TakeDamage(damage);
                animationStage = 0;

                lastState = (Health.Current == 0) ? PlayerState.Dying : PlayerState.Hurt;
            }
        }

        //private void MovePlayer()
        //{
        //    // Move one step
        //    switch (lastDir)
        //    {
        //        case Direction.Up:
        //            Sprite.Position.Y -= vel;
        //            break;
        //        case Direction.Down:
        //            spritePosition.Y += vel;
        //            break;
        //        case Direction.Left:
        //            spritePosition.X -= vel;
        //            break;
        //        case Direction.Right:
        //            spritePosition.X += vel;
        //            break;
        //    }
        //}

        private void FindDirection(bool w, bool a, bool s, bool d)
        {
            int xVel = Convert.ToInt32(d) - Convert.ToInt32(a);
            int yVel = Convert.ToInt32(s) - Convert.ToInt32(w);

            if (yVel < 0)
            {
                lastDir = Direction.Up;
                lastState = PlayerState.Running;
            }
            else if (yVel > 0)
            {
                lastDir = Direction.Down;
                lastState = PlayerState.Running;
            }
            else if (xVel < 0)
            {
                lastDir = Direction.Left;
                lastState = PlayerState.Running;
            }
            else if (xVel > 0)
            {
                lastDir = Direction.Right;
                lastState = PlayerState.Running;
            }
            else
            {
                lastState = PlayerState.Idle;
            }
        }

        public void Draw()
        {
            // Get the row of the matrix to get the sprite from
            int row = 0;
            switch (lastDir)
            {
                case Direction.Up:
                    row = 3;
                    break;
                case Direction.Down:
                    row = 0;
                    break;
                case Direction.Left:
                    row = 1;
                    break;
                case Direction.Right:
                    row = 2;
                    break;
            }

            // Get the column
            int col = animationStage;

            Rectangle sourceRect = new Rectangle(
                col * cellSize,
                row * cellSize,
                cellSize,
                cellSize
            );

            Rectangle destinationRect = new Rectangle(
                (int)Body.Position.X,
                (int)Body.Position.Y,
                cellSize * textureScale,
                cellSize * textureScale
            );

            spriteBatch.Draw(currentTexture.texture, Body.Position, sourceRect, Color.White, 0f, new Vector2(32, 32), Vector2.One, SpriteEffects.None, layerDepth: 1f);
        }
    }
}
