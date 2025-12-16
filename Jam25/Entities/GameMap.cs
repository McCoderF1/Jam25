using Microsoft.Xna.Framework;
using System;

namespace Jam25.Entities
{
    public enum TileType
    {
        Empty,
        Floor,
        Wall
    }

    public class GameMap
    {
        public TileType[,] tiles;
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
                int height = rand.Next(minRoomSize, maxRoomSize);

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
                        CreateRoom(newRoom);

                        // Center coordinates of new room

                        var newX = newRoom.Center.X;
                        var newY = newRoom.Center.Y;

                        if (numRooms == 0)
                        {
                            // This is the first room where the player starts
                            player.Body.Position = new Vector2(newX * 32, newY * 32);
                        }
                        else
                        {
                            // All rooms after the first

                            // Centre coordinates of previous room
                            var prevX = rooms[numRooms - 1].Center.X;
                            var prevY = rooms[numRooms - 1].Center.Y;

                            // Flip a coin
                            if (rand.Next(0, 1) == 0)
                            {
                                // Horizontal then vertical
                                CreateHTunnel(prevX, newX, prevY);
                                CreateVTunnel(prevY, newY, newX);

                            }
                            else
                            {
                                // Vertical then horizontal
                                CreateVTunnel(prevY, newY, prevX);
                                CreateHTunnel(prevX, newX, newY);
                            }
                        }
                    }
                }

                // Add new room to list
                rooms[numRooms] = newRoom;
                numRooms += 1;
            }

            // Add walls around floors
            //AddWalls(mapWidth, mapHeight);

        }

        private void AddWalls(int mapWidth, int mapHeight)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    if (tiles[x, y] == TileType.Floor)
                    {
                        for (int nx = -1; nx <= 1; nx++)
                            for (int ny = -1; ny <= 1; ny++)
                                if (tiles[x + nx, y + ny] == TileType.Empty)
                                    tiles[x + nx, y + ny] = TileType.Wall;
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
                    this.tiles[x, y] = TileType.Floor;
                }
            }
        }

        public void CreateHTunnel(int x1, int x2, int y)
        {
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                tiles[x, y] = TileType.Floor;
            }
        }

        public void CreateVTunnel(int y1, int y2, int x)
        {
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                tiles[x, y] = TileType.Floor;
            }
        }

        public void InitialiseTiles()
        {
            tiles = new TileType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = TileType.Wall;
                }
            }
        }
    }
}
