
namespace UserController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
           // builder.Services.AddOpenApi();
           builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(c=>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hardware API v1");
                c.RoutePrefix = "swagger";
            });

            // Configure the HTTP request pipeline.
          //  if (app.Environment.IsDevelopment())
           // {
             //   app.MapOpenApi();
           // }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
