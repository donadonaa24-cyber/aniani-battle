## 最新（2026-10-20）：特殊下9人を実装、全4工程合格。全体ゴールは継続中

全体指示書docs/codex_tasks/DIRECTIONAL_SPECIALS_AND_RECOVERY.md、進行表directional_specials_progress.json。今回A/Bのみ：下+特殊の独立入力・CD表示、透真/シロの相手の向きに基づく背後転移、蓮/結衣/王の超ガード+1/3反撃、湊の床地雷、暁の罠カード、龍太の読書完了後5秒1.5倍、千鶴2.5秒食事後通常蓄積1回復・CD20秒を実装。暁は相手だけにダメージ、命中で消え、20秒期限・各自同時2枚、N投射と枠独立、枚数HUD。ガード時は消えず再接触を受付。湊地雷を内蔵imagegenで生成し原画RGBAのまま接続。通常/特殊/超必殺の標準ダメージは変更せず、1/3と状態効果専用の分類を追加。

今回directional_down_2：BuildAll成功／Edit643/643／Play86/86／Windows成功、失敗・スキップ0。両向きの実キー・9人のCD・回復完了と割込・相手裏側・各自2枚と自分の罠免疫・命中消滅・期限・防御後再接触・既存技と別枠を検証。18枚はTestResults/directional_down_2/captures/directional_down/、代表3枚はdocs/screenshots/directional_down_2/。撮影は舞台全景でHUDオーバーレイを含まない。HUD文字は実PlayModeで検証済み。詳細docs/codex_logs/directional_down_verification.json。初回はEdit639/641、Play85/86。新技の共通反動設定と旧共有技/単一特殊前提の検証を新仕様へ更新して再実行。Unity内部の画像キャッシュ診断はログに残るが、今回テスト失敗なし。

次は工程C docs/codex_tasks/DIRECTIONAL_RECOVERY_STAGE_C.md：全9人の上復帰・接触1/3・着地制限・CPU/練習/技表へ。art_source/directional_specials/に千鶴風船v2、透真ドローン、暁羽ばたき、龍太本キックの各1原画を生成済み、まだゲーム未接続。artwork-status.jsonの状態とSHAを確認し、接続と部位サイズ審査を行う。内蔵生成を使用、全文条件prompts/。未採用千鶴v1も工程記録として保管。

全体に残るもの：上復帰、透真銃口/追尾ドローン/ロケット、王の3方向麻痺/時差横突撃、暁巨大ピラミッド10秒固体壁/カード斬り、龍太超必殺完了後20秒強化、千鶴カレー5秒残留/回復、9人スタミナ、最終Webと全要求監査。Webは20563eaの音版のまま、今回の特殊下は未反映。Windows最新Build/directional_down_2、予備directional_down_1（初回テスト失敗を含むため検証済み予備として扱わない）。半減45判定・14/10ゲージ・既存通常4枚・下/空中N1枚・56動作・承認済み龍太被弾v5・カバン保護・Web512/4096を維持。


## 最新（2026-10-19）：BGM4曲・SE19音の紙舞台版をUnity/Webへ反映完了

指示書docs/codex_tasks/PAPER_THEATRE_AUDIO_REFRESH.md。旧180BPM試作を、メニュー144／SF156／推し駒164／料理172BPM・各16小節の新規オリジナル4曲へ変更。木琴・弦・鍵盤・リード・ベルと舞台ごとのリズム、紙の風切り／厚紙のヒット／木の防御／決着ジングル等19SEを自作PCM合成。24kHz、曲切替0.35秒クロスフェード、強いSEでBGMを滑らかに抑制。BGM/SE音量設定と戦闘・原画を維持。StyleVersion1で一度だけ移行し、以後の調整値は保持する。

今回のpaper_audio_1：BuildAll成功／Edit616/616／Play85/85／Windows成功、失敗・スキップ0。実AudioClipの4曲ループ・切替・同じ曲の再要求・19SE・時間停止中のダッキングと復帰を合格。紙音のピーク/RMS・有限値・決定性・接ぎ目・全小節の発音も確認。音楽的な好みのユーザー最終確認は未実施。試聴WAVは音声確認/現在ゲームで使用中/（4曲＋19個別＋一覧）に保存。生成元SHAと波形の測定は音声一覧.json。

paper_audio_webも実ビルド成功（123,667,948bytes／241.144秒）、Windows復帰成功。435/435画像の原画RGBA保持を監査。内蔵Chromiumで新ビルドの音声有効化、メニュー、料理の舞台、特殊と超必殺を実キー確認。音声running・既存音量0.35/0.65保持・今回起動以降console error/warn 0。4曲/19SEのClip個別検証は実Unity PlayModeで実施。通常Chrome/Edge別個・実機パッド・低メモリ端末は今回未再検査。証跡はdocs/codex_logs/paper_audio_verification.json、docs/screenshots/paper_audio_web/。

