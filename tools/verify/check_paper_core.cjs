// Independently audit the saved source -> alpha cleanup -> padded PNG contract.
const fs = require('node:fs'), crypto = require('node:crypto'), sharp = require('sharp');
const root = 'art_source/paper_roster_core';
const read = p => JSON.parse(fs.readFileSync(p, 'utf8').replace(/^\uFEFF/, ''));
const sha = p => crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const requireThat = (condition, message) => { if (!condition) throw Error(message); };
async function pixels(file) { return sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true }); }
async function main() {
  const runtime = read(root + '/runtime-manifest.json'), results = [];
  requireThat(runtime.characters.length === 8, 'Expected eight newly generated profiles');
  for (const character of runtime.characters) {
    const manifest = read(`${root}/${character.key}/manifest.json`);
    requireThat(character.poses.length === 5, character.key + ': five poses missing');
    for (const pose of character.poses) {
      const job = manifest.jobs.find(j => j.pose === pose.key);
      const entry = manifest.entries.find(e => e.pose === pose.key && e.version === job.version);
      requireThat(entry && sha(entry.source) === entry.sha256, pose.source + ': original SHA');
      requireThat(sha(entry.prepared) === pose.sha256 && sha(entry.edgeCleaned) === entry.edgeCleanedSha256, pose.key + ': output SHA');
      const imported = `AniBato/Assets/_Project/Art/PaperCore/${character.key}_${pose.key}.png`;
      requireThat(sha(imported) === pose.sha256, imported + ': game PNG differs from verified source');
      const raw = await pixels(entry.source), clean = await pixels(entry.edgeCleaned), packed = await pixels(entry.prepared);
      const { width, height } = raw.info;
      requireThat(width === clean.info.width && height === clean.info.height, pose.key + ': cleanup changed dimensions');
      requireThat(packed.info.width === 1304 && packed.info.height === 1450, pose.key + ': shared frame');
      let bottom = -1;
      for (let i = 0; i < raw.data.length; i += 4) {
        for (let channel = 0; channel < 3; channel++) requireThat(raw.data[i + channel] === clean.data[i + channel], pose.key + ': RGB changed');
        const alpha = raw.data[i + 3] >= 128 ? 255 : 0;
        requireThat(clean.data[i + 3] === alpha, pose.key + ': alpha threshold');
        const x = i / 4 % width, y = Math.floor(i / 4 / width), xx = x + entry.offsetX, yy = y + entry.offsetY;
        if (xx < 0 || xx >= 1304 || yy < 0 || yy >= 1450) {
          requireThat(alpha === 0, pose.key + ': visible source cropped'); continue;
        }
        const target = (yy * 1304 + xx) * 4;
        for (let channel = 0; channel < 4; channel++) requireThat(clean.data[i + channel] === packed.data[target + channel], pose.key + ': source resampled or changed');
        if (alpha) bottom = Math.max(bottom, yy);
      }
      for (let y = 0; y < 1450; y++) for (let x = 0; x < 1304; x++) {
        if (x < entry.offsetX || x >= entry.offsetX + width || y < entry.offsetY || y >= entry.offsetY + height)
          requireThat(packed.data[(y * 1304 + x) * 4 + 3] === 0, pose.key + ': visible padding');
      }
      requireThat(bottom === 1328, pose.key + ': sole baseline');
      if (pose.key === 'crouch') requireThat(entry.opaqueHeightRatio < .85, character.key + ': crouch insufficiently low');
      results.push({ character: character.key, pose: pose.key, version: entry.version, sourceSha256: entry.sha256, preparedSha256: pose.sha256,
        sourceRgbPreserved: true, opaqueInteriorPreserved: true, cleanedPixelsPreserved: true, importedGamePngPreserved: true, noResampling: true, sole: bottom });
    }
  }
  requireThat(results.length === 40, 'Expected forty selected new poses');
  fs.writeFileSync(root + '/pixel-audit.json', JSON.stringify({ schemaVersion: 1, count: results.length, note: 'Alpha cleanup changes edge alpha only; original RGBA equality is not claimed.', results }, null, 2) + '\n');
  console.log(JSON.stringify({ selected: results.length, sourceRgbAndOpaqueInteriorPreserved: true, packedWithoutResampling: true, frame: '1304x1450', sole: 1328 }));
}
main().catch(error => { console.error(error.message); process.exitCode = 1; });
