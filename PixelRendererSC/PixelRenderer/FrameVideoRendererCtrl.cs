using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DP12MSTClassLibrary;
using DP14MSTClassLibrary;
using DP12SSTClassLibrary;
using DP14SSTClassLibrary;
using FPSProbeMgr_Gen2;
using System.Xml;
using System.Collections;
using System.IO;

namespace FrameVideoRendererClassLibrary
{
    public partial class FrameVideoRendererCtrl : UserControl
    {
        /// <summary>
        /// Used to pass CancellationToken to Task. Allows Cancel button to stop task
        /// </summary>
        private class GetAudioDataStateObject
        {
            public CancellationToken Token { get; set; }

            public GetAudioDataStateObject(CancellationToken ct)
            {
                Token = ct;
            }
        }

        public class MSAData
        {
            private string m_format = "";
            public string Format { get { return m_format; } set { m_format = value; } }
            private int m_pixelcomponent = 0;
            public int PixelComponent { get { return m_pixelcomponent; } set { m_pixelcomponent = value; } }
            private int m_width = 0;
            public int Width { get { return m_width; } set { m_width = value; } }
            private int m_height = 0;
            public int Height { get { return m_height; } set { m_height = value; } }

            public MSAData(string format, int pixelcomponent, int width, int height)
            {
                Format = format;
                PixelComponent = pixelcomponent;
                Width = width;
                Height = height;
            }
        }

        #region Members
        //private DP11SST m_DP11aProbe = null;
        private DP12SST m_DP12SSTProbe = null;
        private DP12MST m_DP12MSTProbe = null;
        private DP14SST m_DP14SSTProbe = null;
        private DP14MST m_DP14MSTProbe = null;
        IProbeMgrGen2 m_IProbe = null;              // this sets us up for polymorphism...  because DP12MST inherits the FPSProbeMgrGen2 interface,
                                                    // then all the different probe versions can be assigned to this one base type.
        private List<long> pixelList = new List<long>();

        private List<PictureRenderer> pictures = new List<PictureRenderer>();

        private string m_protocol = "";
        private string Protocol { get { return m_protocol; } set { m_protocol = value; } }

        private int m_lanes = 0;
        private int Lanes { get { return m_lanes; } set { m_lanes = value; } }
        private int m_eventwidth = 0;
        private int EventWidth { get { return m_eventwidth; } set { m_eventwidth = value; } }

        private int m_eventsb = 0;
        private int Eventsb { get { return m_eventsb; } set { m_eventsb = value; } }
        private int m_lane0sb = 0;
        private int Lane0sb { get { return m_lane0sb; } set { m_lane0sb = value; } }

        private int m_lane1sb = 0;
        private int Lane1sb { get { return m_lane1sb; } set { m_lane1sb = value; } }

        private int m_lane2sb = 0;
        private int Lane2sb { get { return m_lane2sb; } set { m_lane2sb = value; } }

        private int m_lane3sb = 0;
        private int Lane3sb { get { return m_lane3sb; } set { m_lane3sb = value; } }

        private int m_triggerwidth = 0;
        private int TriggerWidth { get { return m_triggerwidth; } set { m_triggerwidth = value; } }

        private int m_triggersb = 0;
        private int Triggersb { get { return m_triggersb; } set { m_triggersb = value; } }

        private int m_statesbeforetrigger = 0;
        private int StatesBeforeTrigger { get { return m_statesbeforetrigger; } set { m_statesbeforetrigger = value; } }

        private string m_defaultFolderPath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"FuturePlus\FS4500\Instance1");

        private CancellationTokenSource m_tokenSource = null; //To be passed with task
        private CancellationToken m_token = CancellationToken.None;

        #endregion // Members

        #region Ctor

        /// <summary>
        /// Default Constructor(s)
        /// </summary>
        public FrameVideoRendererCtrl()
        {
            InitializeComponent();
            DefaultSetup();
        }
        #endregion // Ctor(s)

        #region Event Handlers

