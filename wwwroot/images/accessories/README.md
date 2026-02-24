# 装飾品アセットディレクトリ

このディレクトリには装飾品（アクセサリー）の画像アイコンを配置します。

## ディレクトリ構造

```
wwwroot/images/accessories/
├── ring/        # リング（Ring）用のアイコン
├── amulet/      # アミュレット（Amulet）用のアイコン
├── earring/    # イアリング（Earring）用のアイコン
├── bracelet/    # ブレスレット（Bracelet）用のアイコン
├── necklace/    # ネックレス（Necklace）用のアイコン
└── belt/        #  Belt）用のアイコン
```

## ファイルの命名規則

装飾品アイコンのファイル名は以下のように命名してください：

### 基本命名規則
`{装飾品タイプ}_{ID}.png`

例：
- `ring_001.png` - 銅のリング
- `amulet_010.png` - 護りのアミュレット
- `earring_020.png` - 知識のイアリング
- `bracelet_030.png` - 体力のブレスレット
- `necklace_040.png` - 銅のネックレス
- `belt_050.png` - 革の Belt

### レアリティ別 接頭辞（オプション）
稀有以上の装飾品には rarity_ を接頭辞ことができます：
- `red_ring_200.png` - 龍眼のリング（Red rarity）
- `orange_amulet_300.png` - 神的印章（Orange rarity）
- `gold_ring_400.png` - 星天使のリング（Gold rarity）
- `rainbow_amulet_500.png` - 宇宙の統率者（Rainbow rarity）

## 推奨画像サイズ

- 標準: 64x64 px
- 大: 128x128 px
- アイコンリスト用: 32x32 px

## 対応画像形式

- PNG（推奨）
- SVG（ベクター形式）
- WebP

## 装飾品ID参照

装飾品のIDと名前の対応は以下ファイルを参照：
`Data/Items/accessories.toml`

## 装飾品タイプ一覧

| タイプ | 日本語名 | ID範囲（例） |
|--------|----------|--------------|
| Ring | リング | 1-702 |
| Amulet | アミュレット | 10-712 |
| Earring | イアリング | 20-722 |
| Bracelet | ブレスレット | 30-732 |
| Necklace | ネックレス | 40-742 |
| Belt |  | 50-752 |

## レアリティ一覧

| レアリティ | 日本語名 | 色 |
|------------|----------|-------|
| White | コモン | 白 |
| Green | 稀少 | 緑 |
| Blue | 高級 | 青 |
| Purple | 高級 | 紫 |
| Red | 稀有 | 赤 |
| Orange | 伝説 | 橙 |
| Gold | 神話 | 金 |
| Rainbow | 最上位 | 虹色 |
