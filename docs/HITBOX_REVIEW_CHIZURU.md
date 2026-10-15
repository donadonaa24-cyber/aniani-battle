# 千鶴の絵と判定の点検（2026-10-15）

実Unityの固定状態描画。攻撃有効フレームの定義とキー原画を設定し、実FighterView／HitboxDebugView／ProjectileViewをCamera.Renderで比較した。対戦中の発生・持続・命中は別の実入力回帰テストで検証。数値は本人中心からのworld座標。灰色抽出のフライパン範囲は柄を含む推定で、最終判断は画像目視。

|技／状態|絵の道具の横範囲（推定）|攻撃の横範囲|点検／対応|
|---|---|---|---|
|[千鶴 地上1 / Attack1](screenshots/hitbox_review_chizuru/Attack1.png)|-0.335～1.484|0.3～1.8|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 地上2 / Attack2](screenshots/hitbox_review_chizuru/Attack2.png)|-0.593～1.57|0.3～1.8|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[フライパン大振り / Attack3](screenshots/hitbox_review_chizuru/Attack3.png)|-0.58～1.523|0.3～1.8|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 足払い / CrouchAttack](screenshots/hitbox_review_chizuru/CrouchAttack.png)|-0.58～1.523|-0.19～0.85|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 背面打ち / BackAttack](screenshots/hitbox_review_chizuru/BackAttack.png)|-1.57～0.593|-1.69～-0.39|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 空中ニュートラル / AirNeutral](screenshots/hitbox_review_chizuru/AirNeutral.png)|-0.628～1.19|-0.85～0.85|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 空中前 / AirForward](screenshots/hitbox_review_chizuru/AirForward.png)|-0.628～1.19|0.39～1.69|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 空中後ろ / AirBack](screenshots/hitbox_review_chizuru/AirBack.png)|-1.277～0.886|-1.69～-0.39|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 空中上 / AirUp](screenshots/hitbox_review_chizuru/AirUp.png)|-0.681～0.581|-0.65～0.65|横範囲を画像で点検。現判定を維持|
|[千鶴 空中下 / AirDown](screenshots/hitbox_review_chizuru/AirDown.png)|-0.39～0.883|-0.45～0.85|横範囲を画像で点検。現判定を維持|
|[千鶴 空中斜め前上 / AirForwardUp](screenshots/hitbox_review_chizuru/AirForwardUp.png)|-0.898～1.029|0.39～1.69|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[千鶴 空中斜め前下 / AirForwardDown](screenshots/hitbox_review_chizuru/AirForwardDown.png)|-0.58～1.106|0.39～1.69|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[爆弾おにぎり / SpecialNeutral](screenshots/hitbox_review_chizuru/SpecialNeutral.png)|-0.203～0.863|0.75～1.65|飛翔は紙版。手持ちを拡大し両方約0.42幅。投射判定は維持|
|[フライパン突撃 / SpecialSide](screenshots/hitbox_review_chizuru/SpecialSide.png)|0.682～1.502|0～2.6|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[おたま旋風 / SpecialUp](screenshots/hitbox_review_chizuru/SpecialUp.png)|-0.285～1.146|-1.2～1.2|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[ひとくち休憩 / SpecialDown](screenshots/hitbox_review_chizuru/SpecialDown.png)|-0.078～0.989|なし|非攻撃の回復技。攻撃判定なし|
|[満腹カレー！ / Ultimate](screenshots/hitbox_review_chizuru/Ultimate.png)|-0.111～1.354|-3.5～6.5|絵と横範囲に差。画像参照。半減済み判定の大幅再変更はせず調整案を後述|
|[やられ判定 Idle](screenshots/hitbox_review_chizuru/Hurtbox_Idle.png)|頭髪・服・靴は緑枠と完全一致させない|-0.55～0.55|しゃがみの高さ縮小と空中の全身を画像で確認。体格の仕様変更は保留|
|[やられ判定 Crouch](screenshots/hitbox_review_chizuru/Hurtbox_Crouch.png)|頭髪・服・靴は緑枠と完全一致させない|-0.55～0.55|しゃがみの高さ縮小と空中の全身を画像で確認。体格の仕様変更は保留|
|[やられ判定 Fall](screenshots/hitbox_review_chizuru/Hurtbox_Fall.png)|頭髪・服・靴は緑枠と完全一致させない|-0.55～0.55|しゃがみの高さ縮小と空中の全身を画像で確認。体格の仕様変更は保留|

## 調整案と保留理由

通常45件の半減とゲージ14/10を保持。今回、判定の数値変更は0件。頭身の大きい紙絵ではフライパン・手足と判定の終端が必ずしも一致しない。横振りに対して長い通常判定、回転した上下空中技の縦方向、復帰技・超必殺の広い効果範囲が調整候補。通常の範囲をさらに絵へ短縮する場合は近接戦の成立とCPU／ゲージ頻度も変わるため、ユーザー確認後に一技ずつ調整する。

横振りの上下方向は原画を追加せず全身の固定傾き（上60度／下−60度／斜め±30度）で表示。フライパンの柄も抽出範囲に含まれるので、数値だけで判定を自動修正しない。緑枠は髪／服の全輪郭を覆うためのものではなく、しゃがみ・空中を含む戦闘用のやられ判定。

表示はF9／ポーズ／メニュー設定。緑＝やられ、赤＝有効、黄＝飛翔、薄赤＝予備。練習はF5速度、F6静止、F7単一tick。
