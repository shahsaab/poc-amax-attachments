using Amazon.S3;
using poc_amax_attachments.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(); 

var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIA6QO2T6FX3PB2Z2XR", "2zds010q8TrexT5KX0pXoyrbV3SniHZIWHJs1Wt0");

var region = Amazon.RegionEndpoint.GetBySystemName("us-east-2");
builder.Services.AddSingleton<IAmazonS3>(provider => new AmazonS3Client(credentials, region));


builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAnyOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
