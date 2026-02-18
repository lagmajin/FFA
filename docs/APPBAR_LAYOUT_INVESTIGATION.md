# AppBar レイアウト不具合 調査レポート

## 概要

MudBlazor の `<MudAppBar>` 内で「タイトルを左側、ボタンを右端」に配置する変更が何度適用しても反映されなかった問題の調査と対策をまとめます。

---

## 根本原因（全3件）

### 原因 1: MudBlazor CSS の読み込み位置が不正

`MudBlazor.min.css` が `App.razor` の `<head>` ではなく `MainLayout.razor` の `<MudMainContent>` 内で読み込まれていた。ブラウザは HTML を上から順に解析するため、`<MudAppBar>` レンダリング時点で CSS が未読み込み → `<MudSpacer />` の `flex-grow: 1` が効かず全要素が左詰め。

**修正:** `App.razor` の `<head>` に移動。

### 原因 2: ユニバーサルリセット `*` が MudBlazor の内部スタイルを破壊

`FFA.styles.css` に以下のルールが存在していた：

```css
* {
    box-sizing: border-box;
    margin: 0;
    padding: 0;
}
```

MudBlazor の `<MudAppBar>` 内部のツールバー（`.mud-toolbar`）は `padding` と `margin` に依存する Flexbox レイアウトを使用している。`*` セレクタの `margin: 0; padding: 0` がこれらを上書きし、Flexbox アイテム間のスペーシングを崩壊させていた。

**修正:** `box-sizing: border-box` のみ残し、`margin: 0; padding: 0` を削除。

### 原因 3: `header` CSS セレクタが MudAppBar の `<header>` 要素に干渉

`FFA.styles.css` に無修飾の `header` セレクタが定義されていた：

```css
header {
    background-color: #1a202c;
    padding: 1rem 2rem;
    display: flex;
    align-items: center;
    ...
}
```

MudBlazor の `<MudAppBar>` は HTML の `<header>` 要素としてレンダリングされる。このカスタム CSS が MudBlazor の内部スタイルを上書きし、`padding: 1rem 2rem` などが強制適用されていた。

**修正:** `header` → `header.site-header` にスコープ限定。

---

## 最終的な対策：インラインスタイルによる防御的レイアウト

MudBlazor コンポーネント（`<MudSpacer />`）に依存するだけでなく、プレーンな `<div>` にインラインスタイルで `margin-left: auto` を指定する防御的アプローチを採用：

```razor
<MudAppBar Color="Color.Primary" Fixed="true" Style="display:flex; flex-wrap:nowrap;">
    <MudIconButton Icon="@Icons.Material.Filled.Menu" ... />
    <MudText Typo="Typo.h6" Style="white-space:nowrap;">FFA World</MudText>
    <MudSpacer />
    <div style="display:flex; align-items:center; gap:4px; flex-shrink:0; margin-left:auto;">
        <!-- 右寄せボタン群 -->
    </div>
</MudAppBar>
```

**なぜこのアプローチが堅牢か：**

| 手法 | 動作条件 | 耐障害性 |
|------|---------|---------|
| `<MudSpacer />` のみ | MudBlazor CSS が正常に読み込まれている | ❌ CSS 未読込で無効 |
| `margin-left: auto` (inline) | Flexbox コンテナであること | ✅ 外部CSS不要 |
| 両方併用 | どちらかが有効なら動作 | ✅✅ 二重保証 |

`margin-left: auto` は CSS Flexbox の仕様で定義されたプロパティで、インラインスタイルとして指定すれば外部スタイルシートに一切依存しない。

---

## 症状

- `<MudSpacer />` を AppBar に配置しても、タイトルとボタンが分離されない
- ボタンが右端に寄らず、すべて左寄せのまま表示される
- Razor コードの構造自体は正しいのに、ブラウザ上ではスタイルが適用されていない

## 根本原因

**MudBlazor の CSS (`MudBlazor.min.css`) が `<head>` ではなく、`<MudMainContent>` の内部に配置されていた。**

### 問題のコード（修正前）

```razor
<!-- Components/Layout/MainLayout.razor -->
<MudLayout>
    <MudAppBar Color="Color.Primary" Fixed="true">
        <!-- ここで MudSpacer が flex-grow:1 を期待するが、CSS が未読み込み -->
        <MudText>FFA World</MudText>
        <MudSpacer />
        <MudStack Row>...</MudStack>
    </MudAppBar>

    <MudMainContent>
        <!-- ⚠️ CSS がここで初めて読み込まれる -->
        <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
        <script src="_content/MudBlazor/MudBlazor.min.js"></script>
        ...
    </MudMainContent>
</MudLayout>
```

