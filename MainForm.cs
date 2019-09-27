using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// ****
// **** PROJECT: Project GemPaint - No name yet!
// ****
// ***********************************************

// TODO:
// * Refactor PreviewBox into a class
// * Refactor DrawBox into a class
// * Implement TDI (Tabbed Dialog Interface), not SDI or MDI.
// * Save images onto disk, instead of ask user if they want to save it. Instead just open them on app start.
// * Able to change color.
// * Add line drawing tool.
// * Add circle/ellipse drawing tool.
// * Add square/rectangle drawing tool.
// * Add status bar with mouse x and y.
// * Add palette bar, 5 colors. But could be up to 12 (or 16).
// * Add fill bucket tool.
// * Add select rectangle tool.
// * Add copy/paste/cut
// * Add get-color (droplet) tool
// *

// BUGS:
// * ...
// * ...

namespace GemPaint
{
    public partial class MainForm : Form
    {
        GemProject gemProject;
        List<GemImage> gemImages = new List<GemImage>();

        // Project files tree view
        TreeNode topNode;
        List<TreeNode> filesNode = new List<TreeNode>();

        //Bitmap bitmap;
        SolidBrush sb;
        Pen pen;

        bool canDraw = false;
        float zoomLevel = 1.0F;
        int oldTrackBarValue = 1;
        int px = 0;
        int py = 0;

        // Preview
        bool canPreview = false;
        int previewW = 0;
        int previewH = 0;
        int preview_max_size = 256;
        int preview_cur_width = 0;
        int preview_cur_height = 0;
        float preview_scale = 1.0F;
        float ratio = 1.0F;



        public MainForm()
        {
            InitializeComponent();
            this.pictureBoxDraw.MouseWheel += Zoom_MouseWheel;
            this.pictureBoxPreview.MouseWheel += Zoom_MouseWheel;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadProject();
        }

        private void pictureBoxDraw_Paint(object sender, PaintEventArgs e)
        {
            int i = treeViewProjectFiles.SelectedNode.Index;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            e.Graphics.ScaleTransform(zoomLevel, zoomLevel);

            float x = 0.0F;
            float y = 0.0F;
            float w = 0.0F;
            float h = 0.0F;
            float imgW = gemImages[i].width;
            float imgH = gemImages[i].height;
            int appH = this.ClientSize.Height - 48;

            float paw = 256.0F;
            float paw_2 = 256.0F / 2.0F;
            float paw_ua = Math.Abs(imgW - imgH);
            float paw_u = (paw_ua/2) / zoomLevel;

            w = imgW;
            h = imgH;
            //int pw = Math.Min((int)w, appH);
            //int ph = Math.Min((int)h, appH);
            int pw = Math.Min((int)w * (int)zoomLevel, appH);
            int ph = Math.Min((int)h * (int)zoomLevel, appH);
            
            pictureBoxDraw.Width = pw; // * (int)zoomLevel;
            pictureBoxDraw.Height = ph; // * (int)zoomLevel; //675


            if (ratio < 1)
            {
                float t_ratio = imgH / imgW;
                float tx = paw / t_ratio;
                x = (((tx/2) / (float)zoomLevel - ((float)px)-0.5F) / tx) * imgW;
                y = ((((paw_2 * t_ratio) / (float)zoomLevel - (float)py) / paw) * imgW - paw_u) * t_ratio;
            }
            else
            {
                x = ((paw_2 / (float)zoomLevel - (float)px) / paw) * imgW;
                y = ((paw_2 / (float)zoomLevel - (float)py) / paw) * imgW - paw_u;
            }

            //int pw = Math.Min((int)w, appH);
            //int ph = Math.Min((int)h, appH);

            //pictureBoxDraw.Width = pw;
            //pictureBoxDraw.Height = ph; //675







            this.Text = x.ToString();


            e.Graphics.DrawImage(gemImages[i].bitmap, (int)x, (int)y, (int)w, (int)h);
        }

        private void pictureBoxDraw_MouseDown(object sender, MouseEventArgs e)
        {
            canDraw = true;
        }

        private void pictureBoxDraw_MouseUp(object sender, MouseEventArgs e)
        {
            canDraw = false;
        }

        private void pictureBoxDraw_MouseMove(object sender, MouseEventArgs e)
        {
            if (canDraw)
            {
                int i = treeViewProjectFiles.SelectedNode.Index;

                //int x = (px - previewW) / (2 * preview_max_size / gemImages[i].width) + (e.X / (int)zoomLevel);
                //int y = (py - previewH) / (2 * preview_max_size / gemImages[i].width) + (e.Y / (int)zoomLevel);

                int x = (int)(float)((float)(px - previewW) / (float)(2 * preview_max_size / gemImages[i].width) + (e.X / (int)zoomLevel));
                int y = (int)(float)((float)(py - previewH) / (float)(2 * preview_max_size / gemImages[i].width) + (e.Y / (int)zoomLevel));



                if (x < 0) x = 0;
                if (y < 0) y = 0;
                if (x >= gemImages[i].width) x = gemImages[i].width - 1;
                if (y >= gemImages[i].height) y = gemImages[i].height - 1;

                if (canDraw)
                {
                    gemImages[i].bitmap.SetPixel(x, y, Color.Blue);
                }

                InvalidateAll();
            }
        }

