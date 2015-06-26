using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

using DeployHelper.Commons;

namespace DeployHelper.Converters
{
    /// <summary>
    /// 配置状態を受け取り、それに応じたVisibilityに変換するコンバータです。
    /// </summary>
    public class StatusVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// コンバート処理を行います。
        /// </summary>
        /// <param name="value">元値</param>
        /// <param name="targetType">対象の型</param>
        /// <param name="parameter">パラメータ</param>
        /// <param name="culture">カルチャー情報</param>
        /// <returns>配置状態に応じたVisibility</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (DeployStatus)value;

            if (!status.CanSelect())
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
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
