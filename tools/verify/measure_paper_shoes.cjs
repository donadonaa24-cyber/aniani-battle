// Compare the major-axis length of white sneaker paint, independent of shoe rotation.
// Read-only analysis: no source illustration is resized or edited.
const sharp = require('sharp');
const path = require('node:path');
const fs = require('node:fs');
async function measure(file, regions) {
  const input = await sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  const { width, height } = input.info;
  const lengths = regions.map(([x0, y0, x1, y1]) => {
    const points = [];
    for (let y = y0; y < Math.min(height, y1); y++) for (let x = x0; x < Math.min(width, x1); x++) {
      const i = (y * width + x) * 4, [r, g, b, a] = input.data.subarray(i, i + 4);
      if (a >= 128 && Math.min(r, g, b) > 150 && Math.max(r, g, b) - Math.min(r, g, b) < 35) points.push([x, y]);
    }
    if (points.length < 500) throw new Error('Insufficient sneaker paint in measurement region.');
    const cx = points.reduce((s, p) => s + p[0], 0) / points.length, cy = points.reduce((s, p) => s + p[1], 0) / points.length;
    let xx = 0, xy = 0, yy = 0;
    for (const [x, y] of points) { xx += (x - cx) ** 2; xy += (x - cx) * (y - cy); yy += (y - cy) ** 2; }
    const angle = .5 * Math.atan2(2 * xy, xx - yy);
    const projected = points.map(([x, y]) => x * Math.cos(angle) + y * Math.sin(angle)).sort((a, b) => a - b);
    return { pixels: points.length, majorAxisPx: +(projected[projected.length - 1] - projected[0]).toFixed(2) };
  });
  return { file: path.relative(process.cwd(), file).replaceAll('\\', '/'), width, height, lengths };
}
async function main() {
  const folder = path.resolve('art_source/paper_cutout/generated');
  const measurements = [];
  measurements.push(await measure(path.join(folder, 'chizuru_paper_idle_v1.png'), [[340,1080,540,1263],[745,1090,990,1263]]));
  for (const version of [1, 2, 3]) measurements.push(await measure(path.join(folder, `chizuru_paper_run_v${version}.png`), [[70,680,350,1000],[840,1010,1246,1263]]));
  fs.writeFileSync('art_source/paper_cutout/shoe-comparison.json', JSON.stringify({ method: 'Opaque light sneaker paint PCA major-axis length; rotation-independent, excludes skin and cream paper edge.', measurements }, null, 2) + '\n');
  process.stdout.write(JSON.stringify(measurements.map(m => ({ file:m.file, size:[m.width,m.height], shoeLengths:m.lengths.map(l=>l.majorAxisPx) })))+'\n');
}
main().catch(error => { process.stderr.write(error.stack+'\n'); process.exitCode = 1; });
