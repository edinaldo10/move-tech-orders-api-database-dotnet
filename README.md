# move-tech-orders-api-database

**Provisionamento de um sistema e conexão com banco de dados**.



---

## Contexto
A modelagem de dados já está documentada em `docs/data-model.md` e o código já usa Entity Framework Core com suporte a SQLite e PostgreSQL.

Neste trabalho se **provisionou o PostgreSQL na Magalu Cloud e conectou-se a aplicação**.

---

## O que você vai fazer

- [ ] Criar uma instância PostgreSQL no DBaaS da Magalu Cloud
- [ ] Criar o banco `orders` manualmente no console
- [ ] Configurar o GitHub Secret `DATABASE_URL`
- [ ] Atualizar `k8s/app.yaml` com a variável de ambiente `DATABASE_URL`
- [ ] Atualizar `.github/workflows/deploy.yml` com o step de criação do Kubernetes Secret
- [ ] Disparar o deploy e validar `/health` com `"database": "ok"`

---

## Como rodar localmente

**Pré-requisito:** Docker Desktop instalado.

```bash
docker compose up --build
```

Acesse: http://localhost:8000/docs (ou http://localhost:8000/scalar/v1)

---

## Secrets necessários no GitHub

Configure em Settings → Secrets and variables → Actions:

| Secret | Descrição |
|---|---|
| `MGC_REGISTRY_USER` | Usuário do Container Registry da MGC |
| `MGC_REGISTRY_PASSWORD` | Senha do Container Registry da MGC |
| `MGC_REGISTRY_NAME` | Nome do registry na MGC |
| `MGC_KUBECONFIG` | Conteúdo do kubeconfig.yaml |
| `DATABASE_URL` | String de conexão do PostgreSQL |

---


