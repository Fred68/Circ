using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phys
	{
	public class PhRamo : IPhysRamo
		{
		double i;
		double v;
		double g;

		public double I
			{
			get {return i;}
			set {i = value;}
			}
		public double V
			{
			get {return v;}
			set {v = value;}
			}
		public double G
			{
			get {return g;}
			set {g = value;}
			}

		public PhRamo()
			{
			i = 0.0;
			v = 0.0;
			g = 1e-10;
			}
		}
	}
