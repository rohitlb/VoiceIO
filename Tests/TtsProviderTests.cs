using NUnit.Framework;
using TTS_STT_utility.Interfaces;
using TTS_STT_utility.Tests.Stubs;
using TTS_STT_utility.Providers;

namespace TTS_STT_utility.Tests;

[TestFixture]
public class TtsProviderTests
{
    private string _testOutputDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testOutputDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, true);
        }
    }

    [Test]
    public async Task StubTtsProvider_SynthesizeToFile_ShouldCreateWavFile()
    {
        // Arrange
        var provider = new StubTtsProviderForTests();
        var text = "Hello, world!";
        var outputPath = Path.Combine(_testOutputDir, "test_output.wav");

        // Act
        var result = await provider.SynthesizeToFileAsync(text, outputPath);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(File.Exists(outputPath), Is.True);
        Assert.That(provider.LastText, Is.EqualTo(text));
        Assert.That(provider.LastOutputPath, Is.EqualTo(outputPath));
    }

    [Test]
    public async Task StubTtsProvider_SynthesizeToFile_ShouldHandleFailure()
    {
        // Arrange
        var provider = new StubTtsProviderForTests { ShouldSucceed = false };
        var text = "Test";
        var outputPath = Path.Combine(_testOutputDir, "test_output.wav");

        // Act
        var result = await provider.SynthesizeToFileAsync(text, outputPath);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task StubTtsProvider_ProviderName_ShouldReturnCorrectName()
    {
        // Arrange
        var provider = new StubTtsProviderForTests();

        // Act & Assert
        Assert.That(provider.ProviderName, Is.EqualTo("Test Stub TTS Provider"));
    }
}