        private void pictureBoxDraw_MouseHover(object sender, EventArgs e)
        {
            //pictureBoxDraw.Focus();
        }

        private void Zoom_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;//disable default mouse wheel
            if (e.Delta > 0)
            {
                if (trackBarZoom.Value < trackBarZoom.Maximum)
                {
                    trackBarZoom.Value++;
                }
            }
            else
            {
                if (trackBarZoom.Value > trackBarZoom.Minimum)
                {
                    trackBarZoom.Value--;
                }
            }

            TranslateZoomLevel(trackBarZoom.Value);

            UpdateZoom();
        }

        private void TranslateZoomLevel(int trackBarValue)
        {
            zoomLevel = (float)Math.Pow(2, trackBarValue-1);
        }

        private void trackBarZoom_Scroll(object sender, EventArgs e)
        {
            /*
            if (trackBarZoom.Value < trackBarZoom.Maximum)
            {
                trackBarZoom.Value++;
            }
            if (trackBarZoom.Value > trackBarZoom.Minimum)
            {
                trackBarZoom.Value--;
            }
            */


            if (oldTrackBarValue < trackBarZoom.Value)
            {
                if (trackBarZoom.Value < trackBarZoom.Maximum)
                {
                    oldTrackBarValue++;
                    if (oldTrackBarValue > trackBarZoom.Maximum)
                        oldTrackBarValue = trackBarZoom.Maximum;
                    trackBarZoom.Value = oldTrackBarValue;
                }
            }
            else if (oldTrackBarValue > trackBarZoom.Value)
            {
                if (trackBarZoom.Value >= trackBarZoom.Minimum)
                {
                    oldTrackBarValue--;
                    if (oldTrackBarValue < 1)
                        oldTrackBarValue = 1;
                    trackBarZoom.Value = oldTrackBarValue;
                }
            }
     

            TranslateZoomLevel(trackBarZoom.Value);

            UpdateZoom();

            /*
            float oldZoomLevel = zoomLevel;

            TranslateZoomLevel(trackBarZoom.Value);

            DeterminePreviewProportion();

            previewW = preview_cur_width / (int)zoomLevel;
            previewH = preview_cur_height / (int)zoomLevel;

            InvalidateAll();
            */
        }

        private void UpdateZoom()
        {
            //oldZoomLevel = zoomLevel;
            oldTrackBarValue = trackBarZoom.Value;

            //TranslateZoomLevel(trackBarZoom.Value);

            DeterminePreviewProportion();

            previewW = preview_cur_width / (int)zoomLevel;
            previewH = preview_cur_height / (int)zoomLevel;

            InvalidateAll();
        }

        private void pictureBoxPreview_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

            DeterminePreviewProportion();