Windowsは最新Build/paper_audio_1と予備minato_pull_2。直近TestResultsはpaper_audio_1・minato_pull_2・charge_art_2とdotnet類を保持。今回の自動撮影937ファイルをTestResults/paper_audio_1_auto_captureへ保存し、履歴付き194件は退避SHAが全て一致することを確認して元の履歴を保持。次回全体検証は -CaptureRoot TestResults/<タグ>/captures を付ける。整理のSHA証跡はdocs/codex_logs/paper_audio_*retention.jsonとpaper_audio_verified_capture_backups.json。

湊引力は25a121cで全4工程/Web検証・push済み。この音工程の残作業なし。次はユーザーの試聴や追加指示に応じて調整する。半減45判定・ゲージ14/10・全原画・承認済み龍太被弾v5・保護カバン・Webメモリ512/4096を維持。Claudeへ：完了済みの原画生成や音の旧試作を再適用せず、この最新記録と音声一覧を確認する。

## 最新（2026-10-19）：湊のブラックホール引力、Unity/Web実装完了

指示書docs/codex_tasks/MINATO_BLACK_HOLE_ATTRACTION.md。半径10u・26u/sで対戦相手を中心へ継続的に引く純C#外部移動成分を追加。通常の足場AABBへ通し、自分・範囲外・消滅後・接触済み・保護中・停止中へ作用しない。接触時2ハートと既存防御／多重ヒット防止、静止場の5×4u・位置・180tickは維持。湊のみMoveSetSchemaVersion1→2へ一度移行し、再生成後の調整値を保持。

今回のminato_pull_2はBuildAll成功／Edit614/614／Play84/84／Windows成功（失敗・スキップ0）。左右・両プレイヤー・離れる歩き/ダッシュ・空中・範囲外・一度だけ2ハート・ジャスト防御・停止/保護・固体壁・決定性・移行の反復を確認。PlayModeの実キーでも左右で吸い寄せ、3→1ハートになる流れを合格。6PNGはdocs/screenshots/minato_pull_2/、実件数とSHAはdocs/codex_logs/minato_pull_verification.json。初回minato_pull_1のコンパイルエラーは修正後に全工程を再実行済み。

minato_pull_webも実ビルド成功（123,660,664bytes／234.127秒）、Windowsへ復帰成功。435/435画像のRGBA保持を監査。実ブラウザで湊本人x=-5を維持し、相手x=5→4.5688→2.8467へ吸引・接触Hitstunを確認。練習のFreezeHeartsによりWeb画面のハートは固定されるため、2ハート減少の証跡は左右の実Unity PlayModeを使用。音声running、console error/warn 0。証跡はdocs/screenshots/minato_pull_web/。開始HEAD3052c3b、同時編集なし。原画生成なし。半減45判定・ゲージ14/10・全原画・カバン予備・Webメモリ512/4096は維持。次はユーザー追加のBGM/SE改修へ。

## 最新（2026-10-19）：溜め原画9人分・龍太の左拳2件、Unity/Webへ実装完了

全9人の溜め中専用1原画を生成して接続。溜め中だけ専用絵を保持し、既存の傾き・沈み・色変化・紙片を併用。解除後は既存の方向別攻撃原画へ。art_source/paper_charge/に原画・全文条件・参照・SHA・部位比較・現在採用を保存し、progress.jsonはcomplete。日本語の現採用画像はイラスト確認/現在ゲームで採用中/各人物/溜め中.png。採用後のゲーム接続とユーザーの美術最終承認は区別する。

龍太の走りv3・ジャンプ上昇v3は、画面右の本人左拳を閉じたまま内側と握った指を見せる形へ修正。確認中フォルダと現採用の両方を更新。通常被弾の承認v5は変更しない。先行charge_ryuta_hands_1も全4合格（Edit582/582・Play81/81）、18PNGはdocs/screenshots/charge_ryuta_hands_1/paper_motion_ryuta/。

今回の最終charge_art_2はBuildAll成功／Edit593/593／Play83/83／Windows成功、失敗・スキップ0。全9人・左右・60/120/180tick・最大維持・短押し・上下解除・被弾/接地喪失・描画非干渉と既存56動作を確認。36PNG中19PNGと一覧はdocs/screenshots/charge_art_2/。実件数とSHAはdocs/codex_logs/charge_artwork_verification.json。初回charge_art_1はNUnit構文エラーのみで停止、修正して全工程を再実行済み。

charge_art_webは実ビルド成功（123,660,075bytes／6ファイル／218.730秒）、Windows設定へ復帰成功。435/435画像の原画RGBA保持を監査。内蔵Chromiumで新ビルド起動、9人選択、龍太の溜め180tick保持・解除してAttackへ・走り・上昇を実キーで確認。音声running、今回起動以降console error/warn 0。docs/screenshots/charge_art_web/に実画面・状態・監査。WebのDXT表示と原画の無損失保持は区別。通常Chrome/Edge個別、実機パッド、低メモリ端末は今回未再検査。

