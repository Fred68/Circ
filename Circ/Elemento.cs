using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;						// Serializzazione in Json
using System.Drawing;						// Point e altro

namespace Circ
	{
	public class Elemento : IID, IDraw
		{
		public static uint UNASSIGNED = 0;
		uint id;					// ID dell'oggetto
		string name;				// Descrizione
		bool sel;					// Selezionato
		Def.ClipFlag clipped;		// Fuori dalla vista attuale (non in display list)

		/// <summary>
		/// Costruttore
		/// </summary>
		public Elemento()
			{
			name = string.Empty;
			id = UNASSIGNED;
			sel = false;
			clipped = Def.ClipFlag.Inside;
			}

		#region PROPRIETÀ PER SERIALIZZAZIONE

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

		#endregion

		public virtual void Regen(Vista v)
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
