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
