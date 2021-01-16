using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;

namespace Circ
	{
	public class Vista
		{

		readonly Panel p;						// Pannello su cui disegnare
		Graphics g;								// Oggetto per la grafica

		DisplayList dl;							// DisplayList
	
		Point cursor;							// Posizione del cursore nella vista
		int raggioSelezioneSq;					// Raggio di selezione standard (al quadrato)
		int raggioSq;							// Raggio di selezione al quadrato (nullo se non deve selezionare)
		Def.Shape filter;						// Filtro selezione

		Size szClient;							// Dimensioni del pannello su cui disegnare
		Point cenClient;						// e suo centro
		
		Point2D centro;							// Centro vista
		Point2D scalaXY;						// Scala
		Point2D szWorldTopLeft;					// Limiti della vista reali
		Point2D szWorldBottomRight;

		IEnumerable<Elemento> elEnum;			// Enumeratore degli elelemti nella lista degli oggetti grafici

		#region PROPRIETÀ PUBBLICHE
		public Graphics G
			{
			get {return g;}
			}
		
		public Point CenClient
			{
			get {return cenClient;}
			}

		public Point2D Centro
			{
			get {return centro;}
			}
		
		public Tuple<Point2D,Point2D> SzWorld
			{
			get {return new Tuple<Point2D,Point2D>(szWorldTopLeft,szWorldBottomRight);}
			}

		public Point2D ScalaXY
			{
			get {return scalaXY;}
			}
			
		public Pen[] PEN
			{
			#warning Aggiungere controllo numero e penne standard
			get {return dl.PEN;}
			}
		public int Pens
			{
			get {return dl.PEN.Length;}
			}

		/*public*/ DisplayList DisplayList
			{
			get {return dl;}
			}
		
		#endregion
	
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="obj">Pannello a cui appartiene la vista</param>
		public Vista(Panel obj)
			{
			p = obj;

			#warning Aggiungere proprietà e controllo scala > epsilon
			centro = new Point2D(0,0);
			scalaXY = new Point2D(1,1);

			dl = new DisplayList();
			
			cursor = new Point(0,0);
			raggioSq = 0;
			raggioSelezioneSq = Def.RAGGIO_SELEZIONE*Def.RAGGIO_SELEZIONE;
			filter = Def.Shape.Tutti;

			Resize();
			}

		/// <summary>
		/// Traformazioni vista.
		/// IsDisplayListUpdated = false
		/// </summary>
		#region Trasformazioni vista: Resize, Zoom, Pan...

		public void Resize()
			{
			#if DEBUG
			LOG.Write("Resize");
			#endif
			szClient = p.ClientSize;
			cenClient = new Point(szClient.Width/2, szClient.Height/2);
			szWorldTopLeft = Scala(new Point(0,0));
			szWorldBottomRight = Scala(new Point(szClient.Width,szClient.Height));
			dl.IsUpdated = false;
			}

		public void Zoom(double x)
			{
			if((x > 0.05) && (x < 20))
				{
				#if DEBUG
				LOG.Write("Zoom()");
				#endif
				scalaXY = scalaXY * x;
				dl.IsUpdated = false;
				}
			}

		public void FitZoom(Dati dt)
			{
			Tuple<Point2D,Point2D> wrldExt = dt.GetExtension();
			Point2D szw = wrldExt.Item2 - wrldExt.Item1;
			Point2D cnw = (wrldExt.Item2 + wrldExt.Item1)/2;
			centro = cnw;													// Pan ottimale: nel centro
			Point2D szc = new Point2D(szClient.Width,szClient.Height);
			Point2D cnc = cenClient;
			Point2D scala_tmp = szw / szc;									// Scale in X e in Y
			double scala_opt = Math.Max(scala_tmp.X,scala_tmp.Y) * Def.ENLARGE_FIT_ZOOM;
			scalaXY.X = scalaXY.Y = 1/scala_opt;							// Scala ottimale
			dl.IsUpdated = false;
			return;
			}
		public void Pan(Point2D pan)
			{
			#if DEBUG
			LOG.Write("Pan()");
			#endif
			centro = centro - pan;
			dl.IsUpdated = false;
			}
		#endregion

		/// <summary>
		/// Segnala che la Display List è da aggiornare
		/// </summary>
		public void SetOutdatedDL()
			{
			dl.IsUpdated = false;
			}

		public void SetCursor(bool on, int x=0, int y=0, Def.Shape flt = Def.Shape.Tutti)
			{
			if(!on)
				{
				raggioSq = 0;
				filter = Def.Shape.None;
				}
			else
				{
				raggioSq = raggioSelezioneSq;
				cursor.X = x; cursor.Y = y;
				filter = flt;
				}
			}

		/// <summary>
		/// Imposta l'enumeratore degli elementi
		/// Genera un'eccezione se è null
		/// </summary>
		/// <param name="elementnumerator">IEnumerable<Elemento></param>
		public void SetElements(IEnumerable<Elemento> elementnumerator)
			{
			if(elementnumerator != null)
				{
				#if(DEBUG)
				LOG.Write(@"SetElements()");
				#endif
				elEnum = elementnumerator;
				}
			else
				{
				throw new Exception("L'enumeratore IEnumerable<Elemento> è nullo");
				}
			}

		/// <summary>
		/// Azzera l'enumeratore degli elementi
		/// e la Display List
		/// </summary>
		public void ClearElements()
			{
			#if(DEBUG)
			LOG.Write(@"ClearElements()");
			#endif
			elEnum = null;
			dl.Reset();
			}

		/// <summary>
		/// Aggiunge un elemento alla Display List
		/// </summary>
		/// <param name="element">Rif. all'Elemento</param>
		/// <param name="shape"></param>
		/// <param name="colour"></param>
		/// <param name="x1">Primo punto</param>
		/// <param name="y1"></param>
		/// <param name="x2">econdo punto</param>
		/// <param name="y2"></param>
		/// <param name="cx">Centro</param>
		/// <param name="cy"></param>
		public void AddDL(Elemento element, Def.Shape shape, Def.Colori colour, int x1, int y1, int x2, int y2, int cx, int cy)
			{
			dl.Add(element, shape, colour, x1, y1, x2, y2, cx, cy);
			}

		/// <summary>
		/// Rigenera la DisplayList, se non è aggiornata.
		/// Ridisegna la vista, se richiesto
		/// Aggiorna l'enumeratore degli elementi, se presente
		/// </summary>
		/// <param name="redraw">true per ridisegnare la vista</param>
		/// <param name="elementEnumerator"></param>
		public void RegenDL(bool redraw, IEnumerable<Elemento> elementEnumerator = null)
			{
			if(elementEnumerator != null)
				{
				SetElements(elementEnumerator);
				}
			dl.indxList.Clear();
			if(elEnum != null)
				{
				#if(DEBUG)
				LOG.Write(@"RegenDL()");
				#endif
				if(!dl.IsUpdated)
					{
					dl.Clear();
					foreach (Elemento e in elEnum)
						{
						e.Draw(this);
						}
					dl.IsUpdated = true;
					}
				}
			if(redraw)
				{
				Redraw();
				}
			}

		/// <summary>
		/// Ridisegna il contenuto della vista
		/// senza aggiornare la Display List
		/// Chiama DisplayList.Play(), che aggiorna gli elementi evidenziati
		/// </summary>
		public void Redraw(bool clear = true)
			{
			#if(DEBUG)
			LOG.Write(@"Redraw()");
			#endif
			g = p.CreateGraphics();
			if(clear)	g.Clear(Def.ColourBackground);
			g.DrawRectangle(PEN[(int)Def.Colori.Black],1,1,p.Width-2,p.Height-2);						// Cornice

			#if(DEBUG)
			g.DrawLine(PEN[(int)Def.Colori.Blue],cenClient.X,cenClient.Y-5,cenClient.X,cenClient.Y+5);	// Centro client
			g.DrawLine(PEN[(int)Def.Colori.Blue],cenClient.X-5,cenClient.Y,cenClient.X+5,cenClient.Y);
			g.DrawEllipse(PEN[(int)Def.Colori.Blue],Scala(centro).X-5,Scala(centro).Y-5,10,10);			// Centro vista
			#endif

			Point origin = Scala(Point2D.Zero);
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X,origin.Y-5,origin.X,origin.Y+5);				// Origine world
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X-5,origin.Y,origin.X+5,origin.Y);

			dl.Play(g, ref cursor, raggioSq, filter);													// Disegna la D.L.



			g.Dispose();
			}

		#region Trasformazioni di scala da World a Client e viceversa
		
		/// <summary>
		/// Scala da World a Client
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public Point Scala(Point2D p)
			{
			Point pt = (Point)((p-centro)*scalaXY);
			return new Point(pt.X + cenClient.X,pt.Y + cenClient.Y);
			}

		/// <summary>
		/// Scala da Client a World
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public Point2D Scala(Point p)
			{
			Point pt = new Point(p.X-cenClient.X, p.Y-cenClient.Y);
			return pt/scalaXY + centro;
			}

		/// <summary>
		/// Scala da Client a World il vettore (trascura le origini)
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public Point2D ScalaVettore(Point p)
			{
			return p/scalaXY;
			}
		#endregion

		/// <summary>
		/// Restituisce uno gli ultimi elementi evidenziati (il primo della lista)
		/// </summary>
		/// <returns>Elemento oppure null</returns>
		public Elemento GetLastHighLighted()
			{
			return dl.GetLastHighLighted();
			}

		}
	}
