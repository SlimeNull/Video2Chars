using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using CHO.Json;
using System.Threading;
using ForMinecraft;
using Accord.Video.FFMPEG;

namespace Video2Chars
{
    class Program
    {
        static string videoname;
        static string outputname;
        static void Main(string[] args)
        {
#if DEBUG

#endif
            ConsArgs consArgs = new ConsArgs(args);
            VideoCharFrameGenerator videoCharFrameGenerator;
            char[] baseChars = "!@#$%^&*()_+=-0987654321qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM,. ".ToArray();

            bool preview = false;

            if (consArgs.Content.Length == 2)
            {
                videoname = consArgs.Content[0];
                outputname = consArgs.Content[1];

                if (consArgs.Propertie.ContainsKey("BASECHARS"))
                {
                    baseChars = consArgs.Propertie["BASECHARS"].ToArray();
                }

                if (consArgs.Booleans.Contains("PREVIEW"))
                {
                    preview = true;
                }

                int left = Console.CursorLeft;
                int top = Console.CursorTop;

                try
                {
                    Image src = Image.FromFile(videoname);
                    ImageCharFrameGenerator imageCharFrameGenerator = new ImageCharFrameGenerator(baseChars);
                    string rst = imageCharFrameGenerator.Bitmap2String((Bitmap)src);
                    File.WriteAllText(outputname, rst);
                    return;
                }
                catch(IOException)
                {
                    Console.WriteLine("Error: Cannot write file");
                    Environment.ExitCode = -2;
                    return;
                }
                catch { }

                videoCharFrameGenerator = new VideoCharFrameGenerator(baseChars);
                videoCharFrameGenerator.OpenVedio(videoname);

                if (videoCharFrameGenerator.IsOpen)
                {
                    int drawIndex = 0;
                    List<string> frames = new List<string>();
                    foreach (string i in videoCharFrameGenerator.CharFrames)
                    {
                        drawIndex++;
                        frames.Add(i);
                        if (preview)
                        {
                            Console.CursorLeft = 0;
                            Console.CursorTop = 0;
                            Console.WriteLine(i);
                            Console.Title = $"Null.Video2Chars: {videoname} 渲染进度:{drawIndex}/{videoCharFrameGenerator.FrameCount}";
                        }
                        else
                        {
                            Console.Write($"\rNull.Video2Chars: {videoname} 渲染进度:{drawIndex}/{videoCharFrameGenerator.FrameCount}");
                        }
                    }
                    Console.WriteLine();
                    File.WriteAllText(outputname, JsonData.Create(frames).ToJsonText());
                }
                else
                {
                    Console.WriteLine("Error: Cannot parse file");
                    Environment.ExitCode = -2;
                    return;
                }
            }
            else
            {
                Console.WriteLine("Argument count error, arguments must contain source file and output file, argument count must be 2");
                Environment.Exit(-1);
            }
        }
    }
}
