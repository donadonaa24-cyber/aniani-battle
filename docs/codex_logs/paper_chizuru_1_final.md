# 千鶴の紙人形待機・一枚絵歩行（2026-10-14）

千鶴のIdle/Moveだけ、生成済みの透明イラスト1枚へ切り替えた。原画を同一バイトでUnityへ保存し、足元支点の傾きと上下移動を段階的に切り替える。顔/服/部位の変形やAIの別ポーズ生成は不要。

- 旧千鶴と同じ身長2.24u。alpha32以上の外形からPPUを算出、X中央/足の最下端をピボットに設定。Bilinear/Uncompressed/NPOTNone/mipmapなし。
- PaperCharacterMotion SO：待機±0.75度/4fps、歩行±5度/8fps/上下0.025u。8段階/1秒反復、足の左右幅から持ち上げを補正して床沈みを防ぐ。左右反転にも対応。
- 固定tickのAnimationSample.Ageを使用。描画を繰り返しても位相が進まず、ポーズ/ヒット停止のAge停止へ追従。攻撃等で子の回転/上下を消し、再戦/キャラ変更で持ち越さない。頭上表示も高さ/傾き/上下へ追従。
- Idle/Move以外は旧アニメ、透真/蓮は紙表示なし。SOのEnabled OFFで元の千鶴へ戻せる。戦闘シミュレーション/速度/入力/判定/スタミナは変更していない。

| 検証 | 今回の実結果 |
|---|---|
| Unity BuildAll | 成功 exit0 |
| Unity EditMode | 441/441、失敗/スキップ0 |
| Unity PlayMode | 36/36、失敗/スキップ0 |
| Windowsビルド | 成功 exit0/Success、Build/paper_chizuru_1/AniBato.exe |
| API事前コンパイル | 警告0/エラー0（Unityの代わりと扱わない） |

新Edit17件：8位相、周期、保持、両向き床補正、待機、データ検証、原画SHA/PPU/ピボット/千鶴だけ有効。新Play4件：実Rendererの待機/左右/全8位相/保持/移動距離、ポーズ/攻撃/再戦、両スロット/他キャラ、Enabled OFF。旧クリップの画素/寸法アサーションを維持し、ライブ千鶴の期待だけ変更仕様に対応する厳密な紙Sprite参照へ更新した。証跡はTestResults/paper_chizuru_1_*。

Unity実Cameraから18PNGを保存して目視。単体の拡大描画idle.png、左右各8コマ、ゲームCameraのscene_idle.png（Overlay HUDは含まない）。[右歩行GIF](../screenshots/paper_chizuru_unity/walk_right.gif)/[左歩行GIF](../screenshots/paper_chizuru_unity/walk_left.gif)は実描画PNGを再描画せず形式変換した1秒ループ。画像処理ライブラリsharpでGIFの8ページを再確認。実機のOSキー入力/モニター撮影は行っていない。

既存の原画/ピクセル素材/停止中の透真差分を保全。透真の拳銃原画v2とドローンv2は準備出力まで、新しい技の戦闘仕様は未実装であり、本件で再開していない。最新ビルド2版はpaper_chizuru_1/art3e。

残課題：千鶴の走り/跳躍/攻撃/防御/被弾/勝敗は旧ピクセルで、試作中は画風が混在する。次はユーザーにこの揺れを確認してもらい、角度/速度と他動作の展開を決める。実機で試す場合は最新ビルドのキャラ選択で千鶴を選ぶ。今後play_and_captureを使う前は必ず「PCに触らないで」。この合格区切りを通常commit/origin mainへpushし、反映SHAをRESUMEへ記録する。
