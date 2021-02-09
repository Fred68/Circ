using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;

using Fred68.Tools.Log;

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
		Point	verso;							// +-1 world rispetto allo schermo (che è positivo vs. dx e vs. basso).
		Point2D szWorldTopLeft;					// Limiti della vista reali
		Point2D szWorldBottomRight;

		IEnumerable<Elemento> elEnum;			// Enumeratore degli elementi nella lista degli oggetti grafici

		Griglia grid;

		#region PROPRIETÀ PUBBLICHE
		
		/// <summary>
		/// Oggetto grafico
		/// </summary>
		public Graphics G
			{
			get {return g;}
			}
		
		/// <summary>
		/// Centro della Client Area
		/// </summary>
		public Point CenClient
			{
			get {return cenClient;}
			}

		/// <summary>
		/// Centro della vista
		/// </summary>
		public Point2D Centro
			{
			get {return centro;}
			}
		
		/// <summary>
		/// Limiti della vista in coordinate World
		/// </summary>
		public Tuple<Point2D,Point2D> SzWorld
			{
			get {return new Tuple<Point2D,Point2D>(szWorldTopLeft,szWorldBottomRight);}
			}

		/// <summary>
		/// Fattore di scala
		/// </summary>
		public Point2D ScalaXY
			{
			get {return scalaXY;}
			}
		
		/// <summary>
		/// Orientamento asse Y (1 in basso, -1 in alto)
		/// </summary>
		public Point2D Verso
			{
			get {return verso;}
			}
		public Pen[] PEN
			{
			#warning Aggiungere controllo numero e penne standard
			get {return dl.PEN;}
			}

		/// <summary>
		/// Array delle penne
		/// </summary>
		public int Pens
			{
			get {return dl.PEN.Length;}
			}

		/// <summary>
		/// Limiti dell'area visualizzata in coordinate World
		/// </summary>
		public Point2D SzWorldTopLeft
			{get {return szWorldTopLeft;}}
		public Point2D SzWorldBottomRight
			{get {return szWorldBottomRight;}}

		/// <summary>
		/// Restituisce lo stato della griglia
		/// </summary>
		public bool IsGridOn
			{get {return grid.Active;}}
		/// <summary>
		/// Imposta il passo della griglia
		/// </summary>
		public double GridStep
			{
			get {return grid.Step;}
			set {grid.Step=value;}
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
			verso = new Point(1,-1);

			dl = new DisplayList();
			
			cursor = new Point(0,0);
			raggioSq = 0;
			raggioSelezioneSq = Def.RAGGIO_SELEZIONE*Def.RAGGIO_SELEZIONE;
			filter = Def.Shape.Tutti;

			grid = new Griglia();

			Resize();
			}

		#region TRASFORMAZIONI VISTA: Resize, Zoom, Pan... IsDisplayListUpdated = false

		public void Resize()
			{
			#if DEBUG
			LOG.Write("Resize");
			#endif
			szClient = p.ClientSize;
			cenClient = new Point(szClient.Width/2, szClient.Height/2);
			RecalcSzWlorld();
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
				RecalcSzWlorld();
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
			double scala_opt = Math.Max(scala_tmp.X,scala_tmp.Y) * Def.ZOOM_FIT_ENLARGE;
			scalaXY.X = scalaXY.Y = 1/scala_opt;							// Scala ottimale
			RecalcSzWlorld();
			dl.IsUpdated = false;
			return;
			}
		public void Pan(Point2D pan)
			{
			#if DEBUG
			LOG.Write("Pan()");
			#endif
			centro = centro - pan;
			RecalcSzWlorld();
			dl.IsUpdated = false;
			}
		#endregion

		/// <summary>
		/// Ricalcola le coordinate World della finestra della vista e la griglia
		/// (per il clipping)
		/// </summary>
		public void RecalcSzWlorld()
			{
			szWorldTopLeft = Scala(new Point(0,0));
			szWorldBottomRight = Scala(new Point(szClient.Width,szClient.Height));
			grid.Recalc(this);
			}

		/// <summary>
		/// Segnala che la Display List è da aggiornare
		/// </summary>
		public void SetOutdatedDL()
			{
			dl.IsUpdated = false;
			}

		/// <summary>
		/// Abilita o disabilita l'evidenziazione degli elementi, di forma Def.Shape (flags), vicini al cursore
		/// </summary>
		/// <param name="on">true on, false off</param>
		/// <param name="x">x cursore</param>
		/// <param name="y">x cursore</param>
		/// <param name="flt">Def.Shape filtro</param>
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
				throw new Exception("L'enumeratore IEnumerable<Elemento> non può essere nullo");
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
		public void AddDL(Elemento element, Def.Shape shape, Def.Colori colour, int cx, int cy)
			{
			dl.Add(element, shape, colour, cx, cy);
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
			filter = Def.Shape.Tutti;
			dl.indxList.Clear();
			if(elEnum != null)
				{
				#if(DEBUG)
				LOG.Write(@"RegenDL()");
				uint _debug_clipped = 0;
				#endif
				if(!dl.IsUpdated)
					{
					dl.Clear();
					ClipElementi();
					
					foreach (Elemento e in elEnum)
						{
						if(e.Clipped == Def.ClipFlag.Inside)
							{
							e.Regen(this);
							}
						else
							{
							#if(DEBUG)
							_debug_clipped++;
							#endif
							}
						}
					dl.IsUpdated = true;
					}
				#if(DEBUG)
				if(_debug_clipped > 0)	LOG.Write(@"RegenDL(): eseguito clip su "+_debug_clipped.ToString() + " elementi");
				#endif
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
			g.DrawRectangle(PEN[(int)Def.Colori.Black],1,1,p.Width-2,p.Height-2);		// Cornice
			grid.Draw(g,this,PEN[(int)Def.Colori.Blue]);
			DrawWorldAxes(g);															// Assi
			dl.Play(g, ref cursor, raggioSq, filter);									// Disegna la D.L.
			g.Dispose();
			}
		

		/// <summary>
		/// Disegna gli assi X Y world sullo schermo
		/// </summary>
		/// <param name="g"></param>
		private void DrawWorldAxes(Graphics g)
			{
			Point origin = Scala(Point2D.Zero);															// Origine world
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X,origin.Y-5*verso.Y,origin.X,origin.Y+15*verso.Y);
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X-3*verso.X,origin.Y+10*verso.Y,origin.X,origin.Y+15*verso.Y);
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X+3*verso.X,origin.Y+10*verso.Y,origin.X,origin.Y+15*verso.Y);

			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X-5*verso.X,origin.Y,origin.X+15*verso.X,origin.Y);
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X+10*verso.X,origin.Y-3*verso.Y,origin.X+15*verso.X,origin.Y);
			g.DrawLine(PEN[(int)Def.Colori.Red],origin.X+10*verso.X,origin.Y+3*verso.Y,origin.X+15*verso.X,origin.Y);

			#if(DEBUG)
			g.DrawLine(PEN[(int)Def.Colori.Blue],cenClient.X,cenClient.Y-5,cenClient.X,cenClient.Y+5);	// Centro client
			g.DrawLine(PEN[(int)Def.Colori.Blue],cenClient.X-5,cenClient.Y,cenClient.X+5,cenClient.Y);
			g.DrawEllipse(PEN[(int)Def.Colori.Blue],Scala(centro).X-5,Scala(centro).Y-5,10,10);			// Centro vista
			#endif
			}

		/// <summary>
		/// Inverte la direzione dell'asse X
		/// </summary>
		public void SwapAxisX()
			{
			verso.X = - verso.X;
			Resize();
			SetOutdatedDL();
			}

		/// <summary>
		/// Inverte la direzione dell'asse Y
		/// </summary>
		public void SwapAxisY()
			{
			verso.Y = - verso.Y;
			Resize();
			SetOutdatedDL();
			}

		#region TRASFORMAZIONI DI SCALA da World a Client e viceversa
		
		/// <summary>
		/// Scala da World a Client
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public Point Scala(Point2D p)
			{
			Point pt = (Point)((p-centro)*scalaXY*verso);
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
			return verso*(pt/scalaXY) + centro;
			}

		/// <summary>
		/// Scala da Client a World il vettore (trascura le origini)
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public Point2D ScalaVettore(Point p)
			{
			return verso*(p/scalaXY);
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

		/// <summary>
		/// Verifica se un punto è nella finestra visibile
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public Def.ClipFlag IsInsideWorld(Point2D p)
			{
			Def.ClipFlag pos = Def.ClipFlag.Inside;
			if((p.X - szWorldTopLeft.X)*verso.X < 0)
				pos |= Def.ClipFlag.Left;
			else if((p.X - szWorldBottomRight.X)*verso.X > 0)
				pos |= Def.ClipFlag.Right;

			if((p.Y - szWorldTopLeft.Y)*verso.Y < 0)
				pos |= Def.ClipFlag.Top;
			else if((p.Y - szWorldBottomRight.Y)*verso.Y > 0)
				pos |= Def.ClipFlag.Bottom;		

			return pos;
			}

		/// <summary>
		/// Esegue il clipping di tutti gli elementi
		/// con riferimento alla vista (this)
		/// </summary>
		/// <param name="v"></param>
		void ClipElementi()
			{
			// Già verificato (elEnum != null) prima della chiamata, in RegenDL(...)
			filter = Def.Shape.Nodo;			// Prima i nodi
			foreach (Elemento e in elEnum)
				{
				e.Clip(this);
				}
			filter = Def.Shape.Ramo;			// Poi i rami
			foreach (Elemento e in elEnum)
				{
				e.Clip(this);
				}
			}

		/// <summary>
		/// Cambia lo stato della griglia
		/// </summary>
		public void ToggleGriglia()
			{
			grid.Active = !(grid.Active);
			}

		/// <summary>
		/// Cambia il passo della griglia
		/// </summary>
		/// <param name="mul"></param>
		public void GrigliaStepMultiply(double mul)
			{
			grid.Step = grid.Step * mul;
			}
		}
	}
