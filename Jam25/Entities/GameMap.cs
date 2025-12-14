using Microsoft.Xna.Framework;
using System;

namespace Jam25.NewFolder
{
    public class Tile
    {
        public bool IsBlocked;
        public bool IsBlockSight;
        public Tile(bool isBlocked, bool isBlockSight)
        {
            IsBlocked = isBlocked;
            IsBlockSight = isBlockSight;
        }
    }

    public class Player
    {

    }
    internal class GameMap
    {

        private Tile[,] tiles;
        private readonly int width;
        private readonly int height;

        public void MakeMap(int maxRooms, int minRoomSize, int maxRoomSize, int mapWidth, int mapHeight, Player player)
        {
            Rectangle[] rooms = new Rectangle[maxRooms];
            int numRooms = 0;

            Random rand = new Random();

            for (int i = 0; i < maxRooms; i++)
            {
                // Random width and height room
                int width = rand.Next(minRoomSize, maxRoomSize);
                int height = rand.Next(maxRoomSize, minRoomSize);

                // random position of room without going out of bounds of map
                int x = rand.Next(0, mapWidth - width - 1);
                int y = rand.Next(0, mapHeight - height - 1);

                // Rectangle
                Rectangle newRoom = new Rectangle(x, y, width, height);

                // Loop through the rooms and see if they intersect
                foreach (var otherRoom in rooms)
                {
                    if (newRoom.Intersects(otherRoom))
                        break;
                    else
                    {
                        // No intersection means room valid

                        // Paint room to map


                    }
                }
            }
        }

        public GameMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            InitialiseTiles();
        }

        public void CreateRoom(Rectangle room)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                for (int y = room.Top + 1; y < room.Bottom; y++)
                {
                    // Set map tile to floor
                    this.tiles[x, y].IsBlocked = false;
                    this.tiles[x, y].IsBlockSight = false;
                }
            }
        }

        private void InitialiseTiles()
        {
            tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Set all tiles to blocked
                    tiles[x, y] = new Tile(true, true);
                }
            }
        }
    }
}
