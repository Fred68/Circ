using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace Circ
	{
	public class Colori
		{

		public enum Colore					// Indici dei colori 
			{
			Selected,						// Selezionato
			Highlighted,					// Evidenziato
			Background,
			Nodo,
			Ramo,
			Origine,
			Griglia,		
			};
		
		public static Color BackgroundColor {get;} = Color.FromArgb(20,20,20);

		public Pen[] PEN {get;}		// Penne
		public Brush[] BRUSH {get;}	// Pennelli
		public Font[] FONT {get;}	// Caratteri

		public Colori()
			{
			int N =  Enum.GetNames(typeof(Colori.Colore)).Length;		// Numero di colori

			PEN = new Pen[]
				{
				Pens.Cyan,
				Pens.Yellow,
				new Pen(BackgroundColor,1),
				Pens.LightGray,
				Pens.DarkGreen,
				Pens.Red,
				Pens.DarkBlue
				};

			


			BRUSH = new Brush[]
				{
				Brushes.Cyan,
				Brushes.Yellow,
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


			if(PEN.Length != N)		throw new Exception("Le dimensioni di PEN[] sono diverse da enum Colore.");
			if(PEN.Length != N)		throw new Exception("Le dimensioni di BRUSH[] sono diverse da enum Colore.");


			}
			
		}
	}
