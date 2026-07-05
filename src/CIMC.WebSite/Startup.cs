using CIMC.Data;
using CIMC.EntityFrameworkCore;
using CIMC.Helper;
using CIMC.EntityFramework;
using MySite.Web.Config;
using MySite.Web.Filters;
using MySite.Web.Models.MapperConfig;
using MySite.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MySite.Web
{
    public class Startup
    {

        private IConfiguration Configuration { get; }
        private readonly string SqlConnection = "Default";
        private readonly string AllowSpecificMethods = "AllowSpecificMethods";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 数据访问
            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString(SqlConnection), MySqlServerVersion.LatestSupportedServerVersion)
            );
            services.AddScoped(typeof(IRepository<>), typeof(AppRepository<>));

            #endregion

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "DataProtection-Keys")));

            #region  序列化数据
            services.AddMvc().AddNewtonsoftJson(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //不更改元数据的key的大小写
                options.SerializerSettings.ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                //配置序列化时时间格式为yyyy-MM-dd HH:mm:ss            
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
            #endregion

            #region  缓存
            services.AddMemoryCache();

            //由于初始化的时候我们就需要用，所以使用Bind的方式读取配置
            if (Convert.ToBoolean(Configuration["Redis:IsEnabled"]))
            {
                services.AddSingleton(typeof(ICacheService), new RedisCacheHelper(new RedisCacheOptions
                {
                    Configuration = Configuration["Redis:Configuration"],
                    InstanceName = Configuration["Redis:InstanceName"]
                }));
            }
            else
            {
                services.AddSingleton<IMemoryCache>(factory =>
                {
                    var cache = new MemoryCache(new MemoryCacheOptions());
                    return cache;
                });
                services.AddSingleton<ICacheService, MemoryCacheHelper>();
            }
            #endregion  

            #region cookies jwt认证
            services.Configure<JwtConfig>(Configuration.GetSection("JwtSettings"));
            services.Configure<UploadConfig>(Configuration.GetSection("UploadConfig"));
            //由于初始化的时候我们就需要用，所以使用Bind的方式读取配置        
            var jwtSettings = new JwtConfig();
            Configuration.Bind("JwtSettings", jwtSettings);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    //认证失败，会自动跳转到这个地址
                    options.LoginPath = "/Admin/ReLogin";
                    options.LogoutPath = "/Admin/ReLogin";
                })
                .AddJwtBearer(options =>
                {
                    //主要是jwt  token参数设置
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //Token颁发机构
                        ValidIssuer = jwtSettings.Issuer,
                        //颁发给谁
                        ValidAudience = jwtSettings.Audience,
                        //这里的key要进行加密，需要引用Microsoft.IdentityModel.Tokens
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        //是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                        ValidateLifetime = true,
                        //允许的服务器时间偏移量
                        ClockSkew = TimeSpan.Zero,

                    };
                });

            services.Configure<CookiePolicyOptions>(options =>
            {
                //This lambda determines whether user consent for non-essential cookies is needed for a given request.
                //options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            #endregion

            #region 注册Swagger服务

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Version = "v2.0", Title = "King" });
                //获取应用程序所在目录(绝对,不受工作目录影响，建议采用此方法获取路径)
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer {Token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                ////项目属性生成配置的xml文件名
                //var xmlPath = Path.Combine(basePath, "MySite.Web.xml");
                //options.IncludeXmlComments(xmlPath, true); //启用控制器注释
            });

            #endregion

            #region 注入组件
            services.AddAutoMapper(typeof(AutoMapperConfig));
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #endregion

            #region 注入权限
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            services.Configure<AuditLogConfig>(Configuration.GetSection("AuditLog"));
            var auditLogConfig = new AuditLogConfig();
            Configuration.Bind("AuditLog", auditLogConfig);

            if (auditLogConfig.IsEnabled)
            {
                services.AddScoped<IAuditLogService, AuditLogService>();
                services.AddSingleton<IAuditLogQueue, AuditLogQueue>();
                services.AddHostedService<AuditLogBackgroundService>();
            }
            else
            {
                services.AddScoped<IAuditLogService, NoopAuditLogService>();
            }
            #endregion

            #region 跨域
            services.AddCors(options =>
            {
                options.AddPolicy(AllowSpecificMethods, builder =>
                    {
                        builder.WithOrigins(
                            Configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
            });
            #endregion

            #region 注入服务

            //services.AddHostedService<JobService>();

            #endregion

          

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddControllersWithViews()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddRazorRuntimeCompilation();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "zh-CN", "en-US" };
                options.SetDefaultCulture("zh-CN")
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);
                options.FallBackToParentUICultures = true;

                // 只使用 Cookie 确定语言，移除 QueryString 和 Accept-Language 提供程序
                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
            });

            services.AddMvc(options =>
            {
                //options.Filters.Add<ActionFilter>();
                options.Filters.Add<ExceptionFilter>();
                if (auditLogConfig.IsEnabled)
                {
                    options.Filters.Add<AuditLogFilter>();
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Admin/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
  
            app.UseStaticFiles();
            app.UseRouting();
            app.UseRequestLocalization();
            app.UseCors(AllowSpecificMethods);
            app.UseAuthentication();//一定要在这个位置（app.UseAuthorization()上面）jwt
            app.UseAuthorization();
            //app.UseHttpsRedirection();
            // 设置配置  
            AppSettingsReader.SetConfiguration(Configuration);          

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "home",
                    pattern: "",
                    defaults: new
                    {
                        controller = "Home",
                        action = "Index"
                    });

                endpoints.MapControllerRoute(
                    name: "About",
                    pattern: "about",
                    defaults: new
                    {
                        controller = "Home",
                        action = "About"
                    });                  

                endpoints.MapControllerRoute(
                    name: "ProductDetail",
                    pattern: "products/detail-{id}.html",
                    defaults: new
                    {
                        controller = "Home",
                        action = "ProductDetail"
                    });

                endpoints.MapControllerRoute(
                    name: "Products",
                    pattern: "products/{category?}",
                    defaults: new
                    {
                        controller = "Home",
                        action = "Products"
                    });
               

                endpoints.MapControllerRoute(
                    name: "ArticlePreview",
                    pattern: "news/preview-{id}.html", new
                    {
                        controller = "Home",
                        action = "ArticlePreview"
                    });

                endpoints.MapControllerRoute(
                    name: "Article",
                    pattern: "news/info-{id}.html", new
                    {
                        controller = "Home",
                        action = "Article"
                    });

                endpoints.MapControllerRoute(
                    name: "News",
                    pattern: "news/{category?}",
                    defaults: new
                    {
                        controller = "Home",
                        action = "News"
                    });

                endpoints.MapControllerRoute(
                    name: "Jobs",
                    pattern: "jobs",
                    defaults: new
                    {
                        controller = "Home",
                        action = "Jobs"
                    });                

                endpoints.MapControllerRoute(
                    name: "Contact",
                    pattern: "contact",
                    defaults: new
                    {
                        controller = "Home",
                        action = "Contact"
                    });
              

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Admin}/{action=Index}/{id?}");
            });

            #region 初始化数据
            var serviceScopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
            var serviceScope = serviceScopeFactory.CreateScope();

            using (var dbContext = serviceScope.ServiceProvider.GetService<AppDbContext>())
            {
                dbContext.Database.Migrate();
                new DataInitializer().Create(dbContext);//注册默认超级管理员和前台内容
            }
            #endregion
        }


    }
}
