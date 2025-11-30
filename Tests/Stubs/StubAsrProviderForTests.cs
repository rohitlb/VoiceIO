using TTS_STT_utility.Interfaces;

namespace TTS_STT_utility.Tests.Stubs;

/// <summary>
/// Stub ASR provider for testing purposes.
/// </summary>
public class StubAsrProviderForTests : IAsrProvider
{
    public string ProviderName => "Test Stub ASR Provider";
    
    public string? MockResult { get; set; } = "Test recognition result";
    public string? LastAudioFilePath { get; private set; }
    public bool ShouldReturnNull { get; set; } = false;

    public Task<string?> RecognizeFromFileAsync(string audioFilePath)
    {
        LastAudioFilePath = audioFilePath;
        
        if (ShouldReturnNull || !File.Exists(audioFilePath))
        {
            return Task.FromResult<string?>(null);
        }
        
        return Task.FromResult<string?>(MockResult);
    }
}

