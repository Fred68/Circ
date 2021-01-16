using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;						// Per draw
using Newtonsoft.Json;						// Serializzazione in Json


namespace Circ
	{
	public class Ramo : Elemento, IDraw, IID
		{
		uint[] n;					// ID dei nodi di partenza ed arrivo
		Nodo[] nd;					// Nodi di partenza ed arrivo
		/// <summary>
		/// Costruttore
		/// </summary>
		public Ramo() : base()
			{
			n = new uint[2];
			nd = new Nodo[2];
			n[0] = n[1] = UNASSIGNED;
			nd[0] = nd[1] = null;
			}

		#region PROPRIETÀ PER SERIALIZZAZIONE

		public uint N1 {get{return n[0];} set{n[0]=value;}}
		public uint N2 {get{return n[1];} set{n[1]=value;}}

		[JsonIgnore]
		public Nodo Nd1 {get{return nd[0];} set{nd[0]=value;}}

		[JsonIgnore]
		public Nodo Nd2 {get{return nd[1];} set{nd[1]=value;}}

		#endregion
		public override void Draw(Vista v)
			{
			Point2D p1 = nd[0].P;
			Point2D p2 = nd[1].P;
			if((p1 != null) && (p2 != null))
				{
				Point pv1 = v.Scala(p1);
				Point pv2 = v.Scala(p2);
				Point c = Center(v);
				v.AddDL(this,Def.Shape.Ramo, Def.Colori.Red, pv1.X, pv1.Y, pv2.X, pv2.Y, c.X, c.Y);
				}
			}
		public override Point Center(Vista v)
			{
			return v.Scala(Point2D.Midpoint(nd[0].P,nd[1].P));
			}
		}	// Fine classe Ramo
	}
