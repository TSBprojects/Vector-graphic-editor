using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Лаба_8
{
    public partial class Form1 : Form
    {
        Pen p;
        Size size;
        Figure currFigure;
        Point Current;
        Point Previous;
        Point StartP;
        bool isPressed;
        List<Figure> list = new List<Figure>();
        bool Line = false, Rectangle = false, selectMode = false, resizeMode = false, drawMode = false, move = false;
        public Form1()
        {
            InitializeComponent();
            InitializeButtons();
            p = new Pen(Color.Black, 1);
        }

        private void SelectedFigureOnTop()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsSelected)
                {
                    Figure tmp;
                    tmp = list[list.Count - 1];
                    list[list.Count - 1] = list[i];
                    list[i] = tmp;
                }
            }
        }
        private Figure IsAnyResizePoint(Point e)
        {
            foreach (Figure f in list) if (f.IsResizePoint(e)) return f;
            return null;
        }
        private Figure IsAnyFigureHere(Point e)
        {
            foreach (Figure f in list) if (f.IsFigureHere(e)) return f;
            return null;
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Figure tmp = IsAnyResizePoint(e.Location);
            if (tmp != null) { SelectedFigureOnTop(); tmp.ResetResizePoints(); currFigure = tmp; resizeMode = true; }
            else
            {
                tmp = IsAnyFigureHere(e.Location);
                if (tmp != null) { SelectedFigureOnTop(); foreach (Figure F in list) F.ResetResizePoints(); currFigure = tmp; selectMode = true; }
                else currFigure = null;
            }
            if (!selectMode && !resizeMode) foreach (Figure f in list) f.IsSelected = false;
            else foreach (Figure f in list) if (currFigure != f) f.IsSelected = false;
            pictureBox1.Refresh();
            Previous = e.Location;
            isPressed = true;
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label1.Text = $"{e.X }, {e.Y} пкс";
            foreach (Figure f in list) { f.IsFigureHere(e.Location, false); f.IsResizePoint(e.Location, false); }
            if (isPressed)
            {
                move = true;
                if (selectMode)
                {
                    currFigure.Translate(e.Location);
                    pictureBox1.Refresh();
                }
                else if (resizeMode)
                {
                    currFigure.Resize(e.Location);
                    pictureBox1.Refresh();
                }
                else
                {
                    Current = e.Location;
                    pictureBox1.Refresh();
                }
            }
        }
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            label1.Text = " ";
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Previous != e.Location && !selectMode && !resizeMode && drawMode)
            {
                if (Line) Addline();
                else if (Rectangle) AddRectangle();
                else AddCircle();
            }
            isPressed = false; selectMode = false; resizeMode = false; move = false;
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (Line) DrawLine(e);
            else if (Rectangle) DrawRectangle(e);
            else DrawCircle(e);
        }
        private void Addline()
        {
            list.Add(new Line(new Pen(p.Color, p.Width), Previous, Current));
            list[list.Count - 1].IsSelected = true;
            currFigure = list[list.Count - 1];
            pictureBox1.Refresh();
        }
        private void AddRectangle()
        {
            list.Add(new Rect(new Pen(p.Color, p.Width), StartP, size));
            list[list.Count - 1].IsSelected = true;
            currFigure = list[list.Count - 1];
            pictureBox1.Refresh();
        }
        private void AddCircle()
        {
            list.Add(new Circle(new Pen(p.Color, p.Width), StartP, size));
            list[list.Count - 1].IsSelected = true;
            currFigure = list[list.Count - 1];
            pictureBox1.Refresh();
        }
        private void DrawLine(PaintEventArgs e)
        {
            if (!selectMode && !resizeMode && move && drawMode) e.Graphics.DrawLine(p, Previous, Current);
            foreach (Figure f in list) f.Draw(e.Graphics);
        }
        private void DrawRectangle(PaintEventArgs e)
        {
            if (!selectMode && !resizeMode && move && drawMode)
            {
                int width = Current.X - Previous.X, height = Current.Y - Previous.Y;
                if (width < 0 && height < 0)
                {
                    size = new Size(width * (-1), height * (-1));
                    StartP = Current;
                    e.Graphics.DrawRectangle(p, new Rectangle(StartP, size));
                }
                else if (width < 0)
                {
                    size = new Size(width * (-1), height);
                    StartP = new Point(Previous.X + width, Previous.Y);
                    e.Graphics.DrawRectangle(p, new Rectangle(StartP, size));
                }
                else if (height < 0)
                {
                    size = new Size(width, height * (-1));
                    StartP = new Point(Previous.X, Previous.Y + height);
                    e.Graphics.DrawRectangle(p, new Rectangle(StartP, size));
                }
                else
                {
                    size = new Size(width, height);
                    StartP = Previous;
                    e.Graphics.DrawRectangle(p, new Rectangle(StartP, size));
                }
            }
            foreach (Figure f in list) f.Draw(e.Graphics);
        }
        private void DrawCircle(PaintEventArgs e)
        {
            if (!selectMode && !resizeMode && move && drawMode)
            {
                int width = Current.X - Previous.X, height = Current.Y - Previous.Y;
                if (width < 0 && height < 0)
                {
                    size = new Size(width * (-1), height * (-1));
                    StartP = Current;
                    e.Graphics.DrawEllipse(p, new Rectangle(StartP, size));
                }
                else if (width < 0)
                {
                    size = new Size(width * (-1), height);
                    StartP = new Point(Previous.X + width, Previous.Y);
                    e.Graphics.DrawEllipse(p, new Rectangle(StartP, size));
                }
                else if (height < 0)
                {
                    size = new Size(width, height * (-1));
                    StartP = new Point(Previous.X, Previous.Y + height);
                    e.Graphics.DrawEllipse(p, new Rectangle(StartP, size));
                }
                else
                {
                    size = new Size(width, height);
                    StartP = Previous;
                    e.Graphics.DrawEllipse(p, new Rectangle(StartP, size));
                }
            }
            foreach (Figure f in list) f.Draw(e.Graphics);
        }

        private void HandB_Click(object sender, EventArgs e)
        {
            ClearBorders();
            HandB.FlatAppearance.BorderSize = 1;
            drawMode = false;
            Line = false;
            Rectangle = false;
        }
        private void LineB_Click(object sender, EventArgs e)
        {
            ClearBorders();
            LineB.FlatAppearance.BorderSize = 1;
            Line = true;
            Rectangle = false;
            drawMode = true;
        }
        private void CyrcleB_Click(object sender, EventArgs e)
        {
            ClearBorders();
            CyrcleB.FlatAppearance.BorderSize = 1;
            Line = false;
            Rectangle = false;
            drawMode = true;
        }
        private void RectangleB_Click(object sender, EventArgs e)
        {
            ClearBorders();
            RectangleB.FlatAppearance.BorderSize = 1;
            Line = false;
            Rectangle = true;
            drawMode = true;
        }

        private void White_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.White);
        }
        private void Black_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Black);
        }
        private void Gray25_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.LightGray);
        }
        private void Gray50_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Gray);
        }
        private void Brown_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Brown);
        }
        private void DarkRed_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.DarkRed);
        }
        private void Pink_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Pink);
        }
        private void Red_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Red);
        }
        private void Golden_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Gold);
        }
        private void Orange_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Orange);
        }
        private void LightYellow_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.LightYellow);
        }
        private void Yellow_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Yellow);
        }
        private void Herbal_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.GreenYellow);
        }
        private void Green_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Green);
        }
        private void LightTurquoise_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.PaleTurquoise);
        }
        private void Turquoise_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.DeepSkyBlue);
        }
        private void Sizy_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.SteelBlue);
        }
        private void Indigo_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Indigo);
        }
        private void Lilac_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Thistle);
        }
        private void Purple_Click(object sender, EventArgs e)
        {
            ChangeColor(Color.Purple);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            trackBar1.Minimum = 1;
            trackBar1.Maximum = 20;
            if (currFigure != null) currFigure.Pen.Width = trackBar1.Value;
            p.Width = trackBar1.Value;
            pictureBox1.Refresh();
        }
        private void ChangeColor(Color c)
        {
            if (currFigure != null) currFigure.Pen.Color = c;
            p.Color = c;
            ActiveColor.BackColor = c;
            pictureBox1.Refresh();
        }
        private void InitializeButtons()
        {
            HandB.FlatAppearance.BorderSize = 1;
            LineB.FlatAppearance.BorderSize = 0;
            CyrcleB.FlatAppearance.BorderSize = 0;
            RectangleB.FlatAppearance.BorderSize = 0;
            HandB.FlatAppearance.MouseOverBackColor = Color.Transparent;
            HandB.FlatAppearance.MouseDownBackColor = Color.Transparent;
            LineB.FlatAppearance.MouseOverBackColor = Color.Transparent;
            LineB.FlatAppearance.MouseDownBackColor = Color.Transparent;
            CyrcleB.FlatAppearance.MouseOverBackColor = Color.Transparent;
            CyrcleB.FlatAppearance.MouseDownBackColor = Color.Transparent;
            RectangleB.FlatAppearance.MouseOverBackColor = Color.Transparent;
            RectangleB.FlatAppearance.MouseDownBackColor = Color.Transparent;
            HandB.FlatAppearance.BorderColor = Color.Red;
            LineB.FlatAppearance.BorderColor = Color.Red;
            CyrcleB.FlatAppearance.BorderColor = Color.Red;
            RectangleB.FlatAppearance.BorderColor = Color.Red;
        }
        private void ClearBorders()
        {
            HandB.FlatAppearance.BorderSize = 0;
            LineB.FlatAppearance.BorderSize = 0;
            CyrcleB.FlatAppearance.BorderSize = 0;
            RectangleB.FlatAppearance.BorderSize = 0;
        }

    }
}
