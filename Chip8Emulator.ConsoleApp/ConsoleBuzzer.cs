using ManagedBass;

namespace Chip8Emulator.ConsoleApp;

class ConsoleBuzzer : IBuzzer
{
    private int _soundHandle;

    public ConsoleBuzzer()
    {
        Bass.Init();

        SoundWaveGenerator waveGenerator = new SoundWaveGenerator();
        float[] data = waveGenerator.CreateSoundWave();
        _soundHandle = Bass.CreateSample(data.Length, data.Length, 1, 1, BassFlags.Loop | BassFlags.Float);
        Bass.SampleSetData(_soundHandle, data);
        _soundHandle = Bass.SampleGetChannel(_soundHandle);
    }

    public void StartPlaySound()
    {
        Bass.ChannelPlay(_soundHandle);
    }

    public void StopPlaySound()
    {
        Bass.ChannelPause(_soundHandle);
    }
}
