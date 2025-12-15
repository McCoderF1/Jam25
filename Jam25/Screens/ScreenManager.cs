using HDT.Gaming;

namespace Jam25.Screens
{
    public class ScreenManager : BasicScreenManager
    {
        public ScreenManager(StartScreen startScreen)
        {
            //TODO: Screen navigation
            //startScreen.Stats += (_, _) => ChangeScreen(statScreen);
            //startScreen.Settings += (_,_) => ChangeScreen(settingsScreen);

            ChangeScreen(startScreen);
        }

        private void StartScreen_Settings(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
