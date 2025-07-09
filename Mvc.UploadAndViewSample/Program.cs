using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var config = builder.Configuration;
string? saveFolder = config.GetSection("Config:SaveFolder").Value;
string? uploadFileFolder = config.GetSection("Config:UploadFileFolder").Value;

if (!string.IsNullOrEmpty(saveFolder) && !string.IsNullOrEmpty(uploadFileFolder))
{
    var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), saveFolder, uploadFileFolder);
    Directory.CreateDirectory(physicalPath);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(physicalPath),
        RequestPath = "/" + uploadFileFolder
    });
}

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
