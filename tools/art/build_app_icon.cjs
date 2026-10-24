// Deterministic packaging only: the original artwork is generated with image_gen.
const fs = require('node:fs');
const path = require('node:path');
const sharp = require('sharp');
const root = path.resolve(__dirname, '../..');
const source = path.join(root, 'art_source/app_icon/anibato-icon-original-v1.png');
const output = path.join(root, 'AniBato/Assets/_Project/Art/Branding');
async function main() {
  fs.mkdirSync(output, { recursive: true });
  await sharp(source).resize(1024, 1024).png().toFile(path.join(output, 'app-icon-legacy.png'));
  // Adaptive icons expose a 72/108 viewport; leave enough space for launcher masks.
  const foreground = await sharp(source).resize(656, 656).png().toBuffer();
  await sharp({create:{width:1024,height:1024,channels:4,background:'#092342'}})
    .png().toFile(path.join(output, 'app-icon-background.png'));
  await sharp({create:{width:1024,height:1024,channels:4,background:'#00000000'}})
    .composite([{input:foreground,left:184,top:184}]).png()
    .toFile(path.join(output, 'app-icon-foreground.png'));
  console.log('Packaged legacy, adaptive background and safe-zone foreground (1024 px).');
}
main().catch(e=>{ console.error(e);process.exitCode=1; });
