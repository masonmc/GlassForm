using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;

namespace GlassForm
{
	public class GlassForm : Form
	{
	
		public GlassForm()
		{
			
			
		}

		bool m_isGlassCurrently = false;

		bool m_isGlass = true;
		public bool IsGlass { get { return m_isGlass; } set { m_isGlass = true; } }

		enum eGlassSupported
		{
			Yes,
			No,
			NotSure
		};

		[DllImport("dwmapi.dll")]
		static extern void DwmIsCompositionEnabled(ref bool pfEnabled);

		struct Margins
		{
			public int Left;
			public int Right;
			public int Top;
			public int Bottom;
		}

		[DllImport("dwmapi.dll")]
		static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMargins);

		static void CheckForGlassSupport()
		{
			if (Environment.OSVersion.Version.Major < 6)
			{
				sm_glassSupported = eGlassSupported.No;
				return;
			}

			
			bool isGlassSupported = false;
			DwmIsCompositionEnabled(ref isGlassSupported);
			sm_glassSupported = isGlassSupported ? eGlassSupported.Yes : eGlassSupported.No;
		}

		void TurnOnGlass()
		{
			Margins marg;
			marg.Top = 0; // extend 20 pixels from the top
			marg.Left = -1;
			marg.Right = 0;
			marg.Bottom = 0;
			DwmExtendFrameIntoClientArea(this.Handle, ref marg);

			BackColor = Color.Black;

			m_isGlassCurrently = true;
		}

		static eGlassSupported sm_glassSupported = eGlassSupported.NotSure;

		protected override void  WndProc(ref Message m)
		{
			if (sm_glassSupported == eGlassSupported.NotSure)
			{
				CheckForGlassSupport();
			}
			if (IsGlass && !m_isGlassCurrently && sm_glassSupported == eGlassSupported.Yes)
			{
				TurnOnGlass();
			}

			bool intercepted = false;
			if (m_isGlassCurrently)
			{
				// if this is a click and it is on the client
				if (m.Msg == 0x84)
				{
					this.DefWndProc(ref m);
					if (m.Result.ToInt32() == 1)
					{
						m.Result = new IntPtr(2); // lie and say they clicked on the title bar
						intercepted = true;
					}
				}
			}

			if (!intercepted)
			{
				base.WndProc(ref m);
			}
		}

		

	}
}
