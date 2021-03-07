using System.Drawing;

namespace Circ
	{
	interface IDraw
		{
		void Regen(Vista v, bool addToDisplayList);				// Funzione per ricalcolare la scala e inserire l'oggetto nella display-list di una vista

		void Draw(Graphics g, Vista v, bool high, bool sel);	// Funzione per disegnare l'oggetto. I colori sono contenuti nella vista
		Point Center { get; }									// Proprietà centro dell'oggetto
		Def.ClipFlag Clip(Vista v);								// Calcola il clipping
		System.Drawing.Drawing2D.Matrix Trasform { get; }		// Matrice di traslazione e rotazione (al punto medio)

		}
	}
