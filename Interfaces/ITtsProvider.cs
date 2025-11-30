namespace TTS_STT_utility.Interfaces;

/// <summary>
/// Interface for Text-to-Speech providers.
/// </summary>
public interface ITtsProvider
{
    /// <summary>
    /// Converts text to speech and saves it as a WAV file.
    /// </summary>
    /// <param name="text">The text to convert to speech.</param>
    /// <param name="outputPath">The path where the WAV file should be saved.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> SynthesizeToFileAsync(string text, string outputPath);
    
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string ProviderName { get; }
}

