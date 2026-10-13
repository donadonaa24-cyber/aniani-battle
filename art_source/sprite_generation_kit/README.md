# あにバト！！ピクセルアート再制作指示書

この指示書・参照PNG・prompts.json・spec.json・palettes.jsonをセットで画像生成サービスや別の開発担当へ渡す。生成サービスやモデルの種類に依存しない条件を以下に固定する。原画のファイル名、参照の役割、最終プロンプトも保存する。

## 1. 渡す参照と優先順位

1. references/<キャラ>_standing_224x160.png：採用済みゲーム用待機。容姿・顔・頭/靴の大きさ・画素倍率・色の正しい基準。
2. references/<キャラ>_approved_idle.png：生成前の大きい採用原画。小さい顔や衣装の形を確認する補助。原画の画像寸法をゲーム用サイズと取り違えない。
3. references/<キャラ>_crouch_224x160.png：採用済みしゃがみ。しゃがみの腰/膝/服の参考。別モーションでも人物の倍率は1の立ち姿勢で決める。
4. palettes.json：各キャラの既存全クリップ共通32色、#RRGGBB形式。新しいモーションごとにパレットを作り直さない。

旧素材や第三者の騎士の画像を新キャラの容姿基準にしない。衣装・体格・画風は上の採用済み3キャラで揃える。立ち/しゃがみ/呼吸の姿勢は本作独自表現。

## 2. 全キャラ・全モーション共通の固定値

| 項目 | 条件 |
|---|---|
| 実装用1コマ | **224×160px**。余白も含む固定キャンバス、透過RGBA PNG |
| 座標 | 描画画像の左上(0,0)、x=0〜223、y=0〜159 |
| 接地（地上の待機/しゃがみ） | 靴の最下端がy=159。呼吸中は底の12行（y148〜159）を同じ画素に保つ |
| 水平の基準 | 胴の中心x=112。伸ばした腕・マント・尾を含む外接矩形で人物を中央寄せしない |
| Unity | PPU50、pivot(0.5,0)、Point、Mipなし、圧縮なし、NPOT縮小なし |
| 画素 | 正方形の整数ピクセル。回転補間・半透明の縁・ぼかし・グラデーションなし |
| 色 | キャラ別共通パレット32色以下、alphaは0または255。透明部のRGBは0 |
| 向き | 原画は右向き、同じ横/斜め横の角度。左向きはゲームの反転表示で作る |
| 体の大きさ | 頭・手・靴・小物を同じ物理サイズで保持。各ポーズを外接高さへ独立リサイズしない |
| 待機/しゃがみ | 8F/6fps、約1.33秒ループ。横8コマ1792×160px、コマ間の余白/枠なし |
| プレビュー | 等倍224×160、整数3倍672×480。拡大画像をゲームへ渡さない |

GIFは1コマ17/100秒、背景色#222731の確認用。ゲーム用PNGは透明のまま、再生は正確に6fps。空中/エフェクトの姿勢は動作に適したanchorを先に定義し、人物の倍率と224×160セルを維持する。

PPU50では1コマ4.48×3.2unit。人物の立ち姿勢は透真2.48、蓮2.00、千鶴2.24unit。身長差を維持し、セルの幅/高さは全員同じにする。

## 3. キャラ別の固定情報

| キー | 立ち基準 | ART3Cしゃがみ中立の実高さ | 顔・衣装 |
|---|---:|---:|---|
| toma / 透真 | 124px | 91px | 黒い無造作髪、青い目、黒/チャコールのビジネススーツ、白シャツ、暗い青紫のネクタイ、黒ズボン/革靴。黒い長方形の携帯端末を腰の近くで持つ |
| ren / 蓮 | 100px | 77px | 茶髪、青緑の目、人間の少年。クリーム色の犬顔フード/茶色の垂れ耳、クリーム袖/青緑の袖口、赤橙の骨留めスカーフ、肉球ミトン、黒系短パン/橙裾、赤/クリーム/青緑のマント、犬の尾、白ソックス/色付きスニーカー |
| chizuru / 千鶴 | 112px | 84px | 茶色ボブ/茶色の目、オレンジのセーター、紺のフレアスカート、素足の脚、白スニーカー。私服を維持、シェフ服に変えない |

立ちの身長は全モーションの倍率を決める基準。しゃがみの実高さは腰と膝を曲げた結果で、独立したリサイズの目標にしない。現行しゃがみは91/77/84pxで固定し、描き直しで姿勢が変わる場合も頭/靴の倍率を先に照合する。

