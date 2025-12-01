// Program.cs
// Single-file C# (.NET 8) TTS/STT utility (stub providers) with simple CLI and self-test.
// Build & run: (after dotnet SDK installed)
// 1) mkdir SpeechSingle && cd SpeechSingle
// 2) dotnet new console --output . --force
// 3) Replace generated Program.cs with this file
// 4) dotnet run -- tts --text "hello" --output hello.wav
// 5) dotnet run -- asr --input hello.wav
// 6) dotnet run -- --selftest

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

//////////// Interfaces ////////////

interface ITtsProvider
{
    /// Synthesize text -> WAV file at outputPath.
    Task SynthesizeToWavAsync(string text, string outputPath);
    string ProviderName { get; }
}

interface IAsrProvider
{
    /// Recognize speech in wav file and return recognized text.
    Task<string> RecognizeFromWavAsync(string wavPath);
    string ProviderName { get; }
}

//////////// AudioBuffer (queue-based) ////////////
// Demonstrates a complex data structure and a normalization algorithm.

class AudioBuffer
{
    private readonly Queue<float> _queue;
    private readonly int _maxSize;

    public AudioBuffer(int maxSize = 44100 * 5) // default 5 seconds at 44.1k
    {
        _queue = new Queue<float>();
        _maxSize = maxSize;
    }

    public int Count => _queue.Count;

    public void EnqueueSamples(IEnumerable<float> samples)
    {
        foreach (var s in samples)
        {
            if (_queue.Count >= _maxSize)
            {
                // overflow handling - drop oldest sample
                _queue.Dequeue();
            }
            _queue.Enqueue(s);
        }
    }

    public float[] DequeueAll()
    {
        var arr = _queue.ToArray();
        _queue.Clear();
        return arr;
    }

    // Simple normalization: scale so max amplitude hits 0.95
    public void NormalizeInPlace()
    {
        if (_queue.Count == 0) return;
        var arr = _queue.ToArray();
        float max = 0f;
        foreach (var v in arr) { var av = Math.Abs(v); if (av > max) max = av; }
        if (max < 1e-6f) return;
        float scale = 0.95f / max;
        _queue.Clear();
        foreach (var v in arr) _queue.Enqueue(v * scale);
    }
}

//////////// WAV helper ////////////

static class WavWriter
{
    public static void WriteWav16(string path, int sampleRate, short[] samples)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        int bytesPerSample = 2;
        int channels = 1;
        int byteRate = sampleRate * channels * bytesPerSample;
        int blockAlign = channels * bytesPerSample;
        int subchunk2Size = samples.Length * bytesPerSample;
        int chunkSize = 36 + subchunk2Size;

        // RIFF header
        bw.Write(Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(chunkSize);
        bw.Write(Encoding.ASCII.GetBytes("WAVE"));

        // fmt subchunk
        bw.Write(Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16); // subchunk1 size
        bw.Write((short)1); // PCM
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write((short)blockAlign);
        bw.Write((short)(8 * bytesPerSample));

        // data subchunk
        bw.Write(Encoding.ASCII.GetBytes("data"));
        bw.Write(subchunk2Size);

        foreach (var s in samples) bw.Write(s);
    }

    public static short[] FloatToPcm16(float[] samples)
    {
        short[] outSamples = new short[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            float v = samples[i];
            if (v > 1f) v = 1f;
            if (v < -1f) v = -1f;
            outSamples[i] = (short)(v * short.MaxValue);
        }
        return outSamples;
    }
}

//////////// Sine/Tone-based Stub TTS ////////////
// Map each character to a fixed frequency in a range.
// Synthesize a short sine burst per character with small gap to separate.

class StubTtsProvider : ITtsProvider
{
    public string ProviderName => "StubTTS";

