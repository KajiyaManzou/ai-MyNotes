using ai_MyNotes.Models;
using ai_MyNotes.Services.Exceptions;
using TG.Blazor.IndexedDB;

namespace ai_MyNotes.Services
{
    /// <summary>
    /// メモデータアクセスサービス
    /// </summary>
    public class MemoService : IMemoService
    {
        private readonly IndexedDBManager _dbManager;
        private readonly IErrorHandlingService _errorHandlingService;

        public MemoService(IndexedDBManager dbManager, IErrorHandlingService errorHandlingService)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
            _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
        }

        /// <summary>
        /// IndexedDBの初期化
        /// </summary>
        /// <returns>初期化が成功したかどうか</returns>
        /// <exception cref="IndexedDbException">初期化に失敗した場合</exception>
        public async Task<bool> InitializeDatabaseAsync()
        {
            try
            {
                // データベースの初期化を試行
                await _dbManager.OpenDb();
                return true;
            }
            catch (Exception ex)
            {
                // エラーハンドリングサービスでエラーを処理
                var errorMessage = await _errorHandlingService.HandleIndexedDbErrorAsync(ex, "Database initialization");
                throw IndexedDbException.InitializationFailure(ex);
            }
        }

        /// <summary>
        /// IndexedDB接続テスト
        /// </summary>
        /// <returns>接続が成功したかどうか</returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // データベースの初期化確認
                if (!await InitializeDatabaseAsync())
                {
                    return false;
                }

