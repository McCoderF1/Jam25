using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDT.Gaming.Physics;
using Jam25.Entities;
using Jam25.Entities.Enemies;

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
