using Neurotec.Biometrics;
using Neurotec.Biometrics.Client;
using Neurotec.Biometrics.Gui;
using Neurotec.IO;
using Neurotec.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;


namespace TwinsDay2017_VoiceDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

            #region Neurotec Variables

            // Neurotec constants for obtaining licenses
            public const int Port = 5000;
            public const string Address = "/local";
            private string[] voiceLicenseComponents = { "Biometrics.VoiceExtractionFast", "Biometrics.VoiceMatching" , "Media" };
            private static NBiometricClient mainClient;
            private NBiometricTask enrollment = new NBiometricTask(NBiometricOperations.Enroll);

            #endregion

            #region Subject Veriables

            // Identification Subjects
            private NSubject identSubject;
            private NSubject[] referenceSubject;

            // Verification (1:1) Subjects
            private NSubject verSubjectTop;
            private NSubject verSubjectBottom;

            // Verification (1:N) Subjects
            private NSubject verifySubject;

            #endregion

            #region Matching Lists/Dictionaries

            private List<Match> matchList;
            private Dictionary<string, Record> recordList;
            private Dictionary<string, Record> AllRecordList;
            private struct Record
            {
                //public List<string> directoryList;
                public List<string> voiceList;
            }

            #endregion

            #region Subject Scan Lists

            private List<string> identScanFiles;
            private List<string> topScanFiles;
            private List<string> bottomScanFiles;
            private List<string> verifyScanFiles;

            #endregion

            #region Settings and Default Variables

            // Settings Variables
            private string[] defaultMatches = { "5", "10", "15", "20", "30" };
            private double[] defaultFAR = { 0.01, 0.001, 0.0001, 0.00001 };
            private Settings curSettings;
            private Settings defaultSettings;

            private const double defMatchFAR = 0.01;
            private const int defNumMatches = 5;
            private const string defExt = ".wav";
            private const string FileFilter = "Audio Files|*.wav|Templates|*.dat";
            private static string TWD17_Server = @"D:\TWD17_SERVER\";
            private static string DataFolder = @"D:\TWD17_SERVER\TWD17 Applications (Deployable)\DEPLOY\Demo Data";
            private string TEMPLATEFOLDER = DataFolder + @"\Audio";
            private List<string> defaultDatabases = new List<string> { @"\\192.168.5.10\R\TwinsDay2014\Audio",
                                                                       @"\\192.168.5.10\R\TwinsDay 2015\Audio",
                                                                       @"\\192.168.5.10\X\Previous_Collections\Twins2016\ADIO",
                                                                       TWD17_Server + "ADIO" };
            
            #endregion

            #region Media Player Variables

            // Media Player Elements
            private MediaPlayer probePlayer;
            private MediaPlayer topPlayer;
            private MediaPlayer bottomPlayer;
            private MediaPlayer resultPlayer;
            private MediaPlayer verifyPlayer;
            private bool probeSlide;
            private bool topSlide;
            private bool bottomSlide;
            private bool resultSlide;
            private bool verifySlide;
            private DispatcherTimer probeTimer;
            private DispatcherTimer topTimer;
            private DispatcherTimer bottomTimer;
            private DispatcherTimer resultTimer;
            private DispatcherTimer verifyTimer;

            #endregion

            #region Backgroundworker/Process Variables

            // Background workers/processes
            private BackgroundWorker progress;
            private BackgroundWorker identifyProgress;
            private BackgroundWorker matchProgress;
            private int templateCount;
            private int matchCount;
            private int identifyCount;
            private int identifyMax;

            #endregion

            #region Other Variables

            // Static Elements
            private static int lineNum = 1;

            // Semaphores
            private Semaphore templateWriteHold;

            #endregion

        #endregion

        #region Main Window Constructor

            public MainWindow()
            {
                /* Default Settings Construction */
                defaultSettings = new Settings(defMatchFAR * 100, defaultDatabases, defNumMatches, GetScore(defMatchFAR));

                /* Current Settings Construction */
                curSettings = new Settings(defaultSettings);

                // Start the component initialization for the WPF/XAML application
                InitializeComponent();

                // Initialize the match list and set the corresponding listbox item source to the match list
                matchList = new List<Match>();
                TopMatchList.ItemsSource = matchList;

                // Initialize the subject record dictionary
                recordList = new Dictionary<string, Record>();
                AllRecordList = new Dictionary<string, Record>();

                // Initialize the scanner file record list(s)
                identScanFiles = new List<string>();
                topScanFiles = new List<string>();
                bottomScanFiles = new List<string>();
                verifyScanFiles = new List<string>();

                // Initialize all the media players
                probePlayer = new MediaPlayer();
                topPlayer = new MediaPlayer();
                bottomPlayer = new MediaPlayer();
                resultPlayer = new MediaPlayer();
                verifyPlayer = new MediaPlayer();

                probeTimer = new DispatcherTimer();
                topTimer = new DispatcherTimer();
                bottomTimer = new DispatcherTimer();
                verifyTimer = new DispatcherTimer();

                // Initialize the media opened methods
                probePlayer.MediaOpened += new EventHandler(probePlayer_MediaOpened);
                topPlayer.MediaOpened += new EventHandler(topPlayer_MediaOpened);
                bottomPlayer.MediaOpened += new EventHandler(bottomPlayer_MediaOpened);
                resultPlayer.MediaOpened += new EventHandler(resultPlayer_MediaOpened);
                verifyPlayer.MediaOpened += new EventHandler(verifyPlayer_MediaOpened);

                // Initialize background worker/progress bar worker
                progress = new BackgroundWorker();
                progress.WorkerReportsProgress = true;
                progress.WorkerSupportsCancellation = true;
                progress.DoWork += new DoWorkEventHandler(template_DoWork);
                progress.ProgressChanged += new ProgressChangedEventHandler(template_ProgressChanged);
                progress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(template_RunWorkerCompleted);

                identifyProgress = new BackgroundWorker();
                identifyProgress.WorkerReportsProgress = true;
                identifyProgress.WorkerSupportsCancellation = true;
                identifyProgress.DoWork += new DoWorkEventHandler(identify_DoWork);
                identifyProgress.ProgressChanged += new ProgressChangedEventHandler(identify_ProgressChanged);
                identifyProgress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(identify_RunWorkerCompleted);

                matchProgress = new BackgroundWorker();
                matchProgress.WorkerReportsProgress = true;
                matchProgress.WorkerSupportsCancellation = true;
                matchProgress.DoWork += new DoWorkEventHandler(matchProgress_DoWork);
                matchProgress.ProgressChanged += new ProgressChangedEventHandler(matchProgress_ProgressChanged);
                matchProgress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(matchProgress_RunWorkerCompleted);

                // Initialize any semaphores/thread holds we may use
                templateWriteHold = new Semaphore(1, 1);


                /* Loading default matching combobox items */
                // # of matches ComboBox
                for (int index = 0; index < defaultMatches.Length; ++index)
                    NumOfMatches.Items.Add(defaultMatches[index]);

                // FAR ComboBox
                for (int i = 0; i < defaultFAR.Length; i++)
                    setMatchFAR.Items.Add(defaultFAR[i] * 100);

                // Check access of each of the default databases
                foreach (string uncPath in defaultDatabases)
                    GetAccess(uncPath);

                // Set each of the settings text boxes to their respective values
                NumOfMatches.Text = defNumMatches.ToString();
                setMatchFAR.Text = (100 * defMatchFAR).ToString();
                setThreshold.Text = GetScore(defMatchFAR).ToString();
                ImportDatabases(defaultDatabases);

                // Make sure buttons are not enabled after loading the changes
                RevertSetting.IsEnabled = false;
                DefaultSetting.IsEnabled = false;

                Log("");
                Log("Window Initialized");

                /* Begin the license obtaining process and the initilization of the biometric client */
                Start();
            }

        #endregion

        #region Identification Functions

            #region Import/Clear/Reset Functions

                /// <summary>
                /// Starts the template import process for the probe image
                /// </summary>
                private void ImportFile(object sender, RoutedEventArgs e)
                {
                    identSubject = null;

                    resultPlayer.Close();
                    ResultRID.Content = "---";
                    matchList.Clear();
                    TopMatchList.Items.Refresh();
                    IdentificationReset.IsEnabled = false;

                    // Make sure that we are only using text-dependent extraction for our identification
                    mainClient.VoicesExtractTextDependentFeatures = true;
                    mainClient.VoicesExtractTextIndependentFeatures = false;
                    mainClient.VoicesUniquePhrasesOnly = false;

                    Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                    openPicker.DefaultExt = defExt;
                    openPicker.Filter = FileFilter;

                    if (openPicker.ShowDialog() == true)
                    {
                        IdentificationImportPath.Text = openPicker.FileName;
                        GetTemplate(openPicker.FileName, out identSubject);

                        if (IdentificationImportPath.Text != string.Empty)
                        {
                            // Media player functionality provided by wpf-tutorial.com
                            probePlayer.Open(new Uri(IdentificationImportPath.Text));
                            
                            probeTimer.Interval = TimeSpan.FromSeconds(1);
                            probeTimer.Tick += probeTick;

                            IdentificationRID.Content = "Playing: " + identSubject.Id;

                            EnableIdentify();

                            Log("Probe Voice Imported");

                            IdentificationClearWindow.IsEnabled = true;
                            IdentifyScan.IsEnabled = false;
                            IdentificationScanPath.IsEnabled = false;
                        }
                    }
                }

                /// <summary>
                /// Clears the identification tab/window
                /// </summary>
                private void IdentificationClearWindow_Click(object sender, RoutedEventArgs e)
                {
                    IdentificationImportPath.Text = string.Empty;
                    IdentificationRID.Content = "---";
                    ResultRID.Content = "---";

                    identSubject = null;
                    matchList.Clear();
                    TopMatchList.Items.Refresh();

                    probePlayer.Close();
                    resultPlayer.Close();

                    GenerateMatches.IsEnabled = false;
                    IdentificationClearWindow.IsEnabled = false;
                    IdentificationReset.IsEnabled = false;

                    IdentifyImport.IsEnabled = true;
                    IdentificationImportPath.IsEnabled = true;

                    IdentifyScan.IsEnabled = true;
                    IdentificationScanPath.IsEnabled = true;
                    IdentificationScanPath.Text = "";

                    identScanFiles.Clear();
                    IdentifyVoiceSelect.Items.Clear();
                    IdentifyVoiceSelect.IsEnabled = false;
                    IdentifySelect.IsEnabled = false;
                    IdentifyDeselect.IsEnabled = false;

                    Log("");
                    Log("Cleared Identification Page");
                }

                /// <summary>
                /// When the reset button is clicked; resets the match list
                /// </summary>
                private void IdentificationReset_Click(object sender, RoutedEventArgs e)
                {
                    ResultRID.Content = "---";
                    resultPlayer.Close();

                    matchList.Clear();
                    TopMatchList.Items.Refresh();

                    IdentificationReset.IsEnabled = false;

                    Log("");
                    Log("Reset Identification Results");
                }

            #endregion

            #region Main Process Functions

                /// <summary>
                /// The work method for the identification background process
                /// </summary>
                private void identify_DoWork(object sender, DoWorkEventArgs e)
                {
                    // Get all audio templates
                    string[] allTemplates = Directory.GetFiles(TEMPLATEFOLDER, "*.dat");

                    if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; }
                    else
                    {
                        identifyMax = allTemplates.Length;

                        this.Dispatcher.Invoke(() =>
                        {
                            // Clear list
                            matchList.Clear();
                            TopMatchList.Items.Refresh();

                            // Set the maximum for the identification progress bar
                            IdentificationStatus.Maximum = identifyMax + 40;
                        });

                        if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                        else
                        {
                            // Perform enrollment of all templates
                            EnrollTemplates(allTemplates);

                            // Update our identification background worker/progress bar
                            identifyCount += 20;
                            (sender as BackgroundWorker).ReportProgress(identifyCount, "Enrollment Completed");
                            Thread.Sleep(5);

                            if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                            else if (identSubject != null && referenceSubject != null && referenceSubject.Length > 0)
                            {
                                // Make sure that only text-dependent extraction happens
                                mainClient.VoicesExtractTextDependentFeatures = true;
                                mainClient.VoicesExtractTextIndependentFeatures = false;
                                mainClient.VoicesUniquePhrasesOnly = false;

                                // Matching threshold will be specified by the user input FAR/Threshold value
                                mainClient.MatchingThreshold = (byte)curSettings.GetThreshold();

                                // Start identification
                                mainClient.BeginIdentify(identSubject, AsyncIdentification, null);

                                identifyCount += 20;
                                (sender as BackgroundWorker).ReportProgress(identifyCount, "Performing Identification");
                                Thread.Sleep(5);
                            }
                        }
                    }
                }

                /// <summary>
                /// Method called when the identification background process reports progress
                /// </summary>
                private void identify_ProgressChanged(object sender, ProgressChangedEventArgs e)
                {
                    IdentificationStatus.Value = e.ProgressPercentage;
                    IdentStatusInfo.Text = "Progress: " + (string)e.UserState;
                }

                /// <summary>
                /// Method called when the identification background process finishes
                /// </summary>
                private void identify_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
                {
                    

                    if (e.Error != null)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Error.Message);
                        Log(string.Format("Error occurred in the identification process: {0}", e.Error.Message));
                    }
                    else if (e.Cancelled)
                    {
                        GenerateMatches.IsEnabled = true;
                        //IdentifyImport.IsEnabled = true;
                        //IdentificationImportPath.IsEnabled = true;
                        IdentificationReset.IsEnabled = true;

                        IdentificationStatus.Value = 0;
                        IdentStatusInfo.Text = "Progress: ";

                        
                        Log("Identification Canceled");
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("Operation Completed");
                        Log("Identification Completed");
                    }
                }

                /// <summary>
                /// Begins the identification process
                /// </summary>
                private void GenerateMatches_Click(object sender, RoutedEventArgs e)
                {
                    if(!CheckData())
                        System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                    // Check if background worker already has a job
                    else if (!identifyProgress.IsBusy)
                    {
                        // Clear the biometric client and start from scratch
                        mainClient.Clear();

                        Log("");
                        Log("Starting Identification");

                        mainClient.VoicesUniquePhrasesOnly = false;

                        GenerateMatches.IsEnabled = false;
                        //IdentifyImport.IsEnabled = false;
                        //IdentificationImportPath.IsEnabled = false;

                        IdentificationStatus.Value = 0;

                        // Start Asynchronous Process
                        identifyProgress.RunWorkerAsync();
                    }
                }

            #endregion

            #region Other Functions

                /// <summary>
                /// Checks to see if the Identify/Generate Matches button should be enabled
                /// </summary>
                private void EnableIdentify()
                {
                    GenerateMatches.IsEnabled = ValidSubject(identSubject);
                }

                /// <summary>
                /// Opens a new image window/popup
                /// </summary>
                private void OpenImage_Click(object sender, RoutedEventArgs e)
                {
                    int index = ((int)((System.Windows.Controls.Button)sender).Tag) - 1;

                    // Open image if there exists a face image/no placeholder image
                    if (!matchList[index].LargeImage.Equals("/Images/NoImage.Png"))
                    {
                        ImageDisplay resultDisplay = new ImageDisplay(matchList[index].LargeImage, matchList[index].ID);
                        this.Dispatcher.BeginInvoke(new Action(() => resultDisplay.Show()));
                    }
                }

            #endregion

        #endregion

        #region Matching Progress/Backgroundworker Functions

            /// <summary>
            /// Action Performed when the match background worker starts
            /// </summary>
            private void matchProgress_DoWork(object sender, DoWorkEventArgs e)
            {
                this.Dispatcher.Invoke(() =>
                {
                    // Reset progress bar to 0 and set maximum to # of matches and the # of matches we will display
                    IdentificationStatus.Value = 0;
                    IdentificationStatus.Maximum = identSubject.MatchingResults.Count + curSettings.GetNumMatches();

                    Log("");
                    Log("Adding Matches");
                });

                matchCount = 0;

                // Loop through all results
                foreach (NMatchingResult result in identSubject.MatchingResults)
                {
                    if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                    else
                    {
                        // Add result
                        AddMatch(result);

                        // Increment progress by 1 and reupdate progress bar
                        matchCount++;
                        matchProgress.ReportProgress(matchCount, "Adding Match - " + result.Id);
                        Thread.Sleep(1);
                    }
                }

                this.Dispatcher.Invoke(() =>
                {
                    Log("All Matches Added");
                    Log("");
                    Log("Displaying Matches");
                });

                if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                else { DisplayMatches(); }
            }

            /// <summary>
            /// Action Performed when the match background worker finishes
            /// </summary>
            private void matchProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                GenerateMatches.IsEnabled = true;
                IdentifyImport.IsEnabled = true;
                IdentificationImportPath.IsEnabled = true;
                IdentificationReset.IsEnabled = true;

                IdentificationStatus.Value = 0;
                IdentStatusInfo.Text = "Progress: ";
            }

            /// <summary>
            /// Action Performed when the match background worker reports progress
            /// </summary>
            private void matchProgress_ProgressChanged(object sender, ProgressChangedEventArgs e)
            {
                IdentificationStatus.Value = e.ProgressPercentage;
                IdentStatusInfo.Text = "Progress: " + (string)e.UserState;
            }

            /// <summary>
            /// Adds a matching result to the top match list
            /// </summary>
            /// <param name="newMatch">The NMatchingResult match</param>
            private void AddMatch(NMatchingResult newMatch)
            {
                int maxDisplay = curSettings.GetNumMatches();

                string file = GetTokenAudioPath(newMatch.Id);

                double far = GetFAR(newMatch.Score);
                double falseProb = GetFalseProbability(far);

                if (matchList.Count > 0)
                {
                    bool found = false;
                    for (int i = 0; found != true && i < matchList.Count && i < maxDisplay; i++)
                    {
                        if (newMatch.Score > matchList[i].Score)
                        {
                            Match temp = new Match(file, newMatch.Id, newMatch.Score, far, falseProb, DataFolder + @"\Face Img\");
                            matchList.Insert(i, temp);
                            found = true;
                        }
                    }

                    // If loop ended and score was not higher than anything already in the list, insert at the end
                    if (found != true)
                    {
                        Match temp = new Match(file, newMatch.Id, newMatch.Score, far, falseProb, DataFolder + @"\Face Img\");
                        matchList.Add(temp);
                    }
                }
                else
                {
                    // The list is empty, so insert into list

                    Match temp = new Match(file, newMatch.Id, newMatch.Score, far, falseProb, DataFolder + @"\Face Img\");
                    matchList.Add(temp);
                }

                // If the number of elements in the list is higher than the maximum amount, then remove the last element
                if (matchList.Count > maxDisplay)
                {
                    matchList.RemoveAt(maxDisplay);
                }
            }

            /// <summary>
            /// Displays the list of top n matches to the GUI
            /// </summary>
            private void DisplayMatches()
            {
                int maxDisplay = curSettings.GetNumMatches();

                for (int i = 0; i < matchList.Count && i < maxDisplay; i++)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                    matchList[i].Rank = i + 1;
                    });

                    matchCount++;
                    matchProgress.ReportProgress(matchCount, "Displaying Matches");
                    Thread.Sleep(1);
                }

                this.Dispatcher.Invoke(() =>
                {
                    TopMatchList.Items.Refresh();

                    Log("Matches Displayed");
                });
            }

        #endregion

        #region Verification (1:1) Functions

            #region Import Functions

                /// <summary>
                /// Starts the template import process for the left image
                /// </summary>
                private void VerificationImportTop_Click(object sender, RoutedEventArgs e)
                {
                    if (GetCheckedOptions())
                    {
                        // Change text-dependency options so that they cannot be altered
                        TextDependentCheck.IsEnabled = false;
                        TextIndependentCheck.IsEnabled = false;

                        // Clear the top subject
                        verSubjectTop = null;

                        Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                        openPicker.DefaultExt = defExt;
                        openPicker.Filter = FileFilter;

                        if (openPicker.ShowDialog() == true)
                        {

                            // Clear the import path and determine new path
                            VerificationImportPathTop.Text = openPicker.FileName;
                            GetTemplate(openPicker.FileName, out verSubjectTop);

                            if (openPicker.FileName != string.Empty)
                            {
                                topPlayer.Open(new Uri(VerificationImportPathTop.Text));
                                
                                topTimer.Interval = TimeSpan.FromSeconds(1);
                                topTimer.Tick += topTick;


                                TopRID.Content = "Playing: " + verSubjectTop.Id;

                                EnableVerify();

                                Log("Top Verification Voice Imported");

                                VerificationClear.IsEnabled = true;
                                VerificationClearTop.IsEnabled = true;
                                TopScan.IsEnabled = false;
                                TopScanPath.IsEnabled = false;
                            }

                            // No file path was chosen, so reset the checkboxes to be enabled again (only if other file path is empty)
                            else
                            {
                                TextDependentCheck.IsEnabled = true;
                                TextIndependentCheck.IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Please select at least one or both of the extraction options");
                    }
                }

                /// <summary>
                /// Starts the template import process for the right image
                /// </summary>
                private void VerificationImportBottom_Click(object sender, RoutedEventArgs e)
                {
                    if (GetCheckedOptions())
                    {
                        // Change text-dependency options so that they cannot be altered
                        TextDependentCheck.IsEnabled = false;
                        TextIndependentCheck.IsEnabled = false;
                
                        // Clear the bottom subject
                        verSubjectBottom = null;

                        Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                        openPicker.DefaultExt = defExt;
                        openPicker.Filter = FileFilter;

                        if (openPicker.ShowDialog() == true)
                        {

                            // Clear the import path and determine new path
                            VerificationImportPathBottom.Text = openPicker.FileName;
                            GetTemplate(openPicker.FileName, out verSubjectBottom);

                            if (openPicker.FileName != string.Empty)
                            {
                                bottomPlayer.Open(new Uri(VerificationImportPathBottom.Text));
                                
                                bottomTimer.Interval = TimeSpan.FromSeconds(1);
                                bottomTimer.Tick += bottomTick;

                                BottomRID.Content = "Playing: " + verSubjectBottom.Id;

                                EnableVerify();

                                Log("Bottom Verification Voice Imported");

                                VerificationClear.IsEnabled = true;
                                VerificationClearBottom.IsEnabled = true;
                                BottomScan.IsEnabled = false;
                                BottomScanPath.IsEnabled = false;
                            }

                            // No file path was chosen, so reset the checkboxes to be enabled again (only if other file path is empty)
                            else
                            {
                                TextDependentCheck.IsEnabled = true;
                                TextIndependentCheck.IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Please select at least one or both of the extraction options");
                    }
                }

            #endregion

            #region Clear and Reset Functions

                /// <summary>
                /// Clears the top subject/voice
                /// </summary>
                private void VerificationClearTop_Click(object sender, RoutedEventArgs e)
                {
                    VerificationClearTop.IsEnabled = false;
            
                    // Reset Verification buttons
                    VerifyVoices.IsEnabled = false;

                    verSubjectTop = null;
                    VerificationImportPathTop.Text = string.Empty;
                    topPlayer.Close();

                    // Clear ComboBoxes
                    TopVoiceSelect.Items.Clear();
                    TopVoiceSelect.IsEnabled = false;

                    // Clear lists
                    topScanFiles.Clear();

                    TopRID.Content = "---";

                    // Reset Import Buttons
                    VerificationImportTop.IsEnabled = true;
                    VerificationImportPathTop.IsEnabled = true;

                    // Reset Scan Buttons
                    TopScan.IsEnabled = true;
                    TopScanPath.IsEnabled = true;
                    TopScanPath.Text = string.Empty;
                    TopSelect.IsEnabled = false;
                    TopDeselect.IsEnabled = false;

                    if (!BottomScanPath.IsEnabled)
                        VerificationClear.IsEnabled = false;
                }

                /// <summary>
                /// Clears the bottom subject/voice
                /// </summary>
                private void VerificationClearBottom_Click(object sender, RoutedEventArgs e)
                {
                    VerificationClearBottom.IsEnabled = false;

                    // Reset Verification buttons
                    VerifyVoices.IsEnabled = false;

                    verSubjectBottom = null;
                    VerificationImportPathBottom.Text = string.Empty;
                    bottomPlayer.Close();

                    // Clear ComboBoxes
                    BottomVoiceSelect.Items.Clear();
                    BottomVoiceSelect.IsEnabled = false;

                    // Clear lists
                    bottomScanFiles.Clear();

                    BottomRID.Content = "---";

                    // Reset Import Buttons
                    VerificationImportBottom.IsEnabled = true;
                    VerificationImportPathBottom.IsEnabled = true;

                    // Reset Scan Buttons
                    BottomScan.IsEnabled = true;
                    BottomScanPath.IsEnabled = true;
                    BottomScanPath.Text = string.Empty;
                    BottomSelect.IsEnabled = false;
                    BottomDeselect.IsEnabled = false;

                    if(!TopScanPath.IsEnabled)
                        VerificationClear.IsEnabled = false;
                }

                /// <summary>
                /// Clears all verification information from the GUI
                /// </summary>
                private void VerificationClear_Click(object sender, RoutedEventArgs e)
                {
                    Log("");

                    VerificationClearTop_Click(null, null);
                    VerificationClearBottom_Click(null, null);

                    TextDependentCheck.IsEnabled = true;
                    TextIndependentCheck.IsEnabled = true;
                    UniqueCheck.IsEnabled = true;

                    VerificationClear.IsEnabled = false;

                    Log("Cleared Verification (1:1) Page");
                }

                /// <summary>
                /// Resets the verification results box
                /// </summary>
                private void VerificationReset_Click(object sender, RoutedEventArgs e)
                {
                    VerifyResults.Text = string.Empty;
                    VerificationReset.IsEnabled = false;
                    VerificationStore.IsEnabled = false;
                }

            #endregion

            /// <summary>
            /// Checks to see if the Verify button should be enabled
            /// </summary>
            private void EnableVerify()
            {
                VerifyVoices.IsEnabled = ValidSubject(verSubjectTop) && ValidSubject(verSubjectBottom);
            }

            /// <summary>
            /// Starts the verification process
            /// </summary>
            private void Verification_Click(object sender, RoutedEventArgs e)
            {
                // If user happened to perform identification (which sets unique phrases to false) between importing a subject and starting verification
                // it resets depending if the check box has been selected or not
                mainClient.VoicesUniquePhrasesOnly = UniqueCheck.IsChecked.Value;

                if (verSubjectTop != null && verSubjectBottom != null)
                {
                    Log("");
                    Log("Starting Verification (1:1)");

                    mainClient.BeginVerify(verSubjectTop, verSubjectBottom, AsyncVerification, null);
                }

                // Add potential checks or messages for user if verification can't be completed
            }

        #endregion

        #region Verification (1:N) Functions

            /// <summary>
            /// Imports the subject for verification
            /// </summary>
            private void VerifyImport_Click(object sender, RoutedEventArgs e)
            {
                verifySubject = null;

                Microsoft.Win32.OpenFileDialog openPicker = new Microsoft.Win32.OpenFileDialog();

                openPicker.DefaultExt = defExt;
                openPicker.Filter = FileFilter;

                if (openPicker.ShowDialog() == true)
                {
                    // Clear the import path and determine new path
                    VerifyImportPath.Text = openPicker.FileName;
                    GetTemplate(openPicker.FileName, out verifySubject);

                    if (openPicker.FileName != string.Empty)
                    {
                        verifyPlayer.Open(new Uri(VerifyImportPath.Text));
                        
                        verifyTimer.Interval = TimeSpan.FromSeconds(1);
                        verifyTimer.Tick += verifyTick;


                        VerifyRID.Content = "Playing: " + verifySubject.Id;

                        EnableVerifyN();

                        Log("Verification Image Imported");

                        VerifySubject_Clear.IsEnabled = true;
                        VerifyScan.IsEnabled = false;
                        VerifyScanPath.IsEnabled = false;
                    }
                }
            }

            /// <summary>
            /// Clears the verification subject
            /// </summary>
            private void VerifyNClear_Click(object sender, RoutedEventArgs e)
            {
                VerifySubjectVoice.IsEnabled = false;
                VerifySubject_Clear.IsEnabled = false;

                verifySubject = null;
                VerifyImportPath.Text = string.Empty;
                verifyPlayer.Close();

                // Clear ComboBoxes
                VerifyVoiceSelect.Items.Clear();
                VerifyVoiceSelect.IsEnabled = false;

                // Clear lists
                verifyScanFiles.Clear();

                VerifyRID.Content = "---";

                // Reset Import Buttons
                VerifyImport.IsEnabled = true;
                VerifyImportPath.IsEnabled = true;

                // Reset Scan Buttons
                VerifyScan.IsEnabled = true;
                VerifyScanPath.IsEnabled = true;
                VerifyScanPath.Text = string.Empty;
                VerifySelect.IsEnabled = false;
                VerifyDeselect.IsEnabled = false;

                Log("Cleared Verification (1:N) Page");
            }

            /// <summary>
            /// Resets the verication results
            /// </summary>
            private void VerifyNReset_Click(object sender, RoutedEventArgs e)
            {
                VerifyN_Results.Text = string.Empty;
                VerifySubject_Reset.IsEnabled = false;
                VerifyStoreResult.IsEnabled = false;
            }

            /// <summary>
            /// Enables the verification button if the subject is valid
            /// </summary>
            private void EnableVerifyN()
            {
                VerifySubjectVoice.IsEnabled = ValidSubject(verifySubject);
            }

            /// <summary>
            /// Event occurred that starts the verification (1:N) process
            /// </summary>
            private void VerifyN_Click(object sender, RoutedEventArgs e)
            {
                if(!CheckData())
                        System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                else
                {
                    // Clear the biometric client and start from scratch
                    mainClient.Clear();
                
                    // Get all voice templates
                    string[] allTemplates = Directory.GetFiles(TEMPLATEFOLDER, "*.dat");
                
                    // Enroll templates
                    EnrollTemplates(allTemplates);

                

                    if (verifySubject != null)
                    {
                        Log("");
                        Log("Starting Verification (1:N)");

                        // Make sure that only text-dependent extraction happens
                        mainClient.VoicesExtractTextDependentFeatures = true;
                        mainClient.VoicesExtractTextIndependentFeatures = false;
                        mainClient.VoicesUniquePhrasesOnly = false;

                        // Start verification
                        mainClient.BeginVerify(verifySubject, AsyncVerifyAll, null);
                    }
                }
            }

        #endregion

        #region Template Creation Functions

            /// <summary>
            /// Generates all template files for every directory/database in the settings
            /// </summary>
            private void GenerateAll_Click(object sender, RoutedEventArgs e)
            {
                if(!CheckData())
                        System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                // Check if background worker already has a job
                else if (!progress.IsBusy)
                {
                    GenerateAll.IsEnabled = false;
                    GenerateSelected.IsEnabled = false;
                    CancelTemplate.IsEnabled = true;

                    TemplateStatus.Maximum = GetNumberSubjects();
                    TemplateStatus.Value = 0;

                    Log("");
                    Log("Generating all templates");

                    progress.RunWorkerAsync(GetAccessibleDirectories(curSettings.GetDatabases()));
                }
            }

            /// <summary>
            /// Generates all template files for each directory/database specified by the user
            /// </summary>
            private void GenerateSelected_Click(object sender, RoutedEventArgs e)
            {
                if(!CheckData())
                    System.Windows.MessageBox.Show("The data folder has not been appropriately set.\nPlease check the settings before continuing","Folder Not Set", MessageBoxButton.OK);

                // Check if background worker already has a job and if there are actually any items selected
                else if (!progress.IsBusy && DatabaseDisplay.SelectedItems.Count > 0)
                {
                    string[] selectionList = GetAccessibleDirectories(DatabaseDisplay.SelectedItems.OfType<string>().ToArray());

                    // See if # of accessible directories is greater than 0
                    if (selectionList.Length > 0)
                    {
                        GenerateAll.IsEnabled = false;
                        GenerateSelected.IsEnabled = false;
                        CancelTemplate.IsEnabled = true;

                        // Create a copy of the selections, since they may potentially change

                        TemplateStatus.Maximum = GetNumberSubjects(selectionList);

                        Log("");
                        Log("Generating templates for selected directories");

                        progress.RunWorkerAsync(selectionList);
                    }
                }
            }

            /// <summary>
            /// Creates a new template without any returned values/attributes
            /// </summary>
            /// <param name="currentDirectory">The directory to be searched</param>
            /// <param name="RID">The RID of the intended subject</param>
            private void CreateNewTemplate(string RID, List<string> voiceFiles)
            {
                NSubject newSubject = new NSubject();
                newSubject.Id = RID;

                foreach (string audio in voiceFiles)
                {
                    using (NBuffer voiceBuffer = NBuffer.FromArray(ConvertAudio(audio)))
                    {
                        NVoice voice = new NVoice { SampleBuffer = voiceBuffer };

                        // Add the voice to the subject
                        newSubject.Voices.Add(voice);
                    }
                }

                NBiometricTask task = mainClient.CreateTask(NBiometricOperations.Segment | NBiometricOperations.CreateTemplate, newSubject);
                mainClient.PerformTask(task);
                //mainClient.CreateTemplate(newSubject);

                WriteTemplate(newSubject);
            }

            /// <summary>
            /// Records all data for subjects in each directory
            /// </summary>
            /// <param name="allDirectory">An array of directories to search through</param>
            private void RecordSubjects(string[] allDirectory)
            {
                // reset our dictionary
                recordList.Clear();

                // Loop through each directory provided
                for (int i = 0; i < allDirectory.Length && !progress.CancellationPending; i++)
                {
                    // Get all of the sub directories, which should be our RID folders
                    string[] ridDirectory = Directory.GetDirectories(allDirectory[i]);

                    // Loop through all of the RID's
                    for (int j = 0; j < ridDirectory.Length && !progress.CancellationPending; j++)
                    {
                        // Get the RID from the directory name
                        string RID = new DirectoryInfo(ridDirectory[j]).Name;

                        // Check if RID already exists in our dictionary
                        if (!recordList.ContainsKey(RID))
                        {
                            // Start a new record
                            //Record temp = new Record { id = RID, directoryList = new List<string>(), imageList = new List<string>() };
                            Record temp = new Record { voiceList = new List<string>() };

                            // Look in the remaining directories to see if they contain the RID
                            // If so, add that RID directory path to the record                        
                            foreach (string dir in GetAccessibleDirectories(curSettings.GetDatabases()))
                            {
                                if (Directory.Exists(dir + "\\" + RID))
                                {
                                    temp.voiceList.AddRange(GetAllVoices(dir + "\\" + RID));
                                }
                            }

                            if (temp.voiceList.Count > 0)
                            {
                                // Add our record into the dictionary
                                recordList.Add(RID, temp);

                                // Check if overall dictionary already contains the record and if the number of images is different than what we just found
                                if (AllRecordList.ContainsKey(RID))
                                {
                                    if (AllRecordList[RID].voiceList.Count != temp.voiceList.Count)
                                        AllRecordList[RID] = temp;
                                }
                                else
                                {
                                    AllRecordList.Add(RID, temp);
                                }


                                templateCount++;
                                progress.ReportProgress(templateCount, "Recorded Subject Files - " + RID);
                                Thread.Sleep(1);
                            }
                        }
                    }
                }
            }

            #region Main Template Processes

                /// <summary>
                /// The work method for the template background process
                /// </summary>
                private void template_DoWork(object sender, DoWorkEventArgs e)
                {
                    templateCount = 0;

                    // Repopulate our dictionary with all of the appropriate subjects/records
                    RecordSubjects((string[])e.Argument);

                    templateCount = 0;

                    if ((sender as BackgroundWorker).CancellationPending) { e.Cancel = true; } // Check if process has been canceled
                    else if (recordList != null && recordList.Count > 0)
                    {
                        // Loop through the subjects we recorded
                        foreach (KeyValuePair<string, Record> subject in recordList)
                        {
                            if ((sender as BackgroundWorker).CancellationPending) // Check if process has been canceled
                            {
                                e.Cancel = true;
                                return; // Exit out of function
                            }
                            else
                            {
                                // Try to get the template
                                string template = Directory.GetFiles(TEMPLATEFOLDER, subject.Key + ".dat", SearchOption.TopDirectoryOnly).FirstOrDefault();

                                // No Template Found
                                if (template == null)
                                {
                                    // No template found for subject, so start creating a template
                                    CreateNewTemplate(subject.Key, subject.Value.voiceList);
                                }

                                // Template Found
                                else
                                {
                                    // Read in subject from the template file
                                    NSubject templateSubject = NSubject.FromFile(template);

                                    // If template has nothing in it, it will throw a nullpointerexception. So, we'll want to overwrite the original file anyways.
                                    try
                                    {
                                        // Check if number of voice records from template is less than the number of voices for a given RID
                                        // If less, then we know we must recreate the template to include all voice data
                                        // If not, nothing happens

                                        if (subject.Value.voiceList != null && templateSubject.GetTemplate().Voices.Records.Count < subject.Value.voiceList.Count)
                                            CreateNewTemplate(subject.Key, subject.Value.voiceList);
                                    }
                                    catch
                                    {
                                        CreateNewTemplate(subject.Key, subject.Value.voiceList);
                                    }
                                }

                                // Increment Progress bar
                                templateCount++;
                                (sender as BackgroundWorker).ReportProgress(templateCount, "Saved Subject - " + subject.Key);
                                Thread.Sleep(1);
                            }
                        }
                    }
                }

                /// <summary>
                /// Method called when the template background process reports progress
                /// </summary>
                private void template_ProgressChanged(object sender, ProgressChangedEventArgs e)
                {
                    TemplateStatus.Value = e.ProgressPercentage;
                    TemplateStatusInfo.Text = "Progress: " + (string)e.UserState;
                }

                /// <summary>
                /// Method called when the template background process finishes
                /// </summary>
                private void template_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
                {
                    GenerateAll.IsEnabled = true;
                    GenerateSelected.IsEnabled = true;
                    CancelTemplate.IsEnabled = false;
                    TemplateStatus.Value = 0;
                    TemplateStatusInfo.Text = "Progress: ";

                    if (e.Error != null)
                    {
                        System.Windows.Forms.MessageBox.Show(e.Error.Message);
                    }
                    else if (e.Cancelled)
                    {
                        //System.Windows.Forms.MessageBox.Show("Template Process Canceled");
                        Log("Template Process Canceled");
                    }
                    else
                    {
                        //System.Windows.Forms.MessageBox.Show("Operation Completed");
                        Log("Template Process Completed");
                    }
                }

            #endregion

            /// <summary>
            /// Writes a subject template to a file
            /// </summary>
            /// <param name="subject">The subject of the template</param>
            /// <param name="directory">The directory to be saved to</param>
            private void WriteTemplate(NSubject subject)
            {
                // Semaphore used to make sure only one thread is writing at a time
                templateWriteHold.WaitOne();

                using (NBuffer subjectBuffer = subject.GetTemplateBuffer())
                {
                    File.WriteAllBytes(TEMPLATEFOLDER + "\\" + subject.Id + ".dat", subjectBuffer.ToArray());
                }

                templateWriteHold.Release();
            }

            /// <summary>
            /// Prompts the user to cancel the template generation process
            /// </summary>
            private void CancelTemplate_Click(object sender, RoutedEventArgs e)
            {
                MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure you want to cancel the template generation?",
                                                                               "Cancel Template", System.Windows.MessageBoxButton.YesNo);
                if (dialogResult == MessageBoxResult.Yes)
                    if (progress.IsBusy)
                        progress.CancelAsync();
            }

        #endregion

        #region Settings Action Commands

            #region Apply/Cancel/Revert/Default Settings

                /// <summary>
                /// Applies the values entered into the current settings
                /// </summary>
                private void ApplySettings(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Applying Settings");

                    int newMatches = 0;
                    double newFAR = 0;
                    int newThreshold = 0;
                    /*Check if input number of matches is an integer*/
                    /*Stores input into newMatches*/
                    if (Int32.TryParse(NumOfMatches.Text, out newMatches))
                    {
                        curSettings.SetNumMatches(newMatches);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid Number of Matches");
                        return;
                    }

                    /*Check if far is an integer*/
                    /*Stores input into newFAR*/
                    if (Double.TryParse(setMatchFAR.Text, out newFAR))
                    {
                        curSettings.SetFar(newFAR);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Invalid FAR");
                        return;
                    }

                    //if (!DatabaseDisplayList.HasItems)
                    //{
                    //    System.Windows.MessageBox.Show("No Databases Specified!");
                    //    return;
                    //}

                    /*Stores threshold as the biometric client threshold*/
                    if (Int32.TryParse(setThreshold.Text, out newThreshold))
                    {
                        curSettings.SetThreshold(newThreshold);
                        mainClient.MatchingThreshold = (byte)newThreshold;
                    }

                    List<string> listDatabases = new List<string>();
                    listDatabases = DatabaseDisplayList.Items.Cast<string>().ToList();
                    curSettings.SetDatabases(listDatabases);

                    System.Windows.MessageBox.Show("Settings Applied!");
                    Log("New Settings Applied");

                    ApplySetting.IsEnabled = false;
                    CancelSetting.IsEnabled = false;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Cancels any settings changes to before anything was altered
                /// </summary>
                private void CancelSettings(object sender, RoutedEventArgs e)
                {
                    MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure you want to cancel any changes?",
                                                                                   "Cancel Settings", System.Windows.MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        Log("");
                        Log("Canceling Settings Changes");

                        NumOfMatches.Text = curSettings.GetNumMatches().ToString();
                        setMatchFAR.Text = curSettings.GetFar().ToString();
                        setThreshold.Text = curSettings.GetThreshold().ToString();
                        ImportDatabases(curSettings.GetDatabases());

                        ApplySetting.IsEnabled = false;
                        CancelSetting.IsEnabled = false;
                    }
                    else
                    {
                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                    }

                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Reverts the settings to the previously set values
                /// </summary>
                private void RevertSettings(object sender, RoutedEventArgs e)
                {
                    MessageBoxResult dialogResult = System.Windows.MessageBox.Show("Are you sure you want to revert to previously applied settings?",
                                                                                   "Revert Settings", System.Windows.MessageBoxButton.YesNo);
                    if (dialogResult == MessageBoxResult.Yes)
                    {
                        Log("");
                        Log("Reverting Settings");

                        // Revert Settings
                        curSettings.Revert();

                        NumOfMatches.Text = curSettings.GetNumMatches().ToString();
                        setMatchFAR.Text = curSettings.GetFar().ToString();
                        setThreshold.Text = curSettings.GetThreshold().ToString();
                        ImportDatabases(curSettings.GetDatabases());

                        ApplySetting.IsEnabled = false;
                        CancelSetting.IsEnabled = false;
                    }

                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Restores the settings page back to its default state
                /// </summary>
                private void RestoreDefaultSettings(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Restoring Settings to Default");

                    NumOfMatches.Text = defaultSettings.GetNumMatches().ToString();
                    setMatchFAR.Text = defaultSettings.GetFar().ToString();
                    setThreshold.Text = defaultSettings.GetThreshold().ToString();
                    ImportDatabases(defaultSettings.GetDatabases());

                    // Set current settings to default
                    curSettings.SetSettings(defaultSettings);

                    ApplySetting.IsEnabled = false;
                    CancelSetting.IsEnabled = false;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = false;
                }

            #endregion

            #region Directory/Network Management Functions

                /// <summary>
                /// Lets user select a directory/database and adds it to the database list in the GUI
                /// </summary>
                private void AddDirectory_Click(object sender, RoutedEventArgs e)
                {
                    // Have to specify the Windows.Forms to avoid confusion of Windows Forms and WPF versions
                    System.Windows.Forms.FolderBrowserDialog folderPicker = new System.Windows.Forms.FolderBrowserDialog();
                    System.Windows.Forms.DialogResult result = folderPicker.ShowDialog();

                    // previously used folderPicker.SelectedPath != null
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        Log("");
                        Log(string.Format("Adding Database: {0}", folderPicker.SelectedPath));

                        DatabaseDisplayList.Items.Add(folderPicker.SelectedPath);
                        DatabaseDisplay.Items.Add(folderPicker.SelectedPath);

                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                        RevertSetting.IsEnabled = true;
                        DefaultSetting.IsEnabled = true;
                    }
                }

                /// <summary>
                /// Opens a user prompt to enter in a network drive path
                /// </summary>
                private void AddNetworkDrive_Click(object sender, RoutedEventArgs e)
                {
                    UserInput networkInput = new UserInput("Enter the server address", "Add Network Drive", "Address: ");
                    networkInput.Owner = this;

                    if (networkInput.ShowDialog() == true)
                    {
                        Log("");
                        Log(string.Format("Adding Network/Server: {0}", networkInput.ServerName));

                        DatabaseDisplayList.Items.Add(networkInput.ServerName);
                        DatabaseDisplay.Items.Add(networkInput.ServerName);

                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                    }

                }

                /// <summary>
                /// Removes a specific database from the GUI
                /// </summary>
                private void RemoveDatabase_Click(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Removing Database(s)");

                    object[] selectionList = new object[DatabaseDisplayList.SelectedItems.Count];
                    DatabaseDisplayList.SelectedItems.CopyTo(selectionList, 0);
                    foreach (object selection in selectionList)
                    {
                        DatabaseDisplayList.Items.Remove(selection);
                        DatabaseDisplay.Items.Remove(selection);
                    }

                    ApplySetting.IsEnabled = true;
                    CancelSetting.IsEnabled = true;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Clears the list of databases from the GUI
                /// </summary>
                private void ClearDatabases_Click(object sender, RoutedEventArgs e)
                {
                    Log("");
                    Log("Clearing All Databases");

                    DatabaseDisplayList.Items.Clear();
                    DatabaseDisplay.Items.Clear();

                    ApplySetting.IsEnabled = true;
                    CancelSetting.IsEnabled = true;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                /// <summary>
                /// Opens a dialog box to set the Twins Day 2017 server address
                /// </summary>
                private void SetServerData_Click(object sender, RoutedEventArgs e)
                {
                    //UserInput TWDServerInput = new UserInput("Enter the Twins Day 2017 server address:", "Add TWD17 Server", "Address: ", TWD17_Server, false);
                    //TWDServerInput.Owner = this;

                    //if (TWDServerInput.ShowDialog() == true)
                    //{
                    //    string server = TWDServerInput.ServerName;

                    //    // We want the server path to end with a backslash
                    //    // Test if last character is '\' and if not, add it
                    //    if (!server[server.Length - 1].Equals('\\'))
                    //        server += "\\";

                    //    Log("");
                    //    Log(string.Format("Setting TWD 2017 Server: {0}", server));

                    //    // If server was included in our directory list, update it accordingly
                    //    curSettings.ChangeDatabase(TWD17_Server + "ADIO", server + "ADIO");

                    //    TWD17_Server = server;

                    //    // Default databases have changed due to static TWD17_Server variable, so update the default settings
                    //    defaultSettings.SetDatabases(defaultDatabases);

                    //    // Update our directory and network lists
                    //    ImportDatabases(curSettings.GetDatabases());
                    //}

                    ServerSet InputServer = new ServerSet(TWD17_Server, DataFolder);
                    InputServer.Owner = this;
                    InputServer.Show();
                }

                public void SetServer(string serverPath)
                {
                    TWD17_Server = serverPath;

                    if (!TWD17_Server[TWD17_Server.Length - 1].Equals('\\'))
                        TWD17_Server += "\\";
                    
                    // If server was included in our directory list, update it accordingly
                    curSettings.ChangeDatabase(TWD17_Server + "FACE", TWD17_Server + "FACE");

                    // Default databases have changed due to static TWD17_Server variable, so update the default settings
                    defaultSettings.SetDatabases(defaultDatabases);

                    // Update our directory and network lists
                    ImportDatabases(curSettings.GetDatabases());

                    Log("");
                    Log(string.Format("Setting TWD 2017 Server: {0}", TWD17_Server));
                }

                public void SetData(string dataPath)
                {
                    DataFolder = dataPath;

                    Log(string.Format("Setting Data Folder: {0}", DataFolder));
                }

                private bool CheckData()
                {
                    bool result = false;

                    if (GetAccess(DataFolder, false))
                    {
                        string tempPath = DataFolder;

                        if (!tempPath[tempPath.Length - 1].Equals('/'))
                            tempPath += "//";

                        if (Directory.Exists(tempPath + "Audio") && Directory.Exists(tempPath + "Face") && Directory.Exists(tempPath + "Face Img"))
                            result = true;
                    }

                    return result;
                }

                private bool CheckServer()
                {
                    return GetAccess(TWD17_Server, false);
                }

            #endregion

            #region User Text Input/ComboBox Functions

                /// <summary>
                /// Event when the NumOfMatches combobox text is changed
                /// </summary>
                private void NumOfMatches_TextChanged(object sender, TextChangedEventArgs e)
                {
                    ApplySetting.IsEnabled = true;
                    CancelSetting.IsEnabled = true;
                    RevertSetting.IsEnabled = true;
                    DefaultSetting.IsEnabled = true;
                }

                #region Threshold and FAR Functions

                    /// <summary>
                    /// Limits the set threshold text box to only enter in numbers
                    /// </summary>
                    private void setThreshold_PreviewTextInput(object sender, TextCompositionEventArgs e)
                    {
                        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
                        e.Handled = regex.IsMatch(e.Text);
                    }

                    /// <summary>
                    /// Clears the FAR text when threshold gains focus
                    /// </summary>
                    private void setThreshold_GotFocus(object sender, RoutedEventArgs e)
                    {
                        setMatchFAR.Text = string.Empty;
                    }

                    /// <summary>
                    /// Changes the FAR text if the threshold is changed
                    /// </summary>
                    private void setThreshold_LostFocus(object sender, RoutedEventArgs e)
                    {
                        if (setThreshold.Text == string.Empty)
                        {
                            setMatchFAR.Text = string.Empty;
                        }
                        else
                        {
                            int threshold = 0;
                            Int32.TryParse(setThreshold.Text, out threshold);

                            double farResult = GetFAR(threshold) * 100;
                            if (farResult >= 0.0001)
                                setMatchFAR.Text = Math.Round(farResult, 4).ToString();
                            else if (farResult >= 0.0000000001)
                                setMatchFAR.Text = string.Format("{0:0.##########}", Math.Round(farResult, 10));
                            else
                                setMatchFAR.Text = farResult.ToString("0.0###e-00");
                        }

                        ApplySetting.IsEnabled = true;
                        CancelSetting.IsEnabled = true;
                    }

                    /// <summary>
                    /// Clears the threshold text when FAR gains focus
                    /// </summary>
                    private void setMatchFAR_GotFocus(object sender, RoutedEventArgs e)
                    {
                        setThreshold.Text = string.Empty;
                    }

                    /// <summary>
                    /// Changes the threshold text if the FAR is changed
                    /// </summary>
                    private void setMatchFAR_LostFocus(object sender, RoutedEventArgs e)
                    {
                        double far = 0;

                        if (!(setMatchFAR.Text == string.Empty || setMatchFAR.Text.Equals(".") || setMatchFAR.Text.Equals("0.")) && Double.TryParse(setMatchFAR.Text, out far))
                        {
                            if (far <= 0 || far > 100)
                            {
                                setThreshold.Text = string.Empty;
                            }
                            else
                            {
                                setThreshold.Text = GetScore(far / 100).ToString();

                                ApplySetting.IsEnabled = true;
                                CancelSetting.IsEnabled = true;
                            }
                        }
                        else
                            setThreshold.Text = string.Empty;
                    }

                #endregion

            #endregion

            #region Status Button Methods

                /// <summary>
                /// Loading behavior for satus buttons
                /// </summary>
                private async void StatusButton_Load(object sender, RoutedEventArgs e)
                {
                    await Task.Run(() =>
                    {
                        string tag = string.Empty;
                        this.Dispatcher.Invoke(() => { tag = (string)(sender as System.Windows.Controls.Button).Tag; });

                        if (GetAccess(tag, false))
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightGreen; });
                        else
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightCoral; });
                    });
                }

                /// <summary>
                /// Click behavior for status buttons
                /// </summary>
                private async void StatusButton_Click(object sender, RoutedEventArgs e)
                {
                    (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightSkyBlue;

                    await Task.Run(() =>
                    {
                        Thread.Sleep(500);

                        string tag = string.Empty;
                        this.Dispatcher.Invoke(() => { tag = (string)(sender as System.Windows.Controls.Button).Tag; });

                        if (GetAccess(tag, false))
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightGreen; });
                        else
                            this.Dispatcher.Invoke(() => { (sender as System.Windows.Controls.Button).Background = System.Windows.Media.Brushes.LightCoral; });
                    });
                }

            #endregion

            #region ScrollViewer Methods

                /// <summary>
                /// Event triggered when the directory list is scrolled
                /// </summary>
                private void DatabaseDisplayList_ScrollChanged(object sender, ScrollChangedEventArgs e)
                {
                    ScrollViewer ListScroller = GetDescendant(DatabaseDisplayList, typeof(ScrollViewer)) as ScrollViewer;
                    ScrollViewer StatusScroller = GetDescendant(DBStatusList, typeof(ScrollViewer)) as ScrollViewer;
                    StatusScroller.ScrollToVerticalOffset(ListScroller.VerticalOffset);
                }

                /// <summary>
                /// Event triggered when the status list is scrolled
                /// </summary>
                private void DBStatusList_ScrollChanged(object sender, ScrollChangedEventArgs e)
                {
                    ScrollViewer ListScroller = GetDescendant(DatabaseDisplayList, typeof(ScrollViewer)) as ScrollViewer;
                    ScrollViewer StatusScroller = GetDescendant(DBStatusList, typeof(ScrollViewer)) as ScrollViewer;
                    ListScroller.ScrollToVerticalOffset(StatusScroller.VerticalOffset);
                }

                /// <summary>
                /// Gets the child/descendant from a WPF element
                /// </summary>
                /// <returns>The WPF element descendant</returns>
                private Visual GetDescendant(Visual element, Type type)
                {
                    //Based on an answer on https://social.msdn.microsoft.com/Forums/vstudio/en-US/38413d0a-7388-4191-a7a6-fd66e469d502/two-listbox-scrollbar-in-synchronisation?forum=wpf

                    if (element == null)
                        return null;
                    if (element.GetType() == type)
                        return element;

                    Visual foundElement = null;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).ApplyTemplate();
                    }

                    bool found = false;
                    for (int i = 0; !found && i < VisualTreeHelper.GetChildrenCount(element); i++)
                    {
                        Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                        foundElement = GetDescendant(visual, type);
                        if (foundElement != null)
                            found = true;
                    }

                    return foundElement;
                }

            #endregion

            #region Window and Keyboard Events

                /// <summary>
                /// Main window closing behavior
                /// </summary>
                private void Window_Closing(object sender, CancelEventArgs e)
                {

                    string message = string.Empty;
                    int identifier = 0;
                    bool templateBusy = false;
                    bool needConfirm = false;

                    // See if any background workers are currently running and ask if they want to really close
                    // If yes, cancel all of the running background processes
                    if (identifyProgress.IsBusy) // || matchProgress.IsBusy)
                    {
                        needConfirm = true;

                        message = "Identification is still working.\n\nDo you still want to close?";
                        identifier = 1;

                        if (matchProgress.IsBusy)
                            identifier = 0;

                        if (progress.IsBusy)
                        {
                            message = "Identification and template generation are still working.\n\nDo you still want to close?";
                            templateBusy = true;
                        }

                    }
                    else if (progress.IsBusy)
                    {
                        needConfirm = true;

                        message = "Template generation is still working.\n\nDo you still want to close?";
                        identifier = 2;
                    }

                    if (needConfirm)
                    {
                        MessageBoxResult choice = System.Windows.MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (choice == MessageBoxResult.Yes)
                        {
                            switch (identifier)
                            {
                                case 0:
                                    // Cancel matching, but don't break
                                    matchProgress.CancelAsync();
                                    goto case 1;
                                case 1:
                                    // Cancel identification/matching
                                    identifyProgress.CancelAsync();

                                    if (templateBusy)
                                        goto case 2;

                                    break;
                                case 2:
                                    // Cancel template generation
                                    progress.CancelAsync();
                                    break;
                            }
                        }
                        else
                        {
                            // Cancel the closing operation
                            e.Cancel = true;
                            return;
                        }
                    }

                    // Release all Neurotec Licenses
                    foreach (string license in voiceLicenseComponents)
                    {
                        NLicense.ReleaseComponents(license);
                    }

                    // Close any subwindows we may have open
                    foreach (Window subWindow in App.Current.Windows)
                    {
                        if (subWindow != this)
                            subWindow.Close();
                    }
                }

                /// <summary>
                /// Sets focus to the window when the user clicks outside of a control
                /// </summary>
                private void Window_MouseDown(object sender, MouseButtonEventArgs e)
                {
                    MainGrid.Focus();
                }

                /// <summary>
                /// Sets focus to the window when the user presses enter within a control
                /// </summary>
                private void Settings_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        MainGrid.Focus();
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Performs the appropriate actions when tabs are changed
                /// </summary>
                private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    if (e.OriginalSource is System.Windows.Controls.TabControl)
                    {
                        if (((IdentifyTab != null && IdentifyTab.IsSelected) || (VerifyTab != null && VerifyTab.IsSelected) || (VerifyNTab != null && VerifyNTab.IsSelected)) && mainClient != null)
                        {
                            mainClient.Cancel();
                        }
                        else if (TemplateTab != null && TemplateTab.IsSelected)
                        {
                            ImportDatabases(curSettings.GetDatabases());

                            DatabaseDisplay.Items.Refresh();
                        }
                        else if (SettingsTab != null && SettingsTab.IsSelected)
                        {
                            NumOfMatches.Text = curSettings.GetNumMatches().ToString();
                            setMatchFAR.Text = curSettings.GetFar().ToString();
                            setThreshold.Text = curSettings.GetThreshold().ToString();
                            ImportDatabases(curSettings.GetDatabases());

                            DatabaseDisplayList.Items.Refresh();

                            // Make sure buttons are not enabled after loading the changes
                            ApplySetting.IsEnabled = false;
                            CancelSetting.IsEnabled = false;
                        }
                        else if (LogTab != null && LogTab.IsSelected)
                        {
                            LogText.ScrollToEnd();
                        }
                    }

                    e.Handled = true;
                }

                /// <summary>
                /// Sets the identification results to immediately handle the BringIntoView event
                /// </summary>
                private void ResultViewHandler(object sender, RequestBringIntoViewEventArgs e)
                {
                    e.Handled = true;
                }

            #endregion

        #endregion

        #region Neurotec Asynch Functions

            /// <summary>
            /// Asynchronously ends the template creation process
            /// </summary>
            private void AsyncCreation(IAsyncResult r)
            {
                // Based loosely on the Neurotechnology Simple Faces Demo

                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncCreation), r);
                }
                else
                {
                    try
                    {
                        NBiometricTask task = mainClient.EndPerformTask(r);
                        NBiometricStatus status = task.Status;
                        if (status != NBiometricStatus.Ok)
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("The template was not extracted: {0}.", status), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Log(string.Format("The template was not extracted: {0}.", status));
                        }
                        //Log("Template was created");
                    }
                    catch (Exception ex)
                    {
                        Log(string.Format("Template creation failed: {0}.", ex.ToString()));
                    }
                }
            }

            /// <summary>
            /// Asynchronously ends the verification (1:1) process
            /// </summary>
            private void AsyncVerification(IAsyncResult r)
            {
                // Based loosely on the Neurotechnology Simple Faces Demo

                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncVerification), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndVerify(r);
                        if (status == NBiometricStatus.Ok || status == NBiometricStatus.MatchNotFound)
                        {
                            int score = verSubjectTop.MatchingResults[0].Score;

                            // Print results/information into the VerifyResults text box
                            // We can change this later to display whatever we want to plug in
                            VerifyResults.Text += string.Format("Top RID: {0}\nBottom RID: {1}\nScore: {2}\n\n", verSubjectTop.Id, verSubjectBottom.Id, score);

                            VerificationReset.IsEnabled = true;
                            VerificationStore.IsEnabled = true;
                        }
                        //Log("Verification Completed");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error: {0}.", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("Error Occurred in Verification Process");
                    }
                }
            }

            /// <summary>
            /// Asynchronously ends the verification (1:N) process
            /// </summary>
            private void AsyncVerifyAll(IAsyncResult r)
            {
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncVerifyAll), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndVerify(r);
                        if (status == NBiometricStatus.Ok || status == NBiometricStatus.MatchNotFound)
                        {
                            int score = verifySubject.MatchingResults[0].Score;

                            // Print results/information into the VerifyResults text box
                            // We can change this later to display whatever we want to plug in
                            VerifyN_Results.Text += string.Format("Subject RID: {0}\nScore: {1}\n\n", verifySubject.Id, score);

                            VerifySubject_Reset.IsEnabled = true;
                            VerifyStoreResult.IsEnabled = true;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Incomplete Verification: {0}", status), "Incomplete Verification", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Log(string.Format("Incomplete Verification: {0}", status));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error: {0}.", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("Error Occurred in Verification Process");
                    }
                }
            }

            /// <summary>
            /// Asynchronously ends the Identification process
            /// </summary>
            private void AsyncIdentification(IAsyncResult r)
            {
                // Based loosely on the Neurotechnology Simple Faces Demo
                if (!Dispatcher.CheckAccess())
                {
                    Dispatcher.Invoke(new AsyncCallback(AsyncIdentification), r);
                }
                else
                {
                    try
                    {
                        NBiometricStatus status = mainClient.EndIdentify(r);
                        if (status == NBiometricStatus.Ok || status == NBiometricStatus.MatchNotFound)
                        {
                            Log("Identification successfully completed");

                            matchProgress.RunWorkerAsync();
                        }
                        else
                        {
                            Log(string.Format("Identification completed, but not successful: {0}", status));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("Error: {0}.", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Log("Error Occurred in Identification Process");
                    }
                }
            }

        #endregion

        #region Internal Functions

            #region Neurotec and Common Identify/Verify Functions

                /// <summary>
                /// Starts the Neurotechnology license obtaining process and initializes the biometric client
                /// </summary>
                private void Start()
                {
                    try
                    {
                        foreach (string license in voiceLicenseComponents)
                        {
                            if (NLicense.ObtainComponents(Address, Port, license))
                            {
                                Log(string.Format("License was obtained: {0}", license));
                            }
                            else
                            {
                                Log(string.Format("License could not be obtained: {0}", license));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("An error occurred in the license process:\n\n{0}", ex.ToString()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Log("");
                    Log("Starting Client");
                    mainClient = new NBiometricClient();
                    mainClient.BiometricTypes = NBiometricType.Voice;
                    mainClient.UseDeviceManager = true;
                    mainClient.Initialize();
                    mainClient.VoicesExtractTextDependentFeatures = true;
                    mainClient.VoicesExtractTextIndependentFeatures = false;
                    Log("Client Was Initialized");

            
                }

                /// <summary>
                /// Creates a template for a subject, usually when obtaining from a file
                /// </summary>
                /// <param name="faceView">The biometric face data for the subject</param>
                /// <param name="newSubject">The biometric subject that contains the face template (Note: is an out parameter)</param>
                /// <returns>The file path to the selected image</returns>
                private void GetTemplate(string filename, out NSubject newSubject)
                {
                    newSubject = null;

                    if (filename != string.Empty)
                    {
                        string[] splitName = System.IO.Path.GetFileNameWithoutExtension(filename).Split('_');

                        try
                        {
                            newSubject = NSubject.FromFile(filename);
                            newSubject.Id = splitName[0];
                        }
                        catch { }

                        if (newSubject == null)
                        {
                            try
                            {
                                newSubject = new NSubject();
                                NVoice voice = new NVoice { SampleBuffer = NBuffer.FromArray(ConvertAudio(filename)) };
                                newSubject.Voices.Add(voice);

                                newSubject.Id = splitName[0];

                                Log("");
                                Log(string.Format("Attempting to create template ({0})", newSubject.Id));
                                NBiometricTask task = mainClient.CreateTask(NBiometricOperations.Segment | NBiometricOperations.CreateTemplate, newSubject);
                                mainClient.BeginPerformTask(task, AsyncCreation, null);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Forms.MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }

                /// <summary>
                /// Reads an array of templates and enrolls them to the biometric client
                /// </summary>
                /// <param name="allTemplates">An array of templates</param>
                private void EnrollTemplates(string[] allTemplates)
                {
                    // Go through all templates
                    // Import subject + RID into a new NSubject

                    referenceSubject = new NSubject[allTemplates.Length];

                    for (int i = 0; i < allTemplates.Length; i++)
                    {
                        referenceSubject[i] = NSubject.FromFile(allTemplates[i]);
                        referenceSubject[i].Id = System.IO.Path.GetFileNameWithoutExtension(allTemplates[i]);

                        // Add subjects to the enrollment "task"
                        enrollment.Subjects.Add(referenceSubject[i]);

                        // Added if statement just so that we can run the method without running identification
                        if (identifyProgress.IsBusy)
                        {
                            identifyCount++;
                            identifyProgress.ReportProgress(identifyCount, "Enrolling Subject - " + referenceSubject[i].Id);
                            Thread.Sleep(1);
                        }
                    }

                    // Perform the "task" which contains each subject
                    mainClient.PerformTask(enrollment);
                }

                /// <summary>
                /// Determines if the subject is instantiated and its status is valid
                /// </summary>
                /// <param name="subject">The subject to evaluate</param>
                /// <returns>True, is subject is valid and, False, if not</returns>
                private bool ValidSubject(NSubject subject)
                {
                    return subject != null && (subject.Status == NBiometricStatus.Ok
                        || subject.Status == NBiometricStatus.MatchNotFound
                        || subject.Status == NBiometricStatus.None && subject.GetTemplateBuffer() != null);
                }

            #endregion

            #region Directory/Network Functions

                /// <summary>
                /// Displays a list of databases to the GUI
                /// </summary>
                /// <param name="databases">A list of strings with database file paths</param>
                private void ImportDatabases(List<string> databases)
                {
                    //Log("Importing Databases");

                    DatabaseDisplayList.Items.Clear();
                    DatabaseDisplay.Items.Clear();
                    for (int index = 0; index < databases.Count; ++index)
                    {
                        DatabaseDisplayList.Items.Add(databases[index]);
                        DatabaseDisplay.Items.Add(databases[index]);
                    }
                }

                /// <summary>
                /// Checks to see if each database/directories exists within the system
                /// </summary>
                /// <param name="list">A list of strings with database file paths</param>
                /// <returns>True, if all databases are present or, False, if not</returns>
                private bool AllDatabasesExist(List<string> list)
                {
                    Log("");
                    Log("Checking Databases");

                    bool noErrors = true;
                    for (int index = 0; index < list.Count; ++index)
                    {
                        if (GetAccess(list[index], false) && !Directory.Exists(list[index]))
                        {
                            Log(string.Format("Error - Database not recognized: {0}", list[index]));
                            noErrors = false;
                        }
                    }
                    Log("Database Check Successful");

                    return noErrors;
                }

                /// <summary>
                /// Checks to see if we are able to get access to an UNC path or directory
                /// </summary>
                /// <param name="uncPath">The UNC path or directory path</param>
                /// <returns>True if we can connect, false otherwise</returns>
                public bool GetAccess(string uncPath)
                {
                    bool result = false;

                    // Invoked main dispatcher if we decide to use this method in any background workers
                    // Dispatcher needed to return log messages to GUI (since on different thread of execution)
                    this.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Directory.GetAccessControl(uncPath);
                            result = true;
                            Log("Access granted, connected to: " + uncPath);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Could be read-only, so it still exists
                            result = true;
                            Log("Access unauthorized, connected to: " + uncPath);
                        }
                        catch
                        {
                            // Some other error, so assume it doesn't exist
                            result = false;
                            Log("Error occurred, could not access: " + uncPath);
                        }
                    });

                    return result;
                }

                /// <summary>
                /// Checks to see if we are able to get access to an UNC path or directory
                /// </summary>
                /// <param name="uncPath">The UNC path or directory path</param>
                /// <param name="wantLog">True if you want log messages, false if not</param>
                /// <returns>True if we can connect, false otherwise</returns>
                public bool GetAccess(string uncPath, bool wantLog)
                {
                    bool result = false;

                    if (wantLog)
                        return GetAccess(uncPath);
                    else
                    {
                        try
                        {
                            Directory.GetAccessControl(uncPath);
                            result = true;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Could be read-only, so it still exists
                            result = true;
                        }
                        catch
                        {
                            // Some other error, so assume it doesn't exist
                            result = false;
                        }
                    }
                    return result;
                }

                /// <summary>
                /// Evaluates an array of directory paths and returns only accessible paths
                /// </summary>
                /// <param name="directories">An array of directory paths</param>
                /// <returns>A resulting array of accessible directories</returns>
                private string[] GetAccessibleDirectories(string[] directories)
                {
                    List<string> accessible = new List<string>();

                    foreach (string dir in directories)
                        if (GetAccess(dir, false))
                            accessible.Add(dir);

                    return accessible.ToArray();
                }

                /// <summary>
                /// Evaluates a list of directory paths and returns only accessible paths
                /// </summary>
                /// <param name="directories">A list of directory paths</param>
                /// <returns>A resulting array of accessible directories</returns>
                private string[] GetAccessibleDirectories(List<string> directories)
                {
                    List<string> accessible = new List<string>();

                    foreach (string dir in directories)
                        if (GetAccess(dir, false))
                            accessible.Add(dir);

                    return accessible.ToArray();
                }

            #endregion

            #region Get Audio File Functions

                /// <summary>
                /// Finds the audio result for an RID
                /// </summary>
                /// <param name="RID">A subject's RID</param>
                /// <returns>A string path to the intended voice/audio</returns>
                private string GetTokenAudioPath(string RID)
                {
                    string result = string.Empty;

                    // See if the RID already exists within our dictionary
                    if (AllRecordList.ContainsKey(RID))
                    {
                        // Just return the first audio file from the recorded voice path(s)
                        result = AllRecordList[RID].voiceList[0];
                    }

                    // RID was not in dictionary, so use previous method to get images
                    else
                    {
                        bool found = false;
                        foreach (string directory in GetAccessibleDirectories(curSettings.GetDatabases()))
                        {
                            string tempDir = directory + "\\" + RID;
                            if (!found && Directory.Exists(tempDir))
                            {
                                result = GetAudioToken(tempDir);
                                found = true;
                            }
                        }
                    }

                    // Just in case result somehow ended up being null, return an empty string
                    if (result == null)
                        return string.Empty;

                    return result;
                }

                /// <summary>
                /// Gets all audio/voice files from a directory
                /// </summary>
                /// <param name="currentDirectory">The directory to search</param>
                /// <returns>A string list of paths to audio/voice files</returns>
                private List<string> GetAllVoices(string currentDirectory)
                {
                    // Since with the TWD17 collection we're only really grabbing one file per "directory", the
                    // List collection isn't necessary. However, if for some reason the search parameters change
                    // and you have to grab more than one file per directory, then it's set up so you can return
                    // that collection

                    List<string> voiceFiles = new List<string>();
                    string mainDirectory = new DirectoryInfo(currentDirectory).Parent.FullName;

                    /* TWINS DAY 2017 */
                    if (mainDirectory.Equals(defaultDatabases[defaultDatabases.Count - 1]))
                    {
                        // Do Nothing since we have no audio to contribute for templates
                    }

                    /* TWINS DAY 2014 */
                    else if (mainDirectory.Equals(defaultDatabases[0]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            try
                            {
                                voiceFiles.AddRange(getAllFilePaths(dateDirectory + @"\Nuemann Mic", "*_rainbow.wav"));
                            }
                            catch { }
                        }
                    }

                    /* TWINS DAY 2015 */
                    else if (mainDirectory.Equals(defaultDatabases[1]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            try
                            {
                                voiceFiles.AddRange(getAllFilePaths(dateDirectory + @"\Neumann Mic", "*_ADIO_NEUM_AUD_0004.wav"));
                            }
                            catch { }
                        }
                    }

                    /* TWINS DAY 2016 */
                    else if (mainDirectory.Equals(defaultDatabases[2]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            try
                            {
                                voiceFiles.AddRange(getAllFilePaths(dateDirectory + @"\Dynamic_Mic", "*_ADIO_PVI1_RBOW.wav"));
                            }
                            catch { }
                        }
                    }

                    /* OTHER */
                    else
                    {
                        voiceFiles.AddRange(getAllFilePaths(currentDirectory, "*.wav"));
                    }

                    return voiceFiles;
                }

                /// <summary>
                /// Gets the Token/representative audio file from a given directory
                /// </summary>
                /// <param name="currentDirectory">The directory to search</param>
                /// <returns>A string path to the audio file</returns>
                private string GetAudioToken(string currentDirectory)
                {
                    string result = string.Empty;
                    string mainDirectory = new DirectoryInfo(currentDirectory).Parent.FullName;

                    /* TWINS DAY 2017 */
                    if (mainDirectory.Equals(defaultDatabases[defaultDatabases.Count - 1]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            if (result == null || result == string.Empty)
                            {
                                try
                                {
                                    result = Directory.GetFiles(dateDirectory + @"\PVI1", "*_NRBO*.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* TWINS DAY 2014 */
                    else if (mainDirectory.Equals(defaultDatabases[0]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            if (result == null || result == string.Empty)
                            {
                                try
                                {
                                    result = Directory.GetFiles(dateDirectory + @"\Nuemann Mic", "*_rainbow.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* TWINS DAY 2015 */
                    else if (mainDirectory.Equals(defaultDatabases[1]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            if (result == null || result == string.Empty)
                            {
                                try
                                {
                                    result = Directory.GetFiles(dateDirectory + @"\Neumann Mic", "*_ADIO_NEUM_AUD_0004.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* TWINS DAY 2016 */
                    else if (mainDirectory.Equals(defaultDatabases[2]))
                    {
                        foreach (string dateDirectory in Directory.GetDirectories(currentDirectory))
                        {
                            if (result == null || result == string.Empty)
                            {
                                try
                                {
                                    result = Directory.GetFiles(dateDirectory + @"\Dynamic_Mic", "*_ADIO_PVI1_RBOW.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                                }
                                catch { }
                            }
                        }
                    }

                    /* OTHER */
                    else
                    {
                        //result = getAllFilePaths(currentDirectory, "*.wav")[0];
                        result = Directory.GetFiles(currentDirectory, "*.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    }

                    // No result found/result ended up null, return an empty string
                    if (result == null)
                        return string.Empty;

                    return result;
                }

            #endregion

            #region GetNumberSubjects Functions

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases currently saved</returns>
                private int GetNumberSubjects()
                {
                    int subjectCount = 0;
                    Dictionary<string, string> RIDRecord = new Dictionary<string, string>();

                    // Loop through each directory provided
                    foreach (string dir in GetAccessibleDirectories(curSettings.GetDatabases()))
                    {
                        // Get all of the sub directories, which should be our RID folders
                        string[] ridDirectory = Directory.GetDirectories(dir);

                        // Loop through all of the RID's
                        for (int j = 0; j < ridDirectory.Length; j++)
                        {
                            // Get the RID from the directory name
                            string RID = new DirectoryInfo(ridDirectory[j]).Name;

                            // Check if RID already exists in our dictionary
                            if (!RIDRecord.ContainsKey(RID))
                            {
                                subjectCount++;

                                RIDRecord.Add(RID, RID);
                            }

                        }
                    }

                    return subjectCount;
                }

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases linked</returns>
                private int GetNumberSubjects(string directory)
                {
                    return Directory.GetDirectories(directory).Length;
                }

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases linked</returns>
                private int GetNumberSubjects(List<string> directories)
                {
                    int subjectCount = 0;
                    HashSet<string> RIDRecord = new HashSet<string>();

                    // Loop through each directory provided
                    for (int i = 0; i < directories.Count; i++)
                    {
                        // Get all of the sub directories, which should be our RID folders
                        string[] ridDirectory = Directory.GetDirectories(directories[i]);

                        // Loop through all of the RID's
                        for (int j = 0; j < ridDirectory.Length; j++)
                        {
                            // Get the RID from the directory name
                            string RID = new DirectoryInfo(ridDirectory[j]).Name;

                            // Check if RID already exists in our dictionary
                            if (!RIDRecord.Contains(RID))
                            {
                                subjectCount++;

                                RIDRecord.Add(RID);
                            }

                        }
                    }

                    return subjectCount;
                }

                /// <summary>
                /// Searches through each directory and counts the number of subjects
                /// </summary>
                /// <returns>The total number of subjects from all of the databases linked</returns>
                private int GetNumberSubjects(string[] directories)
                {
                    int subjectCount = 0;
                    HashSet<string> RIDRecord = new HashSet<string>();

                    // Loop through each directory provided
                    for (int i = 0; i < directories.Length; i++)
                    {
                        // Get all of the sub directories, which should be our RID folders
                        string[] ridDirectory = Directory.GetDirectories(directories[i]);

                        // Loop through all of the RID's
                        for (int j = 0; j < ridDirectory.Length; j++)
                        {
                            // Get the RID from the directory name
                            string RID = new DirectoryInfo(ridDirectory[j]).Name;

                            // Check if RID already exists in our dictionary
                            if (!RIDRecord.Contains(RID))
                            {
                                subjectCount++;

                                RIDRecord.Add(RID);
                            }

                        }
                    }

                    return subjectCount;
                }

            #endregion

            #region Convert Audio

                /// <summary>
                /// Converts a WAV file from 24-bit to 16-bit
                /// </summary>
                /// <param name="filename">The audio filename</param>
                /// <returns>A byte array of the audio data</returns>
                private byte[] ConvertAudio(string filename)
                {
                    byte[] audioData = new byte[0];

                    if (File.Exists(filename))
                    {
                        AudioConverter convAudio = new AudioConverter(filename);

                        // Audio was 24 bit
                        if (convAudio.Convert())
                            audioData = convAudio.GetConvertedAudio();

                        // Audio was not 24 bit
                        else
                            audioData = convAudio.GetOriginalAudio();
                    }

                    return audioData;
                }

            #endregion

            #region Conversion Functions

                /// <summary>
                /// Calculates the FAR associated with a threshold/score value
                /// </summary>
                /// <param name="score">The int value of the threshold/score</param>
                /// <returns>The decimal value of the FAR</returns>
                private double GetFAR(int score)
                {
                    return Math.Pow(10, ((double)score / -12));
                }

                /// <summary>
                /// Calculates the threshold/score associated with a FAR value
                /// </summary>
                /// <param name="far">The decimal value of the FAR</param>
                /// <returns>The int value of the threshold/score</returns>
                private int GetScore(double far)
                {
                    return (int)Math.Ceiling(Math.Log10(far) * (-12));
                }

                /// <summary>
                /// Formula from Neurotec documentation to find the False Acceptance Probability
                /// </summary>
                /// <param name="far">The FAR (False Acceptance Rate) in decimal form</param>
                /// <returns>The False Acceptance Probability in percentage</returns>
                private double GetFalseProbability(double far)
                {
                    // Formula from documentation to determine the probability of a false match
                    return (1 - Math.Pow((1 - (far / 100)), identifyMax)) * 100;
                }

            #endregion

            #region Other Functions

                /// <summary>
                /// Sets the corresponding values from CheckBoxes on the Verification GUI
                /// </summary>
                /// <returns>True, if no problems, or False, if boxes are incorrectly checked</returns>
                private bool GetCheckedOptions()
                {
                    // Since the IsChecked property is nullable, I make sure the results return false, if, for some reason, the evaluation returns an exception/null exception
                    try
                    {
                        // Check if at least one of the text-dependent/text-independent boxes are checked
                        if (TextIndependentCheck.IsChecked.Value == false && TextDependentCheck.IsChecked.Value == false)
                        {
                            return false;
                        }
                        else
                        {
                            // Text-Dependent Check
                            mainClient.VoicesExtractTextDependentFeatures = TextDependentCheck.IsChecked.Value;

                            // Text-Independent Check
                            mainClient.VoicesExtractTextIndependentFeatures = TextIndependentCheck.IsChecked.Value;
                        }

                        // Unique Phrases Check
                        mainClient.VoicesUniquePhrasesOnly = UniqueCheck.IsChecked.Value;
                    }
                    catch { return false; }

                    return true;
                }

                /// <summary>
                /// Searches through a directory and returns a list of files
                /// </summary>
                /// <param name="baseDir">The file path of the directory to be searched</param>
                /// <param name="fileFormat">Type of format of files to be searched. So, "*.jpg" would only return .jpg files</param>
                /// <returns>A list of file paths to every matching filetype within the given directory</returns>
                private List<string> getAllFilePaths(string baseDir, string fileFormat)
                {
                    List<string> allFiles = new List<string>();
                    Queue<string> pendingFolders = new Queue<string>();
                    pendingFolders.Enqueue(baseDir);
                    string[] temp;

                    while (pendingFolders.Count > 0)
                    {
                        baseDir = pendingFolders.Dequeue();
                        temp = Directory.GetFiles(baseDir, fileFormat, SearchOption.TopDirectoryOnly);

                        for (int i = 0; i < temp.Length; i++)
                            allFiles.Add(temp[i]);

                        temp = Directory.GetDirectories(baseDir);
                        for (int i = 0; i < temp.Length; i++)
                            pendingFolders.Enqueue(temp[i]);
                    }

                    return allFiles;
                }

                /// <summary>
                /// Method used to display actions into the log page
                /// </summary>
                /// <param name="s">The string message to be logged</param>
                private void Log(string s)
                {
                    LogText.Text += string.Format("{0}\t{1}\n", lineNum, s);
                    lineNum++;
                }

            #endregion

        #endregion

        #region Audio Player Functions

            #region Open Media Functions

                /// <summary>
                /// Method called when the media player opens
                /// </summary>
                private void probePlayer_MediaOpened(object sender, EventArgs e)
                {
                    if (probePlayer.NaturalDuration.HasTimeSpan)
                    {
                        TimeSpan tsProbe = probePlayer.NaturalDuration.TimeSpan;
                        IdentTime.Maximum = tsProbe.TotalSeconds;
                        //Log(string.Format("Total Seconds: {0}", tsProbe.TotalSeconds));
                        //Log(string.Format("Slider Max: {0}", tsProbe.TotalSeconds));
                        IdentTime.SmallChange = 1;
                        IdentTime.LargeChange = Math.Min(10, tsProbe.Seconds / 10);
                    }

                    probeTimer.Start();
                }

                /// <summary>
                /// Method called when the media player opens
                /// </summary>
                private void topPlayer_MediaOpened(object sender, EventArgs e)
                {
                    if (topPlayer.NaturalDuration.HasTimeSpan)
                    {
                        TimeSpan tsTop = topPlayer.NaturalDuration.TimeSpan;
                        TopTime.Maximum = tsTop.TotalSeconds;
                        //Log(string.Format("Total Seconds: {0}", tsProbe.TotalSeconds));
                        //Log(string.Format("Slider Max: {0}", tsProbe.TotalSeconds));
                        TopTime.SmallChange = 1;
                        TopTime.LargeChange = Math.Min(10, tsTop.Seconds / 10);
                    }

                    topTimer.Start();
                }

                /// <summary>
                /// Method called when the media player opens
                /// </summary>
                private void bottomPlayer_MediaOpened(object sender, EventArgs e)
                {
                    if (bottomPlayer.NaturalDuration.HasTimeSpan)
                    {
                        TimeSpan tsBottom = bottomPlayer.NaturalDuration.TimeSpan;
                        BottomTime.Maximum = tsBottom.TotalSeconds;
                        //Log(string.Format("Total Seconds: {0}", tsProbe.TotalSeconds));
                        //Log(string.Format("Slider Max: {0}", tsProbe.TotalSeconds));
                        BottomTime.SmallChange = 1;
                        BottomTime.LargeChange = Math.Min(10, tsBottom.Seconds / 10);
                    }

                    bottomTimer.Start();
                }

                /// <summary>
                /// Method called when the media player opens
                /// </summary>
                private void resultPlayer_MediaOpened(object sender, EventArgs e)
                {
                    if (resultPlayer.NaturalDuration.HasTimeSpan)
                    {
                        TimeSpan tsResult = resultPlayer.NaturalDuration.TimeSpan;
                        ResultTime.Maximum = tsResult.TotalSeconds;
                        //Log(string.Format("Total Seconds: {0}", tsProbe.TotalSeconds));
                        //Log(string.Format("Slider Max: {0}", tsProbe.TotalSeconds));
                        ResultTime.SmallChange = 1;
                        ResultTime.LargeChange = Math.Min(10, tsResult.Seconds / 10);
                    }

                    resultTimer.Start();
                }

                /// <summary>
                /// Method called when the media player opens
                /// </summary>
                private void verifyPlayer_MediaOpened(object sender, EventArgs e)
                {
                    if (verifyPlayer.NaturalDuration.HasTimeSpan)
                    {
                        TimeSpan tsVerify = verifyPlayer.NaturalDuration.TimeSpan;
                        VerifyTime.Maximum = tsVerify.TotalSeconds;
                        VerifyTime.SmallChange = 1;
                        VerifyTime.LargeChange = Math.Min(10, tsVerify.Seconds / 10);
                    }

                    verifyTimer.Start();
                }

            #endregion

            #region Timer/Tick Functions

                /// <summary>
                /// Method called for each tick of the media timer
                /// </summary>
                private void probeTick(object sender, EventArgs e)
                {
                    try
                    {
                        if (probePlayer.Source != null)
                        {
                            if (probeSlide)
                            {
                                IdentificationPlayerStatus.Content = string.Format("{0} / {1}", TimeSpan.FromSeconds(IdentTime.Value).ToString(@"mm\:ss"), probePlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                            else
                            {
                                IdentTime.Value = probePlayer.Position.TotalSeconds;
                                IdentificationPlayerStatus.Content = string.Format("{0} / {1}", probePlayer.Position.ToString(@"mm\:ss"), probePlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                        }
                        else { IdentificationPlayerStatus.Content = "No File Loaded"; }
                    }
                    catch { }
                }

                /// <summary>
                /// Method called for each tick of the media timer
                /// </summary>
                private void topTick(object sender, EventArgs e)
                {
                    try
                    {
                        if (topPlayer.Source != null)
                        {
                            if (topSlide)
                            {
                                TopPlayerStatus.Content = string.Format("{0} / {1}", TimeSpan.FromSeconds(TopTime.Value).ToString(@"mm\:ss"), topPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                            else
                            {
                                TopTime.Value = topPlayer.Position.TotalSeconds;
                                TopPlayerStatus.Content = string.Format("{0} / {1}", topPlayer.Position.ToString(@"mm\:ss"), topPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                        }
                        else { TopPlayerStatus.Content = "No File Selected"; }
                    }
                    catch { }
                }

                /// <summary>
                /// Method called for each tick of the media timer
                /// </summary>
                private void bottomTick(object sender, EventArgs e)
                {
                    try
                    {
                        if (bottomPlayer.Source != null)
                        {
                            if (bottomSlide)
                            {
                                BottomPlayerStatus.Content = string.Format("{0} / {1}", TimeSpan.FromSeconds(BottomTime.Value).ToString(@"mm\:ss"), bottomPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                            else
                            {
                                BottomTime.Value = bottomPlayer.Position.TotalSeconds;
                                BottomPlayerStatus.Content = string.Format("{0} / {1}", bottomPlayer.Position.ToString(@"mm\:ss"), bottomPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                        }
                        else { BottomPlayerStatus.Content = "No File Selected"; }
                    }
                    catch { }
                }

                /// <summary>
                /// Method called for each tick of the media timer
                /// </summary>
                private void resultTick(object sender, EventArgs e)
                {
                    try
                    {
                        if (resultPlayer.Source != null)
                        {
                            if (resultSlide)
                            {
                                ResultPlayerStatus.Content = string.Format("{0} / {1}", TimeSpan.FromSeconds(ResultTime.Value).ToString(@"mm\:ss"), resultPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                            else
                            {
                                ResultTime.Value = resultPlayer.Position.TotalSeconds;
                                ResultPlayerStatus.Content = string.Format("{0} / {1}", resultPlayer.Position.ToString(@"mm\:ss"), resultPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                        }
                        else { ResultPlayerStatus.Content = "No Result Loaded"; }
                    }
                    catch { }
                }

                /// <summary>
                /// Method called for each tick of the media timer
                /// </summary>
                private void verifyTick(object sender, EventArgs e)
                {
                    try
                    {
                        if (verifyPlayer.Source != null)
                        {
                            if (verifySlide)
                            {
                                VerifyPlayerStatus.Content = string.Format("{0} / {1}", TimeSpan.FromSeconds(VerifyTime.Value).ToString(@"mm\:ss"), verifyPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                            else
                            {
                                VerifyTime.Value = verifyPlayer.Position.TotalSeconds;
                                VerifyPlayerStatus.Content = string.Format("{0} / {1}", verifyPlayer.Position.ToString(@"mm\:ss"), verifyPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
                            }
                        }
                        else { VerifyPlayerStatus.Content = "No File Loaded"; }
                    }
                    catch { }
                }

            #endregion

            #region Play/Pause/Stop Functions

                /// <summary>
                /// The media player play button behavior
                /// </summary>
                private void IdentPlay_Click(object sender, RoutedEventArgs e)
                {
                    probePlayer.Play();
                }

                /// <summary>
                /// The media player pause button behavior
                /// </summary>
                private void IdentPause_Click(object sender, RoutedEventArgs e)
                {
                    probePlayer.Pause();
                }

                /// <summary>
                /// The media player stop button behavior
                /// </summary>
                private void IdentStop_Click(object sender, RoutedEventArgs e)
                {
                    probePlayer.Stop();
                }

                /// <summary>
                /// The media player play button behavior
                /// </summary>
                private void ResultPlay_Click(object sender, RoutedEventArgs e)
                {
                    resultPlayer.Play();
                }

                /// <summary>
                /// The media player pause button behavior
                /// </summary>
                private void ResultPause_Click(object sender, RoutedEventArgs e)
                {
                    resultPlayer.Pause();
                }

                /// <summary>
                /// The media player stop button behavior
                /// </summary>
                private void ResultStop_Click(object sender, RoutedEventArgs e)
                {
                    resultPlayer.Stop();
                }

                /// <summary>
                /// The media player play button behavior
                /// </summary>
                private void TopPlay_Click(object sender, RoutedEventArgs e)
                {
                    topPlayer.Play();
                }

                /// <summary>
                /// The media player pause button behavior
                /// </summary>
                private void TopPause_Click(object sender, RoutedEventArgs e)
                {
                    topPlayer.Pause();
                }

                /// <summary>
                /// The media player stop button behavior
                /// </summary>
                private void TopStop_Click(object sender, RoutedEventArgs e)
                {
                    topPlayer.Stop();
                }

                /// <summary>
                /// The media player play button behavior
                /// </summary>
                private void BottomPlay_Click(object sender, RoutedEventArgs e)
                {
                    bottomPlayer.Play();
                }

                /// <summary>
                /// The media player pause button behavior
                /// </summary>
                private void BottomPause_Click(object sender, RoutedEventArgs e)
                {
                    bottomPlayer.Pause();
                }

                /// <summary>
                /// The media player stop button behavior
                /// </summary>
                private void BottomStop_Click(object sender, RoutedEventArgs e)
                {
                    bottomPlayer.Stop();
                }

                /// <summary>
                /// The media player play button behavior
                /// </summary>
                private void VerifyPlay_Click(object sender, RoutedEventArgs e)
                {
                    verifyPlayer.Play();
                }

                /// <summary>
                /// The media player pause button behavior
                /// </summary>
                private void VerifyPause_Click(object sender, RoutedEventArgs e)
                {
                    verifyPlayer.Pause();
                }

                /// <summary>
                /// The media player stop button behavior
                /// </summary>
                private void VerifyStop_Click(object sender, RoutedEventArgs e)
                {
                    verifyPlayer.Stop();
                }

            #endregion

            #region Drag Functions

                /// <summary>
                /// Slider drag behavior for the media player
                /// </summary>
                private void IdentTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
                {
                    probeSlide = true;
                    probePlayer.Pause();
                }

                /// <summary>
                /// Slider end drag behavior for the media player
                /// </summary>
                private void IdentTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
                {
                    probeSlide = false;
                    probePlayer.Position = TimeSpan.FromSeconds(IdentTime.Value);
                    probePlayer.Play();
                }

                /// <summary>
                /// Slider drag behavior for the media player
                /// </summary>
                private void TopTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
                {
                    topSlide = true;
                    topPlayer.Pause();
                }

                /// <summary>
                /// Slider end drag behavior for the media player
                /// </summary>
                private void TopTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
                {
                    topSlide = false;
                    topPlayer.Position = TimeSpan.FromSeconds(TopTime.Value);
                    topPlayer.Play();
                }

                /// <summary>
                /// Slider drag behavior for the media player
                /// </summary>
                private void BottomTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
                {
                    bottomSlide = true;
                    bottomPlayer.Pause();
                }

                /// <summary>
                /// Slider end drag behavior for the media player
                /// </summary>
                private void BottomTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
                {
                    bottomSlide = false;
                    bottomPlayer.Position = TimeSpan.FromSeconds(BottomTime.Value);
                    bottomPlayer.Play();
                }

                /// <summary>
                /// Slider drag behavior for the media player
                /// </summary>
                private void ResultTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
                {
                    resultSlide = true;
                    resultPlayer.Pause();
                }

                /// <summary>
                /// Slider end drag behavior for the media player
                /// </summary>
                private void ResultTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
                {
                    resultSlide = false;
                    resultPlayer.Position = TimeSpan.FromSeconds(ResultTime.Value);
                    resultPlayer.Play();
                }

                /// <summary>
                /// Slider drag behavior for the media player
                /// </summary>
                private void VerifyTime_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
                {
                    verifySlide = true;
                    verifyPlayer.Pause();
                }

                /// <summary>
                /// Slider end drag behavior for the media player
                /// </summary>
                private void VerifyTime_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
                {
                    verifySlide = false;
                    verifyPlayer.Position = TimeSpan.FromSeconds(VerifyTime.Value);
                    verifyPlayer.Play();
                }

            #endregion

            /// <summary>
            /// Loads a result audio into the result media player
            /// </summary>
            private void LoadResult_Click(object sender, RoutedEventArgs e)
            {
                int index = ((int)((System.Windows.Controls.Button)sender).Tag) - 1;

                if (matchList[index].Filename.Equals(string.Empty) || !File.Exists(matchList[index].Filename))
                {
                    System.Windows.MessageBox.Show("No audio playback was found", "Could Not Load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(matchList[index].Filename);
                    string[] splitName = fileName.Split('_');
                    ResultRID.Content = "Playing: " + splitName[0];

                    resultPlayer.Open(new Uri(matchList[index].Filename));
                    resultTimer = new DispatcherTimer();
                    resultTimer.Interval = TimeSpan.FromSeconds(1);
                    resultTimer.Tick += resultTick;
                }
            }

        #endregion

        #region Scanning Functions

            #region Identify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for identification
                /// </summary>
                private void IdentifyScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = IdentificationScanPath.Text.Split('_')[0];
                    identScanFiles = new List<string>();

                    identScanFiles = ScanFiles(rid, IdentifyVoiceSelect);

                    if (identScanFiles.Count > 0)
                    {
                        IdentifyImport.IsEnabled = false;
                        IdentificationImportPath.IsEnabled = false;
                        IdentifyScan.IsEnabled = false;
                        IdentificationScanPath.IsEnabled = false;

                        IdentifyVoiceSelect.IsEnabled = true;
                        IdentificationClearWindow.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void IdentifySelect_Click(object sender, RoutedEventArgs e)
                {
                    IdentifyVoiceSelect.IsEnabled = false;
                    IdentifyDeselect.IsEnabled = true;
                    IdentifySelect.IsEnabled = false;

                    GetTemplate(identScanFiles[IdentifyVoiceSelect.SelectedIndex], out identSubject);

                    probePlayer.Open(new Uri(identScanFiles[IdentifyVoiceSelect.SelectedIndex]));
                    //probeTimer = new DispatcherTimer();
                    probeTimer.Interval = TimeSpan.FromSeconds(1);
                    probeTimer.Tick += probeTick;

                    IdentificationRID.Content = "Playing: " + identSubject.Id;

                    EnableIdentify();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void IdentifyDeselect_Click(object sender, RoutedEventArgs e)
                {
                    IdentifyVoiceSelect.IsEnabled = true;
                    IdentifyDeselect.IsEnabled = false;
                    IdentifySelect.IsEnabled = true;

                    identSubject = null;

                    probePlayer.Close();
                    IdentificationRID.Content = "---";

                    GenerateMatches.IsEnabled = false;
                }

            #endregion

            #region Top Verify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for the left verification subject
                /// </summary>
                private void TopScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = TopScanPath.Text.Split('_')[0];
                    topScanFiles = new List<string>();

                    topScanFiles = ScanFiles(rid, TopVoiceSelect);

                    if (topScanFiles.Count > 0)
                    {
                        VerificationImportTop.IsEnabled = false;
                        VerificationImportPathTop.IsEnabled = false;
                        TopScan.IsEnabled = false;
                        TopScanPath.IsEnabled = false;

                        TopVoiceSelect.IsEnabled = true;
                        VerificationClearTop.IsEnabled = true;
                        VerificationClear.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void TopSelect_Click(object sender, RoutedEventArgs e)
                {
                    TopVoiceSelect.IsEnabled = false;
                    TopDeselect.IsEnabled = true;
                    TopSelect.IsEnabled = false;

                    GetTemplate(topScanFiles[TopVoiceSelect.SelectedIndex], out verSubjectTop);

                    topPlayer.Open(new Uri(topScanFiles[TopVoiceSelect.SelectedIndex]));
                    //topTimer = new DispatcherTimer();
                    topTimer.Interval = TimeSpan.FromSeconds(1);
                    topTimer.Tick += topTick;

                    TopRID.Content = "Playing: " + verSubjectTop.Id;

                    EnableVerify();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void TopDeselect_Click(object sender, RoutedEventArgs e)
                {
                    TopVoiceSelect.IsEnabled = true;
                    TopDeselect.IsEnabled = false;
                    TopSelect.IsEnabled = true;

                    verSubjectTop = null;

                    topPlayer.Close();
                    TopRID.Content = "---";

                    VerifyVoices.IsEnabled = false;
                }

            #endregion

            #region Bottom Verify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for the right verification subject
                /// </summary>
                private void BottomScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = BottomScanPath.Text.Split('_')[0];
                    bottomScanFiles = new List<string>();

                    bottomScanFiles = ScanFiles(rid, BottomVoiceSelect);

                    if (bottomScanFiles.Count > 0)
                    {
                        VerificationImportBottom.IsEnabled = false;
                        VerificationImportPathBottom.IsEnabled = false;
                        BottomScan.IsEnabled = false;
                        BottomScanPath.IsEnabled = false;

                        BottomVoiceSelect.IsEnabled = true;
                        VerificationClearBottom.IsEnabled = true;
                        VerificationClear.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void BottomSelect_Click(object sender, RoutedEventArgs e)
                {
                    BottomVoiceSelect.IsEnabled = false;
                    BottomDeselect.IsEnabled = true;
                    BottomSelect.IsEnabled = false;

                    GetTemplate(bottomScanFiles[BottomVoiceSelect.SelectedIndex], out verSubjectBottom);

                    bottomPlayer.Open(new Uri(bottomScanFiles[BottomVoiceSelect.SelectedIndex]));
                    //bottomTimer = new DispatcherTimer();
                    bottomTimer.Interval = TimeSpan.FromSeconds(1);
                    bottomTimer.Tick += bottomTick;

                    BottomRID.Content = "Playing: " + verSubjectBottom.Id;

                    EnableVerify();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void BottomDeselect_Click(object sender, RoutedEventArgs e)
                {
                    BottomVoiceSelect.IsEnabled = true;
                    BottomDeselect.IsEnabled = false;
                    BottomSelect.IsEnabled = true;

                    verSubjectBottom = null;

                    bottomPlayer.Close();
                    BottomRID.Content = "---";

                    VerifyVoices.IsEnabled = false;
                }

            #endregion

            #region Verify Scan/Select/Deselect Functions

                /// <summary>
                /// Scans in the subject for the right verification subject
                /// </summary>
                private void VerifyScan_Click(object sender, RoutedEventArgs e)
                {
                    string rid = VerifyScanPath.Text.Split('_')[0];
                    verifyScanFiles = new List<string>();

                    verifyScanFiles = ScanFiles(rid, VerifyVoiceSelect);

                    if (verifyScanFiles.Count > 0)
                    {
                        VerifyImport.IsEnabled = false;
                        VerifyImportPath.IsEnabled = false;
                        VerifyScan.IsEnabled = false;
                        VerifyScanPath.IsEnabled = false;

                        VerifyVoiceSelect.IsEnabled = true;
                        VerifySubject_Clear.IsEnabled = true;

                        Log("");
                        Log(string.Format("Scanned in Subject ({0})", rid));
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) did not contain any demo files", rid));
                    }
                }

                /// <summary>
                /// Selects the currently listed image and imports their template
                /// </summary>
                private void VerifySelect_Click(object sender, RoutedEventArgs e)
                {
                    VerifyVoiceSelect.IsEnabled = false;
                    VerifyDeselect.IsEnabled = true;
                    VerifySelect.IsEnabled = false;

                    GetTemplate(verifyScanFiles[VerifyVoiceSelect.SelectedIndex], out verifySubject);

                    verifyPlayer.Open(new Uri(verifyScanFiles[VerifyVoiceSelect.SelectedIndex]));
                    //verifyTimer = new DispatcherTimer();
                    verifyTimer.Interval = TimeSpan.FromSeconds(1);
                    verifyTimer.Tick += verifyTick;

                    VerifyRID.Content = "Playing: " + verifySubject.Id;

                    EnableVerifyN();
                }

                /// <summary>
                /// Clears the current image/subject template
                /// </summary>
                private void VerifyDeselect_Click(object sender, RoutedEventArgs e)
                {
                    VerifyVoiceSelect.IsEnabled = true;
                    VerifyDeselect.IsEnabled = false;
                    VerifySelect.IsEnabled = true;

                    verifySubject = null;

                    verifyPlayer.Close();
                    VerifyRID.Content = "---";

                    VerifySubjectVoice.IsEnabled = false;
                }

            #endregion

            #region Selection Changed Functions

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void IdentifyVoiceSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    IdentifySelect.IsEnabled = true;
                }

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void TopVoiceSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    TopSelect.IsEnabled = true;
                }

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void BottomVoiceSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    BottomSelect.IsEnabled = true;
                }

                /// <summary>
                /// Actiavtes the select button when the selected image changes
                /// </summary>
                private void VerifyVoiceSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    VerifySelect.IsEnabled = true;
                }

            #endregion

            #region Enter Key Functions

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void TopScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        TopScan_Click(null, null);
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void BottomScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        BottomScan_Click(null, null);
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void IdentifyScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        IdentifyScan_Click(null, null);
                        e.Handled = true;
                    }
                }

                /// <summary>
                /// Activates the scanning process when the enter key is pressed
                /// </summary>
                private void VerifyScanPath_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
                {
                    if (e.Key == System.Windows.Input.Key.Enter)
                    {
                        VerifyScan_Click(null, null);
                        e.Handled = true;
                    }
                }

            #endregion

            /// <summary>
            /// Finds all demo files for an RID and loads it into a combobox
            /// </summary>
            /// <param name="RID">The RID identifier for a subject</param>
            /// <param name="combo">The combobox object to write to</param>
            /// <returns>The list of demo files found</returns>
            private List<string> ScanFiles(string RID, System.Windows.Controls.ComboBox combo)
            {
                List<string> scannedFiles = new List<string>();
                string foundVoice;

                if (Directory.Exists(defaultDatabases[defaultDatabases.Count - 1] + "\\" + RID))
                {
                    // Counters, in case we are unable to extract a date from the file path
                    int normalCount = 0;
                    int disguiseCount = 0;

                    // Gets all "Date" folders from RID
                    //string[] subDirectories = Directory.GetDirectories(defaultDatabases[defaultDatabases.Count-1] + "\\" + RID);
                    foreach (string dateFolder in Directory.GetDirectories(defaultDatabases[defaultDatabases.Count - 1] + "\\" + RID))
                    {
                        bool dateExtracted = false;
                        int day = 0;
                        int month = 0;
                        int year = 0;

                        // Try to extract the date out of the directory name
                        try
                        {
                            string dateFolderName = new DirectoryInfo(dateFolder).Name;
                            month = Int32.Parse(dateFolderName.Substring(0, 2));
                            day = Int32.Parse(dateFolderName.Substring(2, 2));
                            year = Int32.Parse(dateFolderName.Substring(6, 2));

                            dateExtracted = true;
                        }
                        catch { }

                        try
                        {
                            // Try to get the normal voice
                            foundVoice = Directory.GetFiles(dateFolder + @"\PVI1", "*_NRBO*.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                            if (foundVoice != null)
                            {
                                normalCount++;

                                if (dateExtracted)
                                    combo.Items.Add(string.Format("Normal Voice ({0}/{1}/{2})", month, day, year));
                                else
                                    combo.Items.Add(string.Format("Normal Voice (#{0})", normalCount));

                                scannedFiles.Add(foundVoice);
                            }
                        }
                        catch { }

                        try
                        {
                            // Try to get the disguised voice
                            foundVoice = Directory.GetFiles(dateFolder + @"\PVI1", "*_DRBO*.wav", SearchOption.TopDirectoryOnly).FirstOrDefault();
                            if (foundVoice != null)
                            {
                                disguiseCount++;

                                if (dateExtracted)
                                    combo.Items.Add(string.Format("Disguised Voice ({0}/{1}/{2})", month, day, year));
                                else
                                    combo.Items.Add(string.Format("Disguised Voice (#{0})", disguiseCount));

                                scannedFiles.Add(foundVoice);
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("The RID ({0}) was not recognized", RID));
                }

                return scannedFiles;
            }

        #endregion

    }
}
