# Demo app for RabbitMq Kubernetes deployment using ASP.NET microservices

**Repo is still under heavy development and some features might not work!**

## Deploy to Kind
```bash
# Add test DNS (Mac OS)
echo "127.0.0.1 rabbitmq-demo.com" | sudo tee -a /etc/hosts

# Deploy
sh deploy/kind/deploy.sh deploy

# Destroy
sh deploy/kind/deploy.sh destroy
```

## Deploy to Minikube
```bash
# Deploy
sh deploy/minikube/deploy.sh deploy

# Add test DNS (Mac OS) Remove after debug!
echo "$(minikube ip) rabbitmq-demo.com" | sudo tee -a /etc/hosts

# Destroy
sh deploy/minikube/deploy.sh destroy
```

## Deploy with docker compose
```bash
# Add test DNS (Mac OS)
echo "127.0.0.1 rabbitmq-demo.com" | sudo tee -a /etc/hosts

# Build
sh deploy/build-services.sh 

# Deploy
docker-compose -f deploy/docker-compose/docker-compose.yml up

# Destroy
docker-compose -f deploy/docker-compose/docker-compose.yml down
```

## Test
```bash
http://rabbitmq-demo.com/service1/test
http://rabbitmq-demo.com/service1/test/service2
http://rabbitmq-demo.com/service1/test/service3
http://rabbitmq-demo.com/service2/test
```

Should not work! Access allowed only throught service1!
```bash
http://rabbitmq-demo.com/service3/test
```
