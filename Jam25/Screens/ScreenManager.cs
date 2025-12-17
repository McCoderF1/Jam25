using HDT.Gaming;
using Jam25.Screens.Transitions;
using Jam25.Stores;
using System.Threading.Tasks;

namespace Jam25.Screens
{
    public class ScreenManager : BasicScreenManager
    {
        public ScreenManager(StartScreen startScreen, SettingsScreen settingsScreen, GameScreen gameScreen, PlayerScreen playerScreen, TransitionHandler transitions)
        {
            //TODO: Screen navigation
            startScreen.Settings += (_, _) => ChangeScreen(settingsScreen);
            startScreen.Start += (_, _) => { PlayerTracker.IncrementRoundsPlayed(); ChangeScreen(gameScreen); };
            startScreen.Player += (_, _) => ChangeScreen(playerScreen);

            settingsScreen.BackToMainMenu += (_, _) => ChangeScreen(startScreen);
            playerScreen.BackToMainMenu += (_, _) => ChangeScreen(startScreen);

            ChangeScreen(startScreen);
        }

        private void StartScreen_Settings(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

    }
}
