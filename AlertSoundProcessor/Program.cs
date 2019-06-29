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
            var fbSoundDir = @"C:\Users\Tobnac\WebstormProjects\fb\assets\communitySounds";
            
            foreach (var member in Directory.EnumerateDirectories(fbSoundDir))
            {
                var name = Path.GetFileName(member);
                
                foreach (var soundFile in Directory.GetFiles(member))
                {
                    var source = soundFile;
                    var resultFolder = @"C:\Users\Tobnac\RiderProjects\AlertSoundProcessor\AlertSoundProcessor\soundRes\";
                    var file = Path.GetFileName(source);
                    
                    SoundProcessor.ProcessSoundFile(source, resultFolder + name + "_" + file);
                    
                    // copy original file for comparison
                    File.Copy(source, @"C:\Users\Tobnac\RiderProjects\AlertSoundProcessor\AlertSoundProcessor\soundRes\" + name + "_" + file.Replace(".mp3", "_og.mp3"), true);
                }
            }
        }
    }
}