指示書docs/codex_tasks/PAPER_CHARGE_ARTWORK.mdの今回範囲に残課題なし。通常commit/pushの保存区切り。次はユーザーの追加指示のみで進め、新しいポーズや方向別溜め差分を自動生成しない。既存56動作、通常4原画、下/空中N各1枚、保護カバン、半減判定、ゲージ14/10、Webメモリ512/4096を維持。Windowsは最新charge_art_2と予備charge_ryuta_hands_1。旧TestResults約5.10GBは実件数とSHAをdocs/codex_logs/test_result_retention_20261019.jsonへ残して整理、直近3回とdotnet類は保持。Claudeへ：この完了済み生成を繰り返さず、現在採用一覧と新しい追加指示を確認する。

## 最新（2026-10-19）：全8人56動作をUnity/Webへ実装・検証・完了

龍太の通常被弾はユーザーが「これでいい」と添付した握り拳v5で確定。開いた掌v6は不採用、v4への推測復帰や追加生成なし。承認参照・選択理由・SHA・個別承認はart_source/paper_roster_motion/ryuta/。採用の日本語画像はイラスト確認/現在ゲームで採用中/龍太/通常被弾.png。透真の反撃の腕と暁の手・脚・5指の修正版も維持。

全8人の走り・上昇・落下・回避・反撃成功・通常被弾・強被弾を各1原画、計56枚で接続。通常4原画、下/空中N各1枚、千鶴・保護カバン、戦闘値・半減判定・ゲージ14/10は維持。原画の個別拡縮なし。進行表art_source/paper_roster_motion/progress.jsonはcomplete。人物別採用SHAと保存済み検証はmotion_roster_native_verification.json、今回の最終全4結果とWeb証跡はmotion_roster_final_verification.json。

今回の最終motion_web_memory_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。全8人の左右・7動作・反撃・通常/強被弾・回転・解除・重力・ヒットストップ・描画非干渉を確認。龍太の承認版を最初に検証したmotion_ryuta_2も全4合格で、18PNGはdocs/screenshots/motion_ryuta_2/paper_motion_ryuta/。最新WindowsはBuild/motion_web_memory_1/AniBato.exe、予備はmotion_ryuta_2。

Web2は実起動OOMで不採用。原画やミップを縮小・削除せず、WebGLBuilderのみ初期512MB／動的拡張上限4096MBに修正。motion_roster_web_3は実ビルド成功（119,935,208bytes／6ファイル／190.338秒）、Windows復帰成功、426/426画像RGBA保持監査合格。内蔵Chromiumで起動・トップ・9人2段選択・透真の走り/上昇・龍太の通常被弾v5と1/3を確認、音声running、Web3起動以降console error/warn 0。docs/screenshots/motion_roster_web_3/に実画面と状態記録。原画保存と非可逆のWeb DXT表示を区別。実機パッド・通常Chrome/Edge個別・低メモリ端末は今回未再検査。

未実装の追加モーションは今回範囲にない。新規原画はユーザーの追加指示後。完了済み56枚を重複生成しない。古いTestResultsは実件数/SHA集計と人物別18PNGを残して整理。通常commit/pushして終了する。以後の再開は新しい指示を確認し、この完了済み作業を繰り返さない。

## 最新（2026-10-19）：龍太は添付承認の握り拳v5、56動作の全4検証完了、Web最終確認中

ユーザーの「これでいい」の画像は龍太の通常被弾v5として確定。開いた掌v6は不採用、追加生成・手の再解釈なし。承認参照はart_source/paper_roster_motion/ryuta/user_approved_hit_light.png、選択理由・SHA・個別承認をmanifestに保存。motion_ryuta_2でBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。18PNGはdocs/screenshots/motion_ryuta_2/paper_motion_ryuta/。現在採用の日本語画像も更新済み。全8人56原画の検証集計はdocs/codex_logs/motion_roster_native_verification.json。

motion_roster_web_2を実ビルド中、Windows復帰後に426画像のRGBA保持を実件数で監査し、8799の検証タブ1をreloadして実ブラウザ確認する。先行motion_roster_webとmotion_ryuta_1は不採用v6を含むため現在採用の証跡ではない。戦闘値、通常4原画、下/空中N各1原画、千鶴の保護カバンは維持。通常commit/pushはWeb復帰と最終記録後。

## 最新（2026-10-19）：全8人56動作のUnity実装完了、Web最終更新へ

