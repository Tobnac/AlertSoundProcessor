using System;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    public static class SoundProcessor
    {
        public static void NormalizeVolume(string inPath)
        {
            using (var reader = new AudioFileReader(inPath))
            {
                var data = GetKeyValues(reader);

                // rewind and amplify
                reader.Position = 0;
                reader.Volume = 1.0f / data.MaxVolume;

                // write out to a new WAV file
                var path = @"C:\Users\Tobnac\Desktop\output.wav";
                var pathb = @"C:\Users\Tobnac\Desktop\outputRes.wav";
                var pathc = @"C:\Users\Tobnac\Desktop\outputResTwo.mp3";
                WaveFileWriter.CreateWaveFile16(path, reader);
                
                AudioTrimmer.TrimWavFile_Amount(path, pathb, data.StartCut, data.EndCut);

//                ConvertToMp3V3(pathb, pathc);
//                ConvertToMp3(pathb, pathc);
//                ReWriteAsMp3(pathb, pathc);
            }
        }

        private static AudioKeyValues GetKeyValues(AudioFileReader reader)
        {
            var res = new AudioKeyValues();
            const double tolerance = 0.009;
            var startIteration = 0;
            var endIteration = 0;
                
            // find the max peak
            float[] buffer = new float[reader.WaveFormat.SampleRate];
            int read;
            do
            {
                read = reader.Read(buffer, 0, buffer.Length);
                for (int n = 0; n < read; n++)
                {
                    res.IterationCount++;
                    var volume = Math.Abs(buffer[n]);

                    if (volume > tolerance && startIteration == 0)
                    {
                        startIteration = res.IterationCount;
                    }

                    if (volume > tolerance)
                    {
                        endIteration = res.IterationCount;
                    }

                    if (volume > res.MaxVolume)
                    {
                        res.MaxVolume = volume;
                    }
                }
            } while (read > 0);

            Console.WriteLine($"Max sample value: {res.MaxVolume}");

            if (res.MaxVolume == 0 || res.MaxVolume > 1.0f) throw new InvalidOperationException("File cannot be normalized");
            
            double startPercent = (double) startIteration / res.IterationCount;
            double endPercent = (double) endIteration / res.IterationCount;
            res.StartCut = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * startPercent); 
            res.EndCut = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * endPercent);
            res.EndCut = reader.TotalTime - res.EndCut;
            
            return res;
        }

        public class AudioKeyValues
        {
            public int IterationCount { get; set; }
            public float MaxVolume { get; set; }
            public TimeSpan StartCut { get; set; }
            public TimeSpan EndCut { get; set; }
        }
    }
}