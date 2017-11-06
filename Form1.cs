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

        class bezier_splines
        {
            private Graphics g;
            private Pen segs_pen;
            private Pen splines_pen;
            private PictureBox pb;

            private List<Point> lp;

            public bezier_splines(Graphics g, PictureBox pb, List<Point> lp)
            {
                this.lp = lp;

                segs_pen = new Pen(Color.Blue);
                splines_pen = new Pen(Color.Red);
                this.g = g;
                this.pb = pb;
            }

            public void add_point(Point p)
            {
                lp.Add(p);
            }

            //private void twopts_spline()
            //{
            //    g.DrawLine(splines_pen, lp[0], lp[1]);
            //}

            //private void threepts_spline()
            //{
            //    g.DrawLine(segs_pen, lp[0], lp[1]);
            //    g.DrawLine(segs_pen, lp[1], lp[2]);

            //    Point last_spline_p = lp[0];
            //    for (double t = 0; t <= 1; t += 0.005)
            //    {
            //        double Q1x = lp[0].X * (1 - t) + lp[1].X * t;
            //        double Q1y = lp[0].Y * (1 - t) + lp[1].Y * t;
            //        double Q2x = lp[1].X * (1 - t) + lp[2].X * t;
            //        double Q2y = lp[1].Y * (1 - t) + lp[2].Y * t;
            //        double res_x = Q1x * (1 - t) + Q2x * t;
            //        double res_y = Q1y * (1 - t) + Q2y * t;

            //        // соединяем последнюю точку сплайна и новую
            //        g.DrawLine(splines_pen, last_spline_p, new Point((int)res_x, (int)res_y));
            //        last_spline_p.X = (int)res_x;
            //        last_spline_p.Y = (int)res_y;
            //    }
            //}

            private void fourpts_spline(Point p0, Point p1, Point p2, Point p3)
            {
                //g.DrawLine(segs_pen, lp[0], lp[1]);
                //g.DrawLine(segs_pen, lp[1], lp[2]);
                //g.DrawLine(segs_pen, lp[2], lp[3]);

                g.DrawLine(segs_pen, p0, p1);
                g.DrawLine(segs_pen, p1, p2);
                g.DrawLine(segs_pen, p2, p3);

                //Point last_spline_p = lp[0];
                Point last_spline_p = p0;
                for (double t = 0; t <= 1; t += 0.005)
                {
                    double p = 1 - t;
                    //double res_x = lp[0].X * Math.Pow(p, 3) + 3 * lp[1].X * p * p * t + 3 * lp[2].X * p * t * t + lp[3].X * t * t * t;
                    //double res_y = lp[0].Y * Math.Pow(p, 3) + 3 * lp[1].Y * p * p * t + 3 * lp[2].Y * p * t * t + lp[3].Y * t * t * t;

                    double res_x = p0.X * Math.Pow(p, 3) + 3 * p1.X * p * p * t + 3 * p2.X * p * t * t + p3.X * t * t * t;
                    double res_y = p0.Y * Math.Pow(p, 3) + 3 * p1.Y * p * p * t + 3 * p2.Y * p * t * t + p3.Y * t * t * t;

                    // соединяем последнюю точку сплайна и новую
                    g.DrawLine(splines_pen, last_spline_p, new Point((int)res_x, (int)res_y));
                    last_spline_p.X = (int)res_x;
                    last_spline_p.Y = (int)res_y;
                }
            }

            private Point segment_center(Point p1, Point p2)
            {
                double x = (p1.X + p2.X) / 2;
                double y = (p2.X * p1.Y - p1.X * p2.Y - x * (p1.Y - p2.Y)) / (p2.X - p1.X);
                return new Point((int)x, (int)y);
            }

            public void build()
            {
				if (lp.Count < 4)
					return;
				else if (lp.Count == 5)// || lp.Count == 6) // (lp.Count - 4) % 3 == 0 — исключения
				{
					lp.Insert(3, segment_center(lp[2], lp[3]));
					lp.Insert(5, segment_center(lp[4], lp[5]));
					fourpts_spline(lp[0], lp[1], lp[2], lp[3]);
					fourpts_spline(lp[3], lp[4], lp[5], lp[6]);
					return;
				}
				else if (lp.Count == 6)
				{
					lp.Insert(3, segment_center(lp[2], lp[3]));
					fourpts_spline(lp[0], lp[1], lp[2], lp[3]);
					fourpts_spline(lp[3], lp[4], lp[5], lp[6]);
					return;
				}
				else if ((lp.Count - 4) % 3 == 1)
				{
					lp.Insert(3, segment_center(lp[2], lp[3]));
					lp.Insert(6, segment_center(lp[5], lp[6]));
				}
				else if ((lp.Count - 4) % 3 == 2)
				{
					lp.Insert(3, segment_center(lp[2], lp[3]));					
				}

				// (lp.Count - 4) % 3 == 0
				for (int i = 0; i < lp.Count - 1; i += 3)
				{
					fourpts_spline(lp[i], lp[i + 1], lp[i + 2], lp[i + 3]);
				}
            }

        }

        public Form1()
        {
            InitializeComponent();

            mainBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(mainBitmap);
            g.Clear(Color.White);
            pictureBox1.Image = mainBitmap;

            bs = new bezier_splines(g, pictureBox1, new List<Point>());
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            bs.add_point(e.Location);

			g.FillEllipse(new SolidBrush(Color.Crimson), e.X - 5, e.Y - 5, 10, 10);
			g.Clear(Color.White);

            pictureBox1.Invalidate();

        }

        private void button1_Click(object sender, EventArgs e)
        {			
            bs.build();
            pictureBox1.Invalidate();
        }
    }
}
