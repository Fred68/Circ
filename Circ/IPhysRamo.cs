using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phys				// Namespace per grandezze fisiche
	{
	/// <summary>
	/// Interfaccia per i dati principali di calcolo per un ramo
	/// Tutti i calcoli (gas, liquido, elettricità) si devono uniformare a questo standard
	/// </summary>
	interface IPhysRamo
		{
		double I {get;}			// I corrente, portata
		double V {get;}			// Differenza di potenziale, pressione, altezza piezometrica ecc...
		double G {get;}			// Conduttanza (Ohm, bar*s/m3, s/m2 ecc...)
		}
	}
