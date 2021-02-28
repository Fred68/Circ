using Newtonsoft.Json;                      // Serializzazione in Json
using System;
using System.Drawing;                       // Per draw
using System.Drawing.Drawing2D;

namespace Circ
	{
	public class Ramo:Elemento, IDraw, ICopyData
		{
		uint[] n;                   // ID dei nodi di partenza ed arrivo
		Nodo[] nd;                  // Nodi di partenza ed arrivo

		Point pc;                   // Centro, cordinata di schermo, scalata
		double rotation;            // angolo di rotazione

		/// <summary>
		/// Costruttore
		/// </summary>
		public Ramo() : base()
			{
			n = new uint[2];
			nd = new Nodo[2];
			n[0] = n[1] = UNASSIGNED;
			nd[0] = nd[1] = null;
			rotation = 0;
			}

		#region PROPRIETÀ (e SERIALIZZAZIONE)

		public uint N1 { get { return n[0]; } set { n[0] = value; } }
		public uint N2 { get { return n[1]; } set { n[1] = value; } }

		[JsonIgnore]
		public Nodo Nd1 { get { return nd[0]; } set { nd[0] = value; } }

		[JsonIgnore]
		public Nodo Nd2 { get { return nd[1]; } set { nd[1] = value; } }

		[JsonIgnore]
		public override Point Center
			{
			get { return pc; }
			}

		#endregion

		public override void Regen(Vista v,bool addToDisplayList = true)
			{
			Point2D p1 = nd[0].P;
			Point2D p2 = nd[1].P;
			if((p1 != null) && (p2 != null))
				{
				nd[0].Regen(v,false);               // Rigenera i nodi di estremità calcolando solo la scala...
				nd[1].Regen(v,false);               // ... ma senza aggiungerli alla display list
				pc = v.Scala(Point2D.Midpoint(nd[0].P,nd[1].P));

				Point2D delta = p2 - p1;
				rotation = Math.Atan2(delta.Y * Math.Sign(v.Verso.Y),delta.X * Math.Sign(v.Verso.X)) * 180 / Math.PI;
				m.Reset();                          // Torna alla matrice identità (non richiedo riallocazione)
				m.Translate(pc.X,pc.Y);
				m.Rotate((float)rotation);

				if(addToDisplayList)
					v.AddDL(this,Def.Shape.Ramo,pc.X,pc.Y);
				}
			}

		public override Def.ClipFlag Clip(Vista v)
			{
			Def.ClipFlag clip1 = Nd1.Clipped;
			Def.ClipFlag clip2 = Nd2.Clipped;

			if((clip1 | clip2) == Def.ClipFlag.Inside)          // Entrambi gli estremi all'interno (bitwise OR è zero): no clip
				{
				clipped = Def.ClipFlag.Inside;
				}
			else if((clip1 & clip2) != Def.ClipFlag.Inside)     //	Bitwise AND non è zero: condividono una area esterna: clip
				{
				clipped = Def.ClipFlag.Outside;
				}
			else
				{
				clipped = Def.ClipFlag.Inside;                  // Clip parziale. Non si esegue il clipping sui double, ...
				}                                               // ...ma si lascia fare a Windows.Graphics, su coordiante intere, più veloce
			return clipped;
			}

		public override void Draw(Graphics g, Vista v, bool high, bool sel)
			{
			Stile.Colore col = sel ? Stile.Colore.Selected : Stile.Colore.Ramo;
			if(high)	col = Stile.Colore.Highlighted;

			g.DrawLine(v.Pen(col), nd[0].Ps, nd[1].Ps);         // Disegna prima la linea, poi sovrappone una figura con il simbolo

			if(Point2D.Mod(new Point(nd[1].Ps.X - nd[0].Ps.X,nd[1].Ps.Y - nd[0].Ps.Y)) > Def.SHAPE_HALFSIZE * Def.SHAPE_HALFSIZE * 4)
				{
				GraphicsPath pth = (GraphicsPath)Def.Shape2D.GetShape(Def.Shape2D.Name.Arrow).Clone();
				pth.Transform(m);
				g.DrawPath(v.Pen(col),pth);
				//g.FillPath(br,pth);
				}

			g.DrawString(id.ToString(), v.Font(0), v.Brush(col), pc.X + Def.NODE_HALFSIZE * 2,pc.Y + Def.NODE_HALFSIZE * 2);

			}
		//public override void Draw(Graphics g,Pen pn,Brush br,Font fn)
		//	{
		//	g.DrawLine(pn,nd[0].Ps,nd[1].Ps);         // Disegna prima la linea, poi sovrappone una figura con il simbolo

		//	if(Point2D.Mod(new Point(nd[1].Ps.X - nd[0].Ps.X,nd[1].Ps.Y - nd[0].Ps.Y)) > Def.SHAPE_HALFSIZE * Def.SHAPE_HALFSIZE * 4)
		//		{
		//		//GraphicsPath pth = (GraphicsPath)Def.Shape2D.GetShape(Def.Shape2D.Shape.Rectangle).Clone();
		//		GraphicsPath pth = (GraphicsPath)Def.Shape2D.GetShape(Def.Shape2D.Name.Arrow).Clone();
		//		pth.Transform(m);
		//		g.DrawPath(pn,pth);
		//		//g.FillPath(br,pth);
		//		}

		//	g.DrawString(id.ToString(),fn,br,pc.X + Def.NODE_HALFSIZE * 2,pc.Y + Def.NODE_HALFSIZE * 2);

		//	}

		public override void CopyData(Elemento e)
			{
			base.CopyData(e);           // Copia i dati base
			if(e is Ramo)
				{ }                     // Al momento non ha altri dati da copiare (resta connesso ai suoi nodi)
			}

		}   // Fine classe Ramo
	}
