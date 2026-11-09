using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MlpWorkbench
{
    /// <summary>
    /// Command-line options used by the unattended demo mode.
    ///
    ///   --demo                load the bundled iris sample, train and leave the
    ///                         window open with the chart, network and matrix
    ///   --run N               train N epochs (0 or omitted = train to the limit)
    ///   --data PATH           use another .cal file instead of samples/iris.cal
    ///   --hidden 8,6          hidden-layer layout (default: suggested layout)
    ///   --seed N              random seed (default 42)
    ///   --eta X               initial learning rate (default 0.30)
    ///   --decay X             per-epoch eta decay (default 0.99)
    ///   --momentum X          momentum in [0, 1) (default 0.90)
    ///   --ratio X             training split ratio (default 0.70)
    ///   --report PATH         write a plain-text report after training
    /// </summary>
    public sealed class DemoOptions
    {
        public bool Enabled { get; set; }
        public bool ShowUsageOnly { get; set; }
        public int Epochs { get; set; }
        public int[] HiddenSizes { get; set; }
        public int Seed { get; set; } = 42;
        public double Eta { get; set; } = 0.30;
        public double Decay { get; set; } = 0.99;
        public double Momentum { get; set; } = 0.90;
        public double Ratio { get; set; } = 0.70;
        public string DataPath { get; set; }
        public string ReportPath { get; set; }

        public const string Usage =
            "MLP Classifier command-line options:\n\n" +
            "  --demo              Load the bundled iris sample, train, and show the results.\n" +
            "  --run N             Number of epochs to train in demo mode (0 = to the epoch limit).\n" +
            "  --data PATH         Use another .cal data file instead of samples/iris.cal.\n" +
            "  --hidden 8,6        Hidden-layer layout (comma separated).\n" +
            "  --seed N            Random seed (default 42).\n" +
            "  --eta X             Initial learning rate (default 0.30).\n" +
            "  --decay X           Per-epoch eta decay (default 0.99).\n" +
            "  --momentum X        Momentum in [0, 1) (default 0.90).\n" +
            "  --ratio X           Training split ratio (default 0.70).\n" +
            "  --report PATH       Write a plain-text training report.\n\n" +
            "Without arguments the normal interactive WinForms application starts.";

        public static DemoOptions Parse(string[] args)
        {
            var options = new DemoOptions();
            if (args == null || args.Length == 0)
                return options;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i] ?? string.Empty;
                switch (arg.ToLowerInvariant())
                {
                    case "--demo":
                        options.Enabled = true;
                        break;
                    case "--help":
                    case "-h":
                    case "/?":
                        options.ShowUsageOnly = true;
                        return options;
                    case "--run":
                        options.Epochs = ParseInt(NextValue(args, ref i, arg), arg, 0, 1000000);
                        break;
                    case "--data":
                        options.DataPath = NextValue(args, ref i, arg);
                        break;
                    case "--report":
                        options.ReportPath = NextValue(args, ref i, arg);
                        break;
                    case "--seed":
                        options.Seed = ParseInt(NextValue(args, ref i, arg), arg, int.MinValue, int.MaxValue);
                        break;
                    case "--eta":
                        options.Eta = ParseDouble(NextValue(args, ref i, arg), arg, 0.0001, 10.0);
                        break;
                    case "--decay":
                        options.Decay = ParseDouble(NextValue(args, ref i, arg), arg, 0.0001, 1.0);
                        break;
                    case "--momentum":
                        options.Momentum = ParseDouble(NextValue(args, ref i, arg), arg, 0.0, 0.999999);
                        break;
                    case "--ratio":
                        options.Ratio = ParseDouble(NextValue(args, ref i, arg), arg, 0.01, 0.99);
                        break;
                    case "--hidden":
                        options.HiddenSizes = ParseHiddenSizes(NextValue(args, ref i, arg));
                        break;
                    default:
                        throw new ArgumentException("Unknown command-line option '" + arg + "'.\n\n" + Usage);
                }
            }

            return options;
        }

        private static string NextValue(string[] args, ref int index, string option)
        {
            if (index + 1 >= args.Length)
                throw new ArgumentException("Option '" + option + "' requires a value.\n\n" + Usage);
            index++;
            return args[index];
        }

        private static int ParseInt(string text, string option, int min, int max)
        {
            int value;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                throw new ArgumentException("Option '" + option + "' expects an integer, but got '" + text + "'.");
            if (value < min || value > max)
                throw new ArgumentException("Option '" + option + "' must be between " + min + " and " + max + ".");
            return value;
        }

        private static double ParseDouble(string text, string option, double min, double max)
        {
            double value;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ||
                double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Option '" + option + "' expects a number, but got '" + text + "'.");
            if (value < min || value > max)
                throw new ArgumentException("Option '" + option + "' must be between " + min + " and " + max + ".");
            return value;
        }

        private static int[] ParseHiddenSizes(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Option '--hidden' expects at least one node count, for example 8 or 8,6.");

            string[] parts = text.Split(new[] { ',', 'x', 'X', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var sizes = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                int size;
                if (!int.TryParse(parts[i].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out size))
                    throw new ArgumentException("Option '--hidden' contains an invalid node count: '" + parts[i] + "'.");
                if (size < MLP.MinNodesPerLayer || size > MLP.MaxNodesPerLayer)
                    throw new ArgumentException(
                        "Hidden layer node counts must be between " + MLP.MinNodesPerLayer + " and " + MLP.MaxNodesPerLayer + ".");
                sizes[i] = size;
            }
            if (sizes.Length > MLP.MaxHiddenLayers)
                throw new ArgumentException("At most " + MLP.MaxHiddenLayers + " hidden layers are supported.");
            return sizes;
        }
    }

    internal static class SampleLocator
    {
        /// <summary>
        /// Finds a bundled sample next to the executable or anywhere up the
        /// directory tree from the executable / current directory.
        /// </summary>
        public static string FindSample(string fileName)
        {
            var candidates = new List<string>();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            candidates.Add(Path.Combine(baseDirectory, "samples", fileName));

            var directory = new DirectoryInfo(baseDirectory);
            for (int i = 0; i < 8 && directory != null; i++)
            {
                candidates.Add(Path.Combine(directory.FullName, "samples", fileName));
                directory = directory.Parent;
            }
            candidates.Add(Path.Combine(Environment.CurrentDirectory, "samples", fileName));
            candidates.Add(Path.Combine(Environment.CurrentDirectory, "..", "samples", fileName));

            return candidates
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(File.Exists);
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DemoOptions options;
            try
            {
                options = DemoOptions.Parse(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MLP Classifier - invalid arguments",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (options.ShowUsageOnly)
            {
                MessageBox.Show(DemoOptions.Usage, "MLP Classifier - help",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var form = new MainForm();
                if (options.Enabled)
                    form.QueueDemo(options);
                Application.Run(form);
            }
            catch (Exception ex)
            {
                try
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mlp-crash.log");
                    File.WriteAllText(logPath, DateTime.Now.ToString("o", CultureInfo.InvariantCulture) +
                                               Environment.NewLine + ex);
                }
                catch
                {
                    // Never let crash logging mask the original error.
                }

                MessageBox.Show("An unexpected error occurred:\n\n" + ex.Message,
                    "MLP Classifier", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

