using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;				// Point struct
using System.Globalization;			// Point struct

namespace Circ
	{

	public class Point2D
		{
		double x,y;
		
		public double X {get{return x;} set{x=value;}}
		public double Y {get{return y;} set{y=value;}}

		/// <summary>
		/// Costruttori
		/// </summary>
		#region COSTRUTTORI
		public Point2D()	{ x=0; y=0; }
		public Point2D(double x, double y)
			{
			this.x = x;
			this.y = y;
			}
		public Point2D(Point2D p)
			{
			this.x = p.x;
			this.y = p.Y;
			}
		#endregion

		#region Operatori
		public static Point2D Zero = new Point2D(0,0);					// Costante
		public override string ToString()
			{
			return $"[{x.ToString("G",CultureInfo.InvariantCulture)},{y.ToString("G",CultureInfo.InvariantCulture)}]";
			}							// ToString()
		public static Point2D operator +(Point2D sx, Point2D dx)		// Somma
			{
			return new Point2D(dx.x + sx.x, dx.y + sx.y);
			}
		public static Point2D operator -(Point2D sx, Point2D dx)		// Sottrazione
		    {
			return new Point2D(sx.x - dx.x, sx.y - dx.y);
		    }
		public static Point2D operator -(Point2D dx)					// Cambio segno
		    {
			return new Point2D(-dx.x, -dx.y);
		    }
		public static double operator ^(Point2D sx, Point2D dx)			// Prodotto scalare
		    {
			return dx.x * sx.x + dx.y * sx.y;
		    }
		public static double operator %(Point2D sx, Point2D dx)			// Prodotto vettoriale perpendicolare
			{
			return sx.x*dx.y - dx.x*sx.y;
			}
		public static Point2D operator *(double sx, Point2D dx)			// Prodotto numero * Punto
			{
			return new Point2D(sx * dx.x, sx * dx.y);
			}
		public static Point2D operator *(Point2D sx, double dx)			// Prodotto  Punto * numero
			{
			return new Point2D(dx * sx.x, dx * sx.y);
			}
		public static Point2D operator *(Point2D sx, Point2D dx)		// Prodotto  Punto * Punto
			{
			return new Point2D(sx.x*dx.x, sx.y*dx.y);
			}
		public static Point2D operator *(Point2D sx, Point dx)			// Prodotto  Punto * Point (int)
			{
			return new Point2D(sx.x*dx.X, sx.y*dx.Y);
			}
		public static Point2D operator *(Point sx, Point2D dx)			// Prodotto   Point (int) * Punto
			{
			return new Point2D(sx.X*dx.X, sx.Y*dx.Y);
			}
		public static Point2D operator /(Point2D sx, double dx)			// Quoziente Punto / numero
		    {
			if (System.Math.Abs(dx) <= Def.EPSILON)
				{
				throw new DivideByZeroException();
				}
			return new Point2D(sx.x / dx, sx.y / dx);
			}
		public static Point2D operator /(Point2D sx, Point2D dx)		// Quoziente Punto / Punto
			{
			if((dx.x <= Def.EPSILON)||(dx.y <= Def.EPSILON))
				throw new DivideByZeroException();	
			return new Point2D(sx.x/dx.x, sx.y/dx.y);
			}
		public static Point2D operator /(Point2D sx, Point dx)			// Quoziente Punto / Point (int)
			{
			if((dx.X == 0)||(dx.Y == 0))
				throw new DivideByZeroException();	
			return new Point2D(sx.x/dx.X, sx.y/dx.Y);
			}
		public static double Mod(Point2D p)								// Modulo
			{
			return (p.x * p.x) + (p.y * p.y);
			}
		public static double Lenght(Point2D p)
			{
			return Math.Sqrt((p.x * p.x) + (p.y * p.y));
			}						// Lunghezza
		public static Point2D Midpoint(Point2D sx, Point2D dx)			// Punto medio
			{
			return new Point2D((sx.x+dx.x)*0.5, (sx.y+dx.y)*0.5);
			}
		#endregion

		#region Operatori di conversione
		public static implicit operator Point2D(Point p)				// Conversione da Point a Point2D
			{
			return new Point2D(p.X, p.Y);
			}				
		public static explicit operator Point(Point2D p)				// Conversione da Point2D a Point
			{
			return new Point((int)p.x,(int)p.y);
			}				
		#endregion
		}

	}
