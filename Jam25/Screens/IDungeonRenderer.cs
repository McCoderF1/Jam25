using Jam25.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Jam25.Screens
{
    /// <summary>
    /// Defines a contract for rendering a dungeon map using a specified drawing context.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for drawing the visual representation of a
    /// dungeon. The rendering behavior may vary depending on the implementation, such as drawing only the background or
    /// the entire map. This interface is typically used in game engines or map editors to abstract the rendering logic
    /// from the underlying graphics framework.</remarks>
    public interface IDungeonRenderer
    {
        /// <summary>
        /// Draws the game map using the specified sprite batch, optionally rendering only the background layers.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch used to draw textures to the screen. Must not be null.</param>
        /// <param name="map">The game map to render. Must not be null.</param>
        /// <param name="backgroundOnly">true to draw only the background layers of the map; otherwise, false to draw the entire map.</param>
        public void Draw(SpriteBatch spriteBatch, GameMap map, bool backgroundOnly);
    }
}
