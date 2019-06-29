using System;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    public static class SoundProcessor
    {
        public static void NormalizeVolume(string inPath)
        {
            float max = 0;
            var iterationLength = 0;
            var startIteration = 0;
            var endIteration = 0;
            const double tolerance = 0.009;

            using (var reader = new AudioFileReader(inPath))
            {   
                // find the max peak
                float[] buffer = new float[reader.WaveFormat.SampleRate];
                int read;
                do
                {
                    read = reader.Read(buffer, 0, buffer.Length);
                    for (int n = 0; n < read; n++)
                    {
                        iterationLength++;
                        var abs = Math.Abs(buffer[n]);

                        if (abs > tolerance && startIteration == 0)
                        {
                            startIteration = iterationLength;
                        }

                        if (abs > tolerance)
                        {
                            endIteration = iterationLength;
                        }

                        if (abs > max)
                        {
                            max = abs;
                        }
                    }
                } while (read > 0);
                
                Console.WriteLine($"Max sample value: {max}");

                if (max == 0 || max > 1.0f) throw new InvalidOperationException("File cannot be normalized");

                // rewind and amplify
                reader.Position = 0;
                reader.Volume = 1.0f / max;

                double startPercent = (double) startIteration / iterationLength;
                double endPercent = (double) endIteration / iterationLength;
                var startTime = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * startPercent); 
                var endTime = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * endPercent);
                endTime = reader.TotalTime - endTime;

                // write out to a new WAV file
                var path = @"C:\Users\Tobnac\Desktop\output.wav";
                var pathb = @"C:\Users\Tobnac\Desktop\outputRes.wav";
                var pathc = @"C:\Users\Tobnac\Desktop\outputResTwo.mp3";
                WaveFileWriter.CreateWaveFile16(path, reader);
                
                AudioTrimmer.TrimWavFile_Amount(path, pathb, startTime, endTime);

//                ConvertToMp3V3(pathb, pathc);
//                ConvertToMp3(pathb, pathc);
//                ReWriteAsMp3(pathb, pathc);
            }
        }
    }
}