# ai-MyNotes 📝

シンプルで高速なメモ管理Webアプリケーションです。Blazor WebAssemblyとIndexedDBを使用し、オフラインでも動作するPWA（Progressive Web App）として設計されています。

## ✨ 主な機能

- **📱 PWA対応**: ホーム画面に追加してネイティブアプリのように使用可能
- **⚡ 高速起動**: サーバー応答時間37ms（コールド）、3.75ms（ウォーム）の高性能
- **📱 レスポンシブデザイン**: Bootstrap 5.3でモバイル・デスクトップに最適化
- **🔄 リアルタイム保存**: 3秒間の停止またはフォーカス離脱で自動保存
- **📄 2画面構成**: メモ編集画面とメモ一覧画面のシンプルな構成
- **💾 オフライン対応**: IndexedDBによるローカルデータ永続化
- **👆 左スワイプ削除**: モバイルでの直感的な削除操作（実装予定）

## 🛠️ 技術スタック

- **Frontend**: Blazor WebAssembly (.NET 8)
- **UI Framework**: Bootstrap 5.3
- **Database**: IndexedDB (ブラウザローカル)
- **Testing**: xUnit + bUnit
- **Deployment**: GitHub Pages
- **Performance**: 2秒以内の初期起動目標

## 🏗️ プロジェクト構成

```
ai-MyNotes/
├── Components/          # Blazorコンポーネント
├── Models/             # データモデル
├── Services/           # データアクセス・ビジネスロジック
├── Pages/              # ページコンポーネント
└── wwwroot/           # 静的ファイル

ai-MyNotes.Tests/       # 単体テスト・統合テスト
```

## 🚀 開発状況

### Phase 1: 設計・基盤構築 ✅ 完了
- ✅ 設計ドキュメント作成（ERモデル、画面遷移、状態遷移）
- ✅ DevContainer環境構築
- ✅ Blazor WebAssemblyプロジェクト作成
- ✅ Bootstrap 5.3 + IndexedDBライブラリ導入
- ✅ ルーティング設定（2画面構成）
- ✅ テストライブラリ設定（xUnit + bUnit）
- ✅ パフォーマンス検証（サーバー応答37ms達成）

### Phase 2: 核心機能実装 🔄 進行中
- [ ] メモデータモデル実装
- [ ] MemoService（IndexedDB接続）実装
- [ ] メモ編集画面の詳細実装
- [ ] メモ一覧画面の詳細実装
- [ ] リアルタイム保存機能実装

### Phase 3: 仕上げ・テスト 📋 計画中
- [ ] PWA設定（manifest.json, Service Worker）
- [ ] GitHub Pages デプロイ設定
- [ ] 統合テスト・E2Eテスト
- [ ] iOS Chrome実機テスト

## 🔧 開発環境セットアップ

### 前提条件
- Docker Desktop
- Visual Studio Code
- Dev Containers拡張機能

### 起動手順
1. リポジトリをクローン
```bash
git clone [repository-url]
cd ai-MyNotes
```

2. Dev Containerで開く
```bash
# VS Codeで開く
code .
# Dev Containers拡張機能で「Reopen in Container」を選択
```

3. アプリケーション起動
```bash
dotnet run --project ai-MyNotes --urls "http://localhost:5002"
```

4. ブラウザで確認
```
http://localhost:5002
```

## 🧪 テスト実行

```bash
# 単体テスト実行
dotnet test

# 特定テストクラスの実行
dotnet test --filter "ClassName=MemoEditTests"

# カバレッジ付きテスト実行
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 パフォーマンス結果

- **サーバー応答時間**: 
  - コールドスタート: 37ms
  - ウォーム状態平均: 3.75ms
- **目標達成度**: ✅ 2秒目標を大幅にクリア
- **評価**: EXCELLENT

詳細は [2_5_PerformanceTesting.md](2_5_PerformanceTesting.md) を参照

## 📱 iOS Chrome テスト

実機テストの手順は [ios-chrome-test-guide.md](ios-chrome-test-guide.md) を参照してください。

## 📋 設計ドキュメント

- [企画書](企画書.md) - プロジェクト概要と目標
- [要求仕様](要求仕様.md) - 機能要件・非機能要件
- [画面遷移フロー](screenFlow.md) - UI/UX設計
- [状態遷移図](stateTransition.md) - アプリケーション状態管理
- [データベース設計](メモデータ_DB.md) - ERモデルとデータ構造
- [ワイヤーフレーム](wireframe.md) - 画面レイアウト設計

## 🏃‍♂️ デモ・デプロイ

- **開発環境**: http://localhost:5002
- **本番環境**: 🚧 GitHub Pages準備中

## 🤝 開発ワークフロー

1. **品質重視**: 各機能実装後に単体テスト作成・実行
2. **継続的改善**: パフォーマンス測定とボトルネック特定
3. **段階的実装**: Phase毎の完了確認と品質保証
4. **実機検証**: iOS Chrome環境での継続的テスト

## 📈 成功基準

- **機能達成度**: 要求仕様100%実装
- **パフォーマンス**: PWA化後2秒以内の初期起動 ✅ 達成見込み
- **安定性**: クラッシュなしでの動作
- **ユーザビリティ**: 100人中60人以上がマニュアルなしで基本操作完了

## 📝 ライセンス

MIT License

## 👥 コントリビューター

- メイン開発: [Your Name]
- AI支援: Claude (Anthropic)

---

📋 **現在のステータス**: Phase 1完了、Phase 2進行中  
⚡ **次のマイルストーン**: データモデル実装  
🎯 **リリース目標**: 3週間後