# Demo app for RabbitMq Kubernetes deployment using ASP.NET microservices

**Repo is still under heavy development and some features might not work!**

## Deploy to Kind
```bash
# Deploy
sh deploy/deploy-kind.sh deploy

# Destroy
sh deploy/deploy-kind.sh destroy
```

## Deploy to Minikube
```bash
# Deploy
sh deploy/deploy-minikube.sh deploy

# Destroy
sh deploy/deploy-minikube.sh destroy
```

## Deploy with docker compose
```bash
# Deploy
docker-compose -f deploy/docker-compose.yml up

# Destroy
docker-compose -f deploy/docker-compose.yml down
```

## Test
```bash
http://localhost:8080/service1/test
http://localhost:8080/service1/test/service2
http://localhost:8080/service1/test/service3
http://localhost:8080/service2/test
```

Should not work! Access allowed only throught service1!
```bash
http://localhost:8080/service3/test
```