### なぜこれが問題なのか

1. **レンダリング順序**: ブラウザは HTML を上から順に解析する。`<MudAppBar>` は `<MudMainContent>` の**前**にレンダリングされる。
2. **CSS 未適用**: AppBar がレンダリングされる時点では `MudBlazor.min.css` がまだ読み込まれていない。
3. **MudSpacer の仕組み**: `<MudSpacer />` は CSS で `flex-grow: 1` を設定することで、Flexbox コンテナ内の余白を占有する。CSS がなければ、この要素はサイズゼロのまま。
4. **結果**: タイトルもボタンも全て左寄せで表示される。

### App.razor に CSS が無かった理由

`App.razor` がルート HTML ドキュメント（`<html>`, `<head>`, `<body>` を含む）であり、本来はここの `<head>` に CSS を配置すべきだった。しかし、おそらく MudBlazor の導入時に `MainLayout.razor` 内に CSS/JS を配置してしまい、そのまま見過ごされていた。

---

## 修正内容

### 1. App.razor の `<head>` に MudBlazor CSS を追加

```razor
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <ResourcePreloader />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />  <!-- ✅ 追加 -->
    <link rel="stylesheet" href="@Assets["app.css"]" />
    <link rel="stylesheet" href="/css/FFA.styles.css" />
    <link rel="stylesheet" href="/css/timeweather.css" />
    <ImportMap />
    <HeadOutlet />
</head>
```

### 2. App.razor の `<body>` に MudBlazor JS を追加（blazor.server.js の前）

```razor
<body>
    ...
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>  <!-- ✅ 追加 -->
    <script src="/_framework/blazor.server.js"></script>
    ...
</body>
```

### 3. MainLayout.razor から CSS/JS の読み込みを削除

`<MudMainContent>` 内にあった以下の行を削除：
```razor
<link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
<script src="_content/MudBlazor/MudBlazor.min.js"></script>
```

---

## 今後のガイドライン

### ルール 1: CSS は必ず `<head>` に配置する

外部ライブラリの CSS は `App.razor` の `<head>` セクションに配置すること。コンポーネント内部やレイアウト内に `<link>` タグを配置すると、レンダリング順序の問題が発生する。

### ルール 2: JS は `<body>` 末尾に配置する

JavaScript ファイルは `App.razor` の `<body>` 末尾、`blazor.server.js` の直前に配置すること。

### ルール 3: 静的アセットの配置場所一覧

| ファイル | 配置場所 |
|---------|---------|
| `MudBlazor.min.css` | `App.razor` → `<head>` |
| `MudBlazor.min.js` | `App.razor` → `<body>` (blazor.server.js の前) |
| `app.css` | `App.razor` → `<head>` |
| `FFA.styles.css` | `App.razor` → `<head>` |
| `timeweather.css` | `App.razor` → `<head>` |
| `auth.js` | `MainLayout.razor` 内でも可（遅延読み込み可） |
| `signalr.min.js` | `MainLayout.razor` 内でも可（遅延読み込み可） |

### ルール 4: MudBlazor コンポーネントの CSS 読み込み順序

```
MudBlazor.min.css → app.css → FFA.styles.css
```

MudBlazor のデフォルトスタイルを先に読み込み、アプリ固有のスタイルで上書きできるようにする。

---

## 関連する MudBlazor レイアウトパターン

AppBar でタイトル左・ボタン右のレイアウトを実現する正しいパターン：

```razor
<MudAppBar>
    <MudIconButton Icon="@Icons.Material.Filled.Menu" ... />
    <MudText Typo="Typo.h6">タイトル</MudText>
    <MudSpacer />  <!-- ← flex-grow:1 で余白を占有 -->
    <MudStack Row>
        <!-- 右寄せしたい要素 -->
    </MudStack>
</MudAppBar>
```

`<MudSpacer />` は Flexbox の `flex-grow: 1` を使ってスペースを埋める。**MudBlazor の CSS が読み込まれていなければ機能しない。**

---

## 日付

2025年 — FFA プロジェクト MudBlazor 移行中に発生・解決
