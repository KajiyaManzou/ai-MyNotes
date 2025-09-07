# ai-MyNotes 画面遷移図

## 基本画面遷移フロー

```mermaid
graph TD
    Start([アプリ起動]) --> MemoEdit
    
    MemoEdit["メモ編集画面<br/>（/ または /edit/{id}）<br/>・メモ本文入力<br/>・リアルタイム保存<br/>・1行目がタイトル"]
    
    MemoList["メモ一覧画面<br/>（/list）<br/>・メモカード一覧表示<br/>・作成日時降順ソート<br/>・タップで編集"]
    
    MemoListEdit["メモ一覧編集画面<br/>（/list-edit）<br/>・削除ボタン付きカード<br/>・削除確認ダイアログ<br/>・物理削除"]
    
    ConfirmDialog["削除確認ダイアログ<br/>・メモタイトル表示<br/>・キャンセル/削除選択"]
    
    %% 基本遷移
    MemoEdit --|📝 メモ一覧ボタン| MemoList
    MemoList --|✏️ 新規作成ボタン| MemoEdit
    MemoList --|メモカードタップ<br/>（編集）| MemoEdit
    MemoList --|🗂️ 編集モードボタン| MemoListEdit
    MemoListEdit --|← 戻るボタン| MemoList
    
    %% 削除フロー
    MemoListEdit --|×削除ボタン| ConfirmDialog
    ConfirmDialog --|キャンセル| MemoListEdit
    ConfirmDialog --|削除実行| MemoListEdit
    
    %% スタイリング
    classDef primary fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef secondary fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef warning fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef start fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    
    class MemoEdit primary
    class MemoList secondary
    class MemoListEdit secondary
    class ConfirmDialog warning
    class Start start
```

## 詳細画面遷移パターン

```mermaid
stateDiagram-v2
    [*] --> AppStart : アプリ起動
    
    AppStart --> MemoEdit_New : 新規メモ作成画面
    
    state MemoEdit_New {
        [*] --> EmptyTextArea : 空のテキストエリア表示
        EmptyTextArea --> Typing : ユーザー入力開始
        Typing --> AutoSaving : 3秒停止 or フォーカス離脱
        AutoSaving --> Saved : 保存完了
        Saved --> Typing : 継続入力
    }
    
    state MemoEdit_Existing {
        [*] --> LoadMemo : 既存メモデータ読み込み
        LoadMemo --> DisplayContent : メモ内容表示
        DisplayContent --> EditingContent : ユーザー編集
        EditingContent --> AutoUpdating : 3秒停止 or フォーカス離脱
        AutoUpdating --> Updated : 更新完了
        Updated --> EditingContent : 継続編集
    }
    
    state MemoList {
        [*] --> LoadMemoList : メモ一覧読み込み
        LoadMemoList --> DisplayCards : カード形式で表示
        DisplayCards --> EmptyList : メモが0件の場合
        DisplayCards --> SortedList : 作成日時降順ソート
        EmptyList --> DisplayMessage : 「メモがありません」表示
    }
    
    state MemoListEdit {
        [*] --> LoadEditableList : 編集可能一覧読み込み
        LoadEditableList --> DisplayDeleteButtons : 削除ボタン付きカード表示
        DisplayDeleteButtons --> ConfirmDelete : 削除ボタンタップ
        ConfirmDelete --> DeleteConfirmed : 「削除」選択
        ConfirmDelete --> DeleteCancelled : 「キャンセル」選択
        DeleteConfirmed --> RefreshList : 一覧更新
        DeleteCancelled --> DisplayDeleteButtons : 削除ボタン表示に戻る
        RefreshList --> DisplayDeleteButtons : 更新後一覧表示
    }
    
    %% 画面間遷移
    MemoEdit_New --> MemoList : メモ一覧ボタン
    MemoEdit_Existing --> MemoList : メモ一覧ボタン
    MemoList --> MemoEdit_New : 新規作成ボタン
    MemoList --> MemoEdit_Existing : メモカードタップ
    MemoList --> MemoListEdit : 編集モードボタン
    MemoListEdit --> MemoList : 戻るボタン
```

## ユーザーアクション別フロー

```mermaid
flowchart LR
    subgraph "新規メモ作成フロー"
        A1[アプリ起動] --> A2[メモ編集画面]
        A2 --> A3[テキスト入力]
        A3 --> A4[自動保存]
        A4 --> A5[メモ完成]
    end
    
    subgraph "既存メモ編集フロー"
        B1[メモ一覧画面] --> B2[メモカードタップ]
        B2 --> B3[メモ編集画面<br/>（データ読み込み済み）]
        B3 --> B4[内容編集]
        B4 --> B5[自動保存]
        B5 --> B6[編集完了]
    end
    
    subgraph "メモ削除フロー"
        C1[メモ一覧画面] --> C2[編集モードボタン]
        C2 --> C3[メモ一覧編集画面]
        C3 --> C4[削除ボタンタップ]
        C4 --> C5[削除確認ダイアログ]
        C5 --> C6{ユーザー選択}
        C6 -->|削除| C7[メモ削除実行]
        C6 -->|キャンセル| C3
        C7 --> C8[一覧更新]
        C8 --> C3
    end
    
    subgraph "画面遷移フロー"
        D1[メモ編集] <--> D2[メモ一覧]
        D2 <--> D3[メモ一覧編集]
        D2 --> D1
    end
```

