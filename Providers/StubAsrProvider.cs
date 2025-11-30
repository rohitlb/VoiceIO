using TTS_STT_utility.Interfaces;
using NAudio.Wave;

namespace TTS_STT_utility.Providers;

/// <summary>
/// Stub ASR provider that simulates speech recognition.
/// In a real implementation, this would use Vosk or another ASR engine.
/// </summary>
public class StubAsrProvider : IAsrProvider
{
    private readonly Dictionary<string, string> _mockTranscriptions;

    public string ProviderName => "Stub ASR Provider";

    public StubAsrProvider()
    {
        // Mock transcription dictionary for demonstration
        _mockTranscriptions = new Dictionary<string, string>
        {
            { "hello", "Hello, how can I help you?" },
            { "test", "This is a test transcription." },
            { "default", "Recognized speech from audio file." }
        };
    }

    public async Task<string?> RecognizeFromFileAsync(string audioFilePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(audioFilePath))
                {
                    Console.Error.WriteLine($"Audio file not found: {audioFilePath}");
                    return null;
                }

                // Read audio file metadata
                using var reader = new WaveFileReader(audioFilePath);
                var duration = reader.TotalTime.TotalSeconds;
                var sampleRate = reader.WaveFormat.SampleRate;

                // Simulate recognition delay
                Thread.Sleep(100);

                // Generate mock transcription based on file characteristics
                // In a real implementation, this would use Vosk or another ASR engine
                var key = duration > 1.0 ? "hello" : "test";
                if (_mockTranscriptions.TryGetValue(key, out var transcription))
                {
                    return transcription;
                }

                return _mockTranscriptions["default"];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in ASR recognition: {ex.Message}");
                return null;
            }
        });
    }
}

