using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using NAudio.Wave;
using Fouad.ViewModels;

namespace Fouad.Services
{
    /// <summary>
    /// Service for playing audio sounds in the application.
    /// Supports various sound effects for user interactions and feedback.
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly string _soundsDirectory;
        private readonly Dictionary<string, string> _soundFiles;
        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioFileReader;
        private ConfigurationService? _configurationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioService"/> class.
        /// </summary>
        public AudioService()
        {
            // Try to get the configuration service
            try
            {
                _configurationService = App.ServiceContainer.Resolve<ConfigurationService>();
            }
            catch
            {
                // If we can't get the configuration service, we'll work with defaults
                _configurationService = null;
            }
            
            _soundsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");
            _soundFiles = new Dictionary<string, string>
            {
                { "Added", "Added.wav" },
                { "Added_Tone1", "Added_Tone1.wav" },
                { "AlreadyAdded", "AlreadyAdded.wav" },
                { "AlreadyAdded_Tone1", "AlreadyAdded_Tone1.wav" },
                { "Error", "Error.wav" },
                { "Error_Tone1", "Error_Tone1.wav" },
                { "NoResults", "NoResults.wav" },
                { "Success", "Success.wav" },
                { "Success_Tone1", "Success_Tone1.wav" }
            };
            
            // Only initialize WaveOutEvent on Windows platforms
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    _waveOut = new WaveOutEvent();
                    System.Diagnostics.Debug.WriteLine("AudioService initialized on Windows platform");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing WaveOutEvent: {ex.Message}");
                    MessageBox.Show($"Error initializing WaveOutEvent: {ex.Message}", "Audio Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AudioService not initialized - not on Windows platform");
            }
            
            // Log the sounds directory
            System.Diagnostics.Debug.WriteLine($"Sounds directory: {_soundsDirectory}");
            System.Diagnostics.Debug.WriteLine($"Sounds directory exists: {Directory.Exists(_soundsDirectory)}");
            
            // List files in sounds directory
            if (Directory.Exists(_soundsDirectory))
            {
                var files = Directory.GetFiles(_soundsDirectory);
                System.Diagnostics.Debug.WriteLine($"Found {files.Length} files in sounds directory:");
                foreach (var file in files)
                {
                    System.Diagnostics.Debug.WriteLine($"  {Path.GetFileName(file)}");
                }
            }
        }

        /// <summary>
        /// Plays a sound by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        public void PlaySound(string soundName)
        {
            System.Diagnostics.Debug.WriteLine($"PlaySound called with: {soundName}");
            
            // Check if audio feedback is enabled
            if (_configurationService != null && !_configurationService.EnableAudioFeedback)
            {
                System.Diagnostics.Debug.WriteLine("Audio feedback is disabled in settings");
                return;
            }
            
            // Check if this specific sound is enabled
            if (_configurationService != null && !_configurationService.GetSoundSetting(soundName))
            {
                System.Diagnostics.Debug.WriteLine($"Sound '{soundName}' is disabled in settings");
                return;
            }
            
            // Only play sound on Windows platforms
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Debug.WriteLine("Not playing sound - not on Windows platform");
                return;
            }
            
            // Check if waveOut is initialized
            if (_waveOut == null)
            {
                System.Diagnostics.Debug.WriteLine("WaveOut not initialized");
                return;
            }
            
            try
            {
                if (_soundFiles.TryGetValue(soundName, out string? fileName))
                {
                    if (fileName != null)
                    {
                        string fullPath = Path.Combine(_soundsDirectory, fileName);
                        System.Diagnostics.Debug.WriteLine($"Full path to sound file: {fullPath}");
                        
                        if (File.Exists(fullPath))
                        {
                            System.Diagnostics.Debug.WriteLine($"Sound file exists: {fullPath}");
                            
                            // Stop any currently playing sound
                            if (_waveOut.PlaybackState == PlaybackState.Playing)
                            {
                                System.Diagnostics.Debug.WriteLine("Stopping currently playing sound");
                                _waveOut.Stop();
                            }
                            
                            // Dispose of previous audio file reader
                            _audioFileReader?.Dispose();
                            
                            // Load and play the new sound
                            System.Diagnostics.Debug.WriteLine("Creating AudioFileReader");
                            _audioFileReader = new AudioFileReader(fullPath);
                            
                            System.Diagnostics.Debug.WriteLine("Initializing WaveOut with audio file");
                            _waveOut.Init(_audioFileReader);
                            
                            System.Diagnostics.Debug.WriteLine("Playing sound");
                            _waveOut.Play();
                            
                            System.Diagnostics.Debug.WriteLine("Sound playback started");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Audio file not found: {fullPath}");
                            
                            // List all files in directory for debugging
                            if (Directory.Exists(_soundsDirectory))
                            {
                                var files = Directory.GetFiles(_soundsDirectory);
                                System.Diagnostics.Debug.WriteLine("Files in sounds directory:");
                                foreach (var file in files)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  {Path.GetFileName(file)}");
                                }
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("File name is null");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Sound not found: {soundName}");
                    System.Diagnostics.Debug.WriteLine("Available sounds:");
                    foreach (var sound in _soundFiles.Keys)
                    {
                        System.Diagnostics.Debug.WriteLine($"  {sound}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                System.Diagnostics.Debug.WriteLine($"Error playing sound {soundName}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Exception stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error playing sound {soundName}: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Audio Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Stops the currently playing sound.
        /// </summary>
        public void StopSound()
        {
            System.Diagnostics.Debug.WriteLine("StopSound called");
            
            // Only stop sound on Windows platforms
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Debug.WriteLine("Not stopping sound - not on Windows platform");
                return;
            }
            
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                System.Diagnostics.Debug.WriteLine("Stopping sound");
                _waveOut.Stop();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No sound playing or WaveOut is null");
            }
        }

        /// <summary>
        /// Disposes of the audio service resources.
        /// </summary>
        public void Dispose()
        {
            System.Diagnostics.Debug.WriteLine("AudioService Dispose called");
            _waveOut?.Dispose();
            _audioFileReader?.Dispose();
        }
    }
}