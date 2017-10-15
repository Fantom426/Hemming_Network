using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Hemming
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        double[,] files;
        double[,] error_files;
        double[,] W;


        public double[,] Load_img(string[] directory)
        {
            Bitmap img = new Bitmap(directory[0]);
            /*using (Graphics g = Graphics.FromImage(img))
                g.Clear(Color.White);*/
            double[,] file = new double[directory.Length, img.Width * img.Height];
            for(int k = 0; k < directory.Length; k++)
            {
               
                int n = 0;
                for (int i = 0; i < img.Width; i++)
                    for (int j = 0; j < img.Height; j++)
                    {
                        if (img.GetPixel(i, j) == Color.FromArgb(0, 0, 0))
                        {
                            file[k, n] = 1;
                            
                        }
                        else
                        {
                            file[k, n] = 0;
                            
                        }
                        n++;
                        
                    }
            }
            //pictureBox1.Image = img;
            return file;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            string[] dir_error_file = Directory.GetFiles(@"C:\NewralNetwork\Hemming\Error");
            error_files = Load_img(dir_error_file);
            string[] dir_file = Directory.GetFiles(@"C:\NewralNetwork\Hemming\Example");
            files = Load_img(dir_file);
            
            W = new double[files.GetLength(0), files.GetLength(1)];
            set_combo(dir_error_file);
        }

        public void init_W()
        {
            for (int i = 0; i < W.GetLength(0); i++)
                for (int j = 0; j < W.GetLength(1); j++)
                    W[i, j] = files[i, j];
        }

        private void button2_Click(object sender, EventArgs e)
        {
            init_W();
        }

        private double[] classif(double[] x_new)
        {
            double[] y = first_layer(x_new);
            double[] y_new = second_layer(y);
            int t = 0;
            double len = length_hem(y, y_new);
            while (t < 500 && length_hem(y, y_new) > 0)
            {

                y = y_new;
                y_new = second_layer(y);
                t++;
            }
            int g = t;
            return y_new;
        }

        private double[] first_layer(double[] x_new)
        {
            double[] y = new double[files.GetLength(0)];   
            for (int i = 0; i < files.GetLength(0); i++)
            {
                double temp = 0;
                for (int j = 0; j < files.GetLength(1); j++)
                {
                    temp += W[i, j] * x_new[i];
                }
                y[i] = temp + files.GetLength(1) / 2.0;

            }
            return y;
        }

        private double[] second_layer(double[] y_old)
        {
            double[] y = new double[files.GetLength(0)];
            double eps = 1.0 / files.GetLength(0);
            for (int j = 0; j < y.Length; j++)
            {
                double temp = 0;
                for (int i = 0; i < y.Length; i++)
                {
                    if (i != j)
                        temp += y_old[i];
                }
                temp *= eps;
                y[j] = f(y_old[j] - temp, 200);
            }
            return y;
        }

        double f(double x, int N = 100)
        {
            if (x < 0) return 0;
            else if (x > 0 && x < N)
                return x;
            else return N;
        }

        private double length_hem(double[] y, double[] d)
        {
            double temp = 0;
            for (int i = 0; i < y.Length; i++)
            {
                if (y[i] != d[i])
                    temp += 1;
            }
            return temp;
        }

        private void set_combo(string[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                string s = a[i].Substring(a[i].LastIndexOf('\\') + 1);
                comboBox1.Items.Add(s);
            }
        }

        private double[] arr_from_matr(double[,] y, int t)
        {
            double[] temp = new double[y.GetLength(1)];
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = y[t, i];

            }
            return temp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            double[] result = classif(arr_from_matr(error_files, comboBox1.SelectedIndex));
            
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == result.Max())
                    textBox1.Text += i.ToString() + " , ";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string dir = @"C:\NewralNetwork\Hemming\Error\" + (comboBox1.SelectedIndex + 1) + ".bmp";
            Bitmap img = new Bitmap(pictureBox1.Width,pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(img))
                g.Clear(Color.White);
            pictureBox1.Image = img;
            int step = 20;   // порог
            img = new Bitmap(dir);
            int average = (int)(pictureBox1.BackColor.R + pictureBox1.BackColor.G + pictureBox1.BackColor.B) / 3;
            double[,] result = new double[img.Width, img.Height];
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);
                    int average2 = (int)(pixel.R + pixel.G + pixel.B) / 3;
                    if (average2 >= average - step && average2 <= average + step)
                    {
                        img.SetPixel(i, j, Color.White);
                        result[i, j] = 0;
                    }
                    else
                    {
                        img.SetPixel(i, j, Color.Black);
                        result[i, j] = 1;
                    }
                }
            pictureBox1.Image = img;
        }
    }
}
