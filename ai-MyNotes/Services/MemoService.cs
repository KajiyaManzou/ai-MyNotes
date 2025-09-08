using ai_MyNotes.Models;
using TG.Blazor.IndexedDB;

namespace ai_MyNotes.Services
{
    /// <summary>
    /// メモデータアクセスサービス
    /// </summary>
    public class MemoService
    {
        private readonly IndexedDBManager _dbManager;

        public MemoService(IndexedDBManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// IndexedDB接続テスト
        /// </summary>
        /// <returns>接続が成功したかどうか</returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // メモ一覧を取得してテストする
                var memos = await GetMemosAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 新しいメモを作成
        /// </summary>
        /// <param name="memo">作成するメモ</param>
        /// <returns>作成されたメモ</returns>
        public async Task<Memo> CreateMemoAsync(Memo memo)
        {
            memo.UpdateTitleFromContent();
            memo.Touch();
            
            var record = new StoreRecord<Memo>
            {
                Storename = MyNotesDatabase.MemoStore,
                Data = memo
            };
            
            await _dbManager.AddRecord(record);
            return memo;
        }

        /// <summary>
        /// 全メモを取得（更新日時降順）
        /// </summary>
        /// <returns>メモのリスト</returns>
        public async Task<List<Memo>> GetMemosAsync()
        {
            var memos = await _dbManager.GetRecords<Memo>(MyNotesDatabase.MemoStore);
            return memos?.OrderByDescending(m => m.UpdatedAt).ToList() ?? new List<Memo>();
        }

        /// <summary>
        /// IDでメモを取得
        /// </summary>
        /// <param name="id">メモID</param>
        /// <returns>メモ、見つからない場合はnull</returns>
        public async Task<Memo?> GetMemoByIdAsync(int id)
        {
            var allMemos = await _dbManager.GetRecords<Memo>(MyNotesDatabase.MemoStore);
            return allMemos?.FirstOrDefault(m => m.Id == id);
        }

        /// <summary>
        /// メモを更新
        /// </summary>
        /// <param name="memo">更新するメモ</param>
        /// <returns>更新されたメモ</returns>
        public async Task<Memo> UpdateMemoAsync(Memo memo)
        {
            memo.UpdateTitleFromContent();
            memo.Touch();
            
            var record = new StoreRecord<Memo>
            {
                Storename = MyNotesDatabase.MemoStore,
                Data = memo
            };
            
            await _dbManager.UpdateRecord(record);
            return memo;
        }

        /// <summary>
        /// メモを削除
        /// </summary>
        /// <param name="id">削除するメモのID</param>
        public async Task DeleteMemoAsync(int id)
        {
            await _dbManager.DeleteRecord(MyNotesDatabase.MemoStore, id);
        }
    }
}