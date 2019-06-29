using System;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    public static class SoundProcessor
    {
        public static void NormalizeVolume(string inPath, string outPath)
        {
            using (var reader = new AudioFileReader(inPath))
            {
                var data = GetKeyValues(reader);

                // rewind and amplify
                reader.Position = 0;
                reader.Volume = 1.0f / data.MaxVolume;

                var stage1TempFile = inPath + "_temp_volNormd";
                var stage2TempFile = inPath + "_temp_volNormTrimWav";
                WaveFileWriter.CreateWaveFile16(stage1TempFile, reader);
                
                AudioTrimmer.TrimWavFile_Amount(stage1TempFile, stage2TempFile, data.StartCut, data.EndCut);

                Mp3Converter.ConvertToMp3V3(stage2TempFile, outPath);
                
                System.IO.File.Delete(stage1TempFile);
                System.IO.File.Delete(stage2TempFile);
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