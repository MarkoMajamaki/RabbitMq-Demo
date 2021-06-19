deploy()
{
    # Start minikube
	minikube start

	# Enable minikube ingress controller
	minikube addons enable ingress

    # Build images into minikube
    eval $(minikube docker-env)

    # Pull RabbitMq
    docker pull rabbitmq:3.8-management
    
    # Build services
    sh deploy/build-services.sh

    # Deploy services
    sh deploy/deploy-services.sh
}

destroy()
{
    minikube delete
}

"$@"