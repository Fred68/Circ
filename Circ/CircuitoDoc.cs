using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;				// Point struct
using Newtonsoft.Json;				// Serializzazione in Json


namespace Circ
	{

	class CircuitoDoc
		{
		static int indx = 1;			// Indice per il nome del documento
		string filename;				// Nome file completo
		bool _isModified;				// flag, se ci sono modiiche da salvare

		Dati dati;						// Classe con i dati del circuito

		public readonly List<Elemento> selezionati;		// Lista elementi selezionati

		#region PROPRIETÀ PER SERIALIZZAZIONE

		public Dati Dati
			{
			get {return dati;}
			set {dati = value;}
			}
		
		#endregion

		/// <summary>
		/// Costruttore
		/// </summary>
		public CircuitoDoc()
			{
			dati = new Dati();
			selezionati = new List<Elemento>();
			dati.Nome = "Circuito"+(indx.ToString()).PadLeft(3,'0');
			indx++;
			filename = string.Empty;
			_isModified = false;
			}
		
		public bool IsModified()
			{
			return _isModified;
			}

		public void Chiudi()
			{
			selezionati.Clear();
			}

		public bool Save(Stream stream, string fileName)
			{
			bool ok = true;
			selezionati.Clear();
			this.filename = fileName;
			System.Windows.Forms.MessageBox.Show(filename);
			try
				{
				dati.Nome = Path.GetFileName(this.filename);
				}
			catch(Exception e)
					{
					ok = false;
					System.Windows.Forms.MessageBox.Show(e.ToString());
					}

			if (ok)
				{
				try
					{
					using(StreamWriter sw = new StreamWriter(stream))
						{
						string jsonstring = JsonConvert.SerializeObject(dati,Formatting.Indented);
						sw.Write(jsonstring);
						}
					}
				catch(Exception e)
					{
					ok = false;
					System.Windows.Forms.MessageBox.Show(e.ToString());
					}
				}

			if(ok)
				{
				_isModified = false;
				}
			return ok;
			}

		public bool Open(Stream stream, string fileName)
			{

			bool ok = true;
			selezionati.Clear();
			this.filename = fileName;
			try
				{
				using(StreamReader sr = new StreamReader(stream))
					{
					var serializer = new JsonSerializer();
					dati = (Dati) serializer.Deserialize(sr,typeof(Dati));
					}
				}
			catch(Exception e)
				{
				ok = false;
				System.Windows.Forms.MessageBox.Show(e.ToString());
				}
			int failed;
			if( (failed = dati.AggiornaRiferimenti()) > 0)
				{
				ok = false;
				System.Windows.Forms.MessageBox.Show($"Falliti {failed} rami");
				}
			if(ok)
				{
				_isModified = false;
				}
			return ok;
			}

		/// <summary>
		/// Aggiunge un nodo
		/// </summary>
		/// <param name="p">Point2d</param>
		/// <returns></returns>
		public Elemento AddNodo(Point2D p)
			{
			Nodo n = new Nodo();
			n.P = p;
			if(!dati.Add(n))
				{
				return null;
				}
			_isModified = true;
			return n;
			}

		/// <summary>
		/// Aggiunge un ramo tra due nodi
		/// </summary>
		/// <param name="id1">primo id</param>
		/// <param name="id2">secondo id</param>
		/// <returns>Elemento o null</returns>
		public Elemento AddRamo(uint id1, uint id2)
			{
			Ramo r = new Ramo();
			r.N1 = id1;
			r.N2 = id2;
			if(!dati.Add(r))
				{
				return null;
				}
			_isModified = true;
			return r;
			}

		/// <summary>
		/// Seleziona l'ultimo elemento evidenziato
		/// </summary>
		public void SelectLastElement(Vista v)
			{
			Elemento el = v.GetLastHighLighted();
			if(el != null)
				{
				el.Selected = !el.Selected;
				if(el.Selected)
					selezionati.Add(el);
				else
					selezionati.Remove(el);
				}
			}
		
		/// <summary>
		/// Deseleziona tutti gli elementi
		/// </summary>
		public void UnselectAll()
			{
			foreach(Elemento el in selezionati)
				{
				el.Selected = false;
				}
			selezionati.Clear();
			}

		}	// Fine classe CircuitoDoc


	}
