using System;
using Jam25.Entities.Enemies;
using Jam25.Scenes;
using Microsoft.Xna.Framework;

namespace Jam25.Models
{
    /// <summary>
    /// Class responsible for spawning enemies in the game.
    /// </summary>
    public class EnemySpawner : IEnemySpawner
    {
        #region Private Members

        private readonly int maxEnemies;
        private readonly int minSpawnDistanceFromPlayer;
        private readonly Func<Vector2> getSpawnPosition;
        private readonly EnemyFactory enemyFactory;
        private Random rand;

        #endregion Private Members

        public EnemySpawner(int maxEnemies, int minSpawnDistanceFromPlayer, Func<Vector2> getSpawnPosition, EnemyFactory enemyFactory)
        {
            this.maxEnemies = maxEnemies;
            this.minSpawnDistanceFromPlayer = minSpawnDistanceFromPlayer;
            this.getSpawnPosition = getSpawnPosition;
            this.enemyFactory = enemyFactory;
            rand = new Random();
        }

        /// <summary>
        /// Spawns enemies if the current count is below the maximum limit.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameScene gameScene, GameTime gameTime)
        {
            while (gameScene.Enemies.Count < maxEnemies)
            {
                SpawnEnemy(gameScene);
            }
        }

        #region Private Methods

        private void SpawnEnemy(GameScene gameScene)
        {
            Vector2 playerPosition = gameScene.Player.Body.Position;

            Vector2 spawnPosition;

            do
            {
                spawnPosition = getSpawnPosition();
            }
            while (Vector2.Distance(spawnPosition, playerPosition) <= minSpawnDistanceFromPlayer);

            Enemy newEnemy;
            double roll = rand.NextDouble();

            if (roll < 0.15)
            {
                // 15% chance for vampire
                newEnemy = enemyFactory.CreateVampireEnemy(spawnPosition);
            }
            else if (roll < 0.25)
            {
                // 10% chance for Orc2 (heavy tank)
                newEnemy = enemyFactory.CreateOrc2Enemy(spawnPosition);
            }
            else if (roll < 0.35)
            {
                // 10% chance for Orc1 (light tank)
                newEnemy = enemyFactory.CreateOrc1Enemy(spawnPosition);
            }
            else if (roll < 0.55)
            {
                // 20% chance for lava slime
                newEnemy = enemyFactory.CreateLavaSlimeEnemy(spawnPosition);
            }
            else
            {
                // 45% chance for regular slime
                newEnemy = enemyFactory.CreateSlimeEnemy(spawnPosition);
            }

            gameScene.Enemies.Add(newEnemy);
        }

        #endregion Private Methods
    }
}
