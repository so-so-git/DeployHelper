using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Serialization;

using DeployHelper.Commons;

using IOPath = System.IO.Path;

namespace DeployHelper.Windows
{
    /// <summary>
    /// メイン画面クラス
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region インスタンス変数

        /// <summary>バイナリ情報グリッドのデータソース</summary>
        private ObservableCollection<BinaryInfo> _binaryInfoGridItemsSource;

        /// <summary>処理実行中かを示すフラグ</summary>
        private bool _isProcessing;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// デフォルトのコンストラクタです。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// ソースのルートパスを取得します。
        /// </summary>
        public string SourceRootPath { get { return "ソースのルート - " + ConfigValues.RootPath; } }

        /// <summary>
        /// 配置先のパスを取得します。
        /// </summary>
        public string DeployPath { get { return "配置先　　　　 - " + ConfigValues.DeployPath; } }

        /// <summary>
        /// 対象ビルド構成コンボボックスのデータソースを取得します。
        /// </summary>
        public string[] TargetConfigItemsSource { get { return ConfigValues.TargetConfigs; } }

        /// <summary>
        /// バイナリ情報グリッドのデータソースを取得または設定します。
        /// </summary>
        public ObservableCollection<BinaryInfo> BinaryInfoGridItemsSource
        {
            get { return _binaryInfoGridItemsSource; }
            set
            {
                if (_binaryInfoGridItemsSource == value)
                {
                    return;
                }

                _binaryInfoGridItemsSource = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("BinaryInfoGridItemsSource"));
                }
            }
        }

        /// <summary>
        /// 状態選択コンボボックスのデータソースを取得します。
        /// </summary>
        public List<KeyValuePair<DeployStatus, string>> SelectStatusItemsSource
        {
            get
            {
                var statusItemsSource = new List<KeyValuePair<DeployStatus, string>>();
                foreach (var statusObj in Enum.GetValues(typeof(DeployStatus)))
                {
                    var status = (DeployStatus)statusObj;

                    if (status.CanSelect())
                    {
                        statusItemsSource.Add(new KeyValuePair<DeployStatus, string>(status, status.ToName()));
                    }
                }

                return statusItemsSource;
            }
        }

        /// <summary>
        /// 処理実行中かを示すフラグを取得または設定します。
        /// </summary>
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                if (_isProcessing == value)
                {
                    return;
                }

                _isProcessing = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsProcessing"));
                }
            }
        }

        #endregion

        #region イベント

        /// <summary>
        /// プロパティ変更時のイベントです。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region バイナリ情報グリッド内容更新

        /// <summary>
        /// バイナリ情報グリッドの内容を最新状態に更新します。
        /// </summary>
        /// <param name="endCallback">処理完了時のコールバックデリゲート</param>
        private void RefreshBinaryInfoGrid(Action endCallback = null)
        {
            string targetConfig = cmbTargetConfig.SelectedValue.ToString();

            var bgWorker = new BackgroundWorker();

            bgWorker.DoWork += (sender, e) =>
                {
                    var rootDir = new DirectoryInfo(ConfigValues.RootPath);
                    List<BinaryInfo> binInfoList = GetBinaryInfoList(rootDir, e.Argument.ToString());

                    BinaryInfoGridItemsSource = new ObservableCollection<BinaryInfo>(binInfoList.OrderByDescending(i => i.BuildedTimestamp));
                };

            bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    IsProcessing = false;
                    this.Cursor = null;

                    if (endCallback != null)
                    {
                        endCallback();
                    }
                };

            IsProcessing = true;
            this.Cursor = Cursors.Wait;

            bgWorker.RunWorkerAsync(targetConfig);
        }

        #endregion

        #region バイナリ情報リスト取得

        /// <summary>
        /// 指定されたディレクトリ以下を再帰的に探索し、バイナリ情報のリストを取得します。
        /// </summary>
        /// <param name="dir">探索するディレクトリ</param>
        /// <returns>検出したバイナリ情報のリスト</returns>
        private List<BinaryInfo> GetBinaryInfoList(DirectoryInfo dir, string targetConfig)
        {
            if (ConfigValues.IgnoreDirs.Contains(dir.Name))
            {
                return new List<BinaryInfo>();
            }

            var pjInfoList = new List<BinaryInfo>();
            var deployDir = new DirectoryInfo(ConfigValues.DeployPath);

            foreach (var projFile in dir.GetFiles("*.vbproj"))
            {
                try
                {
                    string pjName = IOPath.GetFileNameWithoutExtension(projFile.Name);

                    string targetCondition = string.Format(
                        "'$(Configuration)|$(Platform)' == '{0}|{1}'",
                        targetConfig, ConfigValues.TargetPlatform);

                    var root = XDocument.Load(projFile.FullName).Root;
                    var ns = root.Name.Namespace;

                    var condPropGroup = (from e in root.Elements(ns + "PropertyGroup")
                                         where e.Attribute("Condition") != null
                                            && e.Attribute("Condition").Value.Trim() == targetCondition
                                         select e
                                        ).FirstOrDefault();
                    
                    if (condPropGroup == null)
                    {
                        targetCondition = string.Format(
                            "'$(Configuration)|$(Platform)' == '{0}|AnyCPU'",
                            targetConfig);

                        condPropGroup = (from e in root.Elements(ns + "PropertyGroup")
                                         where e.Attribute("Condition") != null
                                            && e.Attribute("Condition").Value.Trim() == targetCondition
                                         select e
                                        ).FirstOrDefault();
                    }

                    var outputDir = new DirectoryInfo(IOPath.Combine(
                        projFile.Directory.FullName, condPropGroup.Element(ns + "OutputPath").Value));

                    var commonPropGroup = (from e in root.Elements(ns + "PropertyGroup")
                                           where e.Attribute("Condition") == null
                                           select e
                                          ).FirstOrDefault();

                    var buildedFileName =
                        commonPropGroup.Element(ns + "AssemblyName").Value +
                        (commonPropGroup.Element(ns + "OutputType").Value == "Library" ? ".dll" : ".exe");

                    var pjInfo = new BinaryInfo();
                    pjInfo.BuildedFile = new FileInfo(IOPath.Combine(outputDir.FullName, buildedFileName));
                    pjInfo.DeployedFile = new FileInfo(IOPath.Combine(deployDir.FullName, pjInfo.BinaryName));
                    pjInfo.ProjectDirectory = projFile.Directory;
                    pjInfo.OutputDirectory = outputDir;
                    pjInfo.IsSelected = false;

                    pjInfoList.Add(pjInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            var childDirs = dir.GetDirectories();
            foreach (var childDir in childDirs)
            {
                pjInfoList.AddRange(GetBinaryInfoList(childDir, targetConfig));
            }

            return pjInfoList;
        }

        #endregion

        #region プロジェクトディレクトリを開く

        /// <summary>
        /// バイナリ情報グリッドで選択されている行のプロジェクトディレクトリを
        /// エクスプローラで開きます。
        /// </summary>
        private void OpenSelectedProjectDirectory()
        {
            var binInfo = grdBinaryInfo.SelectedItem as BinaryInfo;

            var psi = new ProcessStartInfo("explorer", binInfo.ProjectPath);

            var p = new Process();
            p.StartInfo = psi;

            p.Start();
        }

        #endregion

        /********** イベントハンドラ **********/

        #region 画面ロード時

        /// <summary>
        /// 画面ロード時の処理です。
        /// 画面の初期表示を行います。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshBinaryInfoGrid();
        }

        #endregion

        #region バイナリ情報グリッド行ロード時

        /// <summary>
        /// バイナリ情報グリッド行ロード時の処理です。
        /// グリッド行のイベントとコンテキストメニュー作成を行います。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void grdBinaryInfo_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseRightButtonDown -= new MouseButtonEventHandler(DataGridRow_MouseRightButtonDown);
            e.Row.MouseRightButtonDown += new MouseButtonEventHandler(DataGridRow_MouseRightButtonDown);

            e.Row.MouseDoubleClick -= new MouseButtonEventHandler(DataGridRow_MouseDoubleClick);
            e.Row.MouseDoubleClick += new MouseButtonEventHandler(DataGridRow_MouseDoubleClick);

            var menu = new ContextMenu();

            var openDirItem = new MenuItem();
            openDirItem.Header = "プロジェクトのディレクトリを開く";
            openDirItem.Click += (s, e2) => OpenSelectedProjectDirectory();

            menu.Items.Add(openDirItem);

            ContextMenuService.SetContextMenu(e.Row, menu);
        }

        #endregion

        #region バイナリ情報グリッド行右クリック時

        /// <summary>
        /// バイナリ情報グリッド行右クリック時の処理です。
        /// 右クリックした位置の行をカレント行に設定します。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void DataGridRow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            grdBinaryInfo.SelectedIndex = row.GetIndex();
        }

        #endregion

        #region バイナリ情報グリッド行ダブルクリック時

        /// <summary>
        /// バイナリ情報グリッド行ダブルクリック時の処理です。
        /// 対象の行のプロジェクトディレクトリをエクスプローラで開きます。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedProjectDirectory();
        }

        #endregion

        #region 対象ビルド構成コンボ選択変更時

        /// <summary>
        /// 対象ビルド構成コンボボックスの選択値変更時の処理です。
        /// 選択されたビルド構成を表示するよう、バイナリ情報グリッドの内容を更新します。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void cmbTargetConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                RefreshBinaryInfoGrid();
            }
        }

        #endregion

        #region 更新ボタン押下時

        /// <summary>
        /// 更新ボタン押下時の処理です。
        /// バイナリ情報グリッドの内容を最新状態に更新します。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshBinaryInfoGrid();
        }

        #endregion

        #region 状態選択ボタン押下時

        /// <summary>
        /// 状態選択ボタン押下時の処理です。
        /// 状態コンボボックスで選択されている配置状態に当てはまるグリッド行を選択状態にします。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void btnStatusSelect_Click(object sender, RoutedEventArgs e)
        {
            var selectStatus = (DeployStatus)cmbSelectStatus.SelectedValue;

            foreach (var binInfo in _binaryInfoGridItemsSource)
            {
                binInfo.IsSelected = binInfo.Status == selectStatus;
            }
        }

        #endregion

        #region 全選択ボタン押下時

        /// <summary>
        /// 全選択ボタン押下時の処理です。
        /// グリッドの全ての行を選択状態にします。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void btnAllSelect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var binInfo in _binaryInfoGridItemsSource)
            {
                if (binInfo.Status != DeployStatus.NoBuild)
                {
                    binInfo.IsSelected = true;
                }
            }
        }

        #endregion

        #region 全解除ボタン押下時

        /// <summary>
        /// 全解除ボタン押下時の処理です。
        /// グリッドの全ての行の選択を解除します。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void btnAllUnselect_Click(object sender, RoutedEventArgs e)
        {
            foreach (var binInfo in _binaryInfoGridItemsSource)
            {
                binInfo.IsSelected = false;
            }
        }

        #endregion

        #region 配置ボタン押下時

        /// <summary>
        /// 配置ボタン押下時の処理です。
        /// グリッド上で選択されているバイナリ情報の配置を行います。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void btnDeploy_Click(object sender, RoutedEventArgs e)
        {
            var selectedBinInfos = _binaryInfoGridItemsSource.Where(i => i.IsSelected);

            if (!selectedBinInfos.Any())
            {
                MessageBox.Show("配置の対象となるバイナリファイルが選択されていません。",
                    "対象未選択", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (selectedBinInfos.Any(i => i.Status == DeployStatus.Degrade)
                && MessageBox.Show(
                    "デグレードしている可能性があるバイナリが選択されています。\n配置を継続してよろしいですか？",
                    "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }

            IsProcessing = true;
            this.Cursor = Cursors.Wait;

            try
            {
                foreach (var pjInfo in selectedBinInfos)
                {
                    pjInfo.BuildedFile.CopyTo(pjInfo.DeployedFile.FullName, true);

                    if (ConfigValues.IsDeployPDB)
                    {
                        var pdbFile = new FileInfo(IOPath.Combine(
                            pjInfo.BuildedFile.Directory.FullName,
                            IOPath.GetFileNameWithoutExtension(pjInfo.BuildedFile.Name) + ".pdb"));

                        if (pdbFile.Exists)
                        {
                            var deployPath = IOPath.Combine(pjInfo.DeployedFile.Directory.FullName, pdbFile.Name);
                            pdbFile.CopyTo(deployPath, true);
                        }
                    }
                }
            }
            catch
            {
                IsProcessing = false;
                this.Cursor = null;

                throw;
            }

            RefreshBinaryInfoGrid(new Action(
                () =>
                {
                    IsProcessing = false;
                    this.Cursor = null;

                    MessageBox.Show("バイナリファイルの配置が完了しました。",
                        "処理完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            ));
        }

        #endregion
    }
}
