using testTask.WebApi;

class Program
{
    public static void Main(string[] args)
    {
        // TODO: allow connections on dispatch
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Make sure to allow incoming connection on PostgreSQL " +
            "(\\PostgreSQL\\14\\data\\pg_hba.conf)");
        Console.WriteLine("host    all             all             0.0.0.0/0               md5");
        Console.ForegroundColor = ConsoleColor.Gray;
        var app = CreateHostBuilder(args).Build();
        app.Run();
    }


    // EF Core uses this method at design time to access the Context (DB)
    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(
                webBuilder => webBuilder.UseStartup<Startup>());

}