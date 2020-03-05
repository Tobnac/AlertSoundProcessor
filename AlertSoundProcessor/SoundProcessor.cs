using System;
using System.IO;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    public static class SoundProcessor
    {
        public static void ProcessSoundFile(string inPath, string outPath)
        {
            using (var reader = new AudioFileReader(inPath))
            {
                var stage1TempFile = inPath + "_temp_volNormd";
                var stage2TempFile = inPath + "_temp_volNormTrimWav";
                var data = GetKeyValues(reader);

                Console.WriteLine($"File: {(Path.GetFileName(Path.GetDirectoryName(inPath)) + "/" + Path.GetFileName(inPath)).PadRight(40)} Volume: {String.Format("{0:0.000}", data.MaxVolume).PadRight(15)} StartTrim: {data.StartCut.TotalSeconds}s");
                
                NormalizeVolume(reader, data); // convert to WAV file for further processing
                WaveFileWriter.CreateWaveFile16(stage1TempFile, reader);
                AudioTrimmer.TrimWavFile(stage1TempFile, stage2TempFile, data.StartCut, data.EndCut);
                Mp3Converter.ConvertToMp3V3(stage2TempFile, outPath);
                
                // remove temp files
                File.Delete(stage1TempFile);
                File.Delete(stage2TempFile);
            }
        }

        private static void NormalizeVolume(AudioFileReader reader, AudioKeyValues data)
        {
            reader.Volume = 1.0f / data.MaxVolume;
        }

        private static AudioKeyValues GetKeyValues(AudioFileReader reader)
        {
            var res = new AudioKeyValues();
            const double volumeThreshold = 0.009;
            const double endToleranceModifier = 0.5;
            var startIteration = 0;
            var endIteration = 0;
            var totalIterationCount = 0;
            var buffer = new float[reader.WaveFormat.SampleRate];
            int read;
            
            // iterate file
            do
            {
                read = reader.Read(buffer, 0, buffer.Length);
                
                for (var n = 0; n < read; n++)
                {
                    totalIterationCount++;
                    var volume = Math.Abs(buffer[n]);

                    // found start trim pos
                    if (volume > volumeThreshold && startIteration == 0)
                    {
                        startIteration = totalIterationCount;
                    }

                    // update end trim pos -> less threshold/strict because a longer end is not bad
                    if (volume > volumeThreshold * endToleranceModifier)
                    {
                        endIteration = totalIterationCount;
                    }

                    // update max volume
                    if (volume > res.MaxVolume)
                    {
                        res.MaxVolume = volume;
                    }
                }
            } while (read > 0);
            
            // reset current position to start for future reads
            reader.Position = 0;

            if (res.MaxVolume <= 0 || res.MaxVolume > 1.0f) throw new InvalidOperationException("File cannot be normalized");
            
            // convert iteration# to time
            var startPercent = (double) startIteration / totalIterationCount;
            var endPercent = (double) endIteration / totalIterationCount;
            res.StartCut = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * startPercent); 
            res.EndCut = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * endPercent);
            res.EndCut = reader.TotalTime - res.EndCut;
            
            return res;
        }

        private class AudioKeyValues
        {
            public float MaxVolume { get; set; }
            public TimeSpan StartCut { get; set; }
            public TimeSpan EndCut { get; set; }
        }
    }
}