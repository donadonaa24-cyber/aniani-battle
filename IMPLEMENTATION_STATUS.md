## 最新（2026-10-16 Codex）：R4 透真の攻撃16原画・飛翔物3種をゲームへ接続

現在はR4の透真を完了。通常3段・地上上／下・空中N・特殊のドローン指令・超必殺のロケット指令を各「予備→当てる」の2枚へ接続。背面は通常1の2枚を本人の向きに合わせて再利用。銃弾・ドローン・ロケットも新しいアニメ塗り／白縁の原画へ差し替えた。基本5ポーズ、他8人、通常45件の半減判定、ゲージ14/10、千鶴のおにぎりとカバン13原画を維持。発生・有効・硬直・速度・寿命・空中の重力はR3のまま。ダメージ遅延のある技も、その開始までは予備の表示と傾きを保持する。

今回実Unity rework_r4_toma_5：BuildAll成功／Edit525/525／Play59/59／Windowsビルド成功、失敗・スキップ0。起動はBuild/rework_r4_toma_5/AniBato.exe。docs/screenshots/rework_r4_toma/へ実描画72PNG（本人54＋飛翔物18）。左右・背面・予備／有効／硬直／解除、実際の飛翔物の画像・中心・大きさ・回転を検査。OSの実キー入力なし。1回目の追加テストの型名衝突と2回目の場外へ置いたfixtureを修正し、重点2件合格後に全4工程を通し直した。既存の検査のアサーションは弱めていない。

現在のゲーム使用画像はイラスト確認/透真/攻撃原画_ゲーム採用済み/。動作名と予備／当てるを付け、待機基準・一覧・飛翔物も同梱。原画20候補から16を選定、飛翔物3、全文条件／参照役割／SHA／現在版／部位比較／画素監査／採用証跡はart_source/paper_roster_action/toma/。内蔵image_genを使用。人物1304×1450、PPU532.5893、可視足元1328、再サンプル0。枠内RGBAは元と一致。枠外の1/255アルファの空白ノイズ56画素だけは梱包時に除外し、元原画は保持。完全な元画像全画素一致とは呼ばない。頭・靴・手の微差と美術最終承認はpart-reviewに残す。

保存対象は今回の原画・Editor接続・表示処理・検査・資料。全4合格を確認した区切りで通常commit／origin mainへpushする。再開時はgit status／git log／originとの一致を確認すること。報告はdocs/codex_logs/rework_r4_toma_final.md、進行表はcodex_tasks/rework_progress.json。採用確定の補助はadopt_paper_action.cjs（全4証跡とゲームPNGのSHAを先に検査する）。基本の梱包／監査手順は原画フォルダのREADME。

残り：R4の蓮→千鶴の不足→湊→シロ→結衣→黒獅子王→暁→龍太。次の指示書はREWORK_R4_REN_ACTIONS。基本45ポーズと透真の攻撃を再生成しない。R5の生成舞台・集合絵と同絵の選択、R6のWebGLは未完。透真の移動・回避・被弾等にも旧素材が残り、ActionArtworkPendingを維持して全アクション完成とは呼ばない。

Claudeへ：まずtasklistとGit差分。透真の通常3段と上下／空中N、左右の背面、指令と実弾をadoption.json／実描画で確認してほしい。PaperRosterCoreBuilderが毎回基本姿勢を再構築するため、後段のPaperActionArtworkBuilderのマージを外さない。次の人物用に透真固定のImporter／補助を拡張するときは、透真の採用と本人ごとの倍率・SHA・基本5姿勢を維持する。
## 最新（2026-10-16 Codex）：R3 9人の固有技・全4工程合格

最新9人の指定に合わせ、通常武器・特殊1種・超必殺を本人用の独立AttackDataへ接続。透真は銃通常／ドローン特攻／頭上ロケット、千鶴はフライパン／おにぎり／カレー。湊の瞬間移動と残留空間、シロのEMPとALMA、将棋3人の飛車／角／王と召喚、暁のカードとピラミッド、龍太のバットと呪文を実装。追加6人の共有技試作を終了。移動先・足場端・落下・寿命・遅延・同一攻撃の多重防止は純C#で検査。仕様と数値はROSTER_MOVES_CURRENT／BALANCE_CONFIG／SPEC_REVISION_20261016。

今回実Unity rework_r3_3：BuildAll成功／Edit521/521／Play57/57／Windows成功、失敗・スキップ0。起動Build/rework_r3_3/AniBato.exe。実描画37PNGはdocs/screenshots/rework_r3/（特殊・超必殺の予備／有効36＋銃1）。OSキーなし。CPU通常の元3人270戦と9人全組合せ3seedの243戦、全513戦で双方攻撃・ハート減少、CPU自滅としての集計0。元3人は超必殺188/270（69.6%、既存基準50%以上を維持）、9人は155/243。公開CSVはdocs/codex_logs/rework_r3_3_cpu.csvと同_roster_cpu.csv。

1回目は飛車の足場端の浮動小数誤差、召喚編隊と投射制限、銃が届く旧アニメfixtureの合計5失敗を修正。2回目520/57全合格後、透真の連射を追加レビュー。拳銃は専用通常弾1枠で前の銃弾だけを置換し、ドローンと共存する。3段とも実Tickで発射する回帰検査を加え3回目で全工程合格。透真schema2／他8人schema1、既存調整値をBuildAllで上書きしない。原アセット45件の半減CSV、ゲージ14/10、千鶴おにぎり幅約0.4205867world、カバン13原画を保持。基本40原画の画素／梱包監査も合格。ヒントとCPU難易度の設定は変更直後に保存する。

今回の新規原画生成は0枚。専用原画はR4で、追加6人の本人待機による変形、透真・蓮の旧ピクセル、色付きの仮飛翔物が残る。ActionArtworkPendingと選択画面で制作中を示し、完成イラストとは区別する。次はREWORK_R4_ACTION_ARTとREWORK_R4_TOMA_ACTIONS。透真の通常3段・上下・空中N・特殊・超必殺の2原画ずつから人物単位で制作する。基本45ポーズを再生成せず、溜め専用絵も作らない。R5生成舞台と9人集合／同絵の選択、R6 WebGLは未完。透真等の移動・被弾の未移行分も後続として残す。

Claudeへ：最初にtasklistとGit差分を確認し、R0〜R3を重複せずR4の保存済み原画／manifestから再開。9人の技と原画の状態を混同しない。拳銃の3段とドローン併用、瞬間移動の移動先、飛車／角の端、召喚3個で1回分、元45判定と紙原画のサイズを確認してほしい。
## 最新（2026-10-16 Codex）：R2 順番チーム戦・設定画面、全4工程合格

P1の3人→CPUまたはP2の3人を選び、選択順に各ラウンドへ出場、先に2勝でチーム決着。チーム内重複禁止、相手との重複可、上下左右・クリック・取消に対応。2対0で3戦目を省き、1対1なら3人目、引き分けは同じラウンドを再試合。各ラウンドでシミュレーションを新しくし、ハート／蓄積／ゲージ／回数／状態／時間／位置／縮小を初期化する。練習は各1人で、R1の超必殺無制限を維持。

トップをCPUチーム／2人チーム／練習／設定／終了の5項目へ整理。音量・FPS・揺れ・ヒント・ダッシュ・判定・CPU難易度・キー・操作説明・チュートリアルは設定へ。ポーズから設定と操作説明・キー変更へ入ってもtickを止める。編集中の人物の技表を開く。詳細はTEAM_FLOW_CURRENT。

今回実Unity rework_r2_2：BuildAll成功／Edit505/505／Play55/55／Windows成功、失敗・スキップ0。起動Build/rework_r2_2/AniBato.exe。docs/screenshots/rework_r2/に実描画6PNG（CPU編成・取消・ラウンド結果・2対0・設定・2対1）。OSキーなし。1回目の505/55合格後、選択文の見切れ、撮影時の看板更新を修正し、CPU選択と再開時の溜め漏れ検査も補足して2で再確認。旧ボタン／機器別の入力ガード、カメラ3比率、勝敗のアサーションは新手順で維持。

今回は原画生成なし。9人の待機と基本45ポーズは再利用し、R1の仮攻撃絵も残る。次はREWORK_R3_CHARACTER_MOVES（透真：銃通常／ドローン特攻／落下ロケット等、SPEC_REVISION_20261016の9人の最新指定）。R4の専用原画、R5の生成舞台・9人集合をトップ／選択で共用、R6のWebGLは未完。R3〜R6の指示書は保存済み、進行はrework_progress.json。Claudeは最初にtasklistとGit差分、今回505/55証跡とTEAM_FLOW_CURRENTを確認し、完了したR0〜R2や基本45ポーズを重複しない。通常45判定半減・ゲージ14/10・カバン予備は維持。

## 最新（2026-10-16 Codex）：R1 攻撃入力・相殺・練習無制限、全4工程合格

全9人へ地上上攻撃の専用判定を追加（既存の下攻撃は維持）。空中は全方向N1種、攻撃中も横移動と通常の重力／急降下を継続。特殊は全方向ニュートラル1種、千鶴はおにぎり固定。通常の有効判定同士が重なれば双方相殺、停止と硬直・光とSE、通常飛び道具にも適用。練習は開始から満ゲージ・超必殺無制限、終了時は通常制限へ。技表・チュートリアル・CPUも現行入力へ。詳細はCOMBAT_INPUT_CURRENT。

今回実Unity rework_r1_3：BuildAll成功／Edit499/499／Play52/52／Windowsビルド成功、失敗・スキップ0。起動Build/rework_r1_3/AniBato.exe。実描画44PNGはdocs/screenshots/rework_r1/（9人上下の予備／有効36、空中Nの7入力、相殺1）。OSキー送信なし。CPU通常270戦は全て両者が攻撃しハート減少、超必殺246/270（91.1%、既存基準50%以上を維持）。CSVはdocs/codex_logs/rework_r1_3_cpu.csv。

1回目の新規fixture分類2件とCPU不接近、2回目の蓮同士1戦のハート減少不足を修正。空中Nの実射程・移動、相殺後の個別待ち、空振り後の接近と近接の休止時の間合いへ対応。CPU schema6へ移行。通常45件の半減値・ゲージ14/10・ダメージを維持し、アサーションを下げていない。今回は原画生成0枚。上／下の絵は既存上向きや待機の仮表示を含み、透真と蓮は旧ピクセルが残る。新しい9人固有技と専用原画の完成とは呼ばない。

次はREWORK_R2_TEAM_AND_MENU（プレイヤー3人→CPU3人、上下左右選択、同チーム重複禁止、選択順にラウンド1〜3・2勝先取、設定整理）。R3〜R6の指示書も作成済み。最新9人技指定・チーム回答はSPEC_REVISION_20261016、進行はrework_progress.json。Claudeへは最初にtasklistとGit差分、499/52証跡、COMBAT_INPUT_CURRENTとR2を確認してほしい。旧方向空中／特殊の保存資料は現在の入力一覧と区別し、既存の基本45ポーズを再生成しない。

## 最新（2026-10-16 Codex）：R0 溜め攻撃・全4工程合格

地上の通常ボタン保持で溜め、1秒未満の解放は通常、1〜3秒の解放は溜め通常。180tickで上限、保持中は自動発動しない。通常1発分の被弾蓄積、時間に比例した吹っ飛び、最大は現在のメイン足場端付近へ。左右・縮小後・下攻撃のガード崩し経路・被弾中断を確認。空中は溜めず従来通常。専用絵生成0枚、既存全身絵の傾き／沈み込み／色／紙片で全9人を表示し、実位置と判定は変えない。

今回実Unity rework_r0_3：BuildAll成功／Edit484/484／Play49/49／Windowsビルド成功、失敗・スキップ0。起動はBuild/rework_r0_3/AniBato.exe。実描画18PNGはdocs/screenshots/rework_charge/（全景9＋身体9）。OSキー送信なし。2回目の483/49合格後、ガード崩しのStage転送と追加1件を入れ、3回目で再確認した。旧テストは短押し解放の実入力へ更新し、元の表示・有効時間・3段検査を保持。通常45件の半減値とゲージ14/10は維持。

新仕様はdocs/SPEC_REVISION_20261016.md、実装順はdocs/codex_tasks/PAPER_REWORK_20261016.md。チームは選択順に1人ずつ・先に2勝で確定。R1〜R6は未完。次はREWORK_R1_COMBAT_INPUT.md（地上上下／空中Nのみ・滞空廃止／通常相殺／特殊1種／練習超必殺無制限）。R2は3人選択と設定整理、R3以降は9人固有技・必要原画・生成舞台／集合選択・WebGL。前回の基本45ポーズは再生成しない。Claudeへは最初にtasklistとgit status／log、今回の484/49証跡、溜め1〜3秒と足場端の体感を確認してほしい。

## 最新（2026-10-16 Codex）：全9人の基本5ポーズを接続・全工程合格

