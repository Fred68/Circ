using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Windows.Forms;

namespace Circ
	{

	// Thanks to Bob Powell article: http://bobpowell.net/transcontrols.aspx

	class TransparentPanel : Panel
		{
		public TransparentPanel()
				{
				}

		#if(true)			// Trasparenza
		protected override CreateParams CreateParams
			{
			get
				{
				CreateParams cp=base.CreateParams;
				cp.ExStyle|=0x00000020; //WS_EX_TRANSPARENT
				return cp;
				}
			}
		#endif

		protected void InvalidateEx()
			{
			if(Parent==null)
				return;
			Rectangle rc = new Rectangle(this.Location,this.Size);
			Parent.Invalidate(rc,true);
			}

		protected override void OnPaintBackground(PaintEventArgs pevent)
			{}

		protected override void OnPaint(PaintEventArgs e)
			{
			int h=this.Height/2;
			int w=this.Width/2;
			Pen p=new Pen(Color.LightGreen,2);
			int x,y;
			for(x=0,y=0; x<w; x+=w/10, y+=h/10)
				{
				e.Graphics.DrawLine(p,0,0,200,200);
				}
			p.Dispose();
			}
		}
	}
