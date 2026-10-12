6Bのソース・素材・資料を更新しました。**commit はしていません。**

- PlayMode失敗2件を修正。DualSenseの差分入力拒否へUSBレポートで対応し、機器と旧シーンの後始末を補強しました。アサーションは維持しています。[Unity実装](https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/InputSystem/Runtime/Plugins/DualShock/DualShockGamepadHID.cs)
- 生成済み素材を確認・再利用し、背景3種、サムネ、中割り18姿勢、ダッシュ2回入力ON/OFFを接続。重ね合わせはありません。
- .NET **307件合格**、参照コンパイル警告・エラー0、CPU810試合、584F・106GIF・再生成一致を確認。
- 必須24テスト・成果物12項目を整理し、STATUS・RESUME等を更新。7・13のガード仕様変更も明記しました。

**Unity再検証とカメラ最終調整は残っています。** 全体常時表示と常時拡大は未達です。Claudeには失敗2件の解消、DualSense、3画面比率、144Hz、縮小演出を確認してほしいです。

[最終報告](docs/codex_logs/phase6b_final.md) ／ [before・after一覧](docs/screenshots/phase6b_previews.md)（Unity実画面の代替画像）