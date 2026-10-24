# Android専用アプリアイコン（2026-10-24）

所有者依頼：Androidのホーム画面に表示されるアイコンを「あにあにバトル！！」独自のものにする。進行中の操作/HUD/カメラ改善の配布更新へ含める。

- 紙の舞台に合う専用正方形原画を生成し、原本と全文プロンプトを保存する。
- 小さい表示、円形・角丸マスクで主題が切れない構成。AndroidのAdaptive背景/前景、Legacy/Roundを設定する。
- Editorスクリプトから再現可能に設定し、BuildAllで消えないようにする。ゲーム内の戦闘・キャラクター原画は変更しない。
- 実UnityのBuildAll/EditMode/PlayMode/Windowsを確認し、Androidを再構築。APK内の実際のランチャーリソースと署名・起動Activityを検査する。
- 既存のユーザー実機起動報告は旧APKのもの。新APKの実機確認と混同しない。
- 操作/HUD/カメラ改善・WebGL/Windows/Androidの公開更新を完了し、RESUME/STATUSへ結果を記録する。
