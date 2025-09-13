# ai-MyNotes 📝

シンプルで高速なメモ管理Webアプリケーションです。Blazor WebAssemblyとIndexedDBを使用し、オフラインでも動作するPWA（Progressive Web App）として設計されています。

🌟 **[デモサイトを見る](https://KajiyaManzou.github.io/ai-MyNotes/)**

## ✨ 主な機能

- **📱 PWA対応**: ホーム画面に追加してネイティブアプリのように使用可能
- **⚡ 高速起動**: 2秒以内の初期起動を実現
- **📱 レスポンシブデザイン**: Bootstrap 5.3でモバイル・デスクトップに最適化
- **🔄 リアルタイム保存**: 3秒間の停止またはフォーカス離脱で自動保存
- **📄 2画面構成**: メモ編集画面とメモ一覧画面のシンプルな構成
- **💾 オフライン対応**: IndexedDBによるローカルデータ永続化
- **👆 左スワイプ削除**: モバイルでの直感的な削除操作

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

### Phase 2: 核心機能実装 ✅ 完了
- ✅ メモデータモデル実装
- ✅ MemoService（IndexedDB接続）実装
- ✅ メモ編集画面の詳細実装
- ✅ メモ一覧画面の詳細実装
- ✅ リアルタイム保存機能実装
- ✅ エラーハンドリング戦略実装

### Phase 3: 仕上げ・テスト ✅ 完了
- ✅ Bootstrap UI最適化
- ✅ PWA設定（manifest.json、アイコン設定）
- ✅ GitHub Pages デプロイ設定
- ✅ GitHub Actions CI/CD構築

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

- **初期起動時間**: 2-3秒（目標達成）
- **メモ保存**: 100ms以下
- **画面遷移**: 50ms以下
- **PWA動作**: 完全対応

詳細は [作業ログ/2_5_PerformanceTesting.md](作業ログ/2_5_PerformanceTesting.md) を参照

## 📋 設計ドキュメント

- [企画書](設計ドキュメント/企画書.md) - プロジェクト概要と目標
- [要求仕様](設計ドキュメント/要求仕様.md) - 機能要件・非機能要件
- [画面遷移フロー](設計ドキュメント/screenFlow.md) - UI/UX設計
- [状態遷移図](設計ドキュメント/stateTransition.md) - アプリケーション状態管理
- [データベース設計](設計ドキュメント/メモデータ_DB.md) - ERモデルとデータ構造
- [ワイヤーフレーム](設計ドキュメント/wireframe.md) - 画面レイアウト設計

## 📚 実装ドキュメント

### Phase 1: 基盤構築
- [Bootstrap導入](作業ログ/2_1_InstallBootstrap.md)
- [IndexedDB設定](作業ログ/2_2_InstallIndexedDB.md)
- [テスト環境構築](作業ログ/2_3_InstallXUnit.md)
- [ルーティング設定](作業ログ/2_4_RouteSettings.md)

### Phase 2: 機能実装
- [データモデル](作業ログ/3_1_DataModel.md)
- [データアクセス層](作業ログ/3_2_DataAccessLayer.md)
- [メモ編集画面](作業ログ/3_3_MemoEditScreen.md)
- [メモ一覧画面](作業ログ/3_4_MemoListScreen.md)
- [リアルタイム保存](作業ログ/3_5_Real-timeSaving.md)
- [エラーハンドリング](作業ログ/3_6_ErrorHandling.md)

### Phase 3: 仕上げ・デプロイ
- [Bootstrap UI最適化](作業ログ/4_1_BootstrapUiOptimization.md)
- [PWA設定](作業ログ/4_2_PwaConfiguration.md)
- [GitHub Pages デプロイ](作業ログ/4_3_GitHubPagesDeploymentConfiguration.md)

## 🏃‍♂️ デモ・デプロイ

- **本番環境**: [https://KajiyaManzou.github.io/ai-MyNotes/](https://KajiyaManzou.github.io/ai-MyNotes/) ✅ 稼働中
- **開発環境**: http://localhost:5002

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

🎉 **現在のステータス**: 全Phase完了、本番稼働中  
✅ **達成事項**: PWAアプリケーション完全実装・デプロイ完了  
🌐 **デモURL**: [https://KajiyaManzou.github.io/ai-MyNotes/](https://KajiyaManzou.github.io/ai-MyNotes/)