                // メモ一覧を取得してテストする
                var memos = await GetMemosAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 新しいメモを作成
        /// </summary>
        /// <param name="memo">作成するメモ</param>
        /// <returns>作成されたメモ</returns>
        /// <exception cref="ArgumentNullException">memoがnullの場合</exception>
        /// <exception cref="DataOperationException">バリデーションエラーまたは保存失敗の場合</exception>
        /// <exception cref="IndexedDbException">データベース操作エラーの場合</exception>
        public async Task<Memo> CreateMemoAsync(Memo memo)
        {
            if (memo == null)
                throw new ArgumentNullException(nameof(memo));

            try
            {
                // バリデーション実行
                var (isValid, errors) = memo.Validate();
                if (!isValid)
                {
                    var validationException = DataOperationException.ValidationFailure(errors);
                    await _errorHandlingService.HandleDataOperationErrorAsync(validationException, "Create memo validation");
                    throw validationException;
                }

                // 自動修正とタイトル生成
                memo.AutoCorrect();
                memo.UpdateTitleFromContent();
                memo.Touch();

                // IDが既に設定されている場合（更新扱い）は例外
                if (memo.Id != 0)
                {
                    var invalidOpException = DataOperationException.ValidationFailure(new[] { "新規作成ではIDを指定できません" });
                    await _errorHandlingService.HandleDataOperationErrorAsync(invalidOpException, "Create memo ID validation");
                    throw invalidOpException;
                }

                // IndexedDBに保存
                var record = new StoreRecord<Memo>
                {
                    Storename = MyNotesDatabase.MemoStore,
                    Data = memo
                };

                await _dbManager.AddRecord(record);
                
                System.Diagnostics.Debug.WriteLine($"Memo created successfully: Title='{memo.Title}', Content length={memo.Content?.Length ?? 0}");
                return memo;
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is DataOperationException))
            {
                // IndexedDBエラーまたは予期しないエラー
                var saveException = DataOperationException.SaveFailure("Create memo", ex);
                await _errorHandlingService.HandleDataOperationErrorAsync(saveException, "Create memo", memo.Id.ToString());
                throw saveException;
            }
        }

        /// <summary>
        /// 全メモを取得（更新日時降順）
        /// </summary>
        /// <returns>メモのリスト</returns>
        /// <exception cref="DataOperationException">データ読み込み失敗の場合</exception>
        /// <exception cref="IndexedDbException">データベース操作エラーの場合</exception>
        public async Task<List<Memo>> GetMemosAsync()
        {
            try
            {
                var memos = await _dbManager.GetRecords<Memo>(MyNotesDatabase.MemoStore);
                
                if (memos == null || !memos.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No memos found in database");
                    return new List<Memo>();
                }

                // 更新日時で降順ソート
                var sortedMemos = memos
                    .Where(m => m != null) // null要素を除外
                    .OrderByDescending(m => m.UpdatedAt)
                    .ThenByDescending(m => m.CreatedAt) // 更新日時が同じ場合は作成日時で
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Retrieved {sortedMemos.Count} memos from database");
                return sortedMemos;
            }
            catch (Exception ex)
            {
                // データ読み込み失敗エラーとして処理
                var loadException = DataOperationException.LoadFailure("Get all memos", ex);
                await _errorHandlingService.HandleDataOperationErrorAsync(loadException, "Get memos");
                throw loadException;
            }
        }

        /// <summary>
        /// IDでメモを取得
        /// </summary>
        /// <param name="id">メモID</param>
        /// <returns>メモ、見つからない場合はnull</returns>
        /// <exception cref="ArgumentException">IDが無効な場合</exception>
        /// <exception cref="Exception">データベース操作エラーの場合</exception>
        public async Task<Memo?> GetMemoByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("IDは1以上の値である必要があります", nameof(id));

            try
            {
                // IndexedDBから直接IDで取得を試行
                try
                {
                    var memo = await _dbManager.GetRecordById<int, Memo>(MyNotesDatabase.MemoStore, id);
                    if (memo != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Memo found by ID {id}: Title='{memo.Title}'");
                        return memo;
                    }
                }
                catch
                {
                    // 直接取得に失敗した場合は全件取得からフィルタ
                    System.Diagnostics.Debug.WriteLine($"Direct fetch failed for ID {id}, trying full scan");
                }

                // 全件取得してフィルタリング（フォールバック）
                var allMemos = await _dbManager.GetRecords<Memo>(MyNotesDatabase.MemoStore);
                var foundMemo = allMemos?.FirstOrDefault(m => m != null && m.Id == id);

                if (foundMemo == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Memo not found with ID {id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Memo found by full scan ID {id}: Title='{foundMemo.Title}'");
                }

                return foundMemo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get memo by ID {id}: {ex.Message}");
                throw new Exception($"メモ（ID: {id}）の取得中にエラーが発生しました", ex);
            }
        }

        /// <summary>
        /// メモを更新
        /// </summary>
        /// <param name="memo">更新するメモ</param>
        /// <returns>更新されたメモ</returns>
        /// <exception cref="ArgumentNullException">memoがnullの場合</exception>
        /// <exception cref="ArgumentException">IDが無効な場合</exception>
        /// <exception cref="InvalidOperationException">バリデーションエラーまたはメモが存在しない場合</exception>
        /// <exception cref="Exception">データベース操作エラーの場合</exception>
        public async Task<Memo> UpdateMemoAsync(Memo memo)
        {
            if (memo == null)
                throw new ArgumentNullException(nameof(memo));

            if (memo.Id <= 0)
                throw new ArgumentException("更新にはIDが必要です", nameof(memo));

            try
            {
                // 既存メモの存在確認
                var existingMemo = await GetMemoByIdAsync(memo.Id);
                if (existingMemo == null)
                {
                    throw new InvalidOperationException($"更新対象のメモが見つかりません（ID: {memo.Id}）");
                }

                // バリデーション実行
                var (isValid, errors) = memo.Validate();
                if (!isValid)
                {
                    var errorMessage = string.Join(", ", errors);
                    throw new InvalidOperationException($"メモの更新に失敗しました: {errorMessage}");
                }

                // 自動修正とタイトル生成、更新日時設定
                memo.AutoCorrect();
                memo.UpdateTitleFromContent();
                memo.Touch();

                // 作成日時は既存のものを保持
                memo.CreatedAt = existingMemo.CreatedAt;

                // IndexedDBに保存
                var record = new StoreRecord<Memo>
                {
                    Storename = MyNotesDatabase.MemoStore,
                    Data = memo
                };

                await _dbManager.UpdateRecord(record);
                
                System.Diagnostics.Debug.WriteLine($"Memo updated successfully: ID={memo.Id}, Title='{memo.Title}', Content length={memo.Content?.Length ?? 0}");
                return memo;
            }
            catch (Exception ex) when (!(ex is ArgumentNullException || ex is ArgumentException || ex is InvalidOperationException))
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update memo ID {memo.Id}: {ex.Message}");
                throw new Exception($"メモ（ID: {memo.Id}）の更新中にエラーが発生しました", ex);
            }
        }

        /// <summary>
        /// メモを削除
        /// </summary>
        /// <param name="id">削除するメモのID</param>
        /// <returns>削除が成功したかどうか</returns>
        /// <exception cref="ArgumentException">IDが無効な場合</exception>
        /// <exception cref="InvalidOperationException">削除対象のメモが見つからない場合</exception>
        /// <exception cref="Exception">データベース操作エラーの場合</exception>
        public async Task<bool> DeleteMemoAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("IDは1以上の値である必要があります", nameof(id));

            try
            {
                // 削除前に対象メモの存在確認
                var existingMemo = await GetMemoByIdAsync(id);
                if (existingMemo == null)
                {
                    throw new InvalidOperationException($"削除対象のメモが見つかりません（ID: {id}）");
                }

                // IndexedDBから削除
                await _dbManager.DeleteRecord(MyNotesDatabase.MemoStore, id);
                
                System.Diagnostics.Debug.WriteLine($"Memo deleted successfully: ID={id}, Title='{existingMemo.Title}'");
                return true;
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete memo ID {id}: {ex.Message}");
                throw new Exception($"メモ（ID: {id}）の削除中にエラーが発生しました", ex);
            }
        }

        /// <summary>
        /// 複数のメモを一括削除
        /// </summary>
        /// <param name="ids">削除するメモIDのリスト</param>
        /// <returns>削除に成功したIDのリスト</returns>
        /// <exception cref="ArgumentNullException">idsがnullの場合</exception>
        public async Task<List<int>> DeleteMemosAsync(IEnumerable<int> ids)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var successfulDeletes = new List<int>();
            var idList = ids.ToList();

            foreach (var id in idList)
            {
                try
                {
                    await DeleteMemoAsync(id);
                    successfulDeletes.Add(id);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete memo ID {id} in batch operation: {ex.Message}");
                    // 一括削除の場合は個別の失敗は続行
                }
            }

            System.Diagnostics.Debug.WriteLine($"Batch delete completed: {successfulDeletes.Count}/{idList.Count} successful");
            return successfulDeletes;
        }

        /// <summary>
        /// 全メモを削除（初期化用）
        /// </summary>
        /// <returns>削除されたメモ数</returns>
        public async Task<int> DeleteAllMemosAsync()
        {
            try
            {
                var allMemos = await GetMemosAsync();
                var count = allMemos.Count;

                if (count > 0)
                {
                    var ids = allMemos.Select(m => m.Id).ToList();
                    await DeleteMemosAsync(ids);
                }

                System.Diagnostics.Debug.WriteLine($"All memos deleted: {count} memos removed");
                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete all memos: {ex.Message}");
                throw new Exception("全メモの削除中にエラーが発生しました", ex);
            }
        }
    }
}