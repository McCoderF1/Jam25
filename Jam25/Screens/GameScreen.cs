using HDT.Gaming.Audio;
using HDT.Gaming.Input;
using HDT.Gaming.Screens;
using Jam25.Entities;
using Jam25.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Jam25.Screens
{
    public class GameScreen : IScreen
    {

        #region private members

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteBatch spriteBatch;
        private readonly AudioController audioController;
        private readonly Game1 game;
        private readonly Scene gameScene;
        private Texture2D wallsFloor;
        private GameMap gameMap;

        private int mapWidth = 80;
        private int mapHeight = 42;

        private int maxRooms = 30;
        private int maxRoomSize = 10;
        private int minRoomSize = 6;

        private int tileSize = 32;

        private Player player;

        #endregion


        public GameScreen(
            GraphicsDevice gfxDevice,
            SpriteBatch spriteBatch,
            ContentManager content,
            AudioController audioController,
            Game1 game)
        {
            this.graphicsDevice = gfxDevice;
            this.spriteBatch = spriteBatch;
            this.audioController = audioController;
            this.game = game;

            

            gameMap = new GameMap(mapWidth, mapHeight);
            gameMap.MakeMap(maxRooms, minRoomSize, maxRoomSize, mapWidth, mapHeight);
            wallsFloor = game.Content.Load<Texture2D>("Images/walls_floor");

            player = new Player();    // TODO: Pass attributes through this rather than being defined in constructor.
            player.Initalise(content, graphicsDevice);

            gameScene = new(gameMap, player);
        }

        public void Draw()
        {
            //DrawDungeon();
            player.Draw();
        }

        public void Hide()
        {
        }

        public void Show()
        {
        }

        public void Update(GameTime gameTime)
        {
            KeyboardInput.GetInput();
            player.Update(Keyboard.GetState());
        }

        private void DrawDungeon()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Texture2D texture = gameMap.tiles[x, y] switch
                    {
                        TileType.Floor => wallsFloor,
                        TileType.Wall => wallsFloor,
                        _ => null,
                    };

                    if (texture != null)
                    {
                        Rectangle sourceRect = gameMap.tiles[x, y] switch
                        {
                            TileType.Floor => new Rectangle(8, 86, 32, 32),
                            TileType.Wall => new Rectangle(8, 16, 32, 12),
                            _ => Rectangle.Empty,
                        };

                        spriteBatch.Draw(
                            texture,
                            new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize),
                            sourceRect,
                            Color.White);
                    }
                }
            }
        }

    }

    #region private methods


    #endregion

}
