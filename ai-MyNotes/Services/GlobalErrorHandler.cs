using ai_MyNotes.Services.Exceptions;
using Microsoft.JSInterop;
using System.Text.Json;

namespace ai_MyNotes.Services
{
    /// <summary>
    /// グローバルエラーハンドラー
    /// アプリケーション全体で捕捉されない例外を処理
    /// </summary>
    public class GlobalErrorHandler
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly Queue<ErrorRecord> _unhandledErrors;
        private readonly int _maxErrorQueueSize = 50;

        public event EventHandler<ErrorEventArgs>? OnUnhandledException;
        public event EventHandler<ErrorEventArgs>? OnCriticalError;

        public GlobalErrorHandler(IJSRuntime jsRuntime, IErrorHandlingService errorHandlingService)
        {
            _jsRuntime = jsRuntime;
            _errorHandlingService = errorHandlingService;
            _unhandledErrors = new Queue<ErrorRecord>();
        }

        /// <summary>
        /// グローバルエラーハンドラーを初期化
        /// </summary>
        public void Initialize()
        {
            // .NET例外ハンドリングの設定
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            
            // JavaScript例外ハンドリングの設定
            _ = Task.Run(async () => await SetupJavaScriptErrorHandlingAsync());
        }

        /// <summary>
        /// 手動でエラーを報告
        /// </summary>
        /// <param name="exception">例外</param>
        /// <param name="context">コンテキスト</param>
        /// <param name="isCritical">クリティカルエラーかどうか</param>
        public async Task ReportErrorAsync(Exception exception, string context = "", bool isCritical = false)
        {
            try
            {
                var errorMessage = await _errorHandlingService.HandleErrorAsync(exception, context);
                var errorRecord = new ErrorRecord
                {
                    Exception = exception,
                    Context = context,
                    ErrorMessage = errorMessage,
                    Timestamp = DateTime.Now,
                    IsCritical = isCritical
                };

                // エラーキューに追加
                AddToErrorQueue(errorRecord);

                // イベント通知
                var eventArgs = new ErrorEventArgs(errorRecord);
                if (isCritical)
                {
                    OnCriticalError?.Invoke(this, eventArgs);
                }
                else
                {
                    OnUnhandledException?.Invoke(this, eventArgs);
                }

                // クリティカルエラーの場合はJavaScriptに通知
                if (isCritical)
                {
                    await NotifyJavaScriptOfCriticalErrorAsync(errorRecord);
                }
            }
            catch (Exception ex)
            {
                // エラーハンドラー自体でエラーが発生した場合
                System.Diagnostics.Debug.WriteLine($"Error in GlobalErrorHandler: {ex}");
                await LogToConsoleAsync($"Critical: Error handler failed: {ex.Message}");
            }
        }

        /// <summary>
        /// 未処理エラーのリストを取得
        /// </summary>
        /// <param name="limit">取得件数制限</param>
        /// <returns>エラーレコードのリスト</returns>
        public List<ErrorRecord> GetUnhandledErrors(int limit = 20)
        {
            return _unhandledErrors.TakeLast(limit).ToList();
        }

        /// <summary>
        /// エラーキューをクリア
        /// </summary>
        public void ClearErrorQueue()
        {
            _unhandledErrors.Clear();
        }

