using System;
using System.Collections.Generic;
using Jam25.Scenes;
using Microsoft.Xna.Framework;

namespace Jam25.Entities.Enemies.Controllers
{
    public class BasicEnemyController : IEnemyController
    {
        #region Private Members

        private const int TILE_SIZE = 32;

        #endregion Private Members

        public void Update(GameScene scene, Enemy enemy, TimeSpan deltaTime)
        {
            Tile[,] gameMap = scene.GameMap.tiles;

            bool canSeePlayer = HasSightOfPlayer(scene, enemy);

            if (canSeePlayer)
            {
                enemy.RefreshPlayerSighting();
            }
            else if (!enemy.HasRecentPlayerSighting)
            {
                enemy.MovementDirection = Vector2.Zero;
                return;
            }

            if (canSeePlayer && enemy.AttackRange >= Vector2.Distance(scene.Player.Body.Position, enemy.Body.Position))
            {
                enemy.MovementDirection = Vector2.Zero;
                return;
            }

            (int x, int y)? nextTileIndex = GetNextMoveTile(gameMap, scene.Player.Body.Position, enemy.Body.Position);

            // If no next tile, do not move, or move directly towards the player.
            if (nextTileIndex == null)
            {
                float distanceToPlayer = Vector2.Distance(scene.Player.Body.Position, enemy.Body.Position);
                if (distanceToPlayer < TILE_SIZE && distanceToPlayer > enemy.AttackRange)
                {
                    Vector2 directionToPlayer = scene.Player.Body.Position - enemy.Body.Position;
                    directionToPlayer.Normalize();
                    enemy.MovementDirection = directionToPlayer;
                }
                else
                {
                    enemy.MovementDirection = Vector2.Zero;
                }

                return;
            }

            // calculate direction to centre of next tile.
            Vector2 nextTileCenter = new(
                (nextTileIndex.Value.x * TILE_SIZE) + TILE_SIZE / 2,
                (nextTileIndex.Value.y * TILE_SIZE) + TILE_SIZE / 2);

            Vector2 moveDirection = nextTileCenter - enemy.Body.Position;
            moveDirection.Normalize();
            enemy.MovementDirection = moveDirection;
        }

        #region Private Methods

        private bool HasSightOfPlayer(GameScene scene, Enemy enemy)
        {
            float distance = Vector2.Distance(scene.Player.Body.Position, enemy.Body.Position);

            if (distance >= enemy.SightRange)
            {
                return false;
            }

            Tile[,] gameMap = scene.GameMap.tiles;
            int width = gameMap.GetLength(0);
            int height = gameMap.GetLength(1);

            (int x, int y) enemyTileIndex = ((int)(enemy.Body.Position.X / TILE_SIZE), (int)(enemy.Body.Position.Y / TILE_SIZE));
            (int x, int y) playerTileIndex = ((int)(scene.Player.Body.Position.X / TILE_SIZE), (int)(scene.Player.Body.Position.Y / TILE_SIZE));

            if (!IsWithinBounds(enemyTileIndex, width, height) || !IsWithinBounds(playerTileIndex, width, height))
            {
                return false;
            }

            return HasClearPath(enemyTileIndex, playerTileIndex, gameMap, width, height);
        }

