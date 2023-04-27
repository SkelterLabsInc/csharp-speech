# AIQ.TALK STT C# Example

This repository contains simple example CLI programs that recognizes the given `resources/.wav` audio file.

## Prerequisites

For AIQ.TALK C# samples, we guarantee .NET Framework 6.0 support only.

### Unsupported Frameworks

The following frameworks are not tested for AIQ.TALK client.

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