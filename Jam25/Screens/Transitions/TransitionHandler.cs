using HDT.Gaming;
using HDT.Gaming.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Screens.Transitions
{
    public class TransitionHandler
    {
        #region private members

        private readonly MerchantScreen merchantScreen;
        private readonly RestAreaScreen restAreaScreen;
        private readonly NextLevelScreen nextLevelScreen;
        private readonly BossScreen bossScreen;

        #endregion

        public event EventHandler MovePassTransition;

        public TransitionHandler(MerchantScreen merchant, RestAreaScreen rest, NextLevelScreen nextLvl, BossScreen boss) 
        {
            merchantScreen = merchant;
            restAreaScreen = rest;
            nextLevelScreen = nextLvl;
            bossScreen = boss;

            merchantScreen.MovePassTransition += (_, _) => MovePassTransition.Invoke(this, EventArgs.Empty);
            restAreaScreen.MovePassTransition += (_, _) => MovePassTransition.Invoke(this, EventArgs.Empty);
            nextLevelScreen.MovePassTransition += (_, _) => MovePassTransition.Invoke(this, EventArgs.Empty);
            bossScreen.MovePassTransition += (_, _) => MovePassTransition.Invoke(this, EventArgs.Empty);
        }

        public IScreen TransitionTo(string screenType)
        {
            return screenType.ToLower() switch
            {
                "merchant" => merchantScreen,
                "restarea" => restAreaScreen,
                "nextlevel" => nextLevelScreen,
                "boss" => bossScreen,
                _ => throw new ArgumentException($"Invalid screen type: {screenType}"),
            };
        }

        public IScreen TransitionRandom()
        {
            var random = new Random();
            int choice = random.Next(0, 3);

            return choice switch
            {
                0 => merchantScreen,
                1 => restAreaScreen,
                2 => nextLevelScreen,
                3 => bossScreen,
            };
        }
    }
}
