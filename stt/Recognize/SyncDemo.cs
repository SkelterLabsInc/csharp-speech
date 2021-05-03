using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Google.Api.Gax.Grpc;
using Google.Cloud.Speech.V1;
using Grpc.Core;

namespace SkelterLabsStt {
  class SyncDemo {
    class Options {
      [Option('f', "path", Default = "../Resources/hello.wav", HelpText = "Audio file path")]
      public string Path { get; set; }

      [Option('h', "host", Default = "aiq.skelterlabs.com", HelpText = "gRPC host address.")]
      public string Host { get; set; }

      [Option('p', "port", Default = 443, HelpText = "gRPC host port number.")]
      public int Port { get; set; }

      [Option('k', "api-key", HelpText = "Authentication key for AIQ Speech")]
      public string ApiKey { get; set; }

      [Option("insecure", Default = false, HelpText = "Make call with insecure mode.")]
      public bool Insecure {get; set; }
    }

    static async Task ProcessAsync(Options options) {
      Task<RecognitionAudio> audioTask = RecognitionAudio.FromFileAsync(options.Path);
      var builder = new SpeechClientBuilder {
        Endpoint = $"{options.Host}:{options.Port}",
      };
      if(options.Insecure)
        builder.ChannelCredentials = ChannelCredentials.Insecure;
      SpeechClient client = builder.Build();

      var config = new RecognitionConfig {
        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
        SampleRateHertz = 16000,
        LanguageCode = LanguageCodes.Korean.SouthKorea,
      };
      var audio = await audioTask;
      var request = new RecognizeRequest {
        Audio = audio,
        Config = config,
      };
      CallSettings callSetting = null;
      if(options.ApiKey != null)
        callSetting = CallSettings.FromHeader("x-api-key", options.ApiKey);

      var response = client.Recognize(request, callSetting);

      List<string> transcript_list = new List<string>();
      foreach (var result in response.Results)
        transcript_list.Add(result.Alternatives[0].Transcript);
      var transcript = string.Join('\n', transcript_list.ToArray());
      Console.WriteLine($"Result: {transcript}");
    }
      
    static async Task Main(string[] args)
    {
      var parsedArguments = Parser.Default.ParseArguments<Options>(args);

      await parsedArguments.WithParsedAsync(opts => ProcessAsync(opts));
    }
  }
}