今回の工程10は完了。千鶴の既存5種類＋新8人40種類、全45種類のガード／しゃがみ／カウンター／勝利／敗北をゲームへ接続。原画55候補から40を選定し、原画・全文条件・SHA・選定版・部位の概算／微差を各manifestとpart-reviewへ保存。1304×1450、PPU532.5893、足元1328、人物の再サンプルなし。外周アルファ処理後の画素一致と元RGBAは区別。頭・靴・手の最終的な絵の承認は別で、微差は各part-reviewに記録。

今回の実Unity paper_core_roster_1：BuildAll成功／Edit472/472／Play48/48／Windowsビルド成功、失敗・スキップ0。起動：Build/paper_core_roster_1/AniBato.exe。新8人の実入力・左右向き・解除・勝敗表示、各14枚で計112PNGをdocs/screenshots/paper_core_<key>/へ保存。実Camera.RenderでOSキー送信なし。40選定原画のRGB／不透明内部／アルファ／無拡縮梱包／接地をcheck_paper_coreで照合しpixel-audit.jsonへ。カバン13原画とおにぎり幅、通常45判定半減とゲージ14/10を維持。

確認先：イラスト確認/全9人_基本5ポーズ/（動作別5一覧と06_Unity実描画_新8人.png）、各人の基本5ポーズ_ゲーム採用済み/（待機基準＋5種類の等倍PNG）。ゲーム採用と絵の最終承認は別。Unity実描画のガード青／カウンター黄は既存の状態色で、原画の配色ではない。再作成はpaper_core_review.cjsと検証済みタグ付きprepare、手順はPAPER_CORE_GENERATION_GUIDE。

残課題：透真と蓮の未制作のジャンプ／走り／回避／攻撃／被弾などは旧ピクセル表示が残る。追加6人の専用動作と固有技は未制作で、専用絵がない状態は待機1枚の変形と共通技の試作を継続。全アクション完成や9人の固有技完成とは呼ばない。千鶴の敗北は既存の座り込みを再利用。

次：PAPER_TOMA_AIR_AND_MOVEMENT.mdで透真を優先して残りの移動・空中動作を少数原画化。拳銃／遮断ステップ／ドローン上昇・爆撃／複数ドローン自爆の希望は後続で性能を別途整理する。PAPER_STAGE_ILLUSTRATIONSとWEBGL_BROWSER_BUILDは後続として保持。Claudeは最初にtasklistとgit status／logを確認し、今回の45種類を再生成しない。絵の修正指示が来たら名前と動作を照合してその原画だけ新しい版で直す。

## 最新（2026-10-15 Codex）：透真の基本5ポーズを接続・全工程合格

全9人のガード／しゃがみ／カウンター／勝利／敗北の制作を進行中。透真は待機v2＋guard v3／crouch v2／counter v4／win v1／lose v1をゲーム接続。待機／歩き15度、ガード、しゃがみ（ジャンプ前と着地にも再利用）、カウンター、勝敗が紙絵。未制作のジャンプ／走り／攻撃等は旧素材。通常45件の半減値・ゲージ14/10・判定・性能は変更していない。

今回実Unity paper_core_toma_4：BuildAll成功／Edit472/472／Play48/48／Windowsビルド成功、失敗・スキップ0。1の旧表示前提4件、2の旧敗北アニメ先頭固定2件、3の既存PNGロック1件を解決して全工程再実行。表示検査は画像名から実Sprite／正しい旧クリップ所属へ強化し、PNG保存は同一内容の上書き回避＋短い再試行（失敗は例外）へ。今回の起動ファイルBuild/paper_core_toma_4/AniBato.exe。実Camera.Render14PNG：docs/screenshots/paper_core_toma/、OSキー送信なし。新しい確認先：イラスト確認/透真/基本5ポーズ_ゲーム採用済み/。5種類別の全員一覧はイラスト確認/全9人_基本5ポーズ/、未生成と採用を区別。

原画・全文条件・SHA・部位の概算表：art_source/paper_roster_core/toma/。頭・靴の幅差は概算8%以内、全高はしゃがみ約79%（当初目安60〜65%より高く、部位実寸と人体を優先）。最終の絵の承認待ち。内蔵image_genを使用し、明示指示に沿った外周アルファ2値化を別保存。RGB／不透明内部と人物倍率を保持し、元RGBA完全一致とは区別する。カウンターv1〜v3は腕・親指の問題で不採用、v4は右前腕で払う構え。カバン13原画と千鶴の既存5ポーズは保持。

次：蓮の5原画は生成済みの候補。guard v1だけ靴の横幅／向きが変わったためv2を作り直してから梱包・審査する。原画はart_source/paper_roster_core/ren/。残りは湊→シロ→結衣→黒獅子王→暁→龍太（計30姿勢）を1人ずつ作り、残り7人の接続をまとめて実Unity全4工程で確認する。制作順の調整理由はPAPER_ROSTER_CORE_POSES冒頭。千鶴は既存5種類を再利用（敗北は座り込みのまま）。全員完成とは扱わない。手順はPAPER_CORE_GENERATION_GUIDE／progress.json。Claudeへ交代するときは生成済みを再生成せず、現在のjobsと選定版を確認。ステージ原画／WebGL指示書は後続として保持。

## 最新（2026-10-15 Codex・検証中）：全9人の基本5ポーズ

透真5候補を生成・梱包・接続。原画9枚からguard v3／crouch v2／counter v4／win v1／lose v1を選定。イラスト確認/透真/基本5ポーズ_確認中/、実Camera.Render14枚はdocs/screenshots/paper_core_toma/。OSキー送信なし。

直近paper_core_toma_2はBuildAll成功／Edit472/472／Play46/48／Windowsビルド成功だが全体不合格。旧敗北クリップを先頭コマに固定した検査2件を修正し、paper_core_toma_3の全4工程を実行中。1では旧表示前提の検査4件、2では上記2件。削除やアサーション緩和ではなく、紙絵の実Sprite一致と未移行アニメの正しい勝敗クリップを確認する。

未commit。今はUnityの終了待ちから再開し、失敗があれば直して全工程をやり直す。合格後に採用状態・STATUS／RESUMEを更新し通常保存、その後に蓮→湊→シロ→結衣→黒獅子王→暁→龍太の各5原画（計35）へ。千鶴5種は既存再利用。進行表art_source/paper_roster_core/progress.json。詳細はPAPER_ROSTER_CORE_POSESと下の制作記録。Claudeのステージ原画／WebGL指示書は後続として保持。既存差分を破棄しない。

## 最新（2026-10-15 Codex・制作中）：全9人の基本5ポーズ

ユーザーの全員生成指定でPAPER_ROSTER_CORE_POSESに着手。順は透真→蓮→湊→シロ→結衣→黒獅子王→暁→龍太、千鶴5種は既存を再利用。Claudeの追加指示書3本を保持。現在は透真5候補を生成・透明梱包し、専用Editor接続と検査を追加した段階。原画9枚のうちguard v3／crouch v2／counter v4／win v1／lose v1を選定。頭・靴の概算差は8%以内、しゃがみ全高は約79%で目安より高く部位寸法を優先、最終審査待ち。候補確認：イラスト確認/透真/基本5ポーズ_確認中/。原画・全文条件・部位表：art_source/paper_roster_core/toma/。画像生成は内蔵ツール、外縁アルファは指示書通り2値化し原画／RGB／倍率を保つ。

今回のUnity検証はこれから。前回保存済み470/47を今回の合格とは扱わない。未commit差分を破棄しない。これからrun_unity.ps1 -Tag paper_core_toma_1をサンドボックス外で実行→結果と画像を確認→通常保存して次の蓮へ。残り7人35原画は未生成、千鶴は再利用対象。ステージ原画／ブラウザ版は後続。
## 最新（2026-10-15 Codex）：9人選択・紙舞台・横振り・判定表示を完了

今回の続行順：Claudeの6fc196aと未commit舞台2ファイルを確認→紙舞台／9人選択を0bbc2b6で保存→千鶴の横振り統一と判定点検。通常45アセットの半減値とゲージ14／10は変更していない（CSV全件の回帰検査合格）。

実装済み：3作品の工作舞台とテーマ別縮小見た目、9人の切り抜きタイトル、厚紙FIGHT／KO、作品別の9人一覧から選択・対戦。追加6人は既存3作品の基本技を共有する1枚絵の試作で、固有技・固有ポーズは未完成。透真は湊に頭身を合わせたv2をタイトル／選択で使用。透真／蓮の戦闘動作素材は旧版のまま。

千鶴：通常はすべて横振り6原画（各段予備→有効の2コマ）。空中上下60度・斜め30度の固定傾き。手持ちおにぎりをv3へ拡大、飛翔は紙版を継続し両方約0.42world幅へ（手持ち約2倍／飛翔約6.5%微調整）。共通ゲーム枠1304×1450・PPU532.5893、人物原画を再サンプルしない。カバン13原画／上下振り6原画と旧採用コピーを保護。現在の採用46画像はイラスト確認/千鶴/01_ゲームで採用済み/、比較は比較_フライパン原画12枚/へ改名し採用／予備を明記。

判定：F9／メイン設定末尾／ポーズ末尾でON/OFF（初期OFF、練習初期ON）。緑やられ／赤有効／黄飛翔／薄赤予備、命中枠を0.5秒保持（静止中は保持）。練習F5で通常→1/2→1/4、F6静止、F7で1tick。ポーズの練習設定からも操作可。コマ送り後は描画位置を現在tickへ同期。通常速度／スロー／コマ送りの同入力列で決定性を確認。

今回実Unity paper_sideswing_3：BuildAll成功／EditMode470/470／PlayMode47/47／Windowsビルド成功、失敗・スキップ0。1も470/47合格したが比較撮影と描画同期を改善、2の同一PNG書込みロックをWriteIfChangedで修正して3で全工程再検証。今回の起動ファイルBuild/paper_sideswing_3/AniBato.exe。最新2ビルドだけ保持。実Camera.Renderの攻撃17＋やられ3＝20PNGはdocs/screenshots/hitbox_review_chizuru/。紙舞台／9人画面はdocs/screenshots/paper_stage_roster_unity/、各動作はpaper_fryingpan_unity/も更新。OSキー送信なし。

残課題：docs/HITBOX_REVIEW_CHIZURU.mdの絵と判定の差。地上3段の先端が現判定より約13〜18%短く、上下空中技・広い必殺の範囲も要検討。性能が変わるため、今回は判定変更0件として画像と修正案を残した。灰色抽出は衣装も含み得るので自動適用しない。絵の手／部位の最終審査、追加6人の固有技と各人のポーズ、紙ヒット演出も後続。

次：ユーザーの判定画像への指摘を優先。新規制作を進める場合はcodex_tasks/PAPER_TOMA_CORE_POSES.mdに従い、透真から1人ずつガード／しゃがみ等へ。現条件と将来効果はPAPER_THEATRE_DIRECTION.md／FUTURE_SKILL_EFFECTS.md。再開時はtasklist→git status／log、本節と最新指示書を読む。採用再生成はbuild_fryingpan_runtime→update_rice_pose、実Unity合格後archive_changed_review→adopt_paper_review→確認フォルダ／一覧、finalize_hitbox_review、check_paper_sources。旧build_paper_runtime_manifestで上書きしない。Claudeへの確認点：17技の先端と緑枠、手・靴、追加6人の試作表示、コマ送りの描画同期。既存差分を破棄しない。

## 最新（2026-10-15 Codex）：紙舞台と9人一覧を検証完了

6fc196aの判定半減45件／ゲージ14・10を保持して未commitの舞台2ファイルから再開。3作品共通地形のまま平面レイヤー・厚紙の縁・テーマ別縮小、紙のタイトル／FIGHT／KO、全9人の作品別一覧と選択・試合開始を実装。追加6人は作品ごとの共通基本技＋待機1枚の試作。固有技／固有動作の完成ではない。透真／蓮の新待機はタイトル・選択に使用し、戦闘素材は旧版。将来効果の拡張位置はFUTURE_SKILL_EFFECTS.md。

今回実Unity paper_stage_roster_4：BuildAll成功／Edit466/466／Play44/44／Windowsビルド成功、失敗0・スキップ0。paper_stage_roster_1／2のコンパイル、3の看板参照／HUD重なりを修正し全工程再検証。docs/screenshots/paper_stage_roster_unity/に今回15PNG（旧3の選択1PNGも残る）。16:9・16:10・4:3の9人カードの全頂点が画面内、盤面36セルと足場／縮小同期、操作ロック／結果経路を確認。OSキー送信なし。

次はPAPER_CHIZURU_SIDESWING_AND_HITBOX_VIEW。最新指定は全通常を横振りへ。加えて飛翔おにぎりは紙版であることを確認、手持ちをその表示寸法へ合わせる。判定表示／残像／練習スロー・コマ送りと全技比較へ。現時点ではこの次工程は未実装。未commit差分は破棄しない。Claudeは本節から交代可能。

