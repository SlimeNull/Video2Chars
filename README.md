# Video2Chars
可用于将黑白视频转换为字符画序列的程序

## 程序用途:
将受支持的图像或者视频转换为字符画或字符画序列

## 使用方式:
```sh
Video2Chars [/Preview] [-BaseChars 基本字符] 源文件 输出文件
```
其中: 当指定 /Preview 时, 每渲染一帧, 都会在控制台中显示它. 输出的字符画将由基本字符构成, 它的默认值是:
```py
"!@#$%^&*()_+=-0987654321qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM,. "
```
当源文件是一个视频时, 输出文件是可以被 [NullPlayer](https://github.com/SlimeNull/NullPlayer) 所播放的.

## 下载链接:
Windows可执行文件以及所需链接库的压缩包 - 更新于2021/1/12: https://chonet.lanzous.com/i5pjDkapbde

## 注意事项:
当图像, 视频为纯黑白色时, 程序能够发挥最好效果
如果无法正常渲染, 例如, 输出文件内容中不包含任何一帧内容, 可能是因为程序无法解析视频文件, 你可以尝试使用格式工厂将该文件重新编码
