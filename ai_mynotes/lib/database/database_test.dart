import 'package:flutter_test/flutter_test.dart';
import 'database_helper.dart';
import '../domain/models/note.dart';
import 'note_repository_impl.dart';

/// データベースのテスト用セットアップ
void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  group('Database Tests', () {
    late DatabaseHelper databaseHelper;
    late NoteRepositoryImpl noteRepository;

    setUp(() async {
      databaseHelper = DatabaseHelper();
      noteRepository = NoteRepositoryImpl();
    });

    tearDown(() async {
      await databaseHelper.close();
    });

    test('Database initialization', () async {
      final db = await databaseHelper.database;
      expect(db, isNotNull);
      expect(db.isOpen, isTrue);
    });

    test('Notes table creation', () async {
      final db = await databaseHelper.database;
      
      // テーブルが存在するか確認
      final tables = await db.query('sqlite_master', where: 'type = ? AND name = ?', whereArgs: ['table', 'notes']);
      expect(tables.length, equals(1));
      
      // テーブル構造を確認
      final tableInfo = await db.rawQuery('PRAGMA table_info(notes)');
      expect(tableInfo.length, equals(5)); // id, title, content, created_at, updated_at
      
      // カラム名を確認
      final columnNames = tableInfo.map((col) => col['name'] as String).toList();
      expect(columnNames, contains('id'));
      expect(columnNames, contains('title'));
      expect(columnNames, contains('content'));
      expect(columnNames, contains('created_at'));
      expect(columnNames, contains('updated_at'));
    });

    test('Note CRUD operations', () async {
      // 新規メモ作成
      final note = Note.create(
        title: 'Test Note',
        content: 'This is a test note content.',
      );
      
      final createdNote = await noteRepository.createNote(note);
      expect(createdNote.id, isNotNull);
      expect(createdNote.title, equals('Test Note'));
      expect(createdNote.content, equals('This is a test note content.'));
      
      // メモ取得
      final retrievedNote = await noteRepository.getNoteById(createdNote.id!);
      expect(retrievedNote, isNotNull);
      expect(retrievedNote!.title, equals('Test Note'));
      
      // メモ更新
      final updatedNote = await noteRepository.updateNote(
        createdNote.update(
          title: 'Updated Test Note',
          content: 'This is an updated test note content.',
        ),
      );
      expect(updatedNote.title, equals('Updated Test Note'));
      expect(updatedNote.content, equals('This is an updated test note content.'));
      
      // メモ削除
      final deleteResult = await noteRepository.deleteNote(createdNote.id!);
      expect(deleteResult, isTrue);
      
      // 削除確認
      final deletedNote = await noteRepository.getNoteById(createdNote.id!);
      expect(deletedNote, isNull);
    });

    test('Note search functionality', () async {
      // テストデータ作成
      await noteRepository.createNote(
        Note.create(title: 'Flutter Development', content: 'Learning Flutter framework'),
      );
      await noteRepository.createNote(
        Note.create(title: 'Dart Programming', content: 'Learning Dart language'),
      );
      await noteRepository.createNote(
        Note.create(title: 'Mobile App', content: 'Building mobile applications'),
      );
      
      // 検索テスト
      final flutterResults = await noteRepository.searchNotes('Flutter');
      expect(flutterResults.length, equals(1));
      expect(flutterResults.first.title, equals('Flutter Development'));
      
      final learningResults = await noteRepository.searchNotes('Learning');
      expect(learningResults.length, equals(2));
      
      final emptyResults = await noteRepository.searchNotes('');
      expect(emptyResults.length, equals(3));
    });

    test('Note count functionality', () async {
      // 初期状態
      expect(await noteRepository.getNoteCount(), equals(0));
      
      // メモ作成後
      await noteRepository.createNote(
        Note.create(title: 'Test Note 1', content: 'Content 1'),
      );
      expect(await noteRepository.getNoteCount(), equals(1));
      
      await noteRepository.createNote(
        Note.create(title: 'Test Note 2', content: 'Content 2'),
      );
      expect(await noteRepository.getNoteCount(), equals(2));
    });
  });
}
