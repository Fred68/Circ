using Fred68.Tools.Messaggi;
using Newtonsoft.Json;              // Serializzazione in Json
using System;
using System.Collections.Generic;
using System.IO;

namespace Circ
	{

	class CircuitoDoc
		{
		static int indx = 1;            // Indice per il nome del documento
		string filename;                // Nome file completo
		bool _isModified;               // flag, se ci sono modiiche da salvare

		Dati dati;                      // Classe con i dati del circuito

		public readonly List<Elemento> selezionati;     // Lista elementi selezionati

#warning	Selezioni varie: tutte con pulsante destro
#warning	Dragging con dx: finestra rettangolare
#warning	Context menù con tasto destro

#warning	Aggiungere comando Duplica elementi

#warning	Far lampeggiare gli elementi selezionati (sfruttare il timer)

#warning	Testo con sfondo
#warning	Sagome 2D con colore di sfondo
#warning	Aggiungere testo per dati aggiuntivi
#warning	Menù checked con gli oggetti da vedere nella descrizione (dinamico, usare Reflection)

#warning	Eseguibile separato per il calcolo e salvataggio su file di scambio (meglio se in C++, non .NET).
//			Dati aggiuntivi fisici. double[] + delegate. No, perché i calcoli sono differenti per acqua, aria, elettricità.
#warning	Usare classi per dati nodo e dati ramo + interfaccia per le funzioni comuni
#warning	Per i nomi delle grandezze, usare Reflection (oppure lista), ma non per i calcoli.
#warning	VEDERE APPUNTI SPECIFICI PER I CALCOLI

#warning	Suoni
#warning	Spostare tutte le modifiche ai dati sotto documento (per impostare il flag privato _isModfied)
#warning	Importante: per gli errori, usare messaggi.clear() nella chiamata più a monte (in mainform.cs, possibilmente)


/*
 
A nodi e rami si devono associare alcune grandezze (double), in base al tipo di calcolo.
Elettrico: R1, R2 (R nelle due direzioni; R interna ecc...), I (corrente imposta), U (tensione imposta), V (potenziale), i (corrente).
Fluido: P (press.), Q (portata), D (diametro), eps (scabrezza ass.), Re (numero di Reynolds), L (lunghezza), csi (perdita concentrata), H (altezza)...
Altri valori: T (temperatura), Potenza dissipata, ecc...

Vedere appunti specifici

 */

		#region PROPRIETÀ (e SERIALIZZAZIONE)

		/// <summary>
		/// Dati del circuito
		/// </summary>
		public Dati Dati
			{
			get { return dati; }
			set { dati = value; }
			}

		[JsonIgnore]
		public bool IsModified
			{
			get { return _isModified; }
			set
				{                           // Ammette soltanto di attivare _isModified, non di azzerarlo !
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
			dati.Nome = "Circuito" + (indx.ToString()).PadLeft(3,'0');
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
		public bool Save(Stream stream,string fileName)
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

			if(ok)
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
		public bool Open(Stream stream,string fileName)
			{
			bool ok = true;
			selezionati.Clear();
			this.filename = fileName;
			try
				{
				using(StreamReader sr = new StreamReader(stream))
					{
					var serializer = new JsonSerializer();
					dati = (Dati)serializer.Deserialize(sr,typeof(Dati));
					}
				}
			catch(Exception e)
				{
				ok = false;
				Messaggi.AddMessage(Messaggi.ERR.ERRORE_OPEN,e.ToString(),Messaggi.Tipo.Errori);
				}
			int failed;
			if((failed = dati.AggiornaRiferimenti()) > 0)
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
		public Elemento AddRamo(uint id1,uint id2)
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
		public Elemento AddRamo(uint id1,Point2D p2)
			{
			Ramo r = null;
			Nodo n2 = (Nodo)AddNodo(p2);
			if(n2 != null)
				r = (Ramo)AddRamo(id1,n2.ID);
			return r;
			}

		/// <summary>
		/// Aggiunge un ramo tra due punti
		/// </summary>
		/// <param name="p1">Point2d</param>
		/// <param name="p2">Point2d</param>
		/// <returns>Elemento o null</returns>
		public Elemento AddRamo(Point2D p1,Point2D p2)
			{
			Ramo r = null;
			Nodo n1 = (Nodo)AddNodo(p1);
			Nodo n2 = (Nodo)AddNodo(p2);
			if((n1 != null) && (n2 != null))
				r = (Ramo)AddRamo(n1.ID,n2.ID);
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
		public void DivideSelezionati()
			{
			List<Elemento> sel = dati.GetSelezionati(true);     // Cerca gli elementi selezionati
			foreach(Elemento e in sel)
				{
				if(e is Ramo)
					{
					DivideRamo((Ramo)e);
					_isModified = true;
					}
				else if(e is Nodo)
					{
					DivideNodo((Nodo)e);
					_isModified = true;
					}
				}
			}

		/// <summary>
		/// Scambia il verso dei rami selezionati
		/// </summary>
		public void InverteRamiSelezionati()
			{
			List<Elemento> sel = dati.GetSelezionati(true);     // Cerca gli elementi selezionati
			foreach(Elemento e in sel)
				{
				if(e is Ramo)
					{
					InverteRamo((Ramo)e);
					_isModified = true;
					}
				}
			}

		/// <summary>
		/// Divide in due un ramo
		/// </summary>
		/// <param name="r"></param>
		private void DivideRamo(Ramo r)
			{
			Nodo n = (Nodo)AddNodo(Point2D.Midpoint(r.Nd1.P,r.Nd2.P));      // Nuovo nodo nel punto medio
			uint id1, id2, id3;
			id1 = r.N1;                         // I nuovi id degli estremi
			id2 = n.ID;
			id3 = r.N2;

			dati.EliminaRamo(r.ID);             // Elimina il ramo r
			Ramo r1 = (Ramo)AddRamo(id1,id2);   // Crea i nuovi rami
			Ramo r2 = (Ramo)AddRamo(id2,id3);

			r1.CopyData(r);                     // Copia le proprietà
			r2.CopyData(r);
			r1.Name += ".1";                    // Diversifica i nomi		
			r2.Name += ".2";
			}

		/// <summary>
		/// Divide il nodo selezionato, aggiungendo un nodo per ogni ramo connesso
		/// </summary>
		/// <param name="n"></param>
		private void DivideNodo(Nodo n)
			{
			List<Elemento> lr = dati.GetElementiUsing(n.ID);
			int count = 0;
			foreach(Elemento r in lr)
				{
				if(r is Ramo)
					{
					Point2D spost;
					Nodo nn = (Nodo)AddNodo(n.P);
					nn.CopyData(n);
					nn.Name += $".{count++}";
					if(((Ramo)r).N1 == n.ID)
						{
						((Ramo)r).N1 = nn.ID;
						((Ramo)r).Nd1 = nn;
						spost = (((Ramo)r).Nd2.P - ((Ramo)r).Nd1.P) / 10;
						}
					else
						{
						((Ramo)r).N2 = nn.ID;
						((Ramo)r).Nd2 = nn;
						spost = (((Ramo)r).Nd1.P - ((Ramo)r).Nd2.P) / 10;
						}
					nn.P += spost;
					}
				}
#if DEBUG
			System.Windows.Forms.MessageBox.Show($"count = {count}");
#endif
			}
		/// <summary>
		/// Scambia i nodi del ramo
		/// </summary>
		/// <param name="r"></param>
		private void InverteRamo(Ramo r)
			{
			Nodo nTmp;
			uint idTmp;
			idTmp = r.N1;
			nTmp = r.Nd1;
			r.N1 = r.N2;
			r.Nd1 = r.Nd2;
			r.N2 = idTmp;
			r.Nd2 = nTmp;
			}

		/// <summary>
		/// Allinea tutti i nodi alla griglia della vista
		/// </summary>
		/// <param name="v">Vista con la griglia</param>
		public void AllineaAllaGriglia(Vista v)
			{
			foreach(Elemento el in Dati.Elementi())
				{
				if(el is Nodo)
					{
					Point2D rp = ((Nodo)el).P / v.GridStep;     // Rapporti 
					rp.X = Math.Round(rp.X,0) * v.GridStep;
					rp.Y = Math.Round(rp.Y,0) * v.GridStep;
					((Nodo)el).P.X = rp.X;
					((Nodo)el).P.Y = rp.Y;
					}
				}
			_isModified = true;
			}

		/// <summary>
		/// Collassa i nodi selezionati sul primo della lista
		/// </summary>
		/// <param name="removeNodes">Elimina i nodi collassati</param>
		/// <returns></returns>
		public int CollassaSelezionati(bool removeNodes = false)
			{
			int count = 0;
			List<Elemento> sel;
			sel = dati.GetSelezionati(true,Def.Stat.Nodi);          // Cerca i nodi selezionati
			if(sel.Count > 1)                                       // Se sono selezionati almeno due nodi
				{
				for(int i = 1;i < sel.Count;i++)                        // Percorre la lista dal 2° elemento
					{
					dati.CollassaNodi(sel[0].ID,sel[i].ID);     // Collassa i nodi sul primo nodo
					if(removeNodes)
						{
						dati.EliminaNodo(sel[i].ID);                // Elimina i nodi, se richiesto (e se è possibile)
						}
					_isModified = true;
					}
				}
			sel = dati.GetSelezionati(true,Def.Stat.Rami);          // Cerca i rami selezionati
			foreach(Elemento e in sel)
				{
				dati.EliminaRamo(e.ID,true);
				_isModified = true;
				}
			return count;
			}

		/// <summary>
		/// Inverte la selezione di tutti gli elementi (filtrati con view Filter)
		/// </summary>
		public void InverteSelezione()
			{
			//dati.ViewFilter = Def.Stat.Tutti;
			foreach(Elemento e in dati.Elementi())
				{
				e.Selected = !e.Selected;
				}
			}

		}   // Fine classe CircuitoDoc


	}
