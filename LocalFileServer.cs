using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
class ServerForm : Form {
 TextBox path=new TextBox(), url=new TextBox(); NumericUpDown port=new NumericUpDown(); Label status=new Label(); Button stop=new Button(); TcpListener server; string root, single; string address;
 public ServerForm(){
 Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath); Text="本地文件服务"; ClientSize=new Size(660,300); Font=new Font("Microsoft YaHei UI",10); StartPosition=FormStartPosition.CenterScreen; AllowDrop=true;
 FormBorderStyle=FormBorderStyle.FixedSingle; MaximizeBox=false;
 var hint=new Label{Text="拖入文件或文件夹，自动启动本地服务",AutoSize=true,Location=new Point(24,22)};
 path.SetBounds(24,60,610,28); path.ReadOnly=true;
 var label=new Label{Text="端口",Location=new Point(24,112),AutoSize=true}; port.SetBounds(75,108,100,30); port.Minimum=1024;port.Maximum=65535;port.Value=8080;
 var start=new Button{Text="启动 / 重启",Location=new Point(190,105),Size=new Size(125,36)}; stop.Text="停止";stop.SetBounds(330,105,85,36);
 url.SetBounds(24,163,610,28);url.ReadOnly=true;
 var open=new Button{Text="浏览器打开",Location=new Point(24,210),Size=new Size(125,36)};var copy=new Button{Text="复制地址",Location=new Point(165,210),Size=new Size(110,36)};
 status.SetBounds(24,261,610,30);status.Text="仅本机可访问；关闭窗口即停止服务。";
 Controls.AddRange(new Control[]{hint,path,label,port,start,stop,url,open,copy,status});
 DragEnter+=(s,e)=>{if(e.Data.GetDataPresent(DataFormats.FileDrop))e.Effect=DragDropEffects.Copy;};
 DragDrop+=(s,e)=>{var files=(string[])e.Data.GetData(DataFormats.FileDrop);if(files.Length!=1){status.Text="请一次拖入一个文件或文件夹。";return;}path.Text=files[0];Start();};
 start.Click+=(s,e)=>Start();stop.Click+=(s,e)=>Stop();
 open.Click+=(s,e)=>{if(address!=null)try{Process.Start(address);}catch(Exception ex){status.Text=ex.Message;}};
 copy.Click+=(s,e)=>{if(address!=null)try{Clipboard.SetText(address);status.Text="地址已复制。";}catch(Exception ex){status.Text=ex.Message;}};
 FormClosing+=(s,e)=>Stop();
 }
 void Stop(){if(server!=null){server.Stop();server=null;}address=null;url.Clear();status.Text="服务已停止。";}
 void Start(){Stop();try{
 string selected=Path.GetFullPath(path.Text);if(File.Exists(selected)){single=selected;root=Path.GetDirectoryName(selected);}else if(Directory.Exists(selected)){single=null;root=selected;}else throw new Exception("请拖入有效的文件或文件夹。");
 server=new TcpListener(IPAddress.Loopback,(int)port.Value);server.Start();address="http://127.0.0.1:"+port.Value+"/";url.Text=address;status.Text="服务运行中，仅本机可访问。";Accept(server,root,single);
 }catch(Exception ex){Stop();status.Text="启动失败："+ex.Message;}}
 async void Accept(TcpListener listener,string folder,string file){try{while(true){var client=await listener.AcceptTcpClientAsync();Task.Run(()=>Serve(client,folder,file));}}catch(ObjectDisposedException){}catch(SocketException){} }
 static string Escape(string x){return WebUtility.HtmlEncode(x);}
 static string Mime(string p){switch(Path.GetExtension(p).ToLowerInvariant()){case ".html":case ".htm":return "text/html; charset=utf-8";case ".css":return "text/css; charset=utf-8";case ".js":return "application/javascript";case ".json":return "application/json";case ".txt":case ".md":return "text/plain; charset=utf-8";case ".png":return "image/png";case ".jpg":case ".jpeg":return "image/jpeg";case ".svg":return "image/svg+xml";case ".pdf":return "application/pdf";case ".mp4":return "video/mp4";default:return "application/octet-stream";}}
 static void Header(Stream s,int code,string type,long length){var b=Encoding.ASCII.GetBytes("HTTP/1.1 "+code+" "+(code==200?"OK":"Error")+"\r\nContent-Type: "+type+"\r\nContent-Length: "+length+"\r\nConnection: close\r\nX-Content-Type-Options: nosniff\r\n\r\n");s.Write(b,0,b.Length);}
 static void TextResponse(Stream s,int code,string text,bool head){var b=Encoding.UTF8.GetBytes(text);Header(s,code,"text/html; charset=utf-8",b.Length);if(!head)s.Write(b,0,b.Length);}
 static void Serve(TcpClient client,string folder,string file){using(client){client.ReceiveTimeout=5000;client.SendTimeout=10000;try{using(var s=client.GetStream()){
 var bytes=new MemoryStream();int state=0;while(bytes.Length<16384){int v=s.ReadByte();if(v<0)return;bytes.WriteByte((byte)v);state=(state==0&&v==13)?1:(state==1&&v==10)?2:(state==2&&v==13)?3:(state==3&&v==10)?4:0;if(state==4)break;}if(state!=4)return;
 var parts=Encoding.ASCII.GetString(bytes.ToArray()).Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries)[0].Split(' ');bool head=parts[0]=="HEAD";if(parts.Length<2||(!head&&parts[0]!="GET")){TextResponse(s,405,"Method not allowed",false);return;}
 string raw=parts[1].Split('?')[0];string decoded=Uri.UnescapeDataString(raw);if(!decoded.StartsWith("/")||decoded.Contains("\\")||decoded.Contains(":")){TextResponse(s,403,"Forbidden",head);return;}
 string target=Path.GetFullPath(Path.Combine(folder,decoded.TrimStart('/').Replace('/',Path.DirectorySeparatorChar)));string prefix=folder.TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
 if(!target.Equals(folder,StringComparison.OrdinalIgnoreCase)&&!target.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)){TextResponse(s,403,"Forbidden",head);return;}
 string check=folder;foreach(string segment in decoded.Trim('/').Split('/')){if(segment.Length==0)continue;check=Path.Combine(check,segment);if((File.Exists(check)||Directory.Exists(check))&&(File.GetAttributes(check)&FileAttributes.ReparsePoint)!=0){TextResponse(s,403,"Links are not served",head);return;}}
 if(file!=null){if(decoded!="/"&&!target.Equals(file,StringComparison.OrdinalIgnoreCase)){TextResponse(s,404,"Not found",head);return;}target=file;}
 if(Directory.Exists(target)){
 string index=Path.Combine(target,"index.html");if(File.Exists(index)&&(File.GetAttributes(index)&FileAttributes.ReparsePoint)==0)target=index;
 else{var html=new StringBuilder("<!doctype html><meta charset=utf-8><title>本地文件服务</title><style>body{font:16px sans-serif;max-width:900px;margin:40px auto}li{padding:8px}</style><h1>文件目录</h1><ul>");
 foreach(string entry in Directory.GetFileSystemEntries(target)){if((File.GetAttributes(entry)&FileAttributes.ReparsePoint)!=0)continue;string name=Path.GetFileName(entry);string relative=entry.Substring(prefix.Length).Replace('\\','/');string link="/"+string.Join("/",Array.ConvertAll(relative.Split('/'),Uri.EscapeDataString));html.Append("<li><a href=\"").Append(Escape(link)).Append("\">").Append(Escape(name)).Append(Directory.Exists(entry)?"/":"").Append("</a></li>");}html.Append("</ul>");TextResponse(s,200,html.ToString(),head);return;}}
 if(!File.Exists(target)){TextResponse(s,404,"Not found",head);return;}using(var input=new FileStream(target,FileMode.Open,FileAccess.Read,FileShare.Read)){Header(s,200,Mime(target),input.Length);if(!head)input.CopyTo(s);}
 }}catch(Exception){}}}
 [STAThread]static void Main(){Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Application.Run(new ServerForm());}
}

