# 実装済み機能リスト

## 防具システム
- [Models/Armor.cs](Models/Armor.cs)
- [Services/ArmorService.cs](Services/ArmorService.cs)

## プレイヤースロット
- [Models/User.cs](Models/User.cs)

## マップシステム
- [Models/Map.cs](Models/Map.cs)

## 転生・マスターシステム
- [Models/User.cs](Models/User.cs)
- [Services/UserService.cs](Services/UserService.cs)

## 実績システム
- [Models/Achievement.cs](Models/Achievement.cs)
- [Services/AchievementService.cs](Services/AchievementService.cs)

## 採掘スキル
- [Models/MiningSkill.cs](Models/MiningSkill.cs)
- [Services/MiningService.cs](Services/MiningService.cs)

## 職業システム
- [Models/Job.cs](Models/Job.cs) - 基本職10、上級職6
- [Models/JobHistory.cs](Models/JobHistory.cs)

## 通信販売システム
- [Models/MailOrder.cs](Models/MailOrder.cs)
- [Services/MailOrderService.cs](Services/MailOrderService.cs)

## ランダムイベント
- [Models/RandomEvent.cs](Models/RandomEvent.cs)
- [Services/RandomEventService.cs](Services/RandomEventService.cs)

## ブラックマーケット
- [Models/BlackMarket.cs](Models/BlackMarket.cs)
- [Services/BlackMarketService.cs](Services/BlackMarketService.cs)

## チャットシステム
- [Models/ChatMessage.cs](Models/ChatMessage.cs)
- [Services/ChatService.cs](Services/ChatService.cs)
- [Components/Pages/Chat.razor](Components/Pages/Chat.razor)
- [wwwroot/css/chat.css](wwwroot/css/chat.css)

## ギルド拡張システム
- [Models/Guild.cs](Models/Guild.cs) - スキル習得機能追加
- [Services/GuildEnhancementService.cs](Services/GuildEnhancementService.cs) - 新規作成
  - ギルドレベルシステム
  - ギルドスキルシステム
  - ギルド戦機能

## ワールドマップシステム（2Dグリッド）
- [Models/WorldGrid.cs](Models/WorldGrid.cs) - 新規作成
  - グリッドベースの場所管理
  - プレイヤ位置追跡
- [Services/WorldService.cs](Services/WorldService.cs) - 新規作成
  - 10x10の2Dワールド
  - 移動システム（北/南/東/西）
  - 敵遭遇システム
  - 街、平原、森、湖、砂漠、雪山、火山、ダンジョン配置

## スタミナシステム
- [Models/Stamina.cs](Models/Stamina.cs) - 新規作成
  - 7種類のスタミナタイプ（移動、戦闘、採掘、釣り、製作、ギルド戦、探索）
- [Services/StaminaService.cs](Services/StaminaService.cs) - 新規作成
  - 自動回復システム
  - 日次リセット機能
  - スタミナ使用/回復API

## 岩割りミニゲーム
- [Services/RockSmashService.cs](Services/RockSmashService.cs) - 新規作成
  - 1ターンで全火力を叩き込み、閾値以上なら破壊して報酬を付与
  - 成功時は希少宝石や岩の欠片をインベントリへ付与、ギル報酬の可能性あり
- [Components/Pages/RockSmash.razor](Components/Pages/RockSmash.razor) - 新規作成
  - UI: ワンボタンで判定し結果表示（ログイン必須）

## フレンドシステム（基盤のみ）
- [Models/Friend.cs](Models/Friend.cs) - 新規作成
  - フレンド状態（申請中、承認済み、拒否、ブロック）
  - フレンド情報、申請情報モデル
- [Services/FriendService.cs](Services/FriendService.cs) - 新規作成
  - フレンド申請/承認/拒否/削除
  - ブロック/ブロック解除
  - オンライン状態判定

## メールシステム（基盤のみ）
- [Models/Mail.cs](Models/Mail.cs) - 新規作成
  - メールタイプ（プレイヤー間、システム、イベント、ギルド）
  - 添付アイテム、ゴールド添付
- [Services/MailService.cs](Services/MailService.cs) - 新規作成
  - プレイヤー間メール送信
  - システムメール、イベント報酬メール
  - 添付品受取、メール削除

## 釣りシステム（基盤のみ）
- [Models/Fish.cs](Models/Fish.cs) - 新規作成
  - 魚情報、釣りスポット、釣り竿、餌モデル
  - 釣り場所、時間帯、天候条件
- [Services/FishingService.cs](Services/FishingService.cs) - 新規作成
  - 釣り実行、魚選択（レアリティ重み付け）
  - 重量計算、価格計算
  - 初期魚データ、釣り竿、餌データ投入

## クラフトシステム（基盤のみ）
- [Models/CraftRecipe.cs](Models/CraftRecipe.cs) - 新規作成
  - レシピ、素材、クラフト結果モデル
  - クラフトカテゴリ（武器、防具、装飾品、消耗品、料理）
- [Services/CraftService.cs](Services/CraftService.cs) - 新規作成
  - クラフト実行、成功率判定
  - 素材消費、アイテム生成
  - 初期レシピデータ投入

---

## 今後の実装候補
- [ ] フレンドシステムUI（Friend.razor）
- [ ] メールシステムUI（Mail.razor）
- [ ] 釣りシステムUI（Fishing.razor）
- [ ] クラフトシステムUI（Craft.razor）
- [ ] ペット/コンパニオンシステム
- [ ] 取引所UI改善
- [ ] その他新規機能
