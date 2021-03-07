using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Colebrook
	{

	class Program
		{
		static void Main(string[] args)
			{
			Console.WriteLine("Colebrook");
			Colebrook col = new Colebrook();
			string inp = string.Empty;
			do
				{
				if(inp == "?")
					{
					Console.WriteLine("var: input var (then value or d default), ?:help, c:calc, x:exit");
					}
				else if(inp == "c")
					{
					col.Calc();
					}
				else if(col.Contain(inp))
					{
					col.Input(inp);
					}
				col.PrintAll();
				Console.Write("\n[var, ?, c, x]:");
				inp = Console.ReadLine();
				}	while(inp != "x");
			}
		}

	public class Dato
		{
		public double v;
		public bool ok;
		public string desc;
		public string name;
		public double defval;

		public Dato(double val, bool ok, string desc, double defval, string name = "")
			{
			this.v = val;
			this.ok = ok;
			this.desc = desc;
			this.defval = defval;
			this.name = name;
			}

		public override string ToString()
			{
			return $"{desc} {name} = {v.ToString()} [{defval.ToString()}]";
			}

		public bool Get()
			{
			double x = 0;
			Console.Write(desc + ":\t");
			string s = Console.ReadLine();
			if(s == "d")
				{
				this.Default();
				}
			else
				{
				ok = double.TryParse(s, out x);
				if(ok)
					{
					this.v = x;
					}
				}
			return ok;
			}

		public void Print()
			{
			Console.WriteLine(this.ToString());
			}

		public void Default()
			{
			this.v = this.defval;
			}
		}


	public class Colebrook
		{
		public readonly double CICLI = 1000;
		public readonly double DIFF = 0.0001;
		public readonly double INVSQRTFA = 1;

		public Dato Fa, D, ro, v, r, e, mu, nu, Re, eD;
		public  int temp;
		List<string> nomi;
	
		/// <summary>
		/// Costruttore
		/// </summary>
		public Colebrook()
			{
			nomi = new List<string>();
			foreach(FieldInfo inf in typeof(Colebrook).GetFields())
				{
				if(inf.FieldType == typeof(Dato))
					{
					nomi.Add(inf.Name);
					}
				}
			Fa = new Dato(0d,true,"Fattore di attrito []", 0.001, nameof(Fa));
			D = new Dato(0d,true,"Diametro interno [m]", 0.020, nameof(D));
			ro = new Dato(0d,true,"Densità [kg/m3]", 1000, nameof(ro));
			v = new Dato(0d,true,"Velocità [m/s]", 1, nameof(v));
			r = new Dato(0d,true,"Perdita di carico unitaria [Pa/m]", 0, nameof(r));
			e = new Dato(0d,true,"Rugosità [m]", 0.0001, nameof(e));
			mu = new Dato(0d,true,"Viscosità dinamica [Pa s]", 8.94e-4, nameof(mu));
			nu = new Dato(0d,true,"Viscosità cinematica [m2/s]", 0, nameof(nu));
			Re = new Dato(0d,true,"Numero di Reynolds []", 2000, nameof(Re));
			eD = new Dato(0d,true,"Rugosità relativa[]", 0.1, nameof(eD));

			ResetDefaults();
			}

		public void ResetDefaults()
			{
			foreach(string n in nomi)
				{
				Dato x =  (Dato) typeof(Colebrook).GetField(n).GetValue(this);
				x.Default();
				}
			}
		public bool Contain(string var)
			{
			return nomi.Contains(var);
			}

		/// <summary>
		/// Stampa la variabile (lento, usa Reflection)
		/// </summary>
		/// <param name="name">Nome della variabile</param>
		/// <returns></returns>
		public void Print(string name)
			{
			Dato x =  (Dato) typeof(Colebrook).GetField(name).GetValue(this);
			x.Print();
			}

		/// <summary>
		/// Stampa tutte le variabili (lento, usa Reflection)
		/// </summary>
		public void PrintAll()
			{
			Console.WriteLine(new string('-',20));
			foreach(string n in nomi)
				Print(n);

			}

		public bool Input(string name)
			{
			bool ok = false;
			if(nomi.Contains(name))
				{
				Dato x =  (Dato) typeof(Colebrook).GetField(name).GetValue(this);
				ok = x.Get();
				}
			return ok;
			}

		public bool Calc()
			{
			bool ok = true;					// Calcola normalmente. TryParse non genera eccezioni, ma imposta a NaN
			try								// Usa comunque try, nel caso di altri errori
				{
				eD.v = e.v / D.v;					// Rugosità relativa
				nu.v = mu.v / ro.v;					// Viscosità cinematica
				Re.v = (ro.v)*(v.v)*(D.v)/(mu.v);	// Numero di Reynolds
				if(Re.v < 2000)
					{
					Fa.v = 64 / Re.v;				// Moto laminare
					}
				else
					{								// Moto turbolento (o transizione): usa Colebrook

					int cicli;
					double diff;
					double invsqrtfaOld = INVSQRTFA;		// 1/sqrt(Fa)
					double invsqrtfa = 2*INVSQRTFA;
					for(cicli=0,diff=double.MaxValue; (cicli < CICLI) && (diff > DIFF); cicli++)
						{
						invsqrtfa = -2 * Math.Log10( (e.v / (3.7 * D.v)) + (2.51 * invsqrtfaOld /Re.v));
						diff = Math.Abs(invsqrtfa - invsqrtfaOld);
						invsqrtfaOld = invsqrtfa;
						Console.WriteLine($"invsqrtfa = {invsqrtfa}");
						}
					if(cicli <= CICLI)
						{
						Fa.v = 1 / (invsqrtfa * invsqrtfa);
						Console.WriteLine($"Colebrook convergenza in {cicli} cicli, errore {diff}");
						}
					else
						{
						ok = false;
						Console.WriteLine($"Colebrook non converge dopo {cicli} cicli, errore {diff}");
						}
					}
				}
			catch
				{
				ok = false;
				}
			string msg;
			ok &= Check(out msg);
			if(!ok)
				Console.WriteLine($"Errore: {msg}");
			return ok;
			}

		public bool Check(out string s)
			{
			bool ok = true;
			StringBuilder strb = new StringBuilder();
			foreach(string n in nomi)
				{
				Dato x =  (Dato) typeof(Colebrook).GetField(n).GetValue(this);
				if(double.IsNaN(x.v) || double.IsInfinity(x.v))
					{
					ok = false;
					strb.Append(x.name + ", ");
					}
				}
			s = strb.ToString().Trim(new char[]{' ', ','});
			return ok;
			}
		}
	}