### 頭・靴を合わせる部分比較

同じ224×160の枠だけでは人物の大きさは揃わない。**採用待機と新しい姿勢を同倍率で並べ、頭/顔・髪・靴の物理サイズを保つこと。** 腰や膝を曲げて全体の高さを下げ、頭や足を拡大してキャンバスを埋めない。

| キャラ | 頭部帯の横幅：待機→しゃがみ | 靴の下8行の幅：待機→しゃがみ（画面左/右） |
|---|---|---|
| 透真 | 37→37px | 14/24→16/25px |
| 蓮 | 58→59px（犬フード/耳込み） | 17/23→17/26px |
| 千鶴 | 43→44px（髪込み） | 17/20→17/21px |

頭部帯は不透明頭頂から透真25/蓮31/千鶴34行の外接横幅。頭全体の縦寸法を測った値ではない。靴はy152〜159の不透明連結成分（高さ4以上・下端157以上）を測り、蓮のマント先端等を除外。今回の同じ向きの待機/しゃがみ比較は頭部幅差2px以下・靴幅差3px以下を検査し、実測差は頭0〜1px・靴0〜3px。proportion-check.jsonに測定領域と結果を保存する。

部位の数値は画素の丸め・足の向きで輪郭が少し変わるため、**等倍/整数拡大の目視確認も必須**。別角度のポーズは投影幅だけに無理に合わせず、元の頭/靴の大きさを描画上で照合する。今回と同じ角度で上の差を超える場合は部分の形を見直すか原画を再生成し、検査値を緩めて通さない。人物全体の独立拡大で部分を合わせない。

## 4. しゃがみ生成プロンプト（共通部分）

以下をそのままコピーし、[CHARACTER]と[STANDING_HEIGHT]を表の値にする。原画は1のPNG、補助画像は2のPNGを添付する。prompts.jsonには今回実際に送ったキャラ別全文も入っている。

```text
Use case: identity-preserve.
Asset type: crouching game sprite for the original platform fighter あにあにバトル！！.
Reference 1: approved game idle; authoritative identity, costume, face, pixel scale and palette.
Reference 2: approved larger idle drawing; supporting face and costume detail.
Subject: [CHARACTER]. Preserve the same face, eye color, hair, costume and accessories.
Create exactly TWO full-body poses in ONE horizontal row: LEFT standing idle, RIGHT low grounded crouch.
Use equal transparent cells, a clear transparent gap, identical ground baseline, the same camera and magnification.
Both figures face RIGHT at the reference angle. Their heads, hands, shoes and accessories keep the SAME size.
Match the approved standing sprite's head/hair and shoe dimensions; do not enlarge the head or feet to fill a shorter crouch.
Crouch by bending the hips and knees, never by shrinking or vertically squashing the whole character.
Both soles rest on the ground; no kneeling, sitting, jumping, merged knees, duplicated limbs, missing accessories or melted faces.
Keep the head angle and expression. Make both legs and both hands readable.
For Chizuru, the skirt naturally covers the bent thighs; retain her casual clothes.
Crisp square pixel clusters, sharp dark outline, restrained shading, the supplied character palette, no blur or soft edges.
No text, labels, numbering, borders, shadows on a floor, background, checkerboard, glow, gradients, ghost limbs or crossfade.
Final export contract: 224x160 pixels per frame, feet baseline y159, torso x112, PPU50, pivot(0.5,0).
Standing-height ruler: [STANDING_HEIGHT] pixels. Apply the standing-based scale equally to the crouching pose.
Genuinely transparent PNG.
```

別モーションの場合はしゃがみの指定部分だけを対象動作へ置き換える。参照・人物の倍率・共通パレット・セル/足元条件は保持する。Toma/Ren/Chizuruの別々の姿勢を一度に1枚へ混ぜない。

## 5. 原画からゲーム用PNGへ

