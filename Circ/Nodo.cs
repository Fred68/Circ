using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;						// Per draw
using Newtonsoft.Json;						// Serializzazione in Json


namespace Circ
	{
	public class Nodo : Elemento, IDraw, IID
		{
		Point2D p;

		/// <summary>
		/// Costruttore
		/// </summary>
		public Nodo() : base()
			{
			p = new Point2D();
			}

		#region PROPRIETÀ PER SERIALIZZAZIONE

		public Point2D P {get{return p;} set{p=value;}}

		#endregion

		public override void Draw(Vista v)
			{
			Point pv = v.Scala(p);
			Point c = Center(v);
			v.AddDL(this,Def.Shape.Nodo, Def.Colori.Black, pv.X-Def.NODE_HALFSIZE, pv.Y-Def.NODE_HALFSIZE, pv.X+Def.NODE_HALFSIZE, pv.Y+Def.NODE_HALFSIZE, c.X, c.Y);
			}
		public override Point Center(Vista v)
			{
			return v.Scala(p);
			}
			

		}	// Fine classe Nodo
	}
