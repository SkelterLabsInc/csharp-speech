# AIQ.TALK STT C# Example

The AIQ.TALK STT API is mostly compatible with the Google Cloud Speech API, so you can use [Google APIs client Library for .NET](https://github.com/googleapis/google-api-dotnet-client) to use AIQ.TALK STT API.

This repository contains simple example CLI programs that recognizes the given `resources/.wav` audio file.

## Prerequisites

For AIQ.TALK C# samples, we guarantees .NET Framework 5.0 support only.

### Unsupported Frameworks

The following frameworks are not supported by Google API client library. So, they cannot be used for AIQ.TALK client neither.

* Silverlight
* UWP
* Xamarin
* Unity

### AIQ API key

To use AIQ.TALK API, you should receive your own API key. 
Get your AIQ API key from the
[AIQ Console](https://aiq.skelterlabs.com/console).

## Samples

NOTE. We support mono audio only now.

### Synchronously transcribe a local file

Perform synchronous transcription on a local audio file.
Synchronous request supports ~1 minute audio length.

```shell
$ dotnet run --project Recognize \
-- \
--api-key=<your API key> \
--path resources/hello.wav
```

### Streaming speech recognition

Perform streaming request on a local audio file.

```shell
$ dotnet run --project StreamingRecognize \
-- \
--api-key=<your API key> \
--path resources/hello.wav
```