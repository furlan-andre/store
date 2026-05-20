# Store API

API MVP para gerenciamento de pedidos, produtos e clientes. O projeto foi estruturado com foco em Clean Architecture, DDD, SOLID, KISS e separação clara entre borda HTTP, aplicação, domínio e infraestrutura.

## Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker e Docker Compose
- Swagger / OpenAPI
- JWT Bearer Authentication
- FluentValidation
- xUnit, Moq e FluentAssertions
- Testcontainers para testes funcionais com PostgreSQL real

## Arquitetura

O projeto segue uma organização em camadas:

```text
src/
  Store.API              Entrada HTTP, controllers, Swagger, JWT e composition root
  Store.Application      Casos de uso, contratos, validações e DTOs
  Store.Domain           Entidades e regras de domínio
  Store.Infrastructure   EF Core, DbContext, migrations, repositories e PostgreSQL
  Store.FunctionalTests  Testes funcionais com API real e PostgreSQL via Testcontainers

tests/
  Store.Tests            Testes unitários de domínio, aplicação e controllers
```

A camada `Store.API` depende das camadas de aplicação e infraestrutura. A camada `Store.Application` concentra os contratos e orquestra os casos de uso. A camada `Store.Domain` não depende de frameworks externos. A camada `Store.Infrastructure` implementa persistência com EF Core e PostgreSQL.

## Como Rodar Com Docker

Este é o modo preferencial para executar o MVP.

Pré-requisitos:

- Docker
- Docker Compose

Na raiz do projeto, execute:

```bash
docker compose up --build -d
```

Serviços expostos:

| Serviço | Endereço | Credenciais |
| --- | --- | --- |
| API | `http://localhost:8080` | JWT |
| Swagger | `http://localhost:8080/swagger` | JWT |
| PostgreSQL | `localhost:5432` | database: `store`, user: `store`, password: `store` |

Para parar os containers:

```bash
docker compose down
```

Para remover também o volume do banco:

```bash
docker compose down -v
```

## Como Rodar Pela IDE

Também é possível executar pelo JetBrains Rider ou Visual Studio usando o projeto `Store.API`.

Pré-requisitos:

- .NET 8 SDK
- PostgreSQL instalado localmente ou um container PostgreSQL rodando
- Banco configurado conforme a connection string do `appsettings.json`

Configuração padrão em `src/Store.API/appsettings.json`:

```json
"ConnectionStrings": {
  "StoreDatabase": "Host=localhost;Port=5432;Database=store;Username=store;Password=store"
}
```

Ao executar pela IDE, garanta que o PostgreSQL esteja disponível em `localhost:5432` com:

- database: `store`
- username: `store`
- password: `store`

O perfil local da API usa Swagger como página inicial. Em execução local pelo launch profile, a API costuma ficar em:

- `http://localhost:5001/swagger`
- `https://localhost:5000/swagger`

As migrations são aplicadas automaticamente na inicialização da API por um hosted service.

## Autenticação JWT

A API usa JWT Bearer Authentication. Como este projeto é um MVP e controle de usuários persistido não fazia parte do escopo, a autenticação foi implementada da forma mais simples possível para prova de conceito: usuário e senha ficam em configuração.

Configuração atual:

```json
"Authentication": {
  "Username": "admin",
  "Password": "admin"
}
```

Para gerar um token:

```http
POST /auth/token
Content-Type: application/json
```

Request:

```json
{
  "username": "admin",
  "password": "admin"
}
```

