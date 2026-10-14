# 電源OFF前の引き継ぎ：千鶴の全紙アクション／漫画演出／集合タイトル（2026-10-14）

**ユーザーの「また電源けすよ」で停止。次の「再開して」で以下の未完了部分から続行する。** 原画生成と保存はすべて終了しており、この停止保存では新しい生成・実装・コンパイル・Unity検証を開始していない。既存差分の破棄・リセットはしない。Claudeへ交代しても本書を使用する。

## 停止時の確定した状態

- HEAD／origin/main：`60501c479e05351539e6669da79ddcd153891e1e`。千鶴の紙待機・旧揺れ歩行の完了版は通常push済み。その後の全アクション制作は**未commit／未push**で、作業ファイルとして保存している。
- `art_source/paper_cutout/generated/`：**43 PNGと43件の全文プロンプト**。内訳は採用済み待機1、走り3版、全28姿勢の初版、4効果、7件の修正版。主要姿勢と効果の初回生成は完了しているので、重複生成しない。
- `source-index.json`：全43枚の実寸・SHA-256・alpha32外形・隅の透明度・プロンプト有無を記録済み。alpha32以上の画素は画像端にない。これは形や部位寸法の合格ではない。候補の`approved:false`は未審査を示し、既に実装済みの待機を不採用に戻した意味ではない。
- `runtime-manifest.json`は**未作成**。新しい全アクション画像・効果はまだゲームへ接続していない。最新EXEは`Build/paper_chizuru_1/AniBato.exe`で、保持ビルドは`paper_chizuru_1`と`art3e`の2つ。
- この停止時点でプロジェクトのUnity検証・撮影・コンパイルの実行中プロセスはなく、画像生成も終了済み。同じフォルダを編集する別担当は確認されなかった。再開時も改めてtasklistと作業内容を確認する。

## ユーザーの最新指定

千鶴を完全2Dアニメ塗り／紙の切り抜きへ全面変更する。頭・髪・顔・胴・手・脚・左右スニーカーの寸法は採用待機を基準に固定し、姿勢の高さで人物を拡大しない。待機だけゆらゆら、方向入力中は歩き約15度／走り約45度を進行方向へ固定する。初回は固定を採用し、移動中の往復揺れ・上下ボブは使わない。空中攻撃は同じ絵を十分なtick数保持し、一瞬のちらつきにしない。被弾は涙と痛そうなデフォルメを許容する。

通常は同じ右手のカバン一撃→身体をひねった二撃→カバン投げ。カバンは攻撃時だけ。既存の通常前進／被弾反動／技性能／判定は保持する。千鶴の全35描画状態と4効果が今回の接続対象。停止中の透真ピクセル制作を自動で再開しない。

指示書：[全アクション](../codex_tasks/PAPER_CHIZURU_ALL_ACTIONS.md)、[走り](../codex_tasks/PAPER_CHIZURU_RUN.md)、[漫画演出](../codex_tasks/COMIC_STYLE_PRESENTATION.md)、[集合タイトル](../codex_tasks/PAPER_TITLE_ENSEMBLE.md)。後続の背景／BGM／SE／ステージ／ギミックは、にぎやかな漫画的演出と「ドカッ」「ドドドドッ」「ピカーンッ」等の文字を追加する指定。まだ実装していない。

集合タイトルの回答は**確定済み**。手前の9人は透真・湊・シロ／蓮・結衣・黒獅子王／千鶴・暁・龍太。透真が中央の看板主人公。背景には**主役と同じ3作品の仲間**を小さく控えめに配置する。他作品を今回追加しない。キャラ資料の確認を開始しただけで、集合絵生成／ロゴ→集合トップ→透過→選択の遷移は未着手。集合絵のサポート参加と戦闘サポート機能の実装を混同しない。

## 保存済み画像と採用待ち

28姿勢のキー：`crouch, guard, jump, fall, dodge, counter, counter_success, hit_light, hit_heavy, ko, win, attack1, attack2, attack3, crouch_attack, back_attack, air_neutral, air_forward, air_back, air_up, air_down, air_forward_up, air_forward_down, special_neutral, special_side, special_up, special_down, ultimate`。

4効果のキー：`bag_throw, rice_ball, mogu, curry_wave`。追加修正版は`attack1/2/3_v2, dodge_v2, crouch_attack_v2, ko_v2, special_side_v2`。初版／不採用候補／走りv1～v3も削除せず保存している。全条件は`all-actions-plan.json`と各`*.prompt.json`にあり、実行中セッションの変数がなくても再開できる。

**最初に解決する問題：** `crouch_attack_v2`と`special_side_v2`の生成結果は**1129×1393**で基準1246×1263と違う。靴修正後でも正式採用していない。頭／靴／輪郭の実寸を比較し、透過余白だけで安全に梱包できるか確認する。人物の画素が切れるなら採用せず、内蔵image_genで修正する。人物をセルに合わせて一律拡縮・切り落とししない。

