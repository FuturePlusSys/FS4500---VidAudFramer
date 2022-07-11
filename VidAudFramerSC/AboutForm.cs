using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Text;

/// Listing 1
/// Complete Code for AboutForm.cs

namespace PixelRenderer
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class ProbeAboutForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox IconBox;
		private System.Windows.Forms.Label TextArea;
		private System.Windows.Forms.Button OKButton;

		private string assemblyVersion = string.Empty;
        private string AssemblyVersion { get { return assemblyVersion; } set { assemblyVersion = value; } }


		private string protocolNameStr = "";
		private string ProtocolNameStr { get { return protocolNameStr; } set { protocolNameStr = value; } }


        private string m_FPGAVersion = string.Empty;
        private string FPGAVersion { get { return m_FPGAVersion; } set { m_FPGAVersion = value; } }


        private string m_serialNumber = string.Empty;
        private string SerialNumber { get { return m_serialNumber; } set { m_serialNumber = value; } }


        private string m_ManufacturerID = string.Empty;
        private string ManufacturerID { get { return m_ManufacturerID; } set { m_ManufacturerID = value; } }


        private string m_ProductID = string.Empty;
        private string ProductID { get { return m_ProductID; } set { m_ProductID = value; } }



        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;


        /// <summary>
        /// Private Default Constructor
        /// </summary>
		private ProbeAboutForm( )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProbeAboutForm));
            this.IconBox = new System.Windows.Forms.PictureBox();
            this.TextArea = new System.Windows.Forms.Label();
            this.OKButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
            this.SuspendLayout();
            // 
            // IconBox
            // 
            this.IconBox.Location = new System.Drawing.Point(40, 56);
            this.IconBox.Name = "IconBox";
            this.IconBox.Size = new System.Drawing.Size(28, 28);
            this.IconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.IconBox.TabIndex = 0;
            this.IconBox.TabStop = false;
            // 
            // TextArea
            // 
            this.TextArea.Location = new System.Drawing.Point(96, 16);
            this.TextArea.Name = "TextArea";
            this.TextArea.Size = new System.Drawing.Size(224, 177);
            this.TextArea.TabIndex = 1;
            this.TextArea.Text = "Text Area";
            this.TextArea.Click += new System.EventHandler(this.TextArea_Click);
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(342, 16);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ProbeAboutForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(442, 202);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.TextArea);
            this.Controls.Add(this.IconBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProbeAboutForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About: ";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private void AboutForm_Load(object sender, System.EventArgs e)
		{
			Icon i			= Owner.Icon;
			this.Icon		= i;
			IconBox.Image	= i.ToBitmap();
            string FPGAVersion = string.Empty;

			Assembly ThisAssembly = Assembly.GetExecutingAssembly();
			AssemblyName ThisAssemblyName = ThisAssembly.GetName();
            string FriendlyVersion = string.Format("{0:D2}.{1:D2}.{2:D2}.{3:D4}", ThisAssemblyName.Version.Major, ThisAssemblyName.Version.Minor, ThisAssemblyName.Version.Build, ThisAssemblyName.Version.Revision);

            Array Attributes = ThisAssembly.GetCustomAttributes( false );

			string Title	= "Unknown Application";
			string Copyright = "Unknown Copyright";
			foreach ( object o in Attributes )
			{
				if ( o is AssemblyTitleAttribute )
				{
					Title = ((AssemblyTitleAttribute)o).Title;
				}
				else if ( o is AssemblyCopyrightAttribute )
				{
					Copyright = ((AssemblyCopyrightAttribute)o).Copyright;
				}
			}

			this.Text = "About " + Title;

			StringBuilder sb = new StringBuilder("");

            //string[] comps = ProductID.Split(new char[] { '_' });
            //string truncatedProductID = comps[0];


			sb.Append ("FuturePlus Systems Corporation" + "\n\n");
			sb.Append( Title + "\n" );
            sb.Append("Program Version:    \t" + FriendlyVersion + "\n\n");

            sb.Append("\n\n" + Copyright + "\n");

			TextArea.Text = sb.ToString();
		}

		private void OKButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}


        /// <summary>
        /// Display the help form with infomation about the connected device.
        /// </summary>
        /// <param name="Owner"></param>
        /// <param name="protocolName"></param>
        /// <param name="assemblyVersion"></param>
        /// <param name="FPGAVersion"></param>
        /// <param name="serialNumber"></param>
        /// <param name="manfacturerID"></param>
        /// <param name="productID"></param>
		internal static void ShowAboutForm( IWin32Window Owner) //, string protocolName, string assemblyVersion ) //string FPGAVersion, string serialNumber, string manfacturerID, string productID)
		{
			System.Diagnostics.Debug.Assert( ( Owner != null ) ||
				!( Owner is IWin32Window ) ,
				"AboutForm MUST be supplied with a valid parent window" );

			ProbeAboutForm form = new ProbeAboutForm( );

			// set the properties
			//form.ProtocolNameStr = protocolName;
   //         form.assemblyVersion = assemblyVersion;
            //form.FPGAVersion = FPGAVersion;
            //form.SerialNumber = serialNumber;
            //form.ManufacturerID = manfacturerID;
            //form.ProductID = productID;
			form.ShowDialog( Owner );
		}

		private void TextArea_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}



