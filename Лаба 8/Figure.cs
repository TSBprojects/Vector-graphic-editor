using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Лаба_8
{
    abstract class Figure
    {
        public abstract Pen Pen { get; set; }
        public abstract void Draw(Graphics g);
        public abstract void Resize(Point cur);
        public abstract bool IsFigureHere(Point cur, bool click = true);
        public abstract void Translate(Point cur);
        public abstract void ResetResizePoints();
        public abstract bool IsSelected { get; set; }
        public abstract bool IsResizePoint(Point cur, bool click = true);
    }
    class Line : Figure
    {
        Point pt1;
        Point pt2;
        Point SelP;
        Point[] Points;
        GraphicsPath[] gp;
        GraphicsPath resizePoint1; bool rP1 = false;
        GraphicsPath resizePoint2; bool rP2 = false;
        public override bool IsSelected { get; set; }
        public override Pen Pen { get; set; }
        public Line(Pen Pen, Point pt1, Point pt2)
        {
            this.Pen = Pen;
            this.pt1 = pt1;
            this.pt2 = pt2;
            IsSelected = false;
            SetAllPoints(); SetAllGP();
        }

        public override bool IsFigureHere(Point cur, bool click)
        {
            for (int i = 0; i < gp.Length; i++)
            {
                if (gp[i].GetBounds().Contains(cur))
                {
                    Cursor.Current = Cursors.SizeAll;
                    if (click) { SelP = Points[i]; IsSelected = true; }
                    return true;
                }
            }
            if (click) { IsSelected = false; rP1 = false; rP2 = false; resizePoint1 = null; resizePoint2 = null; }
            return false;
        }
        public override bool IsResizePoint(Point cur, bool click)
        {
            if (resizePoint1 != null && resizePoint2 != null)
            {
                if (resizePoint1.GetBounds().Contains(cur)) { rP1 = true; Cursor.Current = Cursors.SizeNS; return rP1; }
                else if (resizePoint2.GetBounds().Contains(cur)) { rP2 = true; Cursor.Current = Cursors.SizeNS; return rP2; }
                else return false;
            }
            return false;
        }
        public override void Draw(Graphics g)
        {
            int s=6;
            g.DrawLine(Pen, pt1, pt2);
            if (IsSelected)
            {
                Rectangle r1 = new Rectangle(pt1.X - s / 2, pt1.Y - s / 2, s, s);
                Rectangle r2 = new Rectangle(pt2.X - s / 2, pt2.Y - s / 2, s, s);
                g.FillRectangle(Brushes.White, r1);
                g.DrawRectangle(new Pen(Color.Black,1),r1);
                g.FillRectangle(Brushes.White, r2);
                g.DrawRectangle(new Pen(Color.Black, 1),r2);
                resizePoint1 = new GraphicsPath();
                resizePoint2 = new GraphicsPath();
                resizePoint1.AddRectangle(r1);
                resizePoint2.AddRectangle(r2);
                SetAllGP();
            }
        }
        public override void ResetResizePoints()
        {
            rP1 = false; rP2 = false; resizePoint1 = null; resizePoint2 = null;
        }
        public override void Resize(Point cur)
        {
            if(rP1) pt1 = cur;
            else if(rP2) pt2 = cur;
            SetAllPoints(); SetAllGP();
            IsSelected = true;
        }
        public override void Translate(Point cur)
        {
            int deltaX = cur.X - SelP.X, deltaY = cur.Y - SelP.Y;
            for (int i = 0; i < Points.Length; i++) { Points[i].X += deltaX; Points[i].Y += deltaY; }
            SelP.X += deltaX; SelP.Y += deltaY;
            pt1.X += deltaX; pt2.X += deltaX;
            pt1.Y += deltaY; pt2.Y += deltaY;
            SetAllGP();
        }

        private void SetAllGP()
        {
            Rectangle r;
            GraphicsPath tmp;
            List<GraphicsPath> GpList = new List<GraphicsPath>();
            for (int i = 0; i < Points.Length; i++)
            {
                r = new Rectangle((int)(Points[i].X - Pen.Width), (int)(Points[i].Y - Pen.Width), (int)Pen.Width * 2, (int)Pen.Width * 2);
                tmp = new GraphicsPath();
                tmp.AddRectangle(r);
                GpList.Add(tmp);
            }
            gp = GpList.ToArray();
        }
        private void SetAllPoints()
        {
            double Rab = 0;
            List<Point> Points = new List<Point>();
            Rab = Math.Sqrt((pt2.X - pt1.X) * (pt2.X - pt1.X) + (pt2.Y - pt1.Y) * (pt2.Y - pt1.Y));
            for (int i = 0; i < Rab; i++) Points.Add(GetPoint(i));
            this.Points = Points.ToArray();
        }
        private Point GetPoint(double Rac)
        {
            double Rab = 0, k = 0, Xc = 0, Yc = 0;
            Rab = Math.Sqrt((pt2.X - pt1.X)* (pt2.X - pt1.X) + (pt2.Y - pt1.Y)* (pt2.Y - pt1.Y));
            k = Rac / Rab;
            Xc = pt1.X + (pt2.X - pt1.X) * k;
            Yc = pt1.Y + (pt2.Y - pt1.Y) * k;
            return new Point((int)Xc, (int)Yc);
        }
    }
    class Rect : Figure
    {
        int curRP;
        Point pt;
        Size size;
        Pen SelectPen;
        Point deltaP;
        GraphicsPath gp;
        GraphicsPath[] resizePoints = new GraphicsPath[8];
        public override bool IsSelected { get; set; }
        public override Pen Pen { get; set; }
        public Rect(Pen Pen, Point pt, Size size)
        {
            IsSelected = false;
            this.Pen = Pen;
            this.pt = pt;
            this.size = size;
            gp = new GraphicsPath();
            SelectPen = new Pen(Color.White, 1);
            SelectPen.DashStyle = DashStyle.Dash;
            gp.AddRectangle(new Rectangle(pt,size));
        }

        public override bool IsFigureHere(Point cur, bool click)
        { 
            if (gp.GetBounds().Contains(cur))
            {
                Cursor.Current = Cursors.SizeAll;
                if (click) { deltaP = new Point(cur.X - pt.X, cur.Y - pt.Y); IsSelected = true; }
                return true;
            }
            if (click) { IsSelected = false; ClearResizePoints(); }
            return false;
        }
        public override bool IsResizePoint(Point cur, bool click)
        {
            if (!IsClearResizePoints())
                for (int i = 0; i < 8; i++)
                    if (resizePoints[i].GetBounds().Contains(cur))
                    {
                        if (click) curRP = i;
                        switch (i)
                        {
                            case (0): { Cursor.Current = Cursors.SizeNWSE; break; }
                            case (1): { Cursor.Current = Cursors.SizeNS; break; }
                            case (2): { Cursor.Current = Cursors.SizeNESW; break; }
                            case (3): { Cursor.Current = Cursors.SizeWE; break; }
                            case (4): { Cursor.Current = Cursors.SizeNWSE; break; }
                            case (5): { Cursor.Current = Cursors.SizeNS; break; }
                            case (6): { Cursor.Current = Cursors.SizeNESW;  break; }
                            case (7): { Cursor.Current = Cursors.SizeWE; break; }
                            default: { Cursor.Current = Cursors.Default; break; }
                        }
                        return true;
                    }
            return false;
        }
        public override void Draw(Graphics g)
        {

            g.DrawRectangle(Pen, new Rectangle(pt, size));
            if (IsSelected)
            {
                if (Pen.Color == SelectPen.Color) SelectPen.Color = Color.DeepSkyBlue;
                else SelectPen.Color = Color.White;
                g.DrawRectangle(SelectPen, new Rectangle(pt, size));
                DrawResizePoints(g);
            }
        }
        public override void ResetResizePoints()
        {
            ClearResizePoints();
        }
        public override void Resize(Point cur)
        {
            int tmp1, tmp2;
            switch (curRP)
            {
                case (0):
                    {
                        Cursor.Current = Cursors.SizeNWSE;
                        tmp1 = size.Height + (cur.Y - pt.Y) * (-1);
                        tmp2 = size.Width + (cur.X - pt.X) * (-1);
                        if (tmp1 > 7) { size.Height = tmp1; pt.Y -= (cur.Y - pt.Y)*(-1); }
                        if (tmp2 > 7) { size.Width = tmp2; pt.X -= (cur.X - pt.X) *(-1); }
                        break;
                    }
                case (1):
                    {
                        Cursor.Current = Cursors.SizeNS;
                        tmp1 = size.Height + (cur.Y - pt.Y) * (-1);
                        if (tmp1 > 7){ size.Height = tmp1; pt.Y-=(cur.Y - pt.Y)*(-1); }
                        break;
                    }
                case (2):
                    {
                        Cursor.Current = Cursors.SizeNESW;
                        tmp1 = size.Height + (cur.Y - pt.Y) * (-1);
                        tmp2 = cur.X - pt.X;
                        if (tmp1 > 7) { size.Height = tmp1; pt.Y -= (cur.Y - pt.Y) * (-1); }
                        if (tmp2 > 7) size.Width = tmp2; 
                        break;
                    }
                case (3):
                    {
                        Cursor.Current = Cursors.SizeWE;
                        tmp1 = cur.X - pt.X ;
                        if (tmp1 > 7) size.Width = tmp1;
                        break;
                    }
                case (4):
                    {
                        Cursor.Current = Cursors.SizeNWSE;
                        tmp1 = cur.Y - pt.Y;
                        tmp2 = cur.X - pt.X;
                        if (tmp1 > 7) size.Height = tmp1; 
                        if (tmp2 > 7) size.Width = tmp2; 
                        break;
                    }
                case (5):
                    {
                        Cursor.Current = Cursors.SizeNS;
                        tmp1 = cur.Y - pt.Y;
                        if (tmp1 > 7)  size.Height = tmp1; 
                        break;
                    }
                case (6):
                    {
                        Cursor.Current = Cursors.SizeNESW;
                        tmp1 = cur.Y - pt.Y;
                        tmp2 = size.Width + (cur.X - pt.X)*(-1);
                        if (tmp1 > 7)  size.Height = tmp1; 
                        if (tmp2 > 7) { size.Width = tmp2; pt.X -= (cur.X - pt.X)*(-1); }
                        break;
                    }
                case (7):
                    {
                        Cursor.Current = Cursors.SizeWE;
                        tmp1 = size.Width + (cur.X - pt.X) * (-1);
                        if (tmp1 > 7){size.Width = tmp1; pt.X-=(cur.X-pt.X)*(-1); }
                        break;
                    }
                default: { Cursor.Current = Cursors.Default; break; }
            }
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(pt, size));
            IsSelected = true;
        }
        public override void Translate(Point cur)
        {
            pt.X = cur.X - deltaP.X; pt.Y = cur.Y - deltaP.Y;
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(pt,size));
        }
        private void ClearResizePoints()
        {
            for (int i = 0; i < 8; i++) resizePoints[i] = null;
        }
        private bool IsClearResizePoints()
        {
            for (int i = 0; i < 8; i++) if (resizePoints[i] == null)
                    return true;
            return false;
        }
        private void DrawResizePoints(Graphics g)
        {
            Rectangle[] r = new Rectangle[8] 
            {
                new Rectangle(pt.X - 3, pt.Y - 3, 6, 6),
                new Rectangle(pt.X+size.Width/2 - 3, pt.Y - 3, 6, 6),
                new Rectangle(pt.X+size.Width - 3, pt.Y - 3, 6, 6),
                new Rectangle(pt.X+size.Width - 3, pt.Y+size.Height/2 - 3, 6, 6),
                new Rectangle(pt.X+size.Width - 3, pt.Y+size.Height - 3, 6, 6),
                new Rectangle(pt.X+size.Width/2 - 3, pt.Y+size.Height - 3, 6,6),
                new Rectangle(pt.X - 3, pt.Y+size.Height - 3, 6, 6),
                new Rectangle(pt.X - 3, pt.Y+size.Height/2 - 3, 6, 6)
            };
            for (int i = 0; i < 8; i++)
            {
                g.FillRectangle(Brushes.White, r[i]);
                g.DrawRectangle(new Pen(Color.Black, 1), r[i]);
                resizePoints[i] = new GraphicsPath();
                resizePoints[i].AddRectangle(r[i]);
            }
        }
    }
    class Circle : Figure
    {
        Point pt;
        int curRP;
        Size size;
        Pen SelectPen;
        Point deltaP;
        GraphicsPath gp;
        GraphicsPath[] resizePoints = new GraphicsPath[8];
        public override bool IsSelected { get; set; }
        public override Pen Pen { get; set;  }
        public Circle(Pen Pen, Point pt, Size size)
        {
            IsSelected = false;
            this.Pen = Pen;
            this.pt = pt;
            this.size = size;
            gp = new GraphicsPath();
            SelectPen = new Pen(Color.DeepSkyBlue, 1);
            SelectPen.DashStyle = DashStyle.Dash;
            gp.AddRectangle(new Rectangle(pt, size));
        }

        public override bool IsFigureHere(Point cur, bool click)
        {
            if (gp.GetBounds().Contains(cur))
            {
                Cursor.Current = Cursors.SizeAll;
                if (click) { deltaP = new Point(cur.X - pt.X, cur.Y - pt.Y); IsSelected = true; }
                return true;
            }
            if (click) { IsSelected = false; ClearResizePoints(); }
            return false;
        }
        public override bool IsResizePoint(Point cur, bool click)
        {
            if (!IsClearResizePoints())
                for (int i = 0; i < 8; i++)
                    if (resizePoints[i].GetBounds().Contains(cur))
                    {
                        if (click) curRP = i;
                        switch (i)
                        {
                            case (0): { Cursor.Current = Cursors.SizeNWSE; break; }
                            case (1): { Cursor.Current = Cursors.SizeNS; break; }
                            case (2): { Cursor.Current = Cursors.SizeNESW; break; }
                            case (3): { Cursor.Current = Cursors.SizeWE; break; }
                            case (4): { Cursor.Current = Cursors.SizeNWSE; break; }
                            case (5): { Cursor.Current = Cursors.SizeNS; break; }
                            case (6): { Cursor.Current = Cursors.SizeNESW; break; }
                            case (7): { Cursor.Current = Cursors.SizeWE; break; }
                            default: { Cursor.Current = Cursors.Default; break; }
                        }
                        return true;
                    }
            return false;
        }
        public override void Draw(Graphics g)
        {
            g.DrawEllipse(Pen, new Rectangle(pt, size));
            if (IsSelected)
            {
                g.DrawRectangle(SelectPen, new Rectangle(pt, size));
                DrawResizePoints(g);
            }
        }
        public override void ResetResizePoints()
        {
            ClearResizePoints();
        }
        public override void Resize(Point cur)
        {
            int tmp1, tmp2;
            switch (curRP)
            {
                case (0):
                    {
                        Cursor.Current = Cursors.SizeNWSE;
                        tmp1 = size.Height + (cur.Y - pt.Y) * (-1);
                        tmp2 = size.Width + (cur.X - pt.X) * (-1);
                        if (tmp1 > 7) { size.Height = tmp1; pt.Y -= (cur.Y - pt.Y) * (-1); }
                        if (tmp2 > 7) { size.Width = tmp2; pt.X -= (cur.X - pt.X) * (-1); }
                        break;
                    }
                case (1):
                    {
                        Cursor.Current = Cursors.SizeNS;
                        tmp1 = size.Height + (cur.Y - pt.Y) * (-1);
                        if (tmp1 > 7) { size.Height = tmp1; pt.Y -= (cur.Y - pt.Y) * (-1); }
                        break;
                    }
                case (2):
                    {
                        Cursor.Current = Cursors.SizeNESW;
                        tmp1 = size.Height + (cur.Y - pt.Y) * (-1);
                        tmp2 = cur.X - pt.X;
                        if (tmp1 > 7) { size.Height = tmp1; pt.Y -= (cur.Y - pt.Y) * (-1); }
                        if (tmp2 > 7) size.Width = tmp2;
                        break;
                    }
                case (3):
                    {
                        Cursor.Current = Cursors.SizeWE;
                        tmp1 = cur.X - pt.X;
                        if (tmp1 > 7) size.Width = tmp1;
                        break;
                    }
                case (4):
                    {
                        Cursor.Current = Cursors.SizeNWSE;
                        tmp1 = cur.Y - pt.Y;
                        tmp2 = cur.X - pt.X;
                        if (tmp1 > 7) size.Height = tmp1;
                        if (tmp2 > 7) size.Width = tmp2;
                        break;
                    }
                case (5):
                    {
                        Cursor.Current = Cursors.SizeNS;
                        tmp1 = cur.Y - pt.Y;
                        if (tmp1 > 7) size.Height = tmp1;
                        break;
                    }
                case (6):
                    {
                        Cursor.Current = Cursors.SizeNESW;
                        tmp1 = cur.Y - pt.Y;
                        tmp2 = size.Width + (cur.X - pt.X) * (-1);
                        if (tmp1 > 7) size.Height = tmp1;
                        if (tmp2 > 7) { size.Width = tmp2; pt.X -= (cur.X - pt.X) * (-1); }
                        break;
                    }
                case (7):
                    {
                        Cursor.Current = Cursors.SizeWE;
                        tmp1 = size.Width + (cur.X - pt.X) * (-1);
                        if (tmp1 > 7) { size.Width = tmp1; pt.X -= (cur.X - pt.X) * (-1); }
                        break;
                    }
                default: { Cursor.Current = Cursors.Default; break; }
            }
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(pt, size));
            IsSelected = true;
        }
        public override void Translate(Point cur)
        {
            pt.X = cur.X - deltaP.X; pt.Y = cur.Y - deltaP.Y;
            gp = new GraphicsPath();
            gp.AddRectangle(new Rectangle(pt, size));
        }
        private void ClearResizePoints()
        {
            for (int i = 0; i < 8; i++) resizePoints[i] = null;
        }
        private bool IsClearResizePoints()
        {
            for (int i = 0; i < 8; i++) if (resizePoints[i] == null) return true;
            return false;
        }
        private void DrawResizePoints(Graphics g)
        {
            Rectangle[] r = new Rectangle[8]
            {
                new Rectangle(pt.X - 3, pt.Y - 3, 6, 6),
                new Rectangle(pt.X+size.Width/2 - 3, pt.Y - 3, 6, 6),
                new Rectangle(pt.X+size.Width - 3, pt.Y - 3, 6, 6),
                new Rectangle(pt.X+size.Width - 3, pt.Y+size.Height/2 - 3, 6, 6),
                new Rectangle(pt.X+size.Width - 3, pt.Y+size.Height - 3, 6, 6),
                new Rectangle(pt.X+size.Width/2 - 3, pt.Y+size.Height - 3, 6,6),
                new Rectangle(pt.X - 3, pt.Y+size.Height - 3, 6, 6),
                new Rectangle(pt.X - 3, pt.Y+size.Height/2 - 3, 6, 6)
            };
            for (int i = 0; i < 8; i++)
            {
                g.FillRectangle(Brushes.White, r[i]);
                g.DrawRectangle(new Pen(Color.Black, 1), r[i]);
                resizePoints[i] = new GraphicsPath();
                resizePoints[i].AddRectangle(r[i]);
            }
        }
    }
}
