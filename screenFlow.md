# ai-MyNotes 画面遷移図（2画面構成）

## 基本画面遷移フロー

```mermaid
graph TD
    Start([アプリ起動]) --> MemoEdit
    
    MemoEdit["メモ編集画面<br/>（/ または /edit/{id}）<br/>・メモ本文入力<br/>・リアルタイム保存<br/>・1行目がタイトル"]
    
    MemoList["メモ一覧画面<br/>（/list）<br/>・メモカード一覧表示<br/>・作成日時降順ソート<br/>・タップで編集<br/>・左スワイプで削除"]
    
    ConfirmDialog["削除確認ダイアログ<br/>・メモタイトル表示<br/>・キャンセル/削除選択"]
    
    %% 基本遷移（2画面構成）
    MemoEdit --|📝 メモ一覧ボタン| MemoList
    MemoList --|✏️ 新規作成ボタン| MemoEdit
    MemoList --|メモカードタップ<br/>（編集）| MemoEdit
    
    %% 左スワイプ削除フロー
    MemoList --|左スワイプ| ConfirmDialog
    ConfirmDialog --|キャンセル| MemoList
    ConfirmDialog --|削除実行| MemoList
    
    %% スタイリング
    classDef primary fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef secondary fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    classDef warning fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef start fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    
    class MemoEdit primary
    class MemoList secondary
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
        Typing --> AutoSaving : 3秒停止タイマー
        Typing --> ImmediateSaving : フォーカス離脱（タイマーキャンセル）
        AutoSaving --> Saved : 保存完了
        ImmediateSaving --> Saved : 保存完了
        Saved --> Typing : 継続入力
    }
    
    state MemoEdit_Existing {
        [*] --> LoadMemo : 既存メモデータ読み込み
        LoadMemo --> DisplayContent : メモ内容表示
        DisplayContent --> EditingContent : ユーザー編集
        EditingContent --> AutoUpdating : 3秒停止タイマー
        EditingContent --> ImmediateUpdating : フォーカス離脱（タイマーキャンセル）
        AutoUpdating --> Updated : 更新完了
        ImmediateUpdating --> Updated : 更新完了
        Updated --> EditingContent : 継続編集
    }
    
    state MemoList {
        [*] --> LoadMemoList : メモ一覧読み込み
        LoadMemoList --> DisplayCards : カード形式で表示
        DisplayCards --> EmptyList : メモが0件の場合
        DisplayCards --> SortedList : 作成日時降順ソート
        EmptyList --> DisplayMessage : 「メモがありません」表示
        
        %% 左スワイプ削除状態
        SortedList --> SwipeDetected : 左スワイプ検知
        SwipeDetected --> DeleteButtonRevealed : 削除ボタン表示
        DeleteButtonRevealed --> ConfirmDelete : 削除ボタンタップ
        DeleteButtonRevealed --> SwipeCanceled : スワイプキャンセル
        ConfirmDelete --> DeleteConfirmed : 「削除」選択
        ConfirmDelete --> DeleteCanceled : 「キャンセル」選択
        DeleteConfirmed --> RefreshList : 一覧更新
        DeleteCanceled --> DeleteButtonRevealed : 削除ボタン表示に戻る
        SwipeCanceled --> SortedList : 通常表示に戻る
        RefreshList --> DisplayCards : 更新後一覧表示
    }
    
    %% 画面間遷移
    MemoEdit_New --> MemoList : メモ一覧ボタン
    MemoEdit_Existing --> MemoList : メモ一覧ボタン
    MemoList --> MemoEdit_New : 新規作成ボタン
    MemoList --> MemoEdit_Existing : メモカードタップ
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
    
    subgraph "左スワイプ削除フロー"
        C1[メモ一覧画面] --> C2[左スワイプ操作]
        C2 --> C3[削除ボタン表示]
        C3 --> C4[削除ボタンタップ]
        C4 --> C5[削除確認ダイアログ]
        C5 --> C6{ユーザー選択}
        C6 -->|削除| C7[メモ削除実行]
        C6 -->|キャンセル| C3
        C7 --> C8[一覧更新]
        C8 --> C1
        C3 --> C1
    end
    
    subgraph "画面遷移フロー（2画面構成）"
        D1[メモ編集] <--> D2[メモ一覧]
    end
```

## URL ルーティング仕様（2画面構成）

