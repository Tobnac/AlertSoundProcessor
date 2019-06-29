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
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\Zizaran_2currency.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\BexBloopers_2currency_OG.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\Mathil_6veryvaluable.mp3";
//            var path = @"C:\Users\Tobnac\Documents\My Games\Path of Exile\testSong.mp3";
            var path = @"C:\Users\Tobnac\WebstormProjects\fb\assets\communitySounds\Asuzara\2currency.mp3";

            var fbSoundDir = @"C:\Users\Tobnac\WebstormProjects\fb\assets\communitySounds";
            
            foreach (var member in Directory.EnumerateDirectories(fbSoundDir))
            {
                var name = Path.GetFileName(member);
                
                foreach (var soundFile in Directory.GetFiles(member))
                {
                    var source = soundFile;
//                    var source = path;
                    
                    var file = Path.GetFileName(source);
                    SoundProcessor.NormalizeVolume(source, @"C:\Users\Tobnac\RiderProjects\AlertSoundProcessor\AlertSoundProcessor\soundRes\" + name + file);
                    
                    File.Copy(source, @"C:\Users\Tobnac\RiderProjects\AlertSoundProcessor\AlertSoundProcessor\soundRes\" + name + file.Replace(".mp3", "_og.mp3"));
//                    break;
                }
                
//                break;
            }
            
            
        }
    }
}