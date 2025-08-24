import 'package:sqflite/sqflite.dart';
import '../domain/models/note.dart';
import '../domain/repositories/note_repository.dart';
import 'database_helper.dart';

/// NoteRepositoryのSQLite実装クラス
class NoteRepositoryImpl implements NoteRepository {
  final DatabaseHelper _databaseHelper = DatabaseHelper();

  @override
  Future<List<Note>> getAllNotes() async {
    final db = await _databaseHelper.database;
    final List<Map<String, dynamic>> maps = await db.query(
      'notes',
      orderBy: 'created_at DESC',
    );
    
    return List.generate(maps.length, (i) {
      return Note.fromMap(maps[i]);
    });
  }

  @override
  Future<Note?> getNoteById(int id) async {
    final db = await _databaseHelper.database;
    final List<Map<String, dynamic>> maps = await db.query(
      'notes',
      where: 'id = ?',
      whereArgs: [id],
      limit: 1,
    );
    
    if (maps.isEmpty) return null;
    return Note.fromMap(maps.first);
  }

  @override
  Future<Note> createNote(Note note) async {
    final db = await _databaseHelper.database;
    
    // 新規作成時はIDを除外
    final map = note.toMap();
    map.remove('id');
    
    final id = await db.insert('notes', map);
    
    // 作成されたNoteオブジェクトを返す
    return note.copyWith(id: id);
  }

  @override
  Future<Note> updateNote(Note note) async {
    if (note.id == null) {
      throw ArgumentError('Note must have an ID to update');
    }
    
    final db = await _databaseHelper.database;
    
    // 更新日時を現在時刻に設定
    final updatedNote = note.update(
      title: note.title,
      content: note.content,
    );
    
    final count = await db.update(
      'notes',
      updatedNote.toMap(),
      where: 'id = ?',
      whereArgs: [note.id],
    );
    
    if (count == 0) {
      throw Exception('Note with ID ${note.id} not found');
    }
    
    return updatedNote.copyWith(id: note.id);
  }

  @override
  Future<bool> deleteNote(int id) async {
    final db = await _databaseHelper.database;
    final count = await db.delete(
      'notes',
      where: 'id = ?',
      whereArgs: [id],
    );
    
    return count > 0;
  }

  @override
  Future<List<Note>> searchNotes(String query) async {
    if (query.trim().isEmpty) {
      return getAllNotes();
    }
    
    final db = await _databaseHelper.database;
    final searchQuery = '%$query%';
    
    final List<Map<String, dynamic>> maps = await db.query(
      'notes',
      where: 'title LIKE ? OR content LIKE ?',
      whereArgs: [searchQuery, searchQuery],
      orderBy: 'created_at DESC',
    );
    
    return List.generate(maps.length, (i) {
      return Note.fromMap(maps[i]);
    });
  }

  @override
  Future<int> getNoteCount() async {
    final db = await _databaseHelper.database;
    final result = await db.rawQuery('SELECT COUNT(*) as count FROM notes');
    return Sqflite.firstIntValue(result) ?? 0;
  }
}
