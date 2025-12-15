using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jam25.Models
{
    /// <summary>
    /// Class representing health attributes.
    /// </summary>
    public class Health(int max, int? current = null)
    {
        /// <summary>
        /// Gets or sets the current health value.
        /// </summary>
        public int Current { get; set; } = current ?? max;

        /// <summary>
        /// Gets or sets the maximum health value.
        /// </summary>
        public int Max { get; set; } = max;

        /// <summary>
        /// Reduces current health by the specified amount without going below zero.
        /// </summary>
        /// <param name="amount">Damage amount to apply.</param>
        public void TakeDamage(int amount)
        {
            Current -= amount;

            ClampHealth();
        }

        /// <summary>
        /// Restores current health by the specified amount up to the maximum value.
        /// </summary>
        /// <param name="amount">Healing amount to apply.</param>
        public void Heal(int amount)
        {
            Current += amount;

            ClampHealth();
        }

        #region Private Methods

        private void ClampHealth()
        {
            if (Current < 0) 
                Current = 0;

            if (Current > Max) 
                Current = Max;
        }

        #endregion Private Methods
    }
}
