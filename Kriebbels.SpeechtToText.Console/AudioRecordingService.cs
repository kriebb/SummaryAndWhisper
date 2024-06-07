using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace Kriebbels.SpeechtToText.Console;

public class AudioRecordingService(
    IAudioClientProvider audioClientProvider,
    IFileService fileService,
    ILogger<AudioRecordingService> logger): IAudioRecordingService
{


    
    private readonly CancellationTokenSource _cts = new();
    private readonly WaveFormat _waveFormat =  new(16000, 16, 1);
    private readonly BlockingCollection<byte[]> _blockingCollection = new();
    private readonly WaveInEvent _waveIn = new();

    private Task? _task = null;


    public async Task<Task?> StartRecordAudioAsync(AudioFile path)

    {
        _task = Task.Run(() =>
        {

            using var writer = new WaveFileWriter(path.FullPath, _waveFormat);
            try
            {
                foreach (var data in _blockingCollection.GetConsumingEnumerable())
                {
                    writer.Write(data, 0, data.Length);
                }
            }
            finally
            {
                writer.Close();
            }
        }, _cts.Token);

        audioClientProvider.DisplayCapabilities();

        fileService.Exists(path.FullPath).CheckIf(exists => exists, exists =>
            fileService.Delete(path.FullPath));


        _waveIn.DeviceNumber = 0;
        _waveIn.WaveFormat = _waveFormat;
        _waveIn.BufferMilliseconds = 200;


        _waveIn.DataAvailable += (sender, eventArgs) =>
        {
            var bufferCopy = new byte[eventArgs.BytesRecorded];
            Array.Copy(eventArgs.Buffer, bufferCopy, eventArgs.BytesRecorded);
            _blockingCollection.Add(bufferCopy, _cts.Token);
        };



        _waveIn.StartRecording();

        return _task;
    }

    public void StopRecordAudio()
    {

        try
        {
            _waveIn.StopRecording();
            _cts.Cancel();
        }
        finally
        {
            _blockingCollection.CompleteAdding();

            _waveIn.Dispose();
        }
    }

 

    public void Dispose()
    {
        try
        {
            if(!_cts.IsCancellationRequested)
                _cts.Cancel();
            _cts.Dispose();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Could not dispose of CancellationTokenSource");
        }
        try
        {
            if(!_blockingCollection.IsAddingCompleted)
                _blockingCollection.CompleteAdding();
            
            _blockingCollection.Dispose();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Could not dispose of BlockingCollection");
        }
        try
        {
            _task?.Dispose();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Could not dispose of Task");
        }
        try
        {
            _waveIn.Dispose();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Could not dispose of WaveInEvent");
        }

    }


}

public interface IAudioRecordingService:IDisposable
{
    Task<Task?> StartRecordAudioAsync(AudioFile audioFile);
    void StopRecordAudio();

}
