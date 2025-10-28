using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using SkillSwapAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ SETUP FIREBASE - CORRECT WAY
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"] ?? "firebase-credentials.json";

try
{
    // Verify credentials file exists
    if (!System.IO.File.Exists(firebaseCredentialsPath))
    {
        throw new FileNotFoundException($"Firebase credentials file not found at: {System.IO.Path.GetFullPath(firebaseCredentialsPath)}");
    }

    // Set environment variable for Google Cloud
    var fullPath = System.IO.Path.GetFullPath(firebaseCredentialsPath);
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
    Console.WriteLine($"✅ Credentials file found at: {fullPath}");

    // Create Firestore instance - simple way
    var firestoreDb = FirestoreDb.Create(firebaseProjectId);
    
    builder.Services.AddSingleton(firestoreDb);
    Console.WriteLine($"✅ Firebase Firestore initialized successfully for project: {firebaseProjectId}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Firebase initialization error: {ex.Message}");
    Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
    throw;
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactNativeApp",
        policy =>
        {
            policy.WithOrigins("*")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// Stripe Payment Service
builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

// Agora Video Service
builder.Services.AddScoped<IAgoraService, AgoraService>();
builder.Services.AddHttpClient<IAgoraService, AgoraService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var projectId = builder.Configuration["Firebase:ProjectId"];
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true
        };
    });

// Stripe Configuration
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactNativeApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();