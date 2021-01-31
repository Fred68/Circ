using System;
using System.Collections.Generic;
using System.Text;


/*

ATTENZIONE: TUTTO DA RIFARE.

RAGIONEVOLE USARE UN MISTO TRA SPEEPEST DESCENT E BISEZIONE

Notazione
double f(Matrix x)		La funzione
Matrix x				Il vettore (colonna)
xk, xk1					x(k) e x(k+1) passo e passo successivo

Argomenti:
Matrix guess			Punto iniziale
ref Matrix xmin			Punto di minimo (soluzione)
double beta				Passo per avvio bisezione
double toll				Condizione di uscita su distanza tra xk e xk1
double prec				Condizione di uscita su differenza (assoluta) tra f(xk) e f(xk1)
int iterbisez			Iterazioni massime
int	itergrad			Iterazioni massime

	-	Azzera								i=0
	-	Parte da guess						xk = guess
	-	Ricalcolare gradiente				dk=grad(xk)
	-	Verifica condizione uscita sul gradiente
	-	Avvio bisezione:
	
	Con beta passo fisso, non e` detto che l'intervallo xk...xk+beta*dk contenga il minimo.
	La bisezione deve avere come criteri di arresto la differenza tra aj e bj e la differenza tra f(aj) e f(bj).
	Poi, all'uscita della bisezione, di devono ricalcolare il gradiente e le condizioni di uscita.
	
	
	CONDIZIONI DI USCITA:
	|| xk1-xk ||  < prec
	|| grad(xk) || < eps1
	|| f(xk1) - f(xk) || < toll
	
*/





