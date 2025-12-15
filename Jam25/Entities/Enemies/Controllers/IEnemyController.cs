using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jam25.Scenes;
using static System.Formats.Asn1.AsnWriter;

namespace Jam25.Entities.Enemies.Controllers
{
    /// <summary>
    /// Enemy AI controller interface.
    /// </summary>
    public interface IEnemyController
    {

        void Update(Scene scene, Enemy enemy, float deltaTime);
    }
}