龍太の走りv2・上昇v2・落下v1・回避v1・反撃成功v2・通常被弾v6・強被弾v2を接続。motion_ryuta_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。最終Playは全8人の左右と7動作・通常/強被弾・反撃・解除を検査。18PNGはdocs/screenshots/motion_ryuta_1/paper_motion_ryuta/。通常被弾のユーザー指摘の左手は、右の甲状のv3/v4と右拳状のv5を不採用にし、開いた左の掌を見せるv6へ変更。画面右に親指、上へ4長指の計5指。靴の端切れも正方形1254×1254の余白で解消。採用はイラスト確認/現在ゲームで採用中/龍太/。

暁2624717まで保存済み。全56枚の原画・全文条件・参照役割・版・SHA・整数梱包とRGBA保持が揃い、8人の全4検証完了。docs/codex_logs/motion_roster_native_verification.jsonに各タグの実件数・ログSHAと採用画像SHAを集計。美術最終承認とは区別。最新2つのWindowsはmotion_akatsuki_1とmotion_ryuta_1。現在8799は旧Web版なので完了扱いにしない。

次はbuild_webgl.ps1 -Tag motion_roster_web -RestoreWindows→426画像（実件数を監査）→ローカル8799で実ブラウザの表示・音・エラー確認→Web証跡と全体進行表をcompleteへ→通常commit/push。Cuaでブラウザ2の検証タブ1を作成済み、?verify=1の未起動の読み込み画面。新Web完成後にreloadして確認する。千鶴・通常4原画・下/空中N各1枚・戦闘値・半減判定・保護カバン維持。

## 最新（2026-10-18）：暁まで49動作を接続、龍太を制作中

暁の7動作を接続。motion_akatsuki_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。18PNGはdocs/screenshots/motion_akatsuki_1/paper_motion_akatsuki/。採用はイラスト確認/現在ゲームで採用中/暁/。ユーザー指摘3件は旧候補を不採用：反撃v2は左の掌と親指を正しい握りへ、通常被弾v3は3脚状の膝をなくして2脚2靴・靴寸法も修正、強被弾v3は空の右手を甲側の5指へ。強被弾v2は5指だが掌の向きが逆のため不採用。カードで隠れる指を全部見えると扱わない。小さいカードの題字には簡略化・字形差があることをpart-reviewへ記録。

黒獅子王cb899bfまで保存済み。全56枚中49枚が接続・全4検証済み。龍太は走りv2・上昇v2・落下v1・回避v1の4原画が未接続、未コミット。走り/上昇の左拳を甲側へ修正し、回避は採用しゃがみを部位寸法基準にした。次は反撃成功・通常被弾・強被弾→7枚梱包→Unity全4工程。最後に全8人分のWebGLを更新し、実ブラウザで表示と音を確認する。現在8799のWebは以前の攻撃原画版で、新7動作の反映は最終更新待ち。

指示書PAPER_ROSTER_SEVEN_MOTION_KEYSとprogress.jsonから継続。透真の腕修正版v2、千鶴・通常4原画・下/空中N各1枚・戦闘値・半減判定・保護カバンを維持。検証中はC#と接続manifestを変えない。

## 最新（2026-10-18）：黒獅子王まで42動作を接続、暁を制作中

黒獅子王の7動作を接続。motion_king_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。18PNGはdocs/screenshots/motion_king_1/paper_motion_king/。採用はイラスト確認/現在ゲームで採用中/黒獅子王/。落下v3は尾2本とマント切断、反撃v2は爪の向き、強被弾v2は右端切断を修正。四肢2腕2脚、尾1本、人の顔と被り物の獅子の顔を区別。横長の回避1362×1155も可視1282pxを縮小せず整数梱包し、RGBA7枚を照合。既存の無敵とハート回復点滅が実描画へ掛かるが原画へ焼き込みなし。

結衣c668be4まで通常保存済み。全56枚中42枚が接続・全4検証済み。暁は走りv2・上昇v3・落下v1・回避v2の4原画が未接続、未コミット。左手の握りを甲側へ統一し、正式な料理カード背面を参照。上昇v1/2の曖昧な手とv2の題字消失、回避v1の巨大な頭と靴は不採用。次は暁の反撃成功・通常被弾・強被弾→7枚梱包→全4検証。龍太は未着手。Webは全8人の接続後に更新・実ブラウザ検査。

PAPER_ROSTER_SEVEN_MOTION_KEYSとprogress.jsonから継続。透真の腕修正版v2、通常4原画・下/空中N各1枚・半減判定・戦闘値・保護カバンは維持。検証中に接続manifestとC#を変更しない。

## 最新（2026-10-18）：結衣まで35動作を接続、黒獅子王を制作中

結衣の走り・上昇・落下・回避・反撃成功・通常被弾・強被弾を各1原画で接続。motion_yui_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。18PNGはdocs/screenshots/motion_yui_1/paper_motion_yui/。採用はイラスト確認/現在ゲームで採用中/結衣/。走りv3は過大な前靴と杖の端寄りを修正。反撃v2は杖を内側へ収めた。左手の杖・右ミトン、右脚のみ縞と左脚の素肌を維持。回避の緑の半透明は既存の無敵表示で、原画へ焼き込んでいない。原画RGBAを保持する整数梱包7画像を照合。