# 実装状況（2026-10-15）

## 最新区切り：全9人待機と千鶴フライパン接続（2026-10-15）

千鶴の通常を各2原画（予備→当てる）へ接続。横1段／横2段／上下の最大3段。3段目は手持ちの近接攻撃、前進／後退・発生／硬直／ヒット停止／威力は維持。カバン13原画・以前の確認コピー・旧投擲定義は予備で保護し削除していない。

今回実Unity paper_fryingpan_1：BuildAll成功／EditMode463/463／PlayMode42/42／Windowsビルド成功、失敗0・スキップ0。実Camera.Render30PNG：docs/screenshots/paper_fryingpan_unity/。確認先：イラスト確認/千鶴/01_ゲームで採用済み/の46画像。ゲーム接続と絵の最終審査を区別する。

新8人＋既存千鶴の全9人待機を保存。透真は湊との頭身差を指摘されv2へ再生成、現在の一覧も更新。共通1304×1450の透明梱包・原画画素一致。art_source/paper_roster_idle/manifest.json、イラスト確認/全9人_待機/。8人は戦闘未接続。湊の資料にない下半身は本作用仮補完。透真／蓮のガード等は未生成。

次：codex_tasks/PAPER_STAGES_AND_SCREENS.mdの共通地形・作品別紙舞台／縮小見た目／9人切り抜きタイトル／厚紙FIGHT・KOを実装。新舞台／画面はまだUnity未検証。旧結果を新差分の合格と呼ばない。Claudeは本節と指示書から交代可能。既存差分を破棄しない。

## 作業中：新規8人の待機原画を保存

全9人待機の先行制作という最新指定で新規8枚を生成、千鶴と共通1304×1450の確認9枚／一覧をイラスト確認/全9人_待機/へ。原画／全文条件／画素一致／状態はart_source/paper_roster_idle/。新規8人はゲーム未接続、体格倍率と部位審査待ち。透真／蓮のガード・しゃがみは未着手。次は既依頼の千鶴フライパン接続と実Unity検証。今回はまだUnity実行0回、未commit／未push。

## 最新：開始・決着・結果・全身動作・音を検証（2026-10-15）

実装順をdocs/IMPLEMENTATION_ORDER.mdへ固定、今回の設定はdocs/PAPER_THEATRE_DIRECTION.mdに集約。順1〜4の基本実装を完了。FIGHT!操作停止、KO!／TIME UP!、決着後6tick静止→2.5秒後に結果、勝者を左・敗者を右へ、名前／ステージ／固有勝敗絵。千鶴の引く→出す→戻る・被弾・中心での段階回転を全身Spriteだけで実装。歩き15度／走り45度固定を維持。新規の紙勝敗絵の他キャラ展開は未完。

今回実Unitypaper_theatre_2：BuildAll成功・Edit459/459・Play42/42・Windowsビルド成功、失敗0／スキップ0。初回の型名衝突を明示aliasで修正して全4工程を再実行した。アサーションを弱めていない。最新起動Build/paper_theatre_2/AniBato.exe。実Camera.Renderの30PNGをdocs/screenshots/paper_theatre_unity/へ保存し目視。OSキー送信なし。下の447／38は前回保存済み。

千鶴フライパンは横6＋上下6の比較保存完了。イラスト確認/千鶴/比較_フライパン版_未採用/に日本語名／一覧。1304×1450、靴底1328、透明余白のみで原画画素一致。頭／靴／手の絵の最終審査は未完、ゲームはカバン版のまま。variants/bag-backup.jsonで13原画を削除指示まで保護。180BPMのBGM4種／19SEも試作導入し、TestResults/arcade_audio/へ試聴WAV5件。聴感の好みは未確認。

実装コミット1587fd0をorigin mainへ通常push済み。次はPAPER_STAGE_EFFECTSの紙ヒット演出→工作舞台／縮小演出→9人集合タイトル。透真の旧制作は自動再開しない。詳細はcodex_logs/paper_theatre_2_final.mdとRESUME。

## 以下は前回完了の記録

## 最新：千鶴の新動作をゲームへ接続完了（2026-10-14）

ユーザーの「とりあえず千鶴の新しい動きをゲームに実装して」を優先し、絵の再修正待ちを解除して現参照を先行接続。千鶴の30姿勢／全35描画状態、軽被弾とハート消失の強被弾、4効果、メニュー／結果／カットインを紙イラストへ。待機は小さく揺れ、歩き15度／走り45度を移動方向に固定。空中攻撃は開始から回復まで同じ絵を保持。戦闘性能と透真／蓮、旧ピクセル素材は維持。

**今回の実Unity：BuildAll成功・EditMode447/447・PlayMode38/38・Windowsビルド成功、失敗0／スキップ0。** 証跡TestResults/paper_chizuru_all_2_*。最新起動はBuild/paper_chizuru_all_2/AniBato.exe。all_1の歩き足幅差1件を表示側で修正し、全4工程を再実行して合格。アサーションの許容値は緩めていない。

Unity実描画19PNGはdocs/screenshots/paper_chizuru_all_unity/。歩き左右、走り左右、しゃがみ／ガード／回避／反撃構え／通常／通常特殊／空中7方向を目視。実モニター撮影ではなく、実Camera.RenderとRendererの拡大描画。OSキー送信なし。保存済み旧441／36と今回447／38を区別する。

全30人物は共通PPU532.5893、共通ゲーム用透明枠1304×1450へ。原画画素を拡縮／切断せず全画素一致を実Unityで検査。元の待機1246×1263も同一バイト保持。靴を基準に足元を決めて道具と区別した。元原画自体の手／胴／部位サイズには修正残があり、絵の完成とは扱わない。詳細はruntime-manifestのartReview／artNote。

確認フォルダはイラスト確認/千鶴/01_ゲームで採用済み/に35画像、名前付き一覧／HTML／CSVも更新。すべて原画同一コピー。候補と採用の二重表示なし。修正前の旧PNGはユーザー指定で削除済み、現在原画34＋承認済み手元参考1を保持。ゲームで使用中と絵の最終審査を分けて説明する。

詳細はdocs/codex_logs/paper_chizuru_all_2_final.md、指示書PAPER_CHIZURU_INTEGRATE_NOW、対応表art_source/paper_cutout/runtime-manifest.json。実装コミット7cc23f0をorigin mainへ通常push済み。force pushなし。次はユーザーが千鶴をプレイして動きを確認できる。残る手／部位の絵の修正、漫画演出、9人の集合タイトルは後続。透真の制作は自動再開しない。Claudeも本書と上記資料から交代可能、同時編集禁止。

## 以下はゲーム接続前の履歴

最新：ユーザー承認済みの手元参考working/air_forward_left_fist.pngを、内蔵image_genで空中前方攻撃v8へ反映。掌側の4指と下の親指を目視。全身は未採用、1243×1265／角alpha0／alpha32端切れ0。共通セル1246×1263と部位倍率は未審査。前方v6／v7、斜め下v3もユーザー指摘で不採用。斜め下の背中構図／甲側の左拳は別の修正として残る。反転加工なし。

ユーザーの旧画像削除指示により、generated/は現参照34PNGだけ、承認済み手元参考1PNGはworking/に保持。旧原画41＋管理済み確認コピー41の累計削除をSHA付きで記録。確認フォルダは採用2／候補26／要修正7の35コピーとPNG一覧1枚、旧版フォルダなし。全文条件は保持、削除を自動で復元しない。対応表／審査表／実寸／HTML／CSV／説明を更新、35コピーのSHA一致を確認。以下の旧件数と保存方針は過去の履歴。

**今回Unity追加実行0回、新しい実機スクショ0件、未commit／未push。** 保存済みEdit441／Play36は待機・以前の歩きだけ。全動作接続／runtime-manifest／最後のFootBaseYコンパイル／厳密なテスト／実Unity全4工程は未完。次は全身への指摘確認と残る手／部位審査→接続／検証。詳細とClaude交代手順はdocs/RESUME.md。

## 以下は確認フォルダ初回作成時の記録

**最新：イラスト確認/千鶴/を作成。** 日本語名付きで採用済み2枚（待機／同じ絵の以前の歩き）、未採用候補27枚、要修正6枚、旧版35枚を整理。原画と同一の70コピーをSHA照合、名前と状態付きの35画像のPNG一覧を目視。HTML／説明MD／CSVも保存。HTMLのアプリ内自動表示はローカルファイル制限で拒否され実表示未検証、PNG一覧は表示確認済み。再作成スクリプトとreview-catalogを保存、複製はGit管理外。

BackAttack v8は自由な左手の白い紙縁を復元、AirForward v4は右肩から胸前を通る右腕へカバンを持ち替えた修正候補。添付の左手カバンBackAttack v2／AirForward v2は旧版に分類。候補は部位寸法／共通セル審査前でゲーム未採用。原画69PNG／全文69件を保持、source-index／hand-review更新。**今回Unity追加実行0回、新しい実機スクショ0件、未commit／未push。** 全動作接続／テストは未完了。次は画像名付きのユーザー指摘を取り込み、部位審査→manifest／Editor→実Unity4工程。

## 以下は確認フォルダ作成前の修正記録

最新の局所修正：BackAttack v7は空いた左手の親指を上側、手の甲が見える向きへ再生成。カバンは右手のまま。原画と全文条件を保存し、全身／指を目視、1246×1263・透明背景・alpha32の端切れ0を確認。部位寸法の正式審査とゲーム差し替えは未完了。v6はユーザー指摘で不採用。

紙候補は67PNG／67プロンプト、source-index／hand-review更新済み。Attack3 v6の肩と腕の所属、CrouchAttack v3の寸法と端切れ、残る全件の左右・部位確認が課題。今回のUnity追加実行0回／新スクショ0件、未commit／未push。準備コードを保持、全35状態／4効果の接続と厳密なテストは未完了。次は審査→manifest／Editor→実Unity全4工程。前の60501c4の441／36は新差分の結果として使わない。

## 以下は再開直後および停止時の記録

**2026-10-14ユーザー「再開して」で制作を再開。** 別担当なしを確認、43原画と既存差分を保持。全動作の未完部分を続行し、以下の停止時情報と新検証を区別する。

再開後の最優先：カバン右手／フライパンとおたま左手に統一、全原画の両手が同じ右手になっていないか確認。PAPER_CHIZURU_HAND_REPAIRを作成。靴修正候補3枚を追加保存、持ち手の審査前なので未採用。全件確認が済むまで新素材の接続は保留、今回のUnity検証はまだ0回。

**ユーザー指定で制作を停止。「再開して」で未完了部分から続行する。** [停止時引き継ぎ](docs/codex_logs/paper_chizuru_all_checkpoint.md)とRESUME冒頭が最新。生成・保存・実行中処理は終了済み。既存差分は保存し、破棄／リセットしていない。停止保存で新しい生成／コンパイル／Unity検証は開始していない。

完了保存：43PNG＋43件の全文プロンプト（待機1／走り3版／全28姿勢／4効果／7修正版）、実寸／SHA／透明の一覧。原画の初回生成は重複しない。SO／固定傾き／全状態の入れ物／Editor梱包／効果／結果／カットインのコードを準備したが、runtime-manifest未作成、新素材は未接続。直前のAPI参照コンパイルは警告0／エラー0、最後のFootBaseY追加は未コンパイル。

残課題：頭／左右靴等の寸法と正式採否、crouch_attack_v2／special_side_v2の1129×1393と共通1246×1263の不一致、地上／空中支点、全35状態／強被弾／4効果の接続、厳密な新テストと実Unity検証。待機だけ揺れ、歩き15度／走り45度を進行方向へ固定、空中攻撃の継続表示、涙の被弾が最新指定。新しい全差分は**未commit／未push・実Unity未検証**。

前の待機／旧揺れ歩行の完了版60501c4はorigin mainへ通常push済み、HEADと同一。BuildAll成功・Edit441/441・Play36/36・Windowsビルド成功（失敗／スキップ0）はその版だけの実結果。**停止保存でのUnity実行0回**。docs/screenshots/paper_chizuru_unity/に18PNG＋2GIF、新しい全動作／固定傾きのUnity画像はまだない。

次：サイズ／採否→manifestとEditor→テスト→サンドボックス外Unity全4工程→通常commit／push→漫画演出→集合タイトル。9人（透真・湊・シロ／蓮・結衣・黒獅子王／千鶴・暁・龍太）を手前、透真が中央、背景サポートは**同じ3作品の仲間**で確定。タイトル資料確認のみで集合絵／遷移は未生成・未実装、漫画演出も未実装。停止中の透真制作は自動再開しない。Claudeの確認点と再開手順はRESUME／チェックポイント。

## 以下は待機・歩行の完了記録

