using HDT.Gaming;
using Jam25.Screens.Transitions;
using Jam25.Stores;
using System.Threading.Tasks;

namespace Jam25.Screens
{
    public class ScreenManager : BasicScreenManager
    {
        public ScreenManager(StartScreen startScreen, SettingsScreen settingsScreen, GameScreen gameScreen, PlayerScreen playerScreen, TransitionHandler transitions, DeathScreen deathScreen, CasinoScreen casinoScreen)
        {
            //TODO: Screen navigation
            startScreen.Settings += (_, _) => ChangeScreen(settingsScreen);
            startScreen.Start += (_, _) => { PlayerTracker.IncrementRoundsPlayed(); ChangeScreen(gameScreen); };
            startScreen.Player += (_, _) => ChangeScreen(playerScreen);
            startScreen.Casino += (_, _) => ChangeScreen(casinoScreen);

            settingsScreen.BackToMainMenu += (_, _) => ChangeScreen(startScreen);
            casinoScreen.BackToMainMenu += (_, _) => ChangeScreen(startScreen);
            playerScreen.BackToMainMenu += (_, _) => ChangeScreen(startScreen);

            gameScreen.PlayerDied += (_, _) => ChangeScreen(deathScreen);
            gameScreen.TransitionScreen += (toWhere, _) => ChangeScreen(transitions.TransitionTo(toWhere as string));

            deathScreen.Retry += (_, _) => { PlayerTracker.IncrementRoundsPlayed(); ChangeScreen(gameScreen); };
            deathScreen.BackToMenu += (_, _) => ChangeScreen(startScreen);

            transitions.MovePassTransition += (_, _) => ChangeScreen(gameScreen);

            ChangeScreen(startScreen);
        }

        private void StartScreen_Settings(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

    }
}
