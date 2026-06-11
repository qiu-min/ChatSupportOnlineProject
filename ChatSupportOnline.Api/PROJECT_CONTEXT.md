# ChatSupportOnline Project Context

## 项目目标

这是一个在线客服聊天项目，目标不仅是完成功能，还要作为学习型项目掌握以下后端能力：

- ASP.NET Core Web API
- JWT 身份认证与授权
- EF Core 连接数据库与持久化
- Redis 缓存与在线状态管理
- SignalR 实时通信

项目最终希望达到两个结果：

- 能系统学习一套真实后端项目的核心技术
- 能整理成一段适合写进求职简历的项目经历

## 当前项目状态

当前仓库以后端为主，前端尚未创建。

后端目前已经具备：

- `Program.cs` 中的基础启动配置
- `AppDbContext` 数据库上下文
- 用户、会话、消息三个核心实体
- 基础控制器
- JWT 登录认证基础链路

当前后端已经可以支持以下基础能力：

- 注册
- 登录
- 返回 JWT
- 受保护接口访问
- 用户、会话、消息的基础数据操作

## 当前架构理解

当前代码更偏学习阶段的“最短路径”写法：

- Controller 直接注入 `AppDbContext`
- Controller 中直接编写一部分业务逻辑
- Controller 直接返回或操作实体

这种方式的优点：

- 简单直接
- 容易理解 Web API 到数据库的完整链路
- 适合初学阶段快速建立整体认知

这种方式的局限：

- 控制器容易变胖
- HTTP 逻辑与业务逻辑耦合较高
- 不利于后续扩展测试、缓存、SignalR、复杂权限

更接近真实生产项目的演进方向通常是：

- `Controller -> Service -> DbContext`

推荐逐步演进为：

- `IAuthService`
- `IConversationService`
- `IMessageService`

说明：

- 业务服务通常建议抽接口
- `DbContext` 不一定必须额外包一层接口
- 小中型项目中，Service 直接依赖 `AppDbContext` 是常见做法

## EF Core 核心理解

### AppDbContext 是什么

`AppDbContext` 是 EF Core 中管理实体与数据库交互的核心类。

它主要负责：

- 声明有哪些实体需要映射为数据库表
- 配置主键、外键、字段约束、实体关系
- 让代码可以通过 LINQ 查询和更新数据
- 在保存时把对象变化翻译为 SQL

在当前项目中：

- `DbSet<User>` 对应用户表
- `DbSet<Conversation>` 对应会话表
- `DbSet<Message>` 对应消息表

可以把 `DbSet<T>` 理解为“代码里操作某张表的入口”。

### DbContext 查询是不是在查真实数据库

通常是的，但要区分“构建查询”和“执行查询”。

例如：

```csharp
var query = _dbContext.Users.Where(user => user.IsActive);
```

这一步通常只是构建查询表达式，还没有真正访问数据库。

而下面这些调用通常才会真正触发数据库操作：

- `ToListAsync()`
- `FirstOrDefaultAsync()`
- `AnyAsync()`
- `CountAsync()`
- `FindAsync()`
- `SaveChangesAsync()`

可以这样理解：

- LINQ 表达式是在描述“想怎么查”
- 执行方法才会真正发 SQL 到数据库

### 项目启动后是不是要一直连着数据库

不是。

ASP.NET Core 和 EF Core 的常见工作模式是按需连接，而不是项目一启动就永久保持数据库连接。

通常流程是：

1. 项目启动时注册 `AppDbContext`
2. 请求进来后创建 `DbContext` 实例
3. 当执行查询或保存时才真正打开数据库连接
4. 请求结束后释放或归还连接

所以：

- 不是一直保持长连接
- 只有在真正访问数据库时才需要数据库可用

补充理解：

- `DbContext` 通常是每个请求一个实例
- 它更像一次请求内的数据访问工作单元

## JWT 核心理解

### JWT 的基本作用

JWT 的核心作用是：

- 登录成功后由后端签发 token
- 前端后续请求都带上 token
- 后端自动验证 token 是否合法
- 如果合法，就识别当前请求是谁

最简流程：

1. 用户输入用户名和密码登录
2. 后端验证密码
3. 后端生成 JWT 返回前端
4. 前端在后续请求中带上 `Authorization: Bearer xxx`
5. 后端验证 token 后再执行受保护接口

### PasswordService 的作用

`PasswordService` 只负责密码相关逻辑，不负责普通业务接口鉴权。

它的职责主要是：

- 注册时将明文密码转为哈希再入库
- 登录时验证明文密码和数据库哈希是否匹配

因此：

- 注册和登录接口会用到 `PasswordService`
- 普通业务接口一般不会再验证密码

可以这样记：

- `PasswordService` 负责“登录凭证”
- JWT 负责“登录后的身份证明”

### Claims 是什么

Claim 可以理解为“写在 token 里的身份字段”。

在当前项目中，登录成功后会把用户的一部分关键信息写入 token，例如：

- `UserId`
- `UserName`
- `DisplayName`
- `Role`

这些信息不是完整数据库实体，而是“当前登录身份的说明”。

JWT 可以简单理解为三段结构：

- `header`
- `payload`
- `signature`

其中：

- Claims 主要位于 `payload`
- 签名位于 `signature`

### JwtTokenService 做了什么

`JwtTokenService` 的职责是：

- 从业务用户对象中提取关键身份字段
- 组装 Claim 列表
- 按配置的密钥和算法生成 JWT
- 把 token 返回给前端

