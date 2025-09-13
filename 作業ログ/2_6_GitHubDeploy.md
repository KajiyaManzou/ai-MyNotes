# 2-6. GitHub Pages デプロイ手順

## 概要
Blazor WebAssembly ai-MyNotes アプリケーションをGitHub Pagesにデプロイするための手順書です。

## 実行日時
- **準備完了日**: 2025-09-08
- **対象リポジトリ**: https://github.com/KajiyaManzou/ai-MyNotes.git
- **デプロイURL**: https://KajiyaManzou.github.io/ai-MyNotes/

## 準備完了状況 ✅

### デプロイファイル作成済み
- ✅ **GitHub Actions ワークフロー**: `.github/workflows/deploy.yml`
- ✅ **README.md**: プロジェクト概要・技術スタック
- ✅ **パフォーマンス検証結果**: 2_5_PerformanceTesting.md
- ✅ **iOS Chrome テストガイド**: ios-chrome-test-guide.md
- ✅ **ローカルコミット完了**: 全ファイル準備済み

### ビルド確認済み
- ✅ **Release ビルド**: 正常完了（15MB）
- ✅ **テスト実行**: 全テスト合格
- ✅ **静的ファイル生成**: wwwrootフォルダ生成確認済み

## 手動実行手順

### Step 1: GitHubリポジトリにプッシュ

#### 1-1. SSH キー設定済み環境で実行
```bash
# DevContainer外の環境（ローカルマシンなど）で実行
git push origin main
```

#### 1-2. プッシュ内容確認
```bash
# 最新コミットの確認
git log --oneline -3
```

**期待される出力:**
```
9b84941 GitHub Pages デプロイ設定とドキュメント追加
dd1be45 README.md 作成
b71882a パフォーマンス検証
```

### Step 2: GitHub Pages設定

#### 2-1. GitHubリポジトリページにアクセス
1. ブラウザで https://github.com/KajiyaManzou/ai-MyNotes を開く
2. **Settings** タブをクリック

#### 2-2. Pages設定
1. 左メニューから **Pages** を選択
2. **Source** セクションで:
   - "Deploy from a branch" から **"GitHub Actions"** を選択
3. 設定を保存

### Step 3: 自動デプロイ実行・確認

#### 3-1. GitHub Actions実行確認
1. リポジトリの **Actions** タブを開く
2. "Deploy to GitHub Pages" ワークフローの実行を確認
3. ✅ 緑色のチェックマークで完了を確認

#### 3-2. デプロイ完了確認
1. **Settings** → **Pages** で公開URLを確認
2. **公開URL**: https://KajiyaManzou.github.io/ai-MyNotes/
3. ブラウザでアクセスしてアプリケーション動作を確認

## GitHub Actions ワークフロー詳細

### ワークフロー内容 (.github/workflows/deploy.yml)
```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  deploy-to-github-pages:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET Core SDK
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore ai-MyNotes/ai-MyNotes.csproj
    - name: Build
      run: dotnet build ai-MyNotes/ai-MyNotes.csproj -c Release --no-restore
    - name: Test
      run: dotnet test ai-MyNotes.Tests/ai-MyNotes.Tests.csproj --no-build --verbosity normal
    - name: Publish
      run: dotnet publish ai-MyNotes/ai-MyNotes.csproj -c Release -o release --nologo
    - name: Change base-tag in index.html from / to ai-MyNotes
      run: sed -i 's/<base href="\/" \/>/<base href="\/ai-MyNotes\/" \/>/g' release/wwwroot/index.html
    - name: Add .nojekyll file
      run: touch release/wwwroot/.nojekyll
    - name: Commit wwwroot to GitHub Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: release/wwwroot
        force_orphan: true
```

### 実行ステップ
1. ✅ **チェックアウト**: ソースコード取得
2. ✅ **.NET 8 セットアップ**: ランタイム準備
3. ✅ **依存関係復元**: NuGetパッケージ取得
4. ✅ **ビルド**: Release設定でコンパイル
5. ✅ **テスト実行**: 全単体テスト実行
6. ✅ **パブリッシュ**: 本番用静的ファイル生成
7. ✅ **base-tag修正**: GitHub Pages用パス調整
8. ✅ **.nojekyll追加**: Jekyll処理回避
9. ✅ **デプロイ**: gh-pagesブランチへ公開

## 期待される結果

### デプロイ成功時の確認項目
- [ ] **GitHub Actions**: ✅ 緑色で完了
- [ ] **公開URL**: https://KajiyaManzou.github.io/ai-MyNotes/ でアクセス可能
- [ ] **Bootstrap UI**: レスポンシブデザインが正常表示
- [ ] **画面遷移**: メモ編集（/）⇔ メモ一覧（/list）が動作
- [ ] **IndexedDB**: ライブラリが正常ロード（コンソールエラーなし）
- [ ] **起動時間**: 2秒以内でアプリケーション初期化完了

### デプロイ後のテスト項目
1. **デスクトップブラウザテスト**
   - Chrome, Firefox, Safari, Edge での動作確認
   - レスポンシブデザインの確認

2. **モバイルブラウザテスト**
   - iOS Chrome: パフォーマンス・操作性確認
   - Android Chrome: 基本動作確認

3. **PWA機能テスト**（Phase 3で実装予定）
   - ホーム画面追加
   - オフライン動作

## トラブルシューティング

### よくある問題と解決方法

#### 問題1: Actions実行失敗
**症状**: GitHub Actionsが赤い×で失敗
**確認点**:
- テストが全て合格しているか
- ai-MyNotes.csproj の依存関係は正しいか
- ビルドエラーがないか

#### 問題2: 404エラー
**症状**: デプロイURLにアクセスすると404
**解決方法**:
- Settings > Pages で正しいブランチ（gh-pages）が選択されているか確認
- base-tag が `/ai-MyNotes/` に正しく変更されているか確認

#### 問題3: Blazorアプリが起動しない
**症状**: ローディング画面のまま停止
**確認点**:
- ブラウザのDevToolsでJavaScriptエラーを確認
- _framework フォルダのWASMファイルが正しくロードされているか
- IndexedDBライブラリのスクリプトが読み込まれているか

## Phase 1完了の確認

### ✅ 完了項目
- 設計・基盤構築
- 環境構築（DevContainer）
- プロジェクト初期化（Blazor WebAssembly）
- 依存関係導入（Bootstrap 5.3 + IndexedDB）
- ルーティング設定（2画面構成）
- テストライブラリ設定（xUnit + bUnit）
- パフォーマンス検証（37ms達成）
- **GitHub Pages デプロイ設定** ← 今回追加

### 🎯 次のフェーズ
Phase 1完了後、**Phase 2: 核心機能実装** に進む準備が整います：
- データモデル実装
- MemoService（IndexedDB接続）実装
- メモ編集・一覧画面の詳細機能実装
- リアルタイム保存機能

## 参考情報

### 関連ドキュメント
- [README.md](README.md) - プロジェクト概要
- [2_5_PerformanceTesting.md](2_5_PerformanceTesting.md) - パフォーマンス測定結果
- [ios-chrome-test-guide.md](ios-chrome-test-guide.md) - 実機テスト手順
- [task.md](task.md) - 全体タスクリスト

### GitHub Pages参考URL
- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [GitHub Actions for .NET](https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net)

---

**実行ステータス**: 準備完了 - 手動プッシュ待ち  
**予想デプロイ時間**: 3-5分  
**成功確率**: 高（事前検証済み）