透真a88233f・蓮197b93d・湊4401979・シロ972806dは保存済み。透真反撃の腕修正版v2を維持。全56枚中35枚を接続・全4検証済み。黒獅子王は走り・上昇・落下v3・回避の4候補が未接続、未コミット。落下v1の尾2本とv2の右端切れを不採用にし、v3で尾1本と内へ折ったマントに修正。次は反撃成功と被弾2種→7枚の梱包・Unity検証。暁・龍太は未着手。Webは全8人の接続後に更新・実ブラウザ検査する。

指示書PAPER_ROSTER_SEVEN_MOTION_KEYSとprogress.jsonから継続。通常4原画・下/空中N各1枚・戦闘値・半減判定・保護カバンを維持。検証中はC#と接続manifestを変えず、次人物の原画保存だけ進める。

## 最新（2026-10-18）：透真・蓮・湊・シロの28動作を接続、結衣を制作中

シロの走り・上昇・落下・回避・反撃成功・通常被弾・強被弾を各1枚で接続。motion_shiro_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。18PNGはdocs/screenshots/motion_shiro_1/paper_motion_shiro/。採用はイラスト確認/現在ゲームで採用中/シロ/。落下・回避は端へ寄った候補をv2へ修正。走りの横長1351×1164も可視画素を縮小せず整数移動で梱包し、RGBA7画像を照合。四脚と足先の所属を確認。四足の鼻先が床へ向かないようDash表示のみ10度、人間45度・全員の歩き15度は維持。戦闘値・物理・ヒットストップ・攻撃原画・保護カバン不変。

保存済みは透真a88233f／蓮197b93d／湊4401979。透真の反撃は腕修正版v2を維持。全56枚中28枚を接続して検証済み。結衣は走りv3だけ原画保存、他6条件準備済み。走りv1/2の前靴の大きさと杖の右端近さを、v2直接編集のv3で修正。本人左手の杖、右ミトン、右脚のみ縞、左脚の素肌・紫リボンを維持。結衣は未梱包・未接続・未コミット。黒獅子王／暁／龍太は未着手。Webは全8人接続後に最終版を更新・実ブラウザ検査する。

新指示書PAPER_ROSTER_SEVEN_MOTION_KEYSとprogress.jsonから継続。次は結衣の残6原画→梱包→Unity全4工程。生成は1人ずつ、検証中にC#と接続manifestを変更しない。既存変更を破棄せず、済んだ原画を重複しない。

## 最新（2026-10-18）：透真・蓮・湊の21動作を接続、シロへ

湊の走り・上昇・落下・回避・反撃成功・通常被弾・強被弾を各1枚で接続。motion_minato_1はBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。専用18PNGはdocs/screenshots/motion_minato_1/paper_motion_minato/。左の義手と右の生身腕、目・衣装を維持。原画・全文条件・SHA・部位審査はart_source/paper_roster_motion/minato/。採用はイラスト確認/現在ゲームで採用中/湊/。個別拡縮0、7画像の枠内RGBA保持を確認。美術最終承認とは別。

透真a88233f、蓮197b93dは通常保存済み。透真の反撃は腕修正版v2を維持。全56枚中21枚の接続・全4検証が完了。シロは7候補保存済みで未接続。落下と回避は端の近さをv2へ修正。走りが横長1351×1164で、人物画素を縮小せず透明余白と整数移動で共通枠へ入れる。四足の走りを45度倒すと鼻先が床へ向くため、次工程でシロのDash表示だけ10度へ調整し、実描画と足元を検証する。戦闘値は不変。結衣は参照確認と7条件の準備まで、画像未生成。黒獅子王・暁・龍太は未着手。制作中のシロと結衣は未コミット。Webは8人接続後に更新して実ブラウザ確認する。

新指示書PAPER_ROSTER_SEVEN_MOTION_KEYSとprogress.jsonで継続。同時編集なし、既存差分を破棄しない。検証中のC#と接続manifestは変更せず、次人物の原画保存だけを進める。千鶴・通常4原画・下/空中N各1枚・45半減判定・ゲージ14/10・保護カバンは維持。

## 最新（2026-10-18）：透真と蓮の専用7動作を実装、湊の接続へ

透真の腕修正版v2を含む7枚はa88233fでorigin mainへ保存済み。今回の蓮は走り・上昇・落下・回避・反撃成功・通常被弾・強被弾を各1原画で接続し、motion_ren_2でBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。専用18PNGはdocs/screenshots/motion_ren_2/paper_motion_ren/。採用はイラスト確認/現在ゲームで採用中/蓮/。回避の大きい頭巾候補v1は不採用、待機の部位寸法を指定したv2を使用。初回1のEdit581/582は旧ジャンプ待機代用の固定期待値1件、Play81/81とWindows成功。専用原画の有無を検査するよう修正して2で全4を再確認。戦闘値・物理・ヒットストップ・通常4原画・下/空中N各1枚・保護カバン不変。

