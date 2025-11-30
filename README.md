# TTS/STT Utility

A C# command-line utility for Text-to-Speech (TTS) and Speech-to-Text (STT/ASR) conversion, built with .NET 8.

## Features

- **Text-to-Speech (TTS)**: Convert text strings to WAV audio files
- **Speech-to-Text (ASR)**: Convert WAV audio files to recognized text
- **Extensible Architecture**: Clean interfaces allow easy swapping of providers
- **Multiple Provider Support**: System.Speech (Windows), Vosk (ASR), and stub implementations
- **Cross-Platform**: Works on Windows, macOS, and Linux (with appropriate providers)

## Project Structure

```
TTS_STT_utility/
├── Interfaces/          # Provider interfaces
│   ├── ITtsProvider.cs
│   └── IAsrProvider.cs
├── Providers/           # Provider implementations
│   ├── SystemSpeechTtsProvider.cs
│   ├── StubTtsProvider.cs
│   ├── StubAsrProvider.cs
│   └── VoskAsrProvider.cs
├── Models/              # Data models
│   └── AudioBuffer.cs
├── Services/            # Service classes
│   └── ProviderFactory.cs
├── Tests/               # Unit tests
│   ├── Stubs/
│   ├── TtsProviderTests.cs
│   ├── AsrProviderTests.cs
│   └── AudioBufferTests.cs
├── Program.cs           # CLI entry point
└── TTS_STT_utility.csproj
```

## Prerequisites

- .NET 8 SDK or later
- (Optional) Vosk models for ASR (if using Vosk provider)

## Building the Project

```bash
# Build the main project
dotnet build

# Build and run tests
dotnet test
```

## Usage

### Text-to-Speech (TTS)

Convert text to a WAV audio file:

```bash
# Using auto-detected provider (System.Speech on Windows, Stub on others)
dotnet run -- tts --text "Hello, world!" --output output.wav

# Using specific provider
dotnet run -- tts --text "Hello, world!" --output output.wav --provider stub
dotnet run -- tts --text "Hello, world!" --output output.wav --provider system
```

**Options:**
- `--text`: The text to convert to speech (required)
- `--output`: Output WAV file path (required)
- `--provider`: TTS provider to use (`system`, `stub`, `auto`) - default: `auto`

### Speech-to-Text (ASR)

Convert a WAV audio file to text:

```bash
# Using auto-detected provider
dotnet run -- asr --input audio.wav

# Using specific provider
dotnet run -- asr --input audio.wav --provider stub
dotnet run -- asr --input audio.wav --provider vosk --model-path /path/to/vosk/model
```

**Options:**
- `--input`: Input WAV audio file path (required)
- `--provider`: ASR provider to use (`vosk`, `stub`, `auto`) - default: `auto`
- `--model-path`: Path to ASR model (for Vosk provider)

### Running as a Standalone Executable

After building, you can run the executable directly:

```bash
# Build release version
dotnet publish -c Release

# Run the executable
./bin/Release/net8.0/publish/TTS_STT_utility tts --text "Hello" --output output.wav
./bin/Release/net8.0/publish/TTS_STT_utility asr --input audio.wav
```

## Providers

### TTS Providers

1. **System.Speech.Synthesis** (`system`)
   - Windows-only
   - Uses built-in Windows TTS engine
   - High quality, native integration

2. **Stub TTS Provider** (`stub`)
   - Cross-platform
   - Generates simple tone-based audio
   - Demonstrates architecture and can be replaced with real TTS engines

### ASR Providers

1. **Vosk ASR Provider** (`vosk`)
   - Cross-platform
   - Requires Vosk model files
   - Download models from: https://alphacephei.com/vosk/models
   - Set `VOSK_MODEL_PATH` environment variable or use `--model-path`

2. **Stub ASR Provider** (`stub`)
   - Cross-platform
   - Returns mock transcriptions
   - Useful for testing and demonstration

## Architecture

The project follows clean architecture principles:

- **Interfaces**: `ITtsProvider` and `IAsrProvider` define contracts
- **Providers**: Concrete implementations of the interfaces
- **Factory Pattern**: `ProviderFactory` creates appropriate providers
- **Extensibility**: Easy to add new providers by implementing interfaces

### Adding a New Provider

1. Implement `ITtsProvider` or `IAsrProvider`
2. Add the provider to `ProviderFactory`
3. Update CLI options if needed

Example:

```csharp
public class MyCustomTtsProvider : ITtsProvider
{
    public string ProviderName => "My Custom TTS";
    
    public async Task<bool> SynthesizeToFileAsync(string text, string outputPath)
    {
        // Your implementation
    }
}
```

## Data Structures and Algorithms

The project includes:

- **AudioBuffer**: A queue-based buffer with normalization algorithm
  - Uses `Queue<float>` for efficient sample management
  - Implements audio normalization algorithm to adjust amplitude levels
  - Supports configurable buffer size with automatic overflow handling

## Testing

Run all tests:

```bash
dotnet test
```

The test suite includes:
- Provider tests using stub implementations
- Audio buffer tests
- Integration tests

Tests use NUnit and demonstrate how to test providers using stub classes.

## Dependencies

- **System.CommandLine**: CLI parsing
- **NAudio**: Audio file I/O
- **NUnit**: Unit testing framework

## Limitations

- System.Speech.Synthesis is Windows-only
- Vosk integration requires manual setup (see `VoskAsrProvider.cs` for details)
- Stub providers are for demonstration/testing purposes

## Future Enhancements

- Add more TTS providers (e.g., eSpeak, Festival)
- Complete Vosk integration
- Add audio format conversion
- Add batch processing capabilities
- Add configuration file support

## License

This project is provided as-is for educational and demonstration purposes.

## Contributing

Feel free to extend this project with additional providers or features!