        private bool HasClearPath((int x, int y) start, (int x, int y) end, Tile[,] gameMap, int width, int height)
        {
            int x = start.x;
            int y = start.y;

            int dx = Math.Abs(end.x - start.x);
            int sx = start.x < end.x ? 1 : -1;

            int dy = -Math.Abs(end.y - start.y);
            int sy = start.y < end.y ? 1 : -1;

            int err = dx + dy;

            while (true)
            {
                if ((x, y) != start && !IsWalkable((x, y), gameMap, width, height))
                {
                    return false;
                }

                if (x == end.x && y == end.y)
                {
                    return true;
                }

                int e2 = err << 1;

                if (e2 >= dy)
                {
                    err += dy;
                    x += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }

        private (int x, int y)? GetNextMoveTile(Tile[,] gameMap, Vector2 playerPosition, Vector2 enemyPosition)
        {
            (int x, int y) playerTileIndex = ((int)(playerPosition.X / TILE_SIZE), (int)(playerPosition.Y / TILE_SIZE));
            (int x, int y) enemyTileIndex = ((int)(enemyPosition.X / TILE_SIZE), (int)(enemyPosition.Y / TILE_SIZE));

            int width = gameMap.GetLength(0);
            int height = gameMap.GetLength(1);

            // Within bounds and walkable check
            if (!IsWithinBounds(enemyTileIndex, width, height)
                || !IsWithinBounds(playerTileIndex, width, height)
                || !IsWalkable(enemyTileIndex, gameMap, width, height)
                || !IsWalkable(playerTileIndex, gameMap, width, height))
            {
                return null;
            }

            List<(int x, int y)> path = FindPath(enemyTileIndex, playerTileIndex, gameMap, width, height);

            if (path.Count < 2)
                return null;

            (int x, int y) nextTile = path[1];
            return nextTile;
        }

        private static List<(int x, int y)> FindPath((int x, int y) start, (int x, int y) goal, Tile[,] gameMap, int width, int height)
        {
            PriorityQueue<(int x, int y), float> openSet = new();
            Dictionary<(int x, int y), (int x, int y)> cameFrom = new();
            Dictionary<(int x, int y), float> gScore = new()
            {
                [start] = 0f
            };
            Dictionary<(int x, int y), float> fScore = new()
            {
                [start] = Heuristic(start, goal)
            };
            HashSet<(int x, int y)> closedSet = new();

            openSet.Enqueue(start, fScore[start]);

            while (openSet.Count > 0)
            {
                (int x, int y) current = openSet.Dequeue();

                if (!closedSet.Add(current))
                {
                    continue;
                }

                if (current == goal)
                {
                    return ReconstructPath(current, cameFrom);
                }

                foreach ((int x, int y) neighbor in GetNeighbors(current, gameMap, width, height))
                {
                    if (!IsWalkable(neighbor, gameMap, width, height) || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeGScore = gScore[current] + 1f;

                    if (!gScore.TryGetValue(neighbor, out float existingGScore) || tentativeGScore < existingGScore)
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        float neighborFScore = tentativeGScore + Heuristic(neighbor, goal);
                        fScore[neighbor] = neighborFScore;
                        openSet.Enqueue(neighbor, neighborFScore);
                    }
                }
            }

            return new List<(int x, int y)>();
        }

        private static List<(int x, int y)> ReconstructPath((int x, int y) current, Dictionary<(int x, int y), (int x, int y)> cameFrom)
        {
            List<(int x, int y)> pathNodes = new();
            pathNodes.Add(current);

            while (cameFrom.TryGetValue(current, out (int x, int y) parent))
            {
                current = parent;
                pathNodes.Add(current);
            }

            pathNodes.Reverse();
            return pathNodes;
        }

        private static IEnumerable<(int x, int y)> GetNeighbors((int x, int y) tile, Tile[,] gameMap, int width, int height)
        {
            (int x, int y) east = (tile.x + 1, tile.y);
            (int x, int y) west = (tile.x - 1, tile.y);
            (int x, int y) south = (tile.x, tile.y + 1);
            (int x, int y) north = (tile.x, tile.y - 1);

            bool eastWalkable = IsWalkable(east, gameMap, width, height);
            bool westWalkable = IsWalkable(west, gameMap, width, height);
            bool southWalkable = IsWalkable(south, gameMap, width, height);
            bool northWalkable = IsWalkable(north, gameMap, width, height);

            if (eastWalkable)
            {
                yield return east;
            }

            if (westWalkable)
            {
                yield return west;
            }

            if (southWalkable)
            {
                yield return south;
            }

            if (northWalkable)
            {
                yield return north;
            }

            (int x, int y) southEast = (tile.x + 1, tile.y + 1);
            if (eastWalkable && southWalkable && IsWalkable(southEast, gameMap, width, height))
            {
                yield return southEast;
            }

            (int x, int y) northEast = (tile.x + 1, tile.y - 1);
            if (eastWalkable && northWalkable && IsWalkable(northEast, gameMap, width, height))
            {
                yield return northEast;
            }

            (int x, int y) southWest = (tile.x - 1, tile.y + 1);
            if (westWalkable && southWalkable && IsWalkable(southWest, gameMap, width, height))
            {
                yield return southWest;
            }

            (int x, int y) northWest = (tile.x - 1, tile.y - 1);
            if (westWalkable && northWalkable && IsWalkable(northWest, gameMap, width, height))
            {
                yield return northWest;
            }
        }

        private static float Heuristic((int x, int y) from, (int x, int y) to)
        {
            return MathF.Abs(from.x - to.x) + MathF.Abs(from.y - to.y);
        }

        private static bool IsWalkable((int x, int y) tile, Tile[,] gameMap, int width, int height)
        {
            return IsWithinBounds(tile, width, height) && gameMap[tile.x, tile.y].Type == TileType.Floor;
        }

        private static bool IsWithinBounds((int x, int y) tile, int width, int height)
        {
            return tile.x >= 0 && tile.x < width && tile.y >= 0 && tile.y < height;
        }

        #endregion Private Methods
    }
}
