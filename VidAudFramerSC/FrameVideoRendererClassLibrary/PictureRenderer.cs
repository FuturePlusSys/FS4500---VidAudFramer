using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrameVideoRendererClassLibrary
{
    public partial class PictureRenderer : Form
    {
        #region Members
        private int m_frameid = 0;
        public int Frameid { get { return m_frameid; } set { m_frameid = value; } }

        private Metadata m_data = null;
        private Metadata Data { get { return m_data; } set { m_data = value; } }

        public event Passon PaintingEvent;
        public event Passon GetPixelData;
        public event Passon RequestPictureEvent;
        public event Passon CheckReferenceEvent;
        public event Passon CheckPictureEvent;
        #endregion
        /// <summary>
        /// When form is created, gets Metadata saved to Metadata object.
        /// </summary>
        /// <param name="data"></param>
        public PictureRenderer(Metadata data)
        {
            InitializeComponent();
            Frameid = data.Frameid;
            Data = data;
            init();
            loadMSAData(this);
            this.Text = "Picture Renderer Frame: " + Data.Frameid.ToString(); //Form Heading has frameid.

        }
        /// <summary>
        /// initilize events
        /// </summary>
        private void init() 
        {
            // register for the event in the user control that is statically placed in this form... 
            pictureUserControl1.PaintingPicture += new PaintingEvent(processPaintEvent);
            pictureUserControl1.GettingPixelData += new GetPixelData(getPixelData);
            pictureUserControl1.GettingMetaData += new GetMetaData(getMetaData);
            pictureUserControl1.SendingPicture += new RequestPictureEvent(sendpicture);
            pictureUserControl1.CheckReference += new CheckReferenceEvent(checkreference);
            pictureUserControl1.CheckPicture += new CheckPictureEvent(checkpicture);
        }
        /// <summary>
        /// Passing PaintEvent to FrameVideoRenderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processPaintEvent(object sender, PaintingEventArgs e)
        {
            if (PaintingEvent != null)
                PaintingEvent(this, e);
        }

        private void checkreference(object sender, CheckReferenceArgs e)
        {
            if (CheckReferenceEvent != null)
                CheckReferenceEvent(this, e);
        }

        private void checkpicture(object sender, CheckPictureArgs e)
        {
            if (CheckPictureEvent != null)
                CheckPictureEvent(this, e);
        }
        /// <summary>
        /// Passing PixelData event to FrameVideoRenderer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getPixelData(object sender, GettingPixelDataArgs e)
        {
            if (GetPixelData != null)
                GetPixelData(this, e);
        }

        private void sendpicture(object sender, SendingPictureArgs e)
        {
            if (RequestPictureEvent != null)
                RequestPictureEvent(this, e);
        }
        /// <summary>
        /// Filling eventarg with Metadata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getMetaData(object sender, GettingMetaDataArgs e)
        {
            e.MetaData = Data;
        }
        /// <summary>
        /// When the size of the picturerenderer changes, change picturebox from inside as well
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureRenderer_SizeChanged(object sender, EventArgs e)
        {
            PictureUserControlPanel.Width = this.Width - 60;
            PictureUserControlPanel.Height = this.Height - 60;
            pictureUserControl1.Width = PictureUserControlPanel.Width - 15;
            pictureUserControl1.Height = PictureUserControlPanel.Height - 15;
            foreach (Control control in PictureUserControlPanel.Controls)
            {
                if (control is UserControl)
                {
                    foreach (Control c in control.Controls)
                    {
                        if (c.Name == "PictureBoxPanel")
                        {
                            c.Width = pictureUserControl1.Width - 30;
                            c.Height = pictureUserControl1.Height - c.Location.Y - 30;
                            foreach (Control c2 in c.Controls)
                            {
                                if (c2.Name == "PictureBox")
                                {
                                    c2.Width = c.Width - 15;
                                    c2.Height = c.Height - 15;
                                }
                            }
                        }
                    }
                }
            }

        }
        private void loadMSAData(PictureRenderer pic)
        {
            foreach (Control control in pic.PictureUserControlPanel.Controls)
            {
                if (control is PictureUserControl)
                {
                    foreach (Control con in control.Controls)
                    {
                        if (con.Name == "SettingsRichTextBox")
                        {
                            ((RichTextBox)con).AppendText("MSA Data: \n");
                            ((RichTextBox)con).AppendText("   Format: " + Data.MSAData.Format.ToString() + "\n");
                            ((RichTextBox)con).AppendText("   PixelComponent: " + Data.MSAData.PixelComponent.ToString() + "\n");
                            ((RichTextBox)con).AppendText("   Width: " + Data.MSAData.Width.ToString() + "\n");
                            ((RichTextBox)con).AppendText("   Height: " + Data.MSAData.Height.ToString() + "\n");
                        }
                        if (con.Name == "ResolutionPanel")
                        {
                            foreach (Control num in con.Controls)
                            {
                                if (num.Name == "WidthnumericUpDown")
                                {
                                    ((NumericUpDown)num).Value = Data.MSAData.Width;
                                }
                                else if (num.Name == "HeightnumericUpDown")
                                {
                                    ((NumericUpDown)num).Value = Data.MSAData.Height;
                                }
                            }
                        }
                        //if (con.Name == "pixelformatGroupBox")
                        //{
                        //   foreach (Control radiobutton in con.Controls)
                        //    {
                        //        setFormat(radiobutton);
                        //        setWidth(radiobutton);
                        //    }
                        //}
                    }
                }
            }
        }

        private void pictureUserControl1_Load(object sender, EventArgs e)
        {

        }
    }
    /// <summary>
    /// This is just to pass event PictureRenderer -> FrameVideoRenderer.
    /// Talk to PS, this is probably not the right method of passing the event.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void Passon (object sender, EventArgs e);
}
