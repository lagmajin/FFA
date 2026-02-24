# 防具アセットディレクトリ

このディレクトリには防具の画像アイコンを配置します。

## ディレクトリ構造

```
wwwroot/images/armors/
├── light/      # 軽装（Light）用のアイコン
├── medium/     # 中装（Medium）用のアイコン
├── heavy/      # 重装（Heavy）用のアイコン
├── robe/       # ローブ（Robe）用のアイコン
├── helmet/     # 兜（Helmet）用のアイコン
├── shield/     # 盾（Shield）用のアイコン
└── boots/      # 靴（Boots）用のアイコン
```

## ファイルの命名規則

防具アイコンのファイル名は以下のように命名してください：

### 基本命名規則
`{防具タイプ}_{防具ID}.png`

例：
- `light_001.png` - 革の背心
- `heavy_020.png` - 鉄の铠
- `robe_030.png` - 法衣
- `helmet_040.png` - 革帽子
- `shield_050.png` - 木盾
- `boots_060.png` - 革靴

### レアリティ別 접두어（オプション）
稀有以上の防具には rarity_ を 접두すことができます：
- `red_heavy_200.png` - 龍鱗の铠（Red rarity）
- `orange_shield_300.png` - イージスの盾（Orange rarity）
- `gold_heavy_400.png` - 神々の鎧（Gold rarity）
- `rainbow_heavy_500.png` - 宇宙の鎧（Rainbow rarity）

## 推奨画像サイズ

- 標準: 64x64 px
- 大: 128x128 px
- アイコンリスト用: 32x32 px

## 対応画像形式

- PNG（推奨）
- SVG（ベクター形式）
- WebP

## 防具ID参照

防具のIDと名前の対応は以下ファイルを参照：
`Data/Items/armors.toml`

## 防具タイプ一覧

| タイプ | 日本語名 | ID範囲（例） |
|--------|----------|--------------|
| Light | 軽装 | 1-702 |
| Medium | 中装 | 10-712 |
| Heavy | 重装 | 20-722 |
| Robe | ローブ | 30-732 |
| Helmet | 兜 | 40-742 |
| Shield | 盾 | 50-752 |
| Boots | 靴 | 60-762 |

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
