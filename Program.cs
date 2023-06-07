using System.Configuration;
using FFXIVVenues.DataTransit.Destinations;
using FFXIVVenues.DataTransit.Origins;
using FFXIVVenues.DataTransit.PostActions;
using FFXIVVenues.DataTransit.Transformers;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables("FFXIV_VENUES_DATATRANSIT_")
    .Build();

var originTypeName = config.GetValue<string>("Origin:Type");
if (originTypeName == null)
    throw new ConfigurationErrorsException("Origin:Type is not specified.");
var originType = Type.GetType(originTypeName);
if (originType == null)
    throw new ConfigurationErrorsException($"Could not locate type '{originTypeName}' indicated in Origin:Type.");
if (Activator.CreateInstance(originType) is not IOrigin origin)
    throw new ConfigurationErrorsException($"Type '{originTypeName}' indicated in Origin:Type does not implement the interface IOrigin."); 

config.GetSection("Origin").Bind(origin);
var venues = await origin.FetchAllAsync();

var transformerConfigs = config.GetSection("Transformers").GetChildren();
var i = 0;
foreach (var transformerConfig in transformerConfigs)
{
    var transformerTypeName = transformerConfig.GetValue<string>("Type");
    if (transformerTypeName == null)
        throw new ConfigurationErrorsException($"Transformers.{i}.Type is not specified.");
    var transformerType = Type.GetType(transformerTypeName);
    if (transformerType == null)
        throw new ConfigurationErrorsException($"Could not locate type '{transformerType}' indicated in Transformations.");
    if (Activator.CreateInstance(transformerType) is not ITransformer transformer)
        throw new ConfigurationErrorsException($"Type '{transformerTypeName}' indicated in Transformations does not implement the interface ITransformer.");
    transformerConfig.Bind(transformer);
    venues = await transformer.TransformAsync(venues);
    i++;
}

var destinationTypeName = config.GetValue<string>("Destination:Type");
if (destinationTypeName == null)
    throw new ConfigurationErrorsException("Destination:Type is not specified.");
var destinationType = Type.GetType(destinationTypeName);
if (destinationType == null)
    throw new ConfigurationErrorsException($"Could not locate type '{destinationTypeName}' indicated in Destination:Type.");
if (Activator.CreateInstance(destinationType) is not IDestination destination)
    throw new ConfigurationErrorsException($"Type '{destinationTypeName}' indicated in Destination:Type does not implement the interface IDestination.");

config.GetSection("Destination").Bind(destination);
await destination.SendAsync(venues);

var postActionConfigs = config.GetSection("PostActions").GetChildren();
var a = 0;
foreach (var postActionConfig in postActionConfigs)
{
    var postActionTypeName = postActionConfig.GetValue<string>("Type");
    if (postActionTypeName == null)
        throw new ConfigurationErrorsException($"PostActions.{a}.Type is not specified.");
    var postActionType = Type.GetType(postActionTypeName);
    if (postActionType == null)
        throw new ConfigurationErrorsException($"Could not locate type '{postActionType}' indicated in PostActions.");
    if (Activator.CreateInstance(postActionType) is not IPostAction postAction)
        throw new ConfigurationErrorsException($"Type '{postActionTypeName}' indicated in PostActions does not implement the interface IPostActions.");
    postActionConfig.Bind(postAction);
    await postAction.ExecuteAsync();
    a++;
}