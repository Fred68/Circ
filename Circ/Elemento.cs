using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;						// Serializzazione in Json
using System.Drawing;                       // Point e altro
using System.Drawing.Drawing2D;

namespace Circ
	{
	public class Elemento : IID, IDraw, ICopyData
		{
		public static uint UNASSIGNED = 0;
		protected uint id;					// ID dell'oggetto
		protected string name;				// Descrizione
		protected bool sel;					// Selezionato
		protected Def.ClipFlag clipped;     // Fuori dalla vista attuale (non in display list)
		protected bool connesso;
		protected System.Drawing.Drawing2D.Matrix m;	// Matrice di trasfrmazione 2D

		/// <summary>
		/// Costruttore
		/// </summary>
		public Elemento()
			{
			name = string.Empty;
			id = UNASSIGNED;
			sel = false;
			clipped = Def.ClipFlag.Inside;
			connesso = false;
			m = new Matrix();		// Matrice identità
			}

		#region PROPRIETÀ (e SERIALIZZAZIONE)

		public uint ID
			{
			get {return id;}
			set {id = value;}
			}

		public string Name
			{
			get {return name;}
			set {name = value;}
			}
	
		[JsonIgnore]
		public bool Selected
			{
			get {return sel;}
			set {sel = value;}
			}

		[JsonIgnore]
		public Def.ClipFlag Clipped
			{
			get {return clipped;}
			set {clipped = value;}
			}

		[JsonIgnore]
		public virtual Point Center
			{
			get {throw new Exception("Proprietà public virtual Point Center non sovrascritta nella classe derivata");}
			}

		[JsonIgnore]
		public bool Connesso
			{
			get {return connesso;}
			set {connesso = value;}
			}

		[JsonIgnore]
		public System.Drawing.Drawing2D.Matrix Trasform
			{
			get
				{
				return m;
				}
			}

		#endregion

		public virtual void Regen(Vista v, bool addToDisplayList = true)
			{
			throw new Exception("public virtual void Regen() non sovrascritta nella classe derivata");
			}
		public virtual void Draw(Graphics g, Pen pn, Brush br, Font fn)
			{
			throw new Exception("public virtual void Draw() non sovrascritta nella classe derivata");
			}
		public virtual Def.ClipFlag Clip(Vista v)
			{
			throw new Exception("public virtual Def.ClipFlag Clip() non sovrascritta nella classe derivata");
			}
		

		/// <summary>
		/// CopyData
		/// Copia i dati serializzabili, ma non l'ID, dall'elemento 'e' a this
		/// Clone() con estensione o altro: laborioso (https://www.c-sharpcorner.com/article/cloning-objects-in-net-framework/)
		/// </summary>
		/// <param name="e">elemento da cui copiare i dati</param>
		public virtual void CopyData(Elemento e)
			{
			name = e.name;			// Copia il nome
			}

		}	// Fine classe Dato

	/// <summary>
	/// Confronto tra gli ID,
	/// usato per l'ordinamento
	/// </summary>
	public class ElementoComparer : IComparer<Elemento>
		{
		public int Compare(Elemento x, Elemento y)
			{
			if((x == null) || (y == null))
				return 0;
			else
				return x.ID.CompareTo(y.ID);
			}
		}

	
	
	}
