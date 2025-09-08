using TG.Blazor.IndexedDB;

namespace ai_MyNotes.Models
{
    /// <summary>
    /// ai-MyNotesアプリケーション用IndexedDBデータベース設定
    /// </summary>
    public static class MyNotesDatabase
    {
        /// <summary>
        /// データベース名
        /// </summary>
        public const string DatabaseName = "MyNotesDB";
        
        /// <summary>
        /// データベースバージョン
        /// </summary>
        public const long Version = 1;
        
        /// <summary>
        /// メモストア名
        /// </summary>
        public const string MemoStore = "memos";
    }
}