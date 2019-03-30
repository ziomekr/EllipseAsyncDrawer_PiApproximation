using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Circumference
{

    public partial class Form1 : Form
    {
        List<ellipse> active = new List<ellipse>();
        List<ellipse> ellipses = new List<ellipse>();
        double maxA, maxB;
        BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        string toAdd;
        double totalCircumference = 0;
        double piApprox = 0;
        static List<Color> colors = new List<Color>();
        static Random rand = new Random();
        public string ToAdd { get => toAdd; set => toAdd = value; }
        
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height);
            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                g.Clear(Color.White);
            }
            getColors();
            ellipses.Add(new ellipse(2, 1));
            InitializeBackgroundWorker();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void getColors()
        {
            foreach (var colorValue in Enum.GetValues(typeof(KnownColor)))
            {
                Color color = Color.FromKnownColor((KnownColor)colorValue);
                colors.Add(color);
            }
        }
        private void InitializeBackgroundWorker()
        {
            backgroundWorker1.DoWork +=
                new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.WorkerSupportsCancellation = true;

        }

        private void calculatePoints(ellipse e)
        {
            List<eliPoint> p = e.Points;
            if (p.Count == 0)
            {
                p.Add(new eliPoint(-e.A, 0));
                p.Add(new eliPoint(0, e.B));
                p.Add(new eliPoint(e.A, 0));
                p.Add(new eliPoint(0, -e.B));
            }

            else
            {
                if (e.B == 0 || e.A == 0)
                    return;
                int limit = p.Count / 2;
                for (int i = 0; i < limit; i++)
                {
                    double x = (p[i * 2].X + p[i * 2 + 1].X) / 2;
                    double y = e.B / e.A * Math.Sqrt(e.A * e.A - x * x);
                    p.Insert(i * 2 + 1, new eliPoint(x, y));
                    p.Insert(p.Count - i * 2, new eliPoint(x, -y));
                }
            }

        }

        private void moveToActive()
        {
            active.Clear();
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                    active.Add(ellipses[i]);
            }
        }

        private void calculatePoints(ellipse e, List<eliPoint> p)
        {

            if (p.Count == 0)
            {
                p.Add(new eliPoint(-e.A, 0));
                p.Add(new eliPoint(0, e.B));
                p.Add(new eliPoint(e.A, 0));
                p.Add(new eliPoint(0, -e.B));
            }

            else
            {
                if (e.B == 0 || e.A == 0)
                    return;
                int limit = p.Count / 2;
                for (int i = 0; i < limit; i++)
                {
                    double x = (p[i * 2].X + p[i * 2 + 1].X) / 2;
                    double y = e.B / e.A * Math.Sqrt(e.A * e.A - x * x);
                    p.Insert(i * 2 + 1, new eliPoint(x, y));
                    p.Insert(p.Count - i * 2, new eliPoint(x, -y));
                }
            }

        }

        private List<Point> transformPoints(List<eliPoint> p)
        {
            List<Point> transformed = new List<Point>();
            foreach (eliPoint e in p)
            {
                double x, y;
                if (maxA == 0 || maxB == 0)
                {
                    x = e.X * 512 + 512;
                    y = e.Y * 256 + 256;
                }
                else
                {
                    x = e.X / maxA * 512 + 512;
                    y = e.Y / maxB * 256 + 256;
                }

                transformed.Add(new Point((int)x, (int)y));
            }
            return transformed;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (progressBar1.Value != 0)
                Thread.Sleep(1000);
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            Pen pen = new Pen(Color.Red, 1F);
            
            while (!backgroundWorker1.CancellationPending)
            {
                if (progressBar1.Value == 100)
                    return;
                totalCircumference = 0;
                piApprox = 0;
                g.Clear(Color.White);
                g.DrawLine(new Pen(Color.Black, 2F), 0, 256, 1024, 256);
                foreach (ellipse el in ellipses)
                {
                    calculatePoints(el);
                }
                foreach (ellipse el in active)
                {
                    pen.Color = el.Color;
                    
                    double circumference;
                    if (el.A == 0 || el.B == 0)
                    {
                        circumference = el.A > el.B ? 2 * el.A : 2 * el.B;
                        approxPi(el, 2 * circumference);
                    }
                    else
                    {
                        circumference = calculacteCircumference(el.Points);
                        approxPi(el, circumference);
                    }
                    
                    totalCircumference += circumference;
                    List<Point> p = transformPoints(el.Points);
                    for (int i = 0; i < p.Count; i++)
                    {
                        if (i != p.Count - 1)
                            g.DrawLine(pen, p[i], p[i + 1]);
                        else
                            g.DrawLine(pen, p[i], p[0]);
                    }
                }

                pictureBox1.Invoke(new Action(() => pictureBox1.Refresh()));
                setLabels();
                progressBar1.Invoke(new Action(()=>progressBar1.Value += 10));
                Thread.Sleep(1000);
                
            }
            e.Cancel = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Pen pen = new Pen(Color.Red, 1F);
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            totalCircumference = 0;
            piApprox = 0;
            foreach (ellipse el in active)
            {
                pen.Color = el.Color;
                List<eliPoint> ep = new List<eliPoint>();
                for (int k = 0; k < 10; k++)
                {
                    calculatePoints(el, ep);
                }
                double circumference;
                if (el.A == 0 || el.B == 0)
                {
                    circumference = el.A > el.B ? 2 * el.A : 2 * el.B;
                    approxPi(el, 2 * circumference);
                }
                else
                {
                    circumference = calculacteCircumference(ep);
                    approxPi(el, circumference);
                }
                
                totalCircumference += circumference;
                List<Point> p = transformPoints(ep);
                for (int i = 0; i < p.Count; i++)
                {
                    if (i != p.Count - 1)
                        g.DrawLine(pen, p[i], p[i + 1]);
                    else
                        g.DrawLine(pen, p[i], p[0]);
                }
            } 
            
            pictureBox1.Refresh();
            setLabels();

        }
        private void setLabels()
        {
            minX.Invoke(new Action(() => minX.Text = "Min X:   -" + maxA));
            minY.Invoke(new Action(() => minY.Text = "Min Y:   -" + maxB));
            maxX.Invoke(new Action(() => maxX.Text = "Max X:    " + maxA));
            maxY.Invoke(new Action(() => maxY.Text = "Max Y:    " + maxB));
            circum.Invoke(new Action(() => circum.Text = totalCircumference.ToString()));
            pi.Invoke(new Action(() => pi.Text = piApprox.ToString()));
        }
        public class ellipse
        {
            public double A { get; }
            public double B { get; }

            public Color Color { get; }
            public List<eliPoint> Points { get; }
            public ellipse(double _a, double _b)
            {
                A = Math.Abs(_a);
                B = Math.Abs(_b);
                Points = new List<eliPoint>();
                Color = colors[rand.Next(colors.Count)];
            }

            public void Clear() { Points.Clear(); }
           
        }

        public class eliPoint
        {
            public double X { get; }
            public double Y { get; }

            public eliPoint(double _x, double _y)
            {
                X = _x;
                Y = _y;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (!backgroundWorker1.IsBusy)
            {          
                progressBar1.Value = 0;
                foreach (ellipse el in ellipses)
                {
                    el.Clear();
                }
                backgroundWorker1.RunWorkerAsync();
                setLabels();
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
              
            Form2 addEllipseForm = new Form2(this);
            addEllipseForm.FormClosed += addToCheckBox;
            addEllipseForm.ShowDialog();                   
            
        }

        private void addToCheckBox(object sender, FormClosedEventArgs e)
        {
            if (ToAdd != null)
            {
                checkedListBox1.Items.Add(ToAdd);
                double[] ab = Regex.Matches(ToAdd, @"[+-]?([0-9]*[.,])?[0-9]+").OfType<Match>().Select(m => double.Parse(m.Value)).ToArray();
                ellipses.Add(new ellipse(ab[0], ab[1]));
            }
            ToAdd = null;
            
        }

        public void setXY()
        {
            if (active.Count != 0)
            {
                maxA = active[0].A;
                maxB = active[0].B;
                foreach (ellipse e in active)
                {
                    if (e.A > maxA)
                        maxA = e.A;
                    if (e.B > maxB)
                        maxB = e.B;
                }
            }
            else
            {
                maxA = 0;
                maxB = 0;
            }
        }

        private double calculacteCircumference(List<eliPoint> el)
        {
            double circum = 0;

            for (int i = 0; i < el.Count; i++)
            {
                if (i != el.Count - 1)
                    circum += Math.Sqrt(Math.Pow(el[i].X - el[i + 1].X, 2) + Math.Pow(el[i].Y - el[i + 1].Y, 2));
                else
                    circum += Math.Sqrt(Math.Pow(el[i].X - el[0].X, 2) + Math.Pow(el[i].Y - el[0].Y, 2));

            }
            return circum;
        }
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            CheckedListBox clb = (CheckedListBox)sender;
            clb.ItemCheck -= checkedListBox1_ItemCheck;
            clb.SetItemCheckState(e.Index, e.NewValue);
            clb.ItemCheck += checkedListBox1_ItemCheck;
            moveToActive();
            setXY();
        }

        private void approxPi(ellipse e, double p)
        {
            if (e.A + e.B == 0)
                return;
            double h = Math.Pow(e.A - e.B, 2) / Math.Pow(e.A + e.B, 2);
            double pi = p / ((e.A + e.B) * (1 + 3 * h / (10 + Math.Sqrt(4 - 3 * h))));
            if (piApprox == 0)
                piApprox = pi;
            else
                piApprox = (piApprox + pi) / 2;
        }
    }
}
