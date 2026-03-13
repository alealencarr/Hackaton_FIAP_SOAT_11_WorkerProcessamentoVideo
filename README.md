# FIAP X - Video Processing Worker

Microsserviço responsável pelo processamento de vídeos com FFmpeg.

## Funcionalidades

- Consome fila `video-processing-queue` do RabbitMQ
- Extrai frames dos vídeos usando FFmpeg
- Gera arquivo ZIP com as imagens
- Atualiza status no banco de dados
- Publica notificação na fila `email-queue`

## Executar

```bash
dotnet run --project src/FiapX.Worker
```

## Docker

```bash
docker build -t fiapx-worker .
docker run -e ConnectionStrings__DefaultConnection="..." -e RabbitMQ__HostName=rabbitmq fiapx-worker
```

## Requisitos

- .NET 8
- FFmpeg instalado
- RabbitMQ
- SQL Server
