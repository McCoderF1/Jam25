using System;
using System.Collections.Generic;
using Jam25.Entities.Pickups;
using Jam25.Scenes;
using Microsoft.Xna.Framework;

namespace Jam25.Entities
{
    [Flags]
    public enum WallMask
    {
        None = 0,
        North = 1,
        South = 2,
        West = 4,
        East = 8
    }

    public enum WallShape
    {
        None,
        StraightHorizontal,
        StraightVertical,
        InnerCornerNE,
        InnerCornerNW,
        InnerCornerSE,
        InnerCornerSW,
        OuterCornerNE,
        OuterCornerNW,
        OuterCornerSE,
        OuterCornerSW,
        End,
        Pillar
    }

    public enum TileType
    {
        Empty,
        Floor,
        Wall,
        Door
    }

    public enum DoorOrientation
    {
        Horizontal,
        Vertical
    }

    public class Tile(TileType type)
    {
        public TileType Type = type;
        public WallMask WallMask;
        public WallShape WallShape;

        public DoorOrientation? DoorOrientation;
    }

    public class GameMap
    {
        public Tile[,] tiles;
        private readonly int width;
        private readonly int height;

        public Rectangle[] Rooms { get; private set; }


        public GameMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            InitialiseTiles();
        }

        public void MakeMap(int maxRooms, int minRoomSize, int maxRoomSize, int mapWidth, int mapHeight, GameScene gameScene)
        {
            Rooms = new Rectangle[maxRooms];
            int numRooms = 0;

            Random rand = new Random();

            for (int i = 0; i < maxRooms; i++)
            {
                // Random width and height room
                int width = rand.Next(minRoomSize, maxRoomSize);
                int height = rand.Next(minRoomSize, maxRoomSize);

                // random position of room without going out of bounds of map
                int x = rand.Next(1, mapWidth - width - 1);
                int y = rand.Next(1, mapHeight - height - 1);

                // Rectangle
                Rectangle newRoom = new Rectangle(x, y, width, height);

                // Loop through the rooms and see if they intersect
                foreach (var otherRoom in Rooms)
                {
                    if (newRoom.Intersects(otherRoom))
                        break;
                    else
                    {
                        // No intersection means room valid
                        // Paint room to map
                        CreateRoom(newRoom);

                        // Centre coordinates of new room
                        var newX = newRoom.Center.X;
                        var newY = newRoom.Center.Y;

                        if (numRooms == 0)
                        {
                        }
                        else
                        {
                            // All rooms after the first

                            // Centre coordinates of previous room
                            var prevX = Rooms[numRooms - 1].Center.X;
                            var prevY = Rooms[numRooms - 1].Center.Y;

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
                Rooms[numRooms] = newRoom;
                numRooms += 1;
            }

            // Add walls around floors
            AddWalls(mapWidth, mapHeight);

            ComputeWalls();
        }

        public void AddDoor()
        {
            for (int tries = 0; tries < Rooms.Length; tries++)
            {
                if (PlaceSingleSealedDoor(tiles, Rooms))
                    break;
            }
        }

        public void AddKey(KeyPickup key)
        {
            var rand = new Random();
            int keyRoom = rand.Next(0, Rooms.Length);

            key.Sprite.Position = Rooms[keyRoom].Center.ToVector2() * 32;
        }

        public void AddPlayer(Player player)
        {
            player.Body.Position = Rooms[0].Center.ToVector2() * 32;
        }

        public void Reset()
        {
            InitialiseTiles();
        }

        private bool PlaceSingleSealedDoor(Tile[,] map, Rectangle[] rooms)
        {
            var candidates = GetSealedDoorCandidates(map, rooms);

            if (candidates.Count == 0)
                return false;

            var chosen = candidates[new Random().Next(candidates.Count)];

            Tile tile = map[chosen.x, chosen.y];
            tile.Type = TileType.Door;
            tile.DoorOrientation = chosen.orientation;

            return true;
        }

        private List<(int x, int y, DoorOrientation orientation)> GetSealedDoorCandidates(Tile[,] map, Rectangle[] rooms)
        {
            var candidates = new List<(int, int, DoorOrientation)>();

            foreach (var room in rooms)
            {
                int left = room.Left - 1;
                int right = room.Right;
                int top = room.Top - 1;
                int bottom = room.Bottom;

                // Vertical walls
                // JM: We don't have vertical door graphics at this time
                //for (int y = room.Top; y < room.Bottom; y++)
                //{
                //    if (IsSealedVerticalDoorCandidate(map, left, y))
                //        candidates.Add((left, y, DoorOrientation.Vertical));

                //    if (IsSealedVerticalDoorCandidate(map, right, y))
                //        candidates.Add((right, y, DoorOrientation.Vertical));
                //}

                // Horizontal walls
                for (int x = room.Left + 1; x < room.Right - 1; x++) // Avoid corners
                {
                    if (IsSealedHorizontalDoorCandidate(map, x, top))
                        candidates.Add((x, top, DoorOrientation.Horizontal));

                    if (IsSealedHorizontalDoorCandidate(map, x, bottom))
                        candidates.Add((x, bottom, DoorOrientation.Horizontal));
                }
            }

            return candidates;
        }

        private bool IsSealedVerticalDoorCandidate(Tile[,] map, int x, int y)
        {
            bool northFloor = map[x, y - 1].Type == TileType.Floor;
            bool southFloor = map[x, y + 1].Type == TileType.Floor;

            // Exactly one side touches the room interior
            if (northFloor == southFloor)
                return false;

            return map[x, y].Type == TileType.Wall;
        }

        private bool IsSealedHorizontalDoorCandidate(Tile[,] map, int x, int y)
        {
            bool westFloor = map[x - 1, y].Type == TileType.Floor;
            bool eastFloor = map[x + 1, y].Type == TileType.Floor;

            return map[x, y].Type == TileType.Wall && !westFloor && !eastFloor;
        }


        private void ComputeWalls()
        {
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);

            for (int x = 1; x < w - 1; x++)
                for (int y = 1; y < h - 1; y++)
                {
                    if (tiles[x, y].Type != TileType.Wall)
                        continue;

                    WallMask mask = WallMask.None;

                    if (tiles[x, y - 1].Type == TileType.Floor) mask |= WallMask.North;
                    if (tiles[x, y + 1].Type == TileType.Floor) mask |= WallMask.South;
                    if (tiles[x - 1, y].Type == TileType.Floor) mask |= WallMask.West;
                    if (tiles[x + 1, y].Type == TileType.Floor) mask |= WallMask.East;

                    tiles[x, y].WallMask = mask;
                    tiles[x, y].WallShape = DetermineWallShape(mask);
                }
        }

        private void AddWalls(int mapWidth, int mapHeight)
        {
            for (int x = 1; x < mapWidth - 1; x++)
            {
                for (int y = 1; y < mapHeight - 1; y++)
                {
                    if (tiles[x, y].Type == TileType.Floor)
                    {
                        for (int nx = -1; nx <= 1; nx++)
                            for (int ny = -1; ny <= 1; ny++)
                                if (tiles[x + nx, y + ny].Type == TileType.Empty)
                                    tiles[x + nx, y + ny].Type = TileType.Wall;
                    }
                }
            }
        }

        private WallShape DetermineWallShape(WallMask mask)
        {
            bool n = mask.HasFlag(WallMask.North);
            bool s = mask.HasFlag(WallMask.South);
            bool w = mask.HasFlag(WallMask.West);
            bool e = mask.HasFlag(WallMask.East);

            int count = (n ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0) + (e ? 1 : 0);

            // Pillar (isolated wall)
            if (count == 0)
                return WallShape.Pillar;

            // Ends
            if (count == 1)
                return WallShape.End;

            // Straight walls
            if (n && s && !e && !w)
                return WallShape.StraightVertical;

            if (e && w && !n && !s)
                return WallShape.StraightHorizontal;

            // Inner corners (touching floors)
            if (n && e) return WallShape.InnerCornerNE;
            if (n && w) return WallShape.InnerCornerNW;
            if (s && e) return WallShape.InnerCornerSE;
            if (s && w) return WallShape.InnerCornerSW;

            // Outer corners (missing diagonal floor)
            if (count == 3)
            {
                if (!n) return WallShape.OuterCornerNE;
                if (!s) return WallShape.OuterCornerSE;
                if (!w) return WallShape.OuterCornerNW;
                if (!e) return WallShape.OuterCornerSW;
            }

            return WallShape.None;
        }

        private void CreateRoom(Rectangle room)
        {
            for (int x = room.Left + 1; x < room.Right; x++)
            {
                for (int y = room.Top + 1; y < room.Bottom; y++)
                {
                    // Set map tile to floor
                    this.tiles[x, y].Type = TileType.Floor;
                }
            }
        }

        private void CreateHTunnel(int x1, int x2, int y)
        {
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                tiles[x, y].Type = TileType.Floor;
            }
        }

        private void CreateVTunnel(int y1, int y2, int x)
        {
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                tiles[x, y].Type = TileType.Floor;
            }
        }

        private void InitialiseTiles()
        {
            tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tiles[x, y] = new Tile(TileType.Empty);
                }
            }
        }
    }
}
