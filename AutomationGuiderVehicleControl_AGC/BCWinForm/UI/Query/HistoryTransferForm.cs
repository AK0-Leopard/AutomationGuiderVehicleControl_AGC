using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ObjectRelay;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.mirle.ibg3k0.bcf.App;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class HistoryTransferForm : Form
    {
        BCMainForm mainform;
        BindingSource cmsMCS_bindingSource = new BindingSource();
        List<HCMD_MCSObjToShow> hTranShowList = null;
#pragma warning disable CS0414 // 已指派欄位 'HistoryAlarmsForm.selection_index'，但從未使用過其值。
        int selection_index = -1;
#pragma warning restore CS0414 // 已指派欄位 'HistoryAlarmsForm.selection_index'，但從未使用過其值。
        public HistoryTransferForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            dgv_TransferCommand.AutoGenerateColumns = false;
            mainform = _mainForm;

            dgv_TransferCommand.DataSource = cmsMCS_bindingSource;

            m_StartDTCbx.Value = DateTime.Today;
            m_EndDTCbx.Value = DateTime.Now;
        }

        private async void updateHTransfer()
        {
            DateTime start_time = m_StartDTCbx.Value;
            DateTime end_time = m_EndDTCbx.Value;

            if (start_time > end_time)
            {
                BCFApplication.onWarningMsg($"Transfer query, the start time cannot be later than the end time");
                return;
            }

            var h_tran = await Task.Run(() => mainform.BCApp.SCApplication.TransferBLL.db.hTransfer.loadHTrnasfer(start_time, end_time));
            if (h_tran != null && h_tran.Count > 0)
            {
                string alarm_code = m_id.Text;
                string cmd_id = m_id.Text;
                if (!SCUtility.isEmpty(cmd_id))
                {
                    h_tran = h_tran.Where(tran => SCUtility.isMatche(tran.ID, cmd_id)).ToList();
                }
                hTranShowList = h_tran;
                cmsMCS_bindingSource.DataSource = hTranShowList;
                dgv_TransferCommand.Refresh();
            }
        }

        private void TransferCommandQureyListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainform.removeForm(this.Name);
        }

        private void btnlSearch_Click(object sender, EventArgs e)
        {
            try
            {
                this.btnlSearch.Enabled = false;
                selection_index = -1;
                updateHTransfer();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
            finally
            {
                this.btnlSearch.Enabled = true;
            }
        }

        private async void m_exportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Alarm files (*.xlsx)|*.xlsx";
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK || bcf.Common.BCFUtility.isEmpty(dlg.FileName))
                {
                    return;
                }
                string filename = dlg.FileName;
                //建立 xlxs 轉換物件
                Common.XSLXHelper helper = new Common.XSLXHelper();
                //取得轉為 xlsx 的物件
                ClosedXML.Excel.XLWorkbook xlsx = null;
                await Task.Run(() => xlsx = helper.Export(hTranShowList));
                if (xlsx != null)
                    xlsx.SaveAs(filename);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception");
            }
        }
    }
}