    public Task SynthesizeToWavAsync(string text, string outputPath)
    {
        if (text == null) text = "";
        // parameters
        int sampleRate = 22050;
        double charDurationSec = 0.18; // per-character tone
        double gapSec = 0.04;
        var freqMap = BuildCharFreqMap();

        List<float> allSamples = new List<float>();

        foreach (char c in text)
        {
            double freq;
            if (!freqMap.TryGetValue(char.ToLower(c), out freq))
            {
                freq = 600.0; // fallback
            }
            int samplesCount = (int)(charDurationSec * sampleRate);
            var tone = GenerateSine(freq, sampleRate, samplesCount);
            // apply simple envelope to reduce clicks
            ApplyEnvelope(tone, sampleRate);
            allSamples.AddRange(tone);
            // add gap
            int gapSamples = (int)(gapSec * sampleRate);
            for (int i = 0; i < gapSamples; i++) allSamples.Add(0f);
        }

        // Use AudioBuffer to demonstrate normalization
        var buffer = new AudioBuffer(maxSize: sampleRate * 60);
        buffer.EnqueueSamples(allSamples);
        buffer.NormalizeInPlace();
        var pcmFloat = buffer.DequeueAll();

        var pcm16 = WavWriter.FloatToPcm16(pcmFloat);
        WavWriter.WriteWav16(outputPath, sampleRate, pcm16);

        Console.WriteLine($"[StubTTS] Wrote WAV to {outputPath}, samples: {pcm16.Length}, sampleRate: {sampleRate}");
        return Task.CompletedTask;
    }

    private static void ApplyEnvelope(float[] samples, int sampleRate)
    {
        int attack = Math.Min((int)(0.01 * sampleRate), samples.Length / 10);
        int release = attack;
        for (int i = 0; i < attack; i++)
            samples[i] *= (float)(i / (double)attack);
        for (int i = 0; i < release; i++)
            samples[samples.Length - 1 - i] *= (float)(i / (double)release);
    }

    private static float[] GenerateSine(double freq, int sampleRate, int sampleCount)
    {
        float[] s = new float[sampleCount];
        double increment = 2.0 * Math.PI * freq / sampleRate;
        double phase = 0.0;
        for (int i = 0; i < sampleCount; i++)
        {
            s[i] = (float)Math.Sin(phase);
            phase += increment;
        }
        return s;
    }

    // map a-z, 0-9, space -> distinct frequencies
    private static Dictionary<char, double> BuildCharFreqMap()
    {
        Dictionary<char, double> map = new Dictionary<char, double>();
        string keys = "abcdefghijklmnopqrstuvwxyz0123456789 ";
        double start = 220.0;
        double step = 16.0;
        for (int i = 0; i < keys.Length; i++)
        {
            map[keys[i]] = start + i * step;
        }
        return map;
    }
}

//////////// Goertzel-based Stub ASR ////////////
// Detect dominant frequency per frame and map back to characters.

class StubAsrProvider : IAsrProvider
{
    public string ProviderName => "StubASR-Goertzel";

    public async Task<string> RecognizeFromWavAsync(string wavPath)
    {
        if (!File.Exists(wavPath)) throw new FileNotFoundException(wavPath);

        // Read wav
        using var fs = new FileStream(wavPath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        // Minimal WAV parse: read header enough to get sampleRate and data offset
        // Not robust to all variants — ok for our generated files.
        var riff = new string(br.ReadChars(4));
        if (riff != "RIFF") throw new Exception("Not WAV");
        br.ReadInt32(); // chunk size
        var wave = new string(br.ReadChars(4));
        if (wave != "WAVE") throw new Exception("Not WAV");

        int sampleRate = 22050;
        int bitsPerSample = 16;
        int dataBytes = 0;
        // read chunks until "data"
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var chunkId = new string(br.ReadChars(4));
            int chunkSize = br.ReadInt32();
            if (chunkId == "fmt ")
            {
                short audioFormat = br.ReadInt16();
                short numChannels = br.ReadInt16();
                sampleRate = br.ReadInt32();
                int byteRate = br.ReadInt32();
                short blockAlign = br.ReadInt16();
                bitsPerSample = br.ReadInt16();
                if (chunkSize > 16) br.ReadBytes(chunkSize - 16);
            }
            else if (chunkId == "data")
            {
                dataBytes = chunkSize;
                break;
            }
            else
            {
                br.ReadBytes(chunkSize);
            }
        }

        if (dataBytes == 0) throw new Exception("No data chunk found in WAV.");

        int bytesPerSample = bitsPerSample / 8;
        int sampleCount = dataBytes / bytesPerSample;
        short[] pcm = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            pcm[i] = br.ReadInt16();
        }

