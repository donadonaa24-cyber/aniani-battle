// Loopback only. No files outside the build directory are served.
const http=require('http'),fs=require('fs'),path=require('path');
const root=fs.realpathSync(path.resolve(__dirname,'../../Build/WebGL'));
const port=Number(process.argv[2]||8799);
const server=http.createServer((req,res)=>{
 if(req.method!=='GET'&&req.method!=='HEAD'){res.writeHead(405).end();return;}
 let name;
 try{name=decodeURIComponent(new URL(req.url,'http://localhost').pathname);}
 catch{res.writeHead(400).end();return;}
 let file=path.resolve(root,'.'+name+(name.endsWith('/')?'index.html':''));
 if(!file.startsWith(root+path.sep)){res.writeHead(403).end();return;}
 try{file=fs.realpathSync(file);if(!file.startsWith(root+path.sep)||!fs.statSync(file).isFile())throw 0;}
 catch{res.writeHead(404).end('Not found');return;}
 const headers={'Cache-Control':name.endsWith('.html')||name==='/'?'no-store':'no-cache','X-Content-Type-Options':'nosniff'};
 let logical=file;
 if(logical.endsWith('.gz')){headers['Content-Encoding']='gzip';logical=logical.slice(0,-3);}
 if(logical.endsWith('.br')){headers['Content-Encoding']='br';logical=logical.slice(0,-3);}
 // .unityweb uses Unity's decompression fallback and must NOT have Content-Encoding.
 logical=logical.replace(/\.unityweb$/,'');
 headers['Content-Type']=logical.endsWith('.wasm')?'application/wasm':logical.endsWith('.js')?'application/javascript':logical.endsWith('.html')?'text/html; charset=utf-8':logical.endsWith('.css')?'text/css':logical.endsWith('.png')?'image/png':logical.endsWith('.json')?'application/json':'application/octet-stream';
 headers['Content-Length']=fs.statSync(file).size;
 res.writeHead(200,headers);if(req.method==='HEAD')res.end();else fs.createReadStream(file).pipe(res);
});
server.listen(port,'127.0.0.1',()=>console.log('あにあにバトル！！ http://127.0.0.1:'+port+'/'));
server.on('error',e=>{console.error(e.message);process.exitCode=1;});
