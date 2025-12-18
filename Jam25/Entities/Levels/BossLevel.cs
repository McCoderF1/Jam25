using Microsoft.Xna.Framework;

namespace Jam25.Entities.Levels
{
    internal class BossLevel
    {
        private static Color Background = new Color(151, 55, 26);

        public GameMap Map { get; }

        public BossLevel(int mapWidth, int mapHeight, Player player)
        {
            GameSettings.BackgroundColor = Background;
            var lavaMap = new GameMap(100, 100, TileTheme.Lava);
            lavaMap.MakeMap(1, 20, 20, mapWidth, mapHeight);
            lavaMap.AddLavaWalls();
            lavaMap.ComputeDirection();
            lavaMap.AddPlayer(player);

            Map = lavaMap;
        }

    }
}
