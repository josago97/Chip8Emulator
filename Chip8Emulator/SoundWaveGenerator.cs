using System;

namespace Chip8Emulator;

public class SoundWaveGenerator
{
    private const double DEFAULT_FREQUENCY = 440; // A4
    private const double DEFAULT_AMPLITUDE = 1;
    private const int DEFAULT_SAMPLE_RATE = 44100; // 44100 Hz
    private const WaveType DEFAULT_WAVE_TYPE = WaveType.Sin; // Sin Wave

    public double Frequency { get; set; }
    public double Amplitude { get; set; }
    public int SampleRate { get; set; }
    public WaveType WaveType { get; set; }

    public SoundWaveGenerator(double frequency = DEFAULT_FREQUENCY, double amplitude = DEFAULT_AMPLITUDE, int sampleRate = DEFAULT_SAMPLE_RATE, WaveType waveType = DEFAULT_WAVE_TYPE)
    {
        Frequency = frequency;
        Amplitude = amplitude;
        SampleRate = sampleRate;
        WaveType = waveType;
    }

    /*public Stream CreateSoundFile()
    {
        return null;
    }*/

    public float[] CreateSoundWave()
    {
        float[] result = new float[SampleRate];

        switch (WaveType)
        {
            case WaveType.Sin:
                CreateSinWave(result);
                break;

            case WaveType.Square:
                CreateSquareWave(result);
                break;

            case WaveType.Triangle:
                CreateTriangleWave(result);
                break;

            default:
                CreateSawToothWave(result);
                break;
        }

        return result;
    }

    private void CreateSinWave(float[] samples)
    {
        double angleStep = 2 * Math.PI * Frequency / SampleRate;

        for (int t = 0; t < samples.Length; ++t)
        {
            samples[t] = (float)(Math.Sin(t * angleStep) * Amplitude);
        }
    }

    private void CreateSquareWave(float[] samples)
    {
        double step = 2 * Frequency / SampleRate;

        for (int t = 0; t < samples.Length; ++t)
        {
            double sample = (((t * step) % 2) - 1) * Amplitude;
            sample = sample > 0 ? Amplitude : -Amplitude;
            samples[t] = (float)sample;
        }
    }

    private void CreateTriangleWave(float[] samples)
    {
        double step = 2 * Frequency / SampleRate;

        for (int t = 0; t < samples.Length; ++t)
        {
            double sample = 2 * ((t * step) % 2);
            if (sample > 1) sample = 2 - sample;
            if (sample < -1) sample = -2 - sample;
            samples[t] = (float)(sample * Amplitude);
        }
    }

    private void CreateSawToothWave(float[] samples)
    {
        double step = 2 * Frequency / SampleRate;

        for (int t = 0; t < samples.Length; ++t)
        {
            samples[t] = (float)((((t * step) % 2) - 1) * Amplitude);
        }
    }
}

/// <summary>
/// Wave generated type
/// </summary>
public enum WaveType
{
    /// <summary>
    /// Sine wave
    /// </summary>
    Sin,
    /// <summary>
    /// Square wave
    /// </summary>
    Square,
    /// <summary>
    /// Triangle Wave
    /// </summary>
    Triangle,
    /// <summary>
    /// Sawtooth wave
    /// </summary>
    SawTooth
}
