using System.Collections.Generic;
using HDT.Gaming.Models;
using HDT.Gaming.Physics;
using Jam25.Entities.Enemies.Controllers;
using Jam25.Graphics;
using Microsoft.Xna.Framework;

namespace Jam25.Entities.Enemies
{
    public class Enemy
    {
        #region Private Members

        private EnemyState currentState = EnemyState.Idle;

        #endregion Private Members

        public enum EnemyState
        {
            Idle,
            Running,
            Attacking,
            Hurt,
            Dying
        }

        public AnimatedDirectionalSprite CurrentSprite => Sprites[currentState];

        public Body Body { get; } = new Body();

        public Dictionary<EnemyState, AnimatedDirectionalSprite> Sprites { get; init; }

        public int MovementSpeed { get; init; }

        public IEnemyController EnemyController { get; init; }

        public Health Health { get; init; }

        public Vector2 MovementDirection { get; set; } = Vector2.Zero;

        public Enemy()
        {
            Body.Owner = this;
        }
    }
}
