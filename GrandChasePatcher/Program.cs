using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrandChasePatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!File.Exists("./.version"))
            {
                    Sign("0");
            }

            int version = Convert.ToInt32(File.ReadAllText(@"./.version"));
            switch (version + 1)
            {
                case 1:
                    Sign("1");
                    Patch();
                    goto case 2;
                case 2:
                    Sign("2");
                    changeACBackground();
                    fixMainSoundTrack();
                    goto case 3;
                case 3:
                    Sign("3");
                    changeACBackground();
                    break;
            }

            Start();
        }
        static void Patch()
        {
            var address = getAddr();
            BinaryWriter bw = new BinaryWriter(File.Open(@"./main.exe", FileMode.Open));
            bw.BaseStream.Seek(0xB0A1E0, SeekOrigin.Begin);
            bw.Write(address);
            bw.BaseStream.Seek(0x9FBAEC, SeekOrigin.Begin);
            bw.Write(address);
            bw.Close();
        }

        static void Start()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("start main.exe __kogstudios_original_service__");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
        }

        static void Sign(string version)
        {
            string path = @"./.version";
            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(version);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        static void fixMainSoundTrack()
        {
            File.Delete("./Music\\theme_extinction.mp3");
            File.Copy("./Music\\Track9.mp3", "./Music\\theme_extinction.mp3");
        }
        static void changeACBackground()
        {
            String access = File.ReadAllText(@"./.access");
            String host = File.ReadAllText(@"./addr.txt");

            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential("admin", access);
                client.DownloadFile($"http://{host}:9999/resource/Protect.bmp", @"./Gameguard/Protect.bmp");
            }
        }
        static byte[] getAddr()
        {
            var addr = new byte[0x0F];
            int buffer;
            using (var sr = new StreamReader(@"./addr.txt"))
            {
                for (int i = 0; i < addr.Length; i++)
                {
                    buffer = sr.Read();
                    if (buffer >= 0)
                    {
                        addr[i] = (byte)buffer;
                    }
                    else
                    {
                        addr[i] = 0x00;
                    }
                }
                sr.Close();
            }
            return addr;
        }
    }
}
