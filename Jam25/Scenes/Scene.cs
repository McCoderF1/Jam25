using HDT.Gaming.Physics;
using Jam25.Entities;
using Jam25.Entities.Enemies;
using Jam25.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Jam25.Scenes
{
    /// <summary>
    /// Class representing a game scene.
    /// </summary>
    public class Scene
    {
        public GameMap GameMap { get; }

        public Player Player { get; }

        public List<Enemy> Enemies { get; } = [];

        public PhysicsWorld PhysicsWorld { get; } = new();

        public Scene(GameMap gameMap, Player player)
        {
            GameMap = gameMap;
            Player = player;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var enemy in Enemies)
            {
                enemy.EnemyController?.Update(this, enemy, gameTime.ElapsedGameTime);
                enemy.CurrentSprite.Update(Direction.Down, gameTime);
            }

            PhysicsWorld.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
