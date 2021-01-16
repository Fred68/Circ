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
		Def.Stat oper;
		
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
				}
			}

		public StatoAttivo()
			{
			stat = Def.Stat.Vista;
			oper = Def.Stat.None;
			}

		}
	}
