-- TASK-003: データベース（SQLite）の基本設定
-- 実行日時: 2024年12月19日
-- 実行者: AI Assistant

-- ========================================
-- データベース情報
-- ========================================
-- データベース名: ai_mynotes.db
-- バージョン: 1
-- 文字エンコーディング: UTF-8

-- ========================================
-- notesテーブルの作成
-- ========================================
CREATE TABLE notes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    title TEXT NOT NULL,
    content TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ========================================
-- インデックスの作成（検索パフォーマンス向上）
-- ========================================

-- 作成日時でのソート用インデックス
CREATE INDEX idx_notes_created_at ON notes(created_at);

-- タイトル検索用インデックス
CREATE INDEX idx_notes_title ON notes(title);

-- ========================================
-- テーブル構造の確認用クエリ
-- ========================================

-- テーブル一覧の確認
SELECT name FROM sqlite_master WHERE type='table';

-- notesテーブルの構造確認
PRAGMA table_info(notes);

-- インデックス一覧の確認
SELECT name FROM sqlite_master WHERE type='index' AND tbl_name='notes';

-- ========================================
-- サンプルデータの挿入（テスト用）
-- ========================================

-- テスト用メモ1
INSERT INTO notes (title, content) VALUES (
    'Flutter Development',
    'Learning Flutter framework for mobile app development.'
);

-- テスト用メモ2
INSERT INTO notes (title, content) VALUES (
    'Dart Programming',
    'Learning Dart language fundamentals and best practices.'
);

-- テスト用メモ3
INSERT INTO notes (title, content) VALUES (
    'Mobile App Design',
    'Understanding mobile app design principles and user experience.'
);

-- ========================================
-- データ確認用クエリ
-- ========================================

-- 全メモの取得（作成日時降順）
SELECT * FROM notes ORDER BY created_at DESC;

-- メモ数の確認
SELECT COUNT(*) as note_count FROM notes;

-- タイトル検索のテスト
SELECT * FROM notes WHERE title LIKE '%Flutter%';

-- 内容検索のテスト
SELECT * FROM notes WHERE content LIKE '%Learning%';

-- ========================================
-- 将来の拡張用（バージョン2以降）
-- ========================================

-- タグ機能用テーブル（将来実装予定）
-- CREATE TABLE tags (
--     id INTEGER PRIMARY KEY AUTOINCREMENT,
--     name TEXT UNIQUE NOT NULL
-- );

-- メモとタグの関連テーブル（将来実装予定）
-- CREATE TABLE note_tags (
--     note_id INTEGER,
--     tag_id INTEGER,
--     FOREIGN KEY (note_id) REFERENCES notes(id) ON DELETE CASCADE,
--     FOREIGN KEY (tag_id) REFERENCES tags(id) ON DELETE CASCADE,
--     PRIMARY KEY (note_id, tag_id)
-- );

-- カテゴリ機能用テーブル（将来実装予定）
-- CREATE TABLE categories (
--     id INTEGER PRIMARY KEY AUTOINCREMENT,
--     name TEXT NOT NULL,
--     parent_id INTEGER,
--     FOREIGN KEY (parent_id) REFERENCES categories(id)
-- );

-- ========================================
-- パフォーマンス最適化用クエリ
-- ========================================

-- テーブル統計情報の更新
ANALYZE;

-- データベースの最適化
VACUUM;

-- ========================================
-- バックアップ・復元用クエリ
-- ========================================

-- データベースのバックアップ（外部コマンド）
-- .backup 'ai_mynotes_backup.db'

-- データベースの復元（外部コマンド）
-- .restore 'ai_mynotes_backupup.db'

-- ========================================
-- 注意事項
-- ========================================
-- 1. このSQLファイルは開発・テスト環境用です
-- 2. 本番環境では適切なバックアップ戦略を実装してください
-- 3. インデックスは検索パフォーマンス向上のため作成されています
-- 4. 将来の機能拡張に備えて、テーブル設計は拡張性を考慮しています
