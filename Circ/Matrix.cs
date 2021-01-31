using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Fred68.Tools.Matematica;

namespace Fred68.Tools.Matematica
	{
	/// <summary> Matrix with double data and operations only </summary>
	public class Matrix : MatrixBase<double>
		{
		#region COSTANTI
		private static readonly double one = 1.0;
		private static readonly double zero = 0.0;
		/// <summary>
		/// Valore Uno
		/// </summary>
		public static double One { get { return one; } }
		/// <summary>
		/// Valore Zero
		/// </summary>
		public static double Zero { get { return zero; } }
		private static readonly double epsilon = System.Double.Epsilon;
		/// <summary>
		/// Valore Epsilon
		/// </summary>
		public static double Epsilon { get { return epsilon; } }
		#endregion
		#region COSTRUTTORI
		/// <summary>
		/// Costruttore
		/// </summary>
		public Matrix()	: base()									
			{
			}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="matrixinfo"></param>
		//public Matrix(MatrixInfo matrixinfo) : base (matrixinfo)	
		//	{
		//	}
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		public Matrix(int rows,int cols) : base(rows, cols)			
			{
			}
		/// <summary>
		/// Costruttore, copia da matrice esistente
		/// </summary>
		/// <param name="m">Matrice</param>
		/// <param name="copy">true, copia i valori</param>
		public Matrix(Matrix m, bool copy) : base(m, copy)			
			{
			}
		/// <summary>
		/// Costruttore, inizializza a val
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <param name="val"></param>
		public Matrix(int rows, int cols, double val) : base(rows, cols, val)
			{
			}
		//public double this[int r, int c] : base[int r, int c]
		//	{
		//	}

		//public double this[int r, int c]
		//	{
		//	get
		//		{
		//		return dat[r,c];
		//		}
		//	set
		//		{
		//		dat[r,c] = value;
		//		}
		//	}
		#endregion
		#region OPERATORI e FUNZIONI
		/// <summary>
		/// Operatore trasposta !A
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Matrix operator!(Matrix m)					// Trasposta
			{
			Matrix res = new Matrix(m.Col, m.Row);					// Crea matrice con dimensioni scambiate
			int ir, ic;
			for (ir = 0; ir < m.row; ir++)							// Ricopia i valori, scambiandone le posizioni
				for (ic = 0; ic < m.col; ic++)
					res.dat[ic, ir] = m.dat[ir, ic];
			return res;
			}
		/// <summary>
		/// Somma A+B
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator+(Matrix sx, Matrix dx)		// Somma
			{
			int i,j;
			if ((sx.row != dx.row) || (sx.col != dx.col))
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());

			Matrix m = new Matrix(sx, false);						// Crea matrice delle dimensioni corrette ma vuota
			for (i = 0; i < sx.row; i++)
				for (j = 0; j < sx.col; j++)
					{
					m.dat[i,j] = (sx.dat[i, j]) + (dx.dat[i, j]);
					}
			return m;
			}
		/// <summary>
		/// Differenza A-B
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator-(Matrix sx, Matrix dx)		// Sottrazione
			{
			int i, j;
			if ((sx.row != dx.row) || (sx.col != dx.col))			// Se dimensioni errate...
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());

			Matrix m = new Matrix(sx, false);						// Crea matrice delle dimensioni corrette ma vuota
			for (i = 0; i < sx.row; i++)
				for (j = 0; j < sx.col; j++)
					{
					m.dat[i, j] = (sx.dat[i, j]) - (dx.dat[i, j]);
					}
			return m;
			}
		/// <summary>
		/// Cambio segno
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static Matrix operator-(Matrix m)					// Cambio segno
			{
			int i, j;
			Matrix mn = new Matrix(m, false);
			for (i = 0; i < m.row; i++)
				for (j = 0; j < m.col; j++)
					{
					mn.dat[i, j] = -m.dat[i, j];
					}
			return mn;
			}
		/// <summary>
		/// Prodotto di matrici A*B
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator*(Matrix sx, Matrix dx)		// Prodotto di matrici
			{
			int i, j, cc;
			double sum;
			if(sx.col != dx.row)									// Se dimensioni errate per prodotto righe-colonne...
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());

			Matrix m = new Matrix(sx.row, dx.col);					// Crea matrice con dimensioni corrette
			for (i = 0; i < m.row; i++)								// Cicli i,j riga,colonna
				for (j = 0; j < m.col; j++)
					{
					sum = 0;
					for (cc = 0; cc < sx.col; cc++)					// Ciclo per sommatoria
						{
						sum += sx.dat[i,cc] * dx.dat[cc,j];
						}
					m.dat[i,j] = sum;
					}
			return m;
			}
		/// <summary>
		/// Matrice per numero A*n
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator*(Matrix sx, double dx)		// Prodotto matrice * numero
			{
			Matrix m = new Matrix(sx, false);						// Crea matrice delle dimensioni corrette ma vuota
			int i, j;
			for (i = 0; i < sx.row; i++)
				for (j = 0; j < sx.col; j++)
					{
					m.dat[i, j] = (sx.dat[i, j]) * dx;
					}
			return m;
			}
		/// <summary>
		/// Numero per matrice n*M
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator*(double sx, Matrix dx)		// Prodotto numero * matrice
			{
			Matrix m = new Matrix(dx, false);						// Crea matrice delle dimensioni corrette ma vuota
			int i, j;
			for (i = 0; i < dx.row; i++)
				for (j = 0; j < dx.col; j++)
					{
					m.dat[i, j] = (dx.dat[i, j]) * sx;				// Scrive i valori
					}
			return m;
			}
		/// <summary>
		/// Divisione matrice per numero M/n
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator/(Matrix sx, double dx)		// Divisione matrice / numero
			{
			if(System.Math.Abs(dx) <= Matrix.Epsilon)
				{
				throw new InvalidOperationException(Error.DivisionByZero.ToString());		// Se divisione per zero: eccezione
				}
			Matrix m = new Matrix(sx, false);						// Crea matrice delle dimensioni corrette ma vuota
			int i, j;
			for (i = 0; i < sx.row; i++)
				for (j = 0; j < sx.col; j++)
					{
					m.dat[i, j] = (sx.dat[i, j]) / dx;				// Scrive i valori
					}
			return m;
			}
		/// <summary>
		/// Prodotto puntuale A^B a(ij)*b(ij)
		/// </summary>
		/// <param name="sx"></param>
		/// <param name="dx"></param>
		/// <returns></returns>
		public static Matrix operator^(Matrix sx, Matrix dx)		// Prodotto a(ij) * b(ij)
			{
			int i,j;
			if ((sx.row != dx.row) || (sx.col != dx.col))			// Se dimensioni errate...
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());

			Matrix m = new Matrix(sx, false);						// Crea matrice delle dimensioni corrette ma vuota
			for (i = 0; i < sx.row; i++)
				for (j = 0; j < sx.col; j++)
					{
					m.dat[i,j] = (sx.dat[i, j]) * (dx.dat[i, j]);
					}
			return m;
			}
		/// <summary>
		/// Sommatoria di tutti gli elementi della matrice
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static double Sum(Matrix m)							// Somma di tutti gli elementi della matrice
			{
			int i, j;
			double sum = 0.0;
			for (i = 0; i < m.row; i++)
				for (j = 0; j < m.col; j++)
					{
					sum += m.dat[i, j];
					}
			return sum;
			}
		/// <summary>
		/// Copia matrice
		/// </summary>
		/// <returns></returns>
		public new Matrix Copy()
			{
			return new Matrix(this,true);
			}
		#endregion
		#region MATRICI SPECIALI
		/// <summary>
		/// Matrice identita`
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Matrix Id(int n)
			{
			if(n < 1)
				{
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());
				}
			Matrix id = new Matrix(n, n);
				for(int i=0; i<n; i++)
					id.dat[i, i] = Matrix.one;
			return id;
			}
		/// <summary>
		/// Triangolare inferiore
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Matrix Upper(int n)
			{
			if (n < 1)
				{
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());
				}
			Matrix up = new Matrix(n, n);
			int i,j;
			for (i = 0; i < n; i++)
				for (j=i; j < n; j++)
					up.dat[i, j] = Matrix.one;
			return up;
			}
		/// <summary>
		/// Triangolare inferiore 
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Matrix Lower(int n)
			{
			if (n < 1)
				{
				throw new InvalidOperationException(Error.MatrixWrongDimension.ToString());
				}
			Matrix low = new Matrix(n, n);
			int i, j;
			for (i = 0; i < n; i++)
				for (j = 0; j <= i; j++)
					low.dat[i, j] = Matrix.one;
			return low;
			}
		/// <summary>
		/// Matrice di uno
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <returns></returns>
		public static Matrix Ones(int rows, int cols)
			{
			Matrix ones = new Matrix(rows, cols);
			int i, j;
			for (i = 0; i < ones.Row; i++)
				for (j = 0; j < ones.Col; j++)
					ones.dat[i, j] = Matrix.one;
			return ones;
			}
		/// <summary>
		/// Matrice di zero
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <returns></returns>
		public static Matrix Zeros(int rows, int cols)
			{
			Matrix zeros = new Matrix(rows, cols);
			int i, j;
			for (i = 0; i < zeros.Row; i++)
				for (j = 0; j < zeros.Col; j++)
					zeros.dat[i, j] = Matrix.zero;
			return zeros;
			}
		#endregion
		}

	}