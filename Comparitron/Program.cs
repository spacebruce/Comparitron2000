//For licence details see; http://www.wtfpl.net
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Comparitron
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("Feed me a file formatted:");
                Console.WriteLine("Episode code | Page title - On the first line. Example : E1M1|At Dooms Gate");
                Console.WriteLine("Description text - On the second line. Example : Lorum ipsum whateverum.");
                Console.WriteLine("framenum | witty caption   - Until the end of the file. Example :  01130|Nice hat!");
                Console.WriteLine("Blank lines are ok, but otherwise deviating from the above will screw things up.");
                Console.ReadKey();
                return;
            }

            var inFile = args[0];
            var outFile = Path.ChangeExtension(inFile, "html");
            var outPath = Directory.GetCurrentDirectory();

            Script script = null;
            try
            {
                script = ReadScript(inFile, true);
            }
            catch (FormatException e)
            {
                Console.WriteLine("FORMAT ERROR: {0}", e.Message);
                return;
            }

            WriteScript(script, outFile, outPath, true);

            Console.WriteLine("HTML written to {0}", outFile);

            if (Directory.Exists(@"old\") && (Directory.Exists(@"new\")))
            {
                Console.WriteLine("Moving image files...");
                if (!Directory.Exists(@"output"))
                    Directory.CreateDirectory(@"output");

                foreach (var line in script.Frames)
                {
                    string tvName = string.Format("tv-{0:D5}.jpg", line.Number);
                    File.Copy(@"old\" + tvName, @"output\" + tvName, true);

                    string bdName = string.Format("bd-{0:D5}.jpg", line.Number);
                    File.Copy(@"new\" + bdName, @"output\" + bdName, true);
                }
                Console.WriteLine("Done!");
            }

            Console.WriteLine("All done, go home");
        }

        static Script ReadScript(string inFile, bool debug)
        {
            Script result = null;
            int lineno = 0;
            foreach (var line in File.ReadLines(inFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if(debug) Console.WriteLine(line);

                var parts = line.Split('|');

               

                if (lineno == 0)    //First line for data
                {         
                    var epcode = parts[0].Trim();   //Episode code, for pathing stuff
                    var title = parts[1].Trim();    //Episode title, for headings and stuff
                    result = new Script(title, epcode);
                }
                if (lineno == 1)   //Second line contains page description text
                {
                    result = new Script(result.Title, result.Epcode, line);
                }
                if (lineno >= 2) //Common case goes 3rd for optimisation reasons (sarcasm)
                {
                    if (parts.Length < 2)
                    {
                        throw new FormatException("not enough seperators");
                    }
                    else if (parts.Length > 2)
                    {
                        throw new FormatException("too many separators");
                    }

                    var imageNumber = parts[0].Trim();

                    int number = 0;

                    if (!int.TryParse(imageNumber, out number))
                    {
                        throw new FormatException(string.Format("image number not number at line {0}", lineno));
                    }

                    var text = parts[1].Trim();

                    result.Frames.Add(new Frame(number, text));
                }

                ++lineno;
            }
            return result;
        }

        static void WriteScript(Script script, string outFile, string outPath, bool debug)
        {
            using (var output = new StreamWriter(outFile))
            {
                //Insert top of page from file
                var templateStartName = Path.Combine(outPath, "template-start.html");
                Console.WriteLine(templateStartName);

                if (File.Exists(templateStartName))
                {
                    foreach(var line in File.ReadLines(templateStartName))
                    {
                        //Replacey bits
                        var outLine = line;
                        outLine = outLine.Replace(@"PAGECODE", script.Epcode);
                        outLine = outLine.Replace(@"PAGENAME", script.Title);
                        outLine = outLine.Replace(@"PAGETEXT", script.Text);

                        if (debug) Console.WriteLine(outLine);

                        output.WriteLine(outLine);
                    }
                }
                
                for (var i=0; i< script.Frames.Count; ++i)
                {
                    Frame frame = script.Frames[i];
                    Frame nextFrame = null;

                    if (i < script.Frames.Count- 1) //if within bounds, swap with actual frame.
                    {
                        nextFrame = script.Frames[i + 1];
                    }

                    if (frame.Text != "")
                    {
                        output.WriteLine("<li>");
                        output.WriteLine(frame.Text);
                    }
                    output.WriteLine("<div class=\"twentytwenty-container\">");
                    output.WriteLine("\t<img src=\"./images/{0}/tv-{1:D5}.jpg\" />", script.Epcode, frame.Number);
                    output.WriteLine("\t<img src=\"./images/{0}/bd-{1:D5}.jpg\" />", script.Epcode, frame.Number);
                    output.WriteLine("</div>");

                    if ((nextFrame == null) || (nextFrame.Text != ""))  //if next frame exists but has no text, cap this one off.
                    {
                        output.WriteLine("</li>");
                    }
                }

                //Insert bottom of page
                var templateEndName = outPath + @"\template-end.html";
                if (File.Exists(templateEndName))
                {
                    foreach (var line in File.ReadLines(templateEndName))
                    {
                        if(debug) Console.WriteLine(line);
                        output.WriteLine(line);
                    }
                }
            }
        }
    }
}
