// Stream public release assets without storing another full copy of each package.
const fs = require('node:fs');
const {createHash} = require('node:crypto');
const assert = require('node:assert/strict');
const manifestPath = process.argv[2] || 'TestResults/portal_release/anibato/downloads.json';
const output = process.argv[3] || 'TestResults/native_downloads_http.json';
const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
(async () => {
  const results = [];
  for (const file of manifest.files) {
    const expected = `https://github.com/donadonaa24-cyber/aniani-battle/releases/download/${manifest.version}/${file.name}`;
    assert.equal(file.url, expected, 'verify only this game release');
    const response = await fetch(file.url, {signal: AbortSignal.timeout(300000)});
    assert.equal(response.status, 200, file.name);
    assert.equal(Number(response.headers.get('content-length')), file.bytes);
    assert(response.headers.get('content-disposition')?.includes(file.name), 'download filename');
    let size = 0;
    const hash = createHash('sha256');
    for await (const chunk of response.body) { size += chunk.length; hash.update(chunk); }
    const digest = hash.digest('hex');
    assert.equal(size, file.bytes);
    assert.equal(digest, file.sha256);
    results.push({name: file.name, status: response.status, bytes: size, sha256: digest, verified: true});
    console.log(`PASS ${file.name}: ${size} bytes, SHA-256 matches`);
  }
  const sums = await fetch(manifest.checksums);
  assert.equal(sums.status, 200);
  const text = await sums.text();
  for (const file of manifest.files) assert(text.includes(`${file.sha256}  ${file.name}`));
  const readme = await fetch(manifest.readme);
  assert.equal(readme.status, 200);
  assert((await readme.text()).includes('すべて展開'));
  const result = {date:'2026-10-23', version:manifest.version, publicAnonymousGet:true, files:results, checksumsStatus:sums.status, readmeStatus:readme.status};
  fs.writeFileSync(output, JSON.stringify(result,null,2)+'\n');
})().catch(error => { console.error(error.message); process.exitCode=1; });
