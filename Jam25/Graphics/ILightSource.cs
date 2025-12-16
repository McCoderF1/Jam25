using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Jam25.Graphics
{
    public interface ILightSource
    {
        Vector2 Position { get; }

        float Radius { get; }
    }
}
