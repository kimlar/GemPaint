using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GemPaint
{
    class GemImage
    {
        public string fileName = "";
        public Bitmap bitmap = null;
        public int width = 0;
        public int height = 0;

        public GemImage(int width, int height)
        {
            this.width = width;
            this.height = height;

            // Create bitmap image
            bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        public GemImage(string fileName)
        {
            this.fileName = fileName;
            Load();
        }

        public void Save()
        {
            if(fileName == "")
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if(saveFileDialog1.FileName == null)
                    {
                        MessageBox.Show("Sorry! Need a filename to save", "ERROR");
                        return;
                    }

                    fileName = saveFileDialog1.FileName;
                }
                else
                {
                    // User canceled the saving dialog
                    return;
                }
            }

            // Save it temporary in memory
            Bitmap bt = new Bitmap(bitmap);

            try
            {
                // Release the image
                bitmap.Dispose();

                // File exists? --> Delete if so
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                // Save image as a png-file
                bt.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                // Save the png to a temporary file
                bt.Save("Temp-" + DateTime.Now.Ticks.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            bt.Dispose();


            // Re-Load image
            bitmap = new Bitmap(fileName);

        }

        public void Load()
        {
            bitmap = new Bitmap(fileName);
            this.width = bitmap.Width;
            this.height = bitmap.Height;
        }
    }
}
