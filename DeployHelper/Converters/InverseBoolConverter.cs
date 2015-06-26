using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DeployHelper.Converters
{
    /// <summary>
    /// ブール値反転コンバータ
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        /// <summary>
        /// コンバート処理を行います。
        /// </summary>
        /// <param name="value">元値</param>
        /// <param name="targetType">対象の型</param>
        /// <param name="parameter">パラメータ</param>
        /// <param name="culture">カルチャー情報</param>
        /// <returns>反転したブール値</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !System.Convert.ToBoolean(value);
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
