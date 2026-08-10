# move-tech-orders-api-database

Ponto de partida do **Lab H2 — Provisionar e conectar o banco**.

Parte do curso **Move Tech** — Magalu × Prósper Digital Skills
Formação em Cloud Computing para iniciantes

---

## Contexto

A modelagem de dados já está documentada em `docs/data-model.md` e o código já usa SQLAlchemy.

Seu trabalho neste lab é **provisionar o PostgreSQL na Magalu Cloud e conectar a aplicação**.

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

Acesse: http://localhost:8000/docs

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

## Próximo lab

Após concluir este lab, avance para o **Lab H3 — Validar a persistência**.
