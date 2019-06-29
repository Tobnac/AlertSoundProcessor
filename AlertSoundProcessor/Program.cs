using System;
using System.IO;
using NAudio.Lame;
using NAudio.Wave;

namespace AlertSoundProcessor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
//            var path = @"C:\Users\Tobnac\Zeugs\my girls & B & tumblr\todo sort mfg media\hey adora.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\Zizaran_1maybevaluable.mp3";
            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\Zizaran_2currency.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\BexBloopers_2currency_OG.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\Mathil_6veryvaluable.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\testSong.mp3";
            
            NormalizeVolume(path);
            return;
            
            var reader = new Mp3FileReader(path);
            var buffer = new byte[reader.Length];
            reader.Read(buffer, 0, (int)reader.Length);

            var lengthDiv = reader.Length / 40;
            var vol = 0.0;

            for (var i = 0; i < reader.Length-1; i++)
            {
                if (i % lengthDiv == 0)
                {
                    vol *= 3;
                    vol /= lengthDiv;
                    for (var j = 0; j < vol; j++) Console.Write("-");
                    Console.WriteLine((int) i / lengthDiv);
                    
                    vol = 0;
                    continue;
                }
//                vol += GetVolumes(buffer, i);
                vol += SetVolume(buffer, i);
            }
        }

        private static void Bla(AudioFileReader reader, float max)
        {
            int counter = 0;
            sbyte silenceThreshold = 2;
            bool volumeFound = false;
            bool eof = false;
            long oldPosition = reader.Position;

            var buffer = new float[reader.WaveFormat.SampleRate * 4];
            while (!volumeFound && !eof)
            {
                int samplesRead = reader.Read(buffer, 0, buffer.Length);
                if (samplesRead == 0)
                    eof = true;

                for (int n = 0; n < samplesRead; n++)
                {
                    double ampli = Math.Abs(buffer[n]);
                    if (ampli > 1 || ampli < 0) throw new Exception("invalid ampl");
                    double dB = 20 * Math.Log10(Math.Abs(ampli));
                    Console.WriteLine("DB: " + dB);
                    
                    if (n % 1000 == 0)
                    {
                        for (var i = 0; i < Math.Abs(dB)/100; i++) Console.Write("-");
                        Console.WriteLine((double)n / (double)buffer.Length);
                    }
                }
            }

            // reset position
            reader.Position = oldPosition;

            double silenceSamples = (double)counter / reader.WaveFormat.Channels;
            double silenceDuration = (silenceSamples / reader.WaveFormat.SampleRate) * 1000;
            return;// TimeSpan.FromMilliseconds(silenceDuration);
        }
        
        private static void NormalizeVolume(string inPath)
        {
            float max = 0;
//            TimeSpan startTime = TimeSpan.Zero;
            var patchSize = 1;
            var iterationLength = 0;
            var currentPatch = patchSize;
            var startIteration = 0;
            var endIteration = 0;
            var sum = 0.0;
            var tolerance = 0.009;
            var maxIteration = 0;

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

//                        for (var i = 0; i < abs*10; i++) Console.Write("-");
//                        Console.WriteLine("");
                        
                        if (iterationLength % 1000 == 0)
                        {
//                            for (var i = 0; i < abs*1000; i++) Console.Write("-");
//                            Console.WriteLine((double)n / (double)buffer.Length);
                        }

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
                            maxIteration = iterationLength;
                        }
                    }
                } while (read > 0);
                Console.WriteLine($"Max sample value: {max}");

                if (max == 0 || max > 1.0f)
                    throw new InvalidOperationException("File cannot be normalized");

                // rewind and amplify
                reader.Position = 0;
                reader.Volume = 1.0f / max;
                
//                Bla(reader, max);

//                reader.CurrentTime = TimeSpan.FromSeconds(0.5);

//                return;

                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
//                        Console.WriteLine(GetStartEndTimes(buffer, reader.TotalTime, 0.001 * i, j).Item1);
                    }
                }

                double startPercent = (double) startIteration / iterationLength;
                double endPercent = (double) endIteration /iterationLength;
                var startTime = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * startPercent); 
                var endTime = TimeSpan.FromMilliseconds(reader.TotalTime.TotalMilliseconds * endPercent);
                endTime = reader.TotalTime - endTime;

