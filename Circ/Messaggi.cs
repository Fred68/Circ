using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#warning IMPORTANTE: PER GLI ERRORI, USARE Messaggi.Clear() nella chiamata più a monte (in MainForm.cs, possibilmente)

namespace Fred68.Tools.Messaggi
	{

	/// <summary>
	/// Singolo messaggio
	/// </summary>
	public class MessaggioErrore
		{
		public string Messaggio { get; set; }
		public string Dettaglio { get; set; }
		public MessaggioErrore(string msg, string det)
			{
			Messaggio = msg;
			Dettaglio = det;
			}
		public string ToLine()
			{
			return Messaggio + ((Dettaglio.Length > 0) ? Messaggi.SeparatoreMsg : "") + Dettaglio + System.Environment.NewLine;
			}
		}

	/// <summary>
	/// Classe con tutti i messaggi
	/// </summary>
	static public class Messaggi
		{
		static int LISTE = 2;
		static List<MessaggioErrore>[] _msg = new List<MessaggioErrore>[LISTE];
		static int[] _ultimiMsg = new int[LISTE];

		public static string SeparatoreMsg = " - ";
  
		/// <summary>
        /// Messaggi di errore
        /// </summary>
		public struct ERR
			{
			public static string CIRCUITO_NON_CONNESSO = "Circuito non connesso";
			public static string ERRORE_RINUMERAZIONE = "Errore nella rinumerazione";
			public static string ERRORE_RINUMERAZIONE_NODO = "Errore nella rinumerazione dei nodi";
			public static string ERRORE_RINUMERAZIONE_RAMO = "Errore nella rinumerazione dei rami";
			public static string SELEZIONE_ERRATA = "Errore nella selezione";
			public static string ERRORE_FILE = "Errore sul file";
			public static string ERRORE_SAVE = "Errore nel salvataggio dei dati";
			public static string ERRORE_OPEN = "Errore nell'apertura del file";
			public static string AGGIORNA_RIFERIMENTI = "Errore nell'aggiornamento dei riferimenti";
			}
 
			/// <summary>
        /// Messaggi informativi
        /// </summary>
		public struct MSG
			{
			public static string SELEZIONARE_UN_NODO = "Selezionare un nodo";
			public static string SELEZIONARE_DUE_NODI = "Selezionare due nodi";
			public static string SELEZIONARE_UN_SOLO_NODO = "Selezionare un solo nodo";
			public static string SELEZIONARE_UN_ELEMENTO = "Selezionare un elemento";
			public static string SELEZIONARE_UN_SOLO_ELEMENTO = "Selezionare un solo elemento";
			public static string ELIMINARE_ELEMENTI = "Eliminare:\n{0} nodi\n{1} rami\nselezionati ?";
			public static string SALVARE_FILE = @"Salvare il documento {0} ?";
			public static string EXIT = "Uscire dal programma ?";
			}

		/// <summary>
		/// Messaggi dell'interfaccia grafica (dialog box ecc...)
		/// </summary>
		public struct GUI
			{
			public struct MSG
				{
				public static string MESSAGGIO2 = @"Messaggio";
				}
			public struct TIT
				{
				public static string TITOLO1 = @"Titolo";
				}
			public struct TOOLTIP
				{

				/// <summary>
				/// Barra
				/// </summary>
				public static string TOOLTIP1 = "Tooltip";

				/// <summary>
				/// Pulsanti ed altre entità
				/// </summary>
				public static string TOOLTIP2= @"Tooltip";
				
				}
			}
		
		public enum Tipo {Messaggi=0, Errori, NUM};

		#warning AGGIUNGERE ANCHE MESSAGEBOX.SHOW() MODIFICATO PER I MESSAGGI, PER SEMPLIFICARE LE CHIAMATE.
		#warning AGGIUNGERE ADDMESSAGE() CON OPZIONE MESSAGEBOX.SHOW() IMMEDIATO

		/// <summary>
		/// Costruttore statico
		/// </summary>
		static Messaggi()
			{
			for(int i=0; i < (int)Tipo.NUM; i++)
				{
				_msg[i] = new List<MessaggioErrore>();
				}
			Reset();
			}		

		/// <summary>
		/// Proprietà che indica se ci sono errori
		/// </summary>
		public static bool hasError
			{
			get {return (_msg[(int)Tipo.Errori].Count>0); }
			}

		/// <summary>
		/// Aggiunge un messaggio
		/// </summary>
		/// <param name="msg">Messaggio, string</param>
		/// <param name="dett">Dettagli, string</param>
		/// <param name="typ">Tipo: errore o messaggio</param>
		public static void AddMessage(string msg, string dett = "", Tipo typ = Tipo.Messaggi)
			{
			int i = (int)typ;
			if( (i>=0) && (i<(int)Tipo.NUM) )
				{
				_msg[i].Add(new MessaggioErrore(msg, dett));
				_ultimiMsg[i]++;
				}
			}

		/// <summary>
		/// Restituisce l'ultimo messaggio
		/// </summary>
		/// <param name="typ">Tipo.Errori o </param>
		/// <returns></returns>
		public static MessaggioErrore LastMessage(Tipo typ = Tipo.Errori)
			{
			MessaggioErrore msg = null;
			int i = (int)typ;
			if( (i>=0) && (i<(int)Tipo.NUM) )
				{
				msg = _msg[i].Last();
				}
			if(msg == null)
				msg = new MessaggioErrore(String.Empty, String.Empty);
			return msg;
			}

		/// <summary>
		/// Cancella i messaggi del tipo indicato (oppure tutti)
		/// </summary>
		/// <param name="typ"></param>
		public static void Clear(Tipo typ = Tipo.NUM)
			{
			int i = (int)typ;
			if (i == (int)Tipo.NUM)
				{
				foreach (List<MessaggioErrore> lst in _msg)
					{
					lst.Clear();
					}
				Reset();
				}
			else if ((i >= 0) && (i < (int)Tipo.NUM))
				{
				_msg[i].Clear();
				Reset(typ);
				}
			}

		/// <summary>
		/// Azzera i contatori dei messaggi del tipo indicato (oppure tutti)
		/// </summary>
		/// <param name="typ"></param>
		public static void Reset(Tipo typ = Tipo.NUM)
			{
			int i = (int)typ;
			if (i == (int)Tipo.NUM)
				{
				for (int ii=0; ii<LISTE; ii++)
					{
					_ultimiMsg[ii] = 0;
					}
				}
			else if ((i >= 0) && (i < (int)Tipo.NUM))
				_ultimiMsg[i] = 0;
			}

		/// <summary>
		/// Enumeratore dei messaggi
		/// </summary>
		/// <param name="typ">Tipo: errore o messaggio</param>
		/// <returns>IEnumerable MessaggioErrore</returns>
		public static IEnumerable<MessaggioErrore> Messages(Tipo typ)
			{
			int i = (int)typ;
			if((i >= 0) && (i < (int)Tipo.NUM))
				{
				foreach (MessaggioErrore m in _msg[i])
					yield return m;
				}
			yield break;
			}

		/// <summary>
		/// Enumeratore degli ultimi messaggi
		/// </summary>
		/// <param name="typ">Tipo: errore o messaggio</param>
		/// <returns>IEnumerable MessaggioErrore</returns>
		public static IEnumerable<MessaggioErrore> LastMessages(Tipo typ)
			{
			MessaggioErrore m = null;
			int i = (int)typ;
			if((i >= 0) && (i < (int)Tipo.NUM))
				{
				for(int ii = 0; ii < NumLastMessages(typ); ii++)
					{
					m = (_msg[i])[_msg[i].Count-ii-1];
					yield return m;
					}
				}
			yield break;
			}

		/// <summary>
		/// Numero di messaggi
		/// </summary>
		/// <param name="typ">Tipo: errori o messaggi</param>
		/// <returns>int</returns>
		public static int NumMessages(Tipo typ)
			{
			int n = 0;
			int i = (int) typ;
			if ((i >= 0) && (i < (int)Tipo.NUM))
				{
				n = _msg[i].Count;
				}
			return n;
			}

		/// <summary>
		/// Numero degli ultimi messaggi (dall'ultimo Reset)
		/// </summary>
		/// <param name="typ">Tipo: errori o messaggi</param>
		/// <returns>int</returns>
		public static int NumLastMessages(Tipo typ)
			{
			int n = 0;
			int i = (int) typ;
			if ((i >= 0) && (i < (int)Tipo.NUM))
				{
				n = _ultimiMsg[i];
				}
			return n;
			}

		/// <summary>
		/// Restituisce true se ci sono messaggi o errori
		/// </summary>
		/// <param name="typ"></param>
		/// <returns>bool</returns>
		public static bool HasMessages(Tipo typ)
			{
			bool hasMsg = false;
			if (NumMessages(typ) > 0)
				hasMsg = true;
			return hasMsg;
			}

		/// <summary>
		/// Restiruisce true se ci sono messaggi o errori recenti
		/// </summary>
		/// <param name="typ"></param>
		/// <returns></returns>
		public static bool HasLastMessages(Tipo typ)
			{
			bool hasLastMsg = false;
			if (NumLastMessages(typ) > 0)
				hasLastMsg = true;
			return hasLastMsg;
			}

		/// <summary>
		/// Restituisce un'unica stringa con i messaggi
		/// </summary>
		/// <param name="typ"></param>
		/// <param name="lastMessages"></param>
		/// <returns></returns>
		public static string ToString(Messaggi.Tipo typ, bool lastMessages = false)
			{
			StringBuilder strb = new StringBuilder();
			List<string> lm = new List<string>();
			if(lastMessages)
				{
				foreach (MessaggioErrore msg in LastMessages(typ))
					lm.Add(msg.ToLine());
				}
			else
				{
				foreach (MessaggioErrore msg in Messages(typ))
					lm.Add(msg.ToLine());
				}
			lm = lm.Distinct().ToList();

			foreach (string str in lm)
				strb.Append(str);
			return strb.ToString();
			}

		/// <summary>
		/// Estrae i messaggi completi
		/// </summary>
		/// <returns>string</returns>
        public static string MessaggiCompleti()
            {
            StringBuilder strb = new StringBuilder();
			string s1, s2;
			s1 = Messaggi.ToString(Messaggi.Tipo.Errori);
			s2 = Messaggi.ToString(Messaggi.Tipo.Messaggi);
			if(s1.Length > 0)
				strb.Append("Errori"+ System.Environment.NewLine + s1+ Environment.NewLine);
			if(s2.Length > 0)
				strb.Append("Avvisi"+ System.Environment.NewLine + s2);
            return strb.ToString();
            }
        }
	}
