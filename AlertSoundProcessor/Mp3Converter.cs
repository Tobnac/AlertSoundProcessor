using System;
using System.IO;
using NAudio.Lame;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    public static class Mp3Converter
    {
        public static void ConvertToMp3V3(string inPath, string outPath)
        {
            using (var reader = new AudioFileReader(inPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(outPath))) Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                var writer = new LameMP3FileWriter(outPath, reader.WaveFormat, 128);
                reader.CopyTo(writer);
                reader.Dispose();
                writer.Dispose();
            }
        }
    }
}