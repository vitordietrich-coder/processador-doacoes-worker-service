# Processador de Doacoes Worker Service - Conexao Solidaria

Worker responsavel pelo processamento assíncrono das doacoes da plataforma Conexao Solidaria.

Este servico consome mensagens da fila RabbitMQ e atualiza o valor total arrecadado das campanhas no SQL Server.

---

## 1. Responsabilidade do Worker

O worker executa o seguinte fluxo:

1. Escuta a fila `donations` no RabbitMQ
2. Recebe o evento `DonationReceivedEvent`
3. Busca a campanha no banco de dados
4. Atualiza o campo `TotalRaised`
5. Salva a alteracao no SQL Server
6. Confirma o processamento da mensagem com `BasicAck`

A API apenas registra a doacao e publica o evento. O valor arrecadado e atualizado exclusivamente por este worker.

---

## 2. Requisitos para execucao local

Antes de iniciar, instale:

- .NET 9 SDK
- Docker Desktop
- SQL Server via container Docker
- RabbitMQ via container Docker
- Git

---

## 3. Subir infraestrutura local

Caso ainda nao tenha subido a infraestrutura pelo projeto `campanhas-service`, execute os comandos abaixo.

### 3.1 Criar rede Docker

```bash
docker network create conexao-solidaria-network
```

---

### 3.2 Subir SQL Server

```bash
docker run -d \
  --name sqlserver-campanhas \
  --network conexao-solidaria-network \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

---

### 3.3 Subir RabbitMQ

```bash
docker run -d \
  --name rabbitmq-conexao-solidaria \
  --network conexao-solidaria-network \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

Painel RabbitMQ:

```text
http://localhost:15672
```

Credenciais:

```text
guest
guest
```

---

## 4. Configurar appsettings.json

No projeto `Processador.Doacoes.Worker.Service`, configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DB_Campanhas;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest",
    "DonationQueue": "donations"
  }
}
```

---

## 5. Restaurar pacotes

Na raiz da solucao do worker:

```bash
dotnet restore src/Processador.Doacoes.Worker.Service/Processador.Doacoes.Worker.Service.sln
```

---

## 6. Executar o worker localmente

```bash
dotnet run --project src/Processador.Doacoes.Worker.Service/Processador.Doacoes.Worker.Service
```

Ao iniciar corretamente, o worker deve ficar ativo aguardando mensagens na fila `donations`.

---

## 7. Como testar o worker

### 7.1 Inicie a infraestrutura

Garanta que estejam rodando:

- SQL Server
- RabbitMQ
- Campanhas Service API
- Processador de Doacoes Worker

Validar containers:

```bash
docker ps
```

---

### 7.2 Criar uma doacao pela API

No Swagger da API, execute:

```http
POST /api/v1/Donations
```

Payload:

```json
{
  "campaignId": "GUID_DA_CAMPANHA",
  "amount": 50
}
```

---

### 7.3 Verificar RabbitMQ

Acesse:

```text
http://localhost:15672
```

Entre em:

```text
Queues and Streams > donations
```

Se o worker estiver parado, a mensagem ficara pendente na fila.

Se o worker estiver rodando, a mensagem sera consumida rapidamente.

---

### 7.4 Verificar atualizacao da campanha

Consultar:

```http
GET /api/v1/Campaigns/public
```

O campo `totalRaised` deve ser atualizado pelo worker.

---

## 8. Executar via Docker Compose

Na raiz do repositorio:

```bash
docker compose up -d --build
```

Verificar logs do worker:

```bash
docker logs processador-doacoes-worker
```

---

## 9. Executar no Kubernetes local

Pre-requisito: Kubernetes habilitado no Docker Desktop.

Aplicar manifests:

```bash
kubectl apply -f k8s/
```

Verificar pods:

```bash
kubectl get pods
```

Ver logs do worker:

```bash
kubectl logs deployment/donation-worker
```

---

## 10. Imagem Docker

Imagem esperada no Docker Hub:

```text
SEU_USUARIO_DOCKERHUB/processador-doacoes:latest
```

---

## 11. Variaveis de ambiente no Kubernetes

O deployment do worker deve conter:

```yaml
env:
  - name: ConnectionStrings__DefaultConnection
    value: "Server=sqlserver-service,1433;Database=DB_Campanhas;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
  - name: RabbitMQ__Host
    value: "rabbitmq-service"
  - name: RabbitMQ__Username
    value: "guest"
  - name: RabbitMQ__Password
    value: "guest"
  - name: RabbitMQ__DonationQueue
    value: "donations"
```

---

## 12. Pipeline CI/CD

O GitHub Actions executa:

1. Build do worker
2. Execucao de testes, se existirem
3. Build da imagem Docker
4. Publicacao no Docker Hub

---

## 13. Observacoes para correcao

O worker nao possui Swagger nem endpoints HTTP, pois e um servico de background.

A validacao do funcionamento deve ser feita por:

- Logs do worker
- Painel RabbitMQ
- Consulta publica das campanhas na API
- Alteracao do campo `TotalRaised`
