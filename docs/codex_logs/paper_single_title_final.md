# 空中・下1原画と千鶴・集合絵の修正


全9人の空中Nと下は決め絵1枚。通常1・2段共有2枚＋3段大振り2枚は維持。追加差分はユーザーの追加指示後。最新仕様はPAPER_SINGLE_KEY_AIR_AND_DOWN。千鶴は下・空中の決め、上2枚、空のカレー皿の決めと3段左手の掌側の握りv2を接続。実発射・96tick持続・重力・性能・半減45判定・ゲージ14/10は保持。

集合絵v5を採用。透真は指先5個、暁は本人左の掌側でカード下端をつまみ、湊・暁・龍太の左向きを維持。v2の6指とv3／v4の不自然な保持は不採用、拡大原画を参照して再編集した。カード背面はユーザー添付を参照。タイトルと選択は同じSprite、9つの顔領域を再計測。

今回paper_single_title_3：BuildAll成功／Edit543/543／Play68/68／Windows成功、失敗・スキップ0。実描画144PNGはdocs/screenshots/paper_single_title_3/。WindowsはBuild/paper_single_title_3/AniBato.exe。初回のRect読込と旧2枚前提の問題を修正。2でも全4合格したが旧集合v2なので、5指と左手カード保持の最終証跡は3のみ。

現在の画像はイラスト確認/現在ゲームで採用中/。透真・蓮・千鶴は攻撃12ユニーク原画、残6人は専用攻撃未完成。原画候補・修正前はart_sourceへ残し、採用一覧.jsonと区別。保護カバン13原画は保持。美術の最終承認は別。

WebGLもpaper_single_title_webで実ビルド成功（49,415,100bytes／6ファイル）。内蔵Chromiumで修正版集合トップ・同じ絵の9人2段選択・左右キー・千鶴の下／空中1枚・コマ送り・音声runningを確認。console error 0。ブラウザ5PNGとverification.jsonはdocs/screenshots/paper_single_title_web/。検査URLを通常URLへ戻した。実機パッドと通常Chrome／Edgeは今回再確認していない。再起動後はtools/verify/start_webgl.ps1で起動する。

次はPAPER_MINATO_TWELVE_KEYS、湊→シロ→結衣→黒獅子王→暁→龍太の順。空中・下を各1枚だけ生成し、ほかの追加差分は追加指示後。全体ゴールはactive。再開時はtasklist、Git差分、実行中UnityとWebGLログから確認し、完了分を重複生成せず既存差分を破棄しない。

保存前レビュー：純C#戦闘・AttackDataの差分なし。通常4原画の共有・おにぎり実発射同期・空中重力を既存検査で維持。JSONのRectを専用DTOへ修正し、選択枠の寸法と非重複の検査は維持。現在の全身／原画画素監査も最新1枚採用へ合わせ、検査を省略していない。Unity自動生成シーンの空欄末尾空白は自動出力のまま。
