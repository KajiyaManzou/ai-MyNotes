# GitHub Actions CI/CD設定

## ワークフロー概要

`deploy.yml`は、Blazor WebAssemblyアプリケーションを自動的にGitHub Pagesにデプロイするためのワークフローです。

## トリガー条件

- `main`ブランチへのpush
- `main`ブランチへのpull request（ビルドテストのみ）

## ワークフロー詳細

### 1. Build Job
- .NET 8.0環境のセットアップ
- 依存関係の復元
- **単体テストの実行**（新規追加）
- リリースビルドの実行
- Blazor WebAssemblyアプリのパブリッシュ
- GitHub Pages用のパス設定
  - `base href`の動的設定
  - PWA manifest.jsonのパス更新
- `.nojekyll`ファイルの追加

### 2. Deploy Job  
- GitHub Pages環境への自動デプロイ
- mainブランチプッシュ時のみ実行

## 主要な改良点

### 従来版との比較
| 項目 | 従来版 | 改良版 |
|------|--------|--------|
| テスト実行 | コメントアウト | **自動実行** |
| Deployment方式 | peaceiris/actions-gh-pages | **公式GitHub Pages Actions** |
| パス設定 | 固定（ai-MyNotes） | **リポジトリ名から動的生成** |
| PWA対応 | なし | **manifest.json自動更新** |
| 権限設定 | 従来のtoken方式 | **id-token方式（推奨）** |

### セキュリティ強化
- `permissions`設定による最小権限の原則
- `concurrency`制御による重複実行防止
- id-token方式による安全なデプロイメント

## PWA対応

ワークフローはPWAアプリケーションに対応しており、以下を自動実行：

1. **manifest.jsonの更新**
   - `start_url`をGitHub Pagesのサブパスに変更
   - `scope`をGitHub Pagesのサブパスに変更

2. **Service Worker対応**
   - 将来のService Worker実装時にも対応可能な構造

## リポジトリ設定要件

### GitHub Pages設定
1. Settings > Pages
2. Source: "GitHub Actions"を選択
3. 自動でワークフローが実行される

### 必要な権限
ワークフローファイルに以下が設定済み：
```yaml
permissions:
  contents: read
  pages: write  
  id-token: write
```

## トラブルシューティング

### よくある問題

1. **テスト失敗によるデプロイ停止**
   - 全ての単体テストが成功する必要があります
   - テスト修正後に再プッシュしてください

2. **PWAアイコンパスエラー** 
   - アイコンファイルが正しくwwwroot/iconsに配置されているか確認
   - manifest.jsonのパス設定を確認

3. **GitHub Pages 404エラー**
   - リポジトリ名とbase hrefが一致しているか確認
   - `.nojekyll`ファイルが生成されているか確認

### デバッグ手順

1. Actions タブでワークフロー実行状況を確認
2. ビルドログでエラー詳細を確認  
3. 成果物（artifact）をダウンロードして内容を確認

## 今後の拡張予定

- Service Worker対応の自動デプロイ
- Lighthouse CI連携でのPWA品質チェック
- 複数環境デプロイ（staging/production）
- キャッシュ戦略の最適化