# ART3E再開時の引き継ぎ（2026-10-13）

電源OFF前の停止後「再開して」で中断解除。中断時に実行中の生成/検証はなかった。保存済み証跡を確認して本書を更新し、Unityは再実行していない。Codex/Claudeが交代可能。

## Gitと資料

main HEAD/origin main 182e970、ART3D実装52de881と報告はpush済み。今回は未commit/未push。git statusのゲーム用PNG/JSON、AttackData/CharacterData/シーン/AnimationSet、Battle/Editor/Dataコード、3新テストと制作ツール/素材/指示書は未完成の差分で破棄しない。

tasklist/CIMでは同じ親のCodex app-server/exec-server2プロセス、同フォルダの別編集者/Claude/実Unity Editor/テスト実行なし。他プロジェクトのUnity MCPは触らない。開始時に再確認する。読む順はAGENTS→HANDOFF→RESUME→STATUS→ART3_WALK_ATTACKS。後続ART3F_RUN_BRAKE/ART3G_DAMAGE_RECOILは作成済み。

## 現在の素材と最終要求

原画79PNG/79全文プロンプト、generation-log/source-inventory/source-selection/pending-reviewとnormalizedをart_source/art3_walk_attacks/に保存。採用14件（歩行中割り含む9+通常5）。未採用nullを自動採用しない。

| 対象 | 採用版 | 状態 |
|---|---|---|
| 透真walk / 中割り / dash | v5 / v6 / v3 | 接続/素材検査済み |
| 蓮walk / 中割り / dash | v7 / v5 / v7 | 接続/素材検査済み |
| 千鶴walk / 中割り / dash | v4 / v3 / v2 | 接続/素材検査済み |
| 透真attack1 / 2 / 3 | v9 / v8 / v3 | 正規化合格、未接続 |
| 蓮attack1 | v13 | 正規化合格、未接続 |
| 千鶴attack1 | v3 | 正規化合格、未接続 |
| 蓮attack2 / 3 | null | 未生成 |
| 千鶴attack2 | null | v1不採用、再制作 |
| 千鶴attack3 / bag_throw | null / 未作成 | 未生成・未実装 |

- 歩き各8枚/15fpsループ、脚と腕逆位相、透真左端末の肩接続修正済み。走り4原画を1,1,2,2,3,3,4,4にする8F/15fpsループ、旧ActiveFrame1保持。6クリップ48Fの画素/頭/接地/浮遊/GIF/32色、旧動作/待機/しゃがみと対象外91PNGのSHA一致済み。
- 通常は4原画（立ち0/予備1/打撃2/戻り3）から6F（1,1,2,3,3,3）、15fps非ループ、ActiveFrame2、State6/7/8予定。ゲームは旧通常絵で新素材の全体検査は未完了。
- 全224×160/PPU50/pivot(0.5,0)/足元159/胴基準112/共通32色/二値alpha。立ち実高124/100/112、頭幅37/58/43・帯深さ25/31/34±2、靴の寸法も照合。立ち0由来の共通倍率を全姿勢へ適用、ポーズごとに拡縮しない。
- 透真最終：右パンチ→左パンチ→右キック。腰/肩/重心を動かし通常中は端末をしまう。
- 蓮最終：跳び込み頭突き→頭支持カポエイラ→両腕で床を押す前方両足蹴り。旧PPK/左右頭支持蹴り2回案は上書き。頭頂接地/2腕2脚/各段のつながりを確認。
- 千鶴最終：同じ右手で小カバンを右側から一撃→体をひねって二撃→そのカバンを投げる。カバン本体約20×18px/短い持ち手/小バックル、攻撃時だけ出し放出後は手元空。投げは実飛び道具を1回生成、通常分類/多重ヒット防止/両向き対応。
- 最新手描きはカバン円弧/腰肩回転/荷重/逆さ脚軌道に役立つ。docs/art_reference/_motion_local/art3e_bag_capoeira_user_sketch.jpgとart3g_pain_reference.pngをローカル保存。Git管理外、公開Assets/ZIPへ入れない。手描きの腕支持は脚軌道の参考で、確定した頭頂接地は上書きしない。×の立ち姿へ急に戻さず次段へつなぐ。

## 原画の検査と再制作の注意

- 蓮attack1 v13はscale0.2173913043、頭60/57/58/58、実高100/90/72/100、ピーク床上30px。キー2のみ頭上端58（跳び込み基準高さ102）/浮遊4〜64px、キー3は頭支持。attack2キー1〜3/attack3キー1も床側31px頭幅58±2・クリーム頭頂が床4px接触・腰脚が上。attack3ピークは両腕支持/上側31pxの頭、戻り足接地。走りの浮遊4〜32pxは保持。頭ROI/独立縮尺/アサーション緩和なし。
- 蓮v2〜v12は頭/マント/袖の侵入などで不採用、全原画保持。次段はv13とnormalized/ren/attack1_key_00.pngを参照し58幅/靴倍率維持。手/マントを床側頭帯へ入れない。
- 千鶴attack1 v3はscale0.18241042345、頭45/45/45/45。正面胴の右肩は画面左、背中胴の右肩は画面右。右肩→肘→手首の連続を見る。v2の頭4%縮小指示は38pxまで細くなり不採用。
- 千鶴attack2 v1は頭45/46/46/45で2キー超過、ピークがattack1に似る。手描きの円弧/踏み込みと、右腕が胸前を横切る二撃を明確化。立ち頭を全キーへ固定し独立縮小しない。