        // Convert to float [-1,1]
        float[] floats = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++) floats[i] = pcm[i] / (float)short.MaxValue;

        // We'll scan sequential windows to find contiguous tone bursts and detect dominant freq in each.
        // Parameters must match the TTS generation durations.
        int windowSamples = (int)(0.18 * sampleRate); // same as TTS charDurationSec
        int hop = windowSamples + (int)(0.04 * sampleRate); // char + gap

        var freqMap = BuildCharFreqMap(); // same map as TTS

        // Precompute target freq array
        var targetFreqs = freqMap.Values.ToArray();
        char[] targetChars = freqMap.Keys.ToArray();

        StringBuilder result = new StringBuilder();

        for (int offset = 0; offset + windowSamples <= floats.Length; offset += hop)
        {
            // extract window
            float[] window = new float[windowSamples];
            Array.Copy(floats, offset, window, 0, windowSamples);
            // compute energy to skip silent windows
            double energy = 0;
            for (int i = 0; i < windowSamples; i++) energy += window[i] * window[i];
            energy /= windowSamples;
            if (energy < 1e-5) continue; // silence

            // detect frequency via Goertzel for each candidate target freq
            double bestFreq = 0;
            double bestMag = 0;
            for (int t = 0; t < targetFreqs.Length; t++)
            {
                double mag = GoertzelMagnitude(window, sampleRate, targetFreqs[t]);
                if (mag > bestMag) { bestMag = mag; bestFreq = targetFreqs[t]; }
            }
            // map bestFreq to char
            // find closest freq in map
            double bestDist = double.MaxValue;
            char bestChar = '?';
            for (int i = 0; i < targetFreqs.Length; i++)
            {
                double d = Math.Abs(targetFreqs[i] - bestFreq);
                if (d < bestDist) { bestDist = d; bestChar = targetChars[i]; }
            }
            result.Append(bestChar);
        }

        // post-process: collapse repeats caused by sliding windows
        string raw = result.ToString();
        string collapsed = CollapseRepeats(raw);
        // trim spaces near edges
        return collapsed.Trim();
    }

    private static string CollapseRepeats(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        StringBuilder sb = new StringBuilder();
        char prev = s[0];
        sb.Append(prev);
        for (int i = 1; i < s.Length; i++)
        {
            if (s[i] != prev) { sb.Append(s[i]); prev = s[i]; }
        }
        return sb.ToString();
    }

    // Goertzel algorithm for magnitude at target frequency
    private static double GoertzelMagnitude(float[] samples, int sampleRate, double targetFreq)
    {
        int N = samples.Length;
        double k = (int)(0.5 + ((N * targetFreq) / sampleRate));
        double omega = (2.0 * Math.PI * k) / N;
        double sine = Math.Sin(omega);
        double cosine = Math.Cos(omega);
        double coeff = 2.0 * cosine;
        double q0 = 0, q1 = 0, q2 = 0;
        for (int i = 0; i < N; i++)
        {
            q0 = coeff * q1 - q2 + samples[i];
            q2 = q1;
            q1 = q0;
        }
        double real = (q1 - q2 * cosine);
        double imag = (q2 * sine);
        double magnitude = Math.Sqrt(real * real + imag * imag);
        return magnitude;
    }

    // Same char->freq mapping
    private static Dictionary<char, double> BuildCharFreqMap()
    {
        Dictionary<char, double> map = new Dictionary<char, double>();
        string keys = "abcdefghijklmnopqrstuvwxyz0123456789 ";
        double start = 220.0;
        double step = 16.0;
        for (int i = 0; i < keys.Length; i++)
            map[keys[i]] = start + i * step;
        return map;
    }
}

