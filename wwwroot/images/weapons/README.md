# 武器アセットディレクトリ

このディレクトリには武器の画像アイコンを配置します。

## ディレクトリ構造

```
wwwroot/images/weapons/
├── sword/    # 剣（Sword）用のアイコン
├── axe/      # 斧（Axe）用のアイコン
├── spear/    # 槍（Spear）用のアイコン
├── katana/   # 刀（Katana）用のアイコン
├── bow/      # 弓（Bow）用のアイコン
├── dagger/   # 短剣（Dagger）用のアイコン
├── staff/     # 杖（Staff）用のアイコン
├── hammer/    # 槌（Hammer）用のアイコン
└── fist/     # 拳（Fist）用のアイコン
└── book/     # 本（Book）用のアイコン
└── orb/      # オーブ（Orb）用のアイコン
```

## ファイルの命名規則

武器アイコンのファイル名は以下のように命名してください：

### 基本命名規則
`{武器タイプ}_{武器ID}.png`

例：
- `sword_001.png` - 練習用木剣
- `axe_010.png` - 手斧
- `spear_020.png` - 竹枪
- `katana_030.png` - 竹刀
- `bow_040.png` - 木弓
- `dagger_050.png` - ナイフ
- `staff_060.png` - 木杖
- `hammer_070.png` - 木槌

### レアリティ別 접두어（オプション）
稀有以上の武器には rarity_ を 접두すことができます：
- `red_sword_200.png` - 龙泉剑（Red rarity）
- `orange_axe_301.png` - 天狼牙（Orange rarity）
- `gold_sword_400.png` - 神剣トライデント（Gold rarity）
- `rainbow_sword_500.png` - 創世の剣（Rainbow rarity）

## 推奨画像サイズ

- 標準: 64x64 px
- 大: 128x128 px
- アイコンリスト用: 32x32 px

## 対応画像形式

- PNG（推奨）
- SVG（ベクター形式）
- WebP

## 武器ID参照

武器のIDと名前の対応は以下ファイルを参照：
`Data/Items/weapons.toml`

## 武器タイプ一覧

| タイプ | 日本語名 | ID範囲（例） |
|--------|----------|--------------|
| Sword | 剣 | 1-500 |
| Axe | 斧 | 10-501 |
| Spear | 槍 | 20-502 |
| Katana | 刀 | 30-503 |
| Bow | 弓 | 40-504 |
| Dagger | 短剣 | 50-505 |
| Staff | 杖 | 60-506 |
| Hammer | 槌 | 70-507 |
| Fist | 拳 | 80-780 |
| Book | 本 | 90-790 |
| Orb | オーブ | 85-785 |

## レアリティ一覧

| レアリティ | 日本語名 |  색상 |
|------------|----------|-------|
| White | コモン | 白 |
| Green | 稀少 | 緑 |
| Blue | 高級 | 青 |
| Purple | 高級 | 紫 |
| Red | 稀有 | 赤 |
| Orange | 伝説 | 橙 |
| Gold | 神話 | 金 |
| Rainbow | 最上位 | 虹色 |
