using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<(Func<Vector2, Enemy> createMethod, double spawnRate)> enemySpawnRates;
        private readonly double totalSpawnRate;
        private Random rand;

        #endregion Private Members

        public EnemySpawner(int maxEnemies, int minSpawnDistanceFromPlayer, Func<Vector2> getSpawnPosition, List<(Func<Vector2, Enemy> createMethod, double spawnRate)> enemySpawnRates)
        {
            this.maxEnemies = maxEnemies;
            this.minSpawnDistanceFromPlayer = minSpawnDistanceFromPlayer;
            this.getSpawnPosition = getSpawnPosition;
            this.enemySpawnRates = enemySpawnRates;
            totalSpawnRate = enemySpawnRates.Sum(tuple => tuple.spawnRate);
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

            Enemy newEnemy = null;
            double roll = rand.NextDouble();

            double cumulativeRate = 0.0;
            foreach (var (createMethod, spawnRate) in enemySpawnRates)
            {
                cumulativeRate += (spawnRate / totalSpawnRate);
                if (roll < cumulativeRate)
                {
                    newEnemy = createMethod(spawnPosition);
                    break;
                }
            }

            if (newEnemy is not null)
                gameScene.Enemies.Add(newEnemy);
        }

        #endregion Private Methods
    }
}
