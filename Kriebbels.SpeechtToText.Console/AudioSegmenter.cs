using System.Diagnostics;
using NAudio.Wave;

namespace Kriebbels.SpeechtToText.Console;

public class AudioSegmenter : IAudioSegmenter
{
    public AudioSegment[] FindAudibleSegments(string fileName, Silence[] silences){
        var segments = new List<AudioSegment>();
        using (var reader = new AudioFileReader(fileName)){
            var totalSamples = reader.Length;
            for(var i = 0; i< silences.Length; i++){
                if(i == 0 && silences[i].Start > 0){
                    segments.Add(new AudioSegment(0, silences[i].Start, TimeSpan.FromMilliseconds(silences[i].Start / reader.WaveFormat.SampleRate * 1000)));
                }
                if(i == silences.Length - 1 && silences[i].End < totalSamples){
                    segments.Add(new AudioSegment(silences[i].End, totalSamples, TimeSpan.FromMilliseconds((totalSamples - silences[i].End) / reader.WaveFormat.SampleRate * 1000)));
                }
                if(i < silences.Length - 1){
                    var current = silences[i];
                    var next = silences[i+1];
                    if(current.End < next.Start)
                    {
                        segments.Add(new AudioSegment(current.End, next.Start, TimeSpan.FromMilliseconds((next.Start - current.End) / reader.WaveFormat.SampleRate * 1000)));

                        var lastSegment = segments.Last();
                            
                        Debug.WriteLine($"{ lastSegment.Start} - { lastSegment.End} - { lastSegment.Duration:g}");
                    }
                }
            
            }
        }
        return segments.ToArray();
    }
}