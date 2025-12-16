using HDT.Gaming.Models;
using HDT.Gaming.Physics;
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
        #region Private structs/enums

        private struct PlayerTexture
        {
            public Texture2D texture;
            public int cols;

            public PlayerTexture(Texture2D texture, int cellSize)
            {
                this.texture = texture;
                cols = texture.Width / cellSize;
            }
        }

        #endregion 

        #region Private consts

        // Movement mask: all movement-related bits (Idle, Running, Walking)
        private const PlayerState MovementMask = PlayerState.Idle | PlayerState.Running | PlayerState.Walking;
        private const PlayerState AttackMask = PlayerState.Attacking;

        #endregion

        #region Private members

        private enum Direction { Up, Right, Down, Left }
        private Direction lastDir;

        private Dictionary<PlayerState, PlayerTexture>[] textures;
        private PlayerTexture currentTexture
        {
            get
            {
                var state = LastState;

                // Normalize unsupported combinations to known keys.
                // If Idle + Attacking, just use Attacking.
                if ((state & PlayerState.Attacking) != 0 &&
                    (state & (PlayerState.Running | PlayerState.Walking)) == 0)
                {
                    state = PlayerState.Attacking;
                }

                // If somehow Idle combined with others, prefer Idle
                if ((state & PlayerState.Idle) != 0 &&
                    state != PlayerState.Idle)
                {
                    state = PlayerState.Idle;
                }

                return textures[Level - 1][state];
            }
        }

        private int cellSize;
        private int animationStage;
        private int textureScale;
        private readonly SpriteBatch spriteBatch;

        // Time-based animation fields
        private float animationTime;          // Accumulated time for current frame
        private float frameDuration = 0.1f;   // Seconds per frame (10 fps as example)

        private Vector2 movementDirection = Vector2.Zero;

        private bool isAttacking;

        #endregion

        [Flags]
        public enum PlayerState
        {
            Idle = 0x01,
            Running = 0x02,
            Attacking = 0x04,
            Hurt = 0x08,
            Dying = 0x10,
            Walking = 0x20,
        }

        public PlayerState LastState;

        public Sprite Sprite { get; set; }

        public Body Body { get; set; }

        public int MovementSpeed { get; set; }

        public Health Health { get; set; }

        public Stamina Stamina { get; set; }

        public int Level { get; set; }

        public bool IsAttacking => ((LastState & Player.PlayerState.Attacking) != 0);

        public Player(SpriteBatch spriteBatch)
        {
            lastDir = Direction.Down;
            cellSize = 64;
            Health = new(100);
            Stamina = new Stamina(100);
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
                newTextureSet.Add(PlayerState.Running | PlayerState.Attacking, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_run_attack_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Attacking, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_attack_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Hurt, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_Hurt_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Dying, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_Death_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Walking, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_walk_with_shadow"), cellSize));
                newTextureSet.Add(PlayerState.Walking | PlayerState.Attacking, new PlayerTexture(content.Load<Texture2D>($"{prefix}Swordsman_lvl{level}_walk_attack_with_shadow"), cellSize));
                textures[level - 1] = newTextureSet;
            }

            animationStage = 0;
            animationTime = 0f;
            LastState = PlayerState.Idle;
        }

        public Vector2? Update(GameTime gameTime, KeyboardState keyboardState)
        {
            float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool attackKeyDown = keyboardState.IsKeyDown(Keys.Space);
            bool runKeyDown = keyboardState.IsKeyDown(Keys.LeftShift);

            // ... debug T/L

            switch (LastState)
            {
                case PlayerState.Idle:
                    IncrementAnimation(deltaSeconds);

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D),
                        runKeyDown);

                    if (attackKeyDown && !isAttacking)
                    {
                        isAttacking = true;
                        ResetAnimation();
                        LastState = PlayerState.Attacking;

                        Stamina.TakeStamina(3);
                    }
                    else if (!attackKeyDown)
                    {
                        isAttacking = false;

                        Stamina.Restore(5);
                    }
                    break;

                case PlayerState.Running:
                case PlayerState.Attacking | PlayerState.Running:
                    IncrementAnimation(deltaSeconds);

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D),
                        runKeyDown);

                    if (attackKeyDown && !isAttacking)
                    {
                        isAttacking = true;
                        ResetAnimation();
                        LastState |= PlayerState.Attacking;   // becomes Running | Attacking
                        Stamina.TakeStamina(7);
                    }
                    else if (!attackKeyDown && isAttacking && animationStage == currentTexture.cols - 1)
                    {
                        isAttacking = false;
                        ResetAnimation();
                        LastState &= ~PlayerState.Attacking;  // back to Running
                    }

                    Stamina.TakeStamina(0.5f); // running stamina drain

                    return MovePlayer(deltaSeconds, 2.0f);

                case PlayerState.Walking:
                case PlayerState.Attacking | PlayerState.Walking:
                    IncrementAnimation(deltaSeconds);

                    FindDirection(
                        keyboardState.IsKeyDown(Keys.W),
                        keyboardState.IsKeyDown(Keys.A),
                        keyboardState.IsKeyDown(Keys.S),
                        keyboardState.IsKeyDown(Keys.D),
                        runKeyDown);

                    if (attackKeyDown && !isAttacking)
                    {
                        isAttacking = true;
                        ResetAnimation();
                        LastState |= PlayerState.Attacking;   // Walking | Attacking
                        Stamina.TakeStamina(5);
                    }
                    else if (!attackKeyDown && isAttacking && animationStage == currentTexture.cols - 1)
                    {
                        isAttacking = false;
                        ResetAnimation();
                        LastState &= ~PlayerState.Attacking;  // back to Walking
                    }

                    if(!isAttacking)
                    {
                        Stamina.Restore(1);
                    }

                    return MovePlayer(deltaSeconds, 1.0f);

                // Note, this is idle attack only
                case PlayerState.Idle | PlayerState.Attacking:
                case PlayerState.Attacking:
                    IncrementAnimation(deltaSeconds);
                    if (animationStage == currentTexture.cols - 1)
                    {
                        // End of idle attack anim
                        isAttacking = false;
                        LastState = PlayerState.Idle;
                        ResetAnimation();
                    }
                    break;

                // Hurt/Dying unchanged

                case PlayerState.Hurt:
                    IncrementAnimation(deltaSeconds);
                    if (animationStage == currentTexture.cols - 1)
                    {
                        LastState = PlayerState.Idle;
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
            if (LastState != PlayerState.Hurt && LastState != PlayerState.Dying)
            {
                Health.TakeDamage(damage);
                animationStage = 0;
                animationTime = 0f;

                LastState = (Health.Current == 0) ? PlayerState.Dying : PlayerState.Hurt;
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

        #region Private methods

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

        private Vector2 MovePlayer(float deltaSeconds, float speedMultiplier)
        {
            if (movementDirection == Vector2.Zero)
            {
                return Body.Position;
            }

            // Base per-axis speed (what you currently have on pure horizontal/vertical)
            float axisSpeed = speedMultiplier * deltaSeconds * 60f;

            // Length of the input vector (1 for straight, sqrt(2) for perfect diagonal, etc.)
            float length = movementDirection.Length();

            // Apply Pythagoras: total speed = axisSpeed * length
            Vector2 dir = Vector2.Normalize(movementDirection);
            Vector2 offset = dir * axisSpeed;

            return Body.Position + offset;
        }

        private void FindDirection(bool w, bool a, bool s, bool d, bool run)
        {
            // Build movement vector from input
            movementDirection = Vector2.Zero;

            if (w)
            {
                movementDirection.Y -= 1f;
            }

            if (s)
            {
                movementDirection.Y += 1f;
            }

            if (a)
            {
                movementDirection.X -= 1f;
            }

            if (d)
            {
                movementDirection.X += 1f;
            }

            // Preserve non-movement flags (e.g. Attacking)
            var nonMovementFlags = LastState & ~MovementMask;

            if (movementDirection == Vector2.Zero)
            {
                LastState = PlayerState.Idle | nonMovementFlags;
                return;
            }

            // Determine facing direction based on movement vector (for animations)
            if (Math.Abs(movementDirection.X) > Math.Abs(movementDirection.Y))
            {
                lastDir = movementDirection.X < 0 ? Direction.Left : Direction.Right;
            }
            else
            {
                lastDir = movementDirection.Y < 0 ? Direction.Up : Direction.Down;
            }

            var movementState = IsRunning(run) ? PlayerState.Running : PlayerState.Walking;
            LastState = movementState | nonMovementFlags;
        }

        private bool IsRunning(bool runRequest)
            { return runRequest && Stamina.Current > 0; }

        #endregion 
    }
}
