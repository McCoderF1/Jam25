using HDT.Gaming.Audio;
using Jam25.Graphics;
using Jam25.Screens;
using Jam25.Screens.Transitions;
using Jam25.Stores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

namespace Jam25
{
    public class Game1 : Game
    {
        public const string TITLE = "Last Ember";

        public const int STANDARD_WINDOW_WIDTH = 1280;
        public const int STANDARD_WINDOW_HEIGHT = 720;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;

        private AudioController audioController;
        private ScreenManager screenManager;
        private GameContent content;

        internal Torch Torch { get; set; }
        internal Texture2D UiWhitePixel { get; private set; }

        public Game1()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler((_, _) => Exit());
            graphics = new GraphicsDeviceManager(this);

            //TODO: use saved settings <see cref="SettingsScreen"/>
            graphics.PreferredBackBufferWidth = STANDARD_WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = STANDARD_WINDOW_HEIGHT;

            content = new GameContent(Content);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Window.Title = TITLE;
            Window.AllowUserResizing = true;

            Torch = new Torch(
                maxEnergy: 100f,
                drainPerSecond: 1f,
                maxRadius: 250f,
                minRadius: 60f);
        }

        protected override void LoadContent()
        {
            LoadStores();
            spriteBatch = new SpriteBatch(GraphicsDevice);

            content.LoadSprite(SpriteID.PlayerLvl1, "Images/StatsPage/PlayerLvl1Idle", 12, 5, new Vector2(64f, 64f));

            content.LoadFont(FontID.Title, "Fonts/Title");
            content.LoadFont(FontID.Heading, "Fonts/GameState");
            content.LoadFont(FontID.Body, "Fonts/Score");
            content.LoadObjectSpritesheet("Images/supplies_objects");


            UiWhitePixel = new Texture2D(GraphicsDevice, 1, 1);
            UiWhitePixel.SetData(new[] { Color.White });

            audioController = new AudioController();
            audioController.InstallMusic("The Flickering Flame", Content.Load<Song>("Sound/Music/The Flickering Flame"));
            audioController.InstallMusic("Game1", Content.Load<Song>("Sound/Music/Shadows Rise1"));
            audioController.InstallMusic("Game2", Content.Load<Song>("Sound/Music/Shadows Rise2"));
            audioController.InstallMusic("Game3", Content.Load<Song>("Sound/Music/Shadows and Smoke1"));
            audioController.InstallMusic("Game4", Content.Load<Song>("Sound/Music/Shadows and Smoke2"));
            audioController.InstallMusic("Death", Content.Load<Song>("Sound/Music/Shadows of the Heart"));
            audioController.InstallEffect("MetalHit", Content.Load<SoundEffect>("Sound/Effects/MetalHit"));
            audioController.InstallEffect("RetroClick", Content.Load<SoundEffect>("Sound/Effects/RetroClick"));
            audioController.InstallEffect("AppClick", Content.Load<SoundEffect>("Sound/Effects/AppClick"));
            audioController.InstallEffect("Miss", Content.Load<SoundEffect>("Sound/Effects/WhooshMiss"));
            audioController.InstallEffect("HitFlesh", Content.Load<SoundEffect>("Sound/Effects/HitFlesh"));
            audioController.InstallEffect("LevelUpSound", Content.Load<SoundEffect>("Sound/Effect/LevelUpSound"));
            audioController.InstallEffect("LevelUp", Content.Load<SoundEffect>("Sound/Effects/LevelUp"));
            audioController.InstallEffect("GetKey", Content.Load<SoundEffect>("Sound/Effects/GetKey"));
            audioController.InstallEffect("TakeItem", Content.Load<SoundEffect>("Sound/Effects/TakeItem"));
            audioController.InstallEffect("CasinoPay", Content.Load<SoundEffect>("Sound/Effect/casinoPay"));
            audioController.InstallEffect("CasinoSpin", Content.Load<SoundEffect>("Sound/Effect/casinoSpin"));
            //add audio here
            // - ok

            AudioManager.InstallController(audioController);

            var bossTransition = new BossScreen(spriteBatch, graphics, Content, audioController);
            var restTransition = new RestAreaScreen(spriteBatch, graphics, Content, audioController);
            var nextLevelTransition = new NextLevelScreen(spriteBatch, graphics, Content, audioController);
            var merchantTransition = new MerchantScreen(spriteBatch, graphics, Content, audioController);

            var transitionHandler = new TransitionHandler(merchantTransition, restTransition, nextLevelTransition, bossTransition);

            var startScreen = new StartScreen(graphics.GraphicsDevice, spriteBatch, Content, audioController, this);
            var settingScreen = new SettingsScreen(spriteBatch, graphics, content, Content, audioController);
            var casinoScreen = new CasinoScreen(spriteBatch, graphics, content, Content, audioController);

            var gameScreen = new GameScreen(graphics.GraphicsDevice, spriteBatch, content, Content, audioController, this);
            var playerScreen = new PlayerScreen(spriteBatch, graphics, content, Content, audioController);
            var deathScreen = new DeathScreen(graphics.GraphicsDevice, spriteBatch, Content);
            var winScreen = new WinScreen(graphics.GraphicsDevice, spriteBatch, Content);

            screenManager = new ScreenManager(startScreen, settingScreen, gameScreen, playerScreen, transitionHandler, deathScreen, casinoScreen, winScreen);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Torch.Update(gameTime);

            screenManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Update render target based on window size
            float scale = Math.Min(
                (float)Window.ClientBounds.Width / STANDARD_WINDOW_WIDTH,
                (float)Window.ClientBounds.Height / STANDARD_WINDOW_HEIGHT);
            int renderWidth = (int)Math.Ceiling((float)Window.ClientBounds.Width / scale);
            int renderHeight = (int)Math.Ceiling((float)Window.ClientBounds.Height / scale);
            if (renderTarget == null || renderTarget.Width != renderWidth || renderTarget.Height != renderHeight) {
                renderTarget?.Dispose();
                renderTarget = new(GraphicsDevice, renderWidth, renderHeight);
            }

            // Draw to render target
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(GameSettings.BackgroundColor);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            screenManager.Draw();

            spriteBatch.End();

            // Draw upscaled render target to window
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height), Color.White);
            spriteBatch.End();

            // Allow correct viewport to be used in update loop
            GraphicsDevice.Viewport = new(0, 0, renderWidth, renderHeight);

            base.Draw(gameTime);
        }

        private void LoadStores()
        {
            PlayerTracker.RestorePlayerProgress();
        }

        internal void DrawTorchBar()
        {
            if (UiWhitePixel == null || Torch == null)
            {
                return;
            }

            const int barWidth = 300;
            const int barHeight = 20;
            const int margin = 20;

            var x = margin;
            var y = margin;

            var backgroundRect = new Rectangle(x, y, barWidth, barHeight);
            spriteBatch.Draw(UiWhitePixel, backgroundRect, Color.Black * 0.7f);

            var normalized = Torch.NormalizedEnergy;
            var fillWidth = (int)(barWidth * normalized);
            if (fillWidth <= 0)
            {
                return;
            }

            var fillRect = new Rectangle(x + 2, y + 2, fillWidth - 4, barHeight - 4);
            var fillColor = Color.Lerp(Color.Red, Color.Orange, normalized);
            spriteBatch.Draw(UiWhitePixel, fillRect, fillColor);
        }

        public void Exit()
        {
            base.Exit();
        }
    }
}
