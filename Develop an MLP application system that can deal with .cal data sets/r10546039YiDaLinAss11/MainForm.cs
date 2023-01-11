using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace r10546039YiDaLinAss11
{
    public partial class MainForm : Form
    {
        // data fields
        MLP theMLP;
        Series series_RMSE;
        
        public MainForm()
        {
            InitializeComponent();
        }
        public void UpdateShow()
        {
            splitContainer2.Panel2.Refresh();
            chtShow.ChartAreas[0].RecalculateAxesScale();
            rtbShow.AppendText("oot-mean-square error: " + Math.Round(theMLP.RMSE, 3).ToString());
        }

        public void UpdateTestClassification_UI()
        {
            float correctneww = theMLP.TestingClassification();
            StringFormat sm;
            string str = string.Empty;

            for (int i = 0; i < theMLP.Dimension_Target; i++)
            {
                for (int j = 0; j < theMLP.Dimension_Target; j++)
                {
                    str += string.Format("{000 }", theMLP.ConfusingTable[i, j]) + "    ";
                }
                str += "\n ";
            }

            rtbShow.AppendText("Correctness: " + Math.Round((correctneww / theMLP.Number_of_Testing_Data), 3).ToString() + "\n"
                + $" ( {correctneww} / {theMLP.Number_of_Testing_Data} )" + "\n" + "\n"
                + "Confusion matrix:" + "\n" + str);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            int[] number_of_Layer = new int[lbShow.Items.Count];
            for (int i = 0; i < lbShow.Items.Count; i++)
                number_of_Layer[i] = int.Parse(lbShow.Items[i].ToString());
            theMLP.Reset_Weights_And_Initial_Condition(number_of_Layer);
            series_RMSE = theMLP.SeriesRMSE;
            chtShow.Series.Clear();
            chtShow.Series.Add(series_RMSE);
            ppgShow.SelectedObject = theMLP;
            splitContainer2.Panel2.Refresh();
            UpdateShow();
            UpdateTestClassification_UI();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK) return;
            theMLP = new MLP();
            theMLP.Read_Data(dlg.FileName);

            ppgShow.SelectedObject = theMLP;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            theMLP.TrainAnEpoch();
            UpdateShow();
        }

        private void button2_Click(object sender, EventArgs e)
        {


            theMLP.Train_To_End();

            UpdateShow();
            UpdateTestClassification_UI();
        }

        private void btnRuntoEnd_Click(object sender, EventArgs e)
        {
            theMLP.Train_To_End();
            UpdateShow();
            UpdateTestClassification_UI();
        }

        private void btnClassification_Click(object sender, EventArgs e)
        {
            UpdateTestClassification_UI();
        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {
            if (theMLP == null) return;
            theMLP.DrawMLP(e.Graphics, e.ClipRectangle);
        }

        private void tbnumber_TextChanged(object sender, EventArgs e)
        {
            if (int.Parse(tbnumber.Text) > lbShow.Items.Count)
            {
                lbShow.Items.Add("4");
            }
            else if (int.Parse(tbnumber.Text) < lbShow.Items.Count)
            {
                lbShow.Items.RemoveAt(lbShow.Items.Count - 1);
            }
        }

        private void tbnode_TextChanged(object sender, EventArgs e)
        {
            if (lbShow.SelectedItem == null) return;
            lbShow.Items[lbShow.SelectedIndex] = tbnode.Text;
        }

        private void lbShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbShow.SelectedItem == null) return;
            decimal s;
            s = decimal.Parse(lbShow.SelectedItem.ToString());
            tbnode.Text = s.ToString();
        }
    }
}
