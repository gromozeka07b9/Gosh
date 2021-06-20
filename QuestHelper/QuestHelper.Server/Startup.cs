using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QuestHelper.Server.Auth;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Microsoft.OpenApi.Models;

namespace QuestHelper.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IConfiguration>(Configuration);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };
                });
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .SetIsOriginAllowed(hostname => true);
                    });
            });
            services.AddMvc();
            services.AddDbContext<ServerDbContext>();
            services.AddScoped<RequestFilter>();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gosh Api", Version = "v1" });
            });
            /*services.AddSwaggerGen(
                sw =>
                {
                    sw.SwaggerDoc("v1",
                        new Info()
 {
                            Title = "GoSh! API", Version = "v1", Description = "Api for GoSh! applications",
                            Contact = new Contact() {Name = "Sergey Dyachenko", Email = "sdyachenko1977@gmail.com"}
                        });
                    var filePath = Path.Combine(System.AppContext.BaseDirectory, "QuestHelper.Server.xml");
                    sw.IncludeXmlComments(filePath);
                }
           );*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseCors("AllowAll");
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(sw => sw.SwaggerEndpoint("/swagger/v1/swagger.json", "Gosh API v1"));
        }
    }
}
