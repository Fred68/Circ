
using Newtonsoft.Json;                      // Serializzazione in Json
using System.Drawing;                       // Per draw
using System.Drawing.Drawing2D;


namespace Circ
	{
	public class Nodo:Elemento, IDraw, ICopyData
		{
		Point2D p;          // Coordinate in World
		Point ps;           // Cordinata di schermo, scalata

		#region COSTRUTTORI
		public Nodo() : base()
			{
			p = new Point2D();
			}
		public Nodo(Point2D pos) : base()
			{
			p = pos;
			}
		#endregion

		#region PROPRIETÀ (e SERIALIZZAZIONE)

		public Point2D P { get { return p; } set { p = value; } }

		[JsonIgnore]
		public Point Ps { get { return ps; } set { ps = value; } }

		[JsonIgnore]
		public override Point Center
			{
			get { return ps; }
			}

		#endregion

		public override void Regen(Vista v, bool addToDisplayList = true)
			{
			ps = v.Scala(p);
			if(addToDisplayList)
				v.AddDL(this,Def.Shape.Nodo,ps.X,ps.Y);
			}

		public override Def.ClipFlag Clip(Vista v)
			{
			clipped = v.IsInsideWorld(p);
			return clipped;
			}
		
		public override void Draw(Graphics g, Vista v, bool high, bool sel)
			{
			GraphicsPath pth = (GraphicsPath)Def.Shape2D.GetShape(Def.Shape2D.Name.Circle).Clone();
			Stile.Colore col = sel ? Stile.Colore.Selected : Stile.Colore.Nodo;
			if(high)	col = Stile.Colore.Highlighted;
			Matrix m = new Matrix();
			m.Translate(ps.X,ps.Y);
			pth.Transform(m);
			g.DrawPath(v.Pen(Stile.Colore.Nodo),pth);
			string str = id.ToString() + System.Environment.NewLine + "---";
			SizeF sz = g.MeasureString(str, v.Font(0));
			g.DrawRectangle(v.Pen(col),ps.X - 2 * Def.FONT_SIZE - Def.NODE_HALFSIZE * 2,ps.Y - Def.FONT_SIZE - Def.NODE_HALFSIZE * 2,sz.Width,sz.Height);
			//g.FillRectangle(     ,ps.X-2*Def.FONT_SIZE-Def.NODE_HALFSIZE*2, ps.Y-Def.FONT_SIZE-Def.NODE_HALFSIZE*2,sz.Width,sz.Height);
			g.DrawString(str,v.Font(0),v.Brush(col),ps.X - 2 * Def.FONT_SIZE - Def.NODE_HALFSIZE * 2,ps.Y - Def.FONT_SIZE - Def.NODE_HALFSIZE * 2);
			}

		public override void CopyData(Elemento e)
			{
			base.CopyData(e);           // Copia i dati base
			if(e is Nodo)               // Non copia la posizione, per poter copiare i dati da un nodo a più nodi diversi
				{ }                     // Non copia la posizione con p = new Point2D( ((Nodo)e).p ); 
			}

		}   // Fine classe Nodo
	}   // Fine namespace