**最新：千鶴の待機/歩行だけ新しい一枚絵へ接続し、実Unity全4工程合格。** [指示書](docs/codex_tasks/PAPER_CHIZURU_UNITY.md)/[最終報告](docs/codex_logs/paper_chizuru_1_final.md)。立ち2.24u/足元支点/原画同一バイト/Bilinear、待機±0.75度/4fps、歩行±5度/8fps/上下0.025u。SO設定・固定tick Age・左右の床沈み補正・頭上追従、攻撃等への切替/ポーズ/再戦/旧表示への無効化を実装。純C#戦闘/移動/判定は変更なし、透真/蓮に紙表示なし。

今回：BuildAll成功、EditMode441/441、PlayMode36/36、Windowsビルド成功（失敗/スキップ0）。TestResults/paper_chizuru_1_*、最新Build/paper_chizuru_1、前回art3eを保持。Unity実カメラの18PNGと左右の8コマGIFをdocs/screenshots/paper_chizuru_unity/へ保存し目視、OSキー送信/実モニター撮影なし。旧千鶴/蓮のPNG/JSONは保持。原画/全文日本語条件はart_source/paper_cutout/。

この完了区切りを通常commit/pushしSHAを記録する。停止中の透真の原画/準備内容を保持し、新技の戦闘処理は未完了。残り：千鶴の他動作は旧ピクセルで混在、今は待機/歩行の体験確認用。次は揺れの所見を確認し、調整と新画風の展開を決める。無断で停止中の旧制作へ戻らない。

## 以下は初回の画風試作と停止中の制作履歴

**最新：紙の切り抜き風2Dへの画風変更を試作。** [指示書](docs/codex_tasks/PAPER_CUTOUT_PROTOTYPE.md)。ユーザー指定で千鶴1体の待機を生成。原画と全文日本語条件/実寸/SHAはart_source/paper_cutout/。本人の衣装と色、両腕両脚の見える斜め横、アニメ塗り/紙縁/透明背景を目視し、保存コピーのSHAとPNG両隅alpha0を確認。

一枚を足元支点に少し左右へ傾け、6〜8fpsで切り替える歩行案は描画側で実装可能。今回の成果は画風試作で、ゲームへ未接続・Unity検証未実行・未commit/未push・実機スクショなし。次は採用方向/共通セル/PPUを定めて歩行を具体化。旧ピクセル原画/未コミット差分/既存ゲームは保持、透真のピクセル制作は停止のまま。保存済みUnity424/424・32/32は前回ART3Eであり、この試作の検証と扱わない。

## 以下は停止中の透真ピクセル制作の記録

**現在はユーザー指定で一時停止。** 拳銃v2の6Fとドローンhover v2の4Fは素材出力/固定224×160/PPU50/旧32色/元画素一致に合格。原画4/採用2を保存、緊急上昇は指示文のみで未生成。新戦闘の接続/透真の残り動作/今回Unity検証は未完了、未commit/未push。実行中の素材処理は終了確認済み。最新の追加指示を聞いてから再開する。詳細はRESUME冒頭。

**最新の方針：透真だけを先に全動作の生成/実装/検証へ。** [指示書](docs/codex_tasks/TOMA_ALL_ACTIONS.md)。拳銃射撃/ドローン爆撃/ドローンにつかまる緊急上昇/複数ドローン自爆突撃をユーザー指定で制作、横の遮断ステップは継続。急停止/強弱被弾/方向別と空中通常/防御反撃/勝敗も透真を先に完成させる。蓮/千鶴の新画像は制作しない。

拳銃v2は4キーの頭37±2/共通立ち倍率/接地159/旧32色を合格、既存special_neutralの6Fへ出力し画素/固定サイズを確認。ドローンv1はコマ幅の変化で不採用、v2修正中。新戦闘の仕様/接続と他の未完成動作は残る。原画/日本語全文条件/採否はart_source/toma_actions/、素材プレビューdocs/art_wip/toma_actions_*、実機スクショ未撮影。**今回Unity未実行・未commit/未push。** 保存済み424/424・32/32/main0650a5aとは区別する。

## 以下は前回の完了記録

**最新：再起動後は保存時のロックが解消し、復旧できた。** [再起動後の詳細](docs/codex_logs/RESTART_CHECKPOINT.md)。欠落スクリプトをGit保存内容から復元、今後は採用原画だけのZIP作成に接続。作成済みZIP721ファイルを再照合して正式名へ置換成功、38.75MiB/採用19原画/全文90プロンプト。全90原画と旧ZIPを保持、差分の破棄/リセットなし。両スクリプトはWindows PowerShell5.1で構文確認成功。

実装と実Unity全4工程は合格済み。今回は保存済みEditMode424/424・PlayMode32/32・BuildAll/Windowsビルド成功を確認しただけで再実行していない。実機スクショ未保存、素材プレビューはdocs/art_wip/art3e_*。

**GitHub反映完了：77c526d（2026-10-14）。** ユーザー明示承認で未公開2097c99だけを作り直し、origin mainへ通常push成功、GitHub実mainの同一SHAを確認。元コミットはローカル保全、作業ファイル3169件のSHAを保持、送信対象の容量超過/個人パス/検査対象の秘密情報なし。公開済み182e970以前の履歴を保持、force push/ファイル破棄/全体リセットなし。ゲーム/素材/テストの変更は合格時から改行のみ。この反映記録は通常の追加コミットで保存する。残りはART3F急停止/切り返し→ART3G強弱被弾画像、後退の戦闘処理は既に完了。次はART3Fの具体仕様を指示書へ追記して着手、Claudeへ交代可能。

## 以下は再起動前の記録（最新は上記）

**最新：ART3Eの素材/実装/実Unity全4工程が完了。** [最終報告](docs/codex_logs/art3e_final.md)。歩き/走り/通常15クリップ102Fと投げ4F、全107クリップ588F。透真右/左/右、蓮跳び込み頭突き/頭支持カポエイラ/腕押し両足蹴り、千鶴右手二撃/投げを接続。通常前進0.18・被弾追加反動0.18/6tick・実ハート消失フラグ・SO移行は実装済み。

実Unity BuildAll成功・EditMode424/424・PlayMode32/32・Windowsビルド成功（失敗/スキップ0）、純C#375/375、895ファイル再生成一致/91PNG保持、ZIP792ファイル照合。最新Build/art3e、前回art3dを保持。実機撮影はキー送信前に中止しPNG未保存、素材プレビューはdocs/art_wip/art3e_*。

残り：ART3F急停止/切り返し→ART3G軽傷/ハート消失の痛い被弾の原画/表示。後退の純C#は完了。原画90/採用19/全文条件/参考/再制作キット/証跡からClaudeへ交代可能。完了差分はこれから正常commit/pushし反映記録を追記。

## 以下の未完了/未検証という記載は制作途中の履歴

ユーザーの再開指示で続行。最新は[再開時引き継ぎ](docs/codex_logs/art3e_checkpoint_20261013.md)とRESUME冒頭。未commit/未push、HEAD/origin main 182e970。

- 歩き/走り6クリップ48Fは接続済み。通常5原画（透真3段/蓮attack1 v13/千鶴attack1 v3）は正規化合格・未接続。残る4攻撃/投げカバンは未完成。
- 全通常の前進0.18/回、CPUの射程/足場端予測、実ヒットの追加後退0.18/6tick、実ハート消失フラグとSOの新フィールド移行は実装済み。既存Knockback/攻撃時間を保持。
- 実Unity：通常前進EditMode28/28・被弾後退21/21、BuildAll単独成功。純C#366/366。停止/再開時は保存済み証跡確認のみ。**ART3E全EditMode/PlayMode/Windowsビルド未実行、全体未完了。**
- 残課題：千鶴2段目の頭46px/二撃の体ひねり、蓮頭支持蹴り/両足蹴り、千鶴投げ/飛び道具、全通常のゲーム接続。続いてART3F急停止とART3G強弱被弾（画像は全攻撃生成後）。
- 実機スクショ未撮影。素材はdocs/art_wip/art3e_*、ビルド/ZIPは前回ART3D。79原画/全文条件/採用14件を保存。最新動作参考はGit管理外の_motion_localに保存、ClaudeはRESUMEから交代可能。

## 以下は前の制作途中の履歴（最新は上記）

最新：全3キャラの通常（地上/空中/方向別）に1回0.18の実前進を追加。発生＋持続へ均等配布、3段0.54、硬直/ヒット停止は停止。既存空中ActiveVelocityへ加算し、着地硬直による中断時の横流れを修正。SO移行は新フィールドだけ一度設定、CPU射程/足場端/連係も前進分を予測。純C#345/345・API参照コンパイル警告0/エラー0、**今回実行したUnity EditModeの通常前進28/28合格**（TestResults/normaladvance_EditMode.xml）。ART3E全体のBuildAll/全EditMode/PlayMode/Windowsビルドはまだ未実行、未commit/未push。

歩き/走り6クリップ48Fは接続・素材検査済み。透真の動的通常3原画は正規化合格で未接続。蓮は最終指定の跳び込み頭突き→頭を床につけるカポエイラ→両腕押し出し両足蹴りを制作中。頭突き候補v2〜v6は未採用（頭寸法/複雑姿勢の検査待ち）、採用選択はnullへ戻す。頭部の範囲をマント/腕と区別し、倍率や部分サイズの基準は保持する。千鶴の攻撃時だけカバン3段、ART3F急停止/切り返しも残る。次は通常素材を全検査/接続→キット→Unity全4工程→記録/通常commit/push→ART3F。実機スクショは今回未撮影、素材ボードはdocs/art_wip/art3e_*。Claudeは最新指示書・source-selection/pending-review・検証XMLから再開できる。

## 以下は制作途中の履歴（最新指定は上記）

最終指定：蓮＝跳び込み頭突き/頭支持カポエイラ/腕押し前方両足蹴り。全キャラの通常で実位置を小さく前進（純C#/データ化）も追加。前の蓮案と「移動性能維持」はこの要求分のみ上書き。透真の動的3原画は採用・未接続、千鶴未制作、Unity未実行。

最終の蓮指定：前進頭突き→頭を床につけた右蹴り→左蹴りへ制作案を変更。透真は右/左パンチ/右キックの動的原画3枚を採用・頭/倍率合格（ゲーム未接続）。蓮の旧パンチ候補は保持して不採用。詳細条件は指示書の最新追記。Unity未実行。

追加依頼：通常は右パンチ→左パンチ→右キック（透真/蓮）へ、腰/肩/体軸の移動を加えて再制作。透真は通常中端末をしまう。ART3E後は走りの急停止/切り返し[ART3F](docs/codex_tasks/ART3F_RUN_BRAKE.md)も生成/導入。今回Unityは未実行。

ユーザーの再開指示で続行。同時編集なしを確認。歩き24枚に続き、走り3人も接続・全身/腕/足の拡大確認・素材検査合格（6クリップ48F）。採用dashは透真v3/蓮v7/千鶴v2、頭部±2/接地159/浮遊4〜32px/元画素/GIF/32色、旧回帰と対象外91PNG一致。dashの既存ActiveFrame1も保持。次は通常9原画、Unity全4工程は未実行。未コミット/未push。以下の03:56中断状態は履歴。

ユーザーがPCを切るため03:56 JSTで制作/検証を停止。今回の差分はローカル保存済み、未コミット/未push。HEAD main 182e970。**今回Unity全4工程は未実行**。詳細は[中断引き継ぎ](docs/codex_logs/art3e_checkpoint.md)。

- 完了：3人の歩き8枚/人をゲームへ接続、左右の腕の逆振りと透真の中央の持ち手を修正。基本toma v5/ren v7/chizuru v4、間toma v6/ren v5/chizuru v3。全30キーの頭部±2px、24枚の非重複/画素/接地/alpha/32色/GIF、既存動作回帰と対象外91PNG一致に合格。
- 途中：透真dash v3/千鶴dash v2は正規化合格、走りのゲーム接続は未完。蓮dash v4は外周/最後の左腕に問題、未採用。selectionのren/dashはnullとして誤採用を防止。
- 未着手：通常9原画。透真/蓮パンチ→パンチ→キック、千鶴は攻撃中だけカバン3段。次は蓮走り修正→走り確認→通常→15クリップ102Fの接続→キット→Unity全4工程。
- 検証：純C#317/317、UnityAPI参照コンパイル警告0/エラー0。新しい浮遊/通常のUnityアサーションは素材完成後に実行する。旧dashは現ゲームに残っているため今のまま全合格扱いしない。
- 画像：[歩きGIF/全8枚ボード](docs/art_wip/art3e_previews.md)。実機スクショ未撮影、現在のBuild/art3dは前回版。
- Claudeへ：AGENTS→HANDOFF→RESUME→[指示書](docs/codex_tasks/ART3_WALK_ATTACKS.md)→[中断記録](docs/codex_logs/art3e_checkpoint.md)。原画39PNGと全文プロンプト/採否/倍率/次の生成条件を保存。会話の一時変数に依存せず再開可能。

