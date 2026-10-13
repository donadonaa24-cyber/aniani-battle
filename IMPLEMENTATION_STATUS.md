# 実装状況：ART3E完了・保存処理復旧（2026-10-14）

**最新：再起動後は保存時のロックが解消し、復旧できた。** [再起動後の詳細](docs/codex_logs/RESTART_CHECKPOINT.md)。欠落スクリプトをGit保存内容から復元、今後は採用原画だけのZIP作成に接続。作成済みZIP721ファイルを再照合して正式名へ置換成功、38.75MiB/採用19原画/全文90プロンプト。全90原画と旧ZIPを保持、差分の破棄/リセットなし。両スクリプトはWindows PowerShell5.1で構文確認成功。

実装と実Unity全4工程は合格済み。今回は保存済みEditMode424/424・PlayMode32/32・BuildAll/Windowsビルド成功を確認しただけで再実行していない。実機スクショ未保存、素材プレビューはdocs/art_wip/art3e_*。

2026-10-14、未公開commit2097c99だけの作り直しと通常pushはユーザー「お願い」で承認済み。元コミットをローカルで保全し、ファイル内容を保持して小型ZIPと復旧記録を含める。origin main182e970の公開済み履歴は変更せず、force push/ファイル破棄/全体リセット禁止。正常push後に反映SHAを記録する。次の制作はART3F急停止/切り返し→ART3G強弱被弾画像、後退の戦闘処理は既に完了。Claudeへ交代可能。

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
