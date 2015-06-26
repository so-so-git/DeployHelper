using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using DeployHelper.Commons;

namespace DeployHelper.Converters
{
    /// <summary>
    /// 配置状態を受け取り、配置状態名称に変換するコンバータです。
    /// </summary>
    public class DeployStatusConverter : IValueConverter
    {
        /// <summary>
        /// コンバート処理を行います。
        /// </summary>
        /// <param name="value">元値</param>
        /// <param name="targetType">対象の型</param>
        /// <param name="parameter">パラメータ</param>
        /// <param name="culture">カルチャー情報</param>
        /// <returns>配置状態名称</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DeployStatus)value).ToName();
        }

        /// <summary>
        /// 未実装です。
        /// </summary>
        /// <param name="value">元値</param>
        /// <param name="targetType">対象の型</param>
        /// <param name="parameter">パラメータ</param>
        /// <param name="culture">カルチャー情報</param>
        /// <returns>カルチャー情報</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