## 前回の完了：ART3D基本動作・地上ジャンプ予備

## 最新作業：ジャンプ・ガード・回避・ダッシュ＋浅い予備/着地

[ART3_ACTIONS](docs/codex_tasks/ART3_ACTIONS.md)を先に作成して全3人の新動作を順番に生成・確認し、18クリップ93Fへ接続。固定224×160/PPU50、立ち由来の倍率、頭部帯幅±2px以内、靴/関節も目視確認。採用15原画＋不採用4原画を保存。全106クリップ584F、既存ID/Timing/待機/しゃがみを維持。

- **Unity：BuildAll成功・EditMode357/357・PlayMode30/30・Windowsビルド成功。失敗/スキップ0。** TestResults/art3d_*、2026-10-13 01:53:43/01:55:04 JST終了。初回のschema期待値と追加テストの入力反映待ちを修正して全4工程再合格、旧アサーション維持。
- ユーザーが選んだ地上2tick予備→同初速で踏切、空中2段即時。schema7/GroundJumpStartupFrames=2、新状態は既存landへ接続。スタミナ1度消費・中断払い戻しなし、入力保持、被弾/足場/場外/ヒット停止を検査。着地8tickは浅いかがみ4tick→採用待機4tick。
- 純C#317/317、API参照コンパイル警告/エラー0。素材93F＋待機/しゃがみ回帰、665ファイル再生成一致。再制作指示書§10/キット更新、ZIP341ファイル照合合格。[出力](art_source/art3_actions/README.md)/[プレビュー](docs/art_wip/art3d_previews.md)/[最終報告](docs/codex_logs/art3d_final.md)。
- 最新起動Build/art3d/AniBato.exe、保持art3d/art3c。実機撮影は最前面確保できずキー送信前に停止、[スクショ未保存](docs/screenshots/art3d_previews.md)。Windows目視は残る。旧TestResults111件を容量管理で整理、dotnet類/過去の最終報告は保持。
- 実装/素材コミット**52de881**（`Add fixed-size action sprites and two-tick jump anticipation`）をorigin mainへ通常push済み。HEAD/origin/main一致・clean確認後、この資料の反映記録だけを追記。Claude交代はRESUME冒頭/HANDOFF§4から。次の候補は実機確認、walk/攻撃/勝敗等の新絵柄化、6B手動確認。次の指示書を先に書く。

### ART3D制作中の履歴（以下は当時の状態）

- 途中の区切り：全4動作＋浅い予備/着地、採用15原画＋不採用4原画を保存。51キーの立ち倍率/224×160/頭部帯幅±2px検査・目視確認。18クリップ93Fを接続、全106クリップ584Fを保持。原画/全文プロンプト/採用選択/共通倍率/実装用PNGはart_source/art3_actions/。665ファイル再生成SHA-256一致、待機/しゃがみ回帰合格。再制作指示書§10とZIP更新、339ファイル照合合格。
- 地上JumpStartupを実装：2tick接地した浅いかがみ→同じ初速で踏切。空中2段即時、予備開始でスタミナ1度消費、中断は払い戻さない。新行動のバッファを保持。着地8tickはかがみ4tick→待機4tick。純C#317/317、API参照コンパイル警告/エラー0。Unity初回はBuildAll/ビルド成功、Edit356/357・Play29/30。旧schema6固定期待を7+予備2の検査へ修正、追加Playテストを実入力イベント反映待ちへ修正。該当テスト確認後、全4工程を再実行する。まだ全合格/commit前。ZIPは原画寸法/採否記録追加後340ファイル照合合格。

## ART3C完了時の履歴：3人の新しゃがみと持ち出し用生成キット

[ART3_CROUCH](docs/codex_tasks/ART3_CROUCH.md)を作成してから内蔵image_genで同倍率の立ち/しゃがみ原画を各1枚生成。立ちの実高さを物差しにしてしゃがみへ同じ倍率を適用し、頭/靴のサイズを保持。1枚から整数行移動で8Fの体呼吸を作り、crouchへ接続しました。

- **全員224×160px/コマ・8F/6fps・足元159・PPU50**。中立しゃがみの実高さは透真91/蓮77/千鶴84px。顔/衣装の画素と靴の下12行を固定し、体/頭±2px。新待機と他モーションのPNG・Timing/スプライトIDを保持。
- [実装用素材](art_source/art3_crouch/README.md)：各8枚・1792×160横シート・JSON・等倍GIF、原画/全文プロンプト/分割矩形/倍率を保存。[動くプレビューと同倍率比較PNG](docs/art_wip/art3c_previews.md)を目視確認。
- [詳細な再制作指示書](docs/SPRITE_GENERATION_GUIDE.md)と[持ち出しZIP](art_source/sprite_generation_kit.zip)に参照9PNG・原画3PNG・パレット・プロンプト・規格・採用済み待機/しゃがみを同梱。今後も同じセットで別環境/Claudeへ渡せる。
- **Unity BuildAll成功・EditMode326/326・PlayMode29/29・Windowsビルド成功、失敗/スキップ0**。TestResults/art3c_*。新しゃがみのCrouch状態接続/8F/ループ/倍率/足と顔の画素保持を検査する3ケース追加、既存アサーション維持。
- 素材24F・既存待機24F回帰合格、全106クリップ584F、**425ファイル再生成SHA-256一致**。ZIP内84ファイル照合、Unity API参照コンパイル警告/エラー0。メモリマップ中のGIF書き換え失敗を修正し、同一内容/変更内容の保存と復号を確認。
- 起動：**Build/art3c/AniBato.exe**。保持ビルドはart3c/art3b。[実機撮影](docs/screenshots/art3c_previews.md)は最前面確保に失敗しスクショ未保存。Windows版での新しゃがみ目視確認は未了。プレビューを実画面として扱わない。
- [完了報告](docs/codex_logs/art3c_final.md)、[RESUME](docs/RESUME.md)とHANDOFFを更新。コミット名`Add fixed-size crouch sprites and portable generation guide`、反映先origin main。Claude交代時はtasklist/git statusから確認。
- 次：実機のしゃがみ確認、またはwalk/dash/攻撃の新絵柄化。新しい指示書を先に作成。物理パッド/144Hz等の手動確認も残る。
- 大きさの追加確認：待機との頭部帯幅差は0〜1px、靴の下8行の幅差は0〜3px。等倍/3倍で頭/靴を目視確認し、部分基準と測定記録を再制作指示書/キットへ追加。CompareArt3Proportions.ps1を再生成/キット作成にも組み込み、追加後も425ファイル一致。

## ART3B完了時の履歴：3人の呼吸待機をゲームへ接続

[ART3_IDLE](docs/codex_tasks/ART3_IDLE.md)で生成した透真・蓮・千鶴の3枚を採用。[ART3_BREATHING](docs/codex_tasks/ART3_BREATHING.md)を作成してから呼吸キーを内蔵image_genで各4姿勢生成・保存した。最終8コマは静止原画から整数行移動で作り、顔・衣装の細部を保持。体と頭が上下±2px、靴は固定、8F/6fpsでループする。

- **今後の全キャラ・全モーション：224×160px/コマ、足元y159、PPU50。** 身長は透真124px／蓮100px／千鶴112pxを基準に維持。[制作規格](docs/SPRITE_SPEC.md)をAGENTS・SpriteForge・Unityインポータへ反映した。
- [実装用出力](art_source/art3_idle/breathing/README.md)：全員の透過PNG8枚、1792×160の横シート、JSON、GIF。ゲーム用idle・AnimationSet・Title/Battleを更新。旧rawの縮尺・パレットを保持し、他の動作のPNGは変更なし。
- 素材検査：新idle24F、靴の完全固定・胴の上下動・顔/衣装の画素移動・二値alpha・32色以内・実頭頂高さ・GIF復号が合格。全106クリップ584F、**388ファイル再生成SHA-256一致**。
- **Unity実行：BuildAll成功、EditMode323/323、PlayMode29/29、失敗・スキップ0、Windowsビルド成功**。TestResults/art3b_*。既存テストを維持し、3人のサイズ/体呼吸/足固定/顔の画素保持を検査する3ケースを追加。API参照コンパイルも警告・エラー0。
- 起動：**Build/art3b/AniBato.exe**。最新2つはart3b・phase6b。外部承認で全4工程を実行し、Title/Battle両シーンと実行ファイルの実在を確認。
- 実画面：[art3b撮影一覧](docs/screenshots/art3b_previews.md)。事前に「PCに触らないで」と案内し、2人対戦で全3人の新idleと頭上カウンター・接地を目視確認。撮影スクリプトはゲームのクライアント領域のみ保存する。
- 交代：Codex/Claudeとも[RESUME](docs/RESUME.md)冒頭と[最終報告](docs/codex_logs/art3b_final.md)から再開。完了一式のコミット名は`Add fixed-size breathing idle sprites for three fighters`、反映先origin main。交代時git log/statusで確認する。
- 残課題：移動・攻撃などは既存の絵柄。新待機と同じ絵柄へ描き直す場合は次の指示書を先に作り、同じセル/倍率で進める。物理パッド・144Hzなど6Bの手動確認項目も履歴通り残る。

## 前回の区切り：6B完了記録

[PHASE6B](docs/codex_tasks/PHASE6B.md)は完了済み。**BuildAll成功・EditMode320/320・PlayMode29/29・Windowsビルド成功**を保存済み証跡で確認。実装コミット **9560ed4**（6A・6B）はGitHub origin mainに反映済み。ユーザー指定によりCodexが実装・レビュー・Unity検証・取りまとめを担当し、Claudeへも随時交代できる。

前回は[PHASE6B_CLOSEOUT](docs/codex_tasks/PHASE6B_CLOSEOUT.md)に沿って完了記録・担当分担を更新し、67a8d9cをorigin mainへpush済み。コード・ゲーム用アセット・テストは変更せず、Unityを再実行していない。6Aの305/305・24/26は過去履歴であり、6B合格結果は上記。

## 6Bの変更

- PlayMode失敗のDualSense入力を、破棄されるDeltaStateEventから**実際のUSB HID全状態レポート**へ変更。×・□・OPTIONS／A・X／Spaceの旧アサーションを維持。USBの全8行動入力と中立スティックも追加検査する。
- デバイスは個別try/finallyとUnityTearDownで削除。旧Flow／Runner／Inputを止めてからfixtureを復元し、次のSetUpでも旧シーンを先に停止。スティック・十字キーは入力更新番号ごとに一度読み、メニューと戦闘で共有。DualSense表記と後続CPU選択の旧失敗2件を含め、6BのPlayMode全29件が合格した。
- 内蔵image_genで背景3枚と中割り原画3アトラス（6姿勢/人）を生成。背景・サムネ6PNGを接続し、遠景・中景・床を分離。推し駒の6×6セルと外側消失、AI警告、文化祭の消灯・閉場表示を戦闘状態から描画する。原画と最終プロンプトは[phase6b](art_source/phase6b/generation-log.md)。すべて本作独自表現。
- walkに2、dashに4の**描き下ろし姿勢**を各人追加。旧原画は保持し、専用のraw素材とmanifestへ接続。前後コマの混合なし。各8F・15fps、全106クリップ584Fの構成を維持。低優先度のidleの目元・模様の描き直しは行っていない。
- カメラは表示専用CameraFraming。開始は場外ライン2u内側まで、対戦中は上足場・両者、上足場消失後は残存床と両者へ寄る。比率変更時にも再計算し、追従中の縮尺拡大は必要範囲を即確保。地形・Hurtbox・60Hz処理を変更しない。
- 左右2回ダッシュON/OFF（既定ON、両者共通）をメインメニュー・ポーズ・キーコンフィグへ追加。PlayerPrefs保存、説明・技表・チュートリアルへ反映。OFFでも専用ボタン有効。設定はFrameInputへ含め、ラッチ・追いつきtick・リプレイでも保持。OFF時に古い自動ダッシュ予約と1回目の入力履歴を破棄する。
- ステージ選択は左右の割り当てを表示し、上下でも選択可と明記。既存画面の操作表記・停止・設定行の対応を確認。README、ルール、調整値、構造・拡張資料を更新。

## 6Bの検証

