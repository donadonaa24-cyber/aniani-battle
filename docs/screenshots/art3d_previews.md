# ART3D：実機撮影は未保存（2026-10-13）

Unity全4工程が合格したBuild/art3d/AniBato.exeに対し、事前に「PCに触らないで」と案内してtools/verify/play_and_capture.ps1を実行。Windowsがゲームを最前面にできず、安全チェックでキー送信前に停止。PNGは保存されておらず、Windows実機画面の目視確認は未完了。

素材の[姿勢比較/GIF](../art_wip/art3d_previews.md)は実機のスクリーンショットとは扱わない。Unity PlayModeでは3人の実入力→2tick接地予備→踏切と、ガード/回避/ダッシュの実SpriteRenderer描画を検証済み。

次の担当は、PCを使わない時間に再撮影するか、Build/art3d/AniBato.exeをユーザーが手動で起動して確認する。再撮影前にも必ずPC操作禁止を案内し、最前面チェックを無効にしない。
