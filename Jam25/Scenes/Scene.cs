using HDT.Gaming.Physics;
using Jam25.Entities;
using Jam25.Entities.Enemies;
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
    }
}
