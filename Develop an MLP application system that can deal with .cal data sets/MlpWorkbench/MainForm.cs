using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MlpWorkbench
{
    /// <summary>
    /// WinForms front end. The form owns no training logic of its own; it
    /// validates user input, drives <see cref="MLP"/> and renders the chart,
    /// the diagram and the confusion matrix.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly MLP mlp = new MLP();
        private readonly Series rmseSeries;
        private DemoOptions pendingDemo;
        private bool updatingLayerEditor;
        private bool isTraining;

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;

            rmseSeries = new Series("Test RMSE")
            {
                ChartType = SeriesChartType.Line,
                ChartArea = "ChartArea1",
                Legend = "Legend1",
                Color = System.Drawing.Color.Firebrick,
                BorderWidth = 2,
                XValueType = ChartValueType.Int32,
                IsVisibleInLegend = true
            };
            chtShow.Series.Clear();
            chtShow.Series.Add(rmseSeries);

            ApplyHiddenLayerEditor(new[] { 4 });
            SetUiEnabled(false);
            UpdateStatsLabels();
            AppendLog("MLP Classifier ready.");
            AppendLog("Open a .cal data set (File > Open .cal data...) or use File > Load bundled iris sample.");
            AppendLog("The .cal header is: <samples> <inputs> <targets> <inputWidth>.");
            SetStatus("Open a .cal data file to begin.");
        }

        // ==================================================================
        // Startup / demo mode
        // ==================================================================
        public void QueueDemo(DemoOptions options)
        {
            pendingDemo = options;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (pendingDemo != null)
            {
                DemoOptions options = pendingDemo;
                pendingDemo = null;
                // Demo training is short (well under a second for the bundled
                // iris set). Running it before the first paint guarantees that
                // an unattended screenshot always sees the finished state.
                RunDemo(options);
            }
        }

        private void RunDemo(DemoOptions options)
        {
            try
            {
                string dataPath = options.DataPath;
                if (string.IsNullOrWhiteSpace(dataPath))
                    dataPath = SampleLocator.FindSample("iris.cal");
                if (string.IsNullOrWhiteSpace(dataPath) || !File.Exists(dataPath))
                    throw new FileNotFoundException(
                        "The bundled sample 'samples\\iris.cal' was not found. Pass --data <path> or run from the repository root.");

                Text = "MLP Workbench - Iris demo";

                mlp.RandomSeed = options.Seed;
                mlp.InitialEta = options.Eta;
                mlp.EtaDecay = options.Decay;
                mlp.Momentum = options.Momentum;
                mlp.TrainingRatio = options.Ratio;
                mlp.EpochLimit = options.Epochs > 0 ? options.Epochs : 500;

                mlp.ReadData(dataPath);
                int[] hidden = options.HiddenSizes ?? mlp.SuggestHiddenSizes();
                ApplyHiddenLayerEditor(hidden);
                mlp.Reset(hidden);

                rtbShow.Clear();
                AppendLog("=== Demo mode ===");
                AppendLog("Data: " + dataPath);
                AppendLog(mlp.GetSummaryText());
                AppendLog(string.Format(CultureInfo.InvariantCulture,
                    "Hyperparameters: eta0={0:0.###}, decay={1:0.####}/epoch, momentum={2:0.###}, seed={3}, epochs={4}",
                    mlp.InitialEta, mlp.EtaDecay, mlp.Momentum, mlp.RandomSeed, mlp.EpochLimit));
                AppendLog("Targets are one-hot: " + (mlp.IsOneHotTargets ? "yes" : "no"));
                AppendLog("");

                RefreshAll();
                SetUiEnabled(true);
                SetTrainingState(true);
                SetStatus("Demo: training...");

                var stopwatch = Stopwatch.StartNew();
                mlp.TrainToEnd();
                stopwatch.Stop();

                mlp.TestingClassification();
                RefreshAll();
                tabControl1.SelectedTab = tabPage2;
                AppendTrainingSummary(stopwatch.Elapsed);
                SetStatus(string.Format(CultureInfo.InvariantCulture,
                    "Demo ready: {0}/{1} epochs | test RMSE {2:0.####} | test accuracy {3:0.00}% ({4}/{5})",
                    mlp.CurrentEpoch, mlp.EpochLimit, mlp.TestRmse,
                    mlp.TestAccuracy * 100.0, mlp.CorrectTestPredictions, mlp.NumberOfTestSamples));

                if (!string.IsNullOrWhiteSpace(options.ReportPath))
                    WriteReport(options.ReportPath, dataPath, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                if (IsDisposed)
                    return;

                SetStatus("Demo failed: " + ex.Message);
                AppendLog("ERROR: " + ex.Message);

                if (!string.IsNullOrWhiteSpace(options.ReportPath))
                {
                    try
                    {
                        WriteTextFile(options.ReportPath,
                            "MLP Demo Report" + Environment.NewLine +
                            "===============" + Environment.NewLine +
                            "DEMO FAILED: " + ex + Environment.NewLine);
                    }
                    catch
                    {
                        // Reporting must never hide the original failure.
                    }
                }

                MessageBox.Show(this, "Demo mode failed:\n\n" + ex.Message,
                    "MLP Classifier - demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                    SetTrainingState(false);
            }
        }

        private void WriteReport(string path, string dataPath, TimeSpan elapsed)
        {
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            WriteTextFile(fullPath, mlp.BuildReport(dataPath, elapsed));
        }

        private static void WriteTextFile(string path, string text)
        {
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        // ==================================================================
        // Data loading
        // ==================================================================
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isTraining)
                return;

            dlgOpen.Filter = "CAL data files (*.cal)|*.cal|All files (*.*)|*.*";
            if (dlgOpen.ShowDialog(this) != DialogResult.OK)
                return;

            TryLoadData(dlgOpen.FileName, true);
        }

        private void loadIrisSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isTraining)
                return;

            string path = SampleLocator.FindSample("iris.cal");
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show(this,
                    "The bundled sample 'samples\\iris.cal' was not found.\n\n" +
                    "Run the application from the repository root or use File > Open .cal data...",
                    "Sample not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TryLoadData(path, true);
        }

        private bool TryLoadData(string path, bool showErrors)
        {
            try
            {
                LoadDataCore(path);
                return true;
            }
            catch (Exception ex)
            {
                SetStatus("Could not load data: " + ex.Message);
                AppendLog("ERROR loading data: " + ex.Message);
                SetUiEnabled(mlp.IsDataLoaded);
                if (showErrors)
                {
                    MessageBox.Show(this, ex.Message, "Could not load .cal data",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        private void LoadDataCore(string path)
        {
            mlp.ReadData(path);
            int[] hidden = mlp.SuggestHiddenSizes();
            ApplyHiddenLayerEditor(hidden);
            mlp.Reset(hidden);

            rtbShow.Clear();
            AppendLog("Loaded " + Path.GetFileName(path));
            AppendLog(mlp.GetSummaryText());
            AppendLog("Hidden layers: " + string.Join("-", mlp.HiddenLayerSizes) +
                      " (edit on the Network setup tab, then press Reset Network)");
            if (!mlp.IsOneHotTargets)
                AppendLog("WARNING: targets are not strictly one-hot; classification uses arg-max anyway.");
            AppendLog("");

            RefreshAll();
            SetUiEnabled(true);
            tabControl1.SelectedTab = tabPage2;
            SetStatus(string.Format(CultureInfo.InvariantCulture,
                "Loaded {0}: {1} samples, {2} inputs, {3} classes. Network {4}. Press Train To End.",
                Path.GetFileName(path), mlp.NumberOfSamples, mlp.NumberOfInputs,
                mlp.NumberOfTargets, mlp.NetworkShape));
        }

        // ==================================================================
        // Network setup tab
        // ==================================================================
        private void ApplyHiddenLayerEditor(int[] sizes)
        {
            if (sizes == null || sizes.Length == 0)
                sizes = new[] { 4 };

            int count = Math.Max(1, Math.Min(MLP.MaxHiddenLayers, sizes.Length));
            updatingLayerEditor = true;
            try
            {
                nudLayerCount.Value = count;
                lbHidden.BeginUpdate();
                lbHidden.Items.Clear();
                for (int i = 0; i < count; i++)
                {
                    int value = i < sizes.Length ? sizes[i] : sizes[sizes.Length - 1];
                    value = Math.Max(MLP.MinNodesPerLayer, Math.Min(MLP.MaxNodesPerLayer, value));
                    lbHidden.Items.Add(value);
                }
                lbHidden.SelectedIndex = count - 1;
                lbHidden.EndUpdate();
                if (lbHidden.SelectedIndex >= 0)
                    nudNodes.Value = Convert.ToDecimal(lbHidden.Items[lbHidden.SelectedIndex], CultureInfo.InvariantCulture);
                nudNodes.Enabled = !isTraining;
            }
            finally
            {
                updatingLayerEditor = false;
            }
        }

        private void nudLayerCount_ValueChanged(object sender, EventArgs e)
        {
            if (updatingLayerEditor)
                return;

            int requested = (int)nudLayerCount.Value;
            var sizes = ReadHiddenSizes().ToList();
            while (sizes.Count < requested)
                sizes.Add(sizes.Count > 0 ? sizes[sizes.Count - 1] : Math.Max(2, mlp.NumberOfTargets * 2 + 2));
            while (sizes.Count > requested)
                sizes.RemoveAt(sizes.Count - 1);

            ApplyHiddenLayerEditor(sizes.ToArray());
            MarkLayoutPending();
        }

        private void nudNodes_ValueChanged(object sender, EventArgs e)
        {
            if (updatingLayerEditor)
                return;
            if (lbHidden.SelectedIndex < 0 || lbHidden.SelectedIndex >= lbHidden.Items.Count)
                return;

            lbHidden.Items[lbHidden.SelectedIndex] = (int)nudNodes.Value;
            MarkLayoutPending();
        }

        private void lbHidden_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (updatingLayerEditor)
                return;
            if (lbHidden.SelectedIndex < 0 || lbHidden.SelectedIndex >= lbHidden.Items.Count)
            {
                nudNodes.Enabled = false;
                return;
            }

            updatingLayerEditor = true;
            try
            {
                nudNodes.Value = Convert.ToDecimal(lbHidden.Items[lbHidden.SelectedIndex], CultureInfo.InvariantCulture);
                nudNodes.Enabled = !isTraining;
            }
            finally
            {
                updatingLayerEditor = false;
            }
        }

        private int[] ReadHiddenSizes()
        {
            if (lbHidden.Items.Count == 0)
                return new[] { 4 };

            var sizes = new int[lbHidden.Items.Count];
            for (int i = 0; i < lbHidden.Items.Count; i++)
            {
                object item = lbHidden.Items[i];
                int value;
                if (item is int)
                    value = (int)item;
                else if (!int.TryParse(Convert.ToString(item, CultureInfo.InvariantCulture),
                             NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    value = 4;
                sizes[i] = Math.Max(MLP.MinNodesPerLayer, Math.Min(MLP.MaxNodesPerLayer, value));
            }
            return sizes;
        }

        private bool ValidateHiddenLayout(int[] sizes, out string error)
        {
            error = null;
            if (sizes == null || sizes.Length == 0)
            {
                error = "Choose at least one hidden layer.";
                return false;
            }
            if (sizes.Length > MLP.MaxHiddenLayers)
            {
                error = "At most " + MLP.MaxHiddenLayers + " hidden layers are supported.";
                return false;
            }
            for (int i = 0; i < sizes.Length; i++)
            {
                if (sizes[i] < MLP.MinNodesPerLayer || sizes[i] > MLP.MaxNodesPerLayer)
                {
                    error = "Hidden layer " + (i + 1) + " must contain between " + MLP.MinNodesPerLayer +
                            " and " + MLP.MaxNodesPerLayer + " nodes.";
                    return false;
                }
            }
            return true;
        }

        private void MarkLayoutPending()
        {
            if (isTraining)
                return;
            if (mlp.IsDataLoaded)
                SetStatus("Hidden-layer layout changed. Press Reset Network to rebuild the network.");
        }

        // ==================================================================
        // Training / classification actions
        // ==================================================================
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (isTraining)
                return;
            if (!mlp.IsDataLoaded)
            {
                SetStatus("No data is loaded. Open a .cal file first.");
                return;
            }

            int[] sizes = ReadHiddenSizes();
            string error;
            if (!ValidateHiddenLayout(sizes, out error))
            {
                SetStatus(error);
                MessageBox.Show(this, error, "Invalid network layout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                mlp.Reset(sizes);
                RefreshAll();
                AppendLog(string.Format(CultureInfo.InvariantCulture,
                    "Reset network {0}, seed {1}, split {2} train / {3} test.",
                    mlp.NetworkShape, mlp.RandomSeed, mlp.NumberOfTrainingSamples, mlp.NumberOfTestSamples));
                AppendLog("");
                SetStatus("Network reset to " + mlp.NetworkShape + ". Press Train To End.");
            }
            catch (Exception ex)
            {
                ShowActionError("Reset failed", ex);
            }
        }

        private void btnTrainEpoch_Click(object sender, EventArgs e)
        {
            if (!EnsureReadyForTraining())
                return;

            try
            {
                mlp.TrainOneEpoch();
                RefreshAfterEpoch();
            }
            catch (Exception ex)
            {
                ShowActionError("Training failed", ex);
            }
        }

        private async void btnTrainToEnd_Click(object sender, EventArgs e)
        {
            if (!EnsureReadyForTraining())
                return;

            int remaining = mlp.EpochLimit - mlp.CurrentEpoch;
            if (remaining <= 0)
            {
                SetStatus("Training already reached the epoch limit (" + mlp.EpochLimit + "). Press Reset Network to restart.");
                return;
            }

            SetTrainingState(true);
            SetStatus("Training " + remaining + " epoch(s) on a background thread...");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await Task.Run(() => mlp.TrainToEnd());
                stopwatch.Stop();
                if (IsDisposed)
                    return;

                mlp.TestingClassification();
                RefreshAll();
                AppendTrainingSummary(stopwatch.Elapsed);
                SetStatus(string.Format(CultureInfo.InvariantCulture,
                    "Training finished in {0:0.00}s | epoch {1}/{2} | test RMSE {3:0.####} | accuracy {4:0.00}%",
                    stopwatch.Elapsed.TotalSeconds, mlp.CurrentEpoch, mlp.EpochLimit,
                    mlp.TestRmse, mlp.TestAccuracy * 100.0));
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                    ShowActionError("Training failed", ex);
            }
            finally
            {
                if (!IsDisposed)
                    SetTrainingState(false);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!isTraining)
                return;
            mlp.RequestCancel();
            SetStatus("Stop requested. Training will stop after the current epoch.");
        }

        private void btnClassification_Click(object sender, EventArgs e)
        {
            if (isTraining)
                return;
            if (!mlp.IsDataLoaded)
            {
                SetStatus("No data is loaded. Open a .cal file first.");
                return;
            }
            if (!mlp.IsConfigured)
            {
                SetStatus("The network is not configured. Press Reset Network first.");
                return;
            }

            try
            {
                mlp.TestingClassification();
                RefreshAll();
                AppendLog("--- Classification test ---");
                AppendLog(string.Format(CultureInfo.InvariantCulture,
                    "Accuracy: {0:0.00}% ({1}/{2}) | test MSE {3:0.######} | test RMSE {4:0.######}",
                    mlp.TestAccuracy * 100.0, mlp.CorrectTestPredictions, mlp.NumberOfTestSamples,
                    mlp.TestMse, mlp.TestRmse));
                AppendLog(mlp.GetConfusionMatrixText());
                AppendLog("");
                SetStatus(string.Format(CultureInfo.InvariantCulture,
                    "Classification: {0}/{1} correct ({2:0.00}%), test RMSE {3:0.####}.",
                    mlp.CorrectTestPredictions, mlp.NumberOfTestSamples,
                    mlp.TestAccuracy * 100.0, mlp.TestRmse));
            }
            catch (Exception ex)
            {
                ShowActionError("Classification failed", ex);
            }
        }

        private bool EnsureReadyForTraining()
        {
            if (isTraining)
                return false;
            if (!mlp.IsDataLoaded)
            {
                SetStatus("No data is loaded. Open a .cal file first.");
                return false;
            }
            if (!mlp.IsConfigured)
            {
                SetStatus("The network is not configured. Press Reset Network first.");
                return false;
            }
            return true;
        }

        private void ShowActionError(string title, Exception ex)
        {
            SetStatus(title + ": " + ex.Message);
            AppendLog("ERROR: " + ex.Message);
            MessageBox.Show(this, ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ==================================================================
        // Rendering / state
        // ==================================================================
        private void RefreshAfterEpoch()
        {
            UpdateStatsLabels();
            RefreshChart();
            ppgShow.Refresh();
            splitContainer2.Panel2.Invalidate();
            SetStatus(string.Format(CultureInfo.InvariantCulture,
                "Epoch {0}/{1} | test RMSE {2:0.######} | accuracy {3:0.00}% | eta {4:0.######}",
                mlp.CurrentEpoch, mlp.EpochLimit, mlp.TestRmse, mlp.TestAccuracy * 100.0, mlp.CurrentEta));
        }

        private void RefreshAll()
        {
            UpdateStatsLabels();
            RefreshChart();
            if (!ReferenceEquals(ppgShow.SelectedObject, mlp))
                ppgShow.SelectedObject = mlp;
            else
                ppgShow.Refresh();
            splitContainer2.Panel2.Invalidate();
        }

        private void RefreshChart()
        {
            rmseSeries.Points.Clear();
            var history = mlp.History;
            int count = history.Count;
            if (count > 0)
            {
                int step = Math.Max(1, count / 2000);
                for (int i = 0; i < count; i += step)
                    rmseSeries.Points.AddXY(history[i].Epoch, history[i].TestRmse);
                if ((count - 1) % step != 0)
                    rmseSeries.Points.AddXY(history[count - 1].Epoch, history[count - 1].TestRmse);
            }

            ChartArea area = chtShow.ChartAreas[0];
            area.AxisY.Minimum = 0;
            area.AxisY.IsStartedFromZero = true;
            area.RecalculateAxesScale();
            chtShow.Invalidate();
        }

        private void UpdateStatsLabels()
        {
            if (!mlp.IsDataLoaded)
            {
                lblEpochInfo.Text = "Epoch: - / -";
                lblRmseInfo.Text = "Test RMSE: -";
                lblAccuracyInfo.Text = "Test accuracy: -";
                return;
            }

            lblEpochInfo.Text = string.Format(CultureInfo.InvariantCulture,
                "Epoch: {0} / {1}", mlp.CurrentEpoch, mlp.EpochLimit);
            lblRmseInfo.Text = mlp.HasEvaluation
                ? string.Format(CultureInfo.InvariantCulture, "Test RMSE: {0:0.0000}", mlp.TestRmse)
                : "Test RMSE: -";
            lblAccuracyInfo.Text = mlp.HasEvaluation
                ? string.Format(CultureInfo.InvariantCulture, "Test accuracy: {0:0.00}% ({1}/{2})",
                    mlp.TestAccuracy * 100.0, mlp.CorrectTestPredictions, mlp.NumberOfTestSamples)
                : "Test accuracy: -";
        }

        private void SetUiEnabled(bool enabled)
        {
            bool hasData = enabled && mlp.IsDataLoaded;
            bool configured = hasData && mlp.IsConfigured;

            btnReset.Enabled = hasData && !isTraining;
            btnTrainEpoch.Enabled = configured && !isTraining;
            btnTrainToEnd.Enabled = configured && !isTraining;
            btnClassification.Enabled = configured && !isTraining;
            btnCancel.Enabled = isTraining;

            nudLayerCount.Enabled = !isTraining;
            lbHidden.Enabled = !isTraining;
            nudNodes.Enabled = !isTraining && lbHidden.SelectedIndex >= 0;
            ppgShow.Enabled = !isTraining;
        }

        private void SetTrainingState(bool training)
        {
            isTraining = training;
            progressBar1.Visible = training;
            UseWaitCursor = training;
            SetUiEnabled(mlp.IsDataLoaded);
            if (!training)
                splitContainer2.Panel2.Invalidate();
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void AppendLog(string text)
        {
            rtbShow.AppendText(text + Environment.NewLine);
            rtbShow.SelectionStart = rtbShow.TextLength;
            rtbShow.ScrollToCaret();
        }

        private void AppendTrainingSummary(TimeSpan elapsed)
        {
            AppendLog("--- Training finished ---");
            AppendLog(string.Format(CultureInfo.InvariantCulture,
                "Epochs: {0}/{1} | time {2:0.00}s | eta {3:0.######} | test MSE {4:0.######} | test RMSE {5:0.######}",
                mlp.CurrentEpoch, mlp.EpochLimit, elapsed.TotalSeconds, mlp.CurrentEta, mlp.TestMse, mlp.TestRmse));
            AppendLog(string.Format(CultureInfo.InvariantCulture,
                "Test accuracy: {0:0.00}% ({1}/{2})",
                mlp.TestAccuracy * 100.0, mlp.CorrectTestPredictions, mlp.NumberOfTestSamples));
            AppendLog(mlp.GetConfusionMatrixText());
            AppendLog("");
        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(splitContainer2.Panel2.BackColor);
            if (mlp.IsConfigured)
            {
                try
                {
                    // Do not run a forward pass on the UI thread while the
                    // background trainer is using the shared activation
                    // buffers. The diagram then shows the last safe snapshot.
                    if (!isTraining)
                        mlp.UpdateDisplayActivations();
                    Rectangle bounds = splitContainer2.Panel2.ClientRectangle;
                    bounds.Inflate(-6, -6);
                    mlp.DrawMLP(e.Graphics, bounds);
                }
                catch (Exception)
                {
                    // Painting must never crash the application.
                }
                return;
            }

            using (var font = new System.Drawing.Font("Segoe UI", 11F))
            using (var format = new System.Drawing.StringFormat())
            {
                format.Alignment = System.Drawing.StringAlignment.Center;
                format.LineAlignment = System.Drawing.StringAlignment.Center;
                e.Graphics.DrawString(
                    "Network diagram\r\n\r\nLoad a .cal data file and press Reset Network.",
                    font, System.Drawing.Brushes.Gray, splitContainer2.Panel2.ClientRectangle, format);
            }
        }

        // ==================================================================
        // Menu handlers
        // ==================================================================
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this,
                "MLP Classifier\r\n\r\n" +
                "Multilayer perceptron with sigmoid activations, online backpropagation,\r\n" +
                "optional momentum, min-max normalization, a reproducible train/test split\r\n" +
                "and a .cal data-set reader.\r\n\r\n" +
                "Built with .NET Framework 4.8 (WinForms).\r\n" +
                "MIT License - Copyright (c) 2026 Benny Lin",
                "About MLP Classifier", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (isTraining)
                mlp.RequestCancel();
            base.OnFormClosing(e);
        }
    }
}


