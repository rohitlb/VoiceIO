namespace TTS_STT_utility.Models;

/// <summary>
/// Represents an audio buffer with normalization capabilities.
/// This demonstrates the use of a data structure (buffer) and algorithmic processing (normalization).
/// </summary>
public class AudioBuffer
{
    private readonly Queue<float> _samples;
    private readonly int _maxSize;
    private float _maxAmplitude;

    public AudioBuffer(int maxSize = 10000)
    {
        _samples = new Queue<float>(maxSize);
        _maxSize = maxSize;
        _maxAmplitude = 0f;
    }

    /// <summary>
    /// Adds a sample to the buffer.
    /// </summary>
    public void AddSample(float sample)
    {
        if (_samples.Count >= _maxSize)
        {
            _samples.Dequeue();
        }
        
        _samples.Enqueue(sample);
        
        // Track maximum amplitude for normalization
        var absSample = Math.Abs(sample);
        if (absSample > _maxAmplitude)
        {
            _maxAmplitude = absSample;
        }
    }

    /// <summary>
    /// Adds multiple samples to the buffer.
    /// </summary>
    public void AddSamples(IEnumerable<float> samples)
    {
        foreach (var sample in samples)
        {
            AddSample(sample);
        }
    }

    /// <summary>
    /// Normalizes the audio samples to a target amplitude.
    /// This is an algorithmic step that adjusts audio levels.
    /// </summary>
    /// <param name="targetAmplitude">The target maximum amplitude (0.0 to 1.0).</param>
    /// <returns>Normalized samples as an array.</returns>
    public float[] Normalize(float targetAmplitude = 0.95f)
    {
        if (_maxAmplitude == 0f || _samples.Count == 0)
        {
            return Array.Empty<float>();
        }

        var samples = _samples.ToArray();
        var scaleFactor = targetAmplitude / _maxAmplitude;

        // Apply normalization algorithm
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = Math.Clamp(samples[i] * scaleFactor, -1.0f, 1.0f);
        }

        return samples;
    }

    /// <summary>
    /// Gets all samples without normalization.
    /// </summary>
    public float[] GetSamples()
    {
        return _samples.ToArray();
    }

    /// <summary>
    /// Clears the buffer.
    /// </summary>
    public void Clear()
    {
        _samples.Clear();
        _maxAmplitude = 0f;
    }

    /// <summary>
    /// Gets the current number of samples in the buffer.
    /// </summary>
    public int Count => _samples.Count;
}

