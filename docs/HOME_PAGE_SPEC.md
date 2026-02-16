# ホーム画面 実装仕様

## 概要
ホーム画面は FFA ゲームのエントリーポイントです。未ログイン状態では「ゲーム紹介 + ログイン/新規登録/テストプレイ」を表示します。

## ページ URL
- `/` (ルートパス)

## ヘッダーメニュー（上部ナビゲーション）表示ルール

### 未認証時（ホーム画面 `/` で表示）
**必ず以下の3つだけ表示する：**
- `ログイン` (→ `/login`)
- `新規登録` (→ `/register`)
- `テストプレイ` (→ ホーム内 onclick で testplay 処理実行)

その他のメニュー項目（ダッシュボード、マップ、クエスト等）は **絶対に表示しない**

### 認証済時（全ページで表示）
**必ず以下だけ表示する：**
- `ログアウト` (→ サインアウト API 呼び出し)

ゲームメニュー（マップ、クエスト等）はヘッダーに表示しない。街やフィールド内の中央コンテンツに配置する。

### 未認証時（ホーム以外のページ）
- `ログイン` (→ `/login`)
- `新規登録` (→ `/register`)

## テストプレイ機能

### 動作フロー
1. ホーム画面で「テストプレイ」ボタンをクリック
2. バックエンド側でテストアカウント `testplayer` を自動作成（既に存在する場合はスキップ）
3. `/signin` エンドポイントを Fetch で呼び出し、テストアカウントでサインイン
4. サーバから認証クッキーが返却される
5. `/status` へ強制リロード（`Navigation.NavigateTo("/status", true)`）して遷移し、認証状態を反映

### 実装場所
- **UI トリガー**: `Components/Pages/Home.razor` 内の TestPlay() メソッド
- **ログイン API**: `Program.cs` 内の `/signin` エンドポイント
- **クライアント関数**: `wwwroot/js/auth.js` の `fetchSignIn(url, payload)` 関数

## ヘッダー実装詳細（MainLayout.razor）

### 実装方法
- `<AuthorizeView Context="auth">` を使用して、認証状態を動的に取得
- `auth.User.Identity?.IsAuthenticated` と `IsHomePage()` メソッドを組み合わせ

### コード例
```razor
<AuthorizeView Context="auth">
    @if (auth.User.Identity?.IsAuthenticated ?? false)
    {
        <!-- ログイン済：ログアウトのみ -->
        <NavLink href="#" class="nav-link" @onclick="SignOut">ログアウト</NavLink>
    }
    else if (IsHomePage())
    {
        <!-- ホーム + 未認証：ログイン / 新規登録 / テストプレイ -->
        <NavLink href="/login">ログイン</NavLink>
        <NavLink href="/register">新規登録</NavLink>
        <NavLink href="/">テストプレイ</NavLink>
    }
    else
    {
        <!-- その他ページ + 未認証：ログイン / 新規登録 -->
        <NavLink href="/login">ログイン</NavLink>
        <NavLink href="/register">新規登録</NavLink>
    }
</AuthorizeView>
```

## 画面構成

### チャンピオン情報表示
- 現在の Sky Arena チャンピオン（ユーザー名、レベル、CPU フラグ）を表示
- `IsChampion = true` のユーザーを `UserService.GetAllUsers()` から取得

### ホーム内容エリア
- ゲームの概要説明
- テストプレイボタン
- クエリパラメータ（任意）: `?gil=N&oldcoin=N&premium=N&job=J` で初期統計を表示

## 技術仕様

- **フレームワーク**: Blazor Server (InteractiveServer rendermode)
- **認証**: ASP.NET Core Cookie Authentication
- **通信**: HTTP/HTTPS、SignalR（オプション）
- **クライアント関数**: `fetchSignIn(url, payload)` (wwwroot/js/auth.js)

## テスト方法

1. **未ログイン状態でホームを開く**
   - URL: `https://localhost:7072/`
   - ヘッダーに「ログイン」「新規登録」「テストプレイ」が表示されることを確認

2. **テストプレイをクリック**
   - テストアカウントでサインイン
   - `/status` にリダイレクト、認証が確認される

3. **ログイン後**
   - ヘッダーに「ログアウト」**だけ**が表示されることを確認
   - ゲームメニューは表示されない（中央コンテンツに配置する）

4. **ログアウト**
   - ホームに戻り、ヘッダーが「ログイン」「新規登録」「テストプレイ」に戻ることを確認
