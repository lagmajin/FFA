# Repository instructions link

This repository references the centralized Copilot instructions used by the development environment.

Please consult the external Copilot instruction file at:

- `..\\..\\..\\copilot-instructions.md`

That file contains guidance for assistant/user interaction and should be followed when making changes.

(If you prefer the instructions copied into this repository, tell me and I will add them into `copilot-instructions.md` here.)

## 警告: CS0108 への注意
- このリポジトリで変更を行う際は、コンパイラ警告 `CS0108`（基底クラスのメンバーを隠蔽している）を生じさせないよう注意してください。
- 衝突が起きる場合はまず名前を変更するか、基底が `virtual`/`abstract` の場合は `override` を使用してください。
- やむを得ず `new` による隠蔽を行う場合は、必ずコード内に理由コメントを残してください。
- 変更後は必ずビルドして警告が出ていないことを確認してください。