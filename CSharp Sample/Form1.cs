using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Atalasoft.EZTwain;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;

namespace EZTwain_CSharp_Sample
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>

	public class Form1 : System.Windows.Forms.Form
	{
		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.PictureBox PicBox;
		private System.Windows.Forms.Button Acquire;

		private System.IntPtr[] m_dibs;
        private int m_ipage;        // page index
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Label EzInfo;
		private System.Windows.Forms.Button OpenFile;
		private System.Windows.Forms.Button Left90;
		private System.Windows.Forms.Button Right90;
        private System.Windows.Forms.Button EnumDevs;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private Button Clear;
        private Button Rotate180;
        private Button FindBarcodes;
        private Button MultipageNoUI;
        private Button OCRtest;
        private Label pageinfo;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
             m_dibs = new System.IntPtr[0];
             m_ipage = -1;

			EZTwain.LogFile(1);
            EZTwain.SetAppTitle("C# Sample App");
			EzInfo.Text = String.Format("Using EZTwain Version {0} {1}",
				EZTwain.EasyVersion() / 100.0,
				EZTwain.BuildName());
		}

        private void UpdatePageInfo()
        {
            String s = String.Format(
                "page {0}/{1} ",
                m_ipage + 1, m_dibs.Length
            );
            if (m_ipage >= 0)
            {
                IntPtr dib = m_dibs[m_ipage];
                s += String.Format(" {0}-bit {1}x{2}",
                    EZTwain.DIB_Depth(dib),
                    EZTwain.DIB_Width(dib),
                    EZTwain.DIB_Height(dib)
                );
            }
            pageinfo.Text = s;
        }

        private void ClearImages()
        {
            for (int i = 0; i < m_dibs.Length; i++)
            {
                EZTwain.DIB_Free(m_dibs[i]);
            }
            m_ipage = -1;
            m_dibs = new IntPtr[0];
            PicBox.Image = null;
            PicBox.Update();
        }

        private void RepaintImage()
        {
            UpdatePageInfo();
            if (m_ipage >= 0)
            {
                // reduce the image to fit in the picture box:
                IntPtr hview = EZTwain.DIB_Thumbnail(m_dibs[m_ipage], PicBox.Width, PicBox.Height);
                // convert this to image form and assign to picture box:
                PicBox.Image = EZTwain.DIB_ToImage(hview);
                // free the scaled temporary DIB:
                EZTwain.DIB_Free(hview);
            }
            else
            {
                PicBox.Image = null;
            }
            PicBox.Update();
        } // RepaintImage

		private void SetImage(IntPtr hdib)
		{
            if (m_dibs.Length == 0)
            {
                m_dibs = new IntPtr[1];
                m_dibs[0] = IntPtr.Zero;
                m_ipage = 0;
            }
			if (m_dibs[m_ipage] != IntPtr.Zero) {
                EZTwain.DIB_Free(m_dibs[m_ipage]);
                m_dibs[m_ipage] = IntPtr.Zero;
				PicBox.Image = null;
			}
            m_dibs[m_ipage] = hdib;
            RepaintImage();
		}

        private void SetImages(IntPtr[] dibs, int n)
        {
            ClearImages();
            m_dibs = new IntPtr[n];
            for (int i = 0; i < m_dibs.Length; i++)
            {
                m_dibs[i] = dibs[i];
            }
            if (n > 0)
            {
                m_ipage = 0;
            }
            RepaintImage();
        }

        private void DeleteImage(int i)
        {   // delete the ith image
            if (i >= 0 && i < m_dibs.Length)
            {   // really dopey way to do this, but...
                IntPtr[] dibs = new IntPtr[m_dibs.Length - 1];
                EZTwain.DIB_Free(m_dibs[i]);
                for (int j = 0; j < dibs.Length; j++)
                {
                    dibs[j] = m_dibs[j < i ? j : j + 1];
                }
                m_dibs = dibs;
                if (m_ipage > i || m_ipage >= m_dibs.Length)
                {
                    m_ipage--;
                }
                RepaintImage();
            }
        }

        private void AppendImage(IntPtr dib)
        {
            IntPtr[] dibs = new IntPtr[m_dibs.Length + 1];
            for (int i = 0; i < m_dibs.Length; i++)
            {
                dibs[i] = m_dibs[i];
            }
            dibs[dibs.Length - 1] = dib;
            m_dibs = dibs;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
                ClearImages();
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
            this.button1 = new System.Windows.Forms.Button();
            this.PicBox = new System.Windows.Forms.PictureBox();
            this.Acquire = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.OpenFile = new System.Windows.Forms.Button();
            this.EzInfo = new System.Windows.Forms.Label();
            this.Left90 = new System.Windows.Forms.Button();
            this.Right90 = new System.Windows.Forms.Button();
            this.EnumDevs = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.Rotate180 = new System.Windows.Forms.Button();
            this.FindBarcodes = new System.Windows.Forms.Button();
            this.MultipageNoUI = new System.Windows.Forms.Button();
            this.OCRtest = new System.Windows.Forms.Button();
            this.pageinfo = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.PicBox)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 40);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(134, 24);
            this.button1.TabIndex = 0;
            this.button1.Text = "Select Source...";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // PicBox
            // 
            this.PicBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PicBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PicBox.Location = new System.Drawing.Point(172, 31);
            this.PicBox.Name = "PicBox";
            this.PicBox.Size = new System.Drawing.Size(506, 489);
            this.PicBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.PicBox.TabIndex = 1;
            this.PicBox.TabStop = false;
            // 
            // Acquire
            // 
            this.Acquire.Location = new System.Drawing.Point(13, 100);
            this.Acquire.Name = "Acquire";
            this.Acquire.Size = new System.Drawing.Size(134, 24);
            this.Acquire.TabIndex = 0;
            this.Acquire.Text = "Acquire...";
            this.Acquire.Click += new System.EventHandler(this.Acquire_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(13, 232);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(134, 24);
            this.Save.TabIndex = 2;
            this.Save.Text = "Save to File...";
            this.Save.Click += new System.EventHandler(this.Save_Click);
            // 
            // OpenFile
            // 
            this.OpenFile.Location = new System.Drawing.Point(13, 202);
            this.OpenFile.Name = "OpenFile";
            this.OpenFile.Size = new System.Drawing.Size(134, 24);
            this.OpenFile.TabIndex = 3;
            this.OpenFile.Text = "Load File...";
            this.OpenFile.Click += new System.EventHandler(this.Load_Click);
            // 
            // EzInfo
            // 
            this.EzInfo.Location = new System.Drawing.Point(13, 7);
            this.EzInfo.Name = "EzInfo";
            this.EzInfo.Size = new System.Drawing.Size(320, 21);
            this.EzInfo.TabIndex = 4;
            this.EzInfo.Text = "initializing...";
            // 
            // Left90
            // 
            this.Left90.Location = new System.Drawing.Point(13, 432);
            this.Left90.Name = "Left90";
            this.Left90.Size = new System.Drawing.Size(62, 24);
            this.Left90.TabIndex = 5;
            this.Left90.Text = "Left 90°";
            this.Left90.Click += new System.EventHandler(this.Left90_Click);
            // 
            // Right90
            // 
            this.Right90.Location = new System.Drawing.Point(85, 432);
            this.Right90.Name = "Right90";
            this.Right90.Size = new System.Drawing.Size(62, 24);
            this.Right90.TabIndex = 6;
            this.Right90.Text = "Right 90°";
            this.Right90.Click += new System.EventHandler(this.Right90_Click);
            // 
            // EnumDevs
            // 
            this.EnumDevs.Location = new System.Drawing.Point(13, 70);
            this.EnumDevs.Name = "EnumDevs";
            this.EnumDevs.Size = new System.Drawing.Size(134, 24);
            this.EnumDevs.TabIndex = 7;
            this.EnumDevs.Text = "Enumerate Sources";
            this.EnumDevs.Click += new System.EventHandler(this.EnumDevs_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(13, 331);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(134, 24);
            this.button2.TabIndex = 9;
            this.button2.Text = "DIB->Image->DIB";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(14, 361);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(134, 24);
            this.button3.TabIndex = 11;
            this.button3.Text = "array param test";
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(13, 262);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(134, 24);
            this.Clear.TabIndex = 2;
            this.Clear.Text = "Clear";
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // Rotate180
            // 
            this.Rotate180.Location = new System.Drawing.Point(13, 462);
            this.Rotate180.Name = "Rotate180";
            this.Rotate180.Size = new System.Drawing.Size(134, 24);
            this.Rotate180.TabIndex = 12;
            this.Rotate180.Text = "Rotate 180°";
            this.Rotate180.UseVisualStyleBackColor = true;
            this.Rotate180.Click += new System.EventHandler(this.Rotate180_Click);
            // 
            // FindBarcodes
            // 
            this.FindBarcodes.Location = new System.Drawing.Point(13, 494);
            this.FindBarcodes.Name = "FindBarcodes";
            this.FindBarcodes.Size = new System.Drawing.Size(134, 24);
            this.FindBarcodes.TabIndex = 13;
            this.FindBarcodes.Text = "Find Barcodes";
            this.FindBarcodes.UseVisualStyleBackColor = true;
            this.FindBarcodes.Click += new System.EventHandler(this.FindBarcodes_Click);
            // 
            // MultipageNoUI
            // 
            this.MultipageNoUI.Location = new System.Drawing.Point(13, 130);
            this.MultipageNoUI.Name = "MultipageNoUI";
            this.MultipageNoUI.Size = new System.Drawing.Size(134, 24);
            this.MultipageNoUI.TabIndex = 14;
            this.MultipageNoUI.Text = "Multipage Scan (No UI)";
            this.MultipageNoUI.UseVisualStyleBackColor = true;
            this.MultipageNoUI.Click += new System.EventHandler(this.MultipageNoUI_Click);
            // 
            // OCRtest
            // 
            this.OCRtest.Location = new System.Drawing.Point(14, 301);
            this.OCRtest.Name = "OCRtest";
            this.OCRtest.Size = new System.Drawing.Size(134, 24);
            this.OCRtest.TabIndex = 15;
            this.OCRtest.Text = "OCR Test";
            this.OCRtest.UseVisualStyleBackColor = true;
            this.OCRtest.Click += new System.EventHandler(this.OCRtest_Click);
            // 
            // pageinfo
            // 
            this.pageinfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pageinfo.Location = new System.Drawing.Point(172, 525);
            this.pageinfo.Name = "pageinfo";
            this.pageinfo.Size = new System.Drawing.Size(427, 21);
            this.pageinfo.TabIndex = 16;
            this.pageinfo.Text = "...initializing...";
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(172, 547);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(42, 23);
            this.button4.TabIndex = 17;
            this.button4.Text = "First";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.Location = new System.Drawing.Point(220, 547);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(42, 23);
            this.button5.TabIndex = 17;
            this.button5.Text = "Prev";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button6.Location = new System.Drawing.Point(268, 547);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(42, 23);
            this.button6.TabIndex = 17;
            this.button6.Text = "Next";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button7.Location = new System.Drawing.Point(316, 547);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(42, 23);
            this.button7.TabIndex = 17;
            this.button7.Text = "Last";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // button8
            // 
            this.button8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button8.Location = new System.Drawing.Point(364, 547);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(42, 23);
            this.button8.TabIndex = 17;
            this.button8.Text = "Del";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(689, 574);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.pageinfo);
            this.Controls.Add(this.OCRtest);
            this.Controls.Add(this.MultipageNoUI);
            this.Controls.Add(this.FindBarcodes);
            this.Controls.Add(this.Rotate180);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.EnumDevs);
            this.Controls.Add(this.Right90);
            this.Controls.Add(this.Left90);
            this.Controls.Add(this.EzInfo);
            this.Controls.Add(this.OpenFile);
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.PicBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Acquire);
            this.Name = "Form1";
            this.Text = "EZTwain Pro from C# ";
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.PicBox)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			EZTwain.SelectImageSource(this.Handle);
		}

		private void Acquire_Click(object sender, System.EventArgs e)
		{
            EZTwain.SetMultiTransfer(true);
            ClearImages();
            while (true)
            {
                IntPtr hdib = EZTwain.Acquire(this.Handle);
                if (hdib == IntPtr.Zero)
                {
                    break;
                }
                AppendImage(hdib);
                m_ipage = m_dibs.Length - 1;
                RepaintImage();
                if (EZTwain.State() < 5)
                {
                    break;
                }
            }
            m_ipage = 0;
            if (m_dibs.Length == 0)
            {
                m_ipage = -1;
            }
            RepaintImage();
            EZTwain.ReportLastError("Scanning");
            EZTwain.CloseSource();
            EZTwain.SetMultiTransfer(false);
		}

        private void MultipageNoUI_Click(object sender, EventArgs e)
        {
            System.IntPtr hdib;
            EZTwain.SetHideUI(true);
            if (EZTwain.OpenDefaultSource())
            {
                ClearImages();
                EZTwain.SelectFeeder(true);
                EZTwain.SetPixelType(0);
                EZTwain.SetBitDepth(1);
                EZTwain.SetResolution(200);
                EZTwain.SetAutoDeskew(1);
                EZTwain.SetXferCount(-1);
                EZTwain.SetAutoScan(true);
                EZTwain.SetMultiTransfer(true);
                do
                {
                    // If you can't get a Window handle, use IntPtr.Zero:
                    hdib = EZTwain.Acquire(this.Handle);
                    if (hdib == IntPtr.Zero)
                    {
                        break;
                    }
                    AppendImage(hdib);
                    m_ipage = m_dibs.Length - 1;
                    RepaintImage();
                } while (!EZTwain.IsDone());
                EZTwain.CloseSource();
                m_ipage = 0;
                if (m_dibs.Length == 0)
                {
                    m_ipage = -1;
                }
            }
            RepaintImage();
            EZTwain.ReportLastError("Unable to scan.");
            EZTwain.SetMultiTransfer(false);
        }

		private void Load_Click(object sender, System.EventArgs e)
		{
            IntPtr[] dibs = new IntPtr[1000];
            int n = EZTwain.DIB_LoadArrayFromFilename(dibs, 1000, "");
            if (n > 0)
            {
                // got it
                SetImages(dibs, n);
            }
            EZTwain.ReportLastError("Reading file");
		}

		private void Save_Click(object sender, System.EventArgs e)
		{
			if (m_dibs.Length > 0) 
			{
                EZTwain.DIB_WriteArrayToFilename(m_dibs, m_dibs.Length, "");
                EZTwain.ReportLastError("Saving to file");
			}
		}

		private void Left90_Click(object sender, System.EventArgs e)
		{
            if (m_ipage >= 0) {
				SetImage(EZTwain.DIB_Rotate90(m_dibs[m_ipage], -1));
			}
		}

		private void Right90_Click(object sender, System.EventArgs e)
		{
			if (m_ipage >= 0) 
			{
                SetImage(EZTwain.DIB_Rotate90(m_dibs[m_ipage], 1));
			}
		}

		private void EnumDevs_Click(object sender, System.EventArgs e)
		{
			if (EZTwain.GetSourceList()) 
			{
				string Name = EZTwain.NextSourceName();
				while (Name != "") 
				{
					MessageBox.Show(Name);
					Name = EZTwain.NextSourceName();
				}
			} 
			else 
			{
				MessageBox.Show("No TWAIN devices found.");
			}
			MessageBox.Show("Default Source: "+EZTwain.DefaultSourceName());
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
            double[] whitepoint = new double[3];
            EZTwain.SetTiffTagRationalArray(37829, whitepoint, 3);
            int[] histo = new int[256];
            EZTwain.DIB_GetHistogram(m_dibs[m_ipage], 0, histo);
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            SetImages(new IntPtr[0], 0);
            RepaintImage();
        }

        private void Rotate180_Click(object sender, EventArgs e)
        {
            if (m_ipage >= 0)
            {
                EZTwain.DIB_Rotate180(m_dibs[m_ipage]);
                RepaintImage();
            }
        }

        private void FindBarcodes_Click(object sender, EventArgs e)
        {
            int bars = EZTwain.BARCODE_Recognize(m_dibs[m_ipage], -1, -1);
            if (bars == 0)
            {
                MessageBox.Show("No barcodes detected.");
            }
            else
            {
                String s = String.Format("Barcodes detected: {0}\n", bars);
                for (int i = 0; i < bars; i++)
                {
                    StringBuilder buf = new StringBuilder();
                    buf.Length = 1024;
                    EZTwain.BARCODE_GetText(i, buf);
                    s += String.Format("[{0}] Type={1} Text={2}\n",
                        i,
                        EZTwain.BARCODE_TypeName(EZTwain.BARCODE_Type(i)),
                        buf.ToString());
                }
                MessageBox.Show(s);
            }
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            RepaintImage();
        }

        private void OCRtest_Click(object sender, EventArgs e)
        {
            EZTwain.OCR_ClearText();
            int chars = EZTwain.OCR_RecognizeDib(m_dibs[m_ipage]);
            if (chars >= 0)
            {
                string text = EZTwain.OCR_Text();
                int[] charx = new int[chars];
                int[] chary = new int[chars];
                int[] charw = new int[chars];
                int[] charh = new int[chars];
                EZTwain.OCR_GetCharPositions(charx, chary);
                EZTwain.OCR_GetCharSizes(charw, charh);
                MessageBox.Show(text, "Text returned by OCR");
            }
            else
            {
                EZTwain.ReportLastError("OCR");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (m_dibs.Length > 0)
            {
                m_ipage = 0;
                RepaintImage();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (m_ipage > 0)
            {
                m_ipage--;
                RepaintImage();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (m_ipage+1 < m_dibs.Length)
            {
                m_ipage++;
                RepaintImage();
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            m_ipage = m_dibs.Length - 1;
            RepaintImage();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (m_dibs.Length > 0)
            {
                DeleteImage(m_ipage);
                RepaintImage();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IntPtr hdib = EZTwain.DIB_FromImage(EZTwain.DIB_ToImage(m_dibs[m_ipage]));
            SetImage(hdib);
        }
	}
}
