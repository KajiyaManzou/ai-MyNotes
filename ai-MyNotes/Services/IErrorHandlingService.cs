using ai_MyNotes.Services.Exceptions;

namespace ai_MyNotes.Services
{
    /// <summary>
    /// エラーハンドリングサービスのインターフェース
    /// </summary>
    public interface IErrorHandlingService
    {
        /// <summary>
        /// エラーを処理し、適切なユーザーメッセージを返す
        /// </summary>
        /// <param name="exception">発生した例外</param>
        /// <param name="context">エラー発生コンテキスト</param>
        /// <returns>ユーザー向けエラーメッセージ</returns>
        Task<ErrorMessage> HandleErrorAsync(Exception exception, string context = "");

        /// <summary>
        /// IndexedDB関連エラーのハンドリング
        /// </summary>
        /// <param name="exception">IndexedDB例外</param>
        /// <param name="operation">実行中の操作</param>
        /// <returns>処理済みエラーメッセージ</returns>
        Task<ErrorMessage> HandleIndexedDbErrorAsync(Exception exception, string operation);

        /// <summary>
        /// データ操作エラーのハンドリング
        /// </summary>
        /// <param name="exception">データ操作例外</param>
        /// <param name="operation">実行中の操作</param>
        /// <param name="resourceId">リソースID（オプション）</param>
        /// <returns>処理済みエラーメッセージ</returns>
        Task<ErrorMessage> HandleDataOperationErrorAsync(Exception exception, string operation, string? resourceId = null);

        /// <summary>
        /// エラーログを取得
        /// </summary>
        /// <param name="limit">取得件数制限</param>
        /// <returns>エラーログのリスト</returns>
        List<ErrorLogEntry> GetErrorLog(int limit = 100);

        /// <summary>
        /// エラーログをクリア
        /// </summary>
        void ClearErrorLog();

        /// <summary>
        /// 回復可能なエラーかどうかを判定
        /// </summary>
        /// <param name="exception">例外</param>
        /// <returns>回復可能な場合はtrue</returns>
        bool IsRecoverable(Exception exception);
    }
}