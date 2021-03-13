using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phys
	{
	class PhNodo : IPhysNodo
		{
		double e;
		public double E
			{
			get {return e;}
			set {e = value;}
			}

		public PhNodo()
			{
			e = 0.0;
			}
		}
	}
