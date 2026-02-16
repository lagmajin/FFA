# Copilot Instructions

## User Interaction Style
- Respond to users as an expert or consultant, providing knowledgeable and professional guidance.

## コーディングルール
- 特別な指示がない限り、C#ファイル（.cs）のみ編集する
- UIファイル（.razor, .css, .jsなど）は特別な指示がない限り編集禁止

## UI / Implementation Rules for this project

### 重要: 上部メニューの変更ポリシー
- `Components/Layout/MainLayout.razor` 等の上部メニューは、ユーザーまたはチームからの明示的な指示がない限り変更してはなりません。変更が必要な場合は issue を作成し、コードレビューと承認を得てから実施してください。


### ヘッダーメニュー（上部ナビゲーション）表示ルール

**未認証時（ホーム画面 `/` で表示）**
- `ログイン` → `/login`
- `新規登録` → `/register`
- `テストプレイ` → ホーム内で testplay 処理実行

**認証済時（全ページで表示）**
- `ログアウト` のみ

**未認証時（ホーム以外のページ）**
- `ログイン` 
- `新規登録`

### ゲームメニュー配置
- ゲーム操作メニュー（移動、戦闘、ショップ、クエストなど）は街（Town）やフィールド（Field）の中央コンテンツ領域に配置する
- 上部メニューはグローバルなナビゲーション／アカウント操作に限定する

### ホーム画面実装仕様
詳細は `docs/HOME_PAGE_SPEC.md` を参照。以下の実装が必須：
- `Components/Layout/MainLayout.razor` のヘッダーで `<AuthorizeView Context="auth">` を使用
- `auth.User.Identity?.IsAuthenticated` と `IsHomePage()` メソッドで分岐
- テストプレイは `/signin` エンドポイントを Fetch 経由で呼び出し、クッキーを発行してから `/status` へ遷移

(このファイルはリポジトリ内の実装ルールです。外部の `..\\..\\..\\copilot-instructions.md` とは別にプロジェクト内ルールを明記しています。)

## 実装済み機能
実装済み機能は [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) を参照してください。