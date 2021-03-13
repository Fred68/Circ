using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phys				// Namespace per grandezze fisiche
	{
	/// <summary>
	/// Interfaccia per i dati principali di calcolo per un nodo
	/// Tutti i calcoli (gas, liquido, elettricità) si devono uniformare a questo standard
	/// </summary>
	interface IPhysNodo
		{
		double E {get;}			// E: potenziale, pressione, altezza piezometrica
		}
	}
