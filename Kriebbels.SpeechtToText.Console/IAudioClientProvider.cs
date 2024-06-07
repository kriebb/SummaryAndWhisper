using NAudio.CoreAudioApi;

namespace Kriebbels.SpeechtToText.Console;

public interface IAudioClientProvider
{
    void DisplayCapabilities();
    AudioClient InitializeClient(AudioClientShareMode shareMode);
    AudioClient GetAudioClient();
}