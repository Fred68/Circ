using System;

using System.Drawing;

namespace Circ
	{
	public class Stile
		{

		public enum Colore                  // Indici dei colori 
			{
			Selected,                       // Selezionato
			Highlighted,                    // Evidenziato
			Background,
			Nodo,
			Ramo,
			Origine,
			Griglia,
			};

		public static Color BackgroundColor { get; } = Color.FromArgb(20,20,20);

		public Pen[] PEN { get; }       // Penne
		public Brush[] BRUSH { get; }   // Pennelli
		public Font[] FONT { get; }		// Caratteri

		/// <summary>
		/// Costruttore
		/// </summary>
		public Stile()
			{
			int N = Enum.GetNames(typeof(Stile.Colore)).Length;        // Numero di colori

			PEN = new Pen[]
				{
				Pens.Cyan,						// Select
				Pens.Yellow,					// Highlight
				new Pen(BackgroundColor,1),
				Pens.LightGray,
				Pens.DarkGreen,
				Pens.Red,
				Pens.DarkBlue
				};


			BRUSH = new Brush[]
				{
				Brushes.Cyan,					// Select
				Brushes.Yellow,					// Highlight
				new SolidBrush(BackgroundColor),
				Brushes.LightGray,
				Brushes.DarkGreen,
				Brushes.Red,
				Brushes.DarkBlue
				};

			FONT = new Font[]
				{
				new Font(Def.FONT_NAME, Def.FONT_SIZE)
				};


			if(PEN.Length != N) throw new Exception("Le dimensioni di PEN[] sono diverse da enum Colore.");
			if(PEN.Length != N) throw new Exception("Le dimensioni di BRUSH[] sono diverse da enum Colore.");


			}

		}
	}
