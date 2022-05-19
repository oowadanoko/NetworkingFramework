# 通用网络框架

## 框架介绍

&emsp;&emsp;本项目是在完成了[前一个项目](https://github.com/oowadanoko/MultiplayerTankBattle)之后，经过总结，从头开始打造的一个通用网络框架。以简单易用、能够在各种不同类型的项目中都能通用、减少用户的直接操作从而降低错误发生概率为理念，设计的一款轻量级的通用网络框架。只包含了核心的框架部分，不包含任何具体的协议消息实现，从而减轻框架大小，并提高通用性。但是部分机制因为过于常用（例如：心跳机制、位置同步等），所以作为示例放在 `NetworkingFrameworkExample` 中供参考与学习框架的使用。

> 框架特点：
> - 完全以异步方式通信
> - 解决了粘包半包问题
> - 实现了发送队列机制
> - 实现了消息队列机制
> - 缓冲区自动扩容缩容
> - 完全隐藏了底层细节
> - 用户无需输入字符串
> - 只需要关心业务逻辑

> 本项目对比前一个项目做了很大变动：
> - 客户端与服务器均采用异步方式实现
> - 用户无需输入字符串
> - 对临界区资源进行控制，防止异步访问造成的冲突，但也会造成一定的性能损失
> - 提供了更灵活、更丰富的公共接口
> - 提供用户存储自定义数据的功能
> - 更彻底的封装，对访问权限的控制更严格，不再向用户暴露底层接口
> - 减少了用户手工操作的工作量，降低了因用户手工操作失误造成错误的概率

&emsp;&emsp;本框架完全使用 `.NET 5.0` 原生类库实现，不需要引入任何外部程序集。但是框架中JSON的序列化与反序列化使用了 `System.Text.Json.JsonSerializer` 类实现，该程序集从 `.NET Core 3.0` 引入，早于该版本的 `.Net` 平台需要自行引入 `System.Text.Json.dll` 程序集。需要注意该类的使用方法。 **该类默认只序列化公共属性而不序列化字段** ，如要序列化字段需要在字段前加上特性 `[JsonInclude]`，该特性位于 `System.Text.Json.Serialization` 命名空间中。更具体的使用方法请参考[CSDN官方文档](https://docs.microsoft.com/zh-cn/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-5-0)。

&emsp;&emsp; ~~本框架仍然存在大量 BUG 尚未发现，使用时请注意~~

## 框架结构

&emsp;&emsp;框架中包含3个文件，对应3个命名空间。
- `Oowada.NetworkingFramework.Common` 中的文件作为框架的通用部分，需要同时部署在客户端与服务器。
- `Oowada.NetworkingFramework.Client` 中的文件作为客户端的部分，客户端的大部分任务只需要操作该命名空间下的静态类 `ClientNetManager` 即可完成任务。
- `Oowada.NetworkingFramework.Server` 中的文件作为服务器的部分，服务器的大部分任务只需要操作该命名空间下的静态类 `ServerNetManager` 即可完成任务。
> &emsp;&emsp;除了框架文件，还包含了3个示例（合计7个项目文件），这些示例演示了框架的使用方法，可以作为使用框架的参考资料。
> 1. 心跳机制
>    - 客户端间隔一定时间给服务器发送Ping消息
>    - 服务器收到客户端发送的Ping消息后，更新上次收到Ping消息的时间，并给客户端回复一个Pong消息
>    - 客户端收到服务器回复的Pong消息后，更新上次收到Pong消息的时间
>    - 如果客户端间隔一段时间后还是没有收到服务器回复的Pong消息，就认为已经与服务器失去联系，主动断开连接，释放资源
>    - 如果服务器间隔一段时间后还是没有收到客户端发送的Ping消息，就认为已经与客户端失去联系，主动断开连接，释放资源
> 2. 大量数据的频繁发送
>    - 客户端频繁向服务器发送一长串随机字符串
>    - 服务器收到消息后广播给所有客户端
> 3. 位置同步
>    - 两个客户端都间隔一段时间向服务器发送自己的位置信息
>    - 通过 wasd 键模拟移动
>    - 服务器收到消息后广播给其他客户端
>    - 客户端收到服务器的广播消息后，同步另一个客户端的位置信息

## 使用方法

1. 引入项目源代码或程序集
2. 设计需要的协议消息类
    - 该类需要继承自 `Oowada.NetworkingFramework.Common.BaseMessage`
    - 协议消息类需要同时部署在客户端与服务器
3. 在客户端与服务器代码中注册自定义的协议消息
    - 使用 `RegisterMessage` 方法进行注册
4. 添加消息监听与事件监听
    - 使用 `AddMessageListener` 方法添加消息监听，提供处理消息的方法
    - 使用 `AddNetEventListener` 方法添加网络事件监听，提供处理网络事件的方法
    - 使用 `AddNetMessageEventListener` 方法添加网络消息事件监听，提供处理网络消息事件的方法
5. 添加要在 `BeforeUpdate`、`Update`、`AfterUpdate` 中执行的回调函数
    - 对应调用  `AddBeforeUpdateCallback`、`AddUpdateCallback`、`AddAfterUpdateCallback` 方法添加
6. 开启服务器，客户端连接服务器
    - 使用 `ServerNetManager.StartServer` 开启服务器
    - 使用 `ClientNetManager.Connect` 连接服务器
7. 驱动 `Update` 执行
    - `Update` 中会进行消息的处理，为了框架通用性，要求用户根据实际情况提供 `Update` 的驱动
    - 将 `Update` 方法置于可以循环执行的代码段中，驱动 `Update` 方法执行
        - Unity 3D 中可以置于Update系列函数或协程函数中执行（Unity 3D 对多线程的支持似乎不太友好）
        - 其他应用环境可以使用计时器、多线程等方式驱动
> 具体使用可以参考示例文件