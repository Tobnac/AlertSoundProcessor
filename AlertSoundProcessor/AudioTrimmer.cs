using System;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    public static class AudioTrimmer
    {
        public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            using (var reader = new WaveFileReader(inPath))
            {
                using (var writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    var bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    var startPos = (int) cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                    var endBytes = (int) cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    var endPos = (int) reader.Length - endBytes; 

                    TrimWavFile_Inner(reader, writer, startPos, endPos);
                }
            }
        }

        private static void TrimWavFile_Inner(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
            var buffer = new byte[1024];
            while (reader.Position < endPos)
            {
                var bytesRequired = (int) (endPos - reader.Position);
                if (bytesRequired <= 0) continue;
                
                var bytesToRead = Math.Min(bytesRequired, buffer.Length);
                var bytesRead = reader.Read(buffer, 0, bytesToRead);
                
                if (bytesRead > 0)
                {
                    writer.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
}