using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YuragImageProcessing
{
    public partial class Form1 : Form
    {
        Bitmap loaded;

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            waitLabel.Visible = false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    loaded = new Bitmap(openFileDialog.FileName);
                    pictureBox2.Image = loaded;
                    pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }


        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                pictureBox1.Image = new Bitmap(pictureBox2.Image);

                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                waitLabel.Visible = true;

                Task.Run(() =>
                {
                    Bitmap originalImage = new Bitmap(pictureBox2.Image);
                    Bitmap grayscaleImage = new Bitmap(originalImage.Width, originalImage.Height);

                for (int x = 0; x < originalImage.Width; x++)
                {
                    for (int y = 0; y < originalImage.Height; y++)
                    {
                        Color pixelColor = originalImage.GetPixel(x, y);
                        int averageColor = (int)(pixelColor.R * 0.3 + pixelColor.G * 0.59 + pixelColor.B * 0.11);
                        Color newColor = Color.FromArgb(averageColor, averageColor, averageColor);
                        grayscaleImage.SetPixel(x, y, newColor);
                    }
                }
                    this.Invoke((MethodInvoker)delegate
                    {
                        waitLabel.Visible = false;

                        pictureBox1.Image = new Bitmap(grayscaleImage);
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    });
                });
            }
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                waitLabel.Visible = true;

                Task.Run(() =>
                {
                    Bitmap originalImage = new Bitmap(pictureBox2.Image);
                    Bitmap invertedImage = new Bitmap(originalImage.Width, originalImage.Height);

                for (int x = 0; x < originalImage.Width; x++)
                {
                    for (int y = 0; y < originalImage.Height; y++)
                    {
                        Color pixelColor = originalImage.GetPixel(x, y);
                        Color invertedColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                        invertedImage.SetPixel(x, y, invertedColor);
                    }
                }
                    this.Invoke((MethodInvoker)delegate
                    {
                        waitLabel.Visible = false;
                        pictureBox1.Image = new Bitmap(invertedImage);
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    });
                });
            }
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DrawHistogram(pictureBox1);
        }
        private void DrawHistogram(PictureBox pictureBox)
        {
            if (loaded != null)
            {
                waitLabel.Visible = true;
                Bitmap loadedCopy = new Bitmap(loaded);

                Task.Run(() =>
                {
                    int[] histogram = new int[256];

                    for (int x = 0; x < loadedCopy.Width; x++)
                    {
                        for (int y = 0; y < loadedCopy.Height; y++)
                        {
                            Color pixel = loadedCopy.GetPixel(x, y);
                            int grey = (byte)((pixel.R + pixel.G + pixel.B) / 3);
                            histogram[grey]++;
                        }
                    }

                    int maxCount = histogram.Max();
                    List<int> normalizedHistogram = histogram.Select(value => (int)(value * 100.0 / maxCount)).ToList();

                    this.Invoke((MethodInvoker)delegate
                    {
                        waitLabel.Visible = false;

                        DrawHistogramOnPictureBox(normalizedHistogram, pictureBox1);
                    });
                });
            }
        }
        private void DrawHistogramOnPictureBox(List<int> histogram, PictureBox pictureBox)
        {
            Bitmap histogramBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (Graphics g = Graphics.FromImage(histogramBitmap))
            {
                g.Clear(Color.White);

                int barWidth = pictureBox.Width / histogram.Count;

                for (int i = 0; i < histogram.Count; i++)
                {
                    int barHeight = histogram[i] * 3;
                    g.FillRectangle(Brushes.Black, i * barWidth, pictureBox.Height - barHeight, barWidth, barHeight);
                }
            }

            pictureBox.Image = histogramBitmap;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                waitLabel.Visible = true;

                Task.Run(() =>
                {
                    Bitmap loaded = new Bitmap(pictureBox2.Image);
                    Color pixel;
                    Bitmap processed = new Bitmap(loaded.Width, loaded.Height);

                    for (int x = 0; x < loaded.Width; x++)
                    {
                        for (int y = 0; y < loaded.Height; y++)
                        {
                            pixel = loaded.GetPixel(x, y);
                            int tr = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                            int tg = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                            int tb = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);

                            tr = Math.Min(255, Math.Max(0, tr));
                            tg = Math.Min(255, Math.Max(0, tg));
                            tb = Math.Min(255, Math.Max(0, tb));

                            processed.SetPixel(x, y, Color.FromArgb(tr, tg, tb));
                        }
                    }

                    this.Invoke((MethodInvoker)delegate
                    {
                        waitLabel.Visible = false;
                        pictureBox1.Image = processed;
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    });
                });
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Bitmap Image|*.bmp|JPEG Image|*.jpg|PNG Image|*.png";
                    saveFileDialog.Title = "Save Processed Image";
                    saveFileDialog.ShowDialog();

                    if (saveFileDialog.FileName != "")
                    {
                        
                        ImageFormat imageFormat = ImageFormat.Bmp; 
                        string extension = Path.GetExtension(saveFileDialog.FileName);

                        if (string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase))
                        {
                            imageFormat = ImageFormat.Jpeg;
                        }
                        else if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
                        {
                            imageFormat = ImageFormat.Png;
                        }

                        
                        pictureBox1.Image.Save(saveFileDialog.FileName, imageFormat);
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
