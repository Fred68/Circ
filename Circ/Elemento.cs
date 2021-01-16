using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;						// Serializzazione in Json
using System.Drawing;						// Point e altro

namespace Circ
	{
	public class Elemento : IID /*, IDraw*/
		{
		public static uint UNASSIGNED = 0;
		uint id;					// ID dell'oggetto
		string name;				// Descrizione
		bool sel;					// Selezionato

		/// <summary>
		/// Costruttore
		/// </summary>
		public Elemento()
			{
			name = string.Empty;
			id = UNASSIGNED;
			sel = false;
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

		#endregion

		public virtual void Draw(Vista v)
			{
			throw new Exception("public virtual void Draw(...) non sovrascritta nella classe derivata");
			}
		public virtual Point Center(Vista v)
			{
			throw new Exception("public virtual Point Center(...) non sovrascritta nella classe derivata");
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
