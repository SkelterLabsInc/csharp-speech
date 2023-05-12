using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;
using Google.Cloud.Speech.V1;
using Grpc.Core;
using Grpc.Net.Client;


namespace SkelterLabsStt
{
    class SyncDemo
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

        static void Process(Options options)
        {
            // gRPC client
            var channel = GrpcChannel.ForAddress($"{options.Host}:{options.Port}");
            var client = new Speech.SpeechClient(channel);

            // Configuration
            var config = new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 16000,
                LanguageCode = "ko-KR",
            };

            // Audio data
            byte[] wav = File.ReadAllBytes(options.Path);
            var audio = new RecognitionAudio
            {
                Content = Google.Protobuf.ByteString.CopyFrom(wav)
            };

            // Request
            RecognizeRequest request = new RecognizeRequest
            {
                Config = config,
                Audio = audio,
            };
            var response = client.Recognize(request, new Metadata {
                {"x-api-key", options.ApiKey}
                });

            // Print response
            List<string> transcript_list = new List<string>();
            foreach (var result in response.Results)
                transcript_list.Add(result.Alternatives[0].Transcript);
            var transcript = string.Join('\n', transcript_list.ToArray());
            Console.WriteLine($"Result: {transcript}");
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => Process(opts));
        }
    }
}
