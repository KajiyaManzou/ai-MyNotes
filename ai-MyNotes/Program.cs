using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ai_MyNotes;
using ai_MyNotes.Services;
using ai_MyNotes.Services.Exceptions;
using ai_MyNotes.Models;
using TG.Blazor.IndexedDB;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// TG.Blazor.IndexedDB の設定
builder.Services.AddIndexedDB(dbStore =>
{
    dbStore.DbName = MyNotesDatabase.DatabaseName;
    dbStore.Version = (int)MyNotesDatabase.Version;

    // メモストアの定義
    dbStore.Stores.Add(new StoreSchema
    {
        Name = MyNotesDatabase.MemoStore,
        PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = true }
    });
});

// アプリケーションサービスの登録
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
builder.Services.AddScoped<IMemoService, MemoService>();
builder.Services.AddScoped<GlobalErrorHandler>();

var app = builder.Build();

// グローバルエラーハンドラーの設定
var globalErrorHandler = app.Services.GetRequiredService<GlobalErrorHandler>();
globalErrorHandler.Initialize();

await app.RunAsync();