1. 原画をそのまま保存。サービスが224×160を直接出せなくても、キャンバス条件を外さず後工程で正規化する。透明alphaが本物か確認し、チェック柄/白背景を描き込んだ画像は採用しない。
2. 立ち/しゃがみを透明な間隔で分離し、立ちの不透明な外接高さを測る。倍率s=立ち基準px÷原画の立ち外接高さ。同じsでしゃがみの縦と横を縮小する。しゃがみの高さだけを91/77/84へ無理に合わせない。
3. 正方形の画素格子へ整え、胴をx112、足の下端をy159へ置く。最低左右2px、上3pxの透明余白を確保し、手・耳・マント・尾・靴を切らない。
4. palettes.jsonの既存パレットへ減色し、alphaを0/255にする。顔/目が潰れたら基準絵と並べて修正し、元の衣装や小物を省略して通さない。
5. 採用したしゃがみは1枚を基準に8Fへ展開する。別々の8枚をAIに描かせることで顔/服が揺れる場合は、1枚から整数行移動で作る。
6. 共通キャンバスの等倍PNG8枚と1792×160横シート、JSON、確認用GIFを出力。原画/プロンプト/倍率/分割領域も保存する。

現行の原画分割と倍率はnormalization.jsonに記録。リポジトリではSpriteForgeのModalShrinkと共通パレットへのQuantizeを使う。現在の素材のバイト単位の再現は同じ原画/処理を使い、新しいAI生成の画素が完全一致するとは扱わない。

## 6. 顔・衣装を固定した8F呼吸

基準絵の画素をそのまま動かす。前後画像を混ぜない。行ごとの逆参照は次の通り（左上原点、round_to_evenは中間値を偶数へ丸める）。

```text
offset = [0, -1, -2, -1, 0, 1, 2, 1]
weight(y) = 1                       (y <= 132)
          = (148 - y) / 16           (132 < y < 148)
          = 0                       (y >= 148)
sourceY = y - round_to_even(offset[frame] * weight(y))
sheet[x + frame * 224, y] = base[x, sourceY]
```

画像外のsourceYは透明。顔/上体は同じ画素を上下±2px、下腿で変位を吸収し、靴の下12行は固定。headHeightsは実際の不透明上端から160−topで各コマを測る。fps6では10tick/コマ（60Hz）で、80tick後にコマ0へ戻る。

## 7. JSONとファイルの約束

```json
{
  "schemaVersion": 1,
  "name": "crouch",
  "frameCount": 8,
  "cellWidth": 224,
  "cellHeight": 160,
  "fps": 6,
  "targetHeight": 124,
  "baseline": 159,
  "loop": true,
  "pivotX": 0.5,
  "pivotY": 0,
  "pixelsPerUnit": 50,
  "bindings": [{"State": 24, "ActiveFrame": 0, "Clip": "crouch"}],
  "headHeights": [91,92,93,92,91,90,89,90]
}
```

例は透真。targetHeightは立ちの倍率基準、headHeightsは実コマの頭頂高さ。蓮はtargetHeight100/headHeights[77,78,79,78,77,76,75,76]、千鶴は112/[84,85,86,85,84,83,82,83]。他モーションは正しいname/State/フレーム構成を設計し、既存IDやTimingを勝手に変更しない。

個別PNGはcrouch_00.png〜crouch_07.png、横シートcrouch_sheet.png、メタデータcrouch.json、確認GIFcrouch_preview.gif。ゲームの参照先はArt/Sprites/<キャラ>/crouch.png・crouch.json。スプライトをコマごとにトリミングしない。

## 8. 採用前の検査とやり直し基準

- 立ちと並べ、顔/目/髪/頭・靴のサイズ/衣装/小物を照合。腕2本・脚2本、膝/靴の向き、両足の接地を拡大と等倍の両方で見る。
- 頭/髪と靴の部分寸法を採用待機基準で確認。セル寸法と全体身長だけの合格で済ませない。測定領域・姿勢による差・目視所見を保存する。
- 全24Fの224×160・8F/6fps・PPU50・pivot・透明余白・足元159・二値alpha・共有32色を検査する。
- 全コマで下12行が完全一致、顔/上体が基準絵の画素移動と一致。胴も上下し、輪郭が二重にならない。GIFを全コマ復号する。
- 頭頂の実測とheadHeightsが一致し、ゲームのしゃがみ状態が正しい8Fへ接続する。80tickで正しくループする。
- 保存原画から再生成し、PNG/JSON/GIFのSHA-256一致を確認。頭を縮める・顔を省略する・アサーションを緩めることで不良を通さない。
- ゲーム接続後はUnityのBuildAll/EditMode/PlayMode/ビルドを全実行。実画面を撮る前は「PCに触らないで」と案内。失敗/未検証を合格として記録しない。

