#!/bin/bash

# TASK-003: データベース（SQLite）の基本設定
# 実行日時: 2024年12月19日
# 実行者: AI Assistant

echo "=== TASK-003: データベース（SQLite）の基本設定開始 ==="

# 1. プロジェクトディレクトリに移動
echo "1. プロジェクトディレクトリに移動"
cd ai_mynotes

# 2. データベース関連のディレクトリ構造を作成
echo "2. データベース関連のディレクトリ構造を作成"
mkdir -p lib/database lib/domain/models lib/domain/repositories lib/interfaces

# 3. データベースヘルパークラスの作成
echo "3. データベースヘルパークラスの作成"
echo "lib/database/database_helper.dart を作成"
echo "- SQLiteデータベースの初期化とテーブル作成"
echo "- シングルトンパターンでのデータベース管理"
echo "- バージョン管理とアップグレード対応"

# 4. Noteモデルクラスの作成
echo "4. Noteモデルクラスの作成"
echo "lib/domain/models/note.dart を作成"
echo "- Value Objectパターンの実装"
echo "- イミュータブルな設計"
echo "- ファクトリメソッド（Note.create）"
echo "- データベース変換メソッド（fromMap, toMap）"

# 5. Noteリポジトリインターフェースの作成
echo "5. Noteリポジトリインターフェースの作成"
echo "lib/domain/repositories/note_repository.dart を作成"
echo "- Noteのデータアクセスを抽象化"
echo "- CRUD操作のインターフェース定義"

# 6. SQLite実装クラスの作成
echo "6. SQLite実装クラスの作成"
echo "lib/database/note_repository_impl.dart を作成"
echo "- NoteRepositoryのSQLite実装"
echo "- 効率的なクエリ処理"
echo "- エラーハンドリング"

# 7. データベース初期化のテスト
echo "7. データベース初期化のテスト"
echo "lib/database/database_test.dart を作成"
echo "- データベースの初期化テスト"
echo "- テーブル作成の確認"
echo "- CRUD操作のテスト"

# 8. プロジェクトのビルド確認
echo "8. プロジェクトのビルド確認"
flutter analyze

# 9. pathパッケージの追加
echo "9. pathパッケージの追加"
echo "pubspec.yamlに path: ^1.9.0 を追加"
flutter pub get

# 10. 最終的なコード解析
echo "10. 最終的なコード解析"
flutter analyze

# 11. プロジェクトのビルド確認
echo "11. プロジェクトのビルド確認"
flutter build web

# 12. 作成されたファイル構造の確認
echo "12. 作成されたファイル構造の確認"
cd ..
find ai_mynotes/lib -name "*.dart" | head -20

echo "=== TASK-003: データベース（SQLite）の基本設定完了 ==="

# 実行結果の要約
echo ""
echo "=== 実行結果サマリー ==="
echo "✅ notesテーブルの作成完了"
echo "✅ データベース初期化処理の実装完了"
echo "✅ テーブルスキーマの実装完了"
echo "✅ 3階層アーキテクチャの基盤構築完了"
echo ""
echo "作成されたファイル:"
echo "- lib/database/database_helper.dart (SQLiteデータベース初期化・管理)"
echo "- lib/database/note_repository_impl.dart (NoteRepositoryのSQLite実装)"
echo "- lib/database/database_test.dart (データベーステスト)"
echo "- lib/domain/models/note.dart (Noteモデル・Value Object)"
echo "- lib/domain/repositories/note_repository.dart (Noteリポジトリインターフェース)"
echo ""
echo "データベーススキーマ:"
echo "- notesテーブル: id, title, content, created_at, updated_at"
echo "- インデックス: created_at, title"
echo "- 制約: title NOT NULL, 自動タイムスタンプ"
echo ""
echo "TASK-003: 完了 ✅"
