# ダメージ計算式

実装済みのダメージ計算式は以下の通りです。

ダメージの最終計算:

```
Damage = ATK × (1 - tanh(DEF / K))
```

パラメータ:
- `ATK` (攻撃力): ダメージの基礎値。例: 敵の攻撃値やプレイヤーの攻撃力。
- `DEF` (防御力): 対象の防御力。防御装備やバフ等を合算して使用します。
- `K` (スケーリング定数): 防御の影響度を調整する定数。デフォルト実装値は `500`。

設計上の注意:
- `K` が大きいほど `DEF` による減衰効果は緩やかになります。
- `DEF = K` のとき、tanH(1) ≈ 0.76159 のため、ダメージは約 24% 減衰します（1 - tanh(1) ≈ 0.2384）。
- 出力は小数切り捨てで整数化し、最低 1 ダメージを保証します。

実装箇所:
- `Services/DungeonService.cs` の `ApplyDefenseScaling(int atk, int def, double k = 500.0)` を使用。

使用例:

- プレイヤーの攻撃に防御を適用する場合:
  - `rawAtk = CalculatePlayerDamage(user)` で基礎攻撃力を算出
  - `finalDamage = ApplyDefenseScaling(rawAtk, enemy.Defense)` で最終ダメージを取得

- 敵の攻撃に対してプレイヤー防御を適用する場合:
  - `baseAtk = enemy.Attack + random` などの基礎値を用意
  - `finalDamage = ApplyDefenseScaling(baseAtk, player.EquippedArmor?.Defense ?? 0)`

このドキュメントは実装の簡易仕様です。バランス調整のため `K` や防御値の付与方法は調整してください。
