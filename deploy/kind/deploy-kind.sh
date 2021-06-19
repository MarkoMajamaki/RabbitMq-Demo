deploy()
{
    # Create Kind cluster
    kind create cluster --name rabbitmq-demo --config deploy/kind.conf

    # Build services
    sh deploy/build-services.sh

    # Pull RabbitMq image
	docker pull rabbitmq:3.8-management

    # Load images into cluster
    kind load docker-image rabbitmq_demo/service1:v1 --name rabbitmq-demo
    kind load docker-image rabbitmq_demo/service2:v1 --name rabbitmq-demo
    kind load docker-image rabbitmq_demo/service3:v1 --name rabbitmq-demo
    kind load docker-image rabbitmq:3.8-management --name architecture-demo

    # Deploy services
    sh deploy/deploy-services.sh    

    # Deploy ingress controller
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/master/deploy/static/provider/kind/deploy.yaml
}

destroy()
{
    kind delete cluster --name rabbitmq-demo
}

"$@"