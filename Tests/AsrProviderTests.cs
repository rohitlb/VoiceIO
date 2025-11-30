using NUnit.Framework;
using TTS_STT_utility.Interfaces;
using TTS_STT_utility.Tests.Stubs;

namespace TTS_STT_utility.Tests;

[TestFixture]
public class AsrProviderTests
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
    public async Task StubAsrProvider_RecognizeFromFile_ShouldReturnMockResult()
    {
        // Arrange
        var provider = new StubAsrProviderForTests { MockResult = "Hello, this is a test" };
        var audioFilePath = Path.Combine(_testOutputDir, "test_audio.wav");
        File.WriteAllText(audioFilePath, "dummy audio data");

        // Act
        var result = await provider.RecognizeFromFileAsync(audioFilePath);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("Hello, this is a test"));
        Assert.That(provider.LastAudioFilePath, Is.EqualTo(audioFilePath));
    }

    [Test]
    public async Task StubAsrProvider_RecognizeFromFile_ShouldReturnNullWhenFileNotFound()
    {
        // Arrange
        var provider = new StubAsrProviderForTests();
        var nonExistentPath = Path.Combine(_testOutputDir, "nonexistent.wav");

        // Act
        var result = await provider.RecognizeFromFileAsync(nonExistentPath);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task StubAsrProvider_RecognizeFromFile_ShouldReturnNullWhenConfigured()
    {
        // Arrange
        var provider = new StubAsrProviderForTests { ShouldReturnNull = true };
        var audioFilePath = Path.Combine(_testOutputDir, "test_audio.wav");
        File.WriteAllText(audioFilePath, "dummy audio data");

        // Act
        var result = await provider.RecognizeFromFileAsync(audioFilePath);

        // Assert
        Assert.That(result, Is.Null);
    }
}

