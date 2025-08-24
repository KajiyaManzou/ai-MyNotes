import 'package:sqflite/sqflite.dart';
import 'package:path/path.dart';

/// SQLiteデータベースの初期化とテーブル作成を行うヘルパークラス
class DatabaseHelper {
  static const String _databaseName = 'ai_mynotes.db';
  static const int _databaseVersion = 1;

  // シングルトンパターンでインスタンスを管理
  static final DatabaseHelper _instance = DatabaseHelper._internal();
  factory DatabaseHelper() => _instance;
  DatabaseHelper._internal();

  Database? _database;

  /// データベースインスタンスを取得
  Future<Database> get database async {
    if (_database != null) return _database!;
    _database = await _initDatabase();
    return _database!;
  }

  /// データベースの初期化
  Future<Database> _initDatabase() async {
    String path = join(await getDatabasesPath(), _databaseName);
    
    return await openDatabase(
      path,
      version: _databaseVersion,
      onCreate: _onCreate,
      onUpgrade: _onUpgrade,
    );
  }

  /// データベース作成時の処理
  Future<void> _onCreate(Database db, int version) async {
    // notesテーブルの作成
    await db.execute('''
      CREATE TABLE notes (
        id INTEGER PRIMARY KEY AUTOINCREMENT,
        title TEXT NOT NULL,
        content TEXT,
        created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
        updated_at DATETIME DEFAULT CURRENT_TIMESTAMP
      )
    ''');

    // インデックスの作成（検索パフォーマンス向上のため）
    await db.execute('CREATE INDEX idx_notes_created_at ON notes(created_at)');
    await db.execute('CREATE INDEX idx_notes_title ON notes(title)');
  }

  /// データベースアップグレード時の処理
  Future<void> _onUpgrade(Database db, int oldVersion, int newVersion) async {
    // 将来のバージョンアップグレード時の処理をここに記述
    if (oldVersion < 2) {
      // 例: 新しいテーブルやカラムの追加
    }
  }

  /// データベースを閉じる
  Future<void> close() async {
    final db = await database;
    await db.close();
  }

  /// データベースファイルを削除（テスト用）
  Future<void> deleteDatabase() async {
    String path = join(await getDatabasesPath(), _databaseName);
    await databaseFactory.deleteDatabase(path);
  }
}
