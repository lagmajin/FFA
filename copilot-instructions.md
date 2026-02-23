# Copilot Instructions

## User Interaction Style
- Respond to users as an expert or consultant, providing knowledgeable and professional guidance.

## コーディングルール
- 特別な指示がない限り、C#ファイル（.cs）のみ編集する
- UIファイル（.razor, .css, .jsなど）は特別な指示がない限り編集禁止

## ビルド警告ゼロルール（重要）
- **全ての実装完了後、必ず `dotnet build` を実行し、警告が0になるまで修正すること**
- 警告が残っている状態での `attempt_completion` は禁止
- 警告の種類と対応方法:
  - **CS8602/CS8600**: null参照の可能性 → nullチェックを追加
  - **CS0414**: 未使用フィールド → 使用するか削除
  - **RZ10012**: 不明なコンポーネント → @usingディレクティブを追加
  - **MUD0001**: MudBlazorパラメータ警告 → 正しいパラメータ名に修正
  - **CS0162**: 到達不能コード → 不要なコードを削除
- 実装時は警告が出ないコードを最初から書くことを心がける
- 警告修正も実装の一部として扱う

## コーディングルール - API Layerについて

### 重要: 作成したAPIはまだ使用禁止
API Layerを расширитьため、以下のファイルを作成した。これらは**現時点では使用禁止**。

#### 作成したAPI関連ファイル
- `Models/ApiResponse.cs` - 統一レスポンスモデル
- `Services/ApiService.cs` - API抽象化サービス
- `Controllers/ApiController.cs` - APIコントローラー

#### APIエンドポイント（使用禁止）
```
GET  /api/game/time          - 時間・天候情報取得
POST /api/game/enemy/stats   - 敵ステータス計算（夜間buff込み）
GET  /api/game/enemies/night - 夜間専用敵リスト
GET  /api/map/all           - 全マップリスト
GET  /api/map/country/{id}  - 国別マップ取得
GET  /api/map/neutral       - 中立边境マップ
GET  /api/map/connection    - ゲート接続情報
```

#### 理由
- 現時点では直接サービス呼び出し（`@inject` DI）を使用
- 将来的にDBアクセスや外部APIへの切り替えに備えて基盤だけ作成
- **使用时机は今後の指示があるまで待つこと**

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

## ランタイムデバッグと Blazor 固有の注意点 (追記)

以下は開発中に頻出した問題と今後の維持管理で注意すべき点です。ソースを大きく変更する前にチームで合意を取ってください。

- Blazor Server の SignalR 回線エラー:
  - `ErrorBoundary` はコンポーネント内の例外を捕捉しますが、SignalR 回線（circuit）や transport レイヤのエラーは捕捉できない場合があります。
  - 開発時に回線エラーの詳細を表示するには、`Program.cs` で `AddInteractiveServerComponents(options => options.DetailedErrors = env.IsDevelopment())` を設定してください。**本番で有効にしないでください。**

- DI 登録の重複・ライフタイム競合:
  - 同じサービスを複数回登録したり、Scoped と Singleton を混在させると起動時やランタイムに例外が発生します。`Program.cs` のサービス登録は慎重に扱ってください。

- サービスの直接生成 (`new`) の使用:
  - `UserService` など一部で `new AbilityService()` や `new WorldGridService()` を直接呼び出す実装があります。将来のリファクタでは DI に移行してください。直接生成は設定やスコープをバイパスし、原因追跡を難しくします。

- 認証フローの注意点 (クッキー発行後の反映):
  - `/signin` を `fetch` で呼んでサーバーが Set-Cookie を返す場合、クライアント側では**フルページリロード**（`Navigation.NavigateTo(url, forceLoad: true)`）が必要です。SPA ナビゲーションだけだと SignalR 回線が古い認証状態のままで、画面に「ログインしてください」が残ることがあります。

- ログ出力と未観測例外:
  - Blazor 回線内の未観測タスク・AppDomain の例外は `TaskScheduler.UnobservedTaskException` / `AppDomain.UnhandledException` によりバックエンドでログに記録するようにしてください。

-- これらの注記は将来のデバッグ負荷を減らすための運用ルールです。ソースの振る舞いを変える変更（特に `DetailedErrors` やサービス登録周り）は必ずコードレビューを行ってください。
