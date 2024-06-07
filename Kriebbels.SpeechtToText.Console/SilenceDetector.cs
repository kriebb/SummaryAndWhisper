using NAudio.Wave;

namespace Kriebbels.SpeechtToText.Console;

public class SilenceDetector : ISilenceDetector
{
    public Silence[] FindSilences(string fileName, double silenceThreshold = -40)
    {

        bool IsSilence(float amplitude, double threshold)
        {
            double dB = 20 * Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }

        var silences = new List<Silence>();
        using (var reader = new AudioFileReader(fileName))
        {
            var buffer = new float[reader.WaveFormat.SampleRate * 4];

            long start = 0;
            bool eof = false;
            long counter = 0;
            bool detected = false;
            while (!eof)
            {
                int samplesRead = reader.Read(buffer, 0, buffer.Length);
                if (samplesRead == 0)
                {
                    eof = true;
                    if (detected)
                    {
                        double silenceSamples = (double)counter / reader.WaveFormat.Channels;
                        double silenceDuration = (silenceSamples / reader.WaveFormat.SampleRate) * 1000;
                        silences.Add(
                            new Silence(start, start + counter, TimeSpan.FromMilliseconds(silenceDuration)));
                    }
                }

                for (int n = 0; n < samplesRead; n++)
                {
                    if (IsSilence(buffer[n], silenceThreshold))
                    {
                        detected = true;
                        counter++;
                    }
                    else
                    {
                        if (detected)
                        {
                            double silenceSamples = (double)counter / reader.WaveFormat.Channels;
                            double silenceDuration = (silenceSamples / reader.WaveFormat.SampleRate) * 1000;
                            var last = silences.Count - 1;
                            if (last >= 0)
                            {
                                // see if we can merge with the last silence
                                var gap = start - silences[last].End;
                                var gapDuration = (double)gap / reader.WaveFormat.SampleRate * 1000;
                                if (gapDuration < 500)
                                {
                                    silenceDuration = silenceDuration + silences[last].Duration.TotalMilliseconds;
                                    silences[last] = new Silence(silences[last].Start, counter + silences[last].End,
                                        TimeSpan.FromMilliseconds(silenceDuration));
                                }
                                else
                                {
                                    silences.Add(
                                        new Silence(start, counter, TimeSpan.FromMilliseconds(silenceDuration)));
                                }
                            }
                            else
                            {
                                silences.Add(
                                    new Silence(start, counter, TimeSpan.FromMilliseconds(silenceDuration)));
                            }

                            start = start + counter;
                            counter = 0;
                            detected = false;
                        }
                    }
                }
            }
        }

        return silences.ToArray();
    }
}