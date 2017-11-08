using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bezier_splines
{
    public partial class Form1 : Form
    {

        Graphics g;
        Bitmap mainBitmap;
        bezier_splines bs;
        List<Point> original_pts = new List<Point>();

        class bezier_splines
        {
            private Graphics g;
            private Pen segs_pen;
            private Pen splines_pen;
            private PictureBox pb;

            private List<Point> lp;

            public bezier_splines(Graphics g, PictureBox pb)
            {
                lp = new List<Point>();
                segs_pen = new Pen(Color.Blue);
                splines_pen = new Pen(Color.Red);
                this.g = g;
                this.pb = pb;
            }

            public void add_point(Point p)
            {
                lp.Add(p);
            }

            public int cnt_points()
            {
                return lp.Count;
            }

            private void fourpts_spline(Point p0, Point p1, Point p2, Point p3)
            {
                g.DrawLine(segs_pen, p0, p1);
                g.DrawLine(segs_pen, p1, p2);
                g.DrawLine(segs_pen, p2, p3);

                Point last_spline_p = p0;
                for (double t = 0; t <= 1; t += 0.005)
                {
                    double p = 1 - t;

                    double res_x = p0.X * p * p * p + 3 * p1.X * p * p * t + 3 * p2.X * p * t * t + p3.X * t * t * t;
                    double res_y = p0.Y * p * p * p + 3 * p1.Y * p * p * t + 3 * p2.Y * p * t * t + p3.Y * t * t * t;

                    // соединяем последнюю точку сплайна и новую
                    g.DrawLine(splines_pen, last_spline_p, new Point((int)res_x, (int)res_y));
                    last_spline_p.X = (int)res_x;
                    last_spline_p.Y = (int)res_y;
                }
            }

            private Point segment_center(Point p1, Point p2)
            {
                double x, y;
                if (p1.X != p2.X)
                {
                    x = (p1.X + p2.X) / 2;
                    y = (p2.X * p1.Y - p1.X * p2.Y - x * (p1.Y - p2.Y)) / (p2.X - p1.X);
                }
                else
                {
                    x = p1.X;
                    y = (p1.Y + p2.Y) / 2;
                }
                return new Point((int)x, (int)y);
            }

            public void build(List<Point> origin_pts, int listBox_ind)
            {
                lp.Clear();
                for (int i = 0; i < origin_pts.Count; i++)
                    lp.Add(origin_pts[i]);

                if (lp.Count < 4)
					return;
				else if (lp.Count == 5) // (lp.Count - 4) % 3 == 0 — исключения
				{
					lp.Insert(3, segment_center(lp[2], lp[3]));
					lp.Insert(5, segment_center(lp[4], lp[5]));
					fourpts_spline(lp[0], lp[1], lp[2], lp[3]);
					fourpts_spline(lp[3], lp[4], lp[5], lp[6]);
                    for (int i = 0; i < origin_pts.Count; i++)
                        g.FillEllipse(new SolidBrush(Color.Black), origin_pts[i].X - 5, origin_pts[i].Y - 5, 10, 10);
                    return;
				}
				else if (lp.Count == 6) // (lp.Count - 4) % 3 == 0 — исключения
                {
					lp.Insert(3, segment_center(lp[2], lp[3]));
					fourpts_spline(lp[0], lp[1], lp[2], lp[3]);
					fourpts_spline(lp[3], lp[4], lp[5], lp[6]);
                    for (int i = 0; i < origin_pts.Count; i++)
                        g.FillEllipse(new SolidBrush(Color.Black), origin_pts[i].X - 5, origin_pts[i].Y - 5, 10, 10);
                    return;
				}
				else if ((lp.Count - 4) % 3 == 1)
				{
                    lp.Insert(lp.Count - 4, segment_center(lp[lp.Count - 4], lp[lp.Count - 5]));
                    lp.Insert(lp.Count - 1, segment_center(lp[lp.Count - 1], lp[lp.Count - 2]));
                }
                else if ((lp.Count - 4) % 3 == 2)
                    lp.Insert(lp.Count - 1, segment_center(lp[lp.Count - 1], lp[lp.Count - 2]));

                // (lp.Count - 4) % 3 == 0
                for (int i = 0; i < lp.Count - 1; i += 3)
					fourpts_spline(lp[i], lp[i + 1], lp[i + 2], lp[i + 3]);

                for (int k = 0; k < origin_pts.Count; k++)
                    if (listBox_ind == k)
                        g.FillEllipse(new SolidBrush(Color.YellowGreen), origin_pts[k].X - 5, origin_pts[k].Y - 5, 10, 10);
                    else
                        g.FillEllipse(new SolidBrush(Color.Black), origin_pts[k].X - 5, origin_pts[k].Y - 5, 10, 10);
            }

            public void clear()
            {
                lp.Clear();
            }

        }

        public Form1()
        {
            InitializeComponent();

            mainBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(mainBitmap);
            g.Clear(Color.White);
            pictureBox1.Image = mainBitmap;

            bs = new bezier_splines(g, pictureBox1);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            bs.add_point(e.Location);
            original_pts.Add(e.Location);
            listBox1.Items.Add("Точка " + original_pts.Count.ToString());
            if (bs.cnt_points() > 3)
            {
                g.Clear(Color.White);
                bs.build(original_pts, listBox1.SelectedIndex);

                pictureBox1.Invalidate();
            }
            listBox1.SelectedIndex = -1;
            groupBox1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            bs.clear();
            original_pts.Clear();
            listBox1.Items.Clear();
            pictureBox1.Invalidate();
            groupBox1.Visible = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = listBox1.SelectedIndex;
            for (int k = 0; k < original_pts.Count; k++)
                if (i == k)
                    g.FillEllipse(new SolidBrush(Color.YellowGreen), original_pts[k].X - 5, original_pts[k].Y - 5, 10, 10);
                else
                    g.FillEllipse(new SolidBrush(Color.Black), original_pts[k].X - 5, original_pts[k].Y - 5, 10, 10);

            pictureBox1.Invalidate();

            if (i != -1)
                groupBox1.Visible = true;

            textBox1.Clear();
            textBox2.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            Point p = original_pts[listBox1.SelectedIndex];
            original_pts.Remove(p);
            listBox1.Items.Clear();
            for (int i = 1; i <= original_pts.Count; i++)
                listBox1.Items.Add("Точка " + i.ToString());

            g.Clear(Color.White);
            bs.build(original_pts, listBox1.SelectedIndex);
            pictureBox1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int x, y;
            if (int.TryParse(textBox1.Text, out x) && int.TryParse(textBox2.Text, out y))
            {
                int i = listBox1.SelectedIndex;
                Point p = original_pts[i];
                p.X += x;
                p.Y += y;
                original_pts[i] = p;

                g.Clear(Color.White);
                bs.build(original_pts, listBox1.SelectedIndex);

                pictureBox1.Invalidate();
            }
        }
    }
}