//////////// ProviderFactory ////////////

static class ProviderFactory
{
    public static ITtsProvider GetTts(string name)
    {
        if (string.IsNullOrEmpty(name) || name == "auto" || name == "stub") return new StubTtsProvider();
        // Add logic to choose real providers (SystemSpeech) later.
        throw new ArgumentException($"Unknown TTS provider '{name}'");
    }

    public static IAsrProvider GetAsr(string name)
    {
        if (string.IsNullOrEmpty(name) || name == "auto" || name == "stub") return new StubAsrProvider();
        throw new ArgumentException($"Unknown ASR provider '{name}'");
    }
}

//////////// CLI / Program Entry ////////////

class SimpleCli
{
    static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  tts --text \"hello\" --output out.wav [--provider stub]");
        Console.WriteLine("  asr --input in.wav [--provider stub]");
        Console.WriteLine("  --selftest    run built-in self tests");
    }

    public static async Task<int> Run(string[] args)
    {
        if (args.Length == 0) { PrintHelp(); return 0; }
        if (args.Contains("--selftest"))
        {
            await SelfTest();
            return 0;
        }

        string cmd = args[0].ToLowerInvariant();
        var opts = ParseArgs(args.Skip(1).ToArray());

        if (cmd == "tts")
        {
            if (!opts.TryGetValue("text", out var text) || string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Missing --text");
                return 1;
            }
            if (!opts.TryGetValue("output", out var output) || string.IsNullOrEmpty(output))
            {
                Console.WriteLine("Missing --output");
                return 1;
            }
            string provider = opts.ContainsKey("provider") ? opts["provider"] : "stub";
            var tts = ProviderFactory.GetTts(provider);
            Console.WriteLine($"Using TTS provider: {tts.ProviderName}");
            await tts.SynthesizeToWavAsync(text, output);
            Console.WriteLine("Done.");
            return 0;
        }
        else if (cmd == "asr")
        {
            if (!opts.TryGetValue("input", out var input) || string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Missing --input");
                return 1;
            }
            string provider = opts.ContainsKey("provider") ? opts["provider"] : "stub";
            var asr = ProviderFactory.GetAsr(provider);
            Console.WriteLine($"Using ASR provider: {asr.ProviderName}");
            var text = await asr.RecognizeFromWavAsync(input);
            Console.WriteLine("Recognized text:");
            Console.WriteLine(text);
            return 0;
        }
        else
        {
            PrintHelp();
            return 1;
        }
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var dict = new Dictionary<string, string>();
        string key = null;
        foreach (var a in args)
        {
            if (a.StartsWith("--"))
            {
                key = a.Substring(2).ToLowerInvariant();
                dict[key] = "";
            }
            else if (key != null)
            {
                dict[key] = a;
                key = null;
            }
        }
        return dict;
    }

    // Basic self-test demonstrating synth + recognize correctness
    private static async Task SelfTest()
    {
        Console.WriteLine("Running self test...");
        string text = "hello world";
        string tmp = Path.Combine(Path.GetTempPath(), "stt_tts_selftest.wav");
        var tts = ProviderFactory.GetTts("stub");
        await tts.SynthesizeToWavAsync(text, tmp);
        var asr = ProviderFactory.GetAsr("stub");
        string recognized = await asr.RecognizeFromWavAsync(tmp);
        Console.WriteLine($"Original:   '{text}'");
        Console.WriteLine($"Recognized: '{recognized}'");
        if (recognized.Replace(" ", "") == text.Replace(" ", ""))
            Console.WriteLine("SELFTEST PASSED");
        else
            Console.WriteLine("SELFTEST FAILED (expected approximate match for stub mapping).");
    }
}

//////////// Top-level runner ////////////

//return await SimpleCli.Run(Environment.GetCommandLineArgs().Skip(1).ToArray());

public class Program
{
    public static async Task Main(string[] args)
    {
        await SimpleCli.Run(args);
    }
}
