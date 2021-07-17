# Demo app for RabbitMq Kubernetes deployment using ASP.NET microservices

## Prerequisites

[Install kubectl RabbitMq plugin](https://www.rabbitmq.com/kubernetes/operator/install-operator.html)

## Deploy to Kind
```bash
# Add test DNS (Mac OS) to hosts fle. Remove other DNS references to rabbitmq-demo.com.
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

# Add test DNS (Mac OS) to hosts fle. Remove other DNS references to rabbitmq-demo.com . After debug, remove this line from the file, because next time minikube ip is changed.
echo "$(minikube ip) rabbitmq-demo.com" | sudo tee -a /etc/hosts

# Destroy
sh deploy/minikube/deploy.sh destroy
```

## Deploy with docker compose
```bash
# Add test DNS (Mac OS) to hosts fle. Remove other DNS references to rabbitmq-demo.com.
echo "127.0.0.1 rabbitmq-demo.com" | sudo tee -a /etc/hosts

# Build
sh deploy/build-services.sh 

# Deploy
docker-compose -f deploy/docker-compose/docker-compose.yml up

# Destroy
docker-compose -f deploy/docker-compose/docker-compose.yml down
```

## Build and run with Visual Studio Code
```bash
# Run only RabbitMq from docker-compose
docker-compose -f deploy/docker-compose/docker-compose.yml up -d --build rabbitmq

# Run and debug "All" launch configuration
```

## Open RabbitMq management console

```bash
# Show username and password
username="$(kubectl get secret rabbitmq-default-user -o jsonpath='{.data.username}' | base64 --decode)"
password="$(kubectl get secret rabbitmq-default-user -o jsonpath='{.data.password}' | base64 --decode)"
echo "username: $username"
echo "password: $password"

# Open port for management console
kubectl port-forward "service/rabbitmq" 15672

# Open management console
http://localhost:15672
```
## Test

### When run with Visual Studio Code
Run POST commands to url below using Postman anc check debug terminal
```bash
https://localhost:5000/Message/QueuePublish/test-message
https://localhost:5000/Message/DirectExchangePublish/test-message
https://localhost:5000/Message/TopicExchangePublish/test-message
https://localhost:5000/Message/FanoutExchangePublish/test-message
https://localhost:5000/Message/HeaderExchangePublish/test-message
https://localhost:5000/Message/rpc/test-message
```

### When run in local K8S cluster or docker compose
Run POST commands to url below using Postman and check logs from Subscriber pod
```bash
http://rabbitmq-demo.com/publisher/message/QueuePublish/test-message
http://rabbitmq-demo.com/publisher/message/DirectExchangePublish/test-message
http://rabbitmq-demo.com/publisher/message/TopicExchangePublish/test-message
http://rabbitmq-demo.com/publisher/message/FanoutExchangePublish/test-message
http://rabbitmq-demo.com/publisher/message/HeaderExchangePublish/test-message
http://rabbitmq-demo.com/publisher/message/rpc/test-message
```