            //e.Graphics.ScaleTransform(1 / preview_scale, 1 / (preview_scale));
            //e.Graphics.DrawImage(gemImages[treeViewProjectFiles.SelectedNode.Index].bitmap, 0, 0, preview_cur_width*2, preview_cur_height*2);
            e.Graphics.DrawImage(gemImages[treeViewProjectFiles.SelectedNode.Index].bitmap, 0, 0, preview_cur_width, preview_cur_height);
            //e.Graphics.DrawImage(gemImages[treeViewProjectFiles.SelectedNode.Index].bitmap, 0, 0);
            e.Graphics.ResetTransform();
            //e.Graphics.DrawRectangle(pen, (px - previewW) / 2, (py - previewH) / 2, previewW, previewH);
            //e.Graphics.DrawRectangle(pen, (px - previewW) / 2, (py - previewH) / 2, previewW, previewH);
            e.Graphics.DrawRectangle(pen, px - previewW / 2, py - previewH / 2, previewW, previewH);
        }

        private void pictureBoxPreview_MouseDown(object sender, MouseEventArgs e)
        {
            canPreview = true;

            px = e.X;
            py = e.Y;

            InvalidateAll();
        }

        private void pictureBoxPreview_MouseUp(object sender, MouseEventArgs e)
        {
            canPreview = false;
        }

        private void pictureBoxPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (canPreview)
            {
                px = e.X;
                py = e.Y;

                InvalidateAll();
            }
        }
        private void pictureBoxPreview_MouseHover(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void InvalidateAll()
        {
            pictureBoxDraw.Invalidate();
            pictureBoxPreview.Invalidate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //

            //
            if (sb != null) sb.Dispose();
            if (pen != null) pen.Dispose();

            gemProject.gemProjectAllFiles.Add("NoName.png");
            gemImages.Add(new GemImage(256, 256));

            // Clear tree
            treeViewProjectFiles.Nodes.Clear();

            TreeNode node = new TreeNode("NoName.png");
            filesNode.Add(node);

            topNode = new TreeNode(gemProject.gemProjectName, filesNode.ToArray());

            // Apply it
            treeViewProjectFiles.Nodes.Add(topNode);
            treeViewProjectFiles.ExpandAll();
            treeViewProjectFiles.SelectedNode = node;
            treeViewProjectFiles.Select();

            sb = new SolidBrush(Color.Blue);
            pen = new Pen(new SolidBrush(Color.Black));

            // Drawing
            canDraw = false;

            // Zoom
            zoomLevel = 1.0F;
            trackBarZoom.Value = 1;

            // Preview
            canPreview = false;

            int i = treeViewProjectFiles.SelectedNode.Index;

            DeterminePreviewProportion();

            SetPreviewRect(preview_cur_width, preview_cur_height);

            /*
            previewW = gemImages[i].width / (int)zoomLevel;
            previewH = gemImages[i].height / (int)zoomLevel;
            px = previewW;
            py = previewH;
            */

            InvalidateAll();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            /*
            bitmap.Dispose();
            bitmap = new Bitmap("Test.png");

            PaintEngine_ResetAll();
            */
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //

            /*
            // Save it temporary in memory
            Bitmap bt = new Bitmap(bitmap);

            try
            {
                // Release the image
                bitmap.Dispose();

                // File exists? --> Delete if so
                if (System.IO.File.Exists("Test.png"))
                {
                    System.IO.File.Delete("Test.png");
                }

                // Save image as a png-file
                bt.Save("Test.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                // Save the png to a temporary file
                bt.Save("Temp-" + DateTime.Now.Ticks.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }

            bt.Dispose();


            // Re-Load image
            bitmap = new Bitmap("Test.png");
            */
        }

        private void closeStripMenuItem_Click(object sender, EventArgs e)
        {
            //

            // Clear the project files tree view
            treeViewProjectFiles.Nodes.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            Application.Exit();
        }

        private void LoadProject()
        {
            // Load project file (GEM file)
            gemProject = new GemProject("TestProject.gem");
            gemProject.Load();

            // Update caption
            this.Text = gemProject.gemProjectName + " - " + gemProject.gemProjectAllFiles[0];

            // Clear tree
            treeViewProjectFiles.Nodes.Clear();

            for (int i = 0; i < gemProject.gemProjectAllFiles.Count; ++i)
            {
                // Load image
                gemImages.Add(new GemImage(gemProject.gemProjectAllFiles[i]));

                // Add to tree view
                TreeNode node = new TreeNode(gemProject.gemProjectAllFiles[i]);
                filesNode.Add(node);
            }
            topNode = new TreeNode(gemProject.gemProjectName, filesNode.ToArray());

            // Apply it
            treeViewProjectFiles.Nodes.Add(topNode);
            treeViewProjectFiles.ExpandAll();
            treeViewProjectFiles.SelectedNode = filesNode[0];
            treeViewProjectFiles.Select();

            sb = new SolidBrush(Color.Blue);
            pen = new Pen(new SolidBrush(Color.Black));

            pictureBoxDraw.Width = gemImages[0].width;
            pictureBoxDraw.Height = gemImages[0].height;

            // Drawing
            canDraw = false;

            // Zoom
            zoomLevel = 1.0F;
            trackBarZoom.Value = 1;

            // Preview
            canPreview = false;

            DeterminePreviewProportion();

            SetPreviewRect(preview_cur_width, preview_cur_height);

            InvalidateAll();
        }

        private void SaveProject()
        {
        }

        private void SetPreviewRect(int rx, int ry)
        {
            previewW = rx;
            previewH = ry;
            px = rx/2;
            py = ry/2;
        }

        private void treeViewProjectFiles_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Update caption
            this.Text = gemProject.gemProjectName + " - " + gemProject.gemProjectAllFiles.ElementAt(e.Node.Index);

            pictureBoxDraw.Width = gemImages[e.Node.Index].width;
            pictureBoxDraw.Height = gemImages[e.Node.Index].height;

            DeterminePreviewProportion();

            SetPreviewRect(preview_cur_width, preview_cur_height);

            // Drawing
            canDraw = false;
            zoomLevel = 1.0F;
            trackBarZoom.Value = 1;

            InvalidateAll();
        }

        private void DeterminePreviewProportion()
        {
            int i = treeViewProjectFiles.SelectedNode.Index;

            // Determine size and proportion of the preview image
            //ratio = (float)((float)gemImages[i].width / (float)gemImages[i].height);
            ratio = (float)((float)gemImages[i].width / (float)gemImages[i].height);

            if (gemImages[i].width > gemImages[i].height)
            {
                preview_cur_width = preview_max_size;
                preview_cur_height = (int)((float)(preview_max_size / ratio));

                preview_scale = gemImages[i].width / preview_max_size;
            }
            else
            {
                preview_cur_width = (int)((float)(preview_max_size * ratio));
                preview_cur_height = preview_max_size;

                preview_scale = gemImages[i].height / preview_max_size;
            }
            pictureBoxPreview.Width = preview_cur_width;
            pictureBoxPreview.Height = preview_cur_height;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            InvalidateAll();
        }

    }
}