## 9. リポジトリでの再現

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Crouch.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/CompareArt3Proportions.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildSpriteGenerationKit.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag art3c
```

原画/最終プロンプトはart_source/art3_crouch/。参照/パレット/全文プロンプト/規格/採用済みシートはart_source/sprite_generation_kit/、持ち出し用はart_source/sprite_generation_kit.zip。Windows PowerShell用のスクリプトはASCII、Unityはサンドボックス外・-nographicsなし。新作業の指示書を先に作り、原画を消さず、結果と残課題をRESUMEへ記録する。

## 10. ART3D：ジャンプ・ガード・回避・ダッシュの固定条件

すべて同じ224×160セル・PPU50・pivot(0.5,0)・共通32色。採用待機と同じ倍率/頭部/靴で新しいポーズを描く。現在の実装は次の通り。番号0は原画に添えた立ちの物差しで、ゲーム用待機を差し替える画像ではない。

| 原画グループ | 原画内の並び | ゲームへの割当（キー番号） | フレーム/fps/loop |
|---|---|---|---|
| jump | 立ち0、上昇1、頂点2、下降3 | jump=[1,1,1,2,2,2]、fall=[3,3,3,3,3,3] | jump6F/15/false、fall6F/15/true |
| jump_start | 立ち0、浅いかがみ1 | land=[1,採用待機00]、地上予備はlandのコマ0 | land2F/15/false |
| guard | 立ち0、胸元を守る構え1 | [1,1,1,1,1,1] | 6F/15/true |
| dodge | 立ち0、準備1、低い横回避2、復帰3 | [1,2,3] | 3F/12/false |
| dash | 立ち0、足運び4姿勢1〜4 | [1,1,2,2,3,3,4,4] | 8F/15/true |

地上ジャンプはユーザー指定の**2tick（60Hz、約0.033秒）接地した浅いかがみ→踏切**。スタミナは予備開始で1度だけ支払い、中断しても払い戻さない。踏切の初速/重力と空中2段ジャンプは既存値。予備中に新しく押した行動はバッファに残す。被弾/場外/足場消失で予備を中断し、ヒットストップは残りtickを止める。FighterMode末尾JumpStartup=15、AnimationState末尾JumpStartup=34を追加、State34と既存Land=5は同じlandへ接続。着地8tickの最初4tickが浅いかがみ、次4tickが採用待機。通常しゃがみの深い姿勢を流用しない。浅いかがみの実高さは透真104/蓮89/千鶴98px。

原画の指示では「完全な人物を透明な間隔と外周余白で分離」「立ちと全キーを同じ画素倍率」「頭は同じ角度・幅、手は顎より下」「蓮のマントは肩より下」「速度線・二重の腕脚・新しい装備を描かない」を明示する。ジャンプ原画4人/ガード・浅いかがみ2人/回避4人/ダッシュ5人は、各々1キャラの複数姿勢である。3キャラを1枚に混ぜない。ダッシュは脚の接触/通過と重なりを変える4姿勢を描く。ガードは盾を追加しない。全文の実プロンプトはactions/generation-log.jsonに同梱。

採用原画の立ち高さで倍率sを決め、全キーへ同じModalShrinkを適用する。4連結成分の実マスクから人物を取り出し、内部の分離した瞳等もその人物の外接矩形に完全に含まれる場合に保持する。隣のマントがx方向に重なっても、推測の格子や矩形だけで隣人の画素を混ぜない。図形が接触・画像端で欠ける原画は再生成。空中3キーの共通中心はmin(159−立ち基準高さ/2,157−(正規化後最大空中高さ−1)/2)で、一番背の高いキーにも下2pxの余白を確保する。姿勢ごとの独立リサイズ/独立中心合わせはしない。

normalized/*-scale.jsonに各原画寸法・全キーの分割矩形/面積・共通倍率/配置・頭部帯幅を保存。51キー（15立ち物差し＋36動作）すべての頭部帯幅は待機基準±2px以内。靴は走る/空中/回避で角度と接地足が変わるため、下8行の幅だけを靴全体サイズと取り違えない。同倍率で頭/靴/手足を等倍と3倍で目視照合する。今回採用は蓮dodge v3/dash v2、千鶴dash v2、他はv1。不採用4原画も残し、source-selection.jsonで実際の採用を明示。部分の拡縮や検査値の緩和で通さない。

PNGはready/actions/<キャラ>/<clip>_00.png〜、横シート、JSON、等倍GIF。3倍プレビューはゲームに取り込まない。6クリップ/人93Fを差し替え、全106クリップ584Fと既存スプライトID/Timingを維持する。再現コマンド：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3Actions.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildSpriteGenerationKit.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag art3d
```

