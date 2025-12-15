using HDT.Gaming.Physics;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Jam25
{
    internal class Player
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

        Dictionary<PlayerState, PlayerTexture> textures;

        /*private PlayerTexture attack;
        private PlayerTexture idle;
        private PlayerTexture run;
        private PlayerTexture dying;
        private PlayerTexture hurt;*/

        private int vel;
        int animationStage;
        int cellSize;
        int health;
        private int textureScale;
        private readonly SpriteBatch spriteBatch;

        public Sprite Sprite { get; set; }

        public Body Body { get; set; }

        public int MovementSpeed { get; set; }

        public Player(SpriteBatch spriteBatch)
        {
            lastDir = Direction.Up;
            cellSize = 64;
            health = 100;
            textureScale = 5;
            textures = new Dictionary<PlayerState, PlayerTexture>();

            Body = new Body()
            {
                Owner = this
            };
            this.spriteBatch = spriteBatch;
        }

        public void Initalise(Microsoft.Xna.Framework.Content.ContentManager content, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            // Can add the level 2, 3 textures later
            textures.Add(PlayerState.Idle, new PlayerTexture(content.Load<Texture2D>("PlayerSprite/lvl1/Swordsman_lvl1_idle_with_shadow"), cellSize));
            textures.Add(PlayerState.Running, new PlayerTexture(content.Load<Texture2D>("PlayerSprite/lvl1/Swordsman_lvl1_run_with_shadow"), cellSize));
            textures.Add(PlayerState.Attacking, new PlayerTexture(content.Load<Texture2D>("PlayerSprite/lvl1/Swordsman_lvl1_attack_with_shadow"), cellSize));
            textures.Add(PlayerState.Hurt, new PlayerTexture(content.Load<Texture2D>("PlayerSprite/lvl1/Swordsman_lvl1_Hurt_with_shadow"), cellSize));
            textures.Add(PlayerState.Dying, new PlayerTexture(content.Load<Texture2D>("PlayerSprite/lvl1/Swordsman_lvl1_Death_with_shadow"), cellSize));
            //textures.Add(null, new PlayerTexture(content.Load<Texture2D>("Images/Swordsman_lvl1_Run_Attack_with_shadow"), cellSize));
            //textures.Add(null, new PlayerTexture(content.Load<Texture2D>("Images/Swordsman_lvl1_Walk_with_shadow"), cellSize));
            //textures.Add(null, new PlayerTexture(content.Load<Texture2D>("Images/Swordsman_lvl1_Walk_Attack_with_shadow"), cellSize));

            vel = 3;
            animationStage = 0;
            lastState = PlayerState.Idle;
        }

        public void Update(KeyboardState keyboardState)
        {
            // Placeholder
            if (keyboardState.IsKeyDown(Keys.T) && (lastState != PlayerState.Hurt && lastState != PlayerState.Dying))
            {
                TakeDamage(10);
            }

            // Update based off the current state
            switch (lastState)
            {
                case PlayerState.Idle:
                    animationStage = (animationStage + 1) % 4;

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D)
                    );

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        lastState = PlayerState.Attacking;
                    }
                    break;

                case PlayerState.Running:
                    animationStage = (animationStage + 1) % 4;

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D)
                    );
                    //MovePlayer();

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        lastState = PlayerState.Attacking;
                    }
                    break;

                case PlayerState.Attacking:
                    animationStage++;
                    if (animationStage == textures[lastState].cols)
                    {
                        animationStage = 0;
                        lastState = PlayerState.Idle;
                    }
                    break;

                case PlayerState.Hurt:
                    animationStage++;
                    if (animationStage == textures[lastState].cols)
                    {
                        animationStage = 0;
                        lastState = PlayerState.Idle;
                    }
                    break;
                case PlayerState.Dying:
                    animationStage++;
                    if (animationStage >= textures[lastState].cols)
                    {
                        animationStage = textures[lastState].cols - 1;
                    }
                    break;
            }
        }

        public void TakeDamage(int damage)
        {
            health = Math.Max(0, health - damage);
            animationStage = 0;

            lastState = (health == 0) ? PlayerState.Dying : PlayerState.Hurt;
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
            int col = animationStage % textures[lastState].cols;

            Rectangle sourceRect = new Rectangle(
                col * cellSize /*+ (cellSize / 4)*/,
                row * cellSize/* + (cellSize / 4)*/,
                cellSize /*/ 2*/,
                cellSize /*/ 2*/
            );

            Rectangle destinationRect = new Rectangle(
                (int)Body.Position.X,
                (int)Body.Position.Y,
                cellSize * textureScale,
                cellSize * textureScale
            );

            // To make the sprite larger
            spriteBatch.Draw(textures[lastState].texture, Body.Position, new Rectangle(0, 0, 64, 64), Color.White, 0f, new Vector2(32, 32), Vector2.One, SpriteEffects.None, layerDepth: 1f);
        }
    }
}
