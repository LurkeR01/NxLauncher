using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NxLauncher.Data;
using NxLauncher.Database.Repositories;
using NxLauncher.Factories;
using NxLauncher.Services;
using NxLauncher.ViewModels;

namespace NxLauncher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var collection = new ServiceCollection();

            collection.AddTransient<IGenreRepository, GenreRepository>();
            collection.AddTransient<IGameRepository, GameRepository>();
            collection.AddTransient<IDeveloperRepository, DeveloperRepository>();
            collection.AddTransient<IUserRepository, UserRepository>();
            collection.AddTransient<IScreenshotRepository, ScreenshotRepository>();
            collection.AddTransient<IPurchaseRepository, PurchaseRepository>();
            collection.AddTransient<IFavoritesRepository, FavoritesRepository>();
            collection.AddTransient<IWishlistRepository, WishlistRepository>();
            collection.AddTransient<ICartRepository, CartRepository>();

            collection.AddSingleton<MainViewModel>();
            collection.AddSingleton<AuthenticationService>();

            collection.AddTransient<ShopPageViewModel>();
            collection.AddTransient<LibraryPageViewModel>();
            collection.AddTransient<CartPageViewModel>();
            collection.AddTransient<WishListPageViewModel>();
            collection.AddTransient<LoginViewModel>();
            collection.AddTransient<RegisterViewModel>();
            collection.AddTransient<AddGameViewModel>();
            
            
            collection.AddSingleton<Func<ApplicationPageNames, PageViewModel>>(x => name => name switch
            {
                ApplicationPageNames.Shop => x.GetRequiredService<ShopPageViewModel>(),
                ApplicationPageNames.Library => x.GetRequiredService<LibraryPageViewModel>(),
                ApplicationPageNames.Cart => x.GetRequiredService<CartPageViewModel>(),
                ApplicationPageNames.WishList => x.GetRequiredService<WishListPageViewModel>(),
                _ => throw new InvalidOperationException()
            });

            collection.AddSingleton<PageFactory>();

            var service = collection.BuildServiceProvider();

            var mainViewModel = service.GetRequiredService<MainViewModel>();
            Console.WriteLine("MainViewModel resolved successfully.");

            desktop.MainWindow = new MainView
            {
                DataContext = service.GetRequiredService<MainViewModel>()
            };
            Console.WriteLine("MainWindow created and DataContext assigned successfully.");
            
            mainViewModel.InitializeNavigation();
            Console.WriteLine("Initialization complete. MainWindow should be visible.");
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}