## URL ルーティング仕様

```mermaid
graph TD
    Root["/"] --> MemoEdit_New["メモ編集画面<br/>（新規作成モード）"]
    
    EditWithId["/edit/{id}"] --> MemoEdit_Existing["メモ編集画面<br/>（編集モード）<br/>指定IDのメモを読み込み"]
    
    List["/list"] --> MemoList_Display["メモ一覧画面<br/>作成日時降順で表示"]
    
    ListEdit["/list-edit"] --> MemoListEdit_Display["メモ一覧編集画面<br/>削除機能付き"]
    
    %% パラメータ例
    EditExample["/edit/12345678-1234-4567-8901-123456789012"] --> EditWithId
    
    %% ナビゲーション
    MemoEdit_New -.->|メモ一覧ボタン| List
    MemoEdit_Existing -.->|メモ一覧ボタン| List
    MemoList_Display -.->|新規作成ボタン| Root
    MemoList_Display -.->|カードタップ| EditWithId
    MemoList_Display -.->|編集モードボタン| ListEdit
    MemoListEdit_Display -.->|戻るボタン| List
    
    classDef route fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
    classDef screen fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    
    class Root,EditWithId,List,ListEdit,EditExample route
    class MemoEdit_New,MemoEdit_Existing,MemoList_Display,MemoListEdit_Display screen
```

## 状態管理とデータフロー

```mermaid
sequenceDiagram
    participant U as ユーザー
    participant UI as UI画面
    participant S as サービス層
    participant DB as IndexedDB
    
    Note over U,DB: 新規メモ作成フロー
    
    U->>UI: アプリ起動
    UI->>UI: メモ編集画面表示（新規）
    U->>UI: テキスト入力
    UI->>UI: 3秒タイマー開始
    UI->>S: 自動保存トリガー
    S->>S: UUID生成
    S->>S: 1行目をタイトルに抽出
    S->>DB: メモデータ保存
    DB-->>S: 保存完了
    S-->>UI: 保存成功通知
    UI->>UI: 保存完了インジケーター表示
    
    Note over U,DB: メモ一覧表示フロー
    
    U->>UI: メモ一覧ボタンタップ
    UI->>S: メモ一覧取得要求
    S->>DB: 全メモ取得（作成日時降順）
    DB-->>S: メモ一覧データ
    S-->>UI: ソート済みメモ一覧
    UI->>UI: カード形式で表示
    
    Note over U,DB: メモ削除フロー
    
    U->>UI: 編集モードボタンタップ
    UI->>UI: メモ一覧編集画面表示
    U->>UI: 削除ボタンタップ
    UI->>UI: 削除確認ダイアログ表示
    U->>UI: 削除確認
    UI->>S: メモ削除要求
    S->>DB: 物理削除実行
    DB-->>S: 削除完了
    S-->>UI: 削除成功通知
    UI->>UI: 一覧更新・再描画
```

## エラーハンドリングフロー

```mermaid
graph TD
    Action[ユーザーアクション] --> Process[処理実行]
    Process --> Success{処理成功？}
    
    Success -->|成功| NormalFlow[正常フロー継続]
    Success -->|失敗| ErrorType{エラー種別}
    
    ErrorType -->|保存失敗| SaveError[保存エラー処理]
    ErrorType -->|読み込み失敗| LoadError[読み込みエラー処理]
    ErrorType -->|削除失敗| DeleteError[削除エラー処理]
    ErrorType -->|ネットワークエラー| NetworkError[オフライン状態処理]
    
    SaveError --> RetryDialog[リトライ確認ダイアログ]
    LoadError --> FallbackDisplay[フォールバック表示]
    DeleteError --> ErrorMessage[エラーメッセージ表示]
    NetworkError --> OfflineMode[オフラインモード継続]
    
    RetryDialog --> Process
    FallbackDisplay --> UserAction[ユーザー手動操作]
    ErrorMessage --> PreviousState[前の状態に復帰]
    OfflineMode --> QueueAction[アクション待機]
    
    classDef error fill:#ffebee,stroke:#d32f2f,stroke-width:2px
    classDef retry fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef normal fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    
    class ErrorType,SaveError,LoadError,DeleteError,NetworkError error
    class RetryDialog,FallbackDisplay,ErrorMessage retry
    class NormalFlow,OfflineMode,QueueAction normal
```