キットのactions/に全文プロンプト・採用選択・倍率記録・検査結果・原画・生成ツールを、ready/actions/に実装PNGを格納。再生成ツールの実行には、元リポジトリのmanifest/旧raw/forge-report（既存共通パレット）が必要。別環境で新規生成する場合も、同梱の参照/パレット/仕様を優先し、ゲーム用出力の検査を行う。

## 11. ART3E：歩き8枚・左右交互の走り・地上通常3段

2026-10-13 最終指定（初期案より優先）：透真は右パンチ→左パンチ→右キック、通常中は端末をしまう。蓮は跳び込み頭突き→頭頂を床につけ腰を回すカポエイラ蹴り→両腕で床を押し前方両足蹴り。千鶴は同じ右手の小カバンで右側から一撃→腰/肩をひねり二撃→投げて手元から消す。実通常前進0.18/回、被弾反動は既存Knockbackへ追加0.18/6tick。軽い被弾は上体が後ろへ倒れかけ耐え、実ハート消失は痛い表情/大きい体反応で別原画。被弾画像は全攻撃生成後。ART3F急停止/切り返しも後続。

蓮attack1 v13は頭幅60/57/58/58。キー2のみ頭上端58/浮遊4〜64px、キー3およびattack2キー1〜3/attack3キー1は床側31pxでフード幅58±2・クリーム頭頂が床へ接地。attack3キー2は両腕支持/頭は上側、キー3は足で着地。走り浮遊4〜32pxの基準は保持。頭ROIや独立拡縮で合格させない。千鶴attack1 v3は45/45/45/45（基準43±2）。正面の右肩は画面左、背中の右肩は画面右に見えるため画面位置だけで持ち手を判断しない。

最新手描き参考はカバンの円弧/体軸回転/荷重移動/逆さ脚軌道に使う。ローカル参考はdocs/art_reference/_motion_local/、公開素材/ZIPへ収録しない。原画/全文条件/採否/倍率/SHAはart_source/art3_walk_attacks/、詳しい再開条件はdocs/codex_logs/art3e_checkpoint_20261013.md。現ZIPは前回ART3D版で更新前。

最終の蓮通常指定は前進頭突き→頭を床につけた右蹴り→同支持から左蹴り。以前の蓮パンチ案は上書き。逆さ2/3段目は立ち0の頭部帯を上端31px、動作キー1〜3の逆さフードを床から31pxで測り58±2pxを照合する。体軸と腰を回して左右の脚を交互に伸ばす。立ち100px由来の共通倍率/靴の実寸/2腕2脚/床159を保持。第三者の参考は動作理解のみ、素材/キットへ収録しない。

最新の攻撃指定は透真/蓮とも右パンチ→左パンチ→右キック。腰と肩を回し、予備の後ろ荷重→打撃の前荷重→戻りで体軸を移す。キックは左足接地・右膝ため・右脚伸ばし、腰は前/上体は後ろへ。固定した胴から手足だけ伸ばす姿勢は不採用。頭/靴の実寸と立ち倍率は維持。透真は通常攻撃中に端末をしまい、両手で打てる表現へ（以前の「攻撃中も端末左手」は上書き）。千鶴のカバンは攻撃中だけ、歩き/走りにはなし。

段階確認は BuildArt3WalkAttacks.ps1 -WalkOnly（歩き）→ -GaitsOnly（歩き/走り）→オプションなし（通常を含む全対象）。各段階の素材検査とUnity全4工程の完了は別々に記録する。既存dashのState2/ActiveFrame1、walkのState1/ActiveFrame0、通常のState6/7/8・ActiveFrame2を維持。

走りの接地キー1/3は足159。浮遊キー2/4は**共通倍率を保って頭頂を立ちより2px上**へ配置し、両足の最低画素は地面から4〜32px上に置く。セルのpivot/接地面159は全コマ固定。浮遊の小さい外接矩形を地面まで拡大しない。出力は接地0/1・浮遊2/3・反対接地4/5・反対浮遊6/7、8F/15fps loop。肘を約90度に曲げて左右の腕を脚と逆に振る。描画のみでシミュレーションのGrounded/速度/期間は維持。画素検査とUnityテストで、接地は159・浮遊は足の間隔と頭の高さを別々に厳密確認する。

