using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace Fred68.Tools.Matematica
	{
	/// <summary> LinearSys solves a linear system with Gauss method (reduction) </summary>
	class LinearSys 
		{
		#region COSTANTI
		private static readonly double epsilon = System.Double.Epsilon;
		#endregion
		#region PROTECTED
		protected Matrix a;											// Matrice fattorizzata
		protected MatrixBase<int> pivot;							// Vettore di pivot
		protected double det;										// Determinante
		#endregion
		#region PROPRIETA
		public static double Epsilon { get { return epsilon; } }	// Valore minimo di derminante sotto cui considerare la matrice mal condizionata
		public double Determinant {get {return det;}}				// Determinante
		public Matrix Factorized {get {return a;}}					// Matrice fattorizzata
		#endregion
		#region FUNZIONI
		public void Clear()											// Inizializza il sistema lineare
			{														// Il pivot non viene toccato, inizializzato ogni volta, comunque non accessibile
			det = Matrix.Zero;										// Azzera il determinante
			a = new Matrix();										// Crea matrice vuota, verra` sostituita dopo fattorizzazione
			}
		public bool Factor(Matrix A)								// Fattorizza la matrice A con il metodo di Gauss con pivoting parziale
			{
			int n;													// Dimensione matrice
			if (A.Row != A.Col)										// Verifica che A sia quadrata
				return false;
			n = A.Row;												// Legge dimensione
			if (n < 1)												// Verifica dimensione
				return false;
			pivot = new MatrixBase<int>(n - 1, 1);					// Inizializza dimensioni matrici		
			a = new Matrix(A, true);								// Crea nuova matrice idntica ad A
			if (n == 1)												// Se di ordine 1...
				{
				det = a[0,0];
				if( System.Math.Abs(det) <= LinearSys.Epsilon)		// Se det nullo, esce con errore
					return false;
				return true;										// Se det non nullo, esce regolarmente									
				}
			int k, i, io, j;										// Ciclo di calcolo
			double amax;
			double tmp;
			for(io = 0, det = Matrix.One, k = 0; k < n-1; k++)		// Ciclo 1
				{
				for(amax=0.0, i=k; i<n; i++)						// Cerca la riga io con il massimo amax sulla colonna k
					if(System.Math.Abs(a[i,k]) >= amax )
						{
						io = i;
						amax = System.Math.Abs(a[i,k]);
						}
				pivot[k,0] = io;
				if (amax <= LinearSys.Epsilon)						// Se amax è nullo, esce con errore
					{
					det = 0.0;
					return false;
					}
				if(io != k)											// Se l'indice non è k...
					{
					for(j = k; j < n; j++)							// Scambia le righe k e io
						{
						tmp = a[k, j];
						a[k,j] = a[io, j];
						a[io, j] = tmp;
						}
					det = -det;										// e cambia segno al determinante
					}
				for(i=k+1; i<n; i++)								// Ciclo 2
					{
					a[i, k] =  - a[i,k] / a[k,k];
					for(j = k+1; j < n; j++)
						{
						a[i, j] = a[i,j] + a[i,k]*a[k,j];
						}
					}												// Fine ciclo 2
				det = det * a[k,k];
				}													// Fine ciclo 1
			if (System.Math.Abs(a[n - 1, n - 1]) <= LinearSys.Epsilon)
				{
				det = 0.0;
				return false;
				}	
			det = det * a[n-1, n-1];
			return true;
			}
		public Matrix Solve(Matrix b)								// Risolve il sistema, b deve essere inizializzato
			{
			int n;
			Matrix x;
			if (a.Row != a.Col)										// Verifica che a sia quadrata
				throw new InvalidOperationException(MatrixBase<double>.Error.MatrixWrongDimension.ToString());
			n = a.Row;												// Legge dimensione
			if (n < 1)												// Verifica dimensione
				throw new InvalidOperationException(MatrixBase<double>.Error.MatrixWrongDimension.ToString());
			if ((pivot.Row != n - 1) || (pivot.Col != 1))			// Verifica dimensioni pivot
				throw new InvalidOperationException(MatrixBase<double>.Error.MatrixWrongDimension.ToString());
			if ((b.Row != n) || (b.Col != 1))						// Verifica dimensioni vettore termini noti
				throw new InvalidOperationException(MatrixBase<double>.Error.MatrixWrongDimension.ToString());
			if(System.Math.Abs(det) <= LinearSys.Epsilon)			// Verifica che il determinanet non sia nullo
				throw new InvalidOperationException(MatrixBase<double>.Error.BadConditioned.ToString());
			x = new Matrix(b,true);									// Assegna valore iniziale ad x
			if (n == 1)												// Se di ordine 1, verifica determinante non nullo...
				{
				if (System.Math.Abs(a[0, 0]) <= LinearSys.Epsilon)
					throw new InvalidOperationException(MatrixBase<double>.Error.BadConditioned.ToString());
				x[0, 0] = x[0, 0] / a[0, 0];				// ...e calcola la soluzione semplice
				return x;
				}
			int k, j, i;											// Ciclo di calcolo
			double tmp;

			for(k=0; k<n-1; k++)
				{
				j = pivot[k,0];
				if(j != k)
					{
					tmp = x[j,0];
					x[j, 0] = x[k,0];
					x[k, 0] = tmp;
					}
				for(i = k+1; i < n; i++)
					{
					x[i,0] = x[i,0] + a[i,k] * x[k,0];
					}
				}
			x[n-1, 0] = x[n-1,0] / a[n-1,n-1];
			for(i=n-2; i>=0; i--)
				{
				for(tmp = 0.0, j = i+1; j < n; j++)
					tmp = tmp + a[i,j] * x[j,0];
				x[i,0] = (x[i,0] - tmp) / a[i,i];
				}		
			return x;
			}
		public Matrix Solve(Matrix A, Matrix b)						// Fattorizza e risolve
			{
			Factor(A);
			return Solve(b);
			}
		#endregion
		}
	}
