# 本地文件服务工具

Windows 桌面工具：拖入文件或文件夹，自动通过本机 HTTP 地址提供访问。

## 使用

运行 `LocalFileServer-v2.exe`，拖入文件或文件夹。默认端口 8080，可修改后点击“启动 / 重启”。支持浏览器打开、复制地址和停止服务。

- 仅监听 127.0.0.1，仅本机可访问。
- 文件夹包含 index.html 时显示网页，否则显示目录列表。
- 拖入单个文件时只提供该文件。
- 关闭窗口停止服务。端口被占用时会显示错误。
- 运行需要 .NET Framework 4.5 或更高版本。

## 构建

在 Windows PowerShell 运行 `./build.ps1`。

`LocalFileServer.cs` 是程序源码，`LocalFileServer.ico` 是应用图标。可选的 `make_icon.py` 使用 Python 和 Pillow 重新生成图标。

这是简易静态文件服务，不执行服务端脚本；不支持 HTTPS、视频范围请求或生产环境部署。当前版本已编译，完整的运行测试尚未完成。
