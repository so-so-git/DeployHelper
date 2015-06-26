using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeployHelper.Commons
{
    /// <summary>
    /// 配置状態列挙体
    /// </summary>
    public enum DeployStatus
    {
        /// <summary>未ビルド</summary>
        NoBuild,
        /// <summary>未配置</summary>
        NoDeploy,
        /// <summary>更新</summary>
        Updated,
        /// <summary>デグレード</summary>
        Degrade,
        /// <summary>配置済み</summary>
        Deployed,
    }

    /// <summary>
    /// 配置状態拡張処理定義クラス
    /// </summary>
    public static class DeployStatusExtensions
    {
        /// <summary>
        /// 配置状態を配置状態名に変換します。
        /// </summary>
        /// <param name="status">配置状態</param>
        /// <returns>配置状態名</returns>
        public static string ToName(this DeployStatus status)
        {
            switch (status)
            {
                case DeployStatus.NoBuild:
                    return "未ビルド";

                case DeployStatus.NoDeploy:
                    return "未配置";

                case DeployStatus.Updated:
                    return "更新";

                case DeployStatus.Degrade:
                    return "デグレード";

                case DeployStatus.Deployed:
                    return "配置済み";

                default:
                    throw new ArgumentException("InvalidArgument:GetStatusName");
            }
        }

        /// <summary>
        /// 指定された配置状態のグリッド行を選択可能とするかのフラグを取得します。
        /// </summary>
        /// <param name="status">配置状態</param>
        /// <returns>グリッド行を選択可能とするかのフラグ</returns>
        public static bool CanSelect(this DeployStatus status)
        {
            switch (status)
            {
                case DeployStatus.NoBuild:
                    return false;

                case DeployStatus.NoDeploy:
                case DeployStatus.Updated:
                case DeployStatus.Degrade:
                case DeployStatus.Deployed:
                    return true;

                default:
                    throw new ArgumentException("InvalidArgument:GetStatusName");
            }
        }
    }
}
