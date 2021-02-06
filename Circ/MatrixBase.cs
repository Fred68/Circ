using System;
using System.Collections.Generic;
using System.Text;

namespace Fred68.Tools.Matematica
	{
	/// <summary>
	/// MatrixBase classe base per le matrici
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MatrixBase<T>
							: IFormattable					// Per ToString()
							 where T : new()				// Constraint per tipo T: deve avere metodo new()
		{
		/// <summary>
		/// Error codes
		/// </summary>
		public enum Error
			{
			/// Matrice valida row>0 and col>0
			NullMatrix = 1,
			/// Dimensioni delle matrici non valide per l'operazione
			MatrixWrongDimension = 2,
			/// Divisione per zero
			DivisionByZero = 3,
			/// Mal condizionata determinante = 0
			BadConditioned = 4,
			/// Indici fuori limiti
			OutOfRange = 5,
			/// Errore generico di altra operazione
			Invalid = 6
			}
		#region PROTECTED
		#pragma warning disable 1591						// Disabilita warning in compilazione
		protected int row;									// Numeri di righe...
		protected int col;									// ...e colonne
		protected T[,] dat;									// Matrice dei dati
		protected bool checkIndex;
		#pragma warning restore 1591
		#endregion
		#region PROPRIETA
		/// <summary>
		/// Numero di righe
		/// </summary>
		public int Row {get {return row;}}
		/// <summary>
		/// Numero di colonne
		/// </summary>
		public int Col { get {return col;}}
		/// Abilita o disabilita controllo indici all'accesso dei dati
		/// Dafault: abilitato
		/// </summary>
		public bool IndexCheck
			{
			get { return checkIndex;}
			set { checkIndex = value;}
			}
		/// <summary>
		/// Restituisce true se matrice nulla
		/// </summary>
		/// <returns></returns>
		public bool IsNull
			{
			get { return ((row==0)||(col==0));}
			}
		#endregion
		#region COSTRUTTORI
		/// <summary>
		/// Costruttore
		/// </summary>
		public MatrixBase()									// Costruttore (matrice nulla)
			{
			row = col = 0;
			dat = null;
			checkIndex = true;
			}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="rows">Righe</param>
		/// <param name="cols">Righe</param>
		public MatrixBase(int rows,int cols)				// Costruttore
			{
			if( (rows>0)&&(cols>0) )
				{
				row = rows;
				col = cols;
				dat = new T[row,col];
				}
			else
				{	
				row = col = 0;
				dat = null;
				}
			}
		/// <summary>
		/// Costruttore
		/// (da migliorare, T : IClonable)
		/// </summary>
		/// <param name="m">Matrice di partenza</param>
		/// <param name="copy">Ricopia i dati se true</param>
		public MatrixBase(MatrixBase<T> m, bool copy)		// Costruttore (se true, ricopia i dati)
			{
			row = m.row;									// Copia le dimensioni
			col = m.col;
			if( (row>0)&&(col>0) )					
				{
				dat = new T[row, col];						// Alloca nuovi dati
				if(copy)									// Se richiesto...
					{										// ...copia il contenuto
					int r,c;
					for(r=0; r<row; r++)
						for(c=0; c<col; c++)
							dat[r, c] = m.dat[r, c];		// Nota (*) : usa l'operatore = ma dovrebbe usare Clone()
															// Opportuno mettere un controllo su T, se ICloneable
															// Ma Clone() fa una copia superficiale come object
					}
				}
			else
				{
				dat = null;
				}
			}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="rows">Righe</param>
		/// <param name="cols">Colonne</param>
		/// <param name="val">Dato: copia solo il riferimento, non lo clona</param>
		public MatrixBase(int rows, int cols, T val)
			{
			if( (rows>0)&&(cols>0) )
				{
				row = rows;
				col = cols;
				dat = new T[row,col];
				int r, c;
				for (r = 0; r < row; r++)
					for (c = 0; c < col; c++)
						dat[r, c] = val;					// Se T e` un tipo riferimento, copia solo il riferimento, non lo clona
				}
			else
				{	
				row = col = 0;
				dat = null;
				}
			}
		#endregion
		#region ACCESSO AI DATI e INTERROGAZIONI
		/// <summary>
		/// Verifica se indici corretti
		/// Gli indici partono da zero
		/// </summary>
		/// <param name="r">Riga</param>
		/// <param name="c">Colonna</param>
		/// <returns>true se indici corretti</returns>
		public bool CheckIndex(int r, int c)
			{
			return( (r>=0) && (c>=0) && (r<row) && (c<col));
			}
		/// <summary>
		/// Legge il valore di una cella
		/// Gli indici partono da zero
		/// Se indici errati: ArgumentOutOfRangeException
		/// Usare accesso diretto: T valore = m[r,c] oppure m[i] se una sola riga o colonna
		/// </summary>
		/// <param name="r">Riga</param>
		/// <param name="c">Colonna</param>
		/// <param name="d">ref Dato da leggere</param>
		/// <returns>true se indici corretti</returns>
		public bool Get(int r, int c, ref T d)
			{
			if(checkIndex)
				if(!CheckIndex(r,c))
					{
					throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
					}
			d = dat[r,c];
			return true;
			}
		/// <summary>
		/// Legge il valore di una cella
		/// Gli indici partono da zero
		/// Se indici errati: ArgumentOutOfRangeException
		/// Usare accesso diretto T valore = m[r,c] oppure m[i] se una sola riga o colonna
		/// 
		/// </summary>
		/// <param name="r">Riga</param>
		/// <param name="c">Colonna</param>
		/// <returns>Dato letto</returns>
		public T Get(int r, int c) 
			{
			if(checkIndex)
				if(!CheckIndex(r, c))
					{
					throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
					}
			return dat[r, c];
			}
		/// <summary>
		/// Imposta il valore di una cella
		/// Gli indici partono da zero
		/// Se indici errati: ArgumentOutOfRangeException
		/// Usare accesso diretto m[r,c] = valore oppure m[i]=... se una sola riga o colonna
		/// </summary>
		/// <param name="r">Riga</param>
		/// <param name="c">Colonna</param>
		/// <param name="d">Valore da impostare</param>
		/// <returns></returns>
		public bool Set(int r, int c, T d)
			{
			if(checkIndex)
				if(!CheckIndex(r, c))
					{
					throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
					}
			dat[r, c] = d;
			return true;
			}
		/// <summary>
		/// Accesso diretto con indici
		/// </summary>
		/// <param name="r">Riga</param>
		/// <param name="c">Colonna</param>
		/// <returns></returns>
		public T this[int r, int c]
			{
			get
				{
				if(checkIndex)
					if(!CheckIndex(r, c))
						{
						throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
						}
				return dat[r,c];
				}
			set
				{
				if(checkIndex)
					if(!CheckIndex(r, c))
						{
						throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
						}
				dat[r,c] = value;
				}
			}
		/// <summary>
		/// Accesso diretto con un indice, se una sola riga o colonna
		/// </summary>
		/// <param name="rc">indice della riga o colonna</param>
		/// <returns></returns>
		public T this[int rc]
			{
			get
				{
				if(row == 1)
					return Get(0,rc);
				else if(col == 1)
					return Get(rc,0);
				else
					throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
				}
			set
				{
				if(row == 1)
					Set(0, rc, value);
				else if(col == 1)
					Set(rc, 0, value);
				else
					throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
				}
			}
		/// <summary>
		/// Confronta dimensioni
		/// </summary>
		/// <param name="m">Matrice da confrontare</param>
		/// <returns>true se stesse dimensioni</returns>
		public bool SameSize(MatrixBase<T> m)
			{
			if( (this.Col == m.Col) && (this.Row == m.Row))
				return true;
			return false;
			}
		/// <summary>
		/// Confronta dimensioni
		/// </summary>
		/// <param name="m1">Prima matrice</param>
		/// <param name="m2">Seconda matrice</param>
		/// <returns>true se stesse dimensioni</returns>
		public static bool SameSize(MatrixBase<T> m1, MatrixBase<T> m2)
			{
			return m1.SameSize(m2);
			}
		/// <summary>
		/// Enumerator
		/// Restituisce i valori leggendo in ordine i valori di una riga per volta
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
			{
			int r,c;
			for(r=0; r<row; r++)
				for(c=0; c<col; c++)
					yield return dat[r,c];
			} 
		/// <summary>
		/// Reset enumerator
		/// </summary>
		/// <returns></returns>
		public IEnumerable<T> Reset()
			{
			yield break;
			}
		/// <summary>
		/// Restituisce copia della matrice
		/// </summary>
		/// <returns></returns>
		public MatrixBase<T> Copy()
			{
			return new MatrixBase<T>(this,true);
			}
		#endregion
 		#region MODIFICA
		/// <summary>
		/// Ridimensiona la matrice
		/// Se indici errati: ArgumentOutOfRangeException
		/// </summary>
		/// <param name="rows">Righe</param>
		/// <param name="cols">Colonne</param>
		/// <param name="keep">True per mantenere i dati</param>
		/// <returns></returns>
		public bool Dim(int rows, int cols, bool keep)		// Varia le dimensioni (mantiene il contenuto se true)
			{
			int ir,ic;										// Contatori
			T[,] datOld;									// Matrice dei dati vecchi
			if( (rows<0) || (cols<0))						// Verifica le nuove dimensioni
				throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
			if( (rows==0) || (cols==0) )					// Se una dimensione è nulla, azzera
				{
				row = col = 0;
				dat = null;
				return true;	
				}
			datOld = this.dat;								// Salva reference a vecchia matrice
			dat = new T[rows,cols];							// Alloca nuova matrice e la assegna a dat			
			if(keep)										// Se deve mantenere i valori...
				{
				for(ir=0; ir < rows; ir++)					// ...ricopia la vecchia matrice nella nuova
					for(ic=0; ic < cols; ic++)
						{
						if( (ir < row) && (ic < col) )
							dat[ir,ic] = datOld[ir,ic];		// Ricopia valori...
						else							
							dat[ir,ic]= default(T);			// ...oppure azzera se fuori indice
						}
				}
			row = rows;										// Imposta i nuovi indici
			col = cols;
			return false;
			}
		/// <summary>
		/// Varia le dimensioni mantenendo il contenuto
		/// Se indici errati: ArgumentOutOfRangeException
		/// </summary>
		/// <param name="rows">Righe</param>
		/// <param name="cols">Colonne</param>
		/// <returns></returns>
		public bool Dim(int rows, int cols)					// Varia le dimensioni mantenendo il contenuto
			{
			return Dim(rows,cols,true);
			}
		/// <summary>
		/// Azzera la matrice
		/// Non esegue chiamate al gc
		/// </summary>
		public void Clear()									// Azzera la matrice (non chiama il gc)
			{
			row = col = 0;
			dat = null;
			}
		/// <summary>
		/// Elimina riga
		/// Se indice errato: ArgumentOutOfRangeException
		/// </summary>
		/// <param name="irow">Indice della riga</param>
		/// <returns></returns>
		public bool RemRow(int irow)							// Elimina riga
			{
			int rownew, colnew;									// Nuove dimensioni
			int ir, ic;											// Indici nuova matrice
			int irold, icold;									// Indici vecchia matrice
			T[,] datnew;										// Nuovo array

			if ((irow < 0) || (irow >= row))					// Verifica correttezza indice
				throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
			rownew = row - 1;									// Nuove dimensioni
			colnew = col;
			if((rownew <= 0) || (colnew <= 0))					// Se una dimensione e` nulla, azzera dati e dimensioni
				{
				row = col = 0;
				dat = null;
				return true;									// ed esce
				}
			datnew = new T[rownew, colnew];						// Alloca nuovo array
			for (ir = 0, irold = 0; ir < rownew; ir++, irold++)	// Ciclo doppio su indici vecchi e nuovi
				{
				if (irold == irow) irold++;						// Incrementa (salta) se uguale all'indice eliminato
				for (ic = 0, icold = 0; ic < colnew; ic++, icold++)
					datnew[ir,ic] = dat[irold,icold];
				}
			row = rownew;										// Imposta i nuovi indici
			col = colnew;
			dat = datnew;										// Imposta il nuovo array
			return true;
			}
		/// <summary>
		/// Elimina colonna
		/// Se indice errato: ArgumentOutOfRangeException
		/// </summary>
		/// <param name="icol"></param>
		/// <returns></returns>
		public bool RemCol(int icol)							// Elimina colonna
			{
			int rownew, colnew;									// Nuove dimensioni
			int ir, ic;											// Indici nuova matrice
			int irold, icold;									// Indici vecchia matrice
			T[,] datnew;										// Nuovo array

			if ((icol < 0) || (icol >= col))					// Verifica correttezza indice
				throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
			rownew = row;										// Nuove dimensioni
			colnew = col - 1;
			if ((rownew <= 0) || (colnew <= 0))					// Se una dimensione e` nulla, azzera dati e dimensioni
				{
				row = col = 0;
				dat = null;
				return true;									// ed esce
				}
			datnew = new T[rownew, colnew];						// Alloca nuovo array
			for (ir = 0, irold = 0; ir < rownew; ir++, irold++)	// Ciclo doppio su indici vecchi e nuovi
				{
				for (ic = 0, icold = 0; ic < colnew; ic++, icold++)
					{
					if (icold == icol)	icold++;				// Incrementa (salta) se uguale all'indice eliminato
					datnew[ir, ic] = dat[irold, icold];
					}
				}
			row = rownew;										// Imposta i nuovi indici
			col = colnew;
			dat = datnew;										// Imposta il nuovo array
			return true;
			}
		/// <summary>
		/// Elimina riga e colonna
		/// DA SCRIVERE
		/// </summary>
		/// <param name="irow"></param>
		/// <param name="icol"></param>
		/// <returns></returns>
		public bool RemRowCol(int irow, int icol)				// DA SCRIVERE
			{
			Exception e;
			throw e = new Exception("Funzione da scrivere");
			// return false;
			/*
			template <class DATA> bool MatrixBase <DATA> :: rem_row_col(int irow, int icol)
				{
				DATA *datnew;							// Puntatore a nuovi dati
				int rownew,colnew;						// Nuove dimensioni
				int ir,ic;								// Indici nuova matrice
				int irold,icold;						// Indici vecchia matrice

				if((irow < 0)||(irow >= row)||(icol < 0)||(icol >= col))	// Verifica correttezza indici
					return false;
				rownew = row-1;							// Nuove dimensioni
				colnew = col-1;
				datnew = new DATA[(rownew)*(colnew)];	// Alloca nuova matrice (row-1, col-1)
				if(!datnew)								// Verifica allocazione avvenuta
					return false;

				for(ir=0,irold=0; ir<rownew; ir++,irold++)	// Ciclo doppio su indici vecchi e nuovi
					{
					if(irold==irow)						// Incrementa (salta) se uguale all'indice eliminato
						irold++;
					for(ic=0,icold=0; ic<colnew; ic++,icold++)
						{
						if(icold==icol)					// Incrementa (salta) se uguale all'indice eliminato
							icold++;
						datnew[ir*colnew+ic] = dat[irold*col+icold];
						}
					}
				if (dat) delete [] dat;					// Dealloca la vecchia matrice
				row = rownew;							// Imposta i nuovi indici
				col = colnew;
				dat = datnew;							// Imposta il nuovo puntatore
				return true;
				} 
			 */
			}
		/// <summary>
		/// Restituisce una riga
		/// Se indice errato o matrice nulla: ArgumentOutOfRangeException
		/// </summary>
		/// <param name="irow"></param>
		/// <returns></returns>
		public MatrixBase<T> GetRow(int irow)					// Restituisce una riga
			{
			MatrixBase<T> m;
			int i;
			if((irow<0)||(irow>=row))					// Se indici non corretti o matrice iniziale con dimensione nulla...
				{
				throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
				}
			else if(col<=0)
				{
				throw new ArgumentOutOfRangeException(Error.NullMatrix.ToString());
				}
			m = new MatrixBase<T>(1, col);						// Crea matrice con una riga a tante colonne quanto la matrice dipartenza
			for (i = 0; i < col; i++)							// Ricopia i valori della riga irow per tutte le colonne i
				m.dat[0,i]= dat[irow, i];
			return m;											// Restituisce il riferimento all'oggetto allocato
			}
		/// <summary>
		/// Restituisce una colonna
		/// Se indice errato o matrice nulla: ArgumentOutOfRangeException
		/// </summary>
		/// <param name="icol"></param>
		/// <returns></returns>
		public MatrixBase<T> GetCol(int icol)					// Restituisce una colonna
			{
			MatrixBase<T> m;
			int i;
			if ((icol < 0) || (icol >= col))					// Se indici non corretti o matrice iniziale con dimensione nulla...
				{
				throw new ArgumentOutOfRangeException(Error.OutOfRange.ToString());
				}
			else if(row <= 0)
				{
				throw new ArgumentOutOfRangeException(Error.NullMatrix.ToString());
				}

			m = new MatrixBase<T>(row, 1);						// Crea matrice con una colonna a tante righe quanto la matrice dipartenza
			for (i = 0; i < row; i++)							// Ricopia i valori della colonna icol per tutte le righe i
				m.dat[i, 0] = dat[i, icol];
			return m;											// Restituisce il riferimento all'oggetto allocato
			}
		/// <summary>
		/// Trasposta
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static MatrixBase<T> operator !(MatrixBase<T> m)	// Trasposta
			{
			MatrixBase<T> res = new MatrixBase<T>(m.Col, m.Row);	// Crea matrice con dimensioni scambiate
			int ir, ic;
			for (ir = 0; ir < m.row; ir++)							// Ricopia i valori, scambiandone le posizioni
				for (ic = 0; ic < m.col; ic++)
					res.dat[ic, ir] = m.dat[ir, ic];
			return res;
			}
		/// <summary>
		/// Trapone la matrice (DA VERIFICARE)
		/// </summary>
		/// <returns></returns>
		public bool Transpose()									// Traspone la matrice DA VERIFICARE !!!!!
			{
			T[,] res = new T[Col, Row];							// Crea array con dimensioni scambiate
			int ir, ic;
			for (ir = 0; ir < row; ir++)						// Ricopia i valori, scambiandone le posizioni
				for (ic = 0; ic < col; ic++)
					res[ic, ir] = dat[ir, ic];
			dat = res;											// Assegna il nuovo array
			return true;
			}
		#endregion
		#region CONVERSIONE
		/// <summary>
		/// ToString()
		/// </summary>
		/// <returns></returns>
		public override string ToString()							// Conversione in stringa
			{
			int ir, ic;
			StringBuilder str = new StringBuilder("[");
			if(!IsNull)												// Percorre la matrice solo se e` valida
				{
				for (ir = 0; ir < row; ir++)
					{
					for (ic = 0; ic < col; ic++)
						{
						if (dat[ir, ic] != null)							// Il default(T), usato nelle funzioni Dim(), mette null per i tipi riferimento
							str.Append((dat[ir, ic]).ToString() + "\t");	// e darebbe errore il ToString()
						}
					str.Append("\n ");
					}
				}
			str.Append("] R" + row.ToString() + " x C" + col.ToString() + '\n');
			return str.ToString();
			}
		/// <summary>
		/// ToString()
		/// </summary>
		/// <param name="format"></param>
		/// <param name="formatProvider"></param>
		/// <returns></returns>
		public string ToString(string format, IFormatProvider formatProvider)
			{
			if (format == null)										// Se formato nullo, restiruisce lo standard
				return ToString();
			string formatUp = format.ToUpper();
			int ir, ic;
			StringBuilder str = new StringBuilder("[");
			if (!IsNull)											// Percorre la matrice solo se e` valida
				{
				for (ir = 0; ir < row; ir++)
					{
					for (ic = 0; ic < col; ic++)
						{
						if (dat[ir, ic] != null)							// Il default(T), usato nelle funzioni Dim(), mette null per i tipi riferimento
							str.Append(dat[ir, ic].ToString() + '\t');		// e darebbe errore il ToString()
						}
					str.Append("\n ");
					}
				}
			str.Append("]");
			switch(formatUp)
				{
				case "S":
					str.Append(" R" + row.ToString() + " x C" + col.ToString());
					break;
				default:
					break;
				}
			return str.ToString();
			}
		#endregion
		}
	}	