全56枚のうち14枚を接続して検証済み。湊は7候補保存済みで次に透明梱包・接続、シロは走り/上昇/落下/回避候補を保存中。落下の端の近さはv2へ修正、回避も余白を広げるv2を生成予定。湊とシロは現段階でゲーム未接続、制作中データは未コミット。残4人は未着手。新指示書PAPER_ROSTER_SEVEN_MOTION_KEYSとprogress.jsonから継続。Webは今回未更新、8人接続後に最終版で実ビルドとブラウザ確認。検証中のC#と接続用manifestを変更せず、原画保存のみ進める。同時編集・差分破棄なし。

## 最新（2026-10-18）：透真の専用7動作と反撃の腕修正を実装、残り7人を継続

新指示書はPAPER_ROSTER_SEVEN_MOTION_KEYS。透真の走り・上昇・落下・回避・カウンター成功・通常被弾・強被弾を各1原画で接続。反撃v1はユーザー指摘の異常な腕で不採用。自然に前へ腕を伸ばすv2に差し替えたmotion_toma_3でBuildAll成功／Edit582/582／Play81/81／Windows成功、失敗・スキップ0。18PNGはdocs/screenshots/motion_toma_3/paper_motion_toma/。採用はイラスト確認/現在ゲームで採用中/透真/の日本語7動作。原画・全文条件・SHA・候補と部位差はart_source/paper_roster_motion/toma/。美術最終承認とテストを区別。

途中1はEdit581/582で旧待機代用期待値1件、Play81/81とWindows成功。2は582/81全4合格だが反撃v1のため採用不可。3でv2と全身768px撮影を再確認。戦闘値・ヒットストップ・判定・攻撃原画・保護カバン不変。Webは今回未更新、8人接続後に最終版を実ビルドして確認する。

次は蓮の専用7動作。ローカルに7候補を保存・透明梱包済み、まだゲーム未接続。回避v1の大きい頭はv2へ修正。湊は走り候補のみ保存、他5人は未着手。これらの制作中データは未コミット。現在検証が走っていないことを確認し、蓮のBuildAll／全4へ。完成人物の検証中は次人物の原画保存だけ進め、検証中のC#とpacked-manifestは変更しない。同時編集なし。既存変更を破棄せず、完了した攻撃原画を重複生成しない。

## 進行中（2026-10-18）：8人の不足7動作、全56原画を追加する新しい依頼を開始

指示書はdocs/codex_tasks/PAPER_ROSTER_SEVEN_MOTION_KEYS.md。透真→蓮→湊→シロ→結衣→黒獅子王→暁→龍太の順に、走り・ジャンプ・落下・回避・カウンター成功・通常被弾・強被弾を各1枚生成して接続する。今は透真の着手段階。新規原画と接続は未完了。既存9人の攻撃原画は完成済みで重複しない。直近の保存済みUnity結果はrework_r4_ryuta_2のEdit574/574、Play79/79、BuildAllとWindows成功。今回の検証ではない。Git開始点8fad993、作業開始時に同時編集がないことを確認。既存差分の破棄なし。

2026-10-18現在：9人の通常4原画・上2枠・下と空中N各1枚・特殊／超必殺まで接続済み。rework_r4_ryuta_2全4合格（Edit574／Play79）、Web実動確認済み。追加の溜め／被弾／他差分は追加指示後。詳細はRESUME冒頭。

2026-10-18現在：暁12枠／9原画と4効果を接続。rework_r4_akatsuki_3全4合格（Edit570／Play77）、542実描画。WebGLも実ビルド／内蔵ブラウザ確認済み。8人完成、次はPAPER_RYUTA_TWELVE_KEYS。追加差分は追加指示後。詳細はRESUME冒頭。

2026-10-18現在：黒獅子王12枠／9人物原画と爪・咆哮・既存部下レイラ／シオン／ノアの5効果を接続。rework_r4_king_2全4合格（Edit563／Play75）、464実描画・専用62PNG。召喚物が重なる描画不具合を各Projectileの位置保持へ修正し、戦闘値は不変。WebGL rework_r4_king_fixed_webも実ビルド・ブラウザ確認・console error 0。次はPAPER_AKATSUKI_TWELVE_KEYS→龍太。追加差分は追加指示後。詳細はRESUME冒頭。

2026-10-17現在：結衣12枠／9原画と打撃・魔法2効果を接続。rework_r4_yui_2全4合格（Edit557／Play73）、384実描画。WebGL rework_r4_yui_mip_webは78.9MB・実ブラウザ19PNG・console error 0。小さい表示の人物のざらつきはWeb用ミップマップで軽減、329画像の元RGBA保存を照合。集合絵v5の透真5指と暁本人左のカードつまみを維持。次はPAPER_KING_TWELVE_KEYS。追加差分は追加指示後。詳細はRESUME冒頭。

