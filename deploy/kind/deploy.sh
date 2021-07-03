deploy()
{
    # Create Kind cluster
    kind create cluster --name rabbitmq-demo --config deploy/kind/kind.conf

    # Build services
    sh deploy/build-services.sh

    # Load images into cluster
    kind load docker-image rabbitmq_demo/publisher:v1 --name rabbitmq-demo
    kind load docker-image rabbitmq_demo/subscriber:v1 --name rabbitmq-demo
    kind load docker-image rabbitmq:3.8-management --name rabbitmq-demo

    # Install RabbitMq cluster operator 
    kubectl rabbitmq install-cluster-operator

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