//                var times = GetStartEndTimes(buffer, reader.TotalTime, tolerance, 1);

                // write out to a new WAV file
                var path = @"C:\Users\Tobnac\Desktop\output.wav";
                var pathb = @"C:\Users\Tobnac\Desktop\outputRes.wav";
                var pathc = @"C:\Users\Tobnac\Desktop\outputResTwo.mp3";
                WaveFileWriter.CreateWaveFile16(path, reader);
//                TrimWavFile_Amount(path, pathb, times.Item1, times.Item2);
                
                TrimWavFile_Amount(path, pathb, startTime, endTime);

//                ConvertToMp3V3(pathb, pathc);
//                ConvertToMp3(pathb, pathc);
//                ReWriteAsMp3(pathb, pathc);
            }
        }
        
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

//        public static void ReWriteToMp3(string inPathg1)
//        {
//            WaveStream InStr = new WaveStream(inPath);
//            try
//            {
//                Mp3Writer writer = new Mp3Writer(new FileStream("SomeFile.mp3", 
//                    FileMode.Create), InStr.Format);
//                try
//                {
//                    byte[] buff = new byte[writer.OptimalBufferSize];
//                    int read = 0;
//                    while ( (read = InStr.Read(buff, 0, buff.Length)) > 0 )
//                    {
//                        writer.Write(buff, 0, read);
//                    }
//                }
//                finally
//                {
//                    writer.Close();
//                }
//            }
//            finally
//            {
//                InStr.Close();
//            }
//        }
        
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

        private static Tuple<TimeSpan, TimeSpan> GetStartEndTimes(float[] buffer, TimeSpan length, double tolerance, int patchSize)
        {
            TimeSpan startTime = TimeSpan.Zero, endTime = length;
            var currentPatch = patchSize;
            var sum = 0.0;
//            int start = 0, end = buffer.Length;
            
            // todo: ziz "money" volumes seem weird, the "mmmmm" at the beginning is not detected as volume
            
            for (int n = 0; n < buffer.Length; n++)
            {
                var volume = Math.Abs(buffer[n]);

                if (currentPatch-- > 0)
                {
                    sum += volume;
                    continue;
                }
                
                if (sum/patchSize > tolerance)
                {
//                    start = n;
                    double perc = (double) n / (double) buffer.Length;
                    var start = length.TotalMilliseconds * perc;
                    startTime = TimeSpan.FromMilliseconds(start);
                    break;
                }

                currentPatch = patchSize;
                sum = 0;
            }

            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                var volume = Math.Abs(buffer[i]);
                if (volume > tolerance)
                {
//                    end = i;
                    double perc = (double) i / (double) buffer.Length;
                    var start = length.TotalMilliseconds * perc;
                    endTime = TimeSpan.FromMilliseconds(start);
                    endTime = length - endTime;
                    break;
                }
            }
            
            return new Tuple<TimeSpan, TimeSpan>(startTime, endTime);
        }

        private static double SetVolume(byte[] buffer, int index)
        {
            for (int i = 0 ; i < buffer.Length ; ++i)
            {
                i = index;
                // convert to 16-bit
                short sample = (short) ((buffer[i * 2 + 1] << 8) | buffer[i * 2]);
                
//                Console.WriteLine(sample);
                return sample;

                // scale
                const double gain = 0.5; // value between 0 and 1.0
                sample = (short)(sample * gain + 0.5);

                // back to byte[]
                buffer[i*2+1] = (byte)(sample >> 8);
                buffer[i*2] = (byte)(sample & 0xff);
            }
            
            throw new Exception("f");
        }

        private static double GetVolumes(byte[] buffer, int index)
        {
            short sample16Bit = BitConverter.ToInt16(buffer,index);
//            return sample16Bit;
            double volume = Math.Abs(sample16Bit / 32768.0);
            return volume;
            double decibels = 20 * Math.Log10(volume);
            return decibels;
        }
        
        public static void TrimWavFile_ByteCount(string inPath, string outPath, int startTime, int endTime)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    TrimWavFile(reader, writer, startTime, endTime);
                }
            }
        }
        
        public static void TrimWavFile_Amount(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                    int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    int endPos = (int)reader.Length - endBytes; 

                    TrimWavFile(reader, writer, startPos, endPos);
                }
            }
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            reader.Position = startPos;
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
                        writer.WriteData(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}