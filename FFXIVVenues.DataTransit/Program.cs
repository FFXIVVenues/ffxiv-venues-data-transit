// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables("FFXIV_VENUES_DATATRANSIT_")
    .Build();



Console.WriteLine("Hello, World!");