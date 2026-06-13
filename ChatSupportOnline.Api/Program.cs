using ChatSupportOnline.Api.Configurations;
using ChatSupportOnline.Api.Data;
using ChatSupportOnline.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 注册控制器，让 ASP.NET Core 可以发现并执行我们的 Web API。
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 实体之间有双向导航属性时，默认序列化容易出现循环引用异常。
        // 这里先忽略循环引用，方便学习阶段直接返回实体结构。
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// 绑定 JWT 配置，后续生成和校验 Token 时都使用这一组配置。
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
{
    throw new InvalidOperationException("JWT 密钥未配置，请检查 appsettings.json 中的 Jwt 配置。");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

// 注册 EF Core 的数据库上下文。
// 这里使用 SQL Server，后面我们会在这个上下文上继续接入迁移、认证和消息持久化。
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 注册认证相关服务。
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 注册 JWT Bearer 认证。
// 这一步的作用是告诉 ASP.NET Core 如何从请求头里解析并验证 Bearer Token。
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 注册授权服务，后续我们会基于角色继续细分权限。
builder.Services.AddAuthorization();

var app = builder.Build();

// 仅在开发环境暴露 OpenAPI 文档，避免生产环境默认公开接口元数据。
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 认证中间件必须放在授权之前。
app.UseAuthentication();
app.UseAuthorization();

// 将控制器路由映射到请求管道中。
app.MapControllers();

app.Run();

