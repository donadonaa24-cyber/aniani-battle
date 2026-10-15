# 全9人の基本5ポーズ：検証区切り

2026-10-16、実Unity paper_core_roster_1：BuildAll成功、Edit472/472、Play48/48、Windowsビルド成功。失敗・スキップ0、4工程の終了値はすべて0。起動：Build/paper_core_roster_1/AniBato.exe。前回の透真だけの検証と区別し、今回新7人も接続して全8新プロフィールを確認。今回のテスト失敗なし。

千鶴の既存5種類と新8人40種類で全45。千鶴の敗北は従来の座り込み、それ以外の新8人は控えめな立ち姿。

| 人物 | 選定版 |
|---|---|
| 透真 | guard v3／crouch v2／counter v4／win v1／lose v1 |
| 蓮 | guard v2／crouch v1／counter v1／win v1／lose v1 |
| 湊 | guard v1／crouch v1／counter v1／win v1／lose v1 |
| シロ | guard v2／crouch v2／counter v1／win v1／lose v1 |
| 結衣 | guard v2／crouch v3／counter v1／win v2／lose v1 |
| 黒獅子王 | guard v1／crouch v1／counter v1／win v1／lose v1 |
| 暁 | guard v1／crouch v3／counter v1／win v1／lose v1 |
| 龍太 | guard v1／crouch v2／counter v1／win v1／lose v1 |

原画55候補から40を選定。大きくなった部位、原画端切断、手と肩の問題、しゃがみの浅い姿勢があった候補は不採用と記録。原画・全文条件・参照役割・SHAを保存。頭・靴の投影と露出に微差が残り、全ポーズの輪郭完全一致やユーザーの最終承認とは扱わない。しゃがみは部位寸法を優先して約72〜81%の全高で、当初60〜65%目安との差は記録。

1304×1450／PPU532.5893／接地1328、人物の再サンプルなし。しきい値128の外周アルファ整理と透明余白のみ。check_paper_coreで原画SHA、RGBと不透明内部、整理後画素、接地と高さを40枚照合。元RGBA完全一致と区別する。採用状態更新の前後でruntime-manifestのSHAが一致し、検証後にゲーム用データを変えていない。

実入力の歩行15度・ガード・しゃがみ・カウンター・解除と左右表示、勝敗表示を実Unityで確認。docs/screenshots/paper_core_<key>/へ各14枚、計112PNG。実Camera.Renderの切り出しでOSキーなし。ガード青／カウンター黄は既存状態色。イラスト確認/全9人_基本5ポーズ/の5一覧と06_Unity実描画_新8人.png、各キャラの基本5ポーズ_ゲーム採用済み/を用意。元の確認中フォルダと二重表示しない。

通常45件の半減値とゲージ14/10、当たり判定・物理・発生・ダメージを変更していない。check_paper_sourcesは千鶴46条件とカバン13原画、おにぎりの手持ち／飛翔表示幅一致を確認。カバン版とフライパン予備を保持。

一時保存エラー：黒獅子王counterでコピー後のmanifest更新がOneDriveのUNKNOWNにより失敗。ツール出力と同一SHAの保存済み原画から記録を復旧し、再生成しなかった。保存補助に対象を絞った短い再試行と同一SHAの再登録を追加。別原画の上書きは拒否。Unityのテスト失敗とは別。

次はPAPER_TOMA_AIR_AND_MOVEMENTに沿って透真の未制作動作。透真／蓮には旧ピクセル動作、追加6人には待機変形と共通技が残る。全アクションと固有技が完成したと誤認しない。ClaudeへはRESUME冒頭とprogressから交代可能。
