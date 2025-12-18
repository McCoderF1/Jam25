using Jam25.Entities.Pickups;
using Microsoft.Xna.Framework;

namespace Jam25.Entities.Levels
{
    internal class Dungeon
    {
        private static Color Background = Color.Black;

        private int maxRooms = 10;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        public GameMap Map { get; }

        public Dungeon(int mapWidth, int mapHeight, Player player, KeyPickup key)
        {
            GameSettings.BackgroundColor = Background;

            var dungeonMap = new GameMap(mapWidth, mapHeight);
            dungeonMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight);
            dungeonMap.AddWalls();
            dungeonMap.AddKey(key);
            dungeonMap.AddPlayer(player);
            dungeonMap.AddDoor();

            Map = dungeonMap;
        }
    }
}
