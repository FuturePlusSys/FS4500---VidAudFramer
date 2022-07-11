using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PixelRenderer
{
    public partial class MainForm : Form
    {
        #region Members
        #endregion // Members

        #region Constructor(s)

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion // Constructor(s)

        #region Event Handlers


        /// <summary>
        /// Process Help About menu being clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProbeAboutForm.ShowAboutForm(this);
        }

        #endregion // Event Handlers

        #region Private Methods
        #endregion // Private Metods

        #region Public Methods
        #endregion // Public Methods

        private void frameVideoRendererCtrl1_Load(object sender, EventArgs e)
        {

        }
    }
}
