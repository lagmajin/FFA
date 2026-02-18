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

## チャット国別機能
- [Services/ChatService.cs](Services/ChatService.cs) - 国別チャットキューを追加
- [Components/Pages/Chat.razor](Components/Pages/Chat.razor) - 国別チャットタブを追加

## オークション / 国戦（未統合UI）
- [Services/AuctionService.cs](Services/AuctionService.cs) - 新規作成（オークション基盤）
- [Services/CountryWarService.cs](Services/CountryWarService.cs) - 新規作成（国戦基盤）

## 新規スキャフォールディング
- [Models/Companion.cs](Models/Companion.cs) - コンパニオン（ペット）モデル
- [Services/CompanionService.cs](Services/CompanionService.cs) - コンパニオン管理サービス
- [Models/InstanceDungeon.cs](Models/InstanceDungeon.cs) - インスタンスダンジョンモデル
- [Services/InstanceService.cs](Services/InstanceService.cs) - インスタンス管理サービス
 - [Models/MonsterTemplate.cs](Models/MonsterTemplate.cs) - モンスターテンプレート
 - [Services/MonsterService.cs](Services/MonsterService.cs) - モンスター生成・テンプレ管理サービス
 - [Models/NotoriousMonster.cs](Models/NotoriousMonster.cs) - NM (Notorious Monster) モデル
 - [Services/NotoriousMonsterService.cs](Services/NotoriousMonsterService.cs) - NM 管理サービス（スポーン・討伐・リスポーン管理）

***

(注) 新しいサービスは実装済みですが、フロントエンド（UI）には未統合です。UI 統合の実装は別途指示してください。
