using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace MlpWorkbench
{
    /// <summary>
    /// Immutable per-epoch training statistics used by the convergence chart,
    /// the status labels and the demo report.
    /// </summary>
    public sealed class EpochStatistics
    {
        public EpochStatistics(int epoch, double trainingMse, double testMse, double testRmse, double accuracy, double eta)
        {
            Epoch = epoch;
            TrainingMse = trainingMse;
            TestMse = testMse;
            TestRmse = testRmse;
            Accuracy = accuracy;
            Eta = eta;
        }

        public int Epoch { get; }
        public double TrainingMse { get; }
        public double TestMse { get; }
        public double TestRmse { get; }
        public double Accuracy { get; }
        public double Eta { get; }
    }

    /// <summary>
    /// A fully connected multilayer perceptron trained with online (stochastic)
    /// backpropagation on sigmoid units.
    ///
    /// Conventions
    /// -----------
    /// * Layer 0 holds the input features, the last layer holds the outputs.
    /// * Every non-output layer has an implicit bias value of 1.0. The bias is
    ///   stored in column 0 of the weight matrix, so weights[l][j][0] is the
    ///   bias of neuron j in layer l and weights[l][j][q] (q >= 1) connects
    ///   neuron q-1 of layer l-1 to neuron j of layer l.
    /// * Classification uses the arg-max output unit (one-hot targets).
    /// * The confusion matrix is indexed as [actual, predicted].
    /// </summary>
    public class MLP
    {
        public const int MinSamples = 2;
        public const int MaxHiddenLayers = 6;
        public const int MinNodesPerLayer = 1;
        public const int MaxNodesPerLayer = 512;
        public const int MaxInputOrTargetDimensions = 4096;

        // ------------------------------------------------------------------
        // Data
        // ------------------------------------------------------------------
        private double[][] rawInputs;
        private double[][] rawTargets;
        private double[] featureMin;
        private double[] featureMax;
        private int numberOfSamples;
        private int numberOfInputs;
        private int numberOfTargets;
        private int declaredInputWidth;
        private bool targetsAreOneHot;
        private string sourcePath = string.Empty;

        // ------------------------------------------------------------------
        // Network
        // ------------------------------------------------------------------
        private int[] layerSizes = new int[0];          // input, hidden..., output (no bias entries)
        private int[] hiddenLayerSizes = new int[0];
        private double[][][] weights;                   // [layer][neuron][bias + previous layer]
        private double[][][] momentumDelta;             // same shape as weights
        private double[][] activations;                 // sigmoid outputs per layer
        private double[][] deltas;                      // error terms per layer
        private bool configured;

        // ------------------------------------------------------------------
        // Training
        // ------------------------------------------------------------------
        private int[] dataIndices = new int[0];
        private int[] trainingIndices = new int[0];
        private int[] testIndices = new int[0];
        private readonly List<EpochStatistics> history = new List<EpochStatistics>();
        private Random shuffleRandom;

        private int currentEpoch;
        private int epochLimit = 500;
        private double initialEta = 0.30;
        private double eta;
        private double etaDecay = 0.99;
        private double momentum = 0.90;
        private double trainingRatio = 0.70;
        private int randomSeed = 42;
        private bool finished;
        private volatile bool cancelRequested;

        // ------------------------------------------------------------------
        // Evaluation
        // ------------------------------------------------------------------
        private int[,] confusion = new int[0, 0];
        private double testMse;
        private double testRmse;
        private double trainingMseLast;
        private double accuracy;
        private int correctPredictions;
        private bool hasEvaluation;
        private int displaySampleIndex;

        // ==================================================================
        // Public configuration properties (shown in the PropertyGrid)
        // ==================================================================
        [Category("Training"), DisplayName("Initial learning rate (eta0)"),
         Description("Starting value of eta. Typical range: 0.05 - 0.5 for min-max normalized sigmoid networks.")]
        public double InitialEta
        {
            get { return initialEta; }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0.0 || value > 10.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "InitialEta must be greater than 0 and at most 10.");
                initialEta = value;
            }
        }

        [Category("Training"), DisplayName("Eta decay per epoch"),
         Description("Multiplicative decay applied once per epoch (0 < decay <= 1). 1 means no decay.")]
        public double EtaDecay
        {
            get { return etaDecay; }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "EtaDecay must be greater than 0 and at most 1.");
                etaDecay = value;
            }
        }

        [Category("Training"), DisplayName("Momentum"),
         Description("Optional SGD momentum in [0, 1). 0 disables momentum.")]
        public double Momentum
        {
            get { return momentum; }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value < 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Momentum must be in [0, 1).");
                momentum = value;
            }
        }

        [Category("Training"), DisplayName("Epoch limit"),
         Description("Upper bound on the number of training epochs.")]
        public int EpochLimit
        {
            get { return epochLimit; }
            set
            {
                if (value < 1 || value > 1000000)
                    throw new ArgumentOutOfRangeException(nameof(value), "EpochLimit must be between 1 and 1,000,000.");
                epochLimit = value;
                if (configured)
                    finished = currentEpoch >= epochLimit;
            }
        }

        [Category("Training"), DisplayName("Training ratio"),
         Description("Fraction of samples used for training; the rest form the test set. Must be in (0, 1).")]
        public double TrainingRatio
        {
            get { return trainingRatio; }
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0.0 || value >= 1.0)
                    throw new ArgumentOutOfRangeException(nameof(value), "TrainingRatio must be strictly between 0 and 1.");
                trainingRatio = value;
            }
        }

        [Category("Training"), DisplayName("Random seed"),
         Description("Seed for weight initialization, the train/test shuffle and the per-epoch shuffle. Reset applies it.")]
        public int RandomSeed
        {
            get { return randomSeed; }
            set { randomSeed = value; }
        }

        // ==================================================================
        // Read-only status properties
        // ==================================================================
        [Category("Status"), ReadOnly(true), DisplayName("Samples")]
        public int NumberOfSamples { get { return numberOfSamples; } }

        [Category("Status"), ReadOnly(true), DisplayName("Inputs")]
        public int NumberOfInputs { get { return numberOfInputs; } }

        [Category("Status"), ReadOnly(true), DisplayName("Targets / classes")]
        public int NumberOfTargets { get { return numberOfTargets; } }

        [Category("Status"), ReadOnly(true), DisplayName("Declared input width")]
        public int DeclaredInputWidth { get { return declaredInputWidth; } }

        [Category("Status"), ReadOnly(true), DisplayName("Training samples")]
        public int NumberOfTrainingSamples { get { return trainingIndices.Length; } }

        [Category("Status"), ReadOnly(true), DisplayName("Test samples")]
        public int NumberOfTestSamples { get { return testIndices.Length; } }

        [Category("Status"), ReadOnly(true), DisplayName("Network shape")]
        public string NetworkShape
        {
            get { return configured ? string.Join("-", layerSizes) : "(not configured)"; }
        }

        [Category("Status"), ReadOnly(true), DisplayName("Epoch")]
        public int CurrentEpoch { get { return currentEpoch; } }

        [Category("Status"), ReadOnly(true), DisplayName("Current eta")]
        public double CurrentEta { get { return eta; } }

        [Category("Status"), ReadOnly(true), DisplayName("Test MSE")]
        public double TestMse { get { return testMse; } }

        [Category("Status"), ReadOnly(true), DisplayName("Test RMSE")]
        public double TestRmse { get { return testRmse; } }

        [Category("Status"), ReadOnly(true), DisplayName("Test accuracy")]
        public double TestAccuracy { get { return hasEvaluation ? accuracy : double.NaN; } }

        [Category("Status"), ReadOnly(true), DisplayName("Correct test predictions")]
        public int CorrectTestPredictions { get { return correctPredictions; } }

        [Browsable(false)]
        public bool IsDataLoaded { get { return rawInputs != null; } }

        [Browsable(false)]
        public bool IsConfigured { get { return configured; } }

        [Browsable(false)]
        public bool IsTrainingFinished { get { return finished; } }

        [Browsable(false)]
        public bool HasEvaluation { get { return hasEvaluation; } }

        [Browsable(false)]
        public bool IsOneHotTargets { get { return targetsAreOneHot; } }

        [Browsable(false)]
        public string SourcePath { get { return sourcePath; } }

        [Browsable(false)]
        public int[] HiddenLayerSizes { get { return (int[])hiddenLayerSizes.Clone(); } }

        [Browsable(false)]
        public IReadOnlyList<EpochStatistics> History { get { return history; } }

        [Browsable(false)]
        public int[,] ConfusionMatrix { get { return (int[,])confusion.Clone(); } }

        // Legacy aliases kept so old callers keep compiling.
        [Browsable(false)]
        public double RMSE { get { return testRmse; } }

        [Browsable(false)]
        public float RootMeanSquareError { get { return (float)testRmse; } }

        // ==================================================================
        // .cal parser
        // ==================================================================
        /// <summary>
        /// Reads a .cal data file. The file starts with one header line:
        ///     &lt;samples&gt; &lt;inputs&gt; &lt;targets&gt; &lt;inputWidth&gt;
        /// followed by one data row per sample containing exactly
        /// (inputs + targets) invariant-culture numbers. Empty lines and lines
        /// starting with '#' or '//' are ignored; an inline '#' comment is
        /// stripped. Fields may be separated by spaces, tabs, commas or
        /// semicolons.
        /// </summary>
        public void ReadData(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("No .cal file path was supplied.", nameof(filePath));

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(filePath);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("The .cal file path is not valid: " + filePath, nameof(filePath), ex);
            }

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("The .cal data file was not found: " + fullPath, fullPath);

            string[] lines = File.ReadAllLines(fullPath);
            if (lines.Length == 0)
                throw new InvalidDataException("The .cal file is empty: " + fullPath);

            int headerLineNumber = 0;
            int sampleCount = 0, inputCount = 0, targetCount = 0, inputWidth = 0;
            bool headerFound = false;
            var tokenRows = new List<TokenRow>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = StripComment(lines[i]).Trim();
                if (line.Length == 0)
                    continue;

                string[] tokens = SplitFields(line);
                if (!headerFound)
                {
                    headerLineNumber = i + 1;
                    if (tokens.Length != 4)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): the header must contain 4 integers: <samples> <inputs> <targets> <inputWidth>. Found {2} field(s).",
                                Path.GetFileName(fullPath), headerLineNumber, tokens.Length));

                    sampleCount = ParseHeaderInt(tokens[0], "sample count", headerLineNumber, fullPath);
                    inputCount = ParseHeaderInt(tokens[1], "input count", headerLineNumber, fullPath);
                    targetCount = ParseHeaderInt(tokens[2], "target count", headerLineNumber, fullPath);
                    inputWidth = ParseHeaderInt(tokens[3], "input width", headerLineNumber, fullPath);

                    if (sampleCount < MinSamples)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): a data set needs at least {2} samples, but the header declares {3}.",
                                Path.GetFileName(fullPath), headerLineNumber, MinSamples, sampleCount));
                    if (inputCount < 1 || inputCount > MaxInputOrTargetDimensions)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): the input count must be between 1 and {2}, but it is {3}.",
                                Path.GetFileName(fullPath), headerLineNumber, MaxInputOrTargetDimensions, inputCount));
                    if (targetCount < 1 || targetCount > MaxInputOrTargetDimensions)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): the target count must be between 1 and {2}, but it is {3}.",
                                Path.GetFileName(fullPath), headerLineNumber, MaxInputOrTargetDimensions, targetCount));
                    if (inputWidth < 1)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): the declared input width must be positive, but it is {2}.",
                                Path.GetFileName(fullPath), headerLineNumber, inputWidth));
                    if (sampleCount > 1000000)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): {2} samples is too many (limit 1,000,000).",
                                Path.GetFileName(fullPath), headerLineNumber, sampleCount));
                    if ((long)sampleCount * (inputCount + targetCount) > 100000000L)
                        throw new InvalidDataException(
                            string.Format(CultureInfo.InvariantCulture,
                                "{0} (line {1}): the declared matrix is too large ({2} samples x {3} values). The limit is 100,000,000 values.",
                                Path.GetFileName(fullPath), headerLineNumber, sampleCount, inputCount + targetCount));

                    headerFound = true;
                    continue;
                }

                tokenRows.Add(new TokenRow(i + 1, tokens));
            }

            if (!headerFound)
                throw new InvalidDataException(
                    "The .cal file does not contain a header line. Expected: <samples> <inputs> <targets> <inputWidth>.");

            if (tokenRows.Count != sampleCount)
                throw new InvalidDataException(
                    string.Format(CultureInfo.InvariantCulture,
                        "{0}: the header (line {1}) declares {2} samples, but {3} data row(s) were found.",
                        Path.GetFileName(fullPath), headerLineNumber, sampleCount, tokenRows.Count));

            int expectedFields = inputCount + targetCount;
            var parsedInputs = new double[sampleCount][];
            var parsedTargets = new double[sampleCount][];
            for (int r = 0; r < sampleCount; r++)
            {
                TokenRow row = tokenRows[r];
                if (row.Tokens.Length != expectedFields)
                    throw new InvalidDataException(
                        string.Format(CultureInfo.InvariantCulture,
                            "{0} (line {1}): expected {2} numbers ({3} input(s) + {4} target(s)), but found {5}.",
                            Path.GetFileName(fullPath), row.LineNumber, expectedFields,
                            inputCount, targetCount, row.Tokens.Length));

                var inputs = new double[inputCount];
                var targets = new double[targetCount];
                for (int c = 0; c < inputCount; c++)
                    inputs[c] = ParseDataValue(row.Tokens[c], row.LineNumber, c + 1, "input", fullPath);
                for (int c = 0; c < targetCount; c++)
                    targets[c] = ParseDataValue(row.Tokens[inputCount + c], row.LineNumber, inputCount + c + 1, "target", fullPath);

                parsedInputs[r] = inputs;
                parsedTargets[r] = targets;
            }

            // Feature ranges. Constant features are centered at 0.5 later on.
            var mins = new double[inputCount];
            var maxs = new double[inputCount];
            for (int c = 0; c < inputCount; c++)
            {
                mins[c] = parsedInputs[0][c];
                maxs[c] = parsedInputs[0][c];
            }
            for (int r = 1; r < sampleCount; r++)
            {
                for (int c = 0; c < inputCount; c++)
                {
                    double v = parsedInputs[r][c];
                    if (v < mins[c]) mins[c] = v;
                    if (v > maxs[c]) maxs[c] = v;
                }
            }

            bool oneHot = true;
            for (int r = 0; r < sampleCount && oneHot; r++)
            {
                int ones = 0;
                for (int c = 0; c < targetCount; c++)
                {
                    double t = parsedTargets[r][c];
                    if (Math.Abs(t - 1.0) < 1e-9) ones++;
                    else if (Math.Abs(t) > 1e-9) oneHot = false;
                }
                if (ones != 1) oneHot = false;
            }

            // Commit only after the whole file parsed successfully, so a bad
            // file never destroys a previously loaded data set.
            rawInputs = parsedInputs;
            rawTargets = parsedTargets;
            featureMin = mins;
            featureMax = maxs;
            numberOfSamples = sampleCount;
            numberOfInputs = inputCount;
            numberOfTargets = targetCount;
            declaredInputWidth = inputWidth;
            targetsAreOneHot = oneHot;
            sourcePath = fullPath;

            configured = false;
            hiddenLayerSizes = new int[0];
            layerSizes = new int[0];
            weights = null;
            momentumDelta = null;
            activations = null;
            deltas = null;
            dataIndices = new int[0];
            trainingIndices = new int[0];
            testIndices = new int[0];
            history.Clear();
            confusion = new int[targetCount, targetCount];
            currentEpoch = 0;
            eta = initialEta;
            testMse = 0.0;
            testRmse = 0.0;
            trainingMseLast = 0.0;
            accuracy = 0.0;
            correctPredictions = 0;
            hasEvaluation = false;
            finished = false;
            cancelRequested = false;
            displaySampleIndex = 0;

            if (declaredInputWidth != inputCount)
            {
                // The fourth header field is metadata in the original format.
                // The real column count always comes from the second field.
                System.Diagnostics.Debug.WriteLine(
                    "Declared input width (" + declaredInputWidth + ") differs from the input count (" + inputCount + ").");
            }
        }

        /// <summary>Legacy alias for <see cref="ReadData"/>.</summary>
        public void Read_Data(string filePath)
        {
            ReadData(filePath);
        }

        private static int ParseHeaderInt(string token, string what, int lineNumber, string filePath)
        {
            int value;
            if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                throw new InvalidDataException(
                    string.Format(CultureInfo.InvariantCulture,
                        "{0} (line {1}): the {2} '{3}' is not a valid integer.",
                        Path.GetFileName(filePath), lineNumber, what, token));
            return value;
        }

        private static double ParseDataValue(string token, int lineNumber, int column, string what, string filePath)
        {
            double value;
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidDataException(
                    string.Format(CultureInfo.InvariantCulture,
                        "{0} (line {1}, column {2}): {3} value '{4}' is not a finite number. Use '.' as the decimal separator.",
                        Path.GetFileName(filePath), lineNumber, column, what, token));
            return value;
        }

        private static string StripComment(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            int hash = line.IndexOf('#');
            int slash = line.IndexOf("//", StringComparison.Ordinal);
            int cut = -1;
            if (hash >= 0) cut = hash;
            if (slash >= 0 && (cut < 0 || slash < cut)) cut = slash;
            return cut >= 0 ? line.Substring(0, cut) : line;
        }

        private static string[] SplitFields(string line)
        {
            return line.Split(new[] { ' ', '\t', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private sealed class TokenRow
        {
            public TokenRow(int lineNumber, string[] tokens)
            {
                LineNumber = lineNumber;
                Tokens = tokens;
            }

            public int LineNumber { get; }
            public string[] Tokens { get; }
        }

        // ==================================================================
        // Configuration / reset
        // ==================================================================
        /// <summary>Creates a network with the supplied hidden-layer sizes and random weights.</summary>
        public void Configure(int[] hiddenSizes)
        {
            EnsureData();
            ValidateHiddenSizes(hiddenSizes);

            hiddenLayerSizes = (int[])hiddenSizes.Clone();
            int layerCount = hiddenLayerSizes.Length + 2;
            layerSizes = new int[layerCount];
            layerSizes[0] = numberOfInputs;
            for (int i = 0; i < hiddenLayerSizes.Length; i++)
                layerSizes[i + 1] = hiddenLayerSizes[i];
            layerSizes[layerCount - 1] = numberOfTargets;

            weights = new double[layerCount][][];
            momentumDelta = new double[layerCount][][];
            activations = new double[layerCount][];
            deltas = new double[layerCount][];

            for (int l = 0; l < layerCount; l++)
            {
                activations[l] = new double[layerSizes[l]];
                deltas[l] = new double[layerSizes[l]];
            }

            // Reproducible initialization: a fresh Random seeded from RandomSeed
            // means the same architecture and seed always start identically.
            var initRandom = new Random(randomSeed);
            for (int l = 1; l < layerCount; l++)
            {
                int fanIn = layerSizes[l - 1];
                int fanOut = layerSizes[l];
                weights[l] = new double[fanOut][];
                momentumDelta[l] = new double[fanOut][];
                double limit = Math.Sqrt(6.0 / (fanIn + fanOut)); // Glorot-style range
                for (int j = 0; j < fanOut; j++)
                {
                    weights[l][j] = new double[fanIn + 1];
                    momentumDelta[l][j] = new double[fanIn + 1];
                    weights[l][j][0] = 0.0; // bias starts at zero
                    for (int q = 1; q <= fanIn; q++)
                        weights[l][j][q] = (initRandom.NextDouble() * 2.0 - 1.0) * limit;
                }
            }

            configured = true;
            hasEvaluation = false;
            testMse = 0.0;
            testRmse = 0.0;
            accuracy = 0.0;
            correctPredictions = 0;
            confusion = new int[numberOfTargets, numberOfTargets];
            displaySampleIndex = 0;
        }

        /// <summary>
        /// Applies the hidden-layer layout, re-initializes all weights and
        /// rebuilds the reproducible train/test split.
        /// </summary>
        public void Reset(int[] hiddenSizes)
        {
            EnsureData();
            ValidateHiddenSizes(hiddenSizes);

            Configure(hiddenSizes);            // Separate stream so the split does not depend on how many weight
            // values were drawn.
            shuffleRandom = new Random(unchecked(randomSeed * 486187739 + 17));

            dataIndices = Enumerable.Range(0, numberOfSamples).ToArray();
            Shuffle(dataIndices, shuffleRandom);

            int trainCount = (int)Math.Round(trainingRatio * numberOfSamples, MidpointRounding.AwayFromZero);
            trainCount = Math.Max(1, Math.Min(numberOfSamples - 1, trainCount));
            trainingIndices = dataIndices.Take(trainCount).ToArray();
            testIndices = dataIndices.Skip(trainCount).ToArray();

            eta = initialEta;
            currentEpoch = 0;
            finished = false;
            cancelRequested = false;
            trainingMseLast = 0.0;
            history.Clear();
            confusion = new int[numberOfTargets, numberOfTargets];
            correctPredictions = 0;
            accuracy = 0.0;
            testMse = 0.0;
            testRmse = 0.0;
            hasEvaluation = false;
            displaySampleIndex = testIndices.Length > 0 ? testIndices[0] : trainingIndices[0];
        }

        /// <summary>Legacy alias for <see cref="Reset"/>.</summary>
        public void Reset_Weights_And_Initial_Condition(int[] hiddenNeuronNumbers)
        {
            Reset(hiddenNeuronNumbers);
        }

        /// <summary>Re-applies the current hidden layout and seed.</summary>
        public void Reset()
        {
            if (hiddenLayerSizes.Length == 0)
                throw new InvalidOperationException("No hidden-layer layout has been chosen yet.");
            Reset(hiddenLayerSizes);
        }

        public int[] SuggestHiddenSizes()
        {
            int suggested = Math.Max(2, Math.Min(32, 2 * Math.Max(1, numberOfTargets) + 2));
            return new[] { suggested };
        }

        private void ValidateHiddenSizes(int[] hiddenSizes)
        {
            if (hiddenSizes == null)
                throw new ArgumentNullException(nameof(hiddenSizes), "A hidden-layer layout is required.");
            if (hiddenSizes.Length < 1)
                throw new ArgumentException("At least one hidden layer is required.", nameof(hiddenSizes));
            if (hiddenSizes.Length > MaxHiddenLayers)
                throw new ArgumentException("At most " + MaxHiddenLayers + " hidden layers are supported.", nameof(hiddenSizes));
            for (int i = 0; i < hiddenSizes.Length; i++)
            {
                if (hiddenSizes[i] < MinNodesPerLayer || hiddenSizes[i] > MaxNodesPerLayer)
                    throw new ArgumentException(
                        "Hidden layer " + (i + 1) + " must contain between " + MinNodesPerLayer +
                        " and " + MaxNodesPerLayer + " nodes (found " + hiddenSizes[i] + ").",
                        nameof(hiddenSizes));
            }
        }

        private void EnsureData()
        {
            if (!IsDataLoaded)
                throw new InvalidOperationException("No data is loaded. Open a .cal file first.");
        }

        private void EnsureConfigured()
        {
            EnsureData();
            if (!configured)
                throw new InvalidOperationException("The network is not configured. Open data and press Reset first.");
        }

        // ==================================================================
        // Training
        // ==================================================================
        /// <summary>Runs exactly one training epoch and evaluates the test set.</summary>
        public EpochStatistics TrainOneEpoch()
        {
            EnsureConfigured();
            if (currentEpoch >= epochLimit)
            {
                finished = true;
                return history.Count > 0 ? history[history.Count - 1] : null;
            }

            Shuffle(trainingIndices, shuffleRandom);

            int lastLayer = layerSizes.Length - 1;
            double sse = 0.0;
            long valueCount = 0;

            for (int t = 0; t < trainingIndices.Length; t++)
            {
                int sample = trainingIndices[t];
                Forward(sample);
                ComputeDeltas(sample);
                ApplyWeightUpdate();

                double[] target = rawTargets[sample];
                double[] output = activations[lastLayer];
                for (int k = 0; k < numberOfTargets; k++)
                {
                    double diff = target[k] - output[k];
                    sse += diff * diff;
                    valueCount++;
                }
            }

            trainingMseLast = valueCount > 0 ? sse / valueCount : 0.0;
            currentEpoch++;
            EvaluateTestSet();
            eta *= etaDecay;

            var stats = new EpochStatistics(currentEpoch, trainingMseLast, testMse, testRmse, accuracy, eta);
            history.Add(stats);
            finished = currentEpoch >= epochLimit;
            return stats;
        }

        /// <summary>Trains until the epoch limit, cancellation or completion.</summary>
        public void TrainToEnd()
        {
            EnsureConfigured();
            while (currentEpoch < epochLimit)
            {
                TrainOneEpoch();
                if (cancelRequested)
                    break;
            }
            cancelRequested = false;
            if (currentEpoch >= epochLimit)
                finished = true;
        }

        /// <summary>Trains at most <paramref name="epochs"/> additional epochs.</summary>
        public void TrainEpochs(int epochs)
        {
            EnsureConfigured();
            if (epochs < 0)
                throw new ArgumentOutOfRangeException(nameof(epochs), "The number of epochs cannot be negative.");
            for (int i = 0; i < epochs && currentEpoch < epochLimit; i++)
            {
                TrainOneEpoch();
                if (cancelRequested)
                    break;
            }
            cancelRequested = false;
        }

        /// <summary>Requests a cooperative stop after the current epoch.</summary>
        public void RequestCancel()
        {
            cancelRequested = true;
        }

        private void Forward(int sampleIndex)
        {
            double[] input = rawInputs[sampleIndex];
            double[] firstLayer = activations[0];
            for (int i = 0; i < numberOfInputs; i++)
                firstLayer[i] = Normalize(i, input[i]);
            ForwardLayers();
        }

        private void ForwardRaw(double[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (input.Length != numberOfInputs)
                throw new ArgumentException("Expected " + numberOfInputs + " input value(s), but received " + input.Length + ".", nameof(input));

            for (int i = 0; i < numberOfInputs; i++)
            {
                double value = input[i];
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentException("Input value " + (i + 1) + " is not a finite number.", nameof(input));
                activations[0][i] = Normalize(i, value);
            }
            ForwardLayers();
        }

        private void ForwardLayers()
        {
            for (int l = 1; l < layerSizes.Length; l++)
            {
                double[] previous = activations[l - 1];
                double[] current = activations[l];
                double[][] layerWeights = weights[l];
                for (int j = 0; j < current.Length; j++)
                {
                    double[] row = layerWeights[j];
                    double z = row[0]; // bias
                    for (int q = 1; q < row.Length; q++)
                        z += previous[q - 1] * row[q];
                    current[j] = Sigmoid(z);
                }
            }
        }

        /// <summary>
        /// Error terms (without the learning rate). The learning rate is applied
        /// exactly once, in <see cref="ApplyWeightUpdate"/>.
        /// </summary>
        private void ComputeDeltas(int sampleIndex)
        {
            int lastLayer = layerSizes.Length - 1;
            double[] target = rawTargets[sampleIndex];
            double[] output = activations[lastLayer];
            double[] outputDelta = deltas[lastLayer];
            for (int j = 0; j < outputDelta.Length; j++)
            {
                double a = output[j];
                outputDelta[j] = (target[j] - a) * a * (1.0 - a);
            }

            for (int l = lastLayer - 1; l >= 1; l--)
            {
                double[] layerDelta = deltas[l];
                double[] layerActivation = activations[l];
                double[][] nextWeights = weights[l + 1];
                double[] nextDelta = deltas[l + 1];
                for (int j = 0; j < layerDelta.Length; j++)
                {
                    double sum = 0.0;
                    for (int s = 0; s < nextDelta.Length; s++)
                        sum += nextDelta[s] * nextWeights[s][j + 1];
                    double a = layerActivation[j];
                    layerDelta[j] = a * (1.0 - a) * sum;
                }
            }
        }

        private void ApplyWeightUpdate()
        {
            for (int l = 1; l < layerSizes.Length; l++)
            {
                double[][] layerWeights = weights[l];
                double[][] previousVelocity = momentumDelta[l];
                double[] layerDelta = deltas[l];
                double[] previousActivation = activations[l - 1];

                for (int j = 0; j < layerWeights.Length; j++)
                {
                    double[] row = layerWeights[j];
                    double[] velocityRow = previousVelocity[j];
                    double scaledDelta = eta * layerDelta[j];

                    if (momentum > 0.0)
                    {
                        velocityRow[0] = momentum * velocityRow[0] + scaledDelta;
                        row[0] += velocityRow[0];
                    }
                    else
                    {
                        row[0] += scaledDelta;
                    }

                    for (int q = 1; q < row.Length; q++)
                    {
                        double step = scaledDelta * previousActivation[q - 1];
                        if (momentum > 0.0)
                        {
                            velocityRow[q] = momentum * velocityRow[q] + step;
                            row[q] += velocityRow[q];
                        }
                        else
                        {
                            row[q] += step;
                        }
                    }
                }
            }
        }

        private static double Sigmoid(double z)
        {
            if (z >= 0.0)
            {
                double e = Math.Exp(-z);
                return 1.0 / (1.0 + e);
            }
            double e2 = Math.Exp(z);
            return e2 / (1.0 + e2);
        }

        private double Normalize(int feature, double value)
        {
            double min = featureMin[feature];
            double range = featureMax[feature] - min;
            if (range <= 1e-12)
                return 0.5; // constant feature: no information, stay neutral
            return (value - min) / range;
        }

        // ==================================================================
        // Evaluation / classification
        // ==================================================================
        /// <summary>
        /// Computes MSE/RMSE and the confusion matrix on the test split in a
        /// single forward pass. Handles the empty-test-set case explicitly.
        /// </summary>
        public void EvaluateTestSet()
        {
            EnsureConfigured();

            int classes = numberOfTargets;
            confusion = new int[classes, classes];
            correctPredictions = 0;
            testMse = 0.0;
            double sse = 0.0;
            long valueCount = 0;
            int lastLayer = layerSizes.Length - 1;

            for (int t = 0; t < testIndices.Length; t++)
            {
                int sample = testIndices[t];
                Forward(sample);

                double[] target = rawTargets[sample];
                double[] output = activations[lastLayer];
                for (int k = 0; k < classes; k++)
                {
                    double diff = target[k] - output[k];
                    sse += diff * diff;
                    valueCount++;
                }

                int guess = ArgMax(output);
                int actual = ArgMax(target);
                confusion[actual, guess]++;
                if (guess == actual)
                    correctPredictions++;
            }

            testMse = valueCount > 0 ? sse / valueCount : 0.0;
            testRmse = Math.Sqrt(Math.Max(0.0, testMse));
            accuracy = testIndices.Length > 0 ? correctPredictions / (double)testIndices.Length : 0.0;
            hasEvaluation = true;
        }

        /// <summary>
        /// Runs the classification test and returns the number of correctly
        /// classified test samples. Never divides by zero.
        /// </summary>
        public float TestingClassification()
        {
            EvaluateTestSet();
            return correctPredictions;
        }

        /// <summary>Predicts class probabilities for one raw feature vector.</summary>
        public double[] Predict(double[] rawInput)
        {
            EnsureConfigured();
            ForwardRaw(rawInput);
            return (double[])activations[layerSizes.Length - 1].Clone();
        }

        /// <summary>Predicts the class index (arg-max of the output layer).</summary>
        public int PredictClass(double[] rawInput)
        {
            double[] scores = Predict(rawInput);
            return ArgMax(scores);
        }

        /// <summary>
        /// Returns the squared error per target for one sample, preserving the
        /// legacy helper signature.
        /// </summary>
        public float[] ComputeResults(int dataIndex)
        {
            EnsureConfigured();
            if (dataIndex < 0 || dataIndex >= numberOfSamples)
                throw new ArgumentOutOfRangeException(nameof(dataIndex),
                    "Sample index must be between 0 and " + (numberOfSamples - 1) + ".");
            Forward(dataIndex);
            double[] output = activations[layerSizes.Length - 1];
            double[] target = rawTargets[dataIndex];
            var result = new float[numberOfTargets];
            for (int i = 0; i < numberOfTargets; i++)
            {
                double diff = target[i] - output[i];
                result[i] = (float)(diff * diff);
            }
            return result;
        }

        /// <summary>Returns a copy of the network output for one sample.</summary>
        public double[] ComputeOutput(int dataIndex)
        {
            EnsureConfigured();
            if (dataIndex < 0 || dataIndex >= numberOfSamples)
                throw new ArgumentOutOfRangeException(nameof(dataIndex),
                    "Sample index must be between 0 and " + (numberOfSamples - 1) + ".");
            Forward(dataIndex);
            return (double[])activations[layerSizes.Length - 1].Clone();
        }

        private static int ArgMax(double[] values)
        {
            if (values == null || values.Length == 0)
                return 0;
            int best = 0;
            double bestValue = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                // Strict comparison: ties deterministically resolve to the
                // lower class index instead of being ignored.
                if (values[i] > bestValue)
                {
                    bestValue = values[i];
                    best = i;
                }
            }
            return best;
        }

        private static void Shuffle(int[] array, Random rng)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = array[i];
                array[i] = array[j];
                array[j] = tmp;
            }
        }

        // ==================================================================
        // Text helpers
        // ==================================================================
        public string GetConfusionMatrixText()
        {
            if (!IsDataLoaded)
                return "(no data loaded)";

            int classes = numberOfTargets;
            var sb = new StringBuilder();
            sb.AppendLine("Confusion matrix (rows = actual class, columns = predicted class):");
            sb.Append("actual \\ pred");
            for (int j = 0; j < classes; j++)
                sb.AppendFormat(CultureInfo.InvariantCulture, " {0,6}", "c" + j);
            sb.AppendLine();
            for (int i = 0; i < classes; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "actual c{0}   ", i);
                for (int j = 0; j < classes; j++)
                    sb.AppendFormat(CultureInfo.InvariantCulture, " {0,6}", confusion[i, j]);
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }

        public string GetSummaryText()
        {
            if (!IsDataLoaded)
                return "No data loaded.";
            return string.Format(CultureInfo.InvariantCulture,
                "Network {0} | samples {1} | train {2} | test {3} | inputs {4} | classes {5}",
                NetworkShape, numberOfSamples, trainingIndices.Length, testIndices.Length,
                numberOfInputs, numberOfTargets);
        }

        /// <summary>Plain-text report used by demo mode and for verification.</summary>
        public string BuildReport(string dataFilePath, TimeSpan elapsed)
        {
            var sb = new StringBuilder();
            sb.AppendLine("MLP Demo Report");
            sb.AppendLine("===============");
            sb.AppendLine("Generated:        " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            sb.AppendLine("Data file:        " + (string.IsNullOrEmpty(dataFilePath) ? sourcePath : dataFilePath));
            sb.AppendLine("Samples:          " + numberOfSamples);
            sb.AppendLine("Inputs:           " + numberOfInputs);
            sb.AppendLine("Targets/classes:  " + numberOfTargets);
            sb.AppendLine("One-hot targets:  " + (targetsAreOneHot ? "yes" : "no"));
            sb.AppendLine("Training samples: " + trainingIndices.Length);
            sb.AppendLine("Test samples:     " + testIndices.Length);
            sb.AppendLine("Network shape:    " + NetworkShape + " (sigmoid)");
            sb.AppendLine("Seed:             " + randomSeed);
            sb.AppendLine("Initial eta:      " + initialEta.ToString("0.####", CultureInfo.InvariantCulture));
            sb.AppendLine("Eta decay:        " + etaDecay.ToString("0.####", CultureInfo.InvariantCulture) + " per epoch");
            sb.AppendLine("Momentum:         " + momentum.ToString("0.####", CultureInfo.InvariantCulture));
            sb.AppendLine("Epochs completed: " + currentEpoch + " / " + epochLimit);
            sb.AppendLine("Final eta:        " + eta.ToString("0.######", CultureInfo.InvariantCulture));
            sb.AppendLine("Training time:    " + elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) + " s");
            sb.AppendLine("Training MSE:     " + trainingMseLast.ToString("0.######", CultureInfo.InvariantCulture));
            sb.AppendLine("Test MSE:         " + testMse.ToString("0.######", CultureInfo.InvariantCulture));
            sb.AppendLine("Test RMSE:        " + testRmse.ToString("0.######", CultureInfo.InvariantCulture));
            sb.AppendLine("Test accuracy:    " + (accuracy * 100.0).ToString("0.00", CultureInfo.InvariantCulture) + "% (" +
                              correctPredictions + "/" + testIndices.Length + ")");
            sb.AppendLine();
            sb.AppendLine(GetConfusionMatrixText());
            return sb.ToString();
        }

        // ==================================================================
        // Drawing
        // ==================================================================
        /// <summary>Refreshes the activations shown inside the network diagram.</summary>
        public void UpdateDisplayActivations()
        {
            if (!configured || !IsDataLoaded)
                return;
            if (displaySampleIndex < 0 || displaySampleIndex >= numberOfSamples)
                displaySampleIndex = 0;
            Forward(displaySampleIndex);
        }

        /// <summary>
        /// Draws the network inside <paramref name="bounds"/>. All indices are
        /// guarded, so unusual layer sizes cannot crash the paint handler.
        /// </summary>
        public void DrawMLP(Graphics g, Rectangle bounds)
        {
            if (g == null || !configured || layerSizes == null || layerSizes.Length < 2)
                return;
            if (bounds.Width < 80 || bounds.Height < 80)
                return;

            try
            {
                DrawNetwork(g, bounds);
            }
            catch (Exception)
            {
                // Painting must never take the application down.
            }
        }

        private void DrawNetwork(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int layerCount = layerSizes.Length;
            int lastLayer = layerCount - 1;
            const int labelHeight = 36;
            const int biasHeight = 26;

            float left = bounds.Left + 58f;
            float right = bounds.Right - 58f;
            if (right <= left)
            {
                left = bounds.Left + 20f;
                right = bounds.Right - 20f;
            }
            float top = bounds.Top + labelHeight;
            float bottom = bounds.Bottom - biasHeight;
            if (bottom <= top + 20f)
                return;

            var nodeX = new float[layerCount];
            var nodeY = new float[layerCount][];
            var nodeRadius = new float[layerCount];

            float columnStep = layerCount > 1 ? (right - left) / (layerCount - 1) : 0f;

            for (int l = 0; l < layerCount; l++)
            {
                nodeX[l] = layerCount > 1 ? left + columnStep * l : (left + right) / 2f;
                int count = layerSizes[l];
                nodeY[l] = new float[count];
                float vertical = (bottom - top) / (count + 1);
                for (int j = 0; j < count; j++)
                    nodeY[l][j] = top + vertical * (j + 1);

                float radius = Math.Min(22f, vertical * 0.34f);
                if (columnStep > 0f)
                    radius = Math.Min(radius, columnStep * 0.18f);
                nodeRadius[l] = Math.Max(4f, radius);
            }

            // --- connections -------------------------------------------------
            float maxAbsWeight = 1e-9f;
            long connectionCount = 0;
            for (int l = 1; l < layerCount; l++)
            {
                connectionCount += (long)layerSizes[l] * (layerSizes[l - 1] + 1);
                for (int j = 0; j < layerSizes[l]; j++)
                {
                    for (int q = 0; q < weights[l][j].Length; q++)
                    {
                        float a = (float)Math.Abs(weights[l][j][q]);
                        if (a > maxAbsWeight) maxAbsWeight = a;
                    }
                }
            }

            if (connectionCount <= 4000)
            {
                for (int l = 1; l < layerCount; l++)
                {
                    for (int j = 0; j < layerSizes[l]; j++)
                    {
                        float targetX = nodeX[l];
                        float targetY = nodeY[l][j];
                        for (int q = 0; q < weights[l][j].Length; q++)
                        {
                            float sourceX, sourceY;
                            if (q == 0)
                            {
                                sourceX = nodeX[l - 1];
                                sourceY = bounds.Bottom - biasHeight / 2f;
                            }
                            else
                            {
                                sourceX = nodeX[l - 1];
                                sourceY = nodeY[l - 1][q - 1];
                            }

                            double w = weights[l][j][q];
                            float norm = (float)Math.Min(1.0, Math.Abs(w) / maxAbsWeight);
                            int alpha = 35 + (int)(150 * norm);
                            if (alpha < 10) alpha = 10;
                            Color color = w >= 0.0
                                ? Color.FromArgb(alpha, 24, 116, 60)
                                : Color.FromArgb(alpha, 170, 40, 40);
                            float width = 0.6f + 1.7f * norm;
                            using (var pen = new Pen(color, width))
                            {
                                if (q == 0)
                                    pen.DashStyle = DashStyle.Dot;
                                g.DrawLine(pen, sourceX, sourceY, targetX, targetY);
                            }
                        }
                    }
                }
            }

            using (var layerFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var nodeFont = new Font("Segoe UI", 7.5f))
            using (var smallFont = new Font("Segoe UI", 6.5f))
            using (var centered = new StringFormat())
            {
                centered.Alignment = StringAlignment.Center;
                centered.LineAlignment = StringAlignment.Center;

                // --- layer captions ------------------------------------------
                for (int l = 0; l < layerCount; l++)
                {
                    string caption = l == 0
                        ? "Input\n" + layerSizes[l]
                        : (l == lastLayer ? "Output\n" + layerSizes[l] : "Hidden " + l + "\n" + layerSizes[l]);
                    var captionRect = new RectangleF(nodeX[l] - 55f, bounds.Top + 2f, 110f, labelHeight - 4f);
                    g.DrawString(caption, layerFont, Brushes.DimGray, captionRect, centered);
                }

                // --- bias markers --------------------------------------------
                using (var biasBrush = new SolidBrush(Color.FromArgb(220, 108, 92, 231)))
                using (var biasPen = new Pen(Color.FromArgb(160, 80, 60, 180)))
                {
                    for (int l = 0; l < lastLayer; l++)
                    {
                        float size = 9f;
                        var biasRect = new RectangleF(nodeX[l] - size / 2f, bounds.Bottom - biasHeight / 2f - size / 2f, size, size);
                        g.FillRectangle(biasBrush, biasRect);
                        g.DrawRectangle(biasPen, biasRect.X, biasRect.Y, biasRect.Width, biasRect.Height);
                        g.DrawString("b", smallFont, Brushes.DimGray,
                            new RectangleF(nodeX[l] - 20f, bounds.Bottom - biasHeight / 2f + 5f, 40f, 14f), centered);
                    }
                }

                // --- nodes ----------------------------------------------------
                int predicted = hasEvaluation ? ArgMax(activations[lastLayer]) : -1;
                for (int l = 0; l < layerCount; l++)
                {
                    for (int j = 0; j < layerSizes[l]; j++)
                    {
                        float radius = nodeRadius[l];
                        float cx = nodeX[l];
                        float cy = nodeY[l][j];
                        var rect = new RectangleF(cx - radius, cy - radius, radius * 2f, radius * 2f);

                        Color fillColor;
                        if (l == 0) fillColor = Color.FromArgb(214, 234, 248);
                        else if (l == lastLayer)
                            fillColor = (j == predicted && hasEvaluation)
                                ? Color.FromArgb(255, 183, 77)
                                : Color.FromArgb(212, 245, 220);
                        else fillColor = Color.FromArgb(255, 244, 200);

                        using (var fill = new SolidBrush(fillColor))
                        using (var outline = new Pen(l == lastLayer && j == predicted && hasEvaluation
                            ? Color.FromArgb(230, 126, 34)
                            : Color.FromArgb(120, 120, 120),
                            l == lastLayer && j == predicted && hasEvaluation ? 2f : 1f))
                        {
                            g.FillEllipse(fill, rect);
                            g.DrawEllipse(outline, rect);
                        }

                        if (radius >= 13f)
                        {
                            string label;
                            if (l == 0)
                                label = rawInputs[displaySampleIndex][j].ToString("0.##", CultureInfo.InvariantCulture);
                            else
                                label = activations[l][j].ToString("0.00", CultureInfo.InvariantCulture);
                            Font font = radius >= 18f ? nodeFont : smallFont;
                            g.DrawString(label, font, Brushes.Black, rect, centered);
                        }
                    }
                }
            }
        }
    }
}


