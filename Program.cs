using System.CommandLine;
using TTS_STT_utility.Interfaces;
using TTS_STT_utility.Services;

namespace TTS_STT_utility;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Speech utility for Text-to-Speech and Speech-to-Text conversion");

        // TTS command
        var ttsCommand = new Command("tts", "Convert text to speech (WAV file)");
        var ttsTextOption = new Option<string>(
            "--text",
            description: "The text to convert to speech")
        {
            IsRequired = true
        };
        var ttsOutputOption = new Option<string>(
            "--output",
            description: "Output WAV file path")
        {
            IsRequired = true
        };
        var ttsProviderOption = new Option<string?>(
            "--provider",
            description: "TTS provider to use (system, stub, auto). Default: auto",
            getDefaultValue: () => "auto");

        ttsCommand.AddOption(ttsTextOption);
        ttsCommand.AddOption(ttsOutputOption);
        ttsCommand.AddOption(ttsProviderOption);

        ttsCommand.SetHandler(async (text, output, provider) =>
        {
            await HandleTtsCommand(text, output, provider);
        }, ttsTextOption, ttsOutputOption, ttsProviderOption);

        // ASR command
        var asrCommand = new Command("asr", "Convert speech to text from audio file");
        var asrInputOption = new Option<string>(
            "--input",
            description: "Input WAV audio file path")
        {
            IsRequired = true
        };
        var asrProviderOption = new Option<string?>(
            "--provider",
            description: "ASR provider to use (vosk, stub, auto). Default: auto",
            getDefaultValue: () => "auto");
        var asrModelPathOption = new Option<string?>(
            "--model-path",
            description: "Path to ASR model (for Vosk)");

        asrCommand.AddOption(asrInputOption);
        asrCommand.AddOption(asrProviderOption);
        asrCommand.AddOption(asrModelPathOption);

        asrCommand.SetHandler(async (input, provider, modelPath) =>
        {
            await HandleAsrCommand(input, provider, modelPath);
        }, asrInputOption, asrProviderOption, asrModelPathOption);

        rootCommand.AddCommand(ttsCommand);
        rootCommand.AddCommand(asrCommand);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task HandleTtsCommand(string text, string output, string? provider)
    {
        try
        {
            Console.WriteLine($"Using TTS provider: {provider ?? "auto"}");
            var ttsProvider = ProviderFactory.CreateTtsProvider(provider);
            Console.WriteLine($"Provider: {ttsProvider.ProviderName}");

            Console.WriteLine($"Synthesizing text: \"{text}\"");
            Console.WriteLine($"Output file: {output}");

            var success = await ttsProvider.SynthesizeToFileAsync(text, output);

            if (success)
            {
                Console.WriteLine($"✓ Successfully generated WAV file: {output}");
            }
            else
            {
                Console.Error.WriteLine("✗ Failed to generate WAV file");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static async Task HandleAsrCommand(string input, string? provider, string? modelPath)
    {
        try
        {
            Console.WriteLine($"Using ASR provider: {provider ?? "auto"}");
            var asrProvider = ProviderFactory.CreateAsrProvider(provider, modelPath);
            Console.WriteLine($"Provider: {asrProvider.ProviderName}");

            Console.WriteLine($"Processing audio file: {input}");

            var result = await asrProvider.RecognizeFromFileAsync(input);

            if (result != null)
            {
                Console.WriteLine($"Recognized text: {result}");
            }
            else
            {
                Console.Error.WriteLine("✗ Failed to recognize speech");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}

