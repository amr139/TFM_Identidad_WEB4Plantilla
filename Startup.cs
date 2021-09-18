using System;
using System.IO;
using System.Text;
using FluentValidation.AspNetCore;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Jdenticon.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using WebAgent.Hubs;
using WebAgent.Utils;

namespace WebAgent
{
	public class Startup
	{
		private byte[] key;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Aries Open Api Requires Fluent Validation
			services
				.AddMvc()
				.AddFluentValidation()
				.AddAriesOpenApi(a => a.UseSwaggerUi = true);

			services.AddLogging();
			services.AddSignalR();

			services.AddSingleton<IAgentMiddleware, WSocketMiddleware>();

			// Register agent framework dependency services and handlers
			services.AddAriesFramework(builder =>
			{
				builder.RegisterAgent<SimpleWebAgent>(c =>
				{
					c.AgentName = "WEB4";
					c.EndpointUri = Environment.GetEnvironmentVariable("ENDPOINT_HOST") ?? Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
					c.WalletConfiguration = new WalletConfiguration { Id = "walletWEB4" };
					c.WalletCredentials = new WalletCredentials { Key = Environment.GetEnvironmentVariable("JWT_KEY") };
					c.GenesisFilename = Path.GetFullPath("pool_builder_genesis.txn");
					c.PoolName = "Builder Network";
				});
			});

			services.AddAuthentication(x =>
		    {
			    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		    })
			.AddJwtBearer(x =>
		    {
			   x.Events = new JwtBearerEvents
			   {
				   OnMessageReceived = context =>
				   {
					   context.Token = context.Request.Cookies["jwt_cookie"];
					   return System.Threading.Tasks.Task.CompletedTask;
				   }

			   };

			   x.RequireHttpsMetadata = false;
			   x.SaveToken = true;
			   x.TokenValidationParameters = new TokenValidationParameters
			   {
				   ValidateIssuerSigningKey = true,
				   IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"))),
				   ValidateIssuer = false,
				   ValidateAudience = false
			   };
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
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			// Register agent middleware
			app.UseAriesFramework();

			// Configure OpenApi
			app.UseAriesOpenApi();

			// fun identicons
			app.UseJdenticon();

			app.UseAuthentication();
			app.UseRouting();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "/{action=Index}/{id?}",
					defaults: new { controller = "Credentials" });

				endpoints.MapHub<TestHub>("/MessageHub");
			});

		}
	}
}
