// Read-only comparison of sneaker paint. Ignore isolated fringe pixels and thin paper borders.
const fs=require('node:fs'), path=require('node:path'), sharp=require('sharp');
async function feet(file){
 const {data,info}=await sharp(file).ensureAlpha().raw().toBuffer({resolveWithObject:true});
 const w=info.width,h=info.height,mask=new Uint8Array(w*h),seen=new Uint8Array(w*h);
 for(let y=Math.floor(h*.55);y<h;y++)for(let x=0;x<w;x++){const i=(y*w+x)*4,r=data[i],g=data[i+1],b=data[i+2];if(data[i+3]>=128&&Math.min(r,g,b)>150&&Math.max(r,g,b)-Math.min(r,g,b)<35&&b>=r-8&&g>=r-8)mask[y*w+x]=1;}
 const points=[];
 for(let start=0;start<mask.length;start++)if(mask[start]&&!seen[start]){
  const queue=[start];seen[start]=1;let at=0,minX=w,minY=h,maxX=-1,maxY=-1;
  while(at<queue.length){const p=queue[at++],x=p%w,y=Math.floor(p/w);minX=Math.min(minX,x);maxX=Math.max(maxX,x);minY=Math.min(minY,y);maxY=Math.max(maxY,y);
   for(const n of [x>0?p-1:-1,x+1<w?p+1:-1,y>0?p-w:-1,y+1<h?p+w:-1])if(n>=0&&mask[n]&&!seen[n]){seen[n]=1;queue.push(n);}}
  if(queue.length>=100&&queue.length/((maxX-minX+1)*(maxY-minY+1))>=.2)for(const p of queue)points.push([p%w,Math.floor(p/w)]);
 }
 if(points.length<1000)throw Error('Sneaker paint missing: '+file);
 const sorted=points.slice().sort((a,b)=>a[0]-b[0]), centers=[sorted[Math.floor(sorted.length*.1)].slice(),sorted[Math.floor(sorted.length*.9)].slice()];
 let groups;
 for(let step=0;step<20;step++){groups=[[],[]];for(const p of points){const d=centers.map(c=>(p[0]-c[0])**2+(p[1]-c[1])**2);groups[d[0]<d[1]?0:1].push(p);}for(let i=0;i<2;i++)if(groups[i].length)centers[i]=[0,1].map(axis=>groups[i].reduce((s,p)=>s+p[axis],0)/groups[i].length);}
 const lengths=groups.map(group=>{const [cx,cy]=[0,1].map(axis=>group.reduce((s,p)=>s+p[axis],0)/group.length);let xx=0,xy=0,yy=0;for(const[x,y]of group){xx+=(x-cx)**2;xy+=(x-cx)*(y-cy);yy+=(y-cy)**2;}const angle=.5*Math.atan2(2*xy,xx-yy);const projection=group.map(([x,y])=>x*Math.cos(angle)+y*Math.sin(angle)).sort((a,b)=>a-b);return {pixels:group.length,center:[+cx.toFixed(1),+cy.toFixed(1)],lengthPx:+(projection.at(-1)-projection[0]).toFixed(2)};}).sort((a,b)=>a.lengthPx-b.lengthPx);
 return {file:path.basename(file),width:w,height:h,feet:lengths};
}
async function main(){const folder=path.resolve('art_source/paper_cutout/generated'), results=[];for(const file of fs.readdirSync(folder).filter(f=>f.endsWith('.png')&&!f.includes('_effect_')).sort())results.push(await feet(path.join(folder,file)));
 const baseline=results.find(r=>r.file==='chizuru_paper_idle_v1.png');
 for(const r of results)r.ratios=r.feet.map((foot,i)=>+(foot.lengthPx/baseline.feet[i].lengthPx).toFixed(3));
 fs.writeFileSync('art_source/paper_cutout/feet-all-candidates.json',JSON.stringify({method:'RGBA sneaker light paint, components >=100px and fill >=0.2, 2 spatial clusters, PCA major-axis. Manual review required for overlaps/occluded feet.',baseline,results},null,2)+'\n');
 for(const r of results)process.stdout.write(r.file+': '+r.feet.map(f=>f.lengthPx.toFixed(1)).join('/')+'px; '+r.ratios.join('/')+'\n');
}
main().catch(error=>{process.stderr.write(error.stack+'\n');process.exitCode=1;});
