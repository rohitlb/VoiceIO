using NUnit.Framework;
using TTS_STT_utility.Models;

namespace TTS_STT_utility.Tests;

[TestFixture]
public class AudioBufferTests
{
    [Test]
    public void AudioBuffer_AddSample_ShouldStoreSample()
    {
        // Arrange
        var buffer = new AudioBuffer(10);

        // Act
        buffer.AddSample(0.5f);

        // Assert
        Assert.That(buffer.Count, Is.EqualTo(1));
        var samples = buffer.GetSamples();
        Assert.That(samples[0], Is.EqualTo(0.5f));
    }

    [Test]
    public void AudioBuffer_AddSamples_ShouldStoreMultipleSamples()
    {
        // Arrange
        var buffer = new AudioBuffer(10);
        var samplesToAdd = new[] { 0.1f, 0.2f, 0.3f };

        // Act
        buffer.AddSamples(samplesToAdd);

        // Assert
        Assert.That(buffer.Count, Is.EqualTo(3));
        var samples = buffer.GetSamples();
        Assert.That(samples, Is.EqualTo(samplesToAdd));
    }

    [Test]
    public void AudioBuffer_Normalize_ShouldNormalizeSamples()
    {
        // Arrange
        var buffer = new AudioBuffer(10);
        buffer.AddSamples(new[] { 0.5f, 0.3f, 0.1f });

        // Act
        var normalized = buffer.Normalize(0.95f);

        // Assert
        Assert.That(normalized.Length, Is.EqualTo(3));
        // Maximum should be close to 0.95
        Assert.That(normalized.Max(Math.Abs), Is.GreaterThan(0.9));
        Assert.That(normalized.Max(Math.Abs), Is.LessThanOrEqualTo(1.0));
    }

    [Test]
    public void AudioBuffer_ExceedMaxSize_ShouldRemoveOldestSamples()
    {
        // Arrange
        var buffer = new AudioBuffer(3);

        // Act
        buffer.AddSample(1.0f);
        buffer.AddSample(2.0f);
        buffer.AddSample(3.0f);
        buffer.AddSample(4.0f); // Should remove 1.0f

        // Assert
        Assert.That(buffer.Count, Is.EqualTo(3));
        var samples = buffer.GetSamples();
        Assert.That(samples, Does.Not.Contain(1.0f));
        Assert.That(samples, Does.Contain(4.0f));
    }

    [Test]
    public void AudioBuffer_Clear_ShouldRemoveAllSamples()
    {
        // Arrange
        var buffer = new AudioBuffer(10);
        buffer.AddSamples(new[] { 0.1f, 0.2f, 0.3f });

        // Act
        buffer.Clear();

        // Assert
        Assert.That(buffer.Count, Is.EqualTo(0));
    }
}

