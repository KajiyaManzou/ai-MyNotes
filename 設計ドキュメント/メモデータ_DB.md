# メモデータ データベース

## データベース: Safari IndexedDB

## データ構造

```json
{
    id: string,
    title: string,
    content: string,
    createdAt: Date,
    updatedAt: Date
}
```

## データ項目

- id: string
    - 最初の保存時に自動生成されるUUID 
- title: string
    - メモのタイトル
    - 最大文字数: 100文字
    - 日本語対応
    - contentの1行目
- content: string
    - メモ本文
    - 最大文字数: 1000文字
    - 日本語対応
- createdAt: Date
    - メモの作成日時
- updatedAt: Date
    - メモの最終保存日時

## データモデル
```mermaid
erDiagram
      MEMO {
          string id PK "UUID自動生成"
          string title "最大100文字, contentの1行目"
          string content "最大1000文字, 日本語対応"
          Date createdAt "作成日時"
          Date updatedAt "最終保存日時"
      }

      %% IndexedDBの制約とインデックス情報
      MEMO ||--o{ INDEX_createdAt : "降順ソート用インデックス"
      MEMO ||--o{ INDEX_updatedAt : "検索・ソート用インデックス"
```

## データ操作
```mermaid
  graph TD
      subgraph "IndexedDB (Safari)"
          DB[("ai-MyNotes Database<br/>Version: 1")]

          subgraph "Object Store: memos"
              STORE["Object Store<br/>KeyPath: 'id'<br/>AutoIncrement: false"]

              subgraph "データ構造"
                  MEMO_ENTITY["Memo Entity<br/>├── id: string (PK)<br/>├──
  title: string (100文字)<br/>├── content: string (1000文字)<br/>├── createdAt:
   Date<br/>└── updatedAt: Date"]
              end

              subgraph "インデックス"
                  IDX1["Index: createdAt<br/>Unique: false<br/>降順ソート用"]
                  IDX2["Index: updatedAt<br/>Unique: false<br/>検索・更新用"]
              end
          end
      end

      subgraph "データ操作"
          CREATE["CREATE<br/>UUID生成 → 保存"]
          READ["READ<br/>createdAt降順取得"]
          UPDATE["UPDATE<br/>updatedAt更新"]
          DELETE["DELETE<br/>物理削除"]
      end

      DB --> STORE
      STORE --> MEMO_ENTITY
      STORE --> IDX1
      STORE --> IDX2

      CREATE --> STORE
      READ --> IDX1
      UPDATE --> STORE
      DELETE --> STORE

      style DB fill:#e1f5fe
      style STORE fill:#f3e5f5
      style MEMO_ENTITY fill:#e8f5e8
      style IDX1 fill:#fff3e0
      style IDX2 fill:#fff3e0
```

## バリデーション規則

  1. IDフィールド (id: string)

  必須チェック
  - 空文字列・null・undefined不可
  - UUID形式の検証（36文字、ハイフン含む）

  形式チェック
  ^[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$

  2. タイトルフィールド (title: string)

  必須チェック
  - 空文字列は許可（contentの1行目から自動生成）
  - null・undefined不可

  文字数制限
  - 最大100文字
  - 日本語文字を考慮したUnicode文字数カウント

  文字種制御
  - 制御文字（\n, \r, \t等）の除去
  - 先頭・末尾の空白文字トリム

  3. 本文フィールド (content: string)

  必須チェック
  - 空文字列は許可
  - null・undefined不可

  文字数制限
  - 最大1000文字
  - 日本語文字を考慮したUnicode文字数カウント

  文字種制御
  - タブ文字は4スペースに変換
  - 連続する改行は2つまでに制限

  4. 作成日時 (createdAt: Date)

  必須チェック
  - null・undefined不可
  - 有効なDateオブジェクトであること

  値域チェック
  - 1900年1月1日以降
  - 現在時刻以降は不可（未来日時不可）

  5. 更新日時 (updatedAt: Date)

  必須チェック
  - null・undefined不可
  - 有効なDateオブジェクトであること

  値域チェック
  - createdAt以降の日時であること
  - 現在時刻以降は不可（未来日時不可）

  実装上の考慮事項

  データ保存時の自動処理

  1. titleの自動生成: contentが存在する場合、contentの1行目を抽出してtitleに設定
        titleが存在する場合、contentの1行目を抽出してtitleを更新
  2. 日時の自動設定: 新規作成時にcreatedAt・updatedAtを現在時刻に設定
  3. UUIDの自動生成: 新規作成時にIDを自動生成

  エラーハンドリング

  - バリデーションエラー時は具体的なエラーメッセージを返す
  - 複数エラーが発生した場合は配列で全てのエラーを返す
  - ユーザーフレンドリーなメッセージに変換

  パフォーマンス最適化

  - 文字数カウントはリアルタイムで実行しない（保存時のみ）
  - 重い正規表現チェックは最小限に抑制