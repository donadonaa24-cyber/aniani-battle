# 生成シーン

`AniBato.EditorTools.ProjectBuilder.BuildAll` がUnity Editor APIで `Battle.unity` を生成し、Build Settingsへ登録します。手書きYAMLは使用しません。

フェーズ2のシーン生成はClaudeが確認済みです。更新後はUnityメニュー「**あにバト → フェーズ3プロジェクト生成**」で再生成し、3キャラ・BattleSetup・超必殺カットイン・結果画面の仮キャラ切替を追加してください。フェーズ3のUnity実行はClaudeによる確認待ちです。

ART1ではProjectBuilder.BuildAllを再実行して、千鶴のAnimationSet/私服PortraitとF9判定表示を含むBattleへ更新してください。現存シーンは旧生成物です。CodexはUnity実行未検証で、ClaudeがBuildAllとEditMode182/PlayMode10件を実行します。
