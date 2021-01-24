using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;						// Per draw
using Newtonsoft.Json;						// Serializzazione in Json


namespace Circ
	{
	public class Nodo : Elemento, IDraw
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

		#region PROPRIETÀ PER SERIALIZZAZIONE

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
			Clipped = v.IsInsideWorld(p);
			return Clipped;
			}

		public override void Draw(Graphics g, Pen pn, Brush br, Font fn)
			{
			g.DrawEllipse(pn, ps.X-Def.NODE_HALFSIZE, ps.Y-Def.NODE_HALFSIZE, 2*Def.NODE_HALFSIZE, 2*Def.NODE_HALFSIZE);
			g.DrawString(ID.ToString(), fn, br, ps.X-2*Def.FONT_SIZE-Def.NODE_HALFSIZE*2, ps.Y-Def.FONT_SIZE-Def.NODE_HALFSIZE*2);;
			}

		}	// Fine classe Nodo
	}
