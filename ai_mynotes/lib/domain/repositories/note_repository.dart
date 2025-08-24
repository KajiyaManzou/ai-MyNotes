import '../models/note.dart';

/// Noteのデータアクセスを抽象化するリポジトリインターフェース
abstract class NoteRepository {
  /// すべてのメモを取得（作成日時降順）
  Future<List<Note>> getAllNotes();
  
  /// 指定されたIDのメモを取得
  Future<Note?> getNoteById(int id);
  
  /// 新規メモを作成
  Future<Note> createNote(Note note);
  
  /// メモを更新
  Future<Note> updateNote(Note note);
  
  /// メモを削除
  Future<bool> deleteNote(int id);
  
  /// タイトルまたは内容でメモを検索
  Future<List<Note>> searchNotes(String query);
  
  /// メモの総数を取得
  Future<int> getNoteCount();
}