namespace Fred68.Tools.Matematica
	{
	/// <summary>
	/// Class to find the minimum of a function of one or more variables
	/// with gradient and fixed step
	/// </summary>

	delegate double FunzioneDelegate(Matrix x);		// Dichiara un delegate

	class Fmin 
		{
		#region COSTANTI
		private const double epsilon = 1e-10;					// Per calcolo gradiente. Se troppo piccolo: x + epsilon = x
		#endregion	
		#region PROTECTED
		FunzioneDelegate Funzione;
		#endregion
		#region PROPRIETA
		public static double Epsilon { get { return epsilon; } }
		#endregion
		#region COSTRUTTORE
		public Fmin()
			{
			Clear();
			}
		#endregion
		#region FUNZIONI
		public void Clear()											// Inizializza
			{														
			Funzione = null;
			}
		public void SetFunction(FunzioneDelegate f)
			{
			Funzione = f;
			}
		public double Calcola(Matrix m)
			{
			return Funzione(m);
			}
		public Matrix Gradiente(Matrix x, int indx, double delta = epsilon)
			{
			Matrix g = new Matrix(x,false);						// Gradiente da calcolare
			Matrix dk, xkdk;
			if(indx < 0)
				{
				for(int i=0; i<x.Row; i++)
					{
					dk = Matrix.Zeros(x.Row,x.Col);					// Matrice con il passo delta sulla sola variabile i
					dk.Set(i,0,delta);
					xkdk = x + dk;
					double fx = Funzione(x);
					double fxdk = Funzione(x + dk);
					g.Set(i,0, (fxdk-fx)/delta);					// Imposta la riga i del gradiente
					}
				}
			else
				{
				dk = Matrix.Zeros(x.Row,x.Col);					// Matrice con il passo delta sulla sola variabile i
				dk.Set(indx,0,delta);
				xkdk = x + dk;
				double fx = Funzione(x);
				double fxdk = Funzione(x + dk);
				g.Set(indx,0, (fxdk-fx)/delta);					// Imposta la riga i del gradiente
				}
			return g;
			}
		/// <summary>
		/// Minimo con il metodo del gradiente e del passo fisso.
		/// ALGORITMO ERRATO. In genere non converge.
		/// </summary>
		/// <param name="guess"></param>
		/// <returns></returns>
		public bool TrovaMinimo1(Matrix guess, ref Matrix xmin, double beta, double toll, double prec, ref int iterazioni)
			{
			bool trovato = false;
			int i;
			Matrix xk, dk, xk1;
			xk = guess.Copy();											// Impostazioni iniziali
			xk1 = xk.Copy();
			double dist;

			for(i=0; i < iterazioni; i++)
				{
				dk = - Gradiente(xk,-1);
				Matrix scal = (!dk) * dk;
				dist = scal.Get(0,0);							// Modulo, se vettore lineare			

				if(dist > toll)									
					{
					Matrix xk1new = xk + beta * dk;
					xk = xk1.Copy();							// Ricalcola
					xk1 = xk1new.Copy();
					}
				else if(Math.Abs(Funzione(xk)-Funzione(xk1)) < prec)
					{
					trovato = true;
					iterazioni = i;
					xmin = xk.Copy();
					break;
					}
				}
			return trovato;
			}
		/// <summary>
		/// Metodo del rilassamento o di bisezione
		/// ALGORITMO ERRATO.
		/// </summary>
		/// <param name="guess"></param>
		/// <param name="xmin"></param>
		/// <param name="beta"></param>
		/// <param name="toll"></param>
		/// <param name="prec"></param>
		/// <param name="iterazioni"></param>
		/// <returns></returns>
		public bool TrovaMinimo2(Matrix guess, ref Matrix xmin, double beta, double toll, double prec, ref int iterazioni)
			{
			bool trovato = false;
			int i;
			Matrix xk, dk, xk1;
			xk = guess.Copy();											// Impostazioni iniziali
			xk1 = xk.Copy();
			double dist;

			for(i=0; i < iterazioni; i++)
				{
				int indx = i % guess.Row;
				dk = - Gradiente(xk,indx);
				Matrix scal = (!dk) * dk;
				dist = scal.Get(0,0);					// Modulo, se vettore lineare			

				if(dist > toll)									
					{
					Matrix xk1new = xk + beta * dk;
					xk = xk1.Copy();							// Ricalcola
					xk1 = xk1new.Copy();
					}
				else if(Math.Abs(Funzione(xk)-Funzione(xk1)) < prec)
					{
					trovato = true;
					iterazioni = i;
					xmin = xk.Copy();
					break;
					}
				}
			return trovato;
			}
/*

NOTA GENERALE.

I metodi precedenti, steepest descent, rilassamento (per la scelta delle direzioni) e
passo fisso e bisezione (per la ricerca del minimo (o del massimo)
non sono sufficienti da soli.

E' ragionevole scrivere prima alcune funzioni che lavorino separatamente.
Tutte sono incluse nella classe Fmin e hanno gia` implicito il delegate alla funzione
Tutte devono prima verificare che il delegate non sia nullo.

*/


	/// <summary>
	/// Ricerca per punti
	/// </summary>
	/// <param name="xk">Punto centrale</param>
	/// <param name="range">Meta` ampiezza di ricerca (ammessi valori nulli)</param>
	/// <param name="passiU">Numero di passi unilaterali (almeno 1)</param>
	/// <param name="xmin">Punto con il valore minore</param>
	/// <param name="cicli">Numero di punti calcolati</param>
	/// <returns></returns>
	public bool Campionamento(	Matrix xk,
								Matrix range,
								MatrixBase<int> passiU,
								ref Matrix xmin,
								ref int cicli)
		{
		bool found = false;						
		int n;														// Dimensioni dei vettori
		int i;														// Contatore
		n = xk.Row;
		if((range.Row != n) || (passiU.Row != n) || (xmin.Row != n) || (xk.Col != 1) || (range.Col != 1) || (passiU.Col != 1) || (xmin.Col != 1) )
			{
			return found;											// Verifica indici
			}
		for(i=0; i<n; i++)											// Verifica intervalli (ammessi valori nulli)
			{
			if(range.Get(i,0) < 0.0)
				{
				return found;
				}
			}
		for(i=0; i<n; i++)											// Verifica passi (almeno 1 per lato)
			{
			if(passiU.Get(i,0) < 1)
				{
				return found;
				}
			}
		MatrixBase<int> passi = new MatrixBase<int>(n,1);			// Matrice dei passi
		for(i=0; i<n; i++)											// Calcola i passi effettivi (segmenti, non punti)
			{
			passi.Set(i, 0, passiU.Get(i,0) * 2);
			}			
		Matrix step = new Matrix(n,1);
		for(i=0; i<n; i++)											// Calcola i passi effettivi
			{
			step.Set(i, 0, range.Get(i,0)/passi.Get(i,0));
			}			
		Matrix xo = new Matrix(n,1);
		for(i=0; i<n; i++)											// Calcola i punti di partenza
			{
			xo.Set(i, 0, xk.Get(i,0) - step.Get(i,0) * passiU.Get(i,0));
			}			
		MatrixBase<int> contatori = new MatrixBase<int>(n,1,0);		// Vettore dei contatotri (tutti a 0)
		Matrix x = new Matrix(n,1);									// Vettore dei valori effettivi				
		int iinc = -1;
		double minimo = double.MaxValue;
		double f;
		cicli = 0;
		bool fine = false;
		while(!fine)
			{
			if(iinc >= 0)											// ricalcola nuovo vettore x
				{
				x.Set(	iinc,0, xo.Get(iinc,0) + step.Get(iinc,0) * contatori.Get(iinc,0) );
				}
			else
				{
				for(i=0; i<n; i++)
					x.Set(	i,0, xo.Get(i,0) + step.Get(i,0) * contatori.Get(i,0) );	
				}
			f = Funzione(x);										// Calcola la f del punto x attuale
			if(f < minimo)											// Vede se e` minima (rispetto ai valori trovati finora)
				{
				minimo = f;
				xmin = x.Copy();
				found = true;
				}
			fine = !Incrementa(ref contatori, passi, ref iinc);
			cicli++;
			}
		return found;
		}

	/*
	RILASSAMENTO
	Da scrivere dopo aver completato Bisezione, PassoFisso e Grossolana
	*/

	/*
	STEEPEST
	Da scrivere dopo aver completato Bisezione, PassoFisso e Grossolana
	*/

	/*
	PASSOFISSO
	Analoga a Bisezione (stessi parametri o quasi...)
	Usa pero` il gradiente come condizione di uscita al posto della distanza (oltre alla f)
	*/

	/// <summary>
	/// Incrementa contatori
	/// </summary>
	/// <param name="contatori">Colonna (n,1) con i contatori</param>
	/// <param name="passi">Colonna (n,1) con i passi massimi</param>
	/// <param name="indIncrementato">Indice incrementato, oppure -1 se variato piu` di uno</param>
	/// <returns>true se ha incrementato un indice, false se era gia` l'ultimo passo</returns>
	public static bool Incrementa(	ref MatrixBase<int> contatori,
									MatrixBase<int> passi,
									ref int indIncrementato)
		{
		bool fine = false;						// Flag finale
		int n;									// Dimensione dei vettori
		n = contatori.Row;
		if((passi.Row != n) || (passi.Col!=1) || (contatori.Col != 1) )
			{
			return fine;						// Verifica gli indici
			}

		int indice = 0;							// Inizia con il primo indice
		indIncrementato = 0;					// Indice da ricalcolare (-1 se tutti)
		while(indice < n)
			{
			int c;
			c = contatori.Get(indice,0) + 1;	// Calcola indice incrementato
			if(c < passi.Get(indice,0))			// Se entro i limiti...
			    {
			    contatori.Set(indice,0,c);		// Aggiorna indice
			    if(indIncrementato != -1)		// Lo salva, per aggiornamento calcoli...
			        indIncrementato = indice;	// ... ma solo se non erano gia` cambiati piu` indici
			    fine = true;					// Raggiunta uscita prima della fine
			    break;							// Esce dal ciclo while
			    }
			else
			    {								// ...se superato i limiti
			    contatori.Set(indice,0,0);		// Azzera contatore
			    indice += 1;					// Passa all'indice successivo
			    indIncrementato = -1;
			    continue;						// Continua il ciclo while
			    }
			}									// Se esce normalmente dal while, fine resta true
		if(indice >= n)
			fine = false;						// Se superato ultimo indice: ultimo passo, false
		return fine;
		}

	/// <summary>
	/// Cerca il minimo lungo dk con il metodo di bisezione
	/// </summary>
	/// <param name="xk">Punto di partenza</param>
	/// <param name="dk">Vettore direzione</param>
	/// <param name="dknorm">true per normalizzare dk</param>
	/// <param name="beta">Moltiplicatore per intervallo di ricerca xk+beta*dk</param>
	/// <param name="toll">Uscita se ||x(k+1)-x(k)|| minore di toll</param>
	/// <param name="prec">Uscita se ||f(x(x+1))-f(xk) minore di prec</param>
	/// <param name="nmax">Cicli massimi di ricerca</param>
	/// <param name="xmin">Punto di minimo trovato</param>
	/// <param name="cicli">Cicli impiegati</param>
	/// <returns>true se converge, false se errore o raggiunti nmax cicli</returns>
	public bool Bisezione(	Matrix xk,
							Matrix dk,
							bool dknorm,
							double beta,
							double toll,
							double prec,
							int nmax,
							ref Matrix xmin,
							ref int cicli)
		{
		bool found = false;
		int n;											// Legge dimensione dei vettori e verifica gli indici
		n = xk.Row;
		if((dk.Row != n) || (xmin.Row != n) || (xk.Col != 1) || (dk.Col != 1) || (xmin.Col != 1) )
			{
			return found;						
			}
		Matrix[] a = new Matrix[5];						// Punti di ricerca a[0]=aj, a[1]=dj, a[2]=cj, a[3]=ej, a[4]=bj
		double[] f = new double[5];
		for(int i=0; i<n; i++)	a[i] = new Matrix(n,1);

		double mod = Math.Sqrt(Matrix.Sum(dk ^ dk));	// Calcola il modulo di dk
		if(mod < Epsilon)								// Se troppo piccolo...
			return found;								// ...esce
		if(dknorm)		dk = dk / mod;					// Se richiesto, normalizza dk
		a[0] = xk;										// Imposta estremi e calcola punto medio
		a[4] = xk + beta * dk;
		a[2] = (a[0] + a[4]) / 2;
		for(cicli = 0; cicli < nmax; cicli++)			// Ciclo
			{
			a[1] = (a[0] + a[2]) / 2;					// Calcola punti intermedi
			a[3] = (a[4] + a[2]) / 2;
			double fmin = double.MaxValue;				// Trova il minimo tra le 5 f(a[i])
			int imin = -1;
			for(int i=0; i<5; i++)
				{
				f[i] = Funzione(a[i]);					// Calcola la f(a[i]) con il delegate
				if(f[i] < fmin)
					{
					fmin = f[i];						// Possibile minimo
					imin = i;
					}
				}										// imin e` l'indice del punti di minimo
			Matrix dist = a[0] - a[4];							// Verifica condizioni di arresto
			double fdist = Math.Abs(f[0] - f[4]);
			double xdist = Math.Sqrt(Matrix.Sum(dist^dist));
			if( xdist < toll)									// |aj-bj|| < toll
				{
				found = true;									// Flag
				break;											// Esce dal ciclo for
				}
			if(fdist < prec)									// |f(aj)-f(bj)| < prec
				{
				found = true;
				break;
				}
			switch(imin)			// Punti di ricerca a[0]=aj, a[1]=dj, a[2]=cj, a[3]=ej, a[4]=bj
				{
				case 0:				// Se aj ->	aj1 = aj, bj1 = dj, cj = (aj + bj)/2
					a[4] = a[1];
					a[2] = (a[0] + a[4]) / 2;
					break;
				case 1:				// Se dj ->	aj1 = aj, bj1 = cj, cj1 = dj
					a[4] = a[2];
					a[2] = a[1];
					break;
				case 2:				// Se cj ->	aj1 = dj, bj1 = ej, cj1 = cj
					a[0] = a[1];
					a[4] = a[3];
					break;
				case 3:				// Se ej ->	aj1 = cj, bj1 = bj, cj1 = ej
					a[0] = a[2];
					a[2] = a[3];
					break;
				case 4:				// Se bj ->	bj1 = bj, aj1 = ej, cj = (aj + bj)/2
					a[0] = a[3];
					a[2] = (a[0] + a[4]) / 2;
					break;
				default:
					throw new Exception("Errore calcolo del minimo in Fmin.Bisezione()");
				}
			}
		return found;
		}
		#endregion
		}
	}