走りv3は1244×1265で透明余白の共通梱包が未完。一部の初版とattack3_v2は1245×1263で1px余白が必要。効果は人物と別寸法で、実寸はsource-indexを読む。セル寸法だけで採用しない。

`shoe-comparison.json`の走りv3の靴長軸は待機に対し約+3.31%／+2.72%。これは指定ROIの白塗りを測った参考値で、全部位の合格証明ではない。`feet-all-candidates.json`の自動比較も白い塗り面の概算で、影・重なり・紙縁に影響される。全候補の採否をこの数値だけで決めず、等倍の頭／靴／踵からつま先・目視・実描画を確認する。新姿勢・4効果の正式選択は未完了。

## 未コミットの実装途中

- `PaperCharacterMotion.cs`：PaperPose、全状態対応、同Sprite4コマ／8tick保持、強被弾、固有効果、固定15度／45度の設定を追加。
- 新規`Presentation/PaperTravelLean.cs`：純C#で固定方向の傾きと左右の床補正。時間による往復揺れなし。
- 新規`Editor/PaperSourcePacking.cs`：1246×1263へ透明余白だけを梱包、人物の画素を再サンプリングしない。非透明画素が切れる場合はエラー。同じPPUで姿勢を保持し、効果は別の幅設定。
- 新規`Editor/PaperActionBuilder.cs`／変更`PaperCharacterBuilder.cs`：runtime-manifestの全状態／強被弾／4効果を検査してEditor生成する入れ物。マニフェスト未作成なので新アセット生成は未実施。
- 変更`FighterView.cs, ProjectileView.cs, BattleRunner.cs, MenuScreenView.cs, UltimateCutInView.cs`：姿勢／実ハート消失時の強被弾／紙の投げ物／もぐもぐ／カレー／結果／カットインの参照を準備。
- 最後の`FootBaseY`追加と地上の空中姿勢の補正は**まだコンパイルしていない**。直前のAPI参照コンパイルは警告0／エラー0だったが、最後の差分の合格として扱わない。最低alphaを足元と扱う処理は、下向きカバン攻撃等で道具が最下端になる点もレビューする。
- `PaperCharacterAssetTests`／`PaperCharacterSceneTests`等は旧Schema1／揺れ歩行／旧攻撃への切替を期待するまま。新仕様に即した厳密な期待値と全状態／表示保持／床／割り込みの検査へ更新する必要がある。旧ピクセル・他キャラ・戦闘性能の検査を弱めない。

計測スクリプトは`tools/verify/inspect_paper_sources.cjs, measure_paper_shoes.cjs, compare_paper_feet.cjs`。新原画／各プロンプト／一覧／計測結果／4指示書／本書とRESUME等も未コミット。既存変更はすべてそのまま残している。

## 検証とスクショの区別

**今回の停止保存：新しいUnity実行0回。全アクションのBuildAll／EditMode／PlayMode／Windowsビルドは未実行。**

前の完了版`60501c4`の実結果はBuildAll成功、EditMode **441/441**、PlayMode **36/36**、Windowsビルド成功、失敗／スキップ0。証跡は`TestResults/paper_chizuru_1_*`と[完了報告](paper_chizuru_1_final.md)。この441／36を新差分の合格に流用しない。

保存済み画像は`docs/screenshots/paper_chizuru_unity/`の**18 PNG＋2 GIF**。待機と旧揺れ歩行のUnity実カメラ出力で、全アクション／新固定傾きのスクショはまだない。実モニター撮影／OSキー送信は行っていない。新原画プレビューはUnityの実スクショではない。

## 「再開して」で始める手順

1. `tasklist`でCodex／Claudeを確認し、同フォルダの別担当がいれば編集を待つ。AGENTS→HANDOFF→RESUME／STATUS→本書→指示書を読み、git statusとHEAD／origin/mainを確認する。破棄／resetせず現在の差分から続ける。
2. 全43原画とプロンプトを再利用し、未完の頭／左右靴／部位寸法と2件のキャンバス不一致を確認する。必要な修正候補だけ生成し、採否を保存する。
3. 採用画像から`runtime-manifest.json`に全35状態（共用を含む）と強被弾／4効果、正しい頭／足の基準を定義する。Editor接続を完成し、最後の未コンパイル差分を確認する。
4. 固定15度／45度・待機揺れ・全姿勢・空中継続表示・割り込み・ポーズ・再戦・無効化・他キャラ保持のテストを更新／追加する。
5. サンドボックス外実行の承認で`tools/verify/run_unity.ps1 -Tag paper_chizuru_all_1`を実行。`-nographics`なし。BuildAll／EditMode／PlayMode／Windowsビルドを全合格させ、Unity画像を目視する。実機キー撮影前は必ず「PCに触らないで」と伝える。
6. 全4工程合格の区切りでSTATUS／RESUME／出典を更新し通常commit／origin mainへpushする。force push・公開履歴変更は禁止。
7. 千鶴完了後に漫画演出→9人と同じ3作品の仲間の集合タイトルへ進む。資料にない容姿／設定を勝手に確定しない。透真の停止中制作は別途再開指示が必要。

