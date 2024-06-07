using NAudio.Wave;

namespace Kriebbels.SpeechtToText.Console;

public class AudioFileTrimmer : IAudioFileTrimmer
{
    public IEnumerable<string> FindTrimmedFiles(IEnumerable<AudioSegment> audioSegments,
        string earningsCallFilepath)
    {
        var trimmedFiles = new List<string>();
        foreach(var audioSegment in audioSegments ){
            var trimmedFile = $"./EarningsCall-{audioSegment.Start}-{audioSegment.End}.wav";
            trimmedFiles.Add(trimmedFile);
            using (var reader = new AudioFileReader(earningsCallFilepath))
            {
                reader.Position = audioSegment.Start;
                using (WaveFileWriter writer = new WaveFileWriter(trimmedFile, reader.WaveFormat))
                {
                    var endPos = audioSegment.End;
                    byte[] buffer = new byte[1024];
                    while (reader.Position < endPos)
                    {
                        int bytesRequired = (int)(endPos - reader.Position);
                        if (bytesRequired > 0)
                        {
                            int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                            int bytesRead = reader.Read(buffer, 0, bytesToRead);
                            if (bytesRead > 0)
                            {
                                writer.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }
            }
        }

        return trimmedFiles;
    }}