注意：

- 它不是把整个数据库用户对象放进 token
- 它只放“后续识别身份需要的信息”

### Program.cs 中 JWT 相关配置的作用

`Program.cs` 中和 JWT 相关的配置主要做两件事：

1. 读取 JWT 规则配置
2. 注册 JWT Bearer 认证机制

JWT 配置项包括：

- `Key`：签名密钥
- `Issuer`：签发者
- `Audience`：接收方
- `ExpiresMinutes`：过期时间

这些配置的作用是规定：

- token 如何生成
- token 如何验证

### 注册 JWT Bearer 认证是在做什么

`AddJwtBearer(...)` 不是负责生成 token，而是负责告诉 ASP.NET Core：

- 请求头中的 Bearer Token 要如何解析
- 用什么规则验证 token

常见验证内容包括：

- 签名是否正确
- 是否过期
- `Issuer` 是否匹配
- `Audience` 是否匹配

验证通过后，框架才会允许该请求作为已认证请求继续执行。

### Claims 为什么会出现在控制器的 User 里

这不是因为控制器手动做了转换，而是 ASP.NET Core 的 JWT Bearer 中间件自动完成的。

完整思路：

1. 登录时 `JwtTokenService` 把 claims 写进 JWT
2. 前端带着 JWT 来请求受保护接口
3. `UseAuthentication()` 触发 JWT Bearer 认证
4. JWT Bearer 验证 token
5. 验证成功后，从 token 的 payload 中解析 claims
6. 这些 claims 被组装成 `ClaimsPrincipal`
7. 最终放到控制器里的 `User`

所以：

- `CreateAuthResponse()` 负责“把 claims 写进 token”
- JWT Bearer 中间件负责“把 token 里的 claims 解析成 `User`”

### 控制器里的 User 是什么

控制器里的 `User` 不是数据库实体 `User`，而是 ASP.NET Core 提供的当前登录身份对象。

它的本质是：

- `ClaimsPrincipal`

它表示的是“当前请求是谁”，而不是“数据库里的完整用户实体”。

因此要注意区分：

- `ControllerBase.User`：当前登录身份，从 JWT 来
- `Models.Entities.User`：数据库用户实体，从数据库来

### 接口里如何使用 User

控制器中常见的用法是：

- 从 `User` 中取当前登录用户 ID
- 从 `User` 中取当前用户名
- 从 `User` 中取当前角色
- 判断当前用户是否拥有某个角色
- 再根据用户 ID 去数据库查询完整用户资料

典型用法示例：

```csharp
var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
if (!Guid.TryParse(userIdText, out var userId))
{
    return Unauthorized();
}
```

这个用法非常重要，因为它意味着：

- 后端不应该完全相信前端传来的“我是谁”
- 更安全的方式是后端自己从 token 里读取当前身份

这对于消息发送、当前用户资料、角色控制都非常重要。

## [Authorize] 的理解

`[Authorize]` 的作用是：

- 要求接口必须先通过认证

也就是说：

- 请求必须带合法 JWT
- 否则不能进入该接口

并不是所有接口都要加 `[Authorize]`。

典型情况：

- 注册、登录接口通常允许匿名访问
- 当前用户资料、消息、会话等业务接口通常需要认证

更接近生产实践的原则通常是：

- 默认多数业务接口都应受保护
- 只有少数明确公开的接口允许匿名访问

## 实体、DTO 与返回模型

当前项目为了学习方便，控制器中仍存在直接使用实体的情况，这在入门阶段是可以接受的。

但长期来看，生产项目通常会区分：

- 实体 Entity：数据库映射模型
- 请求模型 Request DTO：接口输入
- 响应模型 Response DTO：接口输出

这样做的原因包括：

- 避免直接暴露敏感字段，例如 `PasswordHash`
- 降低数据库结构变化对前端的直接影响
- 避免导航属性导致的序列化问题
- 让接口返回结构更稳定、更贴近前端需求

## Redis 的定位

Redis 不是用来替代分层设计，也不是因为控制器直接访问数据库才需要 Redis。

Redis 主要用于：

- 缓存热点数据
- 维护用户在线状态
- 保存短期状态信息
- 降低数据库访问压力

所以要区分两个问题：

- Controller 是否应该直接依赖 `DbContext`：这是分层设计问题
- 是否需要 Redis：这是缓存、状态与性能问题

在客服聊天项目里，后续 Redis 的常见用途会是：

- 维护客服/客户在线状态
- 缓存部分高频读取数据
- 辅助刷新令牌或短期会话状态管理

## 当前学习建议

建议按以下顺序继续推进项目：

1. 继续巩固 JWT 与当前登录用户识别机制
2. 使用 EF Core Migration 真正落库建表
3. 将控制器中的业务逻辑逐步抽到 Service 层
4. 接入 Redis 管理在线状态与缓存
5. 接入 SignalR 完成实时消息通信
6. 再补前端，完成完整聊天闭环

## 一句话记忆

- `AppDbContext` 是 EF Core 管理实体与数据库交互的核心入口
- `DbSet<T>` 可以理解成代码里操作某张表的入口
- `PasswordService` 只负责密码处理
- `JwtTokenService` 负责生成包含 claims 的 JWT
- JWT Bearer 中间件负责验证 token 并把 claims 还原为控制器里的 `User`
- 控制器里的 `User` 是当前登录身份，不是数据库实体用户
