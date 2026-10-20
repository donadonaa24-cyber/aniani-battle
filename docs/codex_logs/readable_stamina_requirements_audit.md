# 追加演出・攻撃スタミナの要求対照（2026-10-21）

基準main b12d006からの追加。指示書は `docs/codex_tasks/CHARGED_TELEPORT_AND_COOLDOWN_HUD.md`。原作正史の追加ではなく本作の技・演出として実装。

|要求|反映した内容|主な検証|
|---|---|---|
|湊・シロ上復帰|30〜60tickの溜め、離す/最大で発動、7.5〜12u、被弾中断、青い断片と白い縦閃光|ChargedAbilitiesTests、ReadableAbilitySceneTests|
|復帰を追える速さ|非転移の時間1.5倍/速度2/3、距離維持。転移の前後を長く表示|DirectionalRecoveryTestsの全9人左右の復帰|
|全特殊のCD|P1/P2のN/横共有・下・上・超必殺カード。残秒/着地/枚数/ゲージ/無制限/溜め/召喚段階|HUD状態の純C#、カードの画面内配置/文字高を実Unity確認|
|9人の溜め技|各回答の最短/最大時間、離す/最大/被弾中断。龍太バットも溜め、詠唱は1秒|ChargedAbilitiesTests|
|実際の範囲と効果|シロ最大24u、カレー初撃/残留、ロケット着弾爆発、ブラックホール本体/引力、蓮/結衣の幅、王の上下間隔。2ハート維持|純C#の端での命中、実画像の可視幅/高さと実判定の一致|
|暁ピラミッド|上足場を通過し主足場で着地、拡大と600tick壁|RosterRefinementTests、AkatsukiActionArtworkSceneTests|
|王の特殊の部下|既存レイラ/シオン/ノア、王と同程度の高さ。60tick登場→24tick号令→主足場を3秒横断する速度→端で24tick退場|RoyalMarchTests、RoyalMarchSceneTestsの左右/原画/号令/裂け目|
|次元裂け目原画|built-in image_gen新規生成、原本/全文条件/SHA/採用JSON保持|king_march/adoption.json、Native画像保持、Web梱包監査|
|大きい場外判定|3ステージ共通で左右±32、下−16。足場は不変|RoyalMarchTests、共通地形テスト|
|飛び道具のリスク|通常は最低6を含む総額、特殊18。発射時に1回、二丁/3人も1発動。最低0.2秒後隙|ProjectileEconomyAndPlacementTests、AttackStaminaTests|
|湊/暁の設置|しゃがみ絵、設置60tick+後隙18tick、移動不可/前被弾中断、各2個/CD300|設置の純C#と左右の実入力/しゃがみ原画|
|龍太3段目|新規の蹴り予備/決めの2原画。通常1/2維持、上の旧アッパーは分離して維持|RyutaActionArtworkTests、実キー3段コンボ、RGBA/SHA|
|龍太の通常エフェクト|追加オーバーレイを停止。他の攻撃/共通の命中フィードバックは維持|実入力で全通常段の非表示、特殊/超必殺の継続表示|
|通常最大3秒の飛び|足場端+10u目安、場外4u内側、上24/拘束36。通常蓄積+1、復帰入力可。最大時だけカメラの水平追跡を足場で止める|9人×左右、3画面比率、場外線と画面外の区別|
|通常のST管理|全9人の各段/方向/空中/通常溜めに費用、攻撃中回復0、硬直後の待ちと歩行回復、ST0の歩行/上復帰救済|全9人の単一消費・不足・回復、実HUDと射撃100→93/特殊100→82|
|強さを調べて調整|基準648→調整648→最終648。透真84.7→61.1%、王81.9→66.0%。全採用値はSTAMINA_BALANCE_20261021.md|同じseed/間合い/左右を再測定、最終も同値|

実Unity stamina_final_3：BuildAll成功、Edit773/773、Play96/96、Windows成功、スキップ0。証跡SHAは `readable_stamina_verification.json`。代表12画像は `docs/screenshots/readable_stamina/`、全1234画像は `TestResults/stamina_final_3/captures/`。

WebGL stamina_web_final：成功128,081,814 bytes/239.020秒、Windows設定への復帰成功。445/445PNGをリサイズせず透明余白で梱包、RGBA保持。監査は `readable_stamina_web_pixels.json`。ブラウザの実キーによる確認範囲は最終Web記録へ。全9人全技の実入力網羅は実Unityの結果と区別する。

通常45半減判定、ゲージ14/10、通常4原画/下・空中N各1、保護カバン、承認龍太被弾v5、全身1枚方式を維持。美術の最終承認と、対人での最適運用・実機パッド・別ブラウザ/低メモリ端末の評価は実装完了と区別する。
