using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;


namespace Circ
	{
	public static class Def
		{
		/// <summary>
		/// Indici dei colori
		/// La dimensione deve corrispondere a pen[] e brush[]
		/// </summary>
		public enum Colori					// Indici dei colori 
			{
			Selected,						// Selezionato
			Highlighted,					// Evidenziato
			Black, Red, Green, Blue			// Gli altri colori
			};

		#warning Aggiungere vari tipi di nodo o di ramo (es.: resistenza, diodo, tubo, ecc...)

		/// <summary>
		/// Nomi degli oggetti grafici, delle forme nella display list e dei flag per gli evidenziati
		/// </summary>
		[Flags]
		public enum Shape 
			{
			None		=	0,
			Nodo		=	1 << 0,
			Ramo		=	1 << 1,
			Cornice		=	1 << 2,
			Tutti		=	-1
			};

		[Flags]
		public enum Stat				// Stato inserimento e filtro per scelta Elementi
			{							//	Stato		Filtro		Altro
			None		=	0,			//	-			nulla
			Edit		=	1 << 0,		//	Edit		-
			Nodi		=	1 << 1,		//	Nodi		Nodi
			Rami		=	1 << 2,		//	Rami		Rami
			Vista		=	1 << 3,		//	Vista		-
			//Pan			=	1 << 4,		//	Pan		-		ANNULLATO: usare flag dragging in Mainform
			//Drag		=	1 << 5,			//	Drag	-		ANNULLATO: usare flag dragging in Mainform	
			Tutti		=	-1			//	-			Tutto
			}

		[Flags]
		public enum ClipFlag
			{
			Inside		=	0b0000,		// Inside: 0
			Left		=	0b0001,
			Right		=	0b0010,
			Bottom		=	0b0100,
			Top			=	0b1000,
			Outside		=	-1			// All Ones...
			}


		public enum InputType {String, Int, Double}

		public static readonly int RAGGIO_SELEZIONE = 10;		// Raggio di selezioni per evidenziazione
		public static readonly int DRG_MIN = 5;					// Minima distanza di trascinamento

		public static int FREE_MIN = 5;							// Spazio libero minimo e massimo dell'array della Display List [10]
		public static int FREE_MAX = 10;						// [100]

		public static uint NUM_NODI_MAX = 10000;				// Numero massimo di elementi
		public static uint NUM_RAMI_MAX = 10000;
		public static uint ID_NODI_MAX = 20000;					// Max ID
		public static uint ID_RAMI_MAX = 20000;

		public static double EPSILON = double.Epsilon * 10;		// Epsilon per i calcoli
		public static double MAX_VALUE = double.MaxValue/10;	// Valore massimo
		public static double MIN_VALUE = double.MinValue/10;	// Valore minimo

		public static double ZOOM_DEFAULT_SIZE = 5;				// Semiampiezza finestra di zoom se un solo punto
		public static double ZOOM_FIT_ENLARGE = 1.2;			// Ingrandimento finestra oltre lo zoom ottimale
		public static double ZOOM_STEP = 1.1;					// Incremento dello zoom

		public static string FONT_NAME = "Arial";				// Carattere nella finestra grafica
		public static int FONT_SIZE = 8;

		public static string FONT_D_NAME = "Arial";				// Carattere nelle dialog box
		public static int FONT_D_SIZE = 9;

		public static int NODE_HALFSIZE = 3;					// Dimensioni del nodo disegnato

		public static Color ColourSelected = Color.Cyan;		// Colori di default
		public static Color ColourHighlighed = Color.Yellow;
		public static Color ColourBackground = Color.Black;
		public static Color ColourNodo = Color.LightGray;
		public static Color ColourRamo = Color.Red;

		public static int TIMER_REFRESH = 300;				// Intervallo di refresh di alcune scritte (tra cui il nome file da salvare)
		}
	}
	 