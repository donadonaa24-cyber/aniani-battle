# 紙の舞台の音（2026-10-19）

メニューと3舞台の新規オリジナルBGM4曲、紙・厚紙・木の打音を中心とする19SEへ置換。24kHzの自作PCM合成。旋律／和声／ベース／打楽器と16小節の展開を持たせ、曲切替0.35秒のクロスフェードと滑らかなBGM抑制を追加。BGM/SE別音量と戦闘／原画は維持。設定アセットは一度だけ移行し、以後の調整を保持する。

今回のpaper_audio_1はBuildAll成功、Edit616/616、Play85/85、Windows成功。失敗・スキップ0。実AudioClipと音声再生、4曲切替、時間停止中のBGM抑制と復帰を確認。paper_audio_webも成功（123,667,948bytes／241.144秒）、Windows復帰成功、435/435画像のRGBA保持を確認。新ビルドを内蔵Chromiumで起動し、音声有効化・メニュー・料理の舞台・特殊／超必殺を実キーで確認。音声running、音量0.35/0.65保持、console error/warn 0。

試聴は音声確認/現在ゲームで使用中/、測定値とSHAは音声一覧.json。BGMピーク0.620～0.637、RMS0.088～0.102、SEピーク最大0.820。ユーザーの音楽的な好みの最終承認とは区別。記録はpaper_audio_verification.jsonとdocs/screenshots/paper_audio_web/。

全体テストの自動撮影937件はTestResults/paper_audio_1_auto_captureへ保存。履歴付き194件は一対一の退避SHA一致後に履歴を維持し、無関係な撮影の大量差分を音のcommitへ混ぜない。次回はrun_unity.ps1に -CaptureRoot TestResults/<タグ>/captures を付ける。Windows最新2版、TestResults直近3回とdotnet類を保持。

この音工程の残課題なし。湊の引力は先行25a121cで保存済み。通常Chrome/Edge個別や実機ゲームパッドの追加検査は今回なし。今後はユーザーの試聴／追加指示に応じて調整する。
