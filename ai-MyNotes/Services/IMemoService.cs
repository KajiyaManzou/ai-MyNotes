using ai_MyNotes.Models;

namespace ai_MyNotes.Services
{
    /// <summary>
    /// メモデータアクセスサービスのインターフェース
    /// </summary>
    public interface IMemoService
    {
        /// <summary>
        /// IndexedDBの初期化
        /// </summary>
        /// <returns>初期化が成功したかどうか</returns>
        Task<bool> InitializeDatabaseAsync();

        /// <summary>
        /// IndexedDB接続テスト
        /// </summary>
        /// <returns>接続が成功したかどうか</returns>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// 新しいメモを作成
        /// </summary>
        /// <param name="memo">作成するメモ</param>
        /// <returns>作成されたメモ</returns>
        Task<Memo> CreateMemoAsync(Memo memo);

        /// <summary>
        /// 全メモを取得（更新日時降順）
        /// </summary>
        /// <returns>メモのリスト</returns>
        Task<List<Memo>> GetMemosAsync();

        /// <summary>
        /// IDでメモを取得
        /// </summary>
        /// <param name="id">メモID</param>
        /// <returns>メモ、見つからない場合はnull</returns>
        Task<Memo?> GetMemoByIdAsync(int id);

        /// <summary>
        /// メモを更新
        /// </summary>
        /// <param name="memo">更新するメモ</param>
        /// <returns>更新されたメモ</returns>
        Task<Memo> UpdateMemoAsync(Memo memo);

        /// <summary>
        /// メモを削除
        /// </summary>
        /// <param name="id">削除するメモのID</param>
        /// <returns>削除が成功したかどうか</returns>
        Task<bool> DeleteMemoAsync(int id);

        /// <summary>
        /// 複数のメモを一括削除
        /// </summary>
        /// <param name="ids">削除するメモIDのリスト</param>
        /// <returns>削除に成功したIDのリスト</returns>
        Task<List<int>> DeleteMemosAsync(IEnumerable<int> ids);

        /// <summary>
        /// 全メモを削除（初期化用）
        /// </summary>
        /// <returns>削除されたメモ数</returns>
        Task<int> DeleteAllMemosAsync();
    }
}