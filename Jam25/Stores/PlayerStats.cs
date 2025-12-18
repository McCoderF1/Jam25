using HDT.Mercury.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;

namespace Jam25.Stores
{
    public partial class PlayerStats { }

    /// <summary>
    /// Static class to handle player progress tracking
    /// </summary>
    public static class PlayerTracker
    {
        /// <summary>
        /// Embers needed to level up, ramped up per level (15 levels total)
        /// </summary>
        public static readonly List<int> EmbersPerLevel = new() { 10, 50, 100, 250, 400, 600, 850, 1100, 1450, 2000, 2500, 3000, 3500, 4000, 5000 };

        /// <summary>
        /// Player's stats
        /// </summary>
        public static PlayerStats PlayerStats { get; private set; }

        /// <summary>
        /// Record the player has killed an enemy
        /// </summary>
        public static void RecordKill()
        {
            PlayerStats.Kills++;
        }

        /// <summary>
        /// Record the player has died
        /// </summary>
        public static void RecordDeath()
        {
            PlayerStats.Deaths++;
        }

        /// <summary>
        /// Count the amount of rounds the player has entered
        /// </summary>
        public static void IncrementRoundsPlayed()
        {
            PlayerStats.RoundsPlayed++;
        }

        /// <summary>
        /// Collect ember currency for the player
        /// </summary>
        public static void CollectEmber()
        {
            PlayerStats.EmbersCollected++;
            //Check level up
            CheckLevelUp();
        }

        /// <summary>
        /// Save player progress to save file
        /// </summary>
        public static void SavePlayerProgress()
        {
            try
            {
               File.WriteAllText("Player.LESF", MercurySerializer.SerializeToString(PlayerStats));
            }
            catch(Exception e) { /*Log Error*/ }
        }

        /// <summary>
        /// Restores the players progress from the last save point
        /// </summary>
        public static void RestorePlayerProgress()
        {
            PlayerStats localStats = new();

            try
            {
                MercurySerializer.Deserialize<PlayerStats>(File.ReadAllText("Player.LESF"));
            }
            catch (Exception e) { /*Log Error*/  }

            PlayerStats = localStats;
        }

        #region private methods

        private static void CheckLevelUp()
        {
            if (PlayerStats.TotalLevel == EmbersPerLevel.Count)
                return;

            if(PlayerStats.EmbersCollected >= EmbersPerLevel[PlayerStats.TotalLevel])
            {
                PlayerStats.TotalLevel++;
            }
        }

        #endregion
    }
}
