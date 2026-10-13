// Encode actual Unity camera captures; never redraw or modify the source illustration.
const fs = require('node:fs');
const path = require('node:path');
const sharp = require('sharp');

async function main() {
  const root = path.resolve(__dirname, '../..');
  const folder = path.join(root, 'docs/screenshots/paper_chizuru_unity');
  for (const direction of ['right', 'left']) {
    const frames = [];
    for (let phase = 0; phase < 8; phase++) {
      const name = `walk_${direction}_${String(phase).padStart(2, '0')}.png`;
      const input = await sharp(path.join(folder, name)).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
      if (input.info.width !== 512 || input.info.height !== 512 || input.info.channels !== 4) {
        throw new Error(`Unexpected Unity capture dimensions: ${name}`);
      }
      frames.push(input.data);
    }
    const output = path.join(folder, `walk_${direction}.gif`);
    await sharp(Buffer.concat(frames), { raw: { width: 512, height: 512 * 8, channels: 4, pageHeight: 512 } })
      .gif({ loop: 0, delay: [130, 120, 130, 120, 130, 120, 130, 120], colours: 256, dither: 0, effort: 3 })
      .toFile(output);
    const metadata = await sharp(output, { animated: true }).metadata();
    if (metadata.pages !== 8 || metadata.pageHeight !== 512) throw new Error('GIF frame count changed.');
    process.stdout.write(`${direction}: Unityの実描画8コマを1秒ループのGIFに保存しました。
`);
  }
}
main().catch(error => { process.stderr.write(`${error.stack}
`); process.exitCode = 1; });
