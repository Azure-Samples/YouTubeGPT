# YouTubeGPT

This is a sample application which demonstrates how to use OpenAI models to create a Retrival Augmented Generation (RAG) application using YouTube videos as the data source.

It works by extracting the description of the video and using it as the context for the RAG model. The model can then be used to generate a response to a given prompt.

The application is split into two parts, a data ingestion service and a web application. The data ingestion service is responsible for downloading the video metadata and extracting the description. The web application is responsible for providing a user interface to interact with the model.

## Prerequisites

- .NET 8
- .NEW Aspire 8
- Access to Azure OpenAI Service or an OpenAI API key
  - Models required:
    - Default: `text-embedding-ada-3-small`
      - Override with `dotnet user-secrets set "Azure:AI:EmbeddingDeploymentName" "<your model deployment name>"`
    - Default: `gpt-4o`
      - Override with `dotnet user-secrets set "Azure:AI:ChatDeploymentName" "<your model deployment name>"`
- Docker

## Setup

1. Clone the repository

   ```bash
   gh repo clone aaronpowell/YouTubeGPT
   ```

1. Configure access to the models (use the Azure OpenAI or OpenAI API key)

   ```bash
   cd YouTubeGPT.AppHost

   # Azure OpenAI Service
   dotnet user-secrets set "ConnectionStrings:OpenAI" "Endpoint=https://<your-endpoint>.cognitiveservices.azure.com/;Key=<your-key>"

   # OpenAI API Key
   dotnet user-secrets set "ConnectionStrings:OpenAI" "<your key>"
   ```

1. Run the application

   ```bash
   dotnet run
   ```

1. Open a browser and navigate to `http://localhost:15015` and enter the login token (see the linked instructions for how to get the token)

## Adding YouTube Data

This is handled by the `YouTubeGPT.Ingestion` service. Open it from the Aspire dashboard and enter a YouTube channel URL into the provided input box (eg: https://youtube.com/@dotnet). The service will then download the video metadata and extract the description.

## Searching for Videos

The web application provides a search box where you can enter a query and the model will be used to find the most relevant video to the query. The description of the video will be used as the context for the model.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
