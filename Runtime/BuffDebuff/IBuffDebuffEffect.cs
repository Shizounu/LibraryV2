using System;
using UnityEngine;

namespace Shizounu.Library.BuffDebuff
{
    /// <summary>
    /// Interface for all stat modifiers that can be applied by buffs/debuffs.
    /// Implement this for different types of modifications (damage, speed, armor, etc.).
    /// </summary>
    public interface IStatModifier
    {
        /// <summary>
        /// Unique identifier for this modifier type.
        /// </summary>
        string ModifierID { get; }

        /// <summary>
        /// The value of this modifier. Meaning depends on the modifier type.
        /// </summary>
        float Value { get; }
    }

    /// <summary>
    /// Base interface for buff/debuff effects.
    /// Access this through the BuffDebuffSystem to apply effects to game objects.
    /// </summary>
    public interface IBuffDebuffEffect
    {
        /// <summary>
        /// Unique identifier for this effect instance.
        /// </summary>
        string EffectID { get; }

        /// <summary>
        /// Time remaining for this effect in seconds. Negative means infinite duration.
        /// </summary>
        float TimeRemaining { get; }

        /// <summary>
        /// Whether this effect is still active.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Stack count if this effect can stack.
        /// </summary>
        int StackCount { get; }

        /// <summary>
        /// Called when the effect is applied.
        /// </summary>
        void OnApply();

        /// <summary>
        /// Called when the effect expires or is removed.
        /// </summary>
        void OnRemove();

        /// <summary>
        /// Called each frame to update the effect.
        /// </summary>
        /// <param name="deltaTime">Time since last update in seconds.</param>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// Get all stat modifiers this effect currently provides.
        /// </summary>
        IStatModifier[] GetModifiers();

        /// <summary>
        /// Get a specific stat modifier by ID.
        /// </summary>
        IStatModifier GetModifier(string modifierID);

        /// <summary>
        /// Attempt to add a stack to this effect.
        /// </summary>
        /// <returns>Whether the stack was added successfully.</returns>
        bool TryAddStack();

        /// <summary>
        /// Refresh the duration of this effect.
        /// </summary>
        void RefreshDuration();
    }
}