Claudeに確認してほしい点：候補の寸法と頭／左右靴の形、地上／空中ピボットと床、道具を頭／足と誤判定しないこと、固定傾きの向き、攻撃が一瞬だけになる箇所、実ハート消失時の画像選択、全状態不足、実Unity4工程と保存済み結果の区別。

## 以下は制作途中の履歴（最新は上記の停止状態）

直前の待機/歩行は60501c479e05351539e6669da79ddcd153891e1eをorigin mainへ通常pushし、同じSHAを確認。実Unity BuildAll成功、Edit441/441、Play36/36、Windowsビルド成功。証跡paper_chizuru_1、画像docs/screenshots/paper_chizuru_unity/、最新起動Build/paper_chizuru_1/AniBato.exe。停止中の透真原画/素材準備も同コミットへ保全、新しい戦闘仕様は未実装。

追加指定で現在はPAPER_CHIZURU_ALL_ACTIONSを制作中。全35状態/固有効果が対象、待機原画を部位/縮尺/服/画風の基準にする。走りv1/v2の靴が大きくて不採用、v3の白靴長軸は182.58/209.94px、待機176.73/204.39に対して+3.31%/+2.72%。比較はshoe-comparison.jsonとmeasure_paper_shoes.cjs。v3の生PNGは1244×1265、共通1246×1263への透明余白の梱包は未実施。頭/靴/部位を拡縮しない。原画は全版保持。

保存済み原画：走り3版/しゃがみv1/ガードv1/ジャンプv1。しゃがみ/ガード/ジャンプの部位比較/正式採否と残る25姿勢/4効果は未完了。各全文日本語プロンプトはgenerated/*.prompt.json。all-actions-plan.jsonに全レシピ/主参照/未選択を保存。新しい原画のゲーム接続はまだで、完了とは扱わない。

進捗更新：28PNGを保持（旧待機1＋走り3＋24姿勢のv1）。落下/回避/構え/反撃成功/軽重被弾/KO/勝利/通常三段/しゃがみと後方攻撃/空中7方向/おにぎり投げまで原画を保存。未生成は特殊Side/Up/Down/Ultimateの4姿勢と4効果。新画像の頭/左右靴の全件比較・採否/必要な再修正は未完了、画像の一律寸法だけで正式合格にしない。

最新の移動指定：待機のみ揺らし、歩きは進行方向へ15度、走りは45度の固定または微振動。初回は固定、移動中の往復揺れ/ボブなし。被弾時は涙/痛そうなデフォルメ。COMIC_STYLE_PRESENTATIONには漫画文字/SE/BGM/背景/ステージ/ギミックのにぎやかな演出を記載。ルール/性能/他キャラ画像は変えない。

実装途中：PaperCharacterMotionにPaperPose/繰り返し同Sprite/8tick保持/全状態マップ/強被弾/効果の入れ物、PaperTravelLeanの固定傾き/床補正、FighterViewの姿勢/強被弾選択を追加した。Editorの素材生成・全状態設定/旧テストの仕様更新/新テストはまだ。今回の全体差分は未コンパイル・Unity未検証・未commit/未push。保存済み441/36を新全体の合格と混同しない。

実装進捗更新：PaperSourcePacking（透明余白だけの共通1246×1263梱包、原画の部位/画素を拡縮しない、非透明が切れればエラー）、PaperActionBuilder（全状態/強被弾/4効果の不足検出）を追加。runtime-manifest.jsonはまだ作っていないため新素材を接続しない。ProjectileView/FighterView/BattleRunnerの紙効果、MenuScreenViewの選択/結果、UltimateCutInViewの紙カットインの入れ物を追加。API事前コンパイルは警告0/エラー0。実Unityの新差分検証は未実行、テストは新仕様へ未更新。

集合タイトルも追加：PAPER_TITLE_ENSEMBLE。出演者はユーザー回答で9人（透真/湊/シロ、蓮/結衣/黒獅子王、千鶴/暁/龍太）に確定。9人を前面、その他原作キャラをサポートとして小さく奥へ。透真が中央主役。作品別に資料/人物を確認して生成し前景と合わせる。資料調査を開始したのみ、タイトル原画/遷移実装は未着手。ロゴ→集合トップ→徐々にalphaを下げて選択。集合絵のサポート登場は戦闘サポート機能の完成ではない。

背景サポートの作品範囲もユーザー回答で確定：まず主役と同じ3作品の仲間。他作品のキャラは今回追加しない。

次：新原画を順に生成/比較して確定→全35状態と効果をEditorで接続→固定傾き/全描画状態/空中の継続表示と既存回帰を実Unity全4工程検証→漫画演出→集合タイトル。いずれも指示書/旧ファイルを保持し、全4工程合格の区切りで通常commit/push。実OSキー撮影前は必ず「PCに触らないで」。Claude/Codexの同時編集は禁止。
