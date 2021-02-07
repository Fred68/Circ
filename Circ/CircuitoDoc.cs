using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;				// Point struct
using Newtonsoft.Json;				// Serializzazione in Json
using Fred68.Tools.Messaggi;

namespace Circ
	{

	class CircuitoDoc
		{
		static int indx = 1;			// Indice per il nome del documento
		string filename;				// Nome file completo
		bool _isModified;				// flag, se ci sono modiiche da salvare

		Dati dati;						// Classe con i dati del circuito

		public readonly List<Elemento> selezionati;     // Lista elementi selezionati



#warning	Aggiungere sposta su griglia dei nodi (non dinamica, troppo complesso)

#warning	Collassa ramo (restituisce nodi)
#warning	Collassa due o più nodi

#warning	Testo con sfondo
#warning	Sagome 2D con colore di sfondo
#warning	Spostare gli indici dei colori sotto vista ed usarla per diegnare
#warning	Aggiungere testo per dati aggiuntivi
#warning	Menù checked con gli oggetti da vedere nella descrizione (dinamico, usare Reflection)

#warning	Eseguibile separato nello stesso progetto
#warning	Eseguibile separato per il calcolo e la semplificazione (console) e salvataggio su file di scambio
#warning	Dati aggiuntivi ai nodi
#warning	Suoni
#warning	Spostare tutte le modifiche ai dati sotto documento (per impostare il flag privato _isModied)
#warning	Importante: per gli errori, usare messaggi.clear() nella chiamata più a monte (in mainform.cs, possibilmente)


		#region PROPRIETÀ (e SERIALIZZAZIONE)

		public Dati Dati
			{
			get {return dati;}
			set {dati = value;}
			}
		
		[JsonIgnore]
		public bool IsModified
			{
			get {return _isModified;}
			set {							// Ammette soltanto di attivare _isModified, non di azzerarlo !
				if(value == true)
					_isModified = true;
				}
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
		
		/// <summary>
		/// Chiude il documento
		/// </summary>
		public void Chiudi()
			{
			selezionati.Clear();
			}

		/// <summary>
		/// Salva il documento su strem
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="fileName"></param>
		/// <returns>true se ok</returns>
		public bool Save(Stream stream, string fileName)
			{
			bool ok = true;
			selezionati.Clear();
			this.filename = fileName;
			#if(DEBUG)
			System.Windows.Forms.MessageBox.Show(filename);
			#endif
			try
				{
				dati.Nome = Path.GetFileName(this.filename);
				}
			catch(Exception e)
					{
					ok = false;
					Messaggi.AddMessage(Messaggi.ERR.ERRORE_FILE,e.ToString(),Messaggi.Tipo.Errori);
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
					Messaggi.AddMessage(Messaggi.ERR.ERRORE_SAVE,e.ToString(),Messaggi.Tipo.Errori);
					}
				}

			if(ok)
				{
				_isModified = false;
				}
			return ok;
			}

		/// <summary>
		/// Carica il documento da stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="fileName"></param>
		/// <returns>true se ok</returns>
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
				Messaggi.AddMessage(Messaggi.ERR.ERRORE_OPEN,e.ToString(),Messaggi.Tipo.Errori);
				}
			int failed;
			if( (failed = dati.AggiornaRiferimenti()) > 0)
				{
				ok = false;
				Messaggi.AddMessage(Messaggi.ERR.AGGIORNA_RIFERIMENTI,$"Falliti {failed} rami",Messaggi.Tipo.Errori);
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
			Nodo n = new Nodo(p);
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
		/// Aggiunge un ramo tra un nodo ed un punto
		/// </summary>
		/// <param name="id1">id primo nodo </param>
		/// <param name="p2">Point2d secondo nodo</param>
		/// <returns>Elemento o null</returns>
		public Elemento AddRamo(uint id1, Point2D p2)
			{
			Ramo r = null;
			Nodo n2 = (Nodo)AddNodo(p2);
			if(n2 != null)
				r = (Ramo)AddRamo(id1, n2.ID);
			return r;
			}

		/// <summary>
		/// Aggiunge un ramo tra due punti
		/// </summary>
		/// <param name="p1">Point2d</param>
		/// <param name="p2">Point2d</param>
		/// <returns>Elemento o null</returns>
		public Elemento AddRamo(Point2D p1, Point2D p2)
			{
			Ramo r = null;
			Nodo n1 = (Nodo)AddNodo(p1);
			Nodo n2 = (Nodo)AddNodo(p2);
			if((n1 != null)&&(n2 != null))
				r = (Ramo)AddRamo(n1.ID, n2.ID);
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

		/// <summary>
		/// Divide in due i rami selezionati
		/// </summary>
		public void DivideRamiSelezionati()
			{
			List<Elemento> sel = dati.GetSelezionati(true);		// Cerca gli elementi selezionati
			foreach(Elemento e in sel)
				{
				if(e is Ramo)
					{
					DividiRamo((Ramo)e);
					_isModified = true;
					}
				}
			}

		/// <summary>
		/// Divide in due un ramo
		/// </summary>
		/// <param name="r"></param>
		private void DividiRamo(Ramo r)
			{
			Nodo n = (Nodo)AddNodo(Point2D.Midpoint(r.Nd1.P, r.Nd2.P));		// Nuovo nodo nel punto medio
			uint id1, id2, id3;
			id1 = r.N1;							// I nuovi id degli estremi
			id2 = n.ID;
			id3 = r.N2;

			Dati.EliminaRamo(r.ID);				// Elimina il ramo r
			Ramo r1 = (Ramo)AddRamo(id1,id2);	// Crea i nuovi rami
			Ramo r2 = (Ramo)AddRamo(id2,id3);

			r1.CopyData(r);						// Copia le proprietà
			r2.CopyData(r);
			r1.Name += ".1";					// Diversifica i nomi		
			r2.Name += ".2";
			
			}

		}	// Fine classe CircuitoDoc


	}
