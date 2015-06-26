using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DeployHelper
{
    /// <summary>
    /// アプリケーションクラス
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// アプリケーション開始時の処理です。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// アプリケーション終了時の処理です。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            this.DispatcherUnhandledException -= Application_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// ハンドルされていない例外の発生時の処理です。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            NotifyUnhandledException(e.Exception);
        }

        /// <summary>
        /// ハンドルされていない例外の発生時の処理です。
        /// </summary>
        /// <param name="sender">イベント発生元オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            NotifyUnhandledException(e.ExceptionObject as Exception);
        }

        /// <summary>
        /// ハンドルされていない例外の通知を行います。
        /// </summary>
        /// <param name="ex">例外オブジェクト</param>
        private void NotifyUnhandledException(Exception ex)
        {
            MessageBox.Show(ex.ToString(), "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}
