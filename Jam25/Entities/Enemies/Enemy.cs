using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Jam25.Models;

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
