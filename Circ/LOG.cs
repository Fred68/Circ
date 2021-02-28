using System;
using System.IO;

namespace Fred68.Tools.Log
	{
	/// <summary>
	/// Classe statica per LOG su file
	/// Attiva solo in DEBUG
	/// 
	/// ENABLED:		Abilita il log
	/// LOGFILE:		Nome del file di log
	/// APPEND:			Aggiunge il log in coda al file
	/// MAX_LOG_LEVEL:	Livello massimo dei messaggi di log
	/// </summary>
	public static class LOG
		{

		public static bool ENABLED = false;
		public static readonly string LOGFILE = @"Log.txt";
		public static bool APPEND = false;
		public static int MAX_LOG_LEVEL = 1;

		/// <summary>
		/// Attiva o disattiva il log
		/// </summary>
		public static bool active { get; set; }

#if DEBUG
		static StreamWriter sw = null;
		static bool firstline = false;
#endif

		static LOG()
			{
#if DEBUG
			sw = new StreamWriter(LOGFILE,APPEND);
			sw.AutoFlush = true;
#endif
			active = false;
			}

		/// <summary>
		/// Scrive una linea di log sul file
		/// se il livello di log è inferiore a MAX_LOG_LEVEL 
		/// </summary>
		/// <param name="msg">Testo del messaggio</param>
		/// <param name="logLevel">Livello di log del messaggio</param>
		public static void Write(string msg,int logLevel = 0)
			{
#if DEBUG
			if(active)
				{
				if(!firstline)
					{
					sw.WriteLine(new string('-',50));
					firstline = true;
					}
				if(sw != null)
					if((logLevel <= MAX_LOG_LEVEL) && ENABLED)
						sw.WriteLine(DateTime.Now.ToString("dd/MM/yy HH:mm:ss") + "\t\t" + msg);
				}
#endif
			return;
			}

		/// <summary>
		/// Chiude le operazioni di log
		/// </summary>
		public static void Close()
			{
#if DEBUG
			if(sw != null)
				{
				sw.Close();
				sw = null;
				}
#endif
			}
		}
	}