Response:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-05-20T12:00:00+00:00"
}
```

Use o token no header:

```http
Authorization: Bearer <accessToken>
```

Sem token ou com token inválido, os endpoints protegidos retornam `401 Unauthorized`.

## Seeds

O projeto possui seed via migrations para facilitar a prova de conceito do MVP.

Produtos:

| Id | Nome | Preço | Estoque |
| --- | --- | ---: | ---: |
| 1 | Notebook | 3500.00 | 10 |
| 2 | Keyboard | 150.00 | 50 |
| 3 | Mouse | 80.00 | 100 |

Clientes:

| Id | Nome |
| --- | --- |
| 1 | André |
| 2 | José |
| 3 | Maria |

## Endpoints

Todos os endpoints abaixo, exceto autenticação, exigem JWT.

### Auth

```http
POST /auth/token
```

Gera o token JWT para acesso aos demais endpoints.

### Products

```http
GET /products
GET /products/{id}
```

Exemplo:

```bash
curl -H "Authorization: Bearer <token>" http://localhost:8080/products
```

Response:

```json
[
  {
    "id": 1,
    "name": "Notebook",
    "unitPrice": 3500.00,
    "availableQuantity": 10
  }
]
```

### Customers

```http
GET /customers
GET /customers/{id}
```

Exemplo:

```bash
curl -H "Authorization: Bearer <token>" http://localhost:8080/customers
```

Response:

```json
[
  {
    "id": 1,
    "name": "André"
  }
]
```

### Orders

```http
POST /orders
POST /orders/{id}/confirm
POST /orders/{id}/cancel
GET /orders/{id}
GET /orders
```

Criar pedido:

```http
POST /orders
Authorization: Bearer <token>
Content-Type: application/json
```

Request:

```json
{
  "customerId": 1,
  "currency": "BRL",
  "items": [
    {
      "productId": 1,
      "quantity": 1
    },
    {
      "productId": 2,
      "quantity": 2
    }
  ]
}
```

Response `201 Created`:

```json
{
  "id": 1,
  "customerId": 1,
  "status": 0,
  "total": 3800.00,
  "currency": "BRL",
  "createdAt": "2026-05-20T10:00:00+00:00",
  "confirmedAt": null,
  "cancelledAt": null,
  "items": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 1,
      "unitPrice": 3500.00,
      "subtotal": 3500.00
    },
    {
      "id": 2,
      "productId": 2,
      "quantity": 2,
      "unitPrice": 150.00,
      "subtotal": 300.00
    }
  ]
}
```

Confirmar pedido:

```bash
curl -X POST -H "Authorization: Bearer <token>" http://localhost:8080/orders/1/confirm
```

Ao confirmar, o pedido passa de `Placed` para `Confirmed` e o estoque dos produtos é baixado. A operação é idempotente: chamar o endpoint novamente mantém o mesmo resultado e não baixa estoque duas vezes.

Cancelar pedido:

```bash
curl -X POST -H "Authorization: Bearer <token>" http://localhost:8080/orders/1/cancel
```

Pedidos `Placed` e `Confirmed` podem ser cancelados. Quando um pedido confirmado é cancelado, o estoque é devolvido. A operação também é idempotente.

Buscar pedido por id:

```bash
curl -H "Authorization: Bearer <token>" http://localhost:8080/orders/1
```

Listar pedidos:

```bash
curl -H "Authorization: Bearer <token>" "http://localhost:8080/orders?page=1&pageSize=20"
```

Filtros disponíveis:

| Query string | Descrição |
| --- | --- |
| `customerId` | Filtra por cliente |
| `status` | Filtra por status: `Placed`, `Confirmed` ou `Canceled` |
| `createdFrom` / `createdTo` | Filtra por intervalo de criação |
| `cancelledFrom` / `cancelledTo` | Filtra por intervalo de cancelamento |
| `page` / `pageSize` | Paginação |

Exemplo com filtros:

```bash
curl -H "Authorization: Bearer <token>" "http://localhost:8080/orders?customerId=1&status=Placed&page=1&pageSize=20"
```

Response:

```json
{
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1,
  "items": [
    {
      "id": 1,
      "customerId": 1,
      "status": 0,
      "total": 3800.00,
      "currency": "BRL",
      "createdAt": "2026-05-20T10:00:00+00:00",
      "confirmedAt": null,
      "cancelledAt": null,
      "items": []
    }
  ]
}
```

Status do pedido:

| Valor | Nome |
| ---: | --- |
| 0 | Placed |
| 1 | Confirmed |
| 2 | Canceled |

## Collection Postman

Existe uma collection Postman na raiz do projeto:

```text
Store.postman_collection.json
```

Ela já vem configurada com as principais requisições para testar a API externamente ao Swagger, incluindo:

- geração de JWT
- criação válida de pedido
- cenários inválidos críticos
- confirmação e cancelamento
- idempotência
- filtros de listagem
- chamadas sem token e com token inválido

Por padrão, a variável `baseUrl` está configurada como:

```text
http://localhost:8080
```

## Testes

Para rodar todos os testes:

```bash
dotnet test Store.sln
```

Os testes funcionais usam Testcontainers para subir um PostgreSQL real e validar o fluxo completo da API até o banco.