2026-10-17現在：シロ12枠／10人物原画とEMP・ALMA効果3種を接続。rework_r4_shiro_2全4合格（Edit551／Play71）、292実描画。WebGL rework_r4_shiro_webも57.6MB・実ブラウザ確認・8PNG・console error 0。集合絵v5の透真5指と暁左手を維持。次はPAPER_YUI_TWELVE_KEYS。空中・下各1枚、通常共有4枚、追加差分は追加指示後。詳細はRESUME冒頭。

2026-10-17現在：湊の12枠／10人物原画と効果3種を接続。rework_r4_minato_2全4合格（Edit547／Play69）、220実描画。採用はイラスト確認/現在ゲームで採用中/湊/。WebGLもrework_r4_minato_webで53.5MBの実ビルドと内蔵ブラウザ確認、8PNG、console error 0。次はPAPER_SHIRO_TWELVE_KEYS。空中・下各1枚、通常共有4枚、追加差分は追加指示後。詳細はRESUME冒頭。

2026-10-17現在：paper_single_title_3で全4合格（Edit543／Play68）。空中・下は各1枚、通常は共有4原画。千鶴の握りと集合絵v5を反映。paper_single_title_webも49.4MBで実ビルド・内蔵ブラウザ確認済み。次はPAPER_MINATO_TWELVE_KEYS。追加差分は追加指示後。

2026-10-17現在：paper_single_title_3全4合格（Edit543／Play68）。空中・下各1枚、通常共有4枚。千鶴の不足と3段の握り・集合絵v5を接続。WebGL更新中。次はPAPER_MINATO_TWELVE_KEYS。追加差分は追加指示後。

2026-10-17最新指定：空中N・地上下攻撃は各1原画（決め絵のみ）。通常は各段2原画を維持し1・2段で共有、計4原画。今後の追加差分はユーザーの追加指示後。旧2枚指定・溜め／被弾の自動追加予定より docs/codex_tasks/PAPER_SINGLE_KEY_AIR_AND_DOWN.md を優先。

# 実装順と交代時の確認表（2026-10-16）

2026-10-17最新：蓮の14原画を実Unity rework_r4_ren_2で全4合格（Edit539／Play67）、左右の48実描画を保存。次はPAPER_CHIZURU_MISSING_KEYS、その後湊から人物順に続ける。WebGLの今回差分は未更新。全員の攻撃後に溜め・被弾を制作する。

2026-10-16最新：paper_release_5全4合格（Edit537／Play66）、投げ終わりと2段選択・掌側の二丁拳銃v3を反映。WebGLもpaper_release_web成功・約47.7MB、ブラウザ確認済み。次は蓮の残10原画の審査／接続。全9人の攻撃・小物・溜め・被弾ゴールは継続。詳細はRESUME冒頭とpaper_release_final。

2026-10-16最新：paper_fourkeys_3全4合格（Edit535／Play63）、通常4原画と透真・蓮の紙表示を完了。全攻撃は未完。次はPAPER_RELEASE_PROPS_AND_ROW_SELECTION、その後PAPER_ACTION_FOUR_KEYS_AND_REMAINING。全員の攻撃後に溜め／被弾を生成する最新指示が、旧「溜め絵生成なし」より優先。本人別の最新通常指定も後者へ記録。詳細はRESUME冒頭。

## 今回の上位工程（2026-10-16追加）

最新指定はSPEC_REVISION_20261016、進行表はcodex_tasks/rework_progress.json。先行していた透真の空中原画より今回の変更を優先。

| 順 | 内容 | 現在 |
|---|---|---|
| R0 | 1〜3秒の溜め通常、蓄積1と可変吹っ飛び、専用絵生成なし | 完了。rework_r0_3でEdit484／Play49・全4工程合格、実描画18PNG |
| R1 | 地上上下・空中Nのみ・滞空廃止・通常相殺・特殊1種・練習超必殺無制限 | 完了。rework_r1_3でEdit499／Play52・全4工程合格、実描画44PNG |
| R2 | 3人ずつ選択し順番に最大3ラウンド・先に2勝、設定整理 | 完了。rework_r2_2でEdit505／Play55・全4工程合格、実描画6PNG |
| R3 | 最新の9人の通常武器・特殊・超必殺 | 完了。rework_r3_3でEdit521／Play57・全4工程合格、37実描画。専用原画はR4 |
| R4 | 9人の攻撃・小物原画（追加差分は指示後） | 完了。rework_r4_ryuta_2でEdit574／Play79・全4合格。通常4原画／下と空中N各1枚、Web更新・9人採用監査済み |
| R5 | 生成した紙舞台・足場・ギミック、3作品別9人集合と同絵の選択 | 完了。21生成レイヤーと9人集合を接続、rework_r5_4でEdit527／Play61・全4工程合格、16実描画 |
| R6 | WebGL実装・ローカルブラウザ実動 | 完了。webgl_5成功・約47.4MB、内蔵Chromium実動。Windows全4工程webgl_native_3でEdit533／Play61合格。実機パッドと通常Chrome／Edge個別は未確認 |

