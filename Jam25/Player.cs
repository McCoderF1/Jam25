using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Pickups;
using Jam25.Graphics;
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

        private float speedMultiplier = 2.5f;

        public enum PlayerState { Idle, Running, Attacking, Hurt, Dying }
        private PlayerState lastState;

        private Dictionary<PlayerState, PlayerTexture>[] textures;
        private PlayerTexture currentTexture { get => textures[Level - 1][lastState]; }

        private int cellSize;
        private int animationStage;
        private int textureScale;
        private readonly SpriteBatch spriteBatch;

        // Time-based animation fields
        private float animationTime;          // Accumulated time for current frame
        private float frameDuration = 0.1f;   // Seconds per frame (10 fps as example)

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
                textures[level - 1] = newTextureSet;
            }

            animationStage = 0;
            animationTime = 0f;
            lastState = PlayerState.Idle;
        }

        private void IncrementAnimation(float deltaSeconds)
        {
            animationTime += deltaSeconds;

            while (animationTime >= frameDuration)
            {
                animationTime -= frameDuration;
                animationStage = (animationStage + 1) % currentTexture.cols;
            }
        }
        private void ResetAnimation()
        {
            animationStage = 0;
            animationTime = 0f;
        }

        public Vector2? Update(GameTime gameTime, KeyboardState keyboardState)
        {
            float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Placeholder
            if (keyboardState.IsKeyDown(Keys.T))
            {
                TakeDamage(10);
            }
            if (keyboardState.IsKeyDown(Keys.L))
            {
                Level++;
                if (Level == 4)
                {
                    Level = 1;
                }
            }

            switch (lastState)
            {
                case PlayerState.Idle:
                    IncrementAnimation(deltaSeconds);

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D));

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        ResetAnimation();
                        lastState = PlayerState.Attacking;
                    }
                    break;

                case PlayerState.Running:
                    IncrementAnimation(deltaSeconds);

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D));

                    if (keyboardState.IsKeyDown(Keys.Space))
                    {
                        ResetAnimation();
                        lastState = PlayerState.Attacking;
                    }
                    return MovePlayer(deltaSeconds);

                case PlayerState.Attacking:
                    IncrementAnimation(deltaSeconds);
                    if (animationStage == currentTexture.cols - 1)
                    {
                        lastState = PlayerState.Idle;
                    }
                    break;

                case PlayerState.Hurt:
                    IncrementAnimation(deltaSeconds);
                    if (animationStage == 0)
                    {
                        lastState = PlayerState.Idle;
                    }
                    break;

                case PlayerState.Dying:
                    if (animationStage != currentTexture.cols - 1)
                    {
                        IncrementAnimation(deltaSeconds);
                    }
                    break;
            }

            return null;
        }

        public void TakeDamage(int damage)
        {
            if (lastState != PlayerState.Hurt && lastState != PlayerState.Dying)
            {
                Health.TakeDamage(damage);
                animationStage = 0;
                animationTime = 0f;

                lastState = (Health.Current == 0) ? PlayerState.Dying : PlayerState.Hurt;
            }
        }

        private Vector2 MovePlayer(float deltaSeconds)
        {
            float dx = 0;
            float dy = 0;

            float moveStep = speedMultiplier * deltaSeconds * 60f; // 60 is a reference FPS for tuning

            switch (lastDir)
            {
                case Direction.Up:
                    dy -= moveStep;
                    break;
                case Direction.Down:
                    dy += moveStep;
                    break;
                case Direction.Left:
                    dx -= moveStep;
                    break;
                case Direction.Right:
                    dx += moveStep;
                    break;
            }

            return Vector2.Add(Body.Position, new Vector2(dx, dy));
        }

        private void FindDirection(bool w, bool a, bool s, bool d)
        {
            int xVel = (Convert.ToInt32(d) - Convert.ToInt32(a)) * 10;
            int yVel = (Convert.ToInt32(s) - Convert.ToInt32(w)) * 10;

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

            int col = animationStage;

            Rectangle sourceRect = new Rectangle(
                col * cellSize,
                row * cellSize,
                cellSize,
                cellSize);

            spriteBatch.Draw(
                currentTexture.texture,
                Body.Position,
                sourceRect,
                Color.White,
                0f,
                new Vector2(32, 32),
                Vector2.One,
                SpriteEffects.None,
                layerDepth: 1f);
        }
    }
}
