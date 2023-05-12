using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Google.Cloud.Speech.V1;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;


namespace SkelterLabsStt
{
    class StreamingDemo
    {
        class Options
        {
            [Option('f', "path", Default = "../Resources/hello.wav", HelpText = "Audio file path")]
            public string Path { get; set; }

            [Option('h', "host", Default = "https://aiq.skelterlabs.com", HelpText = "gRPC host address.")]
            public string Host { get; set; }

            [Option('p', "port", Default = 443, HelpText = "gRPC host port number.")]
            public int Port { get; set; }

            [Option('k', "api-key", HelpText = "Authentication key for AIQ.")]
            public string ApiKey { get; set; }
        }

        static async Task<int> ProcessAsync(Options options)
        {
            // gRPC client
            var channelOption = new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), CallCredentials.FromInterceptor((context, metadata) =>
                {
                    metadata.Add("x-api-key", options.ApiKey);
                    return Task.CompletedTask;
                }))
            };
            var channel = GrpcChannel.ForAddress($"{options.Host}:{options.Port}", channelOption);
            var client = new Speech.SpeechClient(channel);
            var call = client.StreamingRecognize();

            Console.WriteLine("Starting background task to receive messages");
            // Register response handler task.
            Task responseHandlerTask = Task.Run(async () =>
            {
                Console.Write("Result: ");
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    foreach (var result in response.Results)
                        if (result.IsFinal)
                            Console.Write($"{result.Alternatives[0].Transcript}");
                }
            });

            Console.WriteLine("Starting to send messages");
            // Configuration
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000,
                LanguageCode = "ko-KR",
            };
            var streaming_config = new StreamingRecognitionConfig
            {
                Config = config,
                InterimResults = true,
            };

            // Only `streaming_config` is expected for the first `StreamingRecognizeRequest`.
            await call.RequestStream.WriteAsync(new StreamingRecognizeRequest
            {
                StreamingConfig = streaming_config,
            });

            // Audio data
            byte[] audioBytes = File.ReadAllBytes(options.Path);
            const int bufferSize = (int)1024;  // 1KB
            for (int startOffset = 0; startOffset < audioBytes.Length; startOffset += bufferSize)
            {
                await call.RequestStream.WriteAsync(
                  new StreamingRecognizeRequest
                  {
                      AudioContent = ByteString.CopyFrom(
                        audioBytes,
                        startOffset,
                        Math.Min(bufferSize, audioBytes.Length - startOffset)),
                  });
            }

            Console.WriteLine("Disconnecting");
            await call.RequestStream.CompleteAsync();
            await responseHandlerTask;

            return 0;
        }

        static async Task Main(string[] args)
        {
            ParserResult<Options> parsedArguments = Parser.Default.ParseArguments<Options>(args);

            await parsedArguments.WithParsedAsync(opts => ProcessAsync(opts));
        }
    }
}
