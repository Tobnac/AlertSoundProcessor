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
            byte[] buffer;
            WaveFormat format;
            using (var reader = new AudioFileReader(inPath))
            {
                format = reader.WaveFormat;
                buffer = new byte[reader.WaveFormat.SampleRate];
                int read;
                do
                {
                    read = reader.Read(buffer, 0, buffer.Length - (buffer.Length % reader.WaveFormat.BlockAlign));
                } while (read > 0);

//                var mp3Buffer = ConvertToMp3(buffer);
                
//                if (File.Exists(outPath)) File.Delete(outPath);
//                var outStream = File.Open(outPath, FileMode.CreateNew);
                if (File.Exists(outPath)) Console.WriteLine("dd");
                var writer = new LameMP3FileWriter(outPath, format, 128, null);
                reader.CopyTo(writer);
                reader.Dispose();
                writer.Dispose();
            }
        }

        public static void ReWriteAsMp3(string inPath, string outPath)
        {
            // todo: convert back to mp3?

            byte[] buffer;
            using (var reader = new AudioFileReader(inPath))
            {
                buffer = new byte[reader.WaveFormat.SampleRate];
                int read;
                do
                {
                    read = reader.Read(buffer, 0, buffer.Length - (buffer.Length % reader.WaveFormat.BlockAlign));
                } while (read > 0);

                var mp3Buffer = ConvertToMp3(buffer);
            }

//            var stream = File.Open(outPath, FileMode.CreateNew);
//            stream.Write();

//            using (var writer = new LameMP3FileWriter(outPath, format, 32, null))
//            {
//                writer.Write(buffer, 0, buffer.Length);
//            }
        }

        
        public static void ConvertToMp3(string inPath, string outPath)
        {
            byte[] buffer;
            byte[] resBuffer;
            WaveFormat format;
            using (var inputStream = new FileStream(inPath, FileMode.Open,
                FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                Console.WriteLine(inputStream.CanRead);
                using (var reader = new WaveFileReader(inputStream))
                {
                    buffer = new byte[reader.WaveFormat.SampleRate];
                    int read;
                    do
                    {
                        read = reader.Read(buffer, 0, buffer.Length - (buffer.Length % reader.WaveFormat.BlockAlign));
                    } while (read > 0);

//                var mp3Buffer = ConvertToMp3(buffer);
                }
            }

            var target = new WaveFormat(8000, 16, 1);
            using (var outPutStream = new MemoryStream())
            using (var waveStream = new WaveFileReader(new MemoryStream(buffer)))
            using (var conversionStream = new WaveFormatConversionStream(target, waveStream))
            using (var writer = new LameMP3FileWriter(outPutStream, conversionStream.WaveFormat, 32, null))
            {
                format = conversionStream.WaveFormat;
                conversionStream.CopyTo(writer);

                resBuffer = outPutStream.ToArray();
            }
            
//            var stream = File.Open(outPath, FileMode.CreateNew);
//            stream.Write();
            
            using (var writer = new LameMP3FileWriter(outPath, format, 32, null))
            {
                writer.Write(resBuffer, 0, resBuffer.Length);
            }
        }
        
        public static byte[] ConvertToMp3(byte[] buffer)
        {
            var target = new WaveFormat(8000, 16, 1);
            using (var outPutStream = new MemoryStream())
            using (var waveStream = new WaveFileReader(new MemoryStream(buffer)))
            using (var conversionStream = new WaveFormatConversionStream(target, waveStream))
            using (var writer = new LameMP3FileWriter(outPutStream, conversionStream.WaveFormat, 32, null))
            {
                conversionStream.CopyTo(writer);

                return outPutStream.ToArray();
            }
        }
    }
}