## 戦闘処理と実検証

NormalForwardDistance0.18（9px相当）を発生+持続に均等配布、3段0.54。向き/バフ発生/空中速度加算/ヒット停止/被弾/着地硬直中断/壁/足場端/CPU連係予測を実装。通常全種へ適用、特殊/超必殺に前進を追加しない。

DamageRecoilDistance0.18/DamageRecoilFrames6を実攻撃へ設定し既存Knockbackへ追加。入射から離れ、停止中は残tick保持/再被弾は置換/ガード回避なし/壁遮断分の補完なし/KO練習リセットで残留なし/カウンターにも反動。LastDamageLostHeartは実ハート数差から設定。画像と表示選択は未実装、ダメージ原画は全攻撃完成後。通常軽傷は上体が後ろに倒れかけ耐える、実ハート消失は痛い表情/大きい体反応、既存吹き飛び/KOを保持。

CharacterDataのNormalMotionSchemaVersion1/DamageReactionSchemaVersion1で未設定の新フィールドだけ一度移行、調整値保持。実BuildAll後のSOに0.18/0.18/6を確認。固定位置制御用BattleTestFactory.StationaryAttackは新移動/反動も明示0にし従来の場外優先検査を隔離、旧アサーション維持。実既定値は新28+21ケースで検査。新ガードテストは被弾までGuardを毎tick保持する条件へ修正済み。

| 検証 | 保存済み実結果 | 証跡 |
|---|---|---|
| 実Unity通常前進EditModeのみ | 28/28、失敗/スキップ0 | TestResults/normaladvance_EditMode.xml |
| 実Unity被弾後退EditModeのみ | 21/21、失敗/スキップ0 | TestResults/damagerecoil_EditMode.xml |
| 実Unity BuildAll単独 | exit0、SO/シーン生成 | TestResults/movementreactions_buildall.log |
| 純C# | 366/366、失敗/スキップ0 | TestResults/art3g-dotnet-battle.xml |
| Unity API参照コンパイル | 制作中確認は警告0/エラー0 | 制作中のTestResultsログ |
| ART3E全EditMode/PlayMode/Windows | 未実行、全4工程未合格 | 今回の完了とは扱わない |
| 前回ART3D | Edit357/357・Play30/30、BuildAll/Windows成功 | art3d_final.md/art3d_* |

実機スクショ未撮影。docs/art_wip/art3e_*は素材ボード/GIF、実画面ではない。最新exe/ZIPは前回ART3D。

## 次の担当の手順

1. 同時編集なしを確認し資料/source-selection/pending-reviewを読む。原画と差分を保持。
2. imagegenスキル/内蔵image_genで蓮2/3段・千鶴2/3段・投げカバンを1枚ずつ生成/目視、version付き原画/全文条件を保存。編集対象を先にview_image。外部APIへ切り替えない。
3. Windows PowerShell5.1でInspectWalkAttackAtlas.ps1 -Character <名> -Group <名> -Version <n>。合格候補だけselectionへ入れNormalizeWalkAttackAtlas.ps1 -Character <名> -Group <名>。失敗ならselectionを元へ戻す（未採用null）。全キー合格後のみPNG保存を保持。会話の一時関数に依存しない。
4. 千鶴投げの速度/寿命/判定/放出tickを指示書へ具体追記してから実装。通常分類/一度生成/手元空/左右/多重ヒット/反動を検査。通常投げ用NormalProjectileClipと既存特殊rice_ballを分ける案は未実装。
5. BuildArt3WalkAttacks.ps1で通常全接続（今は4未採用で停止する）。15クリップ102Fに投げ効果を加え総数を実測。画素/頭/靴/右腕/各段のつながり/回帰91PNG/再生成一致、ガイド/ASSET_SOURCES/キット/READMEを更新。
6. 新NormalAdvanceTests/DamageRecoilTests/WalkNormalAnimationTestsと既存AnimationAsset/ProjectAsset/BattleSceneのアサーション維持。新通常の実素材/描画検査はまだ全実行していない。
7. tools/verify/run_unity.ps1 -Tag art3eをサンドボックス外承認、-nographicsなし、全4工程合格後に資料/最終報告→通常commit/origin mainへpush。force禁止、個人情報なし。
8. ART3F急停止/切り返し、ART3G強弱被弾の順で具体値/状態条件を指示書へ先に追記して実装。後退の純C#処理は既存なので再実装しない。区切りごとにUnity全4工程と記録/通常push。
9. 実機撮影は毎回「PCに触らないで」と案内してから実行、最前面安全検査を緩めない。最新2ビルドだけ保持。

Claude確認点：右肩から手の連続とカバン持ち替えなし、蓮の頭頂接地/2腕2脚/段の接続、等倍頭/靴、前進/後退の停止中断/壁/SO移行/実ハート消失、全4工程/公開差分の個人情報。
