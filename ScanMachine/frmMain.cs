using Atalasoft.EZTwain;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using winforms_templates.Models;

namespace winforms_templates
{
    public partial class frmMain : DevExpress.XtraEditors.XtraForm
    {
        private System.IntPtr[] m_dibs = new System.IntPtr[0]; // DIB is device-independent bitmap
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
            LoadDataComboBox<ReportTypes>(cboReportTypes);
            LoadDataComboBox<Factories>(cboFactories);
            cboReportTypes.ItemIndex = cboFactories.ItemIndex = 0;
            LoadDrives();
        }

        public void LoadDataComboBox<T>(LookUpEdit ctrl, int? foreignKeyID = null) where T : new()
        {
            var sqlHelper = new SqliteHelper<T>();
            IEnumerable<T> result;

            if (foreignKeyID.HasValue)
            {
                var foreignKeyProperty = typeof(T).GetProperties()
                    .FirstOrDefault(p => p.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase) && p.Name != "ID");

                if (foreignKeyProperty == null)
                {
                    throw new Exception($"Foreign key property not found in type {typeof(T).Name}");
                }

                result = sqlHelper.GetByColumnValue(foreignKeyProperty.Name, foreignKeyID.Value);
            }
            else
            {
                result = sqlHelper.GetAll();
            }

