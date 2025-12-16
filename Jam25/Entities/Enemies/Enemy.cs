using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;

namespace Jam25.Entities.Enemies
{
    public class Enemy
    {
        public Body Body { get; }

        public Sprite Sprite { get; }

        public int MovementSpeed { get; }

        public IEnemyController enemyController { get; }

        public Health Health { get; }
    }
}
