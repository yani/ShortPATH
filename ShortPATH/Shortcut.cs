using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShortPATH
{
    class Shortcut
    {
        private string DirectoryPath;

        private string FilePath;

        public string Identifier { get; set; }

        public string Folder { get; set; }
            
        private string FileContents = "@echo off\r\n" +
                                      "SET org_dir=%cd%\r\n" +
                                      "cd /d \"{folder}\"\r\n" +
                                      "%*\r\n" +
                                      "cd /d \"%org_dir%\"";

        public Shortcut(string directoryPath, string identifier = null)
        {
            DirectoryPath = directoryPath;

            if(identifier != null)
            {
                Identifier = identifier;
                FilePath = GetFilePath(identifier);
                LoadFile();
            }
        }

        private Boolean LoadFile()
        {
            if (File.Exists(FilePath))
            {
                string contents = null;

                try
                {
                    using (StreamReader streamReader = new StreamReader(FilePath, Encoding.UTF8))
                    {
                        contents = streamReader.ReadToEnd();
                    }

                } catch(Exception exception)
                {
                    Console.WriteLine("The process failed: {0}", exception.ToString());
                }

                if (!String.IsNullOrEmpty(contents))
                {
                    Regex regex = new Regex("cd /d \"(.+?)\"");
                    Match match = regex.Match(contents);

                    if (match.Success)
                    {
                        string folder = match.Groups[1].ToString();

                        if (!String.IsNullOrEmpty(folder))
                        {
                            Folder = folder.Replace("\\\\", "\\");

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void SaveFile()
        {
            string outputFile = GetFilePath(Identifier);

            // If the identifier has changed we should remove the old file
            if (outputFile != FilePath) {
                File.Delete(FilePath);
            }

            string contents = FileContents.Replace("{folder}", Folder.Replace("\\", "\\\\"));

            File.WriteAllText(outputFile, contents);

            FilePath = outputFile;
        }

        public void Delete()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }

        private string GetFilePath(string identifier)
        {
            return DirectoryPath + "\\" + identifier + ".bat";
        }

        public override string ToString() {
            return Identifier;
        }
    }
}