            ctrl.Properties.DataSource = result;
            ctrl.Properties.ValueMember = "ID";
            ctrl.Properties.DisplayMember = "Name";
            ctrl.EditValue = null;
        }

        public void LoadMachineSouce()
        {
            var devices = GetAvailableTWAINDevices();
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
            try
            {
                if (EZTwain.GetSourceList())
                {
                    buffer.EnsureCapacity(64);
                    while (EZTwain.GetNextSourceName(buffer))
                    {
                        devices.Add(buffer.ToString());
                        buffer.EnsureCapacity(64);
                    }
                }
            }
            catch { MessageBox.Show($"Chưa cài đặt EZTwain", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error); }
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
                EZTwain.SetHideUI(true);
                ClearImages();
                EZTwain.SelectFeeder(true);
                EZTwain.SetPixelType(0);
                EZTwain.SetBitDepth(1);
                EZTwain.SetResolution(300);
                EZTwain.SetAutoDeskew(1);
                EZTwain.SetXferCount(-1);
                EZTwain.SetAutoScan(true);

                string tempDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempScans");
                Directory.CreateDirectory(tempDirectory);
                string baseFileName = "scanned_page";
                string basePath = Path.Combine(tempDirectory, $"{baseFileName}0.png");

                int pagesScanned = EZTwain.AcquireImagesToFiles(this.Handle, basePath);

                if (pagesScanned > 0)
                {
                    for (int i = 0; i < pagesScanned; i++)
                    {
                        string fileName = $"{baseFileName}{i}.png";
                        using (var image = Image.FromFile(fileName))
                        {
                            IntPtr hdib = EZTwain.DIB_FromImage(image);
                            if (hdib != IntPtr.Zero)
                            {
                                AppendImage(hdib);
                            }
                        }
                    }

                    m_ipage = m_dibs.Length > 0 ? 0 : -1;
                    RepaintImage();
                }
                else
                {
                    MessageBox.Show("Scan thất bại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                EZTwain.CloseSource();

                // Delete temp directory and its contents
                Directory.Delete(tempDirectory, true);
            }
            else
            {
                MessageBox.Show("Kết nối thất bại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion EZTwain related stuffs

        #region Process Data (Click events, ...)

        private string getOutputPath()
        {
            string drive = cboDrives.Text.Replace("\\", "");
            string createMonth = (Convert.ToDateTime(dtpCreateDate.EditValue).ToString("yyyy-MM"));
            string reportType = cboReportTypes.Text;
            string factory = cboFactories.Text;
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

        private string GetFullPath()
        {
            string output = getOutputPath();
            string createDate = DateTime.Now.ToString("ddMMyyyyHHmmss");
            string fileType = cboOutput.Text.ToLower();
            string documentCode = txtDocumentCode.Text;
            string fileName = $"{documentCode}_{createDate}.{fileType}";
            string fullPath = Path.Combine(output, fileName);
            return fullPath;
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

        private void btnFlipH_Click(object sender, EventArgs e)
        {
            if (m_ipage >= 0)
            {
                EZTwain.DIB_FlipHorizontal(m_dibs[m_ipage]);
                RepaintImage();
            }
        }

        private void btnFlipV_Click(object sender, EventArgs e)
        {
            if (m_ipage >= 0)
            {
                EZTwain.DIB_FlipVertical(m_dibs[m_ipage]);
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
            try
            {
                if (EZTwain.OpenSource(deviceName))
                    lblInfo.Text = $"Đã kết nối {deviceName}";
                else
                    lblInfo.Text = $"Kết nối tới {deviceName} thất bại";
            }
            catch { MessageBox.Show("Chưa cài đặt EZTwain", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnDefaultSize_Click(object sender, EventArgs e)
        {
            picPreview.Properties.ZoomPercent = 100;
        }

        private (LookUpEdit, LookUpEdit, System.Type) GetComboBoxInfo(string tag)
        {
            switch (tag)
            {
                case "ReportTypes": return (cboReportTypes, null, typeof(ReportTypes));
                case "Factories": return (cboFactories, null, typeof(Factories));
                case "Groups": return (cboGroups, cboFactories, typeof(Groups));
                case "Lvl1": return (cboLvl1, cboGroups, typeof(Level1));
                case "Lvl2": return (cboLvl2, cboLvl1, typeof(Level2));
                case "Lvl3": return (cboLvl3, cboLvl2, typeof(Level3));
                default: return (null, null, null);
            }
        }

        private string GetPromptMessage(string tag)
        {
            switch (tag)
            {
                case "ReportTypes": return ("loại báo cáo");
                case "Factories": return ("nhà máy");
                case "Groups": return ("nhóm");
                case "Lvl1": return ("mức 1");
                case "Lvl2": return ("mức 2");
                case "Lvl3": return ("mức 3");
                default: return ("");
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!(sender is SimpleButton clickedButton)) return;

            var tag = clickedButton.Tag.ToString();
            var (currentCbo, prevCbo, Model) = GetComboBoxInfo(tag);
            if (prevCbo != null && prevCbo.EditValue == null)
            {
                MessageBox.Show("Vui lòng chọn dữ liệu ở ô trên", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return; ;
            }

            var promptMessage = GetPromptMessage(tag);
            var result = XtraInputBox.Show("Thêm " + promptMessage, "Thêm", "");
            if (!string.IsNullOrEmpty(result))
            {
                var sqliteHelperType = typeof(SqliteHelper<>).MakeGenericType(Model);
                var sqliteInstance = Activator.CreateInstance(sqliteHelperType);
                var modelInstance = Activator.CreateInstance(Model);
                foreach (var property in Model.GetProperties())
                {
                    if (property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) && property.CanWrite)
                    {
                        property.SetValue(modelInstance, result);
                    }
                    if (property.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase) && property.Name.Length > 2 && property.CanWrite && prevCbo != null)
                    {
                        property.SetValue(modelInstance, prevCbo.EditValue);
                    }
                }
                var insert = sqliteHelperType.GetMethod("Insert");
                insert.Invoke(sqliteInstance, new object[] { modelInstance });
                var loadData = this.GetType().GetMethod("LoadDataComboBox");
                var LoadData = loadData.MakeGenericMethod(Model);
                var foreignKeyID = (int?)prevCbo?.EditValue;
                LoadData.Invoke(this, new object[] { currentCbo, foreignKeyID });
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (!(sender is SimpleButton clickedButton)) return;
            var tag = clickedButton.Tag.ToString();
            var (currentCbo, prevCbo, Model) = GetComboBoxInfo(tag);

            if (currentCbo != null && currentCbo.Text == "") return;
            var confirm = MessageBox.Show("Bạn có muốn xóa mục này và dữ liệu liên quan không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var sqliteHelperType = typeof(SqliteHelper<>).MakeGenericType(Model);
            var sqliteInstance = Activator.CreateInstance(sqliteHelperType);
            var deleteCurrentOption = sqliteHelperType.GetMethod("Delete");
            deleteCurrentOption.Invoke(sqliteInstance, new object[] { currentCbo.EditValue });
            var loadData = this.GetType().GetMethod("LoadDataComboBox");
            var LoadData = loadData.MakeGenericMethod(Model);
            var foreignKeyID = (int?)prevCbo?.EditValue;
            LoadData.Invoke(this, new object[] { currentCbo, foreignKeyID });
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

            string searchPattern = $"{fileNameStart}*";

            try
            {
                var directories = Directory.EnumerateDirectories(driveName, "*", SearchOption.TopDirectoryOnly);
                int order = 1;
                foreach (var directory in directories)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(directory);
                    if ((dirInfo.Attributes & FileAttributes.Hidden) == 0 && (dirInfo.Attributes & FileAttributes.System) == 0)
                    {
                        try
                        {
                            foreach (string file in Directory.EnumerateFiles(directory, searchPattern, SearchOption.AllDirectories))
                            {
                                FileInfo fileInfo = new FileInfo(file);
                                string fileSize = Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2) == 0
                                                ? "≈ 0MB"
                                                : $"{Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2)}MB";
                                fileTable.Rows.Add(order, fileInfo.Name, fileInfo.CreationTime, fileSize, fileInfo.FullName);
                                order++;
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            MessageBox.Show($"Không có quyền truy cập vào {directory}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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

        private void cboFactories_EditValueChanged(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(cboFactories.EditValue);
            LoadDataComboBox<Groups>(cboGroups, id);
            cboGroups_EditValueChanged(null, null);
        }

        private void cboGroups_EditValueChanged(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(cboGroups.EditValue);
            LoadDataComboBox<Level1>(cboLvl1, id);
            cboLvl1_EditValueChanged(null, null);
        }

        private void cboLvl1_EditValueChanged(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(cboLvl1.EditValue);
            LoadDataComboBox<Level2>(cboLvl2, id);
            cboLvl2_EditValueChanged(null, null);
        }

        private void cboLvl2_EditValueChanged(object sender, EventArgs e)
        {
            var id = Convert.ToInt32(cboLvl2.EditValue);
            LoadDataComboBox<Level3>(cboLvl3, id);
        }

        #endregion Process Data (Click events, ...)

        private void btnScanOld_Click(object sender, EventArgs e)
        {
            string deviceName = cboSource.Text;
            string output = getOutputPath();
            if (!Directory.Exists(output))
            {
                Directory.CreateDirectory(output);
            }
            if (EZTwain.OpenSource(deviceName))
            {
                if (cboScanTypeOld.Text == "All")
                {
                    EZTwain.SetHideUI(true);
                    EZTwain.SetPixelType(0);
                    EZTwain.SetResolution(300);
                    string fullPath = GetFullPath();
                    EZTwain.AcquireMultipageFile(this.Handle, fullPath);
                    Process.Start(output);
                    return;
                }
                if (cboScanTypeOld.Text == "Auto")
                {
                    EZTwain.SetHideUI(true);
                    EZTwain.SetMultiTransfer(true);
                    if (EZTwain.EnableSource(this.Handle))
                    {
                        while (EZTwain.State() > 4)
                        {
                            string fullPath = GetFullPath();
                            int result = EZTwain.AcquireToFilename(this.Handle, fullPath);
                            if (result == 0)
                            {
                                IntPtr[] dibs = new IntPtr[1000];
                                int n = EZTwain.DIB_LoadArrayFromFilename(dibs, 1000, fullPath);
                                if (n <= 0) return;
                                AddImages(dibs, n);
                            }
                        }
                    }
                    Process.Start(output);
                    return;
                }
                if (cboScanTypeOld.Text == "Multi")
                {
                    EZTwain.SetMultiTransfer(true);
                    EZTwain.SetHideUI(true);
                    EZTwain.SetUnits(EZTwain.TWUN_INCHES);
                    EZTwain.SetResolution(200);
                    EZTwain.SelectFeeder(false);
                    EZTwain.ResetImageLayout();
                    EZTwain.SetMultipageFormat(EZTwain.MULTIPAGE_PDF);
                    string fullPath = GetFullPath();
                    int result = EZTwain.BeginMultipageFile(fullPath);
                    if (result == 0)
                    {
                        while (true)
                        {
                            IntPtr hdib = EZTwain.Acquire(this.Handle);
                            EZTwain.DisableSource();

                            if (hdib == IntPtr.Zero)
                                break;

                            int nResult = EZTwain.DibWritePage(hdib);
                            if (nResult != 0)
                                break;
                            AppendImage(hdib);
                            m_ipage = m_dibs.Length > 0 ? 0 : -1;
                            RepaintImage();

                            DialogResult confirmContinue = MessageBox.Show("Bạn có scan trang khác nữa không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (confirmContinue != DialogResult.Yes)
                                break;
                        }
                    }
                    EZTwain.EndMultipageFile();
                    Process.Start(output);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Kết nối thất bại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}