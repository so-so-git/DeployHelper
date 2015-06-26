using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeployHelper.Commons
{
    /// <summary>
    /// アプリケーション設定値クラス
    /// </summary>
    public static class ConfigValues
    {
        /// <summary>
        /// ソースのルートパス設定を取得します。
        /// </summary>
        public static string RootPath { get; private set; }

        /// <summary>
        /// 配置処理の対象とするバイナリファイルの拡張子設定の配列を取得します。
        /// </summary>
        public static string[] TargetExtensions { get; private set; }

        /// <summary>
        /// 配置処理の対象とするビルド構成設定の配列を取得します。
        /// </summary>
        public static string[] TargetConfigs { get; private set; }

        /// <summary>
        /// 配置処理の対象とするプラットフォーム設定を取得します。
        /// </summary>
        public static string TargetPlatform { get; private set; }

        /// <summary>
        /// 処理対象外とするディレクトリ名設定の配列を取得します。
        /// </summary>
        public static string[] IgnoreDirs { get; private set; }

        /// <summary>
        /// 配置先のパス設定を取得します。
        /// </summary>
        public static string DeployPath { get; private set; }

        /// <summary>
        /// PDBファイルを併せて配置するかのフラグを取得します。
        /// </summary>
        public static bool IsDeployPDB { get; private set; }

        /// <summary>
        /// 静的コンストラクタです。
        /// </summary>
        static ConfigValues()
        {
            var setting = ConfigurationManager.AppSettings;

            RootPath = setting["RootPath"];

            TargetExtensions = setting["TargetExtensions"].Split(',')
                .Select(e => e.Trim())
                .Select(e => e.StartsWith(".") ? e : "." + e).ToArray();

            TargetConfigs = setting["TargetConfigs"].Split(',')
                .Select(c => c.Trim()).ToArray();

            TargetPlatform = setting["TargetPlatform"];

            IgnoreDirs = setting["IgnoreDirs"].Split(',')
                .Select(d => d.Trim()).ToArray();

            DeployPath = setting["DeployPath"];

            bool parseResult;
            if (bool.TryParse(setting["IsDeployPDB"], out parseResult))
            {
                IsDeployPDB = parseResult;
            }
            else
            {
                IsDeployPDB = false;
            }
        }
    }
}
