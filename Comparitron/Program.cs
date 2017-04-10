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
                Console.WriteLine("framenum | witty caption   - Until the end of the file. Example :  01130|Nice hat!");
                Console.WriteLine("Blank lines are ok, but otherwise deviating from screw things up.");
                Console.ReadKey();
                return;
            }

            var inFile = args[0];
            var outFile = Path.ChangeExtension(inFile, "html");
            var outPath = Directory.GetCurrentDirectory();

            List<string> frames = new List<string>();
            
            using (var output = new StreamWriter(outFile))
            {
                int lineno = 0;

                string epcode = "changeme"; //Will hopefully get overwritten
                string pageTitle = "temptitle";
                string inputLine;
                string templateName;

                foreach (var line in File.ReadLines(inFile))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Console.WriteLine(line);

                    if (lineno == 0)    //First line for data
                    {
                        var parts = line.Split('|'); 
                        epcode = parts[0].Trim();       //Episode code, for pathing stuff
                        pageTitle = parts[1].Trim();    //Episode title, for headings and stuff

                        //Insert top of page from file
                        templateName = outPath + @"\template-start.html";
                        Console.WriteLine(templateName);

                        if (File.Exists(templateName))
                        {
                            using (StreamReader reader = new StreamReader(templateName))
                            {
                                inputLine = reader.ReadLine();
                                while (!reader.EndOfStream)
                                {
                                    //Replacey bits
                                    inputLine = inputLine.Replace(@"PAGENAME", pageTitle);
                                    inputLine = inputLine.Replace(@"PAGECODE", epcode);

                                    Console.WriteLine(inputLine);

                                    output.WriteLine(inputLine);
                                    inputLine = reader.ReadLine();
                                }
                                reader.Close();
                            }
                        }
                    }
                    else
                    {
                        var parts = line.Split('|');

                        if (parts.Length < 2)
                        {
                            Console.WriteLine("FORMAT ERROR: dumb line, not enough seperators");
                            return;
                        }
                        if (parts.Length > 2)
                        {
                            Console.WriteLine("FORMAT ERROR: help help, too many separators");
                            return;
                        }

                        var imagenumber = parts[0].Trim();
                        var text = parts[1].Trim();

                        frames.Add(imagenumber);

                        output.WriteLine("<li>");
                        output.WriteLine(text);
                        output.WriteLine("<div class=\"twentytwenty-container\">");
                        output.WriteLine("\t<img src=\"./images/{0}/tv-{1}.jpg\" />", epcode,imagenumber);
                        output.WriteLine("\t<img src=\"./images/{0}/bd-{1}.jpg\" />", epcode,imagenumber);
                        output.WriteLine("</div>");
                        output.WriteLine("</li>");
                    }

                    lineno++;
                }

                //Insert bottom of page
                templateName = outPath + @"\template-end.html";
                if (File.Exists(templateName))
                {
                    using (StreamReader reader = new StreamReader(templateName))
                    {
                        inputLine = reader.ReadLine();
                        while (!reader.EndOfStream)
                        {
                            Console.WriteLine(inputLine);

                            output.WriteLine(inputLine);
                            inputLine = reader.ReadLine();
                        }
                        reader.Close();
                    }
                }
            }

            Console.WriteLine("HTML written to {0}", outFile);

            if (Directory.Exists(@"old\") && (Directory.Exists(@"new\")))
            {
                Console.WriteLine("Moving image files...");
                if (!Directory.Exists(@"output"))
                    Directory.CreateDirectory(@"output");

                foreach (var line in frames)
                {
                    string tvName = string.Format("tv-{0}.jpg", line);
                    if (File.Exists(@"output\" + tvName))
                        File.Delete(@"output\" + tvName);
                    File.Copy(@"old\" + tvName, @"output\" + tvName);

                    string bdName = string.Format("bd-{0}.jpg", line);
                    if (File.Exists(@"output\" + bdName))
                        File.Delete(@"output\" + bdName);
                    File.Copy(@"new\" + bdName, @"output\" + bdName);
                }
                Console.WriteLine("Done!");
            }

            Console.WriteLine("All done, go home");
        }
    }
}
