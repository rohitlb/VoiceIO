using TTS_STT_utility.Interfaces;
using TTS_STT_utility.Providers;

namespace TTS_STT_utility.Services;

/// <summary>
/// Factory for creating TTS and ASR providers.
/// Demonstrates extensibility - easy to add new providers.
/// </summary>
public static class ProviderFactory
{
    /// <summary>
    /// Creates a TTS provider based on the specified type.
    /// </summary>
    public static ITtsProvider CreateTtsProvider(string? providerType = null)
    {
        providerType = providerType?.ToLowerInvariant() ?? "auto";

        return providerType switch
        {
#if WINDOWS
            "system" => new SystemSpeechTtsProvider(),
#endif
            "stub" => new StubTtsProvider(),
            "auto" => CreateAutoTtsProvider(),
            _ => throw new ArgumentException($"Unknown TTS provider type: {providerType}. " +
#if WINDOWS
                "Available: system, stub, auto")
#else
                "Available: stub, auto (system is Windows-only)")
#endif
        };
    }

    /// <summary>
    /// Creates an ASR provider based on the specified type.
    /// </summary>
    public static IAsrProvider CreateAsrProvider(string? providerType = null, string? modelPath = null)
    {
        providerType = providerType?.ToLowerInvariant() ?? "auto";

        return providerType switch
        {
            "vosk" => new VoskAsrProvider(modelPath),
            "stub" => new StubAsrProvider(),
            "auto" => CreateAutoAsrProvider(modelPath),
            _ => throw new ArgumentException($"Unknown ASR provider type: {providerType}")
        };
    }

    private static ITtsProvider CreateAutoTtsProvider()
    {
        // Try System.Speech first (Windows), fall back to stub
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            try
            {
                return new SystemSpeechTtsProvider();
            }
            catch
            {
                // Fall through to stub
            }
        }
#endif

        return new StubTtsProvider();
    }

    private static IAsrProvider CreateAutoAsrProvider(string? modelPath = null)
    {
        // Try Vosk if model path is available, otherwise use stub
        var voskPath = modelPath ?? Environment.GetEnvironmentVariable("VOSK_MODEL_PATH");
        if (!string.IsNullOrEmpty(voskPath) && Directory.Exists(voskPath))
        {
            try
            {
                return new VoskAsrProvider(voskPath);
            }
            catch
            {
                // Fall through to stub
            }
        }

        return new StubAsrProvider();
    }
}