透真の端末を持つ左腕は手前の肩から始まる。手を前へ振るときは袖が胴の前を斜めに横切って肘/袖口/手首へつながり、空いた右腕は奥の肩から胴の後ろへ。端末を別の手へ渡さず、親指と手首の向きも同じ持ち手として描く。腕の手先だけ左右を交換しない。

ユーザー追加指定：歩きは**合計8枚すべて別の絵**。待機の物差しは枚数に入れない。前半は左脚で支えて右脚を曲げ、後半は右脚で支えて左脚を曲げる。左右の接触→沈み→通過→上がりを交互にした8F/15fps loop。3/4右向きでは近い左脚を前に、遠い右脚を後ろに描き、腿/脛/靴の陰影と重なりも入れ替える。同じ脚の4姿勢を繰り返して8枚扱いにしない。

**両腕の振り子も必須**。肩を支点に上腕/肘/前腕/手の一続きの形を前後へ振る。コマ0の左脚前接地では左腕が後ろ/右腕が前、コマ4で逆。通過コマ2/6では腕が胴の横を下向きに通り、中間1/3/5/7は半分の角度。左右の腕は逆位相で、脚とも逆位相。固定した構え/片腕だけの振り/手首だけの移動/両腕同方向では採用しない。透真の端末を持つ腕も振り、端末は手に追従する。肩位置、腕長、手の大きさと遠近を保持。全身2倍/肩から手首3倍/足4倍のボードと8枚GIFで7→0の連続まで目視する。ユーザー提供の歩行/走行画像は動作の理解だけに使い、人物/絵柄/衣装をコピーせず、第三者の参照画像はキット/ゲームへ収録しない。

walk原画は立ち0＋接触/通過の基本4姿勢、walk_inbetweens原画は立ち0＋沈み/上がりの新4姿勢。両原画とも採用待機の同一立ち高さから共通倍率を求める。最終順は基本1/間1/基本2/間2/基本3/間3/基本4/間4。靴は必ずどちらかが接地し、走りのように両足を浮かせない。dashも左右の支え脚/曲げ脚を交換した4姿勢を[1,1,2,2,3,3,4,4]の8F/15fps loopへ。以前の走りは保存し、新しい採用はwalk_attacks側に明記する。

生成が左右を取り違える場合は、pose-guides/の自作骨格図を追加入力する。赤=解剖学的左/手前、青=右/奥、手前の脚を重なりで前へ。図はポーズの指示専用で、衣装を赤/青に変えず、ゲームへ取り込まない。頭/顔/フードは採用待機と同じ角度と倍率、頭部帯幅±2pxと靴の目視条件を維持。検査値の緩和や頭/脚の独立拡縮で通さない。

地上通常は透真/蓮が**パンチ→パンチ→キック**、千鶴はカバンで叩く3段。千鶴の小さな茶色の手提げは本作独自演出で、**攻撃の時だけ出す**。待機/歩き/走りへ描き足さない。以後の攻撃原画は最初の採用カバンの寸法・形・短い持ち手を参照して統一。私服（オレンジのセーター・紺スカート・白靴）を維持する。

attack1/2/3は別原画で立ち0＋予備1/打撃2/戻り3、[1,1,2,3,3,3]の6F/15fps/nonloop。State6/7/8、出力ActiveFrame2を保持し、シミュレーションの発生中は予備、持続中だけ打撃ピーク、硬直中は戻り。拳/カバンは胸/腹の高さで顎より下に置き、頭部帯へ混ぜない。キックは軸足接地・蹴り足の自然な膝/足首・元の靴サイズを保つ。ダメージ/判定/連係/攻撃速度の変更は含めない。空中/しゃがみ/後方の方向別通常は別作業として制作する。

walk_attacks/には全文プロンプト/参照の役割/原画/採用選択/寸法/SHA/倍率/頭部値/左右目視記録、ready/walk_attacks/に1コマ224×160PNG・1792×160歩き/走りシート・1344×160攻撃シート・JSON・GIFを同梱。現在の再生成は**BuildArt3WalkAttacks.ps1**を使う（古いBuildArt3Actionsだけでは古い走りへ戻る）。全106クリップ584F・sprite ID/Timingと対象外PNGを保持し、Unityの実行件数は最新RESUME/最終報告を確認する。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildArt3WalkAttacks.ps1 -RebuildCheck
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/RenderGaitReview.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/SpriteForge/BuildSpriteGenerationKit.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag art3e
```