| 検証 | 結果 / 証跡 |
|---|---|
| 純C# NUnit | **307成功・0失敗・0スキップ**。既存292＋新規15ケース。TestResults/phase6b-dotnet-battle.xml |
| Editor／全テストのUnity API参照コンパイル | **警告0・エラー0**。phase6b-dotnet-unity-compile.log |
| Player参照コンパイル | **警告0・エラー0**。phase6b-dotnet-player-compile.log |
| asmdef | **既存6件の参照検査合格、新規なし**。両テストのPresentation参照を維持 |
| CPU | **810試合、両者攻撃・ハート減少、自滅近似0**。ふつう48.09秒・縮小23.7%・超必殺65.6%。phase6b-cpu-final.*。純C#既定値であり実SO集計ではない |
| 描画比率 | 1920×1080・1920×1200・1440×1080、開始/対戦/縮小/旧カメラ計12ケース。phase6b-camera-frames.json。包含テスト合格、実GPU未確認 |
| 素材 | 全106GIF・584F、二値アルファ・32色・足元・控えめな結果姿勢・再生成一致を検査。phase6b-animation-verification.* |
| Unity BuildAll | **成功（exit 0）**。phase6b_runner.txt・phase6b_buildall.log。Title.unity/Battle.unityの実在も確認 |
| Unity EditMode | **320成功・0失敗・0スキップ**。TestResults/phase6b_EditMode.xml（2026-10-12 21:14 JST終了） |
| Unity PlayMode | **29成功・0失敗・0スキップ**。TestResults/phase6b_PlayMode.xml（同21:15 JST終了）。旧失敗2件とUSB/設定/カメラ回帰を含む |
| Windowsビルド | **成功（exit 0・Success）**。phase6b_build.log・Build/phase6b/AniBato.exe。phase6a/phase6bの2つを保持 |

## 原本12章：必須テスト1〜24の対応表

下表の自動テストは6B Unity EditMode320/320・PlayMode29/29で合格。物理DualSense・実GPUの手動所見や6B実画面撮影の記録は今回確認できていないため、自動テストの合格と分けて記載する。テストの削除・アサーション緩和は行っていない。

| # | 原本項目 / 現行検査 | 自動テスト名 | Unity・実機確認 | ユーザー決定による旧→新 |
|---:|---|---|---|---|
| 1 | 通常1回＝1/3 | DamageTests.ThreeSeparateNormalsAccumulateAndBreakOneHeart（1回目） | 6B EditMode合格 / 手動所見は記録未確認 | 変更なし |
| 2 | 通常2回＝2/3 | 同テスト（2回目） | 同上 | 変更なし |
| 3 | 通常3回＝ハート−1・0/3 | 同テスト（3回目）、QueuedThreeAttackChainUsesUniqueIdsAndEnds | 同上 | 変更なし |
| 4 | コンボ切れ後も蓄積保持 | DamageTests.AccumulationPersistsThroughTimeMoveJumpAndGuardInput | 同上 | 変更なし |
| 5 | 特殊後も通常蓄積保持 | PhaseTwoAttackTests.Spec05_SpecialLosesOneHeartAndKeepsNormalAccumulation | 同上 | 変更なし |
| 6 | 3コンボ吹っ飛び・追撃禁止 | PhaseTwoAttackTests.Spec06_ThreeHitComboFinishesAndBlocksSameAttackerThenExpires、TempoTests.ThreeNormalsBreakOneHeartAndComboLimitStillHoldsAfterRecovery | 同上 | 変更なし |
| 7 | 通常ガード・しゃがみ崩し・不足時ブレイク | DefenseTests.UserDecision_NormalIsBlockedByBothFreshAndOldGuard、PhaseSixBattleTests.OrdinaryGuardBlocksEveryKindAndSpendsExactCost／CrouchBreaksEvenFreshGuardAndAddsExactlyOne | 同上、実機でスタミナと蓄積を確認 | **2026-10-12：通常貫通→全攻撃ガード＋消費。しゃがみはジャストも崩し蓄積1** |
| 8 | 特殊ガード | DefenseTests.Spec08_SpecialIsGuardedWithDataDrivenStunAndPush、PhaseSixBattleTests.OrdinaryGuardBlocksEveryKindAndSpendsExactCost | 同上 | 防御可を保持、スタミナ消費を追加 |
| 9 | 近距離通常へカウンター同tick優先 | DefenseTests.Spec09_CounterWinsSameTickInEitherPlayerOrder | 同上 | 変更なし |
| 10 | カウンター失敗の隙 | DefenseTests.Spec10_CounterMissHasFullRecoveryAndCooldown | 同上 | 変更なし |
| 11 | カウンターで蓄積1 | DefenseTests.Spec11_CounterBreaksAtThirdAccumulatedHitAndInvokesHookOnce | 同上 | 変更なし |
| 12 | 超必殺ハート2 | PhaseTwoAttackTests.Spec12_UltimateCosts100LosesTwoHeartsAndPreservesAccumulation、CharacterTests.UltimateCosts100StartsOneCutInAndLosesTwoHeartsOnce | 同上 | 変更なし |
| 13 | 超必殺ガードの十分/不足 | DefenseTests.UserDecision_UltimateIsGuardedButPiercesNormalDodge、PhaseSixBattleTests.EmptyStaminaBreaksNonUltimateWithoutDamageAndUltimateHitsTwo | 同上、実機で65以上/未満を確認 | **2026-10-12：通常ガード不可→必要スタミナがあれば防御、不足時のみハート2貫通** |
| 14 | 超必殺ジャストガード/回避 | DefenseTests.Spec14_JustDefenseAvoidsUltimateAndRewardsGauge、PhaseSixBattleTests.JustGuardWithEmptyStaminaStillBlocksUltimateWithoutCost | 同上 | 防御可能を保持。ジャストガード成立消費0 |
| 15 | 場外即敗北 | PhaseTwoAttackTests.Spec15_KnockbackCrossingBlastLineLosesImmediately、MatchTests.RingOutHasPriorityOverHeartKOAndTimeUp | 6B EditMode／RingOutResult PlayMode合格 | 変更なし |
| 16 | 70秒＝4200tick | MatchTests.Exactly4200TicksIs70SecondsAndFinishedMatchDoesNotAdvance | 6B EditMode合格 / 手動所見は記録未確認 | カットイン/ポーズを停止、ヒットストップは時計進行を保持 |
| 17 | 残り10秒縮小 | StageShrinkTests.WarningAt15SecondsAndStartAtExactly10Seconds／EveryRemovalHasFullWarningAndFinalShapeIsOnlyCentral30Percent | 6B EditMode合格 / 背景演出の手動所見は記録未確認 | 変更なし |
| 18 | 0秒の正しい勝敗 | MatchTests.TimeUpUsesHeartsThenFewerAccumulatedHitsThenDraw、StageShrinkTests.TimeUpComparesHeartsThenAccumulationWithShrinkEnabled | 同上 | ハート→蓄積→引分の暫定規則を保持 |
| 19 | 3キャラ操作可能 | CharacterTests.EachCharacterHasIndependentValidMovesAndGroundAirNormals、BattleSceneTests.Spec19_AllThreeCharactersMoveAndUseNormalsAndSpecialsOnBothPlayerSlots | 6B EditMode/PlayMode合格 | 方向通常・スタミナを追加。歩行等の旧検査を維持 |
| 20 | CPU対戦成立 | CpuTests.CpuMatchesAttackLoseHeartsAndFinishAcrossRoster、GameFlowSceneTests.TitleCpuBattleAdvancesToResultWithAudioAndEffectsEnabled | 6B EditMode/PlayMode合格。CpuSelectionも合格 | 新ガード/スタミナへCPUを追従（6A） |
| 21 | ローカル2人成立 | BattleSceneTests.TwoKeyboardPlayersMoveIndependentlyAndRRestartsFinishedMatch、GameFlowSceneTests.TitleToLocalBattleUsesSelectedCharactersAndStage／GamepadsNavigateBothCharacterSlotsAndStartPausesBattle | 6B PlayMode合格 / 物理パッド2台の記録未確認 | 変更なし、キーコンフィグとダッシュOFFを追加 |
| 22 | リザルト再戦 | GameFlowSceneTests.ResultRematchResetsEntireMatchAndKeepsSelection | 6B PlayMode合格 | 変更なし |
| 23 | 頭上カウンター追従 | BattleSceneTests.HeadCountersFollowMovingFighters／GeneratedSceneRunsAndCreatesBothFightersAndHeadCounters | 6B PlayMode合格 / 3比率・144Hz手動所見は記録未確認 | 位置補間を5Bから保持 |
| 24 | 多重ヒット防止・高FPS | DamageTests.SameAttackOverlappingManyTicksOnlyHitsOnce、InputAndClockTests.PhaseTwoCombatAndEventsAreIdenticalAcrossRenderPartitions、PhaseSixBattleTests.StaminaAndGuardEventsAreIdenticalAcrossRenderRates、PhaseSixBTests.DashOptionSwitchesRemainDeterministicAcrossRenderRates | 6B EditMode決定性・144Hz描画PlayMode合格 / 実GPU手動所見は記録未確認 | インスタンスID・対象集合とtick契約を保持 |

## 原本15章：成果物12項目

| # | 成果物 | 実装状態 / 残り |
|---:|---|---|
| 1 | Unityプロジェクト | AniBato/、Unity6000.6.0f1。6BのBuildAll・生成シーン検査合格、Title/Battle実在 |
| 2 | 戦闘システム | 純C#60Hz、自前AABB、攻防・スタミナ・勝敗を実装。6B Unity回帰合格 |
| 3 | 初期3キャラ | 透真・蓮・千鶴、方向通常・特殊・超必殺・106クリップ。6B自動検証合格、新中割りの手動所見は記録未確認 |
| 4 | 初期3ステージ | 共通地形、縮小、背景/サムネ。6B自動検証合格、実画面と消失演出の手動所見は記録未確認 |
| 5 | CPU対戦 | 3難易度・seed再現・810試合。.NET既定値確認済み、実SO集計待ち |
| 6 | ローカル2人 | キーボード/パッド個別、再接続予約。6B自動検証合格、物理パッド2台の記録未確認 |
| 7 | UI・対戦演出 | 選択・ポーズ・結果・頭上UI・音・補間。6B自動検証合格、比率/144Hz/音の手動所見は記録未確認 |
| 8 | Windowsビルド | **Build/phase6b/AniBato.exe生成・ビルド成功**。phase6a/phase6bの最新2つを保持 |
| 9 | 操作説明 | 機器表記・再割当・練習・14課題・技表・ダッシュ設定。全14課題の実機通し記録未確認 |
| 10 | 実装済み/未実装一覧 | 本書。サポート・追加6キャラ・時間帯差分・2対2/4人・オンライン等は未実装 |
| 11 | テスト結果 | TestResults/phase6b_EditMode.xml（320/320）・PlayMode.xml（29/29）・各.log。別に.NET証跡も保持 |
| 12 | 拡張設計資料 | docs/ARCHITECTURE.mdのキャラ追加・サポート・時間帯差分・入力/オンライン分離を更新 |

## 改善候補・手動確認の記録・Claudeへの交代

- **カメラ要実機調整**：開始の全体表示と近距離拡大を段階化した。全場外ライン内側を常時表示しつつ全比率で従来よりキャラを大きくする要望は、地形・絵の比率を維持する限り同時には満たせない。現在は上足場の収まりと縮小後の拡大を優先。1080p中央の透真は旧20.7%→対戦17.4%→縮小23.8%、蓮16.7%→14.0%→19.2%。対戦中の全体常時表示・常時拡大は未達として扱う。
- スクショは[phase6b一覧](docs/screenshots/phase6b_previews.md)。6枚のサムネbefore/afterと12枚の**オフライン投影図**で代替し、Unityの実画面とは扱わない。背景中景・HUD・消失・追従の時間挙動は実機で確認する。
- 今後のUnity検証はCodexがサンドボックス外実行を承認申請し、tools/verify/run_unity.ps1 -Tag <作業名>で実行。今回の資料更新では再実行していない。6Bの既存全合格を再検証待ちへ戻さない。
- Claudeへ交代する際はdocs/RESUME.md冒頭から再開。同時編集を避け、git status・指示書・証跡を確認する。手動確認の記録が今回見つからなかった対象は、実DualSense/2台再接続、3比率・144Hzのカメラ/HUD・縮小・新中割り、全14課題、音、実SO810試合集計。確認できた所見を追記する。
- 次はユーザーの実プレイ所見から具体的な修正を選び、指示書を作ってから着手。低優先度idleの目元/柄、サポート・追加6キャラ・時間帯差分・多人数・オンライン・正式音源は改善候補/未実装。

---

## 以下はフェーズ6A提出時の履歴（Unity結果は6B冒頭を参照）

[PHASE6A](docs/codex_tasks/PHASE6A.md)の冒頭「追加のユーザー決定」を反映し、ソース・素材・オフライン検証を完了。**6AのUnity BuildAll／EditMode／PlayMode／Windowsビルド／実機は未実行、Claudeへ引き継ぐ。commit・pushなし。** 5BはClaudeがBuildAll成功・EditMode243/243・PlayMode23/23・Windowsビルド成功を確認済み。

## 実装済み

