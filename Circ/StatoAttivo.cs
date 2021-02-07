using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;				// Point

using Fred68.Tools.Log;

namespace Circ
	{

	public class StatoAttivo
		{
		
		Def.Stat stat;
		bool dragging;

		public Point dragIniRel;		// Punto inizio drag relativo (azzerato in Pan ad ogni mouse move=
		public Point dragIniFix;		// Punto inizio drag fisso
		public Elemento dragFromElement;		// Elemento da cui inizia il drag

		public Def.Stat Stato
			{
			get
				{
				return stat;
				}
			set
				{
				#if(DEBUG)
				LOG.Write($"Stato()={stat.ToString()}");
				#endif
				stat = value;
				dragging = false;		// Ad ogni cambio di stato, disattiva sempre il dragging
				}
			}
		public bool Dragging
			{
			get
				{
				return dragging;
				}
			set
				{
				dragging = value;
				if(dragging ==false)	dragFromElement = null;
				}
			}

		public StatoAttivo()
			{
			stat = Def.Stat.Vista;
			dragging = false;
			dragFromElement = null;
			}

		}
	}
