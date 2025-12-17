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

        public TransitionHandler(MerchantScreen merchant, RestAreaScreen rest, NextLevelScreen nextLvl, BossScreen boss) 
        {
            merchantScreen = merchant;
            restAreaScreen = rest;
            nextLevelScreen = nextLvl;
            bossScreen = boss;
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