- 純C#の方向通常27技（3人×地上足払い/背面/空中7方向）。Kind=Normal、近距離カウンター対象、蓄積・3ヒット・コンボ上限・インスタンスIDを共有。空中の前/斜めは移動を伴い、背面の判定/吹っ飛びは実際の打撃方向へ向く。空中の向き固定、データ化した斜め閾値/未登録時フォールバック、下系の着地隙。
- しゃがみ：足位置固定でHurtbox高さ55%、移動不可。上足場から降りるのは下18F長押し。単純な下/しゃがみ攻撃との衝突を回避。
- 整数単位のStaminaSystem。各キャラの最大/回復/遅延/行動消費/ガード費用。拒否時のHUD赤点滅。地上ジャンプと上復帰の救済を実際の着地までテスト。
- ユーザー決定のガード：全種類防御、保持/成立で消費、不足した通常/特殊は無被弾ブレイク、超必殺は貫通。ジャスト成立は消費0。しゃがみはジャストも崩して蓄積1、18F硬直・36Fガード不能。GuardBrokenイベント、HUD・SE。
- 練習（棒立ち/ガード/ジャンプ/ふつうCPU）、ハート/時間の個別固定、場外の練習専用救済、入力/最後の技表示。14段階チュートリアル、2ページ技表、キャラ選択/ポーズから起動。
- DualSense/PlayStation・Xbox・キーボードの表記切替、現在の割り当てに追従。キーコンフィグはP1/P2・機器別、変更/初期化/ローカル保存/重複入替。旧AlternateはPrimary変更時に無効、初期化で復元。対戦ヒントは開始5秒のみでOFF可。
- CPUの通常ガード/しゃがみ崩し/スタミナ・上復帰温存、向き固定に合わせた突進安全判定。旧向きで蓮の構えから場外へ飛ぶケースと復帰資源を使い切るケースを回帰テスト化。
- image_genで方向原画30キー、SpriteForgeで新規10クリップ/人。旧14主要クリップ/人の混合を廃止、綺麗なキー保持で再生。Win/Loseは単一idle原画を基準に胴/足固定・頭1〜2px。計106シート584F、106GIF。
- BuildAllのSchema6移行・方向AttackData27個生成・新アニメ接続。既存数値・独自調整・Timing・NewScene前判定/後の再読込を維持。新asmdefなし。

## 検証結果

| 検証 | 結果 | 証跡 |
|---|---|---|
| 純C# NUnit・全既存回帰＋新規62ケース | **292成功／0失敗／0スキップ** | TestResults/phase6a-dotnet-battle.xml・.log |
| Game/Editor/全テストのUnity API参照コンパイル | 警告0／エラー0 | phase6a-dotnet-unity-compile.log |
| UNITY_EDITORなしのプレイヤー参照コンパイル | 警告0／エラー0 | phase6a-dotnet-player-compile.log |
| asmdef参照 | 既存6件合格、新規なし | phase6a-assembly-references.json |
| CPU全9組×30seed×3難易度 | **810試合、全試合両者攻撃/ハート減少、自滅近似0** | phase6a-cpu-final.csv・docs/CPU_BALANCE_REPORT.md |
| 30/60/144/240Hz決定性 | 状態/スタミナ/イベント一致 | PhaseSixBattleTests・TempoTests |
| 素材・GIF・再現性 | **584F、106GIF全復号、351ファイルSHA-256一致** | phase6a-animation-verification.json・.log |
| 6AのUnity実行・実機 | **未実行** | Claude担当。EditMode305/PlayMode26は想定件数のみ |

旧ルールの検査を新しい具体的な期待値へ置換し、既存テストの削除・アサーション緩和はなし。[置換一覧と必須24項目](docs/PHASE6A_TEST_CHANGES.md)。PlayModeには実DualSense/XInputテストデバイス、設定UI→保存→初期化、練習/技表停止の3件を追加。入力分離/復元の既存構造を維持。

| 難易度 | 平均決着秒 | 縮小 | 超必殺あり | 自滅近似 |
|---|---:|---:|---:|---:|
| やさしい | 23.94 | 0% | 33.0% | 0 |
| ふつう | **48.09** | **23.7%** | **65.6%** | **0** |
| むずかしい | 47.66 | 28.1% | 67.4% | 0 |

純C#既定値での集計。Unity生成済みSOの集計と対人難易度・相性の評価は未実行。

## 変更ファイルと次の作業

Battle：StaminaSystem/DirectionalAttackSelector/PhaseSixADefaults/PracticeSession新規、定義・状態・FSM・移動・防御・接触・CPU。Game：ControlConfiguration/LearningText新規、入力・画面進行・表示・Runner/HUD。Presentation：ControlProfile、方向アニメ・控えめな結果・GuardBroken音。Editor：Schema移行/AttackData生成/スタミナHUD/CPU集計。EditMode/PlayMode、SpriteForge、Validation、資料・原画・PNG/JSON/GIF。

ClaudeはUnityを閉じて `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase6a`、次にCpuBalanceReportBuilder.Writeで実SO810試合を保存する。まずTitle/Battle実在・Schema6・方向27技・全34状態を確認。DualSenseの表記と割り当て、全14課題を順番にプレイ、スタミナのガード貫通/崩し、下長押し、空中斜め/後ろ、Win/Loseを確認。[詳細手順](docs/codex_logs/phase6a_final.md)。

既知の限界：新規攻撃は描き下ろした綺麗な1キーを保持するため、連続した動作の描き足しは今後の改善余地。実機で絵とF9判定・新規原画の顔/衣装・144Hz操作感を確認する。シーン/新SO/.meta/実行ファイルは今回Unityで生成していない。背景/カメラの最終仕上げ、サポート・オンライン・正式音源は残作業。

---

## 以下はフェーズ5B提出時の履歴（その後ClaudeがUnity全合格を確認）

指示書：[PHASE5B](docs/codex_tasks/PHASE5B.md)。**0 → A → Bのソース実装、素材生成、オフライン検証を完了。UnityでのBuildAll・EditMode・PlayMode・Windowsビルド・144Hz実機確認は未実行です。** commit／push／公開なし。

## 今回の変更

- 0：画面入力はボタン・デバイス単位で押下を管理。遷移を起こした操作だけを解放まで遮断し、自動Countdown→Battle／Battle→Result／フォーカス喪失では新しいガードを作らない。キャラのREADY操作も個別に保護。Esc／Start・別キー・別パッドは即有効。再開時はその決定キーだけを戦闘ラッチから除外する。
- Input Systemの押下フラグを画面間／フレーム間で再使用せず、物理的な立ち上がりを1描画フレーム1回にする。押して離す短い入力はInputState.updateCountで重複を避けながら拾う。Removedとして通知されるパッド切断でも予約機器を保持し、逆順の再接続で席を入れ替えない。
- 既存PlayModeの5失敗のアサーションを維持。HeldConfirmテストはP1の実際のREADYキーEnterを保持する準備へ変更した（旧「無関係のEnterも遮断」はPHASE5Bの個別ガード仕様と矛盾）。無関係のEnter／Escと、別パッド決定／Startを即受け付ける新規テストを追加。5件のUnity合格は未確認。
- 同じ入力更新で決定と取消が観測されても、実際の遷移に使った種類だけを遮断する。CPUキャラ選択の確定はP1だけを対象とする。千鶴ジャブの持続姿勢テストは6F化の正確な添字2とSprite名attack1_02へ更新し、厳密な一致検査を維持。
- A：純C#の表示専用RenderSnapshotsをBattleRunnerがtick前後で更新。位置だけをprev/currentとFractionで補間し、FighterView、ProjectileView、頭上数値・ポップアップ、もぐもぐ・波・フィールド・火花、カメラ目標へ接続。弾は攻撃IDで対応し、リスト添字の使い回しで補間しない。
- ヒットストップ／ポーズ／カットイン／KO／再戦・リセット／大きな位置ジャンプ／向き反転は補間を停止。アニメと向きは現在の固定tickのSampleを読む。従来のSprite30Hz間引きは撤去し、ジャスト演出は火花の表示スローを維持。
- 既定VSync ON・targetFrameRate=-1。メインメニュー／ポーズにモニター同期・60・120・144・無制限、端末保存。追いつき上限5tick/フレームと残時間持ち越し。上限と位置ジャンプ閾値はPresentation.assetから設定。
- A素材：各キャラ14アニメ、計42クリップを更新。idle/walk/dashは8F（idle12fps、walk/dash15fps）、jump/fall/attack1〜3/air_attack/特殊4種/guardは6F・15fps。合計245→368F（透真125、蓮118、千鶴125）。
- SpriteForgeに2キーポーズの不透明ランごとの中割り、頭頂・胴・足の微小整合、共通32色への減色、単一基準idleの±1px呼吸、GIF出力を追加。ローカルの手続き生成で、画像生成AI／外部送信は使用していない。手足の透過隙間を伸ばさない。原画は保持し、旧加工結果はart_source/phase5b_keys/へ保存。
- B：現行の作成既定値はPhaseFiveBDefaults。特殊の発生+12F／硬直+18F／再使用120F／ガード時48F以上。上復帰は発生を維持し再使用90F、食事240Fは保持。1人1発の画面内弾制限。
- ハートを失った生存者へ75F無敵＋最低(12,8)の吹っ飛び。攻撃されても無敵は延長せず、無敵中の攻撃入力でも解除・延長しない。カウンターによるハート減少にも適用。原本のハート3、通常3回→1、特殊1、超必殺2、コンボ上限・通常ガード不可は保持。
- CPUは特殊の予備動作への防御を難易度別に増やし、攻撃後は距離を作って立て直す。相手の無敵点滅中は無駄撃ちしない。空振り／防御が続いた場合は間合いを詰める。ゲージ14/10/20/18→12/8/16/14（命中/被弾/カウンター/ジャスト）。
- BuildAllでルール・CPU・キャラをSchemaVersion5へ移行。旧既定値に一致する項目だけ更新し、独自調整・アニメTiming・NewScene前判定／後の再読込を保持。新規asmdefなし。

## 今回の検証

| 検証 | 結果 | 証跡 |
| --- | --- | --- |
| 純C# NUnit・原本1〜18の既存回帰を含む | 230成功／0失敗／0スキップ | TestResults/phase5b-dotnet-battle.xml・.log |
| Game／Editor／両TestsのAPI参照コンパイル | 警告0／エラー0 | phase5b-dotnet-unity-compile.log |
| UNITY_EDITORなしのプレイヤーコンパイル | 警告0／エラー0 | phase5b-dotnet-player-compile.log |
| asmdef参照 | 6件合格、既存Presentation参照を維持 | phase5b-assembly-references.json |
| CPU全9組×30seed×3難易度 | 810試合、全試合両者攻撃・ハート減少、自滅近似0 | phase5b-cpu-final.csv・docs/CPU_BALANCE_REPORT.md |
| 144Hz相当・表示スナップショット計測 | 単調、差分0.06597185〜0.06597233u、補間なしの対照は停止フレームあり | phase5b-render-144hz-dotnet.csv |
| 全素材・GIF復号・idle整合 | 368F、32色以下、二値α、足元、42GIFの全コマ復号、idle胴/足固定・頭頂±1px | phase5b-animation-verification.json・.log |
| SpriteForge再生成の再現性 | PNG/JSON/contact/GIF全217ファイルでSHA-256一致 | phase5b-spriteforge-reproducibility.json・.log |
| PHASE5BのUnity実行／実画面 | **未実行** | Claude担当。Unity想定EditMode243／PlayMode23は合格件数ではない |

純C#追加検査は補間・弾ID・停止／リセット・時間持ち越し、無敵／再使用／1発制限、独自設定の保持と再移行、現行データで30/60/144/240Hzの状態・イベント一致、seed1〜20のふつう180試合の目標範囲を含む。従来Phase1〜3の単独ルールテストは元の短いフレーム定義で保持し、現行ルールのハート数・ダメージ・コンボ上限・無敵をTempoTestsで別途検証する。

## CPU before → after

純C#の作成既定値。beforeは保存済みフェーズ5最終CSV、afterは現行データ。実ScriptableObjectの集計はUnity起動が必要なため未実行。

| 難易度 | 平均秒 | 縮小到達率 | 超必殺発動試合率 | 自滅近似 |
| --- | --- | --- | --- | --- |
| やさしい | 7.23→23.36 | 0→0% | 75.2→45.6% | 0→0 |
| ふつう | **4.83→45.51** | **0→21.9%** | **67.8→56.7%** | 0→0 |
| むずかしい | 4.26→44.28 | 0→17.0% | 55.9→59.3% | 0→0 |

ふつうは指定の平均35〜55秒、縮小20〜40%、超必殺半数前後を満たす。270試合中59縮小、26時間切れ、153超必殺あり（162発動／151命中）。ハート・場外・時間の優先順位と70秒を変更していない。

## 素材・残課題・Claudeへの確認

