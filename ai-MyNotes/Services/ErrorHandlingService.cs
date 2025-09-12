using ai_MyNotes.Services.Exceptions;
using Microsoft.JSInterop;

namespace ai_MyNotes.Services
{
    /// <summary>
    /// エラーハンドリングサービス
    /// アプリケーション全体のエラー処理を統一管理
    /// </summary>
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly List<ErrorLogEntry> _errorLog;

        public ErrorHandlingService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _errorLog = new List<ErrorLogEntry>();
        }

        /// <summary>
        /// エラーを処理し、適切なユーザーメッセージを返す
        /// </summary>
        /// <param name="exception">発生した例外</param>
        /// <param name="context">エラー発生コンテキスト</param>
        /// <returns>ユーザー向けエラーメッセージ</returns>
        public async Task<ErrorMessage> HandleErrorAsync(Exception exception, string context = "")
        {
            // エラーログに記録
            var logEntry = new ErrorLogEntry
            {
                Timestamp = DateTime.Now,
                Exception = exception,
                Context = context,
                ErrorCode = GetErrorCode(exception),
                UserAgent = await GetUserAgentAsync()
            };
            _errorLog.Add(logEntry);

            // コンソールにログ出力（開発用）
            await LogToConsoleAsync(logEntry);

            // ユーザー向けメッセージを生成
            return GenerateUserMessage(exception, context);
        }

        /// <summary>
        /// IndexedDB関連エラーのハンドリング
        /// </summary>
        /// <param name="exception">IndexedDB例外</param>
        /// <param name="operation">実行中の操作</param>
        /// <returns>処理済みエラーメッセージ</returns>
        public async Task<ErrorMessage> HandleIndexedDbErrorAsync(Exception exception, string operation)
        {
            // IndexedDB特有のエラーパターンを分析
            var indexedDbException = AnalyzeIndexedDbException(exception, operation);
            return await HandleErrorAsync(indexedDbException, $"IndexedDB operation: {operation}");
        }

        /// <summary>
        /// データ操作エラーのハンドリング
        /// </summary>
        /// <param name="exception">データ操作例外</param>
        /// <param name="operation">実行中の操作</param>
        /// <param name="resourceId">リソースID（オプション）</param>
        /// <returns>処理済みエラーメッセージ</returns>
        public async Task<ErrorMessage> HandleDataOperationErrorAsync(Exception exception, string operation, string? resourceId = null)
        {
            var dataException = AnalyzeDataOperationException(exception, operation, resourceId);
            return await HandleErrorAsync(dataException, $"Data operation: {operation}");
        }

        /// <summary>
        /// エラーログを取得
        /// </summary>
        /// <param name="limit">取得件数制限</param>
        /// <returns>エラーログのリスト</returns>
        public List<ErrorLogEntry> GetErrorLog(int limit = 100)
        {
            return _errorLog
                .OrderByDescending(e => e.Timestamp)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// エラーログをクリア
        /// </summary>
        public void ClearErrorLog()
        {
            _errorLog.Clear();
        }

        /// <summary>
        /// 回復可能なエラーかどうかを判定
        /// </summary>
        /// <param name="exception">例外</param>
        /// <returns>回復可能な場合はtrue</returns>
        public bool IsRecoverable(Exception exception)
        {
            return exception switch
            {
                IndexedDbException indexedDbEx => indexedDbEx.ErrorCode != "INDEXEDDB_INIT_FAILED",
                DataOperationException dataEx => dataEx.ErrorCode != "DATA_VALIDATION_FAILED",
                NetworkException => true,
                ArgumentNullException => false,
                ArgumentException => false,
                _ => true // 一般的な例外は回復可能と見なす
            };
        }

        private string GetErrorCode(Exception exception)
        {
            return exception switch
            {
                MyNotesException myNotesEx => myNotesEx.ErrorCode,
                ArgumentNullException => "ARGUMENT_NULL",
                ArgumentException => "ARGUMENT_INVALID",
                InvalidOperationException => "INVALID_OPERATION",
                TimeoutException => "TIMEOUT",
                _ => "UNKNOWN_ERROR"
            };
        }

        private ErrorMessage GenerateUserMessage(Exception exception, string context)
        {
            var message = exception switch
            {
                MyNotesException myNotesEx => new ErrorMessage
                {
                    Title = GetErrorTitle(myNotesEx.ErrorCode),
                    Message = myNotesEx.UserFriendlyMessage,
                    Type = GetErrorType(myNotesEx.ErrorCode),
                    IsRecoverable = IsRecoverable(exception),
                    ErrorCode = myNotesEx.ErrorCode,
                    Actions = GetSuggestedActions(myNotesEx.ErrorCode)
                },
                _ => new ErrorMessage
                {
                    Title = "エラーが発生しました",
                    Message = "予期しないエラーが発生しました。ページを再読み込みしてください。",
                    Type = ErrorType.Error,
                    IsRecoverable = true,
                    ErrorCode = GetErrorCode(exception),
                    Actions = new List<string> { "ページを再読み込み", "しばらく時間をおいて再試行" }
                }
            };

            return message;
        }

        private string GetErrorTitle(string errorCode)
        {
            return errorCode switch
            {
                "INDEXEDDB_CONNECTION_FAILED" => "データベース接続エラー",
                "INDEXEDDB_INIT_FAILED" => "データベース初期化エラー",
                "INDEXEDDB_QUOTA_EXCEEDED" => "ストレージ容量不足",
                "DATA_SAVE_FAILED" => "保存エラー",
                "DATA_LOAD_FAILED" => "読み込みエラー",
                "DATA_NOT_FOUND" => "データが見つかりません",
                "DATA_VALIDATION_FAILED" => "入力エラー",
                "NETWORK_TIMEOUT" => "接続タイムアウト",
                "NETWORK_CONNECTION_FAILED" => "ネットワークエラー",
                _ => "エラー"
            };
        }

        private ErrorType GetErrorType(string errorCode)
        {
            return errorCode switch
            {
                "DATA_VALIDATION_FAILED" => ErrorType.Warning,
                "DATA_NOT_FOUND" => ErrorType.Warning,
                "INDEXEDDB_QUOTA_EXCEEDED" => ErrorType.Warning,
                _ => ErrorType.Error
            };
        }

        private List<string> GetSuggestedActions(string errorCode)
        {
            return errorCode switch
            {
                "INDEXEDDB_CONNECTION_FAILED" => new List<string>
                {
                    "ページを再読み込み",
                    "ブラウザの設定を確認",
                    "他のタブを閉じる"
                },
                "INDEXEDDB_QUOTA_EXCEEDED" => new List<string>
                {
                    "不要なメモを削除",
                    "ブラウザのキャッシュをクリア",
                    "ストレージ設定を確認"
                },
                "DATA_SAVE_FAILED" => new List<string>
                {
                    "再試行",
                    "内容をコピーして保存",
                    "ページを再読み込み"
                },
                "NETWORK_CONNECTION_FAILED" => new List<string>
                {
                    "インターネット接続を確認",
                    "しばらく時間をおいて再試行",
                    "オフラインモードで利用"
                },
                _ => new List<string> { "再試行", "ページを再読み込み" }
            };
        }

        private IndexedDbException AnalyzeIndexedDbException(Exception exception, string operation)
        {
            var message = exception.Message.ToLower();
            
            if (message.Contains("quota") || message.Contains("storage"))
            {
                return IndexedDbException.QuotaExceeded(exception);
            }
            
            if (message.Contains("connection") || message.Contains("open"))
            {
                return IndexedDbException.ConnectionFailure(exception);
            }

            if (message.Contains("init") || message.Contains("create"))
            {
                return IndexedDbException.InitializationFailure(exception);
            }

            return IndexedDbException.ConnectionFailure(exception);
        }

        private DataOperationException AnalyzeDataOperationException(Exception exception, string operation, string? resourceId)
        {
            if (exception is InvalidOperationException && exception.Message.Contains("見つかりません"))
            {
                return DataOperationException.NotFound("メモ", resourceId ?? "不明");
            }

            if (operation.Contains("save") || operation.Contains("create") || operation.Contains("update"))
            {
                return DataOperationException.SaveFailure(operation, exception);
            }

            if (operation.Contains("get") || operation.Contains("load") || operation.Contains("read"))
            {
                return DataOperationException.LoadFailure(operation, exception);
            }

            return DataOperationException.SaveFailure(operation, exception);
        }

        private async Task<string> GetUserAgentAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string>("eval", "navigator.userAgent");
            }
            catch
            {
                return "Unknown";
            }
        }

        private async Task LogToConsoleAsync(ErrorLogEntry logEntry)
        {
            try
            {
                var logMessage = $"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss}] {logEntry.ErrorCode}: {logEntry.Exception.Message}";
                if (!string.IsNullOrEmpty(logEntry.Context))
                {
                    logMessage += $" (Context: {logEntry.Context})";
                }
                
                await _jsRuntime.InvokeVoidAsync("console.error", logMessage, logEntry.Exception.ToString());
            }
            catch
            {
                // ログ出力に失敗してもアプリケーションは継続
            }
        }
    }

    /// <summary>
    /// エラーログエントリ
    /// </summary>
    public class ErrorLogEntry
    {
        public DateTime Timestamp { get; set; }
        public Exception Exception { get; set; } = null!;
        public string Context { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    /// <summary>
    /// ユーザー向けエラーメッセージ
    /// </summary>
    public class ErrorMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ErrorType Type { get; set; } = ErrorType.Error;
        public bool IsRecoverable { get; set; } = true;
        public string ErrorCode { get; set; } = string.Empty;
        public List<string> Actions { get; set; } = new List<string>();
    }

    /// <summary>
    /// エラータイプ
    /// </summary>
    public enum ErrorType
    {
        Info,
        Warning,
        Error,
        Critical
    }
}