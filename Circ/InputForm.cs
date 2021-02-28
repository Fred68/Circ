using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Circ
	{
	public partial class InputForm:Form
		{
		int numRighe;
		int maxLenMsg;
		TextBox[] tb;
		InputData[] data;

		public class InputData
			{
			public string Messaggio;
			public Def.InputType Tipo;
			public string Contenuto;
			public bool IsReadonly;
			public bool Ok;
			public InputData()
				{
				Messaggio = Contenuto = string.Empty;
				Tipo = Def.InputType.String;
				Ok = IsReadonly = false;
				}
			public InputData(string messaggio,Def.InputType tipo,string contenuto = "",bool isReadonly = false,bool ok = false)
				{
				Messaggio = messaggio;
				Tipo = tipo;
				Contenuto = contenuto;
				IsReadonly = isReadonly;
				Ok = ok;
				}
			}

		public InputForm(string titolo,ref InputData[] dataArray)
			{
			InitializeComponent();
			data = dataArray;
			this.Text = titolo;
			this.MinimizeBox = this.MaximizeBox = false;
			this.AutoSizeMode = AutoSizeMode.GrowOnly;
			this.AutoSize = true;
			this.Size = new Size(100,100);
			this.Padding = new Padding(10);

			Font fnt = new Font(Def.FONT_D_NAME,Def.FONT_D_SIZE);
			numRighe = data.Length;

			maxLenMsg = 0;
			foreach(InputData i in data)
				{
				if(i.Messaggio.Length > maxLenMsg) maxLenMsg = i.Messaggio.Length;
				}

#warning Usare TextMetrics una tantum per determinare le dimensioni di un testo (es.: "8888")

			tb = new TextBox[numRighe];
			for(int i = 0;i < numRighe;i++)
				{
				Label lb = new Label();
				lb.AutoSize = true;
				lb.Height = Def.FONT_D_SIZE * 2;
				lb.Font = fnt;
				lb.Location = new Point(10,Def.FONT_D_SIZE * 2 * (i + 1));
				lb.Text = data[i].Messaggio;

				tb[i] = new TextBox();
				tb[i].Height = Def.FONT_D_SIZE * 2;
				tb[i].Width = Def.FONT_D_SIZE * 20;
				tb[i].Font = fnt;
				tb[i].Location = new Point(10 + maxLenMsg * fnt.Height,Def.FONT_D_SIZE * 2 * (i + 1));
				tb[i].Text = data[i].Contenuto;
				tb[i].ReadOnly = data[i].IsReadonly;

				this.Controls.Add(lb);
				this.Controls.Add(tb[i]);
				}

			Button bok = new Button();
			Button bcn = new Button();
			bok.Height = bcn.Height = Def.FONT_D_SIZE * 3;
			bok.Width = bcn.Width = Def.FONT_D_SIZE * 7;
			bok.Text = "OK";
			bcn.Text = "Cancel";
			bok.Location = new Point(10,Def.FONT_D_SIZE * 2 * (numRighe + 2));
			bcn.Location = new Point(10 + Def.FONT_D_SIZE * 8,Def.FONT_D_SIZE * 2 * (numRighe + 2));
			this.Controls.Add(bok);
			this.Controls.Add(bcn);

			bok.Click += new System.EventHandler(bok_Click);
			bcn.Click += new System.EventHandler(bcn_Click);
			}

		private void bok_Click(object sender,EventArgs e)
			{
			for(int i = 0;i < numRighe;i++)
				{
				string txt = tb[i].Text;
				switch(data[i].Tipo)
					{
					case Def.InputType.String:
							{
							data[i].Ok = true;              // string: ok
							}
						break;
					case Def.InputType.Int:
							{
							int x;
							if(int.TryParse(txt,out x))
								{
								txt = x.ToString();
								data[i].Ok = true;
								}
							else
								{
								data[i].Ok = false;
								}
							}
						break;
					case Def.InputType.Double:
							{
							double x;
							if(double.TryParse(txt,NumberStyles.Number,CultureInfo.CreateSpecificCulture("en-GB"),out x))
								{
								txt = x.ToString();
								data[i].Ok = true;
								}
							else
								{
								data[i].Ok = false;
								}
							}
						break;
					}
				if(data[i].Ok)
					data[i].Contenuto = txt;
				else
					data[i].Contenuto = string.Empty;
				}
			this.DialogResult = DialogResult.OK;
			this.Close();
			}
		private void bcn_Click(object sender,EventArgs e)
			{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
			}
		}
	}
