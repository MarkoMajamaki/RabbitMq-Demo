# Deploy services
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/rabbitm-config.yaml
kubectl apply -f kubernetes/service1.yaml
kubectl apply -f kubernetes/service2.yaml
kubectl apply -f kubernetes/service3.yaml
kubectl apply -f kubernetes/ingress.yaml

# Get RabbitMq helm repo
helm repo add bitnami https://charts.bitnami.com/bitnami

# Deploy RabbitMq
helm install rabbitmq \
--set replicaCount=3,auth.username=guest,auth.password=guest,extraPlugins=rabbitmq_federation \
bitnami/rabbitmq -n rabbitmq-demo    
