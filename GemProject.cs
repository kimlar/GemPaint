using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GemPaint
{
    class GemProject
    {
        private readonly string gemProjectFormatID = "GEM_PAINT_FORMAT_ID_20160301";
        public string gemProjectFile;
        public string gemProjectName;
        public List<string> gemProjectAllFiles;
        

        public GemProject(string gemProjectFile)
        {
            this.gemProjectFile = gemProjectFile;
        }

        public void Save()
        {
        }

        public void Load()
        {
            string line;
            List<string> lines = new List<string>();

            // Read file
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(gemProjectFile);
                while ((line = file.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                file.Close();
            }
            catch
            {
                MessageBox.Show("Could not open file: " + gemProjectFile, "ERROR");
                return;
            }

            //
            // Parse file
            //

            // Step 1: Check format ID
            if(!lines[0].Substring(0,lines[0].Length).Equals(gemProjectFormatID))
            {
                MessageBox.Show("Not a valid GEM project file. Sorry could not open the file: " + gemProjectFile, "ERROR");
                return;
            }

            // Step 2: Get project name
            string str = "Project: ";
            int len = str.Length;
            if(lines[1].Substring(0,len).Equals(str))
            {
                gemProjectName = lines[1].Substring(len);
            }
            else
            {
                MessageBox.Show("Could not find the project name. Sorry could not open the file: " + gemProjectFile, "ERROR");
            }

            // Step 3: Get list of all files (of the gem project)
            gemProjectAllFiles = new List<string>();
            for(int i = 2; i < lines.Count; ++i)
            {
                gemProjectAllFiles.Add(lines[i]);
            }

            //
            // Success!
            //
        }
    }
}