        /// <summary>
        /// Protocol selection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_2SSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            VC1button.Enabled = false;
            VC2button.Enabled = false;
            VC3button.Enabled = false;
            VC4button.Enabled = false;
            VC1button.Checked = false;
            VC2button.Checked = false;
            VC3button.Checked = false;
            VC4button.Checked = false;
        }
        /// <summary>
        /// Protocol selection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_2MSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_2MSTbutton.Checked == true)
            {
                VC1button.Enabled = true;
                VC2button.Enabled = true;
                VC3button.Enabled = true;
                VC4button.Enabled = true;
            }
        }
        /// <summary>
        /// Protocol selection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_3SSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            VC1button.Enabled = false;
            VC2button.Enabled = false;
            VC3button.Enabled = false;
            VC4button.Enabled = false;
            VC1button.Checked = false;
            VC2button.Checked = false;
            VC3button.Checked = false;
            VC4button.Checked = false;
        }
        /// <summary>
        /// Protocol selection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DP1_3MSTbutton_CheckedChanged(object sender, EventArgs e)
        {
            if (DP1_4MSTbutton.Checked == true)
            {
                VC1button.Enabled = true;
                VC2button.Enabled = true;
                VC3button.Enabled = true;
                VC4button.Enabled = true;
            }
        }
        /// <summary>
        /// Handles Event, When checked, PictureRender will popup, will close when unchecked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameCheckBoxChecked(object sender, EventArgs e)
        {
            if (sender is CheckBox)  
            {
                // research the as operand... CheckBox myCheckbox = sender as Checkbox..
                
                // using(PictureRenderer picture = new PictureRenderer((Metadata)check.Tag))
                // {

                // }

                // determine that the renderer form is not already create..
                // if not created
                //   instantiate PictureRenderer form.
                //   show the form... PictureRenderer.Show();  .ShowDialog() is for modal mode
                // else
                //   update status msg label with erro msg
            }


            if (sender is CheckBox)
            {
                CheckBox check = ((CheckBox)sender); //Get object reference associated with checkbox
                if (check.Checked == true)
                {
                    PictureRenderer picture = new PictureRenderer((Metadata)check.Tag); //Overload Constructor, pass metadata
                    picture.PaintingEvent += new Passon(processPaintEvent); //Initalize Event that will be used from PictureRenderer
                    picture.GetPixelData += new Passon(getpixeldata); //Initalize Event that will be used from PictureRenderer
                    picture.RequestPictureEvent += new Passon(getPicture);
                    picture.CheckReferenceEvent += new Passon(checkReference);
                    picture.CheckPictureEvent += new Passon(checkPicture);
                    pictures.Add(picture); //Add picture to list
                    picture.Show();
                }
                if (check.Checked == false)
                {
                    foreach (PictureRenderer picture in pictures)
                    {
                        if (picture.Frameid == ((Metadata)check.Tag).Frameid) //Find picturerenderer associated with checkbox
                        {
                            picture.Close();
                            pictures.Remove(picture);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If picture renderer painting, disable all other forms, when done enable them again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void processPaintEvent(object sender, EventArgs e)
        {
            if (((PaintingEventArgs)e).Painting == true)
            {
                this.Enabled = false; //Disable main form
                foreach (PictureRenderer picture in pictures)
                {
                    if (picture.Frameid != ((PaintingEventArgs)e).Frameid) //Disable any form besides one painting
                    {
                        picture.Enabled = false;
                    }
                }
            }
            else
            {
                this.Enabled = true;
                foreach (PictureRenderer picture in pictures)
                {
                    if (picture.Frameid != ((PaintingEventArgs)e).Frameid)
                    {
                        picture.Enabled = true;
                    }
                }
            }
        }

        private void checkReference(object sender, EventArgs e)
        {
            foreach (Control control in ComparePanel.Controls)
            {
                if (control is CheckBox)
                {
                    CheckBox check = ((CheckBox)control);
                    if (check.Font.Bold)
                    {
                        ((CheckReferenceArgs)e).ReferenceCheck = false;
                        break;
                    }
                }
            }
        }

        private void checkPicture(object sender, EventArgs e)
        {
            string text = "Frame: " + ((CheckPictureArgs)e).FrameID.ToString() + " ";
            foreach (Control control in ComparePanel.Controls)
            {
                if (control is CheckBox)
                {
                    CheckBox check = ((CheckBox)control);
                    if (check.Text == text || check.Text == (text + "Reference"))
                    {
                        ComparePanel.Controls.Remove(check);
                    }
                }
            }
        }
        /// <summary>
        /// If picture renderer requests data, fill eventargs with data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getpixeldata(object sender, EventArgs e)
        {
            List<byte> data = m_IProbe.GetStateDataChunk(((GettingPixelDataArgs)e).Stateindex, ((GettingPixelDataArgs)e).Statechunk, ((GettingPixelDataArgs)e).VC);
            ((GettingPixelDataArgs)e).Statedata = data;
        }
        /// <summary>
        /// Add List of Pixels to checkbox associated with frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getPicture(object sender, EventArgs e)
        {
            string checktext = "Frame: " + ((SendingPictureArgs)e).Frameid.ToString() + " " + ((SendingPictureArgs)e).Reference;
            bool add = true;
            foreach (Control control in ComparePanel.Controls) //If this frame has been added before
            {
                if (control is CheckBox)
                {
                    CheckBox checkbox = ((CheckBox)control);
                    if (checktext == checkbox.Text)
                    {
                        add = false;
                    }
                }
            }
            if (add == true) //Add since frame not in panel
            {
                CheckBox check = new CheckBox();
                if (((SendingPictureArgs)e).Reference != "")
                {
                    check.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                check.Text = checktext;
                check.Tag = ((SendingPictureArgs)e).Pixels;
                ComparePanel.Controls.Add(check);
            }
                
        }
        /// <summary>
        /// Used to select file path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browsebutton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                DataFolderPath_TextBox.Text = folderDialog.SelectedPath;
            }
        }
        /// <summary>
        /// Stop collecting frames and stop async task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (m_tokenSource != null)
                m_tokenSource.Cancel();
        }
        /// <summary>
        /// Initial setup, preparing to get frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetFramesbutton_Click(object sender, EventArgs e)
        {
            Protocol = getprotocol(); //Save Protocol
            bool errorflag = false; //If error, will prevent program from collecting frames
            string error = "";
            if (Directory.Exists(m_defaultFolderPath))
            {
                if (Protocol == "error")
                {
                    error = "No Protocol Selected";
                    errorflag = true;
                    Runerror err = new Runerror(error);
                    err.Show();
                }
                else
                {
                    createInterfaceObject();  // assign the m_IProbe variable.
                    m_IProbe.Initialize();
                }
                if (Lane1button.Checked)
                {
                    Lanes = 1;
                }
                else if (Lane2button.Checked)
                {
                    Lanes = 2;
                }
                else if (lane4button.Checked)
                {
                    Lanes = 4;
                }
                xmlreader(); //Get tracebuffer data from xml file
                int vc = getvc();
                int states = 0;
                if (vc == 0)
                {
                    error = "No Virtual Channel Selected";
                    errorflag = true;
                    Runerror err = new Runerror(error);
                    err.Show();
                }
                else
                {
                    states = (int)m_IProbe.GetNumberOfStates(vc);
                }
                if (states == 0)
                {
                    error = "No states found. Check Protocol or Probe Manager or Virtual Channel or Filepath";
                    errorflag = true;
                    Runerror err = new Runerror(error);
                    err.Show();
                }
                if (errorflag == false)
                {
                    this.toolStripProgressBar1.Maximum = states;
                    this.toolStripStatusLabel1.Text = "Counting Frames";
                    this.statusStrip1.Update();
                    enable(false);
                    richTextBox1.Text = "";
                    ComparePanel.Controls.Clear();
                    getframes();
                }
            }
            else
            {
                error = "ProbeManager closed, must be open when using this application";
                Runerror err = new Runerror(error);
                err.Show();
            }
        }
        /// <summary>
        /// Fills richtextbox with data about frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getstatesButton_Click(object sender, EventArgs e)
        {
            foreach (Control control in FramesPanel.Controls)
            {
                if (control is CheckBox)
                {
                    Metadata data = (Metadata)control.Tag;
                    string text = data.Frameid + " StartState: " + (data.StartState - data.StatesBeforeTrig).ToString() + " EndState: " + (data.EndState - data.StatesBeforeTrig).ToString() + "First Pixel: " + (data.FirstPixelState - data.StatesBeforeTrig).ToString() + "\n";
                    richTextBox1.AppendText(text); 
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private async void getframes()
        {
            FramesPanel.Controls.Clear(); //Clear all frames previously collected
            foreach (PictureRenderer pic in pictures) //If frames had PictureRenderers open, close them
            {
                pic.Close();
            }
            pictures.Clear();

            // create the cancellatin token
            m_tokenSource = new CancellationTokenSource();
            m_token = m_tokenSource.Token;

            //From Audio Renderer Code
            Task<List<Metadata> >[] tasks = new Task<List<Metadata> >[1];
            ThreadLocal<GetAudioDataStateObject> tls = new ThreadLocal<GetAudioDataStateObject>();

            tasks[0] = new Task<List< Metadata> >((stateObject) => {
                tls.Value = (GetAudioDataStateObject)stateObject;
                ((GetAudioDataStateObject)tls.Value).Token.ThrowIfCancellationRequested();
                return dowork(0,0,tls.Value.Token);
            }, new GetAudioDataStateObject(m_token));

            foreach (Task t in tasks)
                t.Start();

            await Task.WhenAll(tasks); //Code below not run until Task complete

            List<Metadata> metadata = tasks[0].Result; //Get metadata saved to Task from dowork.
            foreach(Metadata data in metadata) //For each metadata object, create new frame and checkbox associated with that frame.
            {
                CheckBox check = new CheckBox();
                data.StatesBeforeTrig = StatesBeforeTrigger;
                check.Tag = data;
                if (data.HDCP == true)
                {
                    check.Text = "Frame: " + data.Frameid.ToString();
                }
                else
                {
                    check.Text = "Frame: " + data.Frameid.ToString() + " HDCP Enabled";
                    check.ForeColor = System.Drawing.Color.Red;
                }
                check.Enabled = false;
                check.CheckedChanged += new EventHandler(FrameCheckBoxChecked);
                FramesPanel.Controls.Add(check);
            }
            enable(true);
            if (FramesPanel.Controls.Count == 0) //No frames found
            {
                string error = "No Frames Found or Cancel Button Pushed";
                Runerror err = new Runerror(error);
                err.Show();
            }

            this.Invoke(new Action(() =>
            {
                this.toolStripStatusLabel1.Text = "Ready";
                this.statusStrip1.Update();
            }));
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ComparePanel.Controls.Count; i++)
            {
                CheckBox check = ((CheckBox)ComparePanel.Controls[i]);
                if (check.Checked == true)
                {
                    ComparePanel.Controls.Remove(check);
                    i -= 1;
                }
            }
        }
        private void TestChecked_Click(object sender, EventArgs e)
        {
            foreach (Control control in ComparePanel.Controls)
            {
                List<PixelData> pic1 = null;
                string pic1frame = "";
                CheckBox check = ((CheckBox)control);
                if (check.Checked == true)
                {
                    pic1 = (List<PixelData>)check.Tag;
                    pic1frame = check.Text;
                    foreach (PixelData pixel in pic1)
                    {
                        if (pixel.XLocation == 0 || pixel.YLocation == 0)
                        {
                            string text = "Frame " + check.Text + "X and Y Location (" + pixel.XLocation.ToString() + "," + pixel.YLocation.ToString() + ") PixelStart: " + pixel.StartState + " PixelEnd: " + pixel.EndState + "\n";
                            richTextBox1.AppendText(text);
                        }
                        if (pixel.XLocation == 1919)
                        {
                            string text = "Frame " + check.Text + "X and Y Location (" + pixel.XLocation.ToString() + "," + pixel.YLocation.ToString() + ") PixelStart: " + pixel.StartState + " PixelEnd: " + pixel.EndState + "\n";
                            richTextBox1.AppendText(text);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Load a picture from outside file to test data files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadReferenceButton_Click(object sender, EventArgs e)
        {
            bool add = true;
            foreach (Control control in ComparePanel.Controls)
            {
                if (control.Font.Bold)
                {
                    string err = "Already a Reference Frame Set";
                    Runerror error = new Runerror(err);
                    error.Show();
                    add = false;
                    break;
                }
            }
            if (add == true)
            {
                List<PixelData> pixels = new List<PixelData>();
                OpenFileDialog folderDialog = new OpenFileDialog();

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    var bmp1 = Image.FromFile(folderDialog.FileName);
                    bmp1 = new Bitmap(folderDialog.FileName);
                    for (int h = 0; h < ((Bitmap)bmp1).Height; h++)
                    {
                        for (int w = 0; w < ((Bitmap)bmp1).Width; w++)
                        {
                            Color c = ((Bitmap)bmp1).GetPixel(w, h);
                            PixelData pixel = new PixelData(h, w, 0, 0, c.R, c.G, c.B, true);
                            pixels.Add(pixel);
                        }
                    }

                }

                CheckBox check = new CheckBox();
                check.Text = "Reference Frame";
                check.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                check.Tag = pixels;
                ComparePanel.Controls.Add(check);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void ComparePicturesButton_Click(object sender, EventArgs e)
        {
            int checkedboxes = 0;
            bool error = false;
            List<PixelData> pic1 = null;
            string pic1frame = "";
            List<PixelData> pic2 = null;
            string pic2frame = "";
            if (ComparePanel.Controls.Count < 2)
            {
                error = true;
            }
            else
            {
                foreach (Control control in ComparePanel.Controls)
                {
                    CheckBox check = ((CheckBox)control);
                    if (check.Checked == true)
                    {
                        checkedboxes += 1;
                        if (checkedboxes == 1)
                        {
                            pic1 = (List<PixelData>)check.Tag;
                            pic1frame = check.Text;
                        }
                        else if (checkedboxes == 2)
                        {
                            pic2 = (List<PixelData>)check.Tag;
                            pic2frame = check.Text;
                        }
                    }
                }
            }
            if (checkedboxes != 2)
            {
                string err = "Must Have Exactly 2 Pictures Checked";
                Runerror errorform = new Runerror(err);
                errorform.Show();
                error = true;
            }
            else
            {
                if (pic1.Count != pic2.Count)
                {
                    string err = "Pictures Have Different Number of Pixels";
                    Runerror errorform = new Runerror(err);
                    errorform.Show();
                    error = true;
                }
            }
            if (error != true)
            {
                int iterator = 0;
                StringBuilder tableRtf = new StringBuilder();
                //beginning of rich text format,dont customize this begining line              
                tableRtf.Append(@"{\rtf1 ");
                for (int i = 0; i < 1; i++)
                {

                    tableRtf.Append(@"\trowd");

                    //A cell with width 1000.
                    tableRtf.Append(@"\cellx1800");

                    //Another cell with width 2000.end point is 3000 (which is 1000+2000).
                    tableRtf.Append(@"\cellx2600");

                    //Another cell with width 1000.end point is 4000 (which is 3000+1000)
                    tableRtf.Append(@"\cellx3400");

                    //Another cell with width 1000.end point is 4000 (which is 3000+1000)
                    tableRtf.Append(@"\cellx4000");
                    tableRtf.Append(@"\cellx4600");
                    tableRtf.Append(@"\cellx5200");
                    tableRtf.Append(@"\cellx7700");


                    tableRtf.Append(@"\intbl Frame# \cell XLocation \cell YLocation \cell RValue \cell GValue \cell BValue \cell State Range of Pixel \cell  \row"); //create row

                }

                tableRtf.Append(@"\pard");
                bool pixelcompare = true;
                int maxcount = 0;
                int pixelmax = 10000;
                while (iterator != pic1.Count)
                {
                    if (maxcount == pixelmax)
                    {
                        string err = pixelmax.ToString() + " Different Pixels, Reached Maximum, Compare Ended";
                        Runerror errorform = new Runerror(err);
                        errorform.Show();
                        break;
                    }
                    if (pic1[iterator].BValue == pic2[iterator].BValue & pic1[iterator].GValue == pic2[iterator].GValue & pic1[iterator].RValue == pic2[iterator].RValue)
                    {

                    }
                    else
                    {
                        pixelcompare = false;
                        for (int i = 0; i < 3; i++)
                        {
                            if (i == 0)
                            {
                                tableRtf.Append(@"\intbl " + pic1frame + @" \cell " + pic1[iterator].XLocation.ToString() + @" \cell " + pic1[iterator].YLocation.ToString() + @" \cell " + pic1[iterator].RValue.ToString() + @" \cell " + pic1[iterator].GValue.ToString() + @" \cell " + pic1[iterator].BValue.ToString() + @" \cell " + pic1[iterator].StartState.ToString() + " - " + pic1[iterator].EndState.ToString() + @" \cell \row"); //create row
                            }
                            else if (i == 1)
                            {
                                tableRtf.Append(@"\intbl " + pic2frame + @" \cell " + pic2[iterator].XLocation.ToString() + @" \cell " + pic2[iterator].YLocation.ToString() + @" \cell " + pic2[iterator].RValue.ToString() + @" \cell " + pic2[iterator].GValue.ToString() + @" \cell " + pic2[iterator].BValue.ToString() + @" \cell " + pic2[iterator].StartState.ToString() + " - " + pic2[iterator].EndState.ToString() + @" \cell \row"); //create row
                            }
                            else
                            {
                                tableRtf.Append(@"\intbl \cell \row"); //create row
                            }

                        }

                        tableRtf.Append(@"\pard");
                        maxcount++;
                        //error = true;
                        //pic1[iterator].Compare = false;
                        //pic2[iterator].Compare = false;
                        //string text = pic1frame + "   X: " + pic1[iterator].XLocation.ToString() + " Y: " + pic1[iterator].YLocation.ToString() + "\n   R: " + pic1[iterator].RValue.ToString() + "\n   G: " + pic1[iterator].GValue.ToString() + "\n   B: " + pic1[iterator].BValue.ToString() + "\n   StateRange: " + pic1[iterator].StartState.ToString() + " - " + pic1[iterator].EndState.ToString() +  "\n";
                        //richTextBox1.AppendText(text);
                        //text = pic2frame + "   X: " + pic2[iterator].XLocation.ToString() + " Y: " + pic2[iterator].YLocation.ToString() + "\n   R: " + pic2[iterator].RValue.ToString() + "\n   G: " + pic2[iterator].GValue.ToString() + "\n   B: " + pic2[iterator].BValue.ToString() + "\n   StateRange: " + pic2[iterator].StartState.ToString() + " - " + pic2[iterator].EndState.ToString() + "\n";
                        //string text = pic1frame + "   X: " + pic1[iterator].XLocation.ToString() + " Y: " + pic1[iterator].YLocation.ToString() + "             " + pic2frame + "   X: " + pic2[iterator].XLocation.ToString() + " Y: " + pic2[iterator].YLocation.ToString(); 
                    }
                    iterator++;
                }
                tableRtf.Append(@"}");

                this.richTextBox1.Rtf = tableRtf.ToString();
                if (pixelcompare == true)
                {
                    richTextBox1.Text = "";
                    MessageForm form = new MessageForm("Pictures " + pic1frame + " and " + pic2frame + " Are the Same");
                    form.Show();
                }
            }
        }

        #endregion // Event Handlers

        #region Private Methods
        /// <summary>
        /// Gatherframes, run on Task and returning metadata object
        /// </summary>
        /// <param name="startstate"></param>
        /// <param name="frames"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private List<Metadata> dowork(int startstate, int frames, CancellationToken token)
        {
            // check if "Task {0} was cancelled before it got started.",
            if (token.IsCancellationRequested == true)
            {
                token.ThrowIfCancellationRequested();
            }
            bool errorflag = false;
            bool startofframe = false;
            bool firstpixel = false;
            bool foundpixel = false;
            string VBS = "0x4A";
            string HBE = "0x15";
            string HMSA = "0x5C";
            string VMSA = "0x1C";
            string VBID = "0x49";
            int stateindex = startstate;
            int chunkSize = 4096;
            int vc = getvc();
            int states = (int)m_IProbe.GetNumberOfStates(vc);
            List<byte> Statechunk = new List<byte>();
            int stateDataLength = 16;
            byte[] stateData = new byte[stateDataLength];
            int clear = 0;
            int startofframestate = 0;
            int firstpixelstate = 0;
            List<Metadata> data = new List<Metadata>();
            List<List<byte>> MSAdata = new List<List<byte>>();
            MSAData MSAObject = null;
            StatesBeforeTrigger = 0; //In case the user selects getframes multiple times and changes file
            bool HDCP = false;
            if (errorflag == false)
            {
                for (; stateindex < states; stateindex += chunkSize)
                {
                    Statechunk = m_IProbe.GetStateDataChunk((int)stateindex, 4096, vc); //Get statechunk from statelisting
                    chunkSize = Statechunk.Count / stateDataLength; //Get size of chunk.
                    if (chunkSize > 0) //Should only be zero if no states left in listing.
                    {
                        for (int chunkIndex = 0; (chunkIndex < chunkSize) && (stateindex + chunkIndex) < states; chunkIndex += 1) //Search through current statechunk
                        {
                            getStateDataFromChunk(chunkIndex * 16, Statechunk, ref stateData); 
                            StringBuilder sb = Geteventcode(stateData); 
                            String eventcode = sb.ToString();
                            if (StatesBeforeTrigger == 0)
                            {
                                if (IsTrigger(stateData) == true)
                                {
                                    StatesBeforeTrigger = stateindex + chunkIndex;
                                }
                            }
                            if (eventcode == VBID)
                            {
                                HDCP = false;
                                HDCP = checkHDCP(stateData);
                                
                            }
                            if (eventcode == HMSA || eventcode == VMSA)
                            {
                                if (startofframe == false)
                                {
                                    startofframe = true;
                                    startofframestate = stateindex + chunkIndex;
                                }
                                List<byte> lanedata = Getlanedata(stateData);
                                MSAdata.Add(lanedata);
                                if (Lanes == 4 & MSAdata.Count == 12)
                                {
                                    MSAObject = createMSADataObject(Lanes, MSAdata);
                                }
                                else if (Lanes == 2 & MSAdata.Count == 21)
                                {
                                    MSAObject = createMSADataObject(Lanes, MSAdata);
                                }
                                else if (Lanes == 1 & MSAdata.Count == 39)
                                {
                                    MSAObject = createMSADataObject(Lanes, MSAdata);
                                }
                            }
                            else if (eventcode == HBE && startofframe == true) //First PixelState of frame
                            {
                                firstpixel = true;
                                if (foundpixel == false)
                                {
                                    firstpixelstate = stateindex + chunkIndex + 1;
                                    foundpixel = true;
                                }
                            }
                            else if (eventcode == VBS && startofframe == true && firstpixel == true) //Found VBS, frame complete. 
                            {
                                //capturedframe
                                startofframe = false;
                                firstpixel = false;
                                foundpixel = false;
                                frames += 1;
                                int endstate = stateindex + chunkIndex;
                                Metadata meta = new Metadata(frames, startofframestate, firstpixelstate, endstate, Lanes, Protocol, vc, DataFolderPath_TextBox.Text, 0, MSAObject, HDCP);
                                data.Add(meta); //Add meta data to list.
                            }
                        }
                    }
                    else
                    {
                        errorflag = true;
                    }
                    Statechunk.Clear(); //Clear statechunk, get next statechunk
                    if (token.IsCancellationRequested) //If cancel pushed, get out of task, clear data.
                    {
                        data.Clear();
                        break;
                    }
                    else
                    {
                        clear += 1;
                        if (clear == 8) //counter to increment, limiting times switching between threads
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.toolStripProgressBar1.Increment(chunkSize * 8);
                                this.statusStrip1.Update();
                            }));
                            clear = 0;
                        }
                    }
                }
            }
            this.Invoke(new Action(() =>
            {
                this.toolStripStatusLabel1.Text = "Done";
                this.toolStripProgressBar1.Value = 0; ;
                this.statusStrip1.Update();
            }));

            return data;
            //this.toolStripProgressBar1.Maximum = 0;
        }

        private bool checkHDCP(byte[] data)
        {
            byte mask = 0x20;
            List<byte> lanedata = new List<byte>();
            lanedata = Getlanedata(data);
            byte VBID = (byte)(lanedata[0] & mask);
            if (VBID != 0x00)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
       
        private MSAData createMSADataObject(int lanes, List<List<byte> > data)
        {
            int height = 0;
            int width = 0;
            int component = 0;
            int MISC0 = 0;
            int formatbyte = 0;
            string format = "";
            if (lanes == 4)
            {
                width = data[5][2];
                width = width << 8;
                width += data[6][2];
                height = data[7][2];
                height = height << 8;
                height += data[8][2];
                MISC0 = data[8][3];
                component = MISC0 & 0xE0;
                component = getcomponent(component);
                formatbyte = MISC0 & 0x06;
                format = getformat(formatbyte);
            }
            if (lanes == 2)
            {
                height = data[16][0];
                height = height << 8;
                height += data[17][0];
                width = data[14][0];
                width = width << 8;
                width += data[15][0];
                MISC0 = data[17][1];
                component = MISC0 & 0xE0;
                component = getcomponent(component);
                formatbyte = MISC0 & 0x06;
                format = getformat(formatbyte);
            }
            if (lanes == 1)
            {
                height = data[26][0];
                height = height << 8;
                height += data[27][0];
                width = data[24][0];
                width = width << 8;
                width += data[25][0];
                MISC0 = data[36][0];
                component = MISC0 & 0xE0;
                component = getcomponent(component);
                formatbyte = MISC0 & 0x06;
                format = getformat(formatbyte);
            }
            MSAData MSAObject = new MSAData(format, component, width, height);
            return MSAObject;
        }

        private int getcomponent(int comp)
        {
            if (comp == 0x00)
            {
                comp = 18;
            }
            else if (comp == 0x20)
            {
                comp = 24;
            }
            else if (comp == 0x40)
            {
                comp = 30;
            }
            else if (comp == 0x60)
            {
                comp = 36;
            }
            else if (comp == 0x80)
            {
                comp = 48;
            }
            return comp;
        }

        private string getformat(int bits)
        {
            string format = "";
            if (bits == 0x00)
            {
                format = "RGB";
            }
            else if (bits == 0x02)
            {
                format = "YCbCr422";
            }
            else if (bits == 0x04)
            {
                format = "YCbCr444";
            }
            return format;
        }
        /// <summary>
        /// Either enable or disable controls in mainform.
        /// </summary>
        /// <param name="flag"></param>
        private void enable(bool flag)
        {
            groupBox1.Enabled = flag;
            groupBox3.Enabled = flag;
            panel2.Enabled = flag;
            Browsebutton.Enabled = flag;
            DataFolderPath_TextBox.Enabled = flag;
            GetFramesbutton.Enabled = flag;
            foreach (Control control in FramesPanel.Controls)
            {
                control.Enabled = flag;
            }
            getstatesButton.Enabled = flag;
            richTextBox1.Enabled = flag;
        }
       
        /// <summary>
        /// Create the class library and initialize the interface variable
        /// </summary>
        private bool createInterfaceObject()
        {
            string protocol = getprotocol();
            bool status = true;
            //string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"FuturePlus");
            string path = DataFolderPath_TextBox.Text;

            switch (protocol)
            {
                case "SST-1.2":
                    if (m_DP12SSTProbe != null)
                        m_DP12SSTProbe = null;
                    m_DP12SSTProbe = new DP12SST(path);
                    m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;
                    break;
                case "MST-1.2":
                    if (m_DP12MSTProbe != null)
                        m_DP12MSTProbe = null;
                    m_DP12MSTProbe = new DP12MST(path);
                    m_IProbe = (IProbeMgrGen2)m_DP12MSTProbe;
                    break;
                case "SST-1.4":
                    if (m_DP14SSTProbe != null)
                        m_DP14SSTProbe = null;
                    m_DP14SSTProbe = new DP14SST(path);
                    m_IProbe = (IProbeMgrGen2)m_DP14SSTProbe;
                    break;
                case "MST-1.4":
                    if (m_DP14MSTProbe != null)
                        m_DP14MSTProbe = null;
                    m_DP14MSTProbe = new DP14MST(path);
                    m_IProbe = (IProbeMgrGen2)m_DP14MSTProbe;
                    break;
            }
            //    if (m_DP11aProbe != null)
            //        m_DP11aProbe = null;

            //    if (m_DP12SSTProbe != null)
            //        m_DP12SSTProbe = null;

            //if (m_DP12SSTProbe != null)
            //    m_DP12SSTProbe = null;
            //if (m_DP12MSTProbe != null)
            //    m_DP12MSTProbe = null;
            //if (m_DP13SSTProbe != null)
            //    m_DP13SSTProbe = null;
            //if (m_DP13MSTProbe != null)
            //    m_DP13MSTProbe = null;

            //switch (toolStripModeComboBox.SelectedIndex)
            //{
            //    case 0:
            //        m_DP11aProbe = new DP11ProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP11aProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //    case 1:
            //        m_DP12SSTProbe = new DP12SSTProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //    case 2:

            //m_DP12SSTProbe = new DP12SST();
            //m_IProbe = (IProbeMgrGen2)m_DP12SSTProbe;   // from now on, all calls to the trace data mgr goes through this class member (interface variable)




            //        break;
            //    case 3:
            //        m_DP13SSTProbe = new DP13SSTProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP13SSTProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //    case 4:
            //        m_DP13MSTProbe = new DP13MSTProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP13MSTProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;

            //    default:
            //        m_DP11aProbe = new DP11ProbeMgrGen2();
            //        m_IProbe = (IProbeMgrGen2)m_DP11aProbe;
            //        m_IProbe.OnLogMsgEvent += new LogMsgEvent(processLogMsgEvent);
            //        m_IProbe.OnTBUploadEvent += new TBUploadEvent(processTBUploadEvent);
            //        m_IProbe.OnProbeCommEvent += new ProbeCommEvent(processProbeCommEvent);
            //        break;
            //}

            return status;
        }


        /// <summary>
        /// The DefaultSetup that first appears and when the clear button is pushed.
        /// </summary>
        private void DefaultSetup()
        {
            Lane1button.Checked = true;
            DataFolderPath_TextBox.Text = m_defaultFolderPath;
            getstatesButton.Enabled = false;
        }

        /// <summary>
        /// Returns the protocal that is being used
        /// </summary>
        /// <returns></returns>
        private string getprotocol()
        {
            string protocal = "";
            if (DP1_2SSTbutton.Checked)
                protocal = "SST-1.2";
            else if (DP1_2MSTbutton.Checked)
                protocal = "MST-1.2";
            else if (DP1_4SSTbutton.Checked)
                protocal = "SST-1.4";
            else if (DP1_4MSTbutton.Checked)
                protocal = "MST-1.4";
            else
                protocal = "error";
            return protocal;
        }

        /// <summary>
        /// Return virtual channel
        /// </summary>
        /// <returns></returns>
        private int getvc()
        {
            int vc = 0;
            if (getprotocol() == "SST-1.4" || getprotocol() == "SST-1.2")
            {
                vc = 1;
            }
            else
            {
                if (VC1button.Checked)
                    vc = 1;
                else if (VC2button.Checked)
                    vc = 2;
                else if (VC3button.Checked)
                    vc = 3;
                else if (VC4button.Checked)
                    vc = 4;
            }
            return vc;
        }
        /// <summary>
        /// Get Event code startbit and width in tracebuffer using xml file.
        /// </summary>
        private void xmlreader()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string mypath = Path.Combine(path, "FuturePlus\\FS4500\\pixelrender.xml");

            XmlReader reader = XmlReader.Create(mypath);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == Protocol)
                {
                    if (reader.GetAttribute("Name") == "Event")
                    {
                        EventWidth = Convert.ToInt32(reader.GetAttribute("Width"));
                        string decoy = reader.GetAttribute("StartBit");
                        Eventsb = Convert.ToInt32(decoy);
                    }
                    if (reader.GetAttribute("Name") == "Trigger")
                    {
                        TriggerWidth = Convert.ToInt32(reader.GetAttribute("Width"));
                        Triggersb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane0")
                    {
                        Lane0sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane1")
                    {
                        Lane1sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane2")
                    {
                        Lane2sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                    if (reader.GetAttribute("Name") == "Lane3")
                    {
                        Lane3sb = Convert.ToInt32(reader.GetAttribute("StartBit"));
                    }
                }
            }
        }

        /// <summary>
        /// Returns a StringBuilding with the eventcode bits
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <returns></returns>
        private StringBuilder Geteventcode(byte[] dataBytes)
        {
            byte result = 0x00;
            if (dataBytes != null)
            {
                StringBuilder sb = new StringBuilder();
                byte bits = dataBytes[(15 - Eventsb / EventWidth)];
                byte bits2 = dataBytes[(15 - (Eventsb - 7) / EventWidth)];
                result = (byte)(bits << EventWidth - ((Eventsb % EventWidth) + 1) | (bits2 >> (Eventsb - 7) % EventWidth));
                sb.Append("0x" + result.ToString("X2"));
                return sb;
            }
            return null;
        }

        /// <summary>
        /// Extracting the lane data from an inputed lane with help from xml file
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <param name="startbit"></param>
        /// <returns></returns>
        private List<byte> Getlanedata(byte[] dataBytes)
        {
            List<byte> result = new List<Byte>();
            int startbit = 0;
            int i = 0;
            while (i != 4)
            {
                switch (i)
                {
                    case 0:
                        startbit = Lane0sb;
                        break;
                    case 1:
                        startbit = Lane1sb;
                        break;
                    case 2:
                        startbit = Lane2sb;
                        break;
                    case 3:
                        startbit = Lane3sb;
                        break;

                    default:
                        break;
                }
                StringBuilder sb = new StringBuilder();
                byte bits = dataBytes[15 - (startbit / 8)];
                byte bits2 = dataBytes[(15 - (startbit - 7) / 8)];
                result.Add((byte)(bits << 8 - ((startbit % 8) + 1) | (bits2 >> (startbit - 7) % 8)));
                i++;
            }
            return result;
        }

        private bool IsTrigger(byte[] dataBytes)
        {
            byte bits = dataBytes[(15 - (Triggersb / 8))];
            int bit = Triggersb % 8;
            byte mask = 0;
            if (bit == 0)
            {
                mask = 0x01;
            }
            else if (bit == 1)
            {
                mask = 0x02;
            }
            else if (bit == 2)
            {
                mask = 0x04;
            }
            else if (bit == 3)
            {
                mask = 0x08;
            }
            else if (bit == 4)
            {
                mask = 0x10;
            }
            else if (bit == 5)
            {
                mask = 0x20;
            }
            else if (bit == 6)
            {
                mask = 0x40;
            }
            else if (bit == 7)
            {
                mask = 0x80;
            }
            bits = (byte)(bits & mask);
            if (bits == 0x00)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Get statedata from chunk and return
        /// </summary>
        /// <param name="index"></param>
        /// <param name="statesChunk"></param>
        /// <param name="stateData"></param>
        /// <returns></returns>
        private byte[] getStateDataFromChunk(int index, List<byte> statesChunk, ref byte[] stateData)
        {

            for (int i = 0; i < stateData.Length; i++)
                stateData[i] = statesChunk[index + i];

            return stateData;
        }


        #endregion // Private Methods

        #region Public Methods

        #endregion // Public Methods

    }
    /// <summary>
    /// Metadata objectclass, public because other forms will use this class.
    /// </summary>
    public class Metadata
    {
        private int m_lanes = 0;
        public int Lanes { get { return m_lanes; } set { m_lanes = value; } }
        private int m_frameid = 0;
        public int Frameid { get { return m_frameid; } set { m_frameid = value; } }

        private int m_vc = 0;
        public int VirtualChannel { get { return m_vc; } set { m_vc = value; } }

        private string m_protocol = "";
        public string Protocol { get { return m_protocol; } set { m_protocol = value; } }
        private int m_startstate = 0;
        public int StartState { get { return m_startstate; } set { m_startstate = value; } }
        private int m_firstpixelstate = 0;
        public int FirstPixelState { get { return m_firstpixelstate; } set { m_firstpixelstate = value; } }
        private int m_endstate = 0;
        public int EndState { get { return m_endstate; } set { m_endstate = value; } }
        private string m_file = "";
        public string Pathfile { get { return m_file; } set { m_file = value; } }

        private int m_statesbeforetrig = 0;
        public int StatesBeforeTrig { get { return m_statesbeforetrig; } set { m_statesbeforetrig = value; } }

        private FrameVideoRendererCtrl.MSAData m_msadata;
        public FrameVideoRendererCtrl.MSAData MSAData { get { return m_msadata; } set { m_msadata = value; } }

        private bool m_HDCP = false;
        public bool HDCP { get { return m_HDCP; } set { m_HDCP = value; } }

        public Metadata(int frameid, int start, int firstpixel, int end, int lanes, string protocol, int vc, string file, int statesbeforetrig, FrameVideoRendererCtrl.MSAData msadata, bool hdcp)
        {
            Frameid = frameid;
            StartState = start;
            FirstPixelState = firstpixel;
            EndState = end;
            Lanes = lanes;
            Protocol = protocol;
            VirtualChannel = vc;
            Pathfile = file;
            StatesBeforeTrig = statesbeforetrig;
            MSAData = msadata;
            HDCP = hdcp;
        }
    }
}
