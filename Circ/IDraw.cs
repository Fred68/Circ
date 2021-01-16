using System.Drawing;

namespace Circ
	{
	interface IDraw
		{
		void Draw(Vista v);			// Funzione per disegnare l'oggetto in una vista
		Point Center(Vista v);		// Coordinate del centro (su schermo)
		bool Selected {get;}		// Selezionato
		}
	}
