/* Sample of Skelter Labs STT streaming client
 * 
 * The below code is based on Google Cloud Speech V1 API document:
 * https://googleapis.github.io/google-cloud-dotnet/docs/Google.Cloud.Speech.V1/api/Google.Cloud.Speech.V1.SpeechClient.html#Google_Cloud_Speech_V1_SpeechClient_StreamingRecognize_Google_Api_Gax_Grpc_CallSettings_Google_Api_Gax_Grpc_BidirectionalStreamingSettings_
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using Google.Api.Gax.Grpc;
using Google.Cloud.Speech.V1;
using Google.Protobuf;
using Grpc.Core;

namespace SkelterLabsStt {
  class StreamingDemo {
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
      public bool Insecure { get; set; }
    }

    static async Task<int> ProcessAsync(Options options) {
      Task<RecognitionAudio> audioTask = RecognitionAudio.FromFileAsync(options.Path);

      var builder = new SpeechClientBuilder {
        Endpoint = $"{options.Host}:{options.Port}",
      };
      if (options.Insecure)
        builder.ChannelCredentials = ChannelCredentials.Insecure;

      SpeechClient client = builder.Build();

      CallSettings callSetting = null;
      if (options.ApiKey != null)
        callSetting = CallSettings.FromHeader("x-api-key", options.ApiKey);

      // Initialize streaming call, retrieving the stream object.
      SpeechClient.StreamingRecognizeStream streamingCall = (
          client.StreamingRecognize(callSettings: callSetting));

      var streamingConfig = new StreamingRecognitionConfig {
        Config = new RecognitionConfig {
          Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
          SampleRateHertz = 16000,
          LanguageCode = LanguageCodes.Korean.SouthKorea,
        },
        InterimResults = false,
      };

      // Only `streaming_config` is expected for the first `StreamingRecognizeRequest`.
      await streamingCall.WriteAsync(
        new StreamingRecognizeRequest {
          StreamingConfig = streamingConfig,
        }
      );

      // Register response handler task.
      Task responseHandlerTask = Task.Run(async () => {
        List<string> transcript_list = new List<string>();

        // Note that C# 8 code can use await foreach
        AsyncResponseStream<StreamingRecognizeResponse> responseStream = (
            streamingCall.GetResponseStream());
        while (await responseStream.MoveNextAsync()) {
          StreamingRecognizeResponse responseItem = responseStream.Current;
          foreach (var result in responseItem.Results)
            transcript_list.Add(result.Alternatives[0].Transcript);
        }
        var transcript = string.Join('\n', transcript_list.ToArray());
        Console.WriteLine($"Result: {transcript}");
      });

      RecognitionAudio audio = await audioTask;
      const int bufferSize = (int)5e+7;
      for (int startOffset = 0; startOffset < audio.Content.Length; startOffset += bufferSize) {
        await streamingCall.WriteAsync(
          new StreamingRecognizeRequest
          {
            AudioContent = ByteString.CopyFrom(
                audio.Content.ToByteArray(),
                startOffset,
                Math.Min(bufferSize, audio.Content.Length - startOffset)),
          });
      }

      await streamingCall.WriteCompleteAsync();
      await responseHandlerTask;

      return 0;
    }

    static async Task Main(string[] args) {
      ParserResult<Options> parsedArguments = Parser.Default.ParseArguments<Options>(args);

      await parsedArguments.WithParsedAsync(opts => ProcessAsync(opts));
    }
  }
}