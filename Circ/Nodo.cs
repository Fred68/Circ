using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;                       // Per draw
using System.Drawing.Drawing2D;
using Newtonsoft.Json;						// Serializzazione in Json


namespace Circ
	{
	public class Nodo : Elemento, IDraw, ICopyData
		{
		Point2D p;			// Coordinate in World
		Point ps;			// Cordinata di schermo, scalata

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

		public Point2D P {get { return p; } set { p = value; } }

		[JsonIgnore]
		public Point Ps {get { return ps; } set { ps = value; } }

		[JsonIgnore]
		public override Point Center
			{
			get {return ps;}
			}

		#endregion

		public override void Regen(Vista v, bool addToDisplayList = true)
			{
			ps = v.Scala(p);
			if(addToDisplayList)
				v.AddDL(this,Def.Shape.Nodo, Def.Colori.Black, ps.X, ps.Y);
			}
		
		public override Def.ClipFlag Clip(Vista v)
			{
			clipped = v.IsInsideWorld(p);
			return clipped;
			}

		public override void Draw(Graphics g, Pen pn, Brush br, Font fn)
			{
			GraphicsPath pth = (GraphicsPath) Def.Shape2D.GetShape(Def.Shape2D.Name.Circle).Clone();
			Matrix m = new Matrix();
			m.Translate(ps.X,ps.Y);
			pth.Transform(m);
			g.DrawPath(pn, pth);
			g.DrawString(id.ToString(), fn, br, ps.X-2*Def.FONT_SIZE-Def.NODE_HALFSIZE*2, ps.Y-Def.FONT_SIZE-Def.NODE_HALFSIZE*2);;
			}

		public override void CopyData(Elemento e) 
			{
			base.CopyData(e);			// Copia i dati base
			if(e is Nodo)				// Non copia la posizione, per poter copiare i dati da un nodo a più nodi diversi
				{}						// Non copia la posizione con p = new Point2D( ((Nodo)e).p ); 
			}

		}	// Fine classe Nodo
	}	// Fine namespace