- GIFはdocs/art_wip/{toma,ren,chizuru}_{idle,walk,dash,jump,fall,attack1,attack2,attack3,air_attack,special_neutral,special_side,special_up,special_down,guard}_preview.gif。[42クリップのリンク一覧](docs/art_wip/phase5b_previews.md)と各キャラcontact.png・分割ページを用意。透真idle/移動、蓮攻撃、千鶴攻撃の一覧を目視し、中割りの手足引き伸ばしを修正した。
- ローカル中割りは描き下ろしの骨格アニメではない。大きく姿勢が変わる箇所は近いキーポーズを使うため、原画間の顔・衣装・ポーズ差は一部残る。idleのボイリングは単一絵からの生成で抑制。動作全体の快適さは144Hz実機待ち。
- 5回帰はソース修正済みだがUnity InputTestFixture配送・DualSense2台・再戦・フォーカスの実確認が必要。キャッシュ／シーン／.asset／.metaとWindows実行ファイルは今回Unityで更新していない。まずBuildAllを実行する。
- CPUの対人難易度・キャラ相性・超必殺命中率（ふつう151/162）は実機調整課題。集計は公平性の保証ではない。サポート／オンライン、背景・カメラの最終調整、PS表記・キーコンフィグはフェーズ6へ残る。
- 次の手順はdocs/RESUME.mdとdocs/codex_logs/phase5b_final.md。Unityを閉じて、外部実行可能なClaudeがtools/verify/run_unity.ps1 -Tag phase5b、実アセットCPU集計、144Hz・DualSense・点滅・硬直・再戦を確認する。HANDOFF §2とAGENTS.mdのIPC制約のためCodexはUnityを実行していない。

---

## 以下はフェーズ5提出時の履歴（最新の判断には上のフェーズ5Bを使用）

Claudeの後続確認はBuildAll成功／EditMode225/225／PlayMode15/20／Windowsビルド成功。以下の「未実行」は当時のCodex提出時点の記録であり、上記Claude結果へ更新されている。

更新日：2026-10-12（JST）。指示書：`docs/codex_tasks/PHASE5.md`。原本：`docs/SPEC_ORIGINAL.md`。

**フェーズ4検証後の修正とフェーズ5のソース実装・.NET代替検証を完了。フェーズ5のUnity BuildAll／EditMode／PlayMode／Windowsビルド／実画面・実音は未実行です。** git commit／push／公開はしていません。

## 前フェーズの確認済み結果と維持したもの

Claudeのフェーズ4結果はBuildAll成功（Title生成確認）、EditMode204/204、PlayMode16/17、Windowsビルド成功（Build/phase4）。今回も保存済みのphase4_PlayMode.xmlで17件中16成功・1失敗を確認しました。これをフェーズ5の合格とは扱いません。

ProjectBuilder.BuildSceneの「NewScene前にisTitleを決定、NewScene後にアセットを読み直す」構造を維持し、CPU／音設定を含む全シーン依存アセットを読み直します。保存後にはApplication.dataPathからTitle/Battle両ファイルの実在も検査。3キャラの画像、SpriteForge、AnimationSetBuilder、技の性能は保持。BattleSceneTests/GameFlowSceneTestsのUnitySetUp先頭の入力分離とUnityTearDown末尾の復元を維持。新しいasmdefはなく、6アセンブリの参照を検査しました。

## フェーズ4検証後の修正

- メニューの決定入力持ち越し：Clear()が消すのは戦闘用FrameInputLatchだけで、Input SystemのwasPressedThisFrameは同じ更新フレーム中に再読されます。次フレームまで残る押下状態も含め、次画面で決定として再利用されていました。
- Show()で全員の決定・キャンセル・Start／Escの解放待ちを開始。保持中や押下が観測されたフレームは解除せず、全キー・パッドが解放された後の新しい押下を受け付けます。戦闘入力のCaptureも解放まで停止し、ポーズ再開ボタンが攻撃へ漏れるのを防止。
- 既存失敗テストGamepadsNavigateBothCharacterSlotsAndStartPausesBattleのStageSelectアサーションは維持。キーボード保持／全員解放／Esc保持／再開J保持のPlayMode回帰テストを追加。
- 縮小ありの時間切れテストを既存9件内で確認。さらにハート差・蓄積差・同数の3ケースを追加して、縮小5段後の時間切れ比較を明示検査。

## フェーズ5の実装済み項目

- 純C# ICpuBrain/CpuBrain/CpuDefinition。相手の状態を反応遅延付きの履歴から読み、距離・攻撃・ガード・カウンター・空中・硬直、自分のゲージ／ハート、床と縮小予告を判断。シード付きxorshiftで再現可能。状態を書き換えずFrameInputだけを返す。
- 通常3段の予約、4方向の特殊（弾・突進・上復帰・弾消去／構え／食事）、歩行・ダッシュ・ジャンプ・2段ジャンプ・復帰用空中ダッシュ、ガード・回避・カウンター・超必殺。飛び道具は飛行時間までガードを保持。硬直末尾の入力バッファで反撃し、カウンターには特殊、ガードには通常を選ぶ機会を設けた。
- 場端は中央へ戻り、落下時は残ったジャンプ・上特殊・空中ダッシュを使う。突進／回避／ダッシュは終了位置を予測して床外への開始を避ける。縮小予告時は次の床幅を使う。
- CpuEasy/Normal/Hard.assetのScriptableObjectをEditorで生成し、BattleSetupから参照。キャラ選択の難易度がP2 CPUへ反映。CPU戦のP2人間入力は消費後に破棄。再戦でCPUの履歴・乱数・予約を初期化する。
- パッドは接続順にP1/P2へ割当、キャラ選択に機器名を表示。切断した機器を予約し、逆順で再接続しても元の側へ戻す。Xbox／PS操作表と接続ルールを画面・READMEに記載。
- AudioManagerがBattleEventを購読。通常／特殊ヒット、ガード、ジャスト2種、カウンター、BREAK、COMBO FINISH、超必殺、KO／場外／時間切れ、縮小警告。カウントダウンとメニュー移動／決定／キャンセルも再生。メニュー＋3ステージに8秒の自作ループ。
- 全音はProceduralAudioのコード合成（16kHz・モノラル）。外部音源・第三者素材・パッケージは追加していない。ポーズでBGM/SE音量と揺れON/OFFを変更し、PlayerPrefsに保存。
- BattleEffectsView/BattleFeedback：ヒット／ガード火花、軽い画面揺れ、ジャストフラッシュ、Sprite更新を短く30Hzへ落とす表示スロー、カウンター／BREAK／COMBO FINISH既存表示、超必殺帯とPortraitの登場、KO／場外／時間切れバナー、既存win/lose＋結果テキスト。
- 演出は描画側だけ。Time.timeScale、戦闘tick、タイマー、位置・当たり判定は変更しない。ジャストのスローも画像と火花だけで、位置は正確に追従する。
- AttackStartedイベントを接触解決前に出し、同tickに開始して被弾中断した攻撃も集計。Hitに攻撃分類、MatchEndedに終了理由を添えてSE／決着表示を安定させた。

## 検証結果

| 検証 | 結果 | 証跡 |
| --- | --- | --- |
| 純C# EditModeソースの.NET代替 | **212成功 / 0失敗 / 0スキップ** | TestResults/phase5-dotnet-battle.xml・.log |
| Game/Editor/EditMode/PlayMode全ソースAPI参照コンパイル | **警告0 / エラー0** | TestResults/phase5-dotnet-unity-compile.log |
| UNITY_EDITORなしのプレイヤー用代替コンパイル | **警告0 / エラー0** | TestResults/phase5-dotnet-player-compile.log |
| プロジェクト内asmdef参照 | **6アセンブリ合格** | TestResults/phase5-assembly-references.json |
| CPU全9組×30seed×3難易度 | **810試合、全試合で両者攻撃／ハート減少／決着** | docs/CPU_BALANCE_REPORT.md、TestResults/phase5-cpu-final.csv |
| 同じ最終CPUでゲージ12/8比較 | **別810試合集計** | TestResults/phase5-cpu-gauge-before.csv・.md |
| フェーズ5 Unity生成／テスト／ビルド／実機 | **未実行** | Claudeへ引き継ぎ |

追加純C#20ケースはCPU12、音／演出5、縮小時間切れ3。CPU回帰は各難易度270試合を含む。同シード入力列と結果、場端・復帰・縮小、生存して全70秒を迎えるケース、超必殺ジャスト、音／演出の有無による決定性、波形の範囲／再現性を検査。

Unity標準想定は**EditMode225件（純C#212＋生成アセット13）／PlayMode20件（既存17＋新規3）**。想定件数であり合格結果ではありません。CPU選択テストは待機相手を前提にしていたため、同シードの独立シミュレーションとの一致でP2人間入力の無視を確認する内容へ更新。新規PlayModeは保持入力、パッド切断／逆順再接続、Title→CPU戦→リザルト。生成済みアセットを用いたふつうCPU270試合のEditModeも追加。

## CPUバランスの要点

同じ最終CPU・全seedを使い、ゲージだけ比較。命中12→14、被弾8→10、カウンター20／ジャスト18／上限100／1試合1回は保持。SchemaVersion4への初回移行は旧命中12・被弾8の組だけ変更し、独自調整値は保持する。

| 難易度 | 超必殺試合率 調整前→後 | 平均決着秒（後） | 発動数／命中数（後） | 自滅近似 |
| --- | --- | ---: | --- | ---: |
| やさしい | 56.3%→75.2% | 7.23 | 318／165 | 0 |
| ふつう | **47.0%→67.8%** | 4.83 | 317／117 | 0 |
| むずかしい | 38.9%→55.9% | 4.26 | 245／75 | 0 |

ふつうで「半数以上の試合で1回以上発動」の目安を満たした。810試合すべてハート決着（同時KO引き分け含む）、場外／時間切れ0。自滅近似は直近120tickに被弾・カウンターがない場外負けで、意図を直接証明しない。勝率・組合せ別の表はCPU_BALANCE_REPORT。Unity生成済みデータの集計は未実行で、CpuBalanceReportBuilder.Writeを用意した。

## 変更ファイル概要

Battle/Cpuの定義・判断・集計4ファイル、BattleEvent/BattleSimulation/MatchResolver/DamageSystem/PhaseOneDefaults/PhaseTwoDefaults。GameのCpuDifficultyData/AudioSettings/AudioManager/BattleEffectsView新規とBattleRunner/InputController/GameFlowController/MenuScreenView/CameraController/FighterView/UltimateCutInView/PresentationSettings/BattleSetup。PresentationのProceduralAudio/BattleFeedback、EditorのProjectBuilder/CpuBalanceReportBuilder。EditMode CpuTests/ProceduralAudioTests/StageShrinkTests/ProjectAssetTests、PlayMode GameFlowSceneTests/InputFixtureFocus。ValidationのProgram/CpuReport/csproj/RunPhase5と各資料。シーン・アセット・.metaの更新はUnity BuildAllで行う。

## 既知の問題・未実装

- フェーズ5の生成済みシーン／新CPU・音アセット／新.metaはまだ存在しない。既存Build/phase4は旧実装のまま。まずClaudeがBuildAllを実行する。
- 実パッド2台、再接続のデバイス同定、InputTestFixtureのネイティブ配送、既存の失敗テストが本当に通るかはUnity未確認。
- CPU対CPUは平均4〜7秒と短く、今回は縮小に届く試合がない。70秒の上限内に超必殺を使える検証であり、人間相手の難易度・試合の長さ・キャラ間の公平性を証明しない。特に千鶴の対むずかしい組合せ差とP1/P2差を実機で判断してほしい。
- 音は短い仮ループ／電子音。重なりの音量、繋ぎ目、スピーカーでの聴感、音設定表示と保存は未確認。
- ジャストの表示スローは世界全体の時間停止ではなくSprite／火花だけ。画面揺れ・フラッシュ・超必殺／決着バナーの強さ、絵と判定の一致、頭上表示の読める状態を実機で確認する。
- 接続順方式で、専用パッド再割当UIは未実装。別機器への交換時は予約枠が残ることがあり、再起動で割当を作り直す。
- サポート・オンライン・正式音源は未実装。透真の目元・蓮の見づらさは以前のユーザー判断どおり後回し。引き分け両者loseも暫定のまま。

## Claudeへの次の作業

Unityを閉じ、サンドボックス外で `powershell -ExecutionPolicy Bypass -File tools/verify/run_unity.ps1 -Tag phase5`。BuildAll→EditMode225想定→PlayMode20想定→Windowsビルド。次にCpuBalanceReportBuilder.Writeで実アセット810試合をTestResults/phase5-cpu-unity.*へ保存。確認順序・操作・注意は `docs/codex_logs/phase5_final.md`。Unity合格と画面／音の所見を本書・RESUMEへ追記してからフェーズ6へ。

現在はworkspace-write／承認不可でUnityライセンスIPCへ接続できないため、HANDOFFのサンドボックス外Unity実行は行えません。権限変更の確認は求めず、AGENTS.mdに従ってUnity実行をClaudeへ引き継ぎました。