以下は前回までの完了履歴。

ユーザー指定の実装順。着手前に指示書、実装、レビュー、実Unity全4工程、証跡、通常commit／origin mainへのpushの順で区切る。同時編集禁止。完了済みを繰り返さず、RESUME冒頭とGit差分から再開する。

| 順 | ゴール／受入条件 | 現在 |
|---|---|---|
| 1 | 所定位置で登場→FIGHT!、表示中tickと操作を止め、消失時に操作開始。開始前の攻撃押下を持ち越さない | 完了。実入力経路の回帰検査合格 |
| 2 | ハート0／場外は中央KO!、時間切れはTIME UP!。一瞬停止→数秒の勝敗演出。決着後に判定やタイマーを進めない | 完了。6tick静止→2.5秒後に結果 |
| 3 | 勝者を左、敗者を右。各キャラ固有の全身勝敗ポーズ、名前／ステージ名、再戦／キャラ選択／タイトル | 完了。既存固有絵。新規の紙勝敗原画は後続 |
| 4 | 1枚絵全体を段階的に引く→出す→止める、被弾後退、中心でくるくる吹っ飛び。待機の微動・歩き15度・走り45度は維持 | 千鶴で完了。固定tick／実Renderer検査合格 |
| 5 | 千鶴フライパンの横振り／上下振りを各3段×予備と当てる2枚 | 比較12枚と接続を完了、paper_fryingpan_1は463/42。次の変更は横振り統一と判定表示の専用指示書へ |
| 6 | BGM／SEを忙しくコミカルな音へ。既存曲の転載なし。開始／攻撃／ヒット／決着を聞き分け、音量設定を維持 | 試作実装完了。180BPM×4、19SE、短いBGM減衰。WAV5件。好みの試聴確認は残る |
| 7 | 3作品共通の判定で工作舞台／縮小を表示。タイトルとFIGHT／KOを紙の舞台へ | 完了。stage_roster_4で466/44。残る紙ヒット演出はPAPER_STAGE_EFFECTSへ |
| 8 | 9人全員の待機→作品別9人一覧選択、追加6人は共通技の試作として試合可能 | 完了。9人選択／試合開始を確認。透真v2、追加6人の固有技未完成を明示 |
| 9 | 横振り統一／判定ON/OFF・残像／練習スロー・コマ送り／全技点検、おにぎり寸法 | 完了。paper_sideswing_3で470/47。20比較PNG、判定の追加短縮は性能変更のため提案を記録 |
| 10 | 全9人のガード・しゃがみ・カウンター・勝利・敗北を揃える。千鶴は既存再利用、新8人は本人ごとに制作 | 完了。千鶴既存5＋新8人40を接続、paper_core_roster_1で472/48・全4工程合格。実描画112PNG、選定版は各manifest参照。絵の微差は最終確認待ち |
| 11 | 基本5種の後に透真のジャンプ・移動・回避・被弾・攻撃／固有技を揃え、蓮へ展開 | 今回追加R0〜R6の後続。PAPER_TOMA_AIR_AND_MOVEMENTで移動・空中の未移行分から。拳銃／ドローンのユーザー指定を保持 |

## 共通の確定設定

共通仕様は[ペープサート演出方針](PAPER_THEATRE_DIRECTION.md)。原本の戦闘仕様はSPEC_ORIGINAL.md。最新のユーザー変更が優先。普通のアニメ塗りの全身絵で、関節分割／ボーンや大量の中割りは初期実装に使わない。迫力は決めポーズ、絵全体の傾き・位置・拡縮、停止、効果で作る。ゲーム独自演出を原作の正史と混同しない。

カバンは右手、フライパン／おたまは左手。画面の左右と本人の左右は別。胸／背中、肩、肘、掌／甲、親指のつながりを全身で確認する。カバン版は明確な削除指示まで保護。新フライパン案の生成は採用決定ではない。

## 再開・交代

1. tasklistでCodex／Claudeを確認し、AGENTS→HANDOFF→RESUME→STATUS→本書を読む。
2. git statusと現在の指示書を確認。未コミット差分を破棄しない。
3. RESUMEに記載された最初の未完から進める。保存済みテストを今回の結果と呼ばない。
4. 実Unityはtools/verify/run_unity.ps1をサンドボックス外で承認申請。全4工程合格後に通常push。
5. 区切り／停止前にRESUMEとSTATUSへ現在工程、変更ファイル、実件数、画像、残課題、次のコマンドを残す。
