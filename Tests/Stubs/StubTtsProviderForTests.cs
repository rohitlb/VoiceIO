using TTS_STT_utility.Interfaces;

namespace TTS_STT_utility.Tests.Stubs;

/// <summary>
/// Stub TTS provider for testing purposes.
/// </summary>
public class StubTtsProviderForTests : ITtsProvider
{
    public string ProviderName => "Test Stub TTS Provider";
    
    public bool ShouldSucceed { get; set; } = true;
    public string? LastText { get; private set; }
    public string? LastOutputPath { get; private set; }

    public Task<bool> SynthesizeToFileAsync(string text, string outputPath)
    {
        LastText = text;
        LastOutputPath = outputPath;
        
        if (ShouldSucceed)
        {
            // Create a minimal valid WAV file for testing
            CreateMinimalWavFile(outputPath);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    private void CreateMinimalWavFile(string path)
    {
        // Create a minimal valid WAV file header
        using var file = File.Create(path);
        var header = new byte[44];
        
        // RIFF header
        System.Text.Encoding.ASCII.GetBytes("RIFF").CopyTo(header, 0);
        BitConverter.GetBytes(36).CopyTo(header, 4); // File size - 8
        System.Text.Encoding.ASCII.GetBytes("WAVE").CopyTo(header, 8);
        
        // fmt chunk
        System.Text.Encoding.ASCII.GetBytes("fmt ").CopyTo(header, 12);
        BitConverter.GetBytes(16).CopyTo(header, 16); // fmt chunk size
        BitConverter.GetBytes((short)1).CopyTo(header, 20); // Audio format (PCM)
        BitConverter.GetBytes((short)1).CopyTo(header, 22); // Channels
        BitConverter.GetBytes(16000).CopyTo(header, 24); // Sample rate
        BitConverter.GetBytes(32000).CopyTo(header, 28); // Byte rate
        BitConverter.GetBytes((short)2).CopyTo(header, 32); // Block align
        BitConverter.GetBytes((short)16).CopyTo(header, 34); // Bits per sample
        
        // data chunk
        System.Text.Encoding.ASCII.GetBytes("data").CopyTo(header, 36);
        BitConverter.GetBytes(0).CopyTo(header, 40); // Data size
        
        file.Write(header, 0, header.Length);
    }
}

