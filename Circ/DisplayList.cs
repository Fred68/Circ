using Fred68.Tools.Log;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Circ
	{

	public class DisplayList
		{
		private struct Command
			{
			public Elemento elemento;       // Puntatore all'elemento originario
			public Def.Shape shape;         // Forma
			public Point p1;                // Punto 1
			public Point p2;                // Punto 2
			//public Stile.Colore colour;    // Colore
			public Point center;            // Centro
			}

		Vista vista;				// Vista (contenente i dati grafici)
		Command[] com;              // Comandi della display list
		int count;                  // Numero di elementi

		bool isDLU;                 // La display list è aggiornata ?
		public bool IsUpdated
			{
			get { return isDLU; }
			set { isDLU = value; }
			}

		public readonly List<int> indxList; // Lista indici evidenziati nell'ultimo Play()

		/// <summary>
		/// Costruttore
		/// </summary>
		public DisplayList(Vista v)
			{

			vista = v;
			indxList = new List<int>();         // Crea la lista degli indici

			/*
			brush = new Brush[]
				{
				new SolidBrush(Def.ColourSelected),			// Selected
				new SolidBrush(Def.ColourHighlighed),		// Highlighted
				new SolidBrush(Def.ColourNodo),				// Nodi
				new SolidBrush(Def.ColourRamo),				// Rami
				new SolidBrush(Color.Green),
				new SolidBrush(Color.Blue),
				new SolidBrush(Def.ColourBackground)
				}; 
			pen = new Pen[]
				{
				new Pen(Def.ColourSelected, 1),				// Come sopra
				new Pen(Def.ColourHighlighed, 1),			
				new Pen(Def.ColourNodo, 1),
				new Pen(Def.ColourRamo, 1),
				new Pen(Color.Green, 1),
				new Pen(Color.Blue, 1),
				new Pen(Def.ColourBackground)
				};
			font = new Font[]
				{
				new Font(Def.FONT_NAME, Def.FONT_SIZE)
				};

			if(pen.Length != Enum.GetNames(typeof(Def.Colori)).Length)
				throw new Exception("Pen array and Color enum do not match");
			if(brush.Length != Enum.GetNames(typeof(Def.Colori)).Length)
				throw new Exception("Brush array and Color enum do not match");
			*/

			com = new Command[Def.FREE_MAX];                // Array
#if(DEBUG)
			LOG.Write($"DisplayList.Command[{com.Length}]");
#endif
			count = 0;
			}

		/// <summary>
		/// Accesso indicizzato
		/// </summary>
		/// <param name="i">Indice da 0 a count</param>
		/// <returns></returns>
		public Elemento this[int i]
			{
			get
				{
				Elemento e = null;
				if((i >= 0) && (i < count))
					e = com[i].elemento;
				return e;
				}
			}

		/// <summary>
		/// Count
		/// </summary>
		public int Count
			{
			get { return count; }
			}

		/// <summary>
		/// Distanza al quadrato tra due Point
		/// </summary>
		/// <param name="p1">Point</param>
		/// <param name="p2">Point</param>
		/// <returns>Distanza al quadrato</returns>
		public static int DistanceSquared(Point p1,Point p2)
			{
			return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
			}

		/// <summary>
		/// Aggiunge un elemento alla Display List
		/// Ridimensiona array se necessario
		/// </summary>
		/// <param name="shape">Forma</param>
		/// <param name="colour">Colore</param>
		/// <param name="x1">Primo punto: X...</param>
		/// <param name="y1">...e Y</param>
		/// <param name="x2">Secondo punto: X...</param>
		/// <param name="y2">...e Y</param>
		public void Add(Elemento element, Def.Shape shape, int cx, int cy)
			{
			com[count].elemento = element;
			com[count].shape = shape;
			//com[count].colour = colour;
			com[count].center.X = cx;
			com[count].center.Y = cy;
			if(element != null)
				count++;                // Aggiunge all'array solo le l'elemento non è nullo
			if(com.Length - count < Def.FREE_MIN)
				{
				Array.Resize<Command>(ref com,count + Def.FREE_MAX);
#if(DEBUG)
				LOG.Write($"DisplayList.Command[{com.Length}]+");
#endif
				}
			return;
			}

		/// <summary>
		/// Svuota la Display List
		/// </summary>
		public void Clear()
			{
			count = 0;
			}

		/// <summary>
		/// Svuota la Display List e la riduce
		/// </summary>
		public void Reset()
			{
			count = 0;
			indxList.Clear();
			if(com.Length > Def.FREE_MAX)
				{
				Array.Resize<Command>(ref com,Def.FREE_MAX);
#if(DEBUG)
				LOG.Write($"DisplayList.Command[{com.Length}]-");
#endif
				}
			}

		/// <summary>
		///  Disegna le forma nella Display List nella supericie grafica GDI+
		///  evidenziando gli oggetti filtrati più vicini al cursore
		///  e aggiungendo gli indici alla lista temporanea
		/// </summary>
		/// <param name="g">Oggetto per grafica gdi+</param>
		/// <param name="p">ref Point del cursore (Point è una struct)</param>
		/// <param name="distanceSq">Raggio di selezione al quadrato</param>
		/// <param name="flags">Shape flag mask</param>
		/// <returns>Numero degli oggetti evidenziati</returns>
		public int Play(Graphics g,ref Point p,int distanceSq,Def.Shape flags = Def.Shape.Tutti)
			{
			indxList.Clear();

			if(g == null) return 0;

			bool sel, high;

			for(int i = 0; i < count; i++)
				{
				sel = com[i].elemento.Selected;
				high = false;
				if(distanceSq > 0)                                                                      // Evidenziato
					{
					if((com[i].shape & flags) != 0)         // Filtro per tipo di forma
						{
						if(DistanceSquared(com[i].center,p) < distanceSq)
							{
							high = true;
							indxList.Add(i);
							}
						}
					}
				com[i].elemento.Draw(g, vista, high, sel);     // Disegna l'elemento su schermo graphics
				}
			return indxList.Count;
			}

#warning Aggiungere funzione per stampare proprietà aggiuntive: ID, Nome ecc... con Filtro flag aggiuntivo

		/// <summary>
		/// Restituisce uno degli ultimi elementi evidenziati (il primo della lista)
		/// </summary>
		/// <returns>Elemento oppure null</returns>
		public Elemento GetLastHighLighted()
			{
			Elemento el = null;
			if(indxList.Count > 0)
				{
				el = this[indxList[0]];
				}
			return el;
			}
		}
	}
