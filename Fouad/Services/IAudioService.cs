using System;

namespace Fouad.Services
{
    /// <summary>
    /// Interface for audio service operations.
    /// </summary>
    public interface IAudioService : IDisposable
    {
        /// <summary>
        /// Plays a sound by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        void PlaySound(string soundName);

        /// <summary>
        /// Stops the currently playing sound.
        /// </summary>
        void StopSound();
    }
}