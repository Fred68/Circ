using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circ
	{

	public class StatoAttivo
		{
		
		Def.Stat stat;
		bool dragging;

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
				}
			}

		public StatoAttivo()
			{
			stat = Def.Stat.Vista;
			dragging = false;
			}

		}
	}
