using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;						// Per draw
using Newtonsoft.Json;						// Serializzazione in Json


namespace Circ
	{
	public class Ramo : Elemento, IDraw
		{
		uint[] n;					// ID dei nodi di partenza ed arrivo
		Nodo[] nd;					// Nodi di partenza ed arrivo

		Point pc;					// Centro, cordinata di schermo, scalata
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

		[JsonIgnore]
		public override Point Center
			{
			get {return pc;}
			}

		#endregion

		public override void Regen(Vista v, bool addToDisplayList = true)
			{
			Point2D p1 = nd[0].P;
			Point2D p2 = nd[1].P;
			if((p1 != null) && (p2 != null))
				{
				nd[0].Regen(v, false);				// Rigenera i nodi di estremità calcolando solo la scala...
				nd[1].Regen(v, false);				// ... ma senza aggiungerli alla display list
				pc = v.Scala(Point2D.Midpoint(nd[0].P,nd[1].P));
				if(addToDisplayList)
					v.AddDL(this,Def.Shape.Ramo, Def.Colori.Red, pc.X, pc.Y);
				}
			}

		public override Def.ClipFlag Clip(Vista v)
			{
			Def.ClipFlag clip1 = Nd1.Clipped;
			Def.ClipFlag clip2 = Nd2.Clipped;

			if( (clip1 | clip2) == Def.ClipFlag.Inside)			// Entrambi gli estremi all'interno (bitwise OR è zero): no clip
				{
				Clipped = Def.ClipFlag.Inside;
				}
			else if((clip1 & clip2) != Def.ClipFlag.Inside)		//	Bitwise AND non è zero: condividono una area esterna: clip
				{
				Clipped = Def.ClipFlag.Outside;
				}
			else
				{
				Clipped = Def.ClipFlag.Inside;					// Clip parziale. Non si esegue il clipping sui double, ...
				}												// ...ma si lascia fare a Windows.Graphics, su coordiante intere, più veloce
			return Clipped;
			}

		public override void Draw(Graphics g, Pen pn, Brush br, Font fn)
			{
			g.DrawLine(pn, nd[0].Ps, nd[1].Ps);
			g.DrawString(ID.ToString(), fn, br,pc.X-2*Def.FONT_SIZE-Def.NODE_HALFSIZE*2,pc.Y-Def.FONT_SIZE-Def.NODE_HALFSIZE*2);
			}
		}	// Fine classe Ramo
	}
