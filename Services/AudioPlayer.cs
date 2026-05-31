using System;
using System.IO;
using System.Media;

namespace CybersecurityBotWPF.Services
{
    public class AudioPlayer
    {
        public static void PlayGreeting(string filePath)  // Made static to fix CA1822
        {
            try
            {
                // Try multiple paths
                string[] possiblePaths = {
                    filePath,
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Audio", "greeting.wav"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Debug", "net8.0-windows", filePath)
                };

                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        using (SoundPlayer player = new SoundPlayer(path))
                        {
                            player.PlaySync();
                        }
                        return;
                    }
                }

                System.Diagnostics.Debug.WriteLine("Audio file not found, continuing without audio");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {ex.Message}");
            }
        }
    }
}