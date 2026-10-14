# スキルとサポートの拡張位置（2026-10-15）

今回9人分の固有技を作ったものではない。追加6人は各作品の既存基本セットを共有する試作キャラ。定義を共有し、調整済み元アセットを書き換えない。

|予定効果|現在の基盤／次の拡張位置|
|---|---|
|通常被弾の蓄積−1|SkillSystem.Heal／AttackDefinition.SelfHealNormalHits。0未満にしない|
|通常を一定時間防ぐ|全攻撃ガードは既存。独立した時間制バリアはFighterStateの残tickと防御解決に追加し、通常ガードのスタミナ・ジャストと優先順を定義する|
|短い麻痺|AttackDefinition.ParalysisFrames／SkillSystem.OnHit、上限45tick。既存おにぎり30tick|
|移動速度上昇|BuffDefinition.MoveMultiplier／DashMultiplier、DurationFrames|
|特殊強化|発生短縮はStartupMultiplierあり。威力等は専用係数と適用対象を定義し、超必殺へ暗黙適用しない|
|飛び道具防御|ProjectileEraseCountあり。時間制なら残tick・消せる数を定義し、ガードとは別に解決する|
|反撃成功の追加効果|ICounterEffect／DataCounterEffect、回復・速度・吹っ飛び補正は実装済み|
|条件付きハート回復|未実装。発動条件／上限3／試合ごとの回数・クールダウンを定義、SkillSystemへ集中し、勝敗確定後の復活を禁止する|

「まかないタイム」は現在の「ひとくち休憩」（蓄積−1、クールダウン240tick）を簡易版として利用。「爆弾おにぎり」は既存の投射＋麻痺30tickを利用。将来呼称を変更するときも挙動を二重実装しない。

純C#固定tickの定義／FighterState／SkillSystem／防御解決へ追加し、Unityは結果表示だけを読む。上書きか重複か、持続の延長、ヒットストップ中の残tick、場外・ハート0・時間切れとの優先順、上限と再使用、同一攻撃の多重発動をテストする。サポートは同じ効果定義へ接続し、キャラ名の分岐を増やさない。
