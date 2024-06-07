using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace Kriebbels.SpeechtToText.Console;

public class AudioClientProvider(ILogger<AudioClientProvider> logger) : IAudioClientProvider
{
    public void DisplayCapabilities()
    {
        for (int n = 0; n < WaveInEvent.DeviceCount; n++)
        {
            WaveInCapabilities capabilities = WaveInEvent.GetCapabilities(n);
           logger.LogInformation("PName:        {0}", capabilities.ProductName);
           logger.LogInformation("Name:         {0} {1}", capabilities.NameGuid, WaveCapabilitiesHelpers.GetNameFromGuid(capabilities.NameGuid));
           logger.LogInformation("Product:      {0} {1}", capabilities.ProductGuid, WaveCapabilitiesHelpers.GetNameFromGuid(capabilities.ProductGuid));
           logger.LogInformation("Manufacturer: {0} {1}", capabilities.ManufacturerGuid, WaveCapabilitiesHelpers.GetNameFromGuid(capabilities.ManufacturerGuid));
        }
    }
    public AudioClient InitializeClient(AudioClientShareMode shareMode)
    {
        AudioClient audioClient = GetAudioClient();
        WaveFormat waveFormat = audioClient.MixFormat;
        long refTimesPerSecond = 10000000;
        audioClient.Initialize(shareMode,
            AudioClientStreamFlags.None,
            refTimesPerSecond,
            0,
            waveFormat,
            Guid.Empty);
        return audioClient;
    }

    public AudioClient GetAudioClient()
    {
        logger.LogInformation("Getting audio client...");

        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice defaultAudioEndpoint = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

        logger.LogInformation("Default audio endpoint: {FriendlyName}", defaultAudioEndpoint.FriendlyName);

        AudioClient audioClient = defaultAudioEndpoint.AudioClient;

        logger.LogInformation("Audio client obtained successfully");

        return audioClient;
    }
}