```mermaid
graph TD
    Root["/"] --> MemoEdit_New["メモ編集画面<br/>（新規作成モード）"]
    
    EditWithId["/edit/{id}"] --> MemoEdit_Existing["メモ編集画面<br/>（編集モード）<br/>指定IDのメモを読み込み"]
    
    List["/list"] --> MemoList_Display["メモ一覧画面<br/>作成日時降順で表示<br/>左スワイプ削除機能"]
    
    %% パラメータ例
    EditExample["/edit/12345678-1234-4567-8901-123456789012"] --> EditWithId
    
    %% ナビゲーション（簡素化）
    MemoEdit_New -.->|メモ一覧ボタン| List
    MemoEdit_Existing -.->|メモ一覧ボタン| List
    MemoList_Display -.->|新規作成ボタン| Root
    MemoList_Display -.->|カードタップ| EditWithId
    
    classDef route fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
    classDef screen fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    
    class Root,EditWithId,List,EditExample route
    class MemoEdit_New,MemoEdit_Existing,MemoList_Display screen
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
    
    Note over U,DB: 左スワイプ削除フロー
    
    U->>UI: メモカードを左にスワイプ
    UI->>UI: 削除ボタンを右側に表示
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
    ErrorType -->|スワイプエラー| SwipeError[スワイプ操作エラー]
    ErrorType -->|ネットワークエラー| NetworkError[オフライン状態処理]
    
    SaveError --> RetryDialog[リトライ確認ダイアログ]
    LoadError --> FallbackDisplay[フォールバック表示]
    DeleteError --> ErrorMessage[エラーメッセージ表示]
    SwipeError --> SwipeRetry[スワイプリトライまたはキャンセル]
    NetworkError --> OfflineMode[オフラインモード継続]
    
    RetryDialog --> Process
    FallbackDisplay --> UserAction[ユーザー手動操作]
    ErrorMessage --> PreviousState[前の状態に復帰]
    SwipeRetry --> SwipeCancel[スワイプキャンセル]
    SwipeCancel --> NormalFlow
    OfflineMode --> QueueAction[アクション待機]
    
    classDef error fill:#ffebee,stroke:#d32f2f,stroke-width:2px
    classDef retry fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    classDef normal fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    
    class ErrorType,SaveError,LoadError,DeleteError,NetworkError,SwipeError error
    class RetryDialog,FallbackDisplay,ErrorMessage,SwipeRetry retry
    class NormalFlow,OfflineMode,QueueAction,SwipeCancel normal
```

## 左スワイプ削除の詳細動作

```mermaid
graph TD
    Start[通常のメモカード] --> SwipeStart[左スワイプ開始]
    SwipeStart --> SwipeProgress[スワイプ進行中]
    SwipeProgress --> ThresholdCheck{スワイプ距離<br/>100px以上？}
    
    ThresholdCheck -->|Yes| RevealButton[削除ボタン表示]
    ThresholdCheck -->|No| SwipeCancel[スワイプキャンセル]
    
    RevealButton --> ButtonTap[削除ボタンタップ]
    RevealButton --> OutsideTap[他の場所タップ]
    RevealButton --> RightSwipe[右スワイプ]
    
    ButtonTap --> ConfirmDialog[削除確認ダイアログ]
    OutsideTap --> SwipeCancel
    RightSwipe --> SwipeCancel
    
    ConfirmDialog --> DeleteConfirm[削除確認]
    ConfirmDialog --> DeleteCancel[削除キャンセル]
    
    DeleteConfirm --> ExecuteDelete[メモ削除実行]
    DeleteCancel --> SwipeCancel
    
    ExecuteDelete --> UpdateList[一覧更新]
    SwipeCancel --> Start
    UpdateList --> Start
    
    classDef normal fill:#e8f5e8,stroke:#388e3c,stroke-width:2px
    classDef action fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    classDef warning fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    
    class Start,SwipeCancel,UpdateList normal
    class SwipeStart,SwipeProgress,RevealButton,ButtonTap action
    class ThresholdCheck,ConfirmDialog,DeleteConfirm,ExecuteDelete warning
```

---

## 設計の特徴

### 1. シンプルな2画面構成
- メモ編集画面 ↔ メモ一覧画面
- 複雑な画面遷移を排除
- ユーザーの認知負荷を軽減

### 2. 直感的な削除操作
- iOS標準的な左スワイプ削除
- 削除確認ダイアログによる安全性
- 物理削除による確実なデータ削除

### 3. リアルタイム保存
- フォーカス離脱優先のタイマー制御
- 3秒停止での自動保存
- データ損失リスクの最小化

### 4. レスポンシブな状態管理
- 各画面の独立した状態管理
- エラーハンドリングの充実
- オフライン対応の考慮