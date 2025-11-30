namespace TTS_STT_utility.Interfaces;

/// <summary>
/// Interface for Automatic Speech Recognition (ASR) / Speech-to-Text providers.
/// </summary>
public interface IAsrProvider
{
    /// <summary>
    /// Recognizes speech from an audio file and returns the transcribed text.
    /// </summary>
    /// <param name="audioFilePath">The path to the WAV audio file.</param>
    /// <returns>The recognized text, or null if recognition failed.</returns>
    Task<string?> RecognizeFromFileAsync(string audioFilePath);
    
    /// <summary>
    /// Gets the provider name.
    /// </summary>
    string ProviderName { get; }
}

