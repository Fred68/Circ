﻿using Fred68.Tools.Log;                     // Per Log e debug
using Fred68.Tools.Matematica;              // Matrici e varie
using Fred68.Tools.Messaggi;                // Messaggi di errore
using System;
using System.Collections.Generic;
using System.Diagnostics;                   // Per Process.Start
using System.Drawing;
using System.IO;                            // Per FileInfo
using System.Linq;
using System.Reflection;                    // Per Assembly()
using System.Text;
using System.Windows.Forms;

namespace Circ
	{
	public partial class MainForm:Form
		{
		CircuitoDoc doc;                            // Documento
		StatoAttivo stato;                          // Stato
		Vista vista;                                // Vista
		Panel drwPanel;                             // Pannello di disegno

		readonly System.Windows.Forms.Timer timerUpdate;    // Timer per aggiornare 
		Dictionary<string,TsBarRef> ts;                     // Dati delle toolbar (per costruire il menù)
		readonly EventHandler menuClickHandler;             // Handler unico dei click per i comandi del menù

		//Point dragIniRel;							// Punto inizio drag relativo (azzerato in Pan ad ogni mouse move=
		//Point dragIniFix;							// Punto inizio drag fisso

		Point pini, pfin, pold;                     // Punti per disegno dinamico linea si schermo
		bool firstLine;                             // Flag per il disegno in inverso


		#region COSTRUTTORE E INIZIALIZZAZIONE

		class TsBarRef
			{
			public ToolStrip tstrip;
			public ToolStripMenuItem mitem;
			public bool visible;
			public TsBarRef(ToolStrip t,ToolStripMenuItem it,bool vis)
				{
				tstrip = t;
				mitem = it;
				visible = vis;
				}
			}

		/// <summary>
		/// Costruttore
		/// </summary>
		public MainForm()
			{
			LOG.active = true;
#if(DEBUG)
			LOG.Write("MainForm()");
#endif

			InitializeComponent();
			timerUpdate = new Timer();
			timerUpdate.Tick += new EventHandler(UpdateViewsHandler);
			menuClickHandler = new EventHandler(MenuClickHandlerFunc);
			}

		/// <summary>
		/// On Load
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainForm_Load(object sender,EventArgs e)
			{
#if(DEBUG)
			LOG.Write("MainForm_Load()");
#endif

			doc = null;                         // Azzera il documento

			drwPanel = CreatePanel(this);       // Crea il pannello

			stato = new StatoAttivo();          // Crea lo stato
			vista = new Vista(drwPanel);        // Crea la vista

			ToolStripMenuItem m = new ToolStripMenuItem();      // Aggiunge il menù delle toolbar
			menuStrip.Items.Insert(1,m);
			m.Text = "Toolbars";

			ts = new Dictionary<string,TsBarRef>();         // Aggiunge i menuitem delle toolbar al menù

			List<Control> tmp = new List<Control>();
			foreach(Control ctrl in GetControls(this)) tmp.Add(ctrl);   // Temporanea, per avere l'ordine inverso
			foreach(Control ctrl in tmp.Reverse<Control>())
				{
				if(ctrl.GetType() == typeof(ToolStrip))
					{
					ts[ctrl.Text] = new TsBarRef((ToolStrip)ctrl,(ToolStripMenuItem)m.DropDownItems.Add(ctrl.Text,null,menuClickHandler),ctrl.Enabled);
					}
				}
			foreach(KeyValuePair<string,TsBarRef> kv in ts)
				{
				kv.Value.mitem.Checked = kv.Value.visible;
				kv.Value.tstrip.Visible = kv.Value.visible;
				kv.Value.tstrip.Enabled = true;
				}

			drwPanel.BringToFront();                            // Porta il pannello di disegno in primo piano (se no resta dietro dopo il docking)
			drwPanel.Dock = DockStyle.Fill;                     // Estende il pannello
			drwPanel.BackColor = Stile.BackgroundColor;        // Colore di sfondo

			pini = pfin = pold = new Point(0,0);
			firstLine = true;

			this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.drwPanel_Wheel);     // L'evento MouseWheel è catturato dal Form principale

#if(DEBUG)
			debugToolStripMenuItem.Enabled = debugToolStripMenuItem.Visible = true;
#else
			debugToolStripMenuItem.Enabled = debugToolStripMenuItem.Visible = false;
#endif

			UpdateMenus();                                      // Aggiorna i menù
			timerUpdate.Interval = Def.TIMER_REFRESH;       // Refresh del controllo se il documento è da salvare
			UpdateToolStrips();                                 // Aggiorna le barre

			// MessageBox.Show((Point2D.Zero + new Point2D(Math.PI/2, -1.5)).ToString());

			}

		/// <summary>
		/// Crea il pannello di disegno principale
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		private Panel CreatePanel(Form parent)
			{
			Panel p = new Panel();
			p.Parent = parent;

			// Aggiunge gli handler degli eventi del pannello
			p.Paint += new System.Windows.Forms.PaintEventHandler(this.drwPanel_Paint);
			p.MouseClick += new System.Windows.Forms.MouseEventHandler(this.drwPanel_MouseClick);
			p.MouseDown += new System.Windows.Forms.MouseEventHandler(this.drwPanel_MouseDown);
			p.MouseMove += new System.Windows.Forms.MouseEventHandler(this.drwPanel_MouseMove);
			p.MouseUp += new System.Windows.Forms.MouseEventHandler(this.drwPanel_MouseUp);
			p.Resize += new System.EventHandler(this.drwPanel_Resize);

			// Attenzione: l'evento MouseWheel non è catturato dal Pannello, quindi non usare:
			// p.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.drwPanel_Wheel);

			return p;
			}

		#endregion

		/// <summary>
		/// Prima della chiusura del form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainFormClosing(object sender,FormClosingEventArgs e)
			{
			if(MessageBox.Show(Messaggi.MSG.EXIT,"Chiusura programma",MessageBoxButtons.YesNo) != DialogResult.Yes)
				{
				e.Cancel = true;
				}
			else
				{
				ChiudiDoc();
				}
			if(doc != null)
				{
				e.Cancel = true;
				}
#if DEBUG
			LOG.Write("...MainFormClosing()");
			LOG.Close();
			Process.Start(LOG.LOGFILE);
#endif
			}

		/// <summary>
		/// Stringa di versione
		/// </summary>
		/// <returns></returns>
		public static string Version()
			{
			StringBuilder strb = new StringBuilder();
			FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
			strb.Append("Informazioni sul programma" + System.Environment.NewLine + System.Environment.NewLine);
			strb.Append(Application.ProductName + System.Environment.NewLine);
			strb.Append("Copyright " + Application.CompanyName + System.Environment.NewLine);
			strb.Append("Versione: " + Application.ProductVersion + System.Environment.NewLine);
			strb.Append("Build: " + Build() + System.Environment.NewLine);
			strb.Append("Eseguibile: " + fi.FullName + System.Environment.NewLine);
			return strb.ToString();
			}

		/// <summary>
		/// Stringa con la 'build'
		/// </summary>
		/// <returns></returns>
		public static string Build()
			{
			StringBuilder strb = new StringBuilder();
			DateTime dt = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
			strb.Append(dt.ToString("yyyyMMdd",System.Globalization.CultureInfo.InvariantCulture));
			strb.Append('.');
			strb.Append(dt.ToString("HHmm",System.Globalization.CultureInfo.InvariantCulture));
			return strb.ToString();
			}

		/// <summary>
		/// Salva il documento
		/// </summary>
		private void SalvaDoc()
			{
#if(DEBUG)
			LOG.Write("SalvaDoc()");
#endif

			doc.Dati.PassoGriglia = vista.GridStep;     // Copia il passo della griglia prima del salvataggio

			Messaggi.Clear();
			bool ok = false;
			Stream stream;
			sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
			sfd.FilterIndex = 1;
			sfd.RestoreDirectory = true;
			sfd.FileName = doc.Dati.Nome;
			if(sfd.ShowDialog() == DialogResult.OK)
				{
				if((stream = sfd.OpenFile()) != null)
					{
					ok = doc.Save(stream,sfd.FileName);
					stream.Close();
					}
				}
			if(!ok)
				{
				Messaggi.AddMessage(Messaggi.ERR.ERRORE_SAVE,$"Salvataggio del file: {doc.Dati.Nome} non eseguito !",Messaggi.Tipo.Errori);
				}
			if(Messaggi.hasError) MessageBox.Show(Messaggi.MessaggiCompleti());
			UpdateMenus();
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		/// <summary>
		/// Apre un documento
		/// Richiama ChiudiDoc() e NuovoDoc()
		/// Se apre in formato testo, il documento resta vuoto, poi viene chiuso
		/// </summary>
		/// <param name="asText"></param>
		private void ApriDoc(bool asText = false)
			{
#if(DEBUG)
			LOG.Write("ApriDoc()");
#endif
			Messaggi.Clear();
			bool ok = false;
			Stream stream;
			ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
			ofd.FilterIndex = 1;
			ofd.RestoreDirectory = true;
			if(ofd.ShowDialog() == DialogResult.OK)
				{
				ChiudiDoc();
				NuovoDoc();
				if((stream = ofd.OpenFile()) != null)
					{
					if(asText)
						{
						Process.Start(ofd.FileName);            // Apre in formato testo con editor di default
						ok = true;
						}
					else
						{
						ok = doc.Open(stream,ofd.FileName);     // Apre normalmente
						stream.Close();
						}
					}
				}
			if(!ok)
				{
				Messaggi.AddMessage(Messaggi.ERR.ERRORE_OPEN,$"Errore nell'apertura del file: {ofd.FileName}",Messaggi.Tipo.Errori);
				ChiudiDoc();
				}
			if(asText)                                          // Se in formato testo, chiude subito il documento
				{
				ChiudiDoc();
				ok = false;                                     // Inibisce le operazioni successive
				}
			if(Messaggi.hasError) MessageBox.Show(Messaggi.MessaggiCompleti());
			UpdateMenus();
			if(ok)
				{
				vista.GridStep = doc.Dati.PassoGriglia;         // Ricopia il passo della griglia dal documento alla vista
				UpdateToolStrips();                             // Aggiorna la statusBar con i dti della griglia
				vista.SetElements(doc.Dati.Elementi());
				vista.SetOutdatedDL();
				}
			if(doc != null)
				vista.FitZoom(doc.Dati);
			vista.RegenDL(true);
			}

		/// <summary>
		/// Chiude il documento
		/// Chiede e lo salva, se necessario
		/// Arresta il timer
		/// </summary>
		private void ChiudiDoc()
			{
#if(DEBUG)
			LOG.Write("ChiudiDoc()");
#endif
			if(doc != null)
				{
				if(doc.IsModified)
					{
					if(MessageBox.Show(String.Format(Messaggi.MSG.SALVARE_FILE,doc.Dati.Nome),"Salvataggio documento",MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
						SalvaDoc();
						}
					}
				doc.Chiudi();
				vista.ClearElements();
				timerUpdate.Stop();
				timerUpdate.Enabled = false;
				doc = null;
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				}
			UpdateMenus();

			}

		/// <summary>
		/// Crea un nuovo documento vuoto
		/// Avvia il timer
		/// Imposta la lista degli elementi
		/// </summary>
		private void NuovoDoc()
			{
#if(DEBUG)
			LOG.Write("NuovoDoc()");
#endif
			ChiudiDoc();
			doc = new CircuitoDoc();
			vista.SetElements(doc.Dati.Elementi());
			timerUpdate.Enabled = true;
			timerUpdate.Start();
			UpdateMenus();
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		/// <summary>
		/// Handler chiamato dal timer
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="e"></param>
		private void UpdateViewsHandler(object Sender,EventArgs e)
			{
			UpdateDocName();
			UpdateIDStats();
			}

		/// <summary>
		/// Aggiorna il titolo del Form principale con Nome del file e asterisco se ha modifiche non salvate
		/// </summary>
		private void UpdateDocName()
			{
			if(doc != null)
				{
				this.Text = doc.Dati.Nome;
				if(doc.IsModified)
					this.Text += '*';
				}
			}

		/// <summary>
		/// Aggiorna le label con i conteggi
		/// </summary>
		private void UpdateIDStats()
			{
			if(doc != null)
				{
				lbNodi.Text = $"nN:{doc.Dati.Nodi.Count.ToString()}";
				lbRami.Text = $"nR:{doc.Dati.Rami.Count.ToString()}";
				lbNodoFree.Text = $"frN:{doc.Dati.FreeIDNodo.ToString()}";
				lbRamoFree.Text = $"frR:{doc.Dati.FreeIDRamo.ToString()}";
				}
			}

		/// <summary>
		/// (Dis)Abilita i menù e le toolbar
		/// se c'é un doc aperto
		/// </summary>
		private void UpdateMenus()
			{
			bool enbl;
			Def.TipoCircuito tp = Def.TipoCircuito.None;
			if(doc != null)
				{
				enbl = true;
				tp = doc.Dati.TipoCirc;
				}
			else
				{
				this.Text = "---";
				enbl = false;
				}
			//this.saveButton.Enabled = enbl;
			this.toolStripFile.Enabled = enbl;
			this.saveToolStripMenuItem.Enabled = enbl;
			this.closeToolStripMenuItem.Enabled = enbl;
			this.toolStripEditor.Enabled = enbl;
			this.toolStripVista.Enabled = this.vistaToolStripMenuItem.Enabled = enbl;
			this.toolStripSelect.Enabled = enbl;
			this.toolStripModifica.Enabled = enbl;
			this.editToolStripMenuItem.Enabled = enbl;
			this.strumentiToolStripMenuItem.Enabled = enbl;
			this.componentiToolStripMenuItem.Enabled = enbl;
			switch(tp)
				{
				case Def.TipoCircuito.Elettrico:
					{
					elettricoToolStripSubMenu.Visible = true;
					idraulicoToolStripSubMenu.Visible = false;
					pneumaticoToolStripSubMenu.Visible = false;
					}
					break;
				case Def.TipoCircuito.Liquido:
					{
					elettricoToolStripSubMenu.Visible = false;
					idraulicoToolStripSubMenu.Visible = true;
					pneumaticoToolStripSubMenu.Visible = false;
					}
					break;
				case Def.TipoCircuito.Gas:
					{
					elettricoToolStripSubMenu.Visible = false;
					idraulicoToolStripSubMenu.Visible = false;
					pneumaticoToolStripSubMenu.Visible = true;
					}
					break;
				default:
					{
					elettricoToolStripSubMenu.Visible = false;
					idraulicoToolStripSubMenu.Visible = false;
					pneumaticoToolStripSubMenu.Visible = false;
					}
					break;

				}
			}

		/// <summary>
		/// Aggiorna toolbar e relative voci di menù
		/// </summary>
		private void UpdateToolStrips()
			{
#warning	Spostare stato e grid in un'altra funzione (e aggiornare le molte chiamate)
			foreach(KeyValuePair<string,TsBarRef> kv in ts)
				{
				kv.Value.mitem.Checked = kv.Value.visible;
				kv.Value.tstrip.Visible = kv.Value.visible;
				}
			labelStat.Text = stato.Stato.ToString().ToUpper();
			lbGrid.Text = vista.IsGridOn ? string.Format("Gr: {0}",vista.GridStep.ToString("#.##")) : "Gr: OFF";
			}

		/// <summary>
		/// Handler per click sulle voci del menù delle toolbar
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="e"></param>
		private void MenuClickHandlerFunc(object Sender,EventArgs e)
			{
			ts[Sender.ToString()].visible = !ts[Sender.ToString()].visible;
			UpdateToolStrips();
			}

		/// <summary>
		/// Enumeratore per tutti i controlli (Control) contenuti in un controllo
		/// </summary>
		/// <param name="ini"></param>
		/// <returns></returns>
		public static IEnumerable<Control> GetControls(Control ini)
			{
			Stack<Control> stack = new Stack<Control>();
			stack.Push(ini);

			while(stack.Any())
				{
				Control ctrl = stack.Pop();
				foreach(Control c in ctrl.Controls)
					{
					stack.Push(c);
					}
				yield return ctrl;
				}
			}

		/// <summary>
		/// Sposta gli elementi selezionati (con una Dialog)
		/// </summary>
		private void SpostaSelezionati()
			{
			if(doc != null)
				{
				InputForm.InputData[] dat = new InputForm.InputData[] {
													new InputForm.InputData("Delta X", Def.InputType.Double, "0"),
													new InputForm.InputData("Delta Y", Def.InputType.Double, "0")
													};
				InputForm inpf = new InputForm("Sposta elementi selezionati",ref dat);

				if(inpf.ShowDialog() == DialogResult.OK)
					{
					double x, y;
					if(double.TryParse(dat[0].Contenuto,out x) && double.TryParse(dat[1].Contenuto,out y))
						{
						doc.Dati.MuoviSelezionati(new Point2D(x,y));
						vista.SetOutdatedDL();
						vista.RegenDL(true);
						}

					}
				}
			}

		/// <summary>
		/// Elimina gli elementi selezionati
		/// </summary>
		private void EliminaSelezionati()
			{
			if(doc != null)
				{
				Tuple<uint,uint> t = doc.Dati.ContaNodiRamiSelezionati();
				if((t.Item1 > 0) || (t.Item2 > 0))
					{
					if(MessageBox.Show(String.Format(Messaggi.MSG.ELIMINARE_ELEMENTI, t.Item1, t.Item2, t.Item1==1?"o":"i", t.Item2==1?"o":"i", (t.Item1+t.Item2)==1?"o":"i"),"Conferma",MessageBoxButtons.YesNo) == DialogResult.Yes)
						{
						
						doc.Dati.EliminaSelezionati();
						vista.SetOutdatedDL();
						vista.RegenDL(true);
						}
					}
				}
			}

		/// <summary>
		/// Rinumera l'ID dell'elemento selezionato
		/// </summary>
		private void Rinumera()
			{
			if(doc != null)
				{
				List<Elemento> lsel = doc.Dati.GetSelezionati(true);
				if(lsel.Count == 1)
					{
					Elemento x = lsel[0];

					InputForm.InputData[] dat = new InputForm.InputData[] {
													new InputForm.InputData("Id vecchio", Def.InputType.Int, x.ID.ToString(), true),
													new InputForm.InputData("Id nuovo", Def.InputType.Int, "0")
													};
					InputForm inpf = new InputForm("Rinumera ID",ref dat);

					if(inpf.ShowDialog() == DialogResult.OK)
						{
						int n;
						if(int.TryParse(dat[1].Contenuto,out n))
							{
							if(x is Nodo)
								{
								doc.Dati.RinumeraIDnodo(x.ID,(uint)n);
								}
							else if(x is Ramo)
								{
								doc.Dati.RinumeraIDramo(x.ID,(uint)n);
								}
							vista.SetOutdatedDL();
							vista.RegenDL(true);
							}
						}
					}
				else
					{
					if(lsel.Count == 0)
						{
						MessageBox.Show(Messaggi.MSG.SELEZIONARE_UN_ELEMENTO);
						}
					else
						{
						MessageBox.Show(Messaggi.MSG.SELEZIONARE_UN_SOLO_ELEMENTO);
						}
					}
				}
			}

		/// <summary>
		/// Inserisce le coordinate di un nodo
		/// </summary>
		private void CoordinateNodo()
			{
			if(doc != null)
				{
				List<Elemento> lsel = doc.Dati.GetSelezionati(true);
				if(lsel.Count == 1)
					{
					Elemento e = lsel[0];
					if(e is Nodo)
						{
						InputForm.InputData[] dat = new InputForm.InputData[] {
														new InputForm.InputData("X", Def.InputType.Double, ((Nodo)e).P.X.ToString()),
														new InputForm.InputData("Y", Def.InputType.Double, ((Nodo)e).P.Y.ToString())
														};
						InputForm inpf = new InputForm("Coordinate nodo",ref dat);

						if(inpf.ShowDialog() == DialogResult.OK)
							{
							double x, y;
							if(double.TryParse(dat[0].Contenuto,out x) && double.TryParse(dat[1].Contenuto,out y))
								{
								((Nodo)e).P.X = x;
								((Nodo)e).P.Y = y;
								doc.IsModified = true;
								vista.SetOutdatedDL();
								vista.RegenDL(true);
								}
							else
								{
								MessageBox.Show("Errore nelle coordinate");
								}
							}
						}
					else
						{
						MessageBox.Show(Messaggi.MSG.SELEZIONARE_UN_NODO);
						}
					}
				else
					{
					MessageBox.Show(Messaggi.MSG.SELEZIONARE_UN_SOLO_NODO);
					}
				}
			}

		/// <summary>
		/// Compatta gli ID di nodi e rami
		/// </summary>
		private void CompattaID()
			{
			Messaggi.Clear();
			bool ok = true;
			if(doc != null)
				{
				ok = doc.Dati.CompattaID();
				if(!ok)
					MessageBox.Show("Errore durante la rinumerazione");
				}
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		/// <summary>
		/// Seleziona o deseleziona tutti gli elementi
		/// </summary>
		/// <param name="select">seleziona o deseleziona</param>
		/// <param name="st">Stato attuale usato come filtro: nodi o rami; se Edit: tutti</param>
		/// <param name="doc">documento</param>
		private void SelectAll(bool select,Def.Stat st,CircuitoDoc doc)
			{
			if(doc != null)
				{
				Def.Stat previousSt = doc.Dati.ViewFilter;          // Memorizza il filtro preesistente
				doc.Dati.ViewFilter = st;                           // Filtro = stato attuale: Nodi o Rami					
				if(st == Def.Stat.Edit)
					doc.Dati.ViewFilter = Def.Stat.Tutti;           // Stato Edit: filtro Tutti
				foreach(Elemento el in doc.Dati.Elementi())
					{
					el.Selected = select;
					}
				doc.Dati.ViewFilter = previousSt;                   // Lo ripristina
				}
			}

		/// <summary>
		/// Paint
		/// </summary>
		private void drwPanel_Paint(object sender,PaintEventArgs e)
			{
			if(doc != null)
				{
#if(DEBUG)
				LOG.Write("drwPanel_Paint()",2);
#endif
				vista.RegenDL(true);
				}
			}

		/// <summary>
		/// Resize
		/// </summary>
		private void drwPanel_Resize(object sender,EventArgs e)
			{
#if(DEBUG)
			LOG.Write("drwPanel_Resize()");
#endif
			vista.Resize();
			vista.RegenDL(true);
			}

		#region MOVIMENTI DEL MOUSE

		/// <summary>
		/// Mouse move
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">e.X, e.Y sono le coordinate del mouse</param>
		private void drwPanel_MouseMove(object sender,MouseEventArgs e)
			{
			switch(stato.Stato)
				{
				case Def.Stat.Vista:
						{
						if(stato.Dragging)
							{
#if(DEBUG)
							LOG.Write("drwPanel_MouseMove():case Def.Stat.Vista + dragging",2);
#endif

							DrawScreenReverseLine(e.Location);      // Disegna la linea (reverse, cancellabile)

							Point delta = new Point(e.Location.X - stato.dragIniRel.X,e.Location.Y - stato.dragIniRel.Y);

							Point2D pan = vista.ScalaVettore(delta);

							vista.SetCursor(false);                 // Disabilita evidenziazione degli elementi
							vista.Pan(pan);
							vista.RegenDL(true);
							stato.dragIniRel = e.Location;
							vista.SetCursor(false);                 // Disabilita evidenziazione
							}
						}
					break;
				case Def.Stat.Edit:
						{
						if(stato.Dragging)
							{
							DrawScreenReverseLine(e.Location);      // Disegna la linea (reverse, cancellabile)
							vista.RegenDL(true);
							}
						else
							{
							vista.SetCursor(true,e.Location.X,e.Location.Y);    // Abilita evidenziazione degli elementi vicini al cursore
							vista.Redraw(false);
							}
						}
					break;
				case Def.Stat.Rami:
						{
						vista.SetCursor(true,e.Location.X,e.Location.Y,Def.Shape.Nodo); // Abilita evidenziazione dei nodi (anche in dragging)
						if(stato.Dragging)
							{
							DrawScreenReverseLine(e.Location);      // Disegna la linea (reverse, cancellabile)
							vista.RegenDL(true);
							}
						else
							{
							vista.Redraw(false);
							}
						}
					break;
				case Def.Stat.Nodi:
						{
						vista.SetCursor(false);         // Disabilita evidenziazione 
						vista.Redraw(false);
						}
					break;
				}

			Point2D p;                          // Aggiorna le ccordinate del cursore. Se dragging: mostra il vettore di spostamento
			if(stato.Dragging)                  // Trascinamento: scala il vettore
				{
				p = vista.ScalaVettore(new Point(e.X - stato.dragIniFix.X,e.Y - stato.dragIniFix.Y));
				}
			else                                // Movimento normale: scala il punto del cursore
				{
				p = vista.Scala(new Point(e.X,e.Y));
				}
			xPos.Text = $"{ (stato.Dragging ? "D" : "") }{String.Format("X:{0:0.###}",p.X)}";
			yPos.Text = $"{ (stato.Dragging ? "D" : "") }{String.Format("Y:{0:0.###}",p.Y)}";
			}

		/// <summary>
		/// Mouse click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e">Contiene i dati dell'evento</param>
		private void drwPanel_MouseClick(object sender,MouseEventArgs e)
			{
			switch(stato.Stato)
				{
				case Def.Stat.Vista:
						{
						}
					break;

				case Def.Stat.Nodi:
						{
#if(DEBUG)
						LOG.Write("drwPanel_MouseClick():case Def.Stat.Nodi");
#endif
						if(e.Button == MouseButtons.Left)
							{
							if(doc != null)
								doc.AddNodo(vista.Scala(new Point(e.X,e.Y)));
							vista.SetOutdatedDL();
							vista.RegenDL(true);
							}
						}
					break;

				case Def.Stat.Rami:
						{
#if(DEBUG)
						LOG.Write("drwPanel_MouseClick():case Def.Stat.Rami");
#endif
						if(e.Button == MouseButtons.Left)
							{
							vista.SetCursor(true,e.Location.X,e.Location.Y,Def.Shape.Nodo); // Abilita evidenziazione dei nodi vicini al cursore
							vista.Redraw(false);
							if(doc != null)
								doc.SelectLastElement(vista);
							}
						}
					break;
				case Def.Stat.Edit:
						{
#if(DEBUG)
						LOG.Write("drwPanel_MouseClick():case Def.Stat.Edit");
#endif
						if(e.Button == MouseButtons.Left)
							{
							vista.SetCursor(true,e.Location.X,e.Location.Y,Def.Shape.Tutti);    // Abilita evidenziazione degli elementi vicini al cursore
							vista.Redraw(false);
							if(doc != null)
								doc.SelectLastElement(vista);
							}
						}
					break;
				}
			}

		/// <summary>
		/// Mouse down (inizio trascinamento)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void drwPanel_MouseDown(object sender,MouseEventArgs e)
			{
			switch(stato.Stato)
				{
				case Def.Stat.Vista:
						{
#if(DEBUG)
						LOG.Write("drwPanel_MouseDown():case Def.Stat.View");
#endif
						if(e.Button == MouseButtons.Left)
							{
							stato.dragIniRel = stato.dragIniFix = e.Location;
							stato.Dragging = true;
							firstLine = true;
							if(doc != null)
								doc.Dati.ViewFilter = Def.Stat.Nodi;
							UpdateToolStrips();
							}
						}
					break;
				case Def.Stat.Nodi:
						{

						}
					break;
				case Def.Stat.Rami:
						{
#if(DEBUG)
						LOG.Write("drwPanel_MouseDown():case Def.Stat.Rami");
#endif
						if(e.Button == MouseButtons.Left)
							{
							Elemento el = vista.GetLastHighLighted();
							if(el != null)
								{
								stato.dragIniRel = stato.dragIniFix = el.Center;
								stato.dragFromElement = el;
								}
							else
								{
								stato.dragIniRel = stato.dragIniFix = e.Location;
								stato.dragFromElement = null;
								}
							stato.Dragging = true;
							firstLine = true;
							UpdateToolStrips();
							}
						}
					break;
				case Def.Stat.Edit:
						{
#if(DEBUG)
						LOG.Write("drwPanel_MouseDown():case Def.Stat.Edit");
#endif
						if(e.Button == MouseButtons.Left)
							{
							stato.dragIniRel = stato.dragIniFix = e.Location;
							stato.Dragging = true;
							firstLine = true;
							UpdateToolStrips();
							}
						}
					break;
				}
			}

		/// <summary>
		/// Mouse up (fine trascinamento)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void drwPanel_MouseUp(object sender,MouseEventArgs e)
			{
			if(stato.Dragging)                  // Se sta eseguendo un dragging...
				if(!(stato.Stato == Def.Stat.Vista))    // ...ma non in Vista
					{                               // Se lo spostamento è piccolo...
					if((Math.Abs(e.Location.X - stato.dragIniFix.X) < Def.DRG_MIN) && (Math.Abs(e.Location.Y - stato.dragIniFix.Y) < Def.DRG_MIN))
						{
						stato.Dragging = false;     // ...annulla il dragging per distinguere un click da un drag.
						vista.RegenDL(false);
						}
					}

			switch(stato.Stato)
				{
				case Def.Stat.Vista:
						{
						if(stato.Dragging)
							{
#if(DEBUG)
							LOG.Write("drwPanel_MouseUp():case Def.Stat.Pan");
#endif
							if(e.Button == MouseButtons.Left)
								{
								stato.Stato = Def.Stat.Vista;
								Point2D p;
								p = vista.ScalaVettore(new Point(e.X - stato.dragIniFix.X,e.Y - stato.dragIniFix.Y));

								stato.dragIniRel.X = stato.dragIniRel.Y = stato.dragIniFix.X = stato.dragIniFix.Y = 0;
								if(doc != null)
									doc.Dati.ViewFilter = Def.Stat.Nodi | Def.Stat.Rami;
								UpdateToolStrips();
								vista.RecalcSzWlorld();
								vista.SetOutdatedDL();
								vista.RegenDL(true);
								}
							}
						}
					break;
				case Def.Stat.Nodi:
						{

						}
					break;
				case Def.Stat.Rami:
						{
						if(stato.Dragging)
							{
#if(DEBUG)
							LOG.Write("drwPanel_MouseUp():case Def.Stat.Rami");
#endif
							if(e.Button == MouseButtons.Left)
								{
								Elemento hi = vista.GetLastHighLighted();
								stato.Stato = Def.Stat.Rami;
								if(doc != null)
									{
									uint id1, id2;
									if(stato.dragFromElement != null)           // Primo nodo
										{
										id1 = stato.dragFromElement.ID;
										}
									else
										{
										Nodo n = (Nodo)doc.AddNodo(vista.Scala(stato.dragIniFix));
										if(n != null)
											{
											id1 = n.ID;
											}
										else
											{
											id1 = Elemento.UNASSIGNED;
											}
										}

									if(hi != null)                              // Secondo nodo
										{
										id2 = hi.ID;
										}
									else
										{
										Nodo n = (Nodo)doc.AddNodo(vista.Scala(new Point(e.X,e.Y)));
										if(n != null)
											{
											id2 = n.ID;
											}
										else
											{
											id2 = Elemento.UNASSIGNED;
											}
										}

									doc.AddRamo(id1,id2);

									}
								stato.dragIniRel.X = stato.dragIniRel.Y = stato.dragIniFix.X = stato.dragIniFix.Y = 0;
								UpdateToolStrips();
								}
							vista.SetOutdatedDL();
							vista.RegenDL(true);
							}
						}
					break;
				case Def.Stat.Edit:
						{
						if(stato.Dragging)
							{
#if(DEBUG)
							LOG.Write("drwPanel_MouseUp():case Def.Stat.Move");
#endif
							if(e.Button == MouseButtons.Left)
								{
								stato.Stato = Def.Stat.Edit;
								Point2D p;
								p = vista.ScalaVettore(new Point(e.X - stato.dragIniFix.X,e.Y - stato.dragIniFix.Y));
								stato.dragIniRel.X = stato.dragIniRel.Y = stato.dragIniFix.X = stato.dragIniFix.Y = 0;
								if(doc != null)
									{
									doc.Dati.MuoviSelezionati(p);
									}
								UpdateToolStrips();
								}
							vista.SetOutdatedDL();
							vista.RegenDL(true);
							}
						}
					break;
				}
			}

		/// <summary>
		/// Moude wheel
		/// Esegue Zoom +/- in modalità Vista
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void drwPanel_Wheel(object sender,MouseEventArgs e)
			{
			if(doc != null)
				{
#if(DEBUG)
				LOG.Write("drwPanel_Wheel():case Def.Stat.Vista");
#endif
				if(!stato.Dragging)
					{
					if((e.Delta > 0) || (e.Delta < 0))
						{
						vista.Zoom(Math.Pow(Def.ZOOM_STEP,e.Delta / SystemInformation.MouseWheelScrollDelta));
						vista.RegenDL(true);
						}
					}
				//switch(stato.Stato)
				//	{
				//	#warning	Permettere lo zoom in ogni stato, purche' non dragging
				//	//case Def.Stat.Vista:
				//	default:
				//		{
				//		#if(DEBUG)
				//		LOG.Write("drwPanel_Wheel():case Def.Stat.Vista");
				//		#endif
				//		if((e.Delta > 0)||(e.Delta < 0))
				//			{
				//			vista.Zoom(Math.Pow(Def.ZOOM_STEP,e.Delta/SystemInformation.MouseWheelScrollDelta));
				//			vista.RegenDL(true);
				//			}
				//		}
				//		break;
				//	}
				}
			}

		#endregion

		/// <summary>
		/// Divide i rami selezionati
		/// </summary>
		private void DivideSelezionati()
			{
			if(doc != null)
				{
				doc.DivideSelezionati();
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				vista.Redraw(true);
				}

			}

		/// <summary>
		/// Disegna in inverso una linea fino al mouse
		/// Non usare: crea caos se si esce dalla finestra
		/// </summary>
		/// <param name="loc"></param>
		private void DrawScreenReverseLine(Point loc)
			{
			pfin = new Point(loc.X + drwPanel.Location.X,loc.Y + drwPanel.Location.Y);
			if(firstLine)
				{
				pini = new Point(stato.dragIniFix.X + drwPanel.Location.X,stato.dragIniFix.Y + drwPanel.Location.Y);
				ControlPaint.DrawReversibleLine(PointToScreen(pini),PointToScreen(pfin),Color.Blue);
				}
			else
				{
				ControlPaint.DrawReversibleLine(PointToScreen(pini),PointToScreen(pold),Color.White);
				ControlPaint.DrawReversibleLine(PointToScreen(pini),PointToScreen(pfin),Color.White);
				}
			pold = pfin;
			firstLine = false;
			}

		/// <summary>
		/// Allinea i nodi alla griglia
		/// </summary>
		private void AllineaAllaGriglia()
			{
			if(doc != null)
				{
				doc.AllineaAllaGriglia(vista);
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				vista.Redraw(true);
				Invalidate();
				}
			}

		/// <summary>
		/// Inverte i rami selezionati
		/// </summary>
		private void InverteRamiSelezionati()
			{
			if(doc != null)
				{
				doc.InverteRamiSelezionati();
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				vista.Redraw(true);
				}
			}

		/// <summary>
		/// Collassa i nodi selezionati sul primo della lista
		/// </summary>
		private void CollassaSelezionati()
			{
			if(doc != null)
				{
				doc.CollassaSelezionati();
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				vista.Redraw(true);
				Invalidate();
				}
			}
		
		/// <summary>
		/// Controlla se il circuito è connesso
		/// </summary>
		private void ControllaNodiIsolati()
			{
			if(doc != null)
				{
				string x = (doc.Dati.VerificaNodiIsolati(true) == true) ? "" : "non ";
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				MessageBox.Show($"Il circuito {x}è connesso");
				}
			}

		private bool ConfermaCambioTipo()
			{
			MessageBox.Show(Messaggi.MSG.CAMBIO_TIPO);
			return true;
			}

		#region TEST

		private void rinumeraNodoToolStripMenuItem_Click(object sender,EventArgs e)
			{
			InputForm.InputData[] dat = new InputForm.InputData[] {
																new InputForm.InputData("Id vecchio", Def.InputType.Int, "0"),
																new InputForm.InputData("Id nuovo", Def.InputType.Int, "0")
															};
			InputForm inpf = new InputForm("Rinumera nodo",ref dat);
			if(inpf.ShowDialog() == DialogResult.OK)
				{
				if(doc != null)
					{
					int v, n;
					if(
						int.TryParse(dat[0].Contenuto,out v)
						&&
						int.TryParse(dat[1].Contenuto,out n))
						{
						doc.Dati.RinumeraIDnodo((uint)v,(uint)n);
						vista.SetOutdatedDL();
						vista.RegenDL(true);
						}

					}
				}
			}



		private void rinumeraRamoToolStripMenuItem_Click(object sender,EventArgs e)
			{
			InputForm.InputData[] dat = new InputForm.InputData[] {
																new InputForm.InputData("Id vecchio", Def.InputType.Int, "0"),
																new InputForm.InputData("Id nuovo", Def.InputType.Int, "0")
															};
			InputForm inpf = new InputForm("Rinumera ramo",ref dat);
			if(inpf.ShowDialog() == DialogResult.OK)
				{
				if(doc != null)
					{
					int v, n;
					if(
						int.TryParse(dat[0].Contenuto,out v)
						&&
						int.TryParse(dat[1].Contenuto,out n))
						{
						doc.Dati.RinumeraIDramo((uint)v,(uint)n);
						vista.SetOutdatedDL();
						vista.RegenDL(true);
						}

					}
				}
			}

		private void testDialogToolStripMenuItem_Click(object sender,EventArgs e)
			{
			InputForm.InputData[] dat = new InputForm.InputData[] {
																new InputForm.InputData("Testo", Def.InputType.String, "Uno"),
																new InputForm.InputData("Intero", Def.InputType.Int, "20"),
																new InputForm.InputData("Reale", Def.InputType.Double, "234.789"),
															};
			InputForm inpf = new InputForm("Titolo",ref dat);
			if(inpf.ShowDialog() == DialogResult.OK)
				{
				StringBuilder strb = new StringBuilder();
				foreach(InputForm.InputData ind in dat)
					strb.Append($"{ind.Messaggio}[{ind.Tipo.ToString()}]: {ind.Contenuto}\n");
				MessageBox.Show(strb.ToString());
				}
			}

		private void controllaNodiIsolatiToolStripMenuItem_Click(object sender,EventArgs e)
			{

			}


		private void gridToolStripMenuItem_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				vista.Redraw(true);
				}

			}
		#endregion




		#region HANDLER DI MENU E PULSANTI

		private void ExitToolStripMenuItem_Click(object sender,EventArgs e)
			{
			Close();
			}

		private void AboutToolStripMenuItem1_Click(object sender,EventArgs e)
			{
			MessageBox.Show(Version());
			}

		private void SaveToolStripMenuItem_Click(object sender,EventArgs e)
			{
			SalvaDoc();
			}

		private void SaveStripButton_Click(object sender,EventArgs e)
			{
			SalvaDoc();
			}

		private void NewToolStripMenuItem_Click(object sender,EventArgs e)
			{
			NuovoDoc();
			}

		private void OpenToolStripMenuItem_Click(object sender,EventArgs e)
			{
			ApriDoc();
			}

		private void CloseToolStripMenuItem_Click(object sender,EventArgs e)
			{
			ChiudiDoc();
			}

		private void NodoMode_TB_Click(object sender,EventArgs e)
			{
			if(doc == null) return;
			stato.Stato = Def.Stat.Nodi;
			UpdateToolStrips();
			}

		private void RamoMode_TB_Click(object sender,EventArgs e)
			{
			if(doc == null) return;
			if(stato.Stato != Def.Stat.Rami)        // Cambia modo
				{
				stato.Stato = Def.Stat.Rami;
				}
			else                                // Se è già in modalità Rami, aggiunge il ramo tra i due nodi selezionati
				{
				bool ramok = false;
				if(doc.selezionati.Count == 2)
					{
					if(((doc.selezionati[0]).GetType() == typeof(Nodo)) && ((doc.selezionati[1]).GetType() == typeof(Nodo)))
						{
						if(doc.AddRamo(doc.selezionati[0].ID,doc.selezionati[1].ID) != null)
							{
							ramok = true;
							}
						}
					doc.UnselectAll();          // Dopo aver aggiunto il ramo, deseleziona tutto
					vista.RegenDL(true);
					}
				doc.UnselectAll();              // Deseleziona tutto, in ogni caso, per evitare problemi ala selezione successiva.
				if(!ramok)
					{
#warning Spostare tutti i messaggi da popup/dialog a toolbar
					MessageBox.Show(Messaggi.MSG.SELEZIONARE_DUE_NODI);
					}
				}

			vista.SetOutdatedDL();
			vista.RegenDL(true);

			UpdateToolStrips();
			}

		private void ViewMode_TB_Click(object sender,EventArgs e)
			{
			if(doc == null) return;
			stato.Stato = Def.Stat.Vista;
			UpdateToolStrips();
			}

		private void EditMode_TB_Click(object sender,EventArgs e)
			{
			if(doc == null) return;
			stato.Stato = Def.Stat.Edit;
			UpdateToolStrips();
			}

		private void Ridisegna_M_Click(object sender,EventArgs e)
			{
			vista.RegenDL(true);
			}

		private void Ridisegna_TB_Click(object sender,EventArgs e)
			{
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		private void openAsTxtToolStripMenuItem_Click(object sender,EventArgs e)
			{
			ApriDoc(true);
			}

		private void ZoomInToolStripButton_Click(object sender,EventArgs e)
			{
			vista.Zoom(Def.ZOOM_STEP);
			vista.RegenDL(true);
			}
		private void ZoomOutToolStripButton_Click(object sender,EventArgs e)
			{
			vista.Zoom(1 / Def.ZOOM_STEP);
			vista.RegenDL(true);
			}
		private void ZoomFitToolStripButton_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				vista.FitZoom(doc.Dati);
				vista.RegenDL(true);
				}
			}

		private void SelectAlltoolStripButton_Click(object sender,EventArgs e)
			{
			SelectAll(true,stato.Stato,doc);        // Seleziona in base allo stato attuale
			vista.SetOutdatedDL();
			vista.Redraw();
			}

		private void SelectNonetoolStripButton_Click(object sender,EventArgs e)
			{
			SelectAll(false,Def.Stat.Tutti,doc);
			vista.SetOutdatedDL();
			vista.Redraw();
			}

		private void RinumeraToolStripButton_Click(object sender,EventArgs e)
			{
			Rinumera();
			}

		private void CompattaIDToolStripButton_Click(object sender,EventArgs e)
			{
			CompattaID();
			}

		private void EliminaToolStripButton_Click(object sender,EventArgs e)
			{
			EliminaSelezionati();
			}

		private void eliminaToolStripMenuItem_Click(object sender,EventArgs e)
			{
			EliminaSelezionati();
			}

		private void spostaToolStripMenuItem_Click(object sender,EventArgs e)
			{
			SpostaSelezionati();
			}

		private void inserisciCoordinateToolStripMenuItem_Click(object sender,EventArgs e)
			{
			CoordinateNodo();
			}

		private void controllaNodiIsolatiToolStripMenuItem_Click_1(object sender,EventArgs e)
			{
			ControllaNodiIsolati();
			}

		private void creaMatriceDiIncidenzaToolStripMenuItem_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				Messaggi.Clear();
				Matrix A = doc.Dati.CreaMatriceDiIncidenza(true);
				if(Messaggi.hasError)
					MessageBox.Show(Messaggi.MessaggiCompleti());
				else
					MessageBox.Show(A.ToString());
				}
			}

		private void DivideToolStripButton_Click(object sender,EventArgs e)
			{
			DivideSelezionati();
			}

		private void dividiRamoToolStripMenuItem_Click(object sender,EventArgs e)
			{
			DivideSelezionati();
			}

		private void grigliaToggle_Click(object sender,EventArgs e)
			{
			vista.ToggleGriglia();
			grigliaToolStripMenuItem.Checked = vista.IsGridOn;
			UpdateToolStrips();
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		private void GrigliaPiùFitta_Click(object sender,EventArgs e)
			{
			vista.GrigliaStepMultiply(1 / Def.ZOOM_STEP);
			UpdateToolStrips();
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		private void GrigliaMenoFitta_Click(object sender,EventArgs e)
			{
			vista.GrigliaStepMultiply(Def.ZOOM_STEP);
			UpdateToolStrips();
			vista.SetOutdatedDL();
			vista.RegenDL(true);
			}

		private void inserisciPasso_Click(object sender,EventArgs e)
			{
			InputForm.InputData[] dat = new InputForm.InputData[] {
										new InputForm.InputData("Passo", Def.InputType.Double, vista.GridStep.ToString()),
										};
			InputForm inpf = new InputForm("Passo griglia",ref dat);

			if(inpf.ShowDialog() == DialogResult.OK)
				{
				double p;
				if(double.TryParse(dat[0].Contenuto,out p))
					{
					if(p > Def.EPSILON)
						{
						vista.GridStep = p;
						}
					UpdateToolStrips();
					vista.SetOutdatedDL();
					vista.RegenDL(true);
					}
				}
			}

		private void allineaAllaGriglia_Click(object sender,EventArgs e)
			{
			AllineaAllaGriglia();
			}

		private void inverteRamo_Click(object sender,EventArgs e)
			{
			InverteRamiSelezionati();
			}

		private void inverteRamoToolStrip_Click(object sender,EventArgs e)
			{
			InverteRamiSelezionati();
			}

		private void collassaNodi_Click(object sender,EventArgs e)
			{
			CollassaSelezionati();
			}

		private void inverteSel_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				doc.InverteSelezione();
				vista.SetOutdatedDL();
				vista.RegenDL(true);
				vista.Redraw(true);
				}
			}

		private void toolCollassa_Click(object sender,EventArgs e)
			{
			CollassaSelezionati();
			}

		private void compattaIDToolStrip_Click(object sender,EventArgs e)
			{
			CompattaID();

			}

		private void controllaIsolati_toolStrip_Click(object sender,EventArgs e)
			{
			ControllaNodiIsolati();
			}

		private void elettricoToolStripMenuItem_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				if(ConfermaCambioTipo())
					{
					doc.Dati.TipoCirc = Def.TipoCircuito.Elettrico;
					elettricoToolStripMenuItem.Checked = true;
					idraulicoToolStripMenuItem.Checked = pneumaticoToolStripMenuItem.Checked = false;
					elettricoToolStripSubMenu.Visible = true;
					idraulicoToolStripSubMenu.Visible = false;
					pneumaticoToolStripSubMenu.Visible = false;
					}
				}
			}

		private void idraulicoToolStripMenuItem_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				if(ConfermaCambioTipo())
					{
					doc.Dati.TipoCirc = Def.TipoCircuito.Elettrico;
					idraulicoToolStripMenuItem.Checked = true;
					elettricoToolStripMenuItem.Checked = pneumaticoToolStripMenuItem.Checked = false;
					elettricoToolStripSubMenu.Visible = false;
					idraulicoToolStripSubMenu.Visible = true;
					pneumaticoToolStripSubMenu.Visible = false;
					}
				}
			}

		private void pneumaticoToolStripMenuItem_Click(object sender,EventArgs e)
			{
			if(doc != null)
				{
				if(ConfermaCambioTipo())
					{
					doc.Dati.TipoCirc = Def.TipoCircuito.Elettrico;
					pneumaticoToolStripMenuItem.Checked = true;
					idraulicoToolStripMenuItem.Checked = elettricoToolStripMenuItem.Checked = false;
					elettricoToolStripSubMenu.Visible = false;
					idraulicoToolStripSubMenu.Visible = false;
					pneumaticoToolStripSubMenu.Visible = true;

					}
				}
			}

		private void inverteAsseXToolStripMenuItem_Click(object sender,EventArgs e)
			{
			vista.SwapAxisX();
			vista.RegenDL(true);
			}

		private void inverteAsseYToolStripMenuItem_Click(object sender,EventArgs e)
			{
			vista.SwapAxisY();
			vista.RegenDL(true);
			}

		#endregion


		}
	}
