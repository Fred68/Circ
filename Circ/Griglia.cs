using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;			// Point

namespace Circ
	{
	public class Griglia
		{
		bool active;
		bool visible;
		double step;
		int imin, imax, jmin, jmax;

		public bool Active
			{
			get {return active;}
			set {active = value;}
			}

		public double Step
			{
			get {return step;}
			set {if(value > Def.EPSILON)	step = value;}
			}

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="passo"></param>		
		public Griglia(double passo = 100)
			{
			Reset();
			step = passo;
			}

		/// <summary>
		/// Reset
		/// </summary>
		/// <param name="passo"></param>
		public void Reset(double passo = 100)
			{
			active = false;
			visible = true;
			Step = passo;
			imin = imax = jmin = jmax = 0;
			}

		public void Recalc(Vista v)
			{
			int dx1, dy1, dx2, dy2;
			// Origine di World è sempre il punto Point2D(0,0).
			dx1 = (int)(v.SzWorldTopLeft.X/step) * Math.Sign(v.Verso.X);
			dy1 = (int)(v.SzWorldTopLeft.Y/step)* Math.Sign(v.Verso.Y);
			dx2 = (int)(v.SzWorldBottomRight.X/step) * Math.Sign(v.Verso.X);
			dy2 = (int)(v.SzWorldBottomRight.Y/step) * Math.Sign(v.Verso.Y);

			imin = Math.Min(dx1,dx2)-1;
			imax = Math.Max(dx1,dx2)+1;
			jmin = Math.Min(dy1,dy2)-1;
			jmax = Math.Max(dy1,dy2)+1;

			if( ((imax-imin) > Def.MAX_GRID_POINTS)||((jmax-jmin) > Def.MAX_GRID_POINTS))
				visible = false;
			else
				visible = true;
			}

		public void Draw(Graphics g, Vista v, Pen pn)
			{
			if(active && visible)
				{
				int i,j;
				for(i=imin; i<imax; i++)
					{
					for(j=jmin; j<jmax; j++)
						{
						Point p = v.Scala( new Point2D(i*step* Math.Sign(v.Verso.X), j*step* Math.Sign(v.Verso.Y)) );
						g.DrawLine(pn,p.X-2,p.Y-2,p.X+2,p.Y+2);
						g.DrawLine(pn,p.X-2,p.Y+2,p.X+2,p.Y-2);
						}
					}
				}
			}
		}
	}
