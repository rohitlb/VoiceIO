#if WINDOWS
using System.Speech.Synthesis;
#endif
using TTS_STT_utility.Interfaces;

namespace TTS_STT_utility.Providers;

/// <summary>
/// TTS provider using System.Speech.Synthesis (Windows only).
/// Falls back to StubTtsProvider on non-Windows platforms.
/// </summary>
public class SystemSpeechTtsProvider : ITtsProvider
{
    public string ProviderName => "System.Speech.Synthesis";

    public async Task<bool> SynthesizeToFileAsync(string text, string outputPath)
    {
#if WINDOWS
        return await Task.Run(() =>
        {
            try
            {
                using var synthesizer = new SpeechSynthesizer();
                
                // Configure output format: WAV, 16kHz, 16-bit, mono
                synthesizer.SetOutputToWaveFile(outputPath, 
                    new System.Speech.AudioFormat.SpeechAudioFormatInfo(16000, 
                        System.Speech.AudioFormat.AudioBitsPerSample.Sixteen, 
                        System.Speech.AudioFormat.AudioChannel.Mono));
                
                synthesizer.Speak(text);
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                // System.Speech is Windows-only
                throw new NotSupportedException(
                    "System.Speech.Synthesis is only available on Windows. " +
                    "Please use StubTtsProvider or another cross-platform TTS solution.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in TTS synthesis: {ex.Message}");
                return false;
            }
        });
#else
        throw new PlatformNotSupportedException(
            "System.Speech.Synthesis is only available on Windows. " +
            "Please use StubTtsProvider or another cross-platform TTS solution.");
#endif
    }
}

