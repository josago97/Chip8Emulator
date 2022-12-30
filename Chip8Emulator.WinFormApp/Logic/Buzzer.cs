using ManagedBass;

namespace Chip8Emulator.WinFormApp.Logic;

internal class Buzzer : IBuzzer
{
    private const int FREQUENCY = 230;

    private int _soundHandle;

    public Buzzer()
    {
        Bass.Init();

        SoundWaveGenerator waveGenerator = new SoundWaveGenerator(FREQUENCY);
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
