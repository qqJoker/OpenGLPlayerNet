# **.Net Core6.0 + AvaloniaUI + Silk.Net.OpenGL + FFmpeg 实现跨平台视频播放器**
注：技术太菜，很多底层原理都是一知半解，项目能跑完全是东拼西凑，CV大法过来的，如果有什么错误请多多指教；参考地址：

https://github.com/kekekeks/Avalonia-Silk.NET-Example

https://github.com/opentk/LearnOpenTK/tree/master/Chapter1

https://blog.csdn.net/GrayOnDream/article/details/122158294

一、项目结构：

​	1、FFmpegPlayerSharp 

​		FFmpeg播放和解码的核心库，包含了视频流创建、解码和多路复用的操作，目前只处理了RTSP视频流，音频流未处理；

​	2、OpenGLPlayer

​		OpenGL渲染的组件库，从Avalonia.OpenGL.Controls.OpenGlControlBase继承实现；

​	3、RtspPlayer

​		Windows窗口


二、代码示例

​	1、FFmpeg初始化：

​		WPF/Avalonia项目下，在App.xaml.cs文件中添加：
```c#
FFmpegPlayerSharp.Entrance.InitFFmpegLibs(enableHwDecoder:false,enableLog:true);
```

​		enableHwDecoder ： 是否启用硬解（目前YUV播放使用的软解，硬解的方案还没折腾出来）

​		enableLog : 是否启用日志打印（包括FFmpeg自带的日志）

​	2、开始播放FFmpeg：

```c#
var playerBase = FFmpegPlayerSharp.PlayerCollection.Instance.GetPlayer("rtsp://balabalabala/");
```

​	3、获取解码视频帧：

​		在playerBase内有FrameQueues队列，存放的就是解码后的数据；


------

记录下，OpenGL和之前做过的Direct3D不太一样，在Windows环境下使用Direct3D无疑是最好的，性能比OpenGL更强，而且对FFmpeg的硬解支持度非常友好（FFmpeg开启dxva2硬解后，解码出来的数据里面本身就包含了Surface和Device信息，可以很轻松的使用D3DImage播放），渲染方式也是解码一帧就实时渲染一帧；但是OpenGL略有不同，虽然也是用的纹理渲染，但是对萌新不是很友好，要渲染硬解后的数据还得折腾半天；Avalonia.OpenGL.Controls.OpenGlControlBase类的OnOpenGlRender很符合GLFW的渲染模式（好像就是用的GLFW），类似于一个定时刷新器，他会一直刷新，你可以在这个方法下主动获取解码数据帧做渲染；
