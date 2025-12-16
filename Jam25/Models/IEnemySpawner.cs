using Jam25.Scenes;
using Microsoft.Xna.Framework;

namespace Jam25.Models
{
    public interface IEnemySpawner
    {
        void Update(GameScene scene, GameTime gameTime);
    }
}