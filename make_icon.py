from PIL import Image,ImageDraw
im=Image.new('RGBA',(1024,1024),(0,0,0,0));d=ImageDraw.Draw(im)
d.rounded_rectangle((32,32,992,992),radius=200,fill='#0F766E')
d.rounded_rectangle((185,210,490,390),radius=45,fill='#FCD34D')
d.rounded_rectangle((185,295,839,730),radius=60,fill='#FBBF24')
d.rounded_rectangle((185,360,839,730),radius=60,fill='#FDE68A')
d.line([(512,560),(512,800)],fill='#134E4A',width=38)
d.line([(330,800),(694,800)],fill='#134E4A',width=38)
for x,y in [(512,548),(330,800),(694,800)]:
 d.rounded_rectangle((x-65,y-52,x+65,y+52),radius=20,fill='#FFFFFF',outline='#134E4A',width=16)
im=im.resize((256,256),Image.Resampling.LANCZOS)
im.save('local-file-server/LocalFileServer.ico',sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
im.save('local-file-server/icon-preview.png')
