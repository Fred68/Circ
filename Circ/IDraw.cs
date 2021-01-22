using System.Drawing;

namespace Circ
	{
	interface IDraw
		{
		void Regen(Vista v);			// Funzione per inserire l'oggetto nella display-list di una vista
		void Draw(Graphics g, Pen pn, Brush br, Font fn);			// Funzione per disegnare l'oggetto
		Point Center {get; }			// Proprietà centro dell'oggetto
		Def.ClipFlag Clip(Vista v);		// Calcola il clipping
		}
	}
