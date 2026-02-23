# 武器アセットディレクトリ

このディレクトリには武器の画像アイコンを配置します。

## ディレクトリ構造

```
wwwroot/images/weapons/
├── sword/    # 剣（Sword）用のアイコン
├── axe/      # 斧（Axe）用のアイコン
├── spear/    # 槍（Spear）用のアイコン
└── katana/   # 刀（Katana）用のアイコン
```

## ファイルの命名規則

武器アイコンのファイル名は以下のように命名してください：

### 基本命名規則
`{武器タイプ}_{武器ID}.png`

例：
- `sword_001.png` - 練習用木剣
- `sword_002.png` - 鉄の剣
- `axe_010.png` - 手斧

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