        /// <summary>
        /// エラー統計を取得
        /// </summary>
        /// <returns>エラー統計情報</returns>
        public ErrorStatistics GetErrorStatistics()
        {
            var errors = _unhandledErrors.ToList();
            var now = DateTime.Now;
            var last24Hours = errors.Where(e => (now - e.Timestamp).TotalHours <= 24).ToList();
            var lastHour = errors.Where(e => (now - e.Timestamp).TotalMinutes <= 60).ToList();

            return new ErrorStatistics
            {
                TotalErrors = errors.Count,
                ErrorsLast24Hours = last24Hours.Count,
                ErrorsLastHour = lastHour.Count,
                CriticalErrors = errors.Count(e => e.IsCritical),
                MostCommonError = GetMostCommonErrorType(errors),
                ErrorsByType = GetErrorsByType(errors)
            };
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                _ = Task.Run(async () => await ReportErrorAsync(exception, "AppDomain.UnhandledException", e.IsTerminating));
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                _ = Task.Run(async () => await ReportErrorAsync(e.Exception, "TaskScheduler.UnobservedTaskException", false));
                e.SetObserved(); // 例外を処理済みとしてマーク
            }
        }

        private async Task SetupJavaScriptErrorHandlingAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("eval", @"
                    window.addEventListener('error', function(event) {
                        console.error('Global JavaScript error:', event.error);
                        if (window.blazorErrorHandler) {
                            window.blazorErrorHandler.reportJavaScriptError(event.error.toString(), event.filename, event.lineno);
                        }
                    });

                    window.addEventListener('unhandledrejection', function(event) {
                        console.error('Unhandled promise rejection:', event.reason);
                        if (window.blazorErrorHandler) {
                            window.blazorErrorHandler.reportJavaScriptError('Promise rejection: ' + event.reason, 'unknown', 0);
                        }
                    });

                    window.blazorErrorHandler = {
                        reportJavaScriptError: function(message, filename, lineno) {
                            // Blazorのエラーハンドラーに通知する処理
                            // 実際の実装では DotNet.invokeMethodAsync を使用
                        }
                    };
                ");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to setup JavaScript error handling: {ex}");
            }
        }

        private void AddToErrorQueue(ErrorRecord errorRecord)
        {
            _unhandledErrors.Enqueue(errorRecord);
            
            // キューサイズ制限
            while (_unhandledErrors.Count > _maxErrorQueueSize)
            {
                _unhandledErrors.Dequeue();
            }
        }

        private async Task NotifyJavaScriptOfCriticalErrorAsync(ErrorRecord errorRecord)
        {
            try
            {
                var errorInfo = new
                {
                    Message = errorRecord.ErrorMessage.Message,
                    Title = errorRecord.ErrorMessage.Title,
                    ErrorCode = errorRecord.ErrorMessage.ErrorCode,
                    Timestamp = errorRecord.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await _jsRuntime.InvokeVoidAsync("console.error", "Critical error reported:", JsonSerializer.Serialize(errorInfo));
            }
            catch
            {
                // JavaScript通知に失敗してもアプリケーションは継続
            }
        }

        private async Task LogToConsoleAsync(string message)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("console.error", $"[GlobalErrorHandler] {message}");
            }
            catch
            {
                // ログ出力に失敗してもアプリケーションは継続
            }
        }

        private string GetMostCommonErrorType(List<ErrorRecord> errors)
        {
            if (!errors.Any()) return "なし";

            return errors
                .GroupBy(e => e.Exception.GetType().Name)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        private Dictionary<string, int> GetErrorsByType(List<ErrorRecord> errors)
        {
            return errors
                .GroupBy(e => e.Exception.GetType().Name)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }

    /// <summary>
    /// エラーレコード
    /// </summary>
    public class ErrorRecord
    {
        public Exception Exception { get; set; } = null!;
        public string Context { get; set; } = string.Empty;
        public ErrorMessage ErrorMessage { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public bool IsCritical { get; set; }
    }

    /// <summary>
    /// エラーイベント引数
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        public ErrorRecord ErrorRecord { get; }

        public ErrorEventArgs(ErrorRecord errorRecord)
        {
            ErrorRecord = errorRecord;
        }
    }

    /// <summary>
    /// エラー統計情報
    /// </summary>
    public class ErrorStatistics
    {
        public int TotalErrors { get; set; }
        public int ErrorsLast24Hours { get; set; }
        public int ErrorsLastHour { get; set; }
        public int CriticalErrors { get; set; }
        public string MostCommonError { get; set; } = string.Empty;
        public Dictionary<string, int> ErrorsByType { get; set; } = new();
    }
}