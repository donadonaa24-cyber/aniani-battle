# フェーズ6A：旧ルールのテスト置換と必須24項目

原本ファイルは変更しない。ユーザーの追加決定（2026-10-12）により、原本12章の7・13を次の新ルールへ置き換える。テストを削除したり成功条件を緩めたりせず、成立イベント・蓄積・ハート・消費量を具体的に検査する。

| 旧テスト | 新テスト/検査 |
|---|---|
| Spec07_NormalPiercesBothFreshAndOldGuard | UserDecision_NormalIsBlockedByBothFreshAndOldGuard：蓄積0、ハート3、新鮮ガードはJustGuard、古いガードはGuarded |
| Spec13_14_UltimatePiercesOldGuardAndNormalDodge(Guard/Dodge) | UserDecision_UltimateIsGuardedButPiercesNormalDodge：ガードはハート3＋Guarded、通常回避はハート1でGuardedなし |
| AlreadyOverlappingActiveUltimateCannotBecomeJustDefense(Guard/Dodge) | 同名でJustなしを保持。ガードは通常ガード成立、通常回避はハート2を失う |
| MatchResultPlaysWinnerAndThenLoserRecoveryWithoutAdvancingSimulation | MatchResultUsesCalmWinAndLoseFromFirstResultTickWithoutAdvancingSimulation：最初の結果tickから25tickすべてWin/Lose、戦闘tick不変 |
| GeneratedAssetsConvertToValidSimulationDefinitionsのSchema5検査 | Schema6を厳密一致、方向枠10・9技Normal・スタミナ正・機能Schema6も検査 |
| FixedSeedSmallStudyLocksTempoShrinkAndUltimateTargets | 現行PhaseSixADefaultsへ入力データを更新。35〜55秒・20〜40%・50〜70%・自滅0のアサーションを維持 |

新規PhaseSixBattleTests/LearningTestsで、スタミナ十分/不足/ジャストとガード崩しを別々に検証。旧防御テストの分類や蓄積・コンボ上限・カウンター・回避の検証も継続する。

| 原本番号 | 現行の対象 | 検証 |
|---|---|---|
| 1〜4 | 通常1/2/3、コンボ切れ後蓄積 | DamageTests (.NET) |
| 5 | 特殊後の蓄積保持 | DefenseTests/CharacterTests/TempoTests (.NET) |
| 6 | 3コンボ・吹っ飛び・追撃禁止 | DamageTests/DefenseTests/TempoTests (.NET) |
| 7（変更） | 通常ガード可、しゃがみが崩す＋蓄積1 | DefenseTests/PhaseSixBattleTests (.NET) |
| 8〜11 | 特殊ガード、カウンター成功/失敗/蓄積 | DefenseTests/PhaseSixBattleTests (.NET) |
| 12 | 超必殺のハート2 | DefenseTests/TempoTests (.NET) |
| 13（変更） | スタミナ十分ならガード、不足なら貫通 | PhaseSixBattleTests (.NET) |
| 14 | ジャストガード/回避 | DefenseTests/PhaseSixBattleTests (.NET) |
| 15〜18 | 場外・70秒・縮小・時間切れ | MatchTests/StageShrinkTests (.NET) |
| 19 | 3キャラの操作 | CharacterTests (.NET)、BattleSceneTests (Unity待ち) |
| 20 | CPU対戦 | CpuTests、810試合集計 (.NET)、GameFlowSceneTests (Unity待ち) |
| 21〜23 | ローカル2人・再戦・頭上追従 | 既存PlayModeのアサーションを維持、Unity待ち |
| 24 | 多重ヒット防止・高FPS決定性 | DamageTests/InputAndClockTests/TempoTests/PhaseSixBattleTests (.NET)、144Hz描画PlayMode (Unity待ち) |

Unity未実行を合格とは扱わない。新規PlayMode3件は実DualSense/XInputテストデバイス、設定UI→保存→初期化、練習/技表の停止を確認する。InputTestFixtureのUnitySetUp冒頭の分離とUnityTearDown末尾の復元を保持し、設定テストでは対象4キーのPlayerPrefsだけを退避・復元する。
