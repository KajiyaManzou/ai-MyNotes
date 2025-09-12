namespace ai_MyNotes.Services.Exceptions
{
    /// <summary>
    /// MyNotesアプリケーション固有の例外基底クラス
    /// </summary>
    public abstract class MyNotesException : Exception
    {
        public string ErrorCode { get; }
        public string UserFriendlyMessage { get; }
        public Dictionary<string, object> ErrorDetails { get; }

        protected MyNotesException(
            string errorCode, 
            string userFriendlyMessage, 
            string technicalMessage, 
            Exception? innerException = null,
            Dictionary<string, object>? errorDetails = null) 
            : base(technicalMessage, innerException)
        {
            ErrorCode = errorCode;
            UserFriendlyMessage = userFriendlyMessage;
            ErrorDetails = errorDetails ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// IndexedDB関連の例外
    /// </summary>
    public class IndexedDbException : MyNotesException
    {
        public IndexedDbException(
            string errorCode,
            string userFriendlyMessage,
            string technicalMessage,
            Exception? innerException = null,
            Dictionary<string, object>? errorDetails = null)
            : base(errorCode, userFriendlyMessage, technicalMessage, innerException, errorDetails)
        {
        }

        // 接続失敗
        public static IndexedDbException ConnectionFailure(Exception? innerException = null)
        {
            return new IndexedDbException(
                "INDEXEDDB_CONNECTION_FAILED",
                "データベースに接続できませんでした。ブラウザの設定を確認するか、ページを再読み込みしてください。",
                "IndexedDB connection failed",
                innerException);
        }

        // 初期化失敗
        public static IndexedDbException InitializationFailure(Exception? innerException = null)
        {
            return new IndexedDbException(
                "INDEXEDDB_INIT_FAILED",
                "データベースの初期化に失敗しました。ブラウザがIndexedDBに対応していない可能性があります。",
                "IndexedDB initialization failed",
                innerException);
        }

        // 容量不足
        public static IndexedDbException QuotaExceeded(Exception? innerException = null)
        {
            return new IndexedDbException(
                "INDEXEDDB_QUOTA_EXCEEDED",
                "ストレージ容量が不足しています。不要なデータを削除するか、ブラウザの設定を確認してください。",
                "IndexedDB quota exceeded",
                innerException);
        }
    }

    /// <summary>
    /// データ操作関連の例外
    /// </summary>
    public class DataOperationException : MyNotesException
    {
        public DataOperationException(
            string errorCode,
            string userFriendlyMessage,
            string technicalMessage,
            Exception? innerException = null,
            Dictionary<string, object>? errorDetails = null)
            : base(errorCode, userFriendlyMessage, technicalMessage, innerException, errorDetails)
        {
        }

        // 保存失敗
        public static DataOperationException SaveFailure(string operation, Exception? innerException = null)
        {
            return new DataOperationException(
                "DATA_SAVE_FAILED",
                "データの保存に失敗しました。しばらく時間をおいてから再試行してください。",
                $"Data save operation failed: {operation}",
                innerException);
        }

        // 読み込み失敗
        public static DataOperationException LoadFailure(string operation, Exception? innerException = null)
        {
            return new DataOperationException(
                "DATA_LOAD_FAILED",
                "データの読み込みに失敗しました。ページを再読み込みしてください。",
                $"Data load operation failed: {operation}",
                innerException);
        }

        // データが見つからない
        public static DataOperationException NotFound(string resourceType, string identifier)
        {
            return new DataOperationException(
                "DATA_NOT_FOUND",
                "指定されたデータが見つかりませんでした。",
                $"{resourceType} not found: {identifier}",
                null,
                new Dictionary<string, object> { { "ResourceType", resourceType }, { "Identifier", identifier } });
        }

        // バリデーション失敗
        public static DataOperationException ValidationFailure(IEnumerable<string> validationErrors)
        {
            var errors = validationErrors.ToList();
            return new DataOperationException(
                "DATA_VALIDATION_FAILED",
                $"入力内容に問題があります: {string.Join("、", errors)}",
                $"Data validation failed: {string.Join(", ", errors)}",
                null,
                new Dictionary<string, object> { { "ValidationErrors", errors } });
        }
    }

    /// <summary>
    /// ネットワーク関連の例外
    /// </summary>
    public class NetworkException : MyNotesException
    {
        public NetworkException(
            string errorCode,
            string userFriendlyMessage,
            string technicalMessage,
            Exception? innerException = null)
            : base(errorCode, userFriendlyMessage, technicalMessage, innerException)
        {
        }

        // 接続タイムアウト
        public static NetworkException ConnectionTimeout(Exception? innerException = null)
        {
            return new NetworkException(
                "NETWORK_TIMEOUT",
                "接続がタイムアウトしました。インターネット接続を確認してください。",
                "Network connection timeout",
                innerException);
        }

        // 接続失敗
        public static NetworkException ConnectionFailure(Exception? innerException = null)
        {
            return new NetworkException(
                "NETWORK_CONNECTION_FAILED",
                "ネットワーク接続に失敗しました。インターネット接続を確認してください。",
                "Network connection failed",
                innerException);
        }
    }
}