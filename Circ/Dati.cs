﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;						// Serializzazione in Json

namespace Circ
	{

	// Verifica del maxID, per evitare Elementi con ID duplicato
	// 1. public class ListID<T> : List<T> where T : IID {...}: soluzione corretta per gestire maxId, ma laboriosa da derivare (con serializzazione).
	// 2. Elemento {static uint maxId;} : Soluzione pratica, ma rischiosa (se Elemento usato in oggetti diversi).
	// 3. Controlli sul maxID nella calsse Dati: semplice, ma non sicura. ID e List<> sono esposti con proprietà pubbliche, per serializzazione.
	// L'applicazione è a singolo documento, si potrebbe scegliere la seconda soluzione.
	// Però il maxId va ricalcolato ogni tanto, per es. dopo eliminazione di un nodo: maxId deve essere una proprietà esposta, ch può essere alterata dall'esterno.
	// Inoltre, se vengono allocati oggetti temporanei, il maxId cresce in modo incontrollato.
	// Si sceglie l'opzione 3, più semplice

	public class Dati
		{
		string nome;                        // Nome del file (senza path)
		List<Nodo> nodi;                    // Liste dei nodi... 
		List<Ramo> rami;                    // ...e dei rami
		uint freeIDnodo, freeIDramo;		// Contatori per un ID libero
		Def.Stat viewFilter;				// Filtro visualizzazione
		bool maxIDerror;					// Errore se raggiunto max ID. Necessaria rinumerazione

		#warning Aggiunta stato Disattivo (in futuro) agli elementi

		/// <summary>
		/// Costruttore
		/// </summary>
		public Dati()
			{
			nome = string.Empty;
			nodi = new List<Nodo>();		// Scelta una collezione standard invece di una nuova classe (es.: DynArray<T>)
			rami = new List<Ramo>();		// perché altrimenti difficilmente serializzabile con Newtonsoft.Json
			
			freeIDnodo = freeIDramo = 1;	// Inizializza contatori
			maxIDerror = false;				//Azzera flag
			viewFilter = Def.Stat.Nodi | Def.Stat.Rami;		// Filtro per IEnumerable<Elemento>
			
			}


		#region PROPRIETÀ PER SERIALIZZAZIONE

		public string Nome
			{
			get {return nome;}
			set {nome = value;}
			}
		public List<Nodo> Nodi
			{
			get {return nodi;}
			set {nodi = value;}
			}
		public List<Ramo> Rami
			{
			get {return rami;}
			set {rami = value;}
			}

		[JsonIgnore]
		public uint FreeIDNodo
			{
			get {return freeIDnodo;}
			}

		[JsonIgnore]
		public uint FreeIDRamo
			{
			get {return freeIDramo;}
			}



		[JsonIgnore]
		public Def.Stat ViewFilter
			{
			get {return viewFilter;}
			set {viewFilter = value;}
			}
		#endregion

		/// <summary>
		/// Aggiunge un elemento, verificando:
		/// tipo di elemento, capienza liste,
		/// riferimenti agli elementi, rami già esistenti
		/// </summary>
		/// <param name="e">Elemento</param>
		/// <returns>true se riuscito</returns>		
		public bool Add(Elemento e)
			{
			bool ok = false;
			if((e is Nodo) && (!maxIDerror))		// Se è Nodo e con ID accettabile
				{
				e.ID = freeIDnodo;
				freeIDnodo++;
				e.Name = $"N{e.ID}";
				if(nodi.Count < Def.NUM_NODI_MAX)	// Controlla capienza
					{
					nodi.Add((Nodo)e);
					ok = true;
					}
				}
			else if((e is Ramo) && (!maxIDerror))	// Se è Ramo e con ID accettabile
				{
				e.ID = freeIDramo;
				freeIDramo++;
				e.Name = $"R{e.ID}";
				if(AggiornaRifNodiInRamo((Ramo)e))
					{
					if(rami.Count < Def.NUM_RAMI_MAX)	// Controlla capienza
						{
						uint n1 = ((Ramo)e).N1;
						uint n2 = ((Ramo)e).N2;
						if(GetRami(n1, n2).Count == 0)
							{
							rami.Add((Ramo)e);
							ok = true;
							}
						}
					}
				}
			CheckMaxID();
			return ok;
			}
		
		/// <summary>
		/// Imposta i rif. ai nodi usando l'ID
		/// </summary>
		/// <param name="r"></param>
		/// <returns></returns>
		private bool AggiornaRifNodiInRamo(Ramo r)
			{
			bool ok = false;
			Nodo nd1,nd2;
			nd1 = (Nodo)GetElemento(r.N1, Def.Stat.Nodi);
			nd2 = (Nodo)GetElemento(r.N2, Def.Stat.Nodi);
			if( (nd1 != null) && (nd2 !=null) )
				{
				r.Nd1 = nd1;
				r.Nd2 = nd2;
				ok = true;
				}
			return ok;
			}

		/// <summary>
		/// Aggiorna tutti i riferimenti agli elementi usando gli ID
		/// Aggiorna i primi ID liberi (freeID) per nodi e rami
		/// </summary>
		/// <returns></returns>
		public int AggiornaRiferimenti()
			{
			int failed = 0;
			freeIDnodo =  freeIDramo = Elemento.UNASSIGNED;

			uint idn, idr, nn, nr;
			idn = idr = nn = nr = 1;
			nn = (uint)nodi.Count;
			nr = (uint)rami.Count;

			foreach(Nodo n in nodi)
				{
				if(n.ID > idn)
					{
					idn = n.ID;
					}
				}
			foreach(Ramo r in rami)
				{
				if(r.ID > idr)
					{
					idr = r.ID;
					}
				if(!AggiornaRifNodiInRamo(r))
					failed++;
				}
			#if(DEBUG)
			System.Windows.Forms.MessageBox.Show($"nn:{nn}, idn:{idn}, nr:{nr}, idr:{idr}");
			#endif
			freeIDnodo = Math.Max(nn,idn)+1;			// Aggiorna gli indici free ID
			freeIDramo = Math.Max(nr,idr)+1;
			CheckMaxID();								// Verifica se superato il max ID
			return failed;
			}
	
		/// <summary>
		/// Verifica se raggiunto il limite di maxID
		/// Se sì, imposta il flag
		/// </summary>
		/// <returns></returns>
		private bool CheckMaxID()
			{
			if((freeIDnodo > Def.ID_NODI_MAX)||(freeIDramo > Def.ID_RAMI_MAX))
				{
				maxIDerror = true;
				}
			else
				{
				maxIDerror = false;
				}
			return maxIDerror;
			}

		/// <summary>
		/// Enumeratore di tutti gli elementi
		/// filtrati con ViewFiter
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Elemento> Elementi()	
			{
			if((viewFilter & Def.Stat.Nodi) != 0)
				{
				foreach (Elemento n in nodi)
					{
					yield return n;
					}
				}
			if((viewFilter & Def.Stat.Rami) != 0)
				{
				foreach (Elemento r in rami)
					{
					yield return r;
					}
				}
			yield break;
			}

		/// <summary>
		/// Ordina gli elementi nella lista
		/// in base alla classe di confronto (sugli ID)
		/// </summary>
		private void Sort()
			{
			nodi.Sort(new ElementoComparer());
			rami.Sort(new ElementoComparer());
			}

		/// <summary>
		/// Trova l'Elemento con l'ID richiesto
		/// </summary>
		/// <param name="id">ID</param>
		/// <param name="typ">Tipo di elemento o combinazione di flag</param>
		/// <returns>Elemento</returns>
		public Elemento GetElemento(uint id, Def.Stat typ)
			{
			Elemento el = null;
			if(id != Elemento.UNASSIGNED)
				{
				if( (typ & Def.Stat.Nodi) != 0 )		//if(typ == Def.Stat.Nodi)
					{
					foreach(Nodo x in nodi)
						{
						if(x.ID == id)
							{
							el = x;
							break;
							}
						}
					}
				else if( (typ & Def.Stat.Rami) != 0)	//else if(typ == Def.Stat.Rami)
					{
					foreach(Ramo x in rami)
						{
						if(x.ID == id)
							{
							el = x;
							break;
							}
						}
					}
				}
			return el;
			}

		/// <summary>
		/// Trova gli Elementi con l'ID richiesto
		/// </summary>
		/// <param name="id">ID</param>
		/// <param name="typ">Tipo di elemento o combinazione di flag</param>
		/// <returns>List<Elemento></returns>
		public List<Elemento> GetElementi(uint id, Def.Stat typ)
			{
			List<Elemento> el = new List<Elemento>();
			if(id != Elemento.UNASSIGNED)
				{
				if( (typ & Def.Stat.Nodi) != 0 )
					{
					foreach(Nodo x in nodi)
						{
						if(x.ID == id)
							{
							el.Add(x);
							break;
							}
						}
					}
				else if( (typ & Def.Stat.Rami) != 0)
					{
					foreach(Ramo x in rami)
						{
						if(x.ID == id)
							{
							el.Add(x);
							break;
							}
						}
					}
				}
			return el;
			}

		/// <summary>
		/// Trova i rami tra due nodi
		/// </summary>
		/// <param name="id1">id di un nodo</param>
		/// <param name="id2">id di un altro nodo</param>
		/// <returns></returns>
		public List<Elemento> GetRami(uint id1, uint id2)
			{
			List<Elemento> l = new List<Elemento>();
			foreach(Ramo r in rami)
				{
				if( (r.N1 == id1) && (r.N2 == id2) || (r.N1 == id2) && (r.N2 == id1) )
					{
					l.Add(r);
					}
				}
			return l;
			}
		
		/// <summary>
		/// Trova tutti gli elementi selezionati
		/// </summary>
		/// <param name="selezionati"></param>
		/// <returns></returns>
		public List<Elemento> GetSelezionati(bool selezionati = true)
			{
			List<Elemento> l = new List<Elemento>();
			foreach(Ramo el in rami)
				{
				if(el.Selected == selezionati)
					{
					l.Add(el);
					}
				}
			foreach(Nodo el in nodi)
				{
				if(el.Selected == selezionati)
					{
					l.Add(el);
					}
				}
			return l;
			}

		/// <summary>
		/// Conta il numero di nodi e rami selezionati
		/// </summary>
		/// <param name="selezionati"></param>
		/// <returns></returns>
		public Tuple<uint,uint> ContaNodiRamiSelezionati(bool selezionati = true)
			{
			uint cn, cr;
			cn = cr = 0;
			foreach(Ramo el in rami)
				{
				if(el.Selected == selezionati)
					{
					cr++;
					}
				}
			foreach(Nodo el in nodi)
				{
				if(el.Selected == selezionati)
					{
					cn++;
					}
				}
			return new Tuple<uint, uint>(cn, cr);
			}

		/// <summary>
		/// Tutti gli elementi che usano il nodo con ID idnodo
		/// </summary>
		/// <param name="IDnodo"></param>
		/// <returns></returns>
		public List<Elemento> GetElementiUsing(uint IDnodo)
			{
			List<Elemento> l = new List<Elemento>();
			foreach(Ramo r in rami)
				{
				if( (r.N1 == IDnodo) || (r.N2 == IDnodo ))
					{
					l.Add(r);
					}
				}
			return l;
			}

		/// <summary>
		/// Estensione (coordinate min e max di tutti gli elementi)
		/// </summary>
		/// <returns></returns>
		public Tuple<Point2D,Point2D>GetExtension()
			{
			Point2D pmin = new Point2D();
			Point2D pmax = new Point2D();

			pmin.X = pmin.Y = Def.MAX_VALUE;			// Calcola l'estensione del rettanglo che racchiude tutti i nodi
			pmax.X = pmax.Y = Def.MIN_VALUE;
			foreach(Nodo n in nodi)			
				{
				if(n.P.X < pmin.X)	{ pmin.X = n.P.X; }
				if(n.P.Y < pmin.Y)	{ pmin.Y = n.P.Y; }
				if(n.P.X > pmax.X)	{ pmax.X = n.P.X; }
				if(n.P.Y > pmax.Y)	{ pmax.Y = n.P.Y; }
				}
			Point2D size =								// Calcola rettangolo...
						new Point2D(
								Math.Max(pmax.X,pmin.X)-Math.Min(pmax.X,pmin.X),
								Math.Max(pmax.Y,pmin.Y)-Math.Min(pmax.Y,pmin.Y)
								);
			Point2D center = pmax/2 + pmin/2;			// ...e centro. Con valori grandi, prima divide, poi somma
			if(size.X < Def.EPSILON || size.X > Def.MAX_VALUE)					// Corregge se necessario (uno o nessun punto)
				{
				size.X = 2 * Def.ZOOM_DEFAULT_SIZE;
				}
			if(size.Y < Def.EPSILON || size.Y > Def.MAX_VALUE)					// Corregge se necessario e...
				{
				size.Y = 2 * Def.ZOOM_DEFAULT_SIZE;
				}
			pmin = center - size/2;						// Ricalcola le estensioni
			pmax = center + size/2;

			return new Tuple<Point2D,Point2D>(pmin,pmax);
			}

		/// <summary>
		/// Rinumera l'ID di un ramo
		/// </summary>
		/// <param name="oldID"></param>
		/// <param name="newID"></param>
		/// <returns>true se ok</returns>
		public bool RinumeraIDramo(uint oldID, uint newID)
			{
			return RinumeraIDramo(oldID, newID, true);
			}

		/// <summary>
		/// Rinumera l'ID di un ramo (private)
		/// </summary>
		/// <param name="oldID"></param>
		/// <param name="newID"></param>
		/// <param name="check_max">Controlla se supera l'ID_MAX</param>
		/// <returns></returns>
		private bool RinumeraIDramo(uint oldID, uint newID, bool check_max)
			{
			bool ok = false;
			List<Elemento> _old, _new;
			_old = GetElementi(oldID, Def.Stat.Rami);		// Rami con l'id attuale (vecchio)
			_new = GetElementi(newID, Def.Stat.Rami);		// Rami con l'id nuovo
			if((_old.Count == 1) && (_new.Count == 0))
				{
				if( (newID < Def.ID_NODI_MAX) || !check_max)
					{
					_old[0].ID = newID;							// Aggiorna l'ID del ramo
					ok = true;
					}
				}
			return ok;
			}

		/// <summary>
		/// Rinumera l'ID di un nodo
		/// </summary>
		/// <param name="oldID"></param>
		/// <param name="newID"></param>
		/// <returns>true se ok</returns>
		public bool RinumeraIDnodo(uint oldID, uint newID)
			{
			return RinumeraIDnodo(oldID, newID, true);
			}

		/// <summary>
		/// Rinumera l'ID di un nodo (private)
		/// </summary>
		/// <param name="oldID"></param>
		/// <param name="newID"></param>
		/// <param name="check_max"></param>
		/// <returns></returns>
		public bool RinumeraIDnodo(uint oldID, uint newID, bool check_max)
			{
			bool ok = false;
			List<Elemento> _old, _new;
			_old = GetElementi(oldID, Def.Stat.Nodi);		// Nodi con l'id attuale (vecchio)
			_new = GetElementi(newID, Def.Stat.Nodi);		// Nodi con l'id nuovo
			if((_old.Count == 1) && (_new.Count == 0))
				{
				if((newID < Def.ID_NODI_MAX) || !check_max)
					{
					_old[0].ID = newID;							// Aggiorna l'ID del nodo
					foreach(Ramo r in rami)						// Percorre tutti i rami
						{
						if(r.N1 == oldID)	r.N1 = newID;		// Se trova l'ID vecchio del nodo vecchio...
						if(r.N2 == oldID)	r.N2 = newID;		// ...lo aggiorna con l'ID nuovo.
						}				// Gli elementi non cambiano ed i puntatori Nd1 e Nd2 ai nodo non devono esser aggiornati
					ok = true;
					}
				}
			return ok;
			}

		/// <summary>
		/// Compatta gli ID, rinumerandoli in modo contiguo da 1 in poi
		/// </summary>
		/// <returns>false se errore in una rinumerazione (rischio id errati)</returns>
		public bool CompattaID()
			{
			uint counterId;
			bool ok = true;
			counterId = Def.ID_NODI_MAX;			// Rinumera nodi e rami, in ordine, dopo l'ID_MAM
			foreach(Nodo n in nodi)
				{
				ok &= RinumeraIDnodo(n.ID, counterId,false);	// Controllo ID_MAX disabilitato
				counterId++;
				}
			counterId = Def.ID_RAMI_MAX;
			foreach(Ramo r in rami)
				{
				ok &= RinumeraIDramo(r.ID, counterId,false);
				counterId++;
				}
			counterId=1;							// Rinumera nodi e rami, in ordine, da 1 in poi
			foreach(Nodo n in nodi)				
				{
				ok &= RinumeraIDnodo(n.ID, counterId, false);
				counterId++;
				}
			counterId=1;
			foreach(Ramo r in rami)
				{
				ok &= RinumeraIDramo(r.ID, counterId, false);
				counterId++;
				}
			return ok;
			}

		/// <summary>
		/// Elimina il ramo ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool EliminaRamo(uint id)
			{
			bool ok = true;
			List<Elemento> l = GetElementi(id, Def.Stat.Rami);
			foreach(Ramo r in l)
				{
				rami.Remove(r);
				}
			return ok;
			}

		/// <summary>
		/// Elimina il nodo ID, se non è utilizzato
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public bool EliminaNodo(uint id)
			{
			bool ok = true;
			List<Elemento> l = GetElementi(id, Def.Stat.Nodi);
			foreach(Nodo n in l)
				{
				if(GetElementiUsing(n.ID).Count == 0)
					{
					nodi.Remove(n); 
					}
				else
					{
					ok = false;
					}
				}
			return ok;
			}

		/// <summary>
		/// Elimina gli elementi selezionati
		/// </summary>
		public void EliminaSelezionati()
			{
			List<Elemento> esel;
			esel = GetSelezionati(true);
			foreach(Elemento x in esel)			// Prima elimina i rami
				{
				if(x is Ramo)
					EliminaRamo(x.ID);
				}
			esel = GetSelezionati(true);		// Poi aggiorna la lista dei selezionati e...		
			foreach(Elemento x in esel)			// ...elimina i nodi
				{
				if(x is Nodo)
					EliminaNodo(x.ID);
				}
			}

		/// <summary>
		/// Muove gli elementi selezionati
		/// </summary>
		/// <param name="vect"></param>
		/// <returns></returns>
		public int MuoviSelezionati(Point2D vect)
			{
			int count = 0;
			foreach(Ramo r in rami)
				{
				if(r.Selected)
					{
					r.Nd1.Selected = r.Nd2.Selected = true;
					}
				}
			foreach(Nodo n in nodi)
				{
				if(n.Selected)
					{
					n.P = n.P + vect;
					count++;
					}
				}
			return count;
			}

		}	// Fine classe DatiCirc
	}