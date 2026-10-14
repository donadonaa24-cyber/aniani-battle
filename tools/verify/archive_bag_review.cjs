// Move only obsolete managed review copies into the explicitly protected bag folder.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8').replace(/^\uFEFF/,''));
const root=process.cwd(),base=path.resolve('イラスト確認/千鶴');
const prior=read(path.join(base,'.managed.json')),cat=read('art_source/paper_cutout/review-catalog.json');
const keep=new Set(cat.entries.map(e=>e.source));
const protectedRows=read('art_source/paper_cutout/variants/bag-backup.json').entries;
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
function within(parent,p){const absolute=path.resolve(p),rel=path.relative(parent,absolute);if(!rel||rel.startsWith('..')||path.isAbsolute(rel))throw Error('Unsafe move path');return absolute;}
within(root,base);let moved=0;
for(const row of prior){
 if(keep.has(row.source))continue;
 const protectedRow=protectedRows.find(r=>r.source===row.source);
 if(!protectedRow||protectedRow.sha256!==row.sha256)throw Error('Not a protected bag review copy');
 const from=within(base,path.join(base,row.file)),to=within(base,path.join(base,'予備_カバン版_削除禁止/以前の採用表示',path.basename(row.file)));
 if(!fs.existsSync(from))continue;
 if(sha(from)!==row.sha256)throw Error('Review copy modified');
 if(fs.existsSync(to))throw Error('Archive destination exists; keep both for review');
 fs.mkdirSync(path.dirname(to),{recursive:true});fs.renameSync(from,to);moved++;
}
process.stdout.write(JSON.stringify({moved,originalsDeleted:0})+'\n');
