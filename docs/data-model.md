# Modelagem de Dados

## Entidades

### Pedido (orders)

| Coluna | Tipo | Descrição |
|---|---|---|
| id | TEXT | Identificador único, gerado automaticamente |
| customer | TEXT | Nome do cliente |
| status | TEXT | Estado do pedido (ex.: open, cancelled) |
| created_at | TIMESTAMP | Data e hora de criação, preenchida automaticamente |

### Item (items)

| Coluna | Tipo | Descrição |
|---|---|---|
| id | TEXT | Identificador único, gerado automaticamente |
| order_id | TEXT | Chave estrangeira → orders(id) |
| sku | TEXT | Código do produto |
| description | TEXT | Descrição do item |
| quantity | INTEGER | Quantidade |

## Relacionamento
Um pedido (`orders`) tem vários itens (`items`): relacionamento 1:N.
A coluna `order_id` em `items` é a chave estrangeira que liga cada item ao seu pedido.
Apagar um pedido apaga automaticamente todos os seus itens (configurado via Entity Framework Core).


## Como as tabelas são criadas
As tabelas são criadas automaticamente pelo Entity Framework Core na inicialização da aplicação (`db.Database.EnsureCreated()`).
As definições estão nas classes de entidades em `Models/Entities.cs`.
