using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using EZTwain_CSharp_Sample;
using Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace winforms_templates
{
    public partial class frmMain : _Form
    {
        private System.IntPtr[] m_dibs = new System.IntPtr[0];
        private int m_ipage = -1; //page index

        public frmMain()
        {
            InitializeComponent();
            dtpCreateDate.EditValue = DateTime.Now;
            LoadData();
        }

        #region Load Data

        public void LoadData()
        {
            LoadMachineSouce();
            cboOutput.SelectedIndex = 0;
            cboScanType.SelectedIndex = 0;
            LoadDataComboBox(cboReportTypes, "ReportTypes");
            LoadDataComboBox(cboFactory, "Factories");
            LoadDataComboBox(cboGroups, "Groups");
            cboReportTypes.SelectedIndex = cboFactory.SelectedIndex = cboGroups.SelectedIndex = 0;
            LoadDataComboBox(cboLvl1, "Level1");
            LoadDataComboBox(cboLvl2, "Level2");
            LoadDataComboBox(cboLvl3, "Level3");
            LoadDrives();
        }

        public void LoadDataComboBox(ComboBoxEdit ctrl, string fileName)
        {
            List<string> lines = ReadFileLines(fileName);
            ctrl.Properties?.Items.Clear();
            ctrl.Properties?.Items.AddRange(lines);
        }

        public void LoadMachineSouce()
        {
            var devices = GetAvailableTWAINDevices();
            devices.Add("Test");
            cboSource.Properties?.Items.Clear();
            cboSource.Properties?.Items.AddRange(devices);
        }

        public void LoadDrives()
        {
            List<string> drives = new List<string>();
            // Get local drives
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    drives.Add(drive.Name);
                }
            }
            // Get network drives
            foreach (var drive in Environment.GetLogicalDrives())
            {
                DriveInfo di = new DriveInfo(drive);
                if (di.DriveType == DriveType.Network)
                {
                    drives.Add(di.Name);
                }
            }
            if (drives.Contains("D:\\"))
            {
                drives.Remove("D:\\");
                drives.Insert(0, "D:\\");
            }

            cboDrives.Properties.Items.Clear();
            cboDrives.Properties.Items.AddRange(drives);
            if (drives.Count > 0) cboDrives.SelectedIndex = 0;
        }

        #endregion Load Data

        #region Process Data (Click events, ...)

        private string getOutputPath()
        {
            string drive = cboDrives.Text.Replace("\\", "");
            string createMonth = (Convert.ToDateTime(dtpCreateDate.EditValue).ToString("yyyy-MM"));
            string reportType = cboReportTypes.Text;
            string factory = cboFactory.Text;
            string group = cboGroups.Text;
            string lvl1 = cboLvl1.Text;
            string lvl2 = cboLvl2.Text;
            string lvl3 = cboLvl3.Text;
            string createDate = (Convert.ToDateTime(dtpCreateDate.EditValue).ToString("yyyy-MM-dd"));
            return $"{drive}\\{createMonth}\\{reportType}\\{factory}\\{group}" +
                (lvl1 == "" ? "" : "\\" + lvl1) +
                (lvl2 == "" ? "" : "\\" + lvl2) +
                (lvl3 == "" ? "" : "\\" + lvl3) +
                $"\\{createDate}";
        }

        private void btnReloadSource_Click(object sender, EventArgs e)
        {
            LoadMachineSouce();
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            string deviceName = cboSource.Text;
            if (cboScanType.Text.ToLower() == "single" && !String.IsNullOrWhiteSpace(deviceName))
            {
                ScanSingle(deviceName);
            }
            else
            {
                ScanMultiple(deviceName);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string output = getOutputPath();
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }

            string createDate = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string fileType = cboOutput.Text.ToLower();
            string documentCode = txtDocumentCode.Text;

            bool saveAsSingleFile = cboScanType.SelectedIndex != 2;

            try
            {
                if (m_dibs.Length > 0)
                {
                    if (saveAsSingleFile)
                    {
                        // Save as a single file
                        string fileName = $"{documentCode}_{createDate}.{fileType}";
                        string fullPath = Path.Combine(output, fileName);
                        EZTwain.DIB_WriteArrayToFilename(m_dibs, m_dibs.Length, fullPath);
                    }
                    else
                    {
                        // Save each image separately
                        for (int i = 0; i < m_dibs.Length; i++)
                        {
                            string fileName = $"{documentCode}_{createDate}_{i + 1}.{fileType}";
                            string fullPath = Path.Combine(output, fileName);
                            EZTwain.DIB_WriteToFilename(m_dibs[i], fullPath);
                        }
                    }

                    EZTwain.ReportLastError("Saving to file");

                    // Open the folder
                    Process.Start(output);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion Process Data (Click events, ...)

        #region CRUD setting files

        private static List<string> ReadFileLines(string fileName)
        {
            string filePath = Path.Combine("config", fileName + ".txt");
            List<string> lines = new List<string>();

            if (File.Exists(filePath))
            {
                lines = new List<string>(File.ReadAllLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)));
            }

            return lines;
        }

        private static bool AddLineToFile(string fileName, string lineToAdd)
        {
            string filePath = Path.Combine("config", fileName + ".txt");
            if (String.IsNullOrWhiteSpace(lineToAdd) || !File.Exists(filePath)) return false;

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(lineToAdd);
                return true;
            }
        }

        private static bool RemoveLineFromFile(string fileName, string lineToRemove)
        {
            string filePath = Path.Combine("config", fileName + ".txt");
            if (String.IsNullOrWhiteSpace(lineToRemove) || !File.Exists(filePath)) return false;
            List<string> lines = File.ReadAllLines(filePath).ToList();
            lines.RemoveAll(line => line.Contains(lineToRemove));
            File.WriteAllLines(filePath, lines);
            return true;
        }

        #endregion CRUD setting files

        #region EZTwain related stuffs

        private List<string> GetAvailableTWAINDevices()
        {
            StringBuilder buffer = new StringBuilder();
            List<string> devices = new List<string>();
            if (EZTwain.GetSourceList())
            {
                buffer.EnsureCapacity(64);
                while (EZTwain.GetNextSourceName(buffer))
                {
                    devices.Add(buffer.ToString());
                    buffer.EnsureCapacity(64);
                }
            }
            return devices;
        }

        private void UpdatePageInfo()
        {
            String pageInfo = String.Format(
                "{0}/{1} ",
                m_ipage + 1, m_dibs.Length
            );
            if (m_ipage >= 0)
            {
                IntPtr dib = m_dibs[m_ipage];
                pageInfo += String.Format(" {0}-bit {1}x{2}",
                    EZTwain.DIB_Depth(dib),
                    EZTwain.DIB_Width(dib),
                    EZTwain.DIB_Height(dib)
                );
            }
            lblPage.Text = "Trang: " + pageInfo;
        }

        private void ClearImages()
        {
            for (int i = 0; i < m_dibs.Length; i++)
            {
                EZTwain.DIB_Free(m_dibs[i]);
            }
            m_ipage = -1;
            m_dibs = new IntPtr[0];
            picPreview.Image = null;
            picPreview.Update();
        }

        private void RepaintImage()
        {
            UpdatePageInfo();
            if (m_ipage >= 0)
            {
                // reduce the image to fit in the picture box:
                IntPtr hview = EZTwain.DIB_Thumbnail(m_dibs[m_ipage], picPreview.Width, picPreview.Height);
                // convert this to image form and assign to picture box:
                picPreview.Image = EZTwain.DIB_ToImage(hview);
                // free the scaled temporary DIB:
                EZTwain.DIB_Free(hview);
            }
            else
            {
                picPreview.Image = null;
            }
            picPreview.Update();
        } // RepaintImage

        private void SetImage(IntPtr hdib)
        {
            if (m_dibs.Length == 0)
            {
                m_dibs = new IntPtr[1];
                m_dibs[0] = IntPtr.Zero;
                m_ipage = 0;
            }
            if (m_dibs[m_ipage] != IntPtr.Zero)
            {
                EZTwain.DIB_Free(m_dibs[m_ipage]);
                m_dibs[m_ipage] = IntPtr.Zero;
                picPreview.Image = null;
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

        private void AddImages(IntPtr[] dibs, int n)
        {
            int currentLength = m_dibs.Length;
            Array.Resize(ref m_dibs, currentLength + n);

            for (int i = 0; i < n; i++)
            {
                m_dibs[currentLength + i] = dibs[i];
            }

            m_ipage = m_dibs.Length > 0 ? currentLength : -1;
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

        #endregion EZTwain related stuffs

        #region Get prompt messages (and related stuffs)

        private string GetComboBoxText(string tag)
        {
            switch (tag)
            {
                case "ReportTypes": return cboReportTypes.Text;
                case "Factories": return cboFactory.Text;
                case "Groups": return cboGroups.Text;
                case "Lvl1": return cboLvl1.Text;
                case "Lvl2": return cboLvl2.Text;
                case "Lvl3": return cboLvl3.Text;
                default: return string.Empty;
            }
        }

        private string GetRemovePromptMessage(string tag, out string txt)
        {
            txt = GetComboBoxText(tag);

            switch (tag)
            {
                case "ReportTypes": return $"Xoá báo cáo \"{txt}\"?";
                case "Factories": return $"Xoá nhà máy \"{txt}\"?";
                case "Groups": return $"Xoá nhóm \"{txt}\"?";
                case "Lvl1": return $"Xoá mức 1 \"{txt}\"?";
                case "Lvl2": return $"Xoá mức 2 \"{txt}\"?";
                case "Lvl3": return $"Xoá mức 3 \"{txt}\"?";
                default: return $"Xoá \"{txt}\"?";
            }
        }

        private string GetPromptMessage(string tag)
        {
            switch (tag)
            {
                case "ReportTypes": return "Tên báo cáo:";
                case "Factories": return "Tên nhà máy:";
                case "Groups": return "Tên nhóm:";
                case "Lvl1": return "Tên mức 1:";
                case "Lvl2": return "Tên mức 2:";
                case "Lvl3": return "Tên mức 3:";
                default: return "Nhập thông tin:";
            }
        }

        private string GetInputTitle(string tag)
        {
            switch (tag)
            {
                case "ReportTypes": return "loại báo cáo";
                case "Factories": return "nhà máy";
                case "Groups": return "nhóm";
                case "Lvl1": return "mức 1";
                case "Lvl2": return "mức 2";
                case "Lvl3": return "mức 3";
                default: return "thông tin";
            }
        }

        private ComboBoxEdit GetComboBox(string tag)
        {
            switch (tag)
            {
                case "ReportTypes": return cboReportTypes;
                case "Factories": return cboFactory;
                case "Groups": return cboGroups;
                case "Lvl1": return cboLvl1;
                case "Lvl2": return cboLvl2;
                case "Lvl3": return cboLvl3;
                default: return null;
            }
        }

        #endregion Get prompt messages (and related stuffs)

        private void btnTest_Click(object sender, EventArgs e)
        {
            var frm = new Form1();
            frm.ShowDialog();
        }

        private void btn90Left_Click(object sender, EventArgs e)
        {
            if (m_ipage >= 0)
            {
                SetImage(EZTwain.DIB_Rotate90(m_dibs[m_ipage], -1));
            }
        }

        private void btn90Right_Click(object sender, EventArgs e)
        {
            if (m_ipage >= 0)
            {
                SetImage(EZTwain.DIB_Rotate90(m_dibs[m_ipage], 1));
            }
        }

        private void btn180_Click(object sender, EventArgs e)
        {
            if (m_ipage >= 0)
            {
                EZTwain.DIB_Rotate180(m_dibs[m_ipage]);
                RepaintImage();
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            if (m_dibs.Length > 0)
            {
                m_ipage = 0;
                RepaintImage();
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (m_ipage > 0)
            {
                m_ipage--;
                RepaintImage();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (m_ipage + 1 < m_dibs.Length)
            {
                m_ipage++;
                RepaintImage();
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            m_ipage = m_dibs.Length - 1;
            RepaintImage();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Xóa ảnh này?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;
            if (m_dibs.Length > 0)
            {
                DeleteImage(m_ipage);
                RepaintImage();
            }
        }

        private void ScanSingle(string deviceName)
        {
            if (EZTwain.OpenSource(deviceName))
            {
                EZTwain.SetMultiTransfer(false);
                ClearImages();

                IntPtr hdib = EZTwain.Acquire(this.Handle);
                if (hdib != IntPtr.Zero)
                {
                    AppendImage(hdib);
                    m_ipage = 0;
                    RepaintImage();
                }
                else
                {
                    m_ipage = -1;
                }

                RepaintImage();
                EZTwain.ReportLastError("Scanning");
                EZTwain.CloseSource();
            }
            else
            {
                MessageBox.Show("Kết nối thất bại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScanMultiple(string deviceName)
        {
            if (EZTwain.OpenSource(deviceName))
            {
                System.IntPtr hdib;
                EZTwain.SetHideUI(true);
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
                    hdib = EZTwain.Acquire(this.Handle);
                    if (hdib == IntPtr.Zero) break;

                    AppendImage(hdib);
                    m_ipage = m_dibs.Length - 1;
                    RepaintImage();
                } while (!EZTwain.IsDone());
                EZTwain.CloseSource();
                m_ipage = m_dibs.Length > 0 ? 0 : -1;
                RepaintImage();
                EZTwain.ReportLastError("Unable to scan.");
                EZTwain.SetMultiTransfer(false);
            }
            else
            {
                MessageBox.Show("Kết nối thất bại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpload_Click(object sender, System.EventArgs e)
        {
            IntPtr[] dibs = new IntPtr[1000];
            int n = EZTwain.DIB_LoadArrayFromFilename(dibs, 1000, "");
            if (n <= 0) return;
            if (cboScanType.Text.ToLower() == "single")
                SetImages(dibs, n);
            else
                AddImages(dibs, n);
            EZTwain.ReportLastError("Reading file");
        }

        private void cboSource_EditValueChanged(object sender, EventArgs e)
        {
            string deviceName = cboSource.Text;
            if (EZTwain.OpenSource(deviceName))
                lblInfo.Text = $"Đã kết nối {deviceName}";
            else
                lblInfo.Text = $"Kết nối tới {deviceName} thất bại";
        }

        private void btnDefaultSize_Click(object sender, EventArgs e)
        {
            picPreview.Properties.ZoomPercent = 100;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var clickedButton = sender as SimpleButton;
            if (clickedButton == null) return;

            var tag = clickedButton.Tag.ToString();
            string promptMessage = GetPromptMessage(tag);
            string inputTitle = $"Thêm {GetInputTitle(tag)}";
            ComboBoxEdit comboBox = GetComboBox(tag);

            var result = XtraInputBox.Show(promptMessage, inputTitle, "");
            if (!string.IsNullOrEmpty(result))
            {
                AddLineToFile(tag, result);
                LoadDataComboBox(comboBox, tag);
                comboBox.SelectedIndex = 0;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var clickedButton = sender as SimpleButton;
            if (clickedButton == null) return;

            var tag = clickedButton.Tag.ToString();
            string promptMessage = GetRemovePromptMessage(tag, out string txt);
            ComboBoxEdit comboBox = GetComboBox(tag);

            var result = MessageBox.Show(promptMessage, "Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                RemoveLineFromFile(tag, txt);
                LoadDataComboBox(comboBox, tag);
                comboBox.SelectedIndex = 0;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string driveName = cboDrives.Text;
            string fileNameStart = txtDocumentCodeSearch.Text;
            if (String.IsNullOrWhiteSpace(fileNameStart)) return;
            var result = SearchFiles(driveName, fileNameStart);
            grdData.DataSource = result;
        }

        private void txtDocumentCodeSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnSearch_Click(null, null);
        }

        public DataTable SearchFiles(string driveName, string fileNameStart)
        {
            DataTable fileTable = new DataTable();
            fileTable.Columns.Add("STT", typeof(int));
            fileTable.Columns.Add("FileName", typeof(string));
            fileTable.Columns.Add("CreatedDate", typeof(DateTime));
            fileTable.Columns.Add("Size", typeof(string));
            fileTable.Columns.Add("AbsolutePath", typeof(string));

            // List of file extensions to search for
            List<string> fileExtensions = ReadFileLines("FileSearch");
            int order = 1;

            try
            {
                var directories = Directory.EnumerateDirectories(driveName, "*", SearchOption.TopDirectoryOnly);
                Parallel.ForEach(directories, directory =>
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    if ((dirInfo.Attributes & FileAttributes.Hidden) == 0 && (dirInfo.Attributes & FileAttributes.System) == 0)
                    {
                        try
                        {
                            foreach (string extension in fileExtensions)
                            {
                                foreach (string file in Directory.EnumerateFiles(directory, $"{fileNameStart}{extension}", SearchOption.AllDirectories))
                                {
                                    FileInfo fileInfo = new FileInfo(file);
                                    string fileSize = Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2) == 0
                                        ? "≈ 0MB"
                                        : $"{Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2)}MB";

                                    lock (fileTable) // Lock the DataTable to prevent race conditions
                                    {
                                        fileTable.Rows.Add(order++, fileInfo.Name, fileInfo.CreationTime, fileSize, fileInfo.FullName);
                                    }
                                }
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            lock (fileTable)
                            {
                                MessageBox.Show($"Không có quyền truy cập vào {directory}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            lock (fileTable)
                            {
                                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return fileTable;
        }

        private void grvData_DoubleClick(object sender, EventArgs e)
        {
            GridView view = sender as GridView;
            GridHitInfo hitInfo = view.CalcHitInfo(view.GridControl.PointToClient(Control.MousePosition));

            if (hitInfo.InRow)
            {
                int rowHandle = hitInfo.RowHandle;
                string path = view.GetRowCellValue(rowHandle, "AbsolutePath").ToString();
                Process.Start(path);
            }
        }
    }
}