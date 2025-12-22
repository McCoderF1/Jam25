using HDT.Gaming.Input;
using Jam25.Entities;
using Jam25.Entities.Pickups;
using Jam25.Scenes;
using Jam25.Screens.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Jam25.Screens
{
    /// <summary>
    /// Provides logic for handling player movement, collision detection, and interaction with pickups and level
    /// completion within the game scene.
    /// </summary>
    /// <remarks>This controller processes player movement based on input and game state, ensuring that the
    /// player cannot move through walls or enemies, and manages interactions with keys, doors, and other pickups. It is
    /// intended for use within the game loop to update player state and progress the level when completion conditions
    /// are met.</remarks>
    internal sealed class PlayerMovementController
    {
        #region Private members

        private readonly GameScene gameScene;
        private readonly GameUserInterface gameUI;
        private readonly KeyPickup key;
        private readonly int mapWidth;
        private readonly int mapHeight;
        private readonly int tileSize;

        private const int PlayerColliderHalfWidth = 9;
        private const int PlayerColliderHalfHeight = 1;

        private const int EnemyColliderHalfWidth = 8;
        private const int EnemyColliderHalfHeight = 8;

        #endregion Private members

        public PlayerMovementController(
            GameScene gameScene,
            GameUserInterface gameUI,
            KeyPickup key,
            int mapWidth,
            int mapHeight,
            int tileSize)
        {
            this.gameScene = gameScene;
            this.gameUI = gameUI;
            this.key = key;
            this.mapWidth = mapWidth;
            this.mapHeight = mapHeight;
            this.tileSize = tileSize;
        }

        /// <summary>
        /// Attempts to move the specified player based on current keyboard input and game state, handling collisions,
        /// pickups, and level completion.
        /// </summary>
        /// <remarks>This method processes player movement along the X and Y axes, resolves collisions
        /// with the environment and enemies, and checks for interactions with pickups and doors. If the player
        /// possesses a key and reaches a door, the key is consumed and the level is marked as completed.</remarks>
        /// <param name="gameTime">The current game time, used to update the player's movement and state.</param>
        /// <param name="player">The player to move. Cannot be null.</param>
        /// <returns>true if the player completes the level as a result of this movement; otherwise, false.</returns>
        public bool MovePlayer(GameTime gameTime, Player player)
        {
            bool levelCompleted = false;

            KeyboardInput.GetInput();
            KeyboardState keyboardState = Keyboard.GetState();
            Vector2? probableTargetPosition = player.Update(gameTime, keyboardState);

            if (probableTargetPosition is not null)
            {
                Vector2 currentPos = player.Body.Position;
                Vector2 targetPos = probableTargetPosition.Value;
                Vector2 delta = targetPos - currentPos;

                // 1) Try move along X
                if (delta.X != 0f)
                {

                    Vector2 testPosX = currentPos + new Vector2(delta.X, 0f);

                    Rectangle rectX = new Rectangle(
                        (int)(testPosX.X - PlayerColliderHalfWidth),
                        (int)(testPosX.Y - PlayerColliderHalfHeight),
                        PlayerColliderHalfWidth * 2,
                        PlayerColliderHalfHeight * 2);

                    if (CanMoveTo(rectX))
                    {
                        currentPos = testPosX;
                    }
                }

                // 2) Then try move along Y from updated X position
                if (delta.Y != 0f)
                {
                    Vector2 testPosY = currentPos + new Vector2(0f, delta.Y);

                    Rectangle rectY = new Rectangle(
                        (int)(testPosY.X - PlayerColliderHalfWidth),
                        (int)(testPosY.Y - PlayerColliderHalfHeight),
                        PlayerColliderHalfWidth * 2,
                        PlayerColliderHalfHeight * 2);

                    if (CanMoveTo(rectY))
                    {
                        currentPos = testPosY;
                    }
                }

                // After axis-resolved movement, did we move at all?
                if (currentPos != player.Body.Position)
                {
                    // After axis-resolved movement, did we move at all?
                    if (currentPos != player.Body.Position)
                    {
                        // First resolve soft collisions with enemies
                        Vector2 resolvedPos = ResolvePlayerEnemyCollisions(currentPos, player);
                        player.Body.Position = resolvedPos;

                        Rectangle playerRect = new Rectangle(
                            (int)(resolvedPos.X - PlayerColliderHalfWidth),
                            (int)(resolvedPos.Y - PlayerColliderHalfHeight),
                            PlayerColliderHalfWidth * 2,
                            PlayerColliderHalfHeight * 2);

                        CollectedItem key = gameUI.CollectedItems.Where(item => item.Name == "Key").FirstOrDefault();

                        if (IsOverDoor(playerRect) && key.Name != null)
                        {
                            if (gameUI.CollectedItems.Contains(key))
                                gameUI.CollectedItems.Remove(key);

                            player.MoveSpeed = 0f;
                            levelCompleted = true;
                        }

                        foreach (IPickup pickup in gameScene.Pickups)
                        {
                            if (Vector2.Distance(pickup.Sprite.Position, Vector2.Subtract(player.Body.Position, new Vector2(tileSize / 2, tileSize / 2))) < tileSize)
                            {
                                pickup.Collect(player);
                            }
                        }
                    }
                }
            }

            return levelCompleted;
        }

        #region Private methods

        private bool CanMoveTo(Rectangle playerRect)
        {
            // Convert world rect to tile indices
            int minTileX = Math.Max(0, playerRect.Left / tileSize);
            int maxTileX = Math.Min(mapWidth - 1, playerRect.Right / tileSize);
            int minTileY = Math.Max(0, playerRect.Top / tileSize);
            int maxTileY = Math.Min(mapHeight - 1, playerRect.Bottom / tileSize);

            for (int tx = minTileX; tx <= maxTileX; tx++)
            {
                for (int ty = minTileY; ty <= maxTileY; ty++)
                {
                    TileType type = gameScene.GameMap.tiles[tx, ty].Type;

                    // Block movement on walls always
                    if (type == TileType.Wall1 || type == TileType.Empty)
                    {
                        return false;
                    }

                    // Doors only block if you don't have the key; allow floor always
                    if (type == TileType.Door && !AnyKey())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool AnyKey()
        {
            return gameUI.CollectedItems.Where(item => item.Name == "Key").Any();
        }

        /// <summary>
        /// Resolves collisions between the player and enemies without pushing:
        /// - If the desired position would overlap an enemy, the move is rejected.
        /// - Enemies are never moved.
        /// </summary>
        private Vector2 ResolvePlayerEnemyCollisions(Vector2 desiredPlayerPos, Player player)
        {
            // Rectangle at the desired position
            Rectangle desiredPlayerRect = new Rectangle(
                (int)(desiredPlayerPos.X - PlayerColliderHalfWidth),
                (int)(desiredPlayerPos.Y - PlayerColliderHalfHeight),
                PlayerColliderHalfWidth * 2,
                PlayerColliderHalfHeight * 2);

            foreach (var enemy in gameScene.Enemies)
            {
                Rectangle enemyRect = new Rectangle(
                    (int)(enemy.Body.Position.X - EnemyColliderHalfWidth),
                    (int)(enemy.Body.Position.Y - EnemyColliderHalfHeight),
                    EnemyColliderHalfWidth * 2,
                    EnemyColliderHalfHeight * 2);

                if (desiredPlayerRect.Intersects(enemyRect))
                {
                    // Reject movement into enemy: stay at current position
                    return player.Body.Position;
                }
            }

            // No enemy collision: accept desired position
            return desiredPlayerPos;
        }


        private bool IsOverDoor(Rectangle playerRect)
        {
            int minTileX = Math.Max(0, playerRect.Left / tileSize);
            int maxTileX = Math.Min(mapWidth - 1, playerRect.Right / tileSize);
            int minTileY = Math.Max(0, playerRect.Top / tileSize);
            int maxTileY = Math.Min(mapHeight - 1, playerRect.Bottom / tileSize);

            for (int tx = minTileX; tx <= maxTileX; tx++)
            {
                for (int ty = minTileY; ty <= maxTileY; ty++)
                {
                    if (gameScene.GameMap.tiles[tx, ty].Type == TileType.Door)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Private methods
    }
}
