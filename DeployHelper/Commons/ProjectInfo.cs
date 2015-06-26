using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeployHelper.Windows;

namespace DeployHelper.Commons
{
    /// <summary>
    /// バイナリ情報クラス
    /// </summary>
    public class BinaryInfo : INotifyPropertyChanged
    {
        #region インスタンス変数

        /// <summary>グリッド行選択フラグ</summary>
        private bool _isSelected;

        /// <summary>ビルドされたバイナリファイル情報</summary>
        private FileInfo _buildedFile;

        /// <summary>配置されているバイナリファイル情報</summary>
        private FileInfo _deployedFile;

        #endregion

        #region プロパティ

        /// <summary>
        /// ビルドされたバイナリファイル情報を取得または設定します。
        /// </summary>
        public FileInfo BuildedFile
        {
            get { return _buildedFile; }
            set
            {
                _buildedFile = value;
                RefreshDeployStatus();
            }
        }

        /// <summary>
        /// 対象のバイナリファイルの名称を取得します。
        /// </summary>
        public string BinaryName
        {
            get
            {
                if (BuildedFile == null)
                {
                    return null;
                }
                return BuildedFile.Name;
            }
        }

        /// <summary>
        /// ビルドされたバイナリファイルのタイムスタンプを取得します。
        /// </summary>
        public DateTime? BuildedTimestamp
        {
            get
            {
                if (BuildedFile == null || !BuildedFile.Exists)
                {
                    return null;
                }
                return BuildedFile.LastWriteTime;
            }
        }

        /// <summary>
        /// 配置されているバイナリファイル情報を取得または設定します。
        /// </summary>
        public FileInfo DeployedFile
        {
            get { return _deployedFile; }
            set
            {
                _deployedFile = value;
                RefreshDeployStatus();
            }
        }

        /// <summary>
        /// 配置されているバイナリファイルのタイムスタンプを取得します。
        /// </summary>
        public DateTime? DeployedTimestamp
        {
            get
            {
                if (DeployedFile == null || !DeployedFile.Exists)
                {
                    return null;
                }
                return DeployedFile.LastWriteTime;
            }
        }

        /// <summary>
        /// 配置状態を取得します。
        /// </summary>
        public DeployStatus Status { get; private set; }

        /// <summary>
        /// バイナリファイルの生成元プロジェクトのディレクトリ情報を取得または設定します。
        /// </summary>
        public DirectoryInfo ProjectDirectory { get; set; }

        /// <summary>
        /// バイナリファイルの生成元プロジェクトのディレクトリパスを取得します。
        /// </summary>
        public string ProjectPath
        {
            get
            {
                if (ProjectDirectory == null)
                {
                    return null;
                }
                return ProjectDirectory.FullName;
            }
        }

        /// <summary>
        /// バイナリファイルビルド時の出力先ディレクトリ情報を取得または設定します。
        /// </summary>
        public DirectoryInfo OutputDirectory { get; set; }

        /// <summary>
        /// グリッド行選択フラグを取得または設定します。
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
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

        #region 配置状態更新

        /// <summary>
        /// 配置状態を更新します。
        /// </summary>
        private void RefreshDeployStatus()
        {
            if (BuildedFile == null || !BuildedFile.Exists)
            {
                Status = DeployStatus.NoBuild;
            }
            else if (DeployedFile == null || !DeployedFile.Exists)
            {
                Status = DeployStatus.NoDeploy;
            }
            else
            {
                if (BuildedTimestamp.Value > DeployedTimestamp.Value)
                {
                    Status = DeployStatus.Updated;
                }
                else if (BuildedTimestamp.Value < DeployedTimestamp.Value)
                {
                    Status = DeployStatus.Degrade;
                }
                else
                {
                    Status = DeployStatus.Deployed;
                }
            }
        }

        